using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace Flurl.Http.Configuration
{
	/// <summary>
	/// A builder for configuring IFlurlClient instances.
	/// </summary>
	public interface IFlurlClientBuilder
	{
		/// <summary>
		/// Configure the IFlurlClient's Settings.
		/// </summary>
		IFlurlClientBuilder WithSettings(Action<FlurlHttpSettings> configAction);

		/// <summary>
		/// Configure the HttpClient wrapped by this IFlurlClient.
		/// </summary>
		IFlurlClientBuilder ConfigureHttpClient(Action<HttpClient> configAction);

		/// <summary>
		/// Configure the inner-most HttpMessageHandler associated with this IFlurlClient.
		/// </summary>
#if NETCOREAPP2_1_OR_GREATER
		IFlurlClientBuilder ConfigureInnerHandler(Action<SocketsHttpHandler> configAction);
#else
		IFlurlClientBuilder ConfigureInnerHandler(Action<HttpClientHandler> configAction);
#endif

		/// <summary>
		/// Add a provided DelegatingHandler to the IFlurlClient.
		/// </summary>
		IFlurlClientBuilder AddMiddleware(Func<DelegatingHandler> create);

		/// <summary>
		/// Builds an instance of IFlurlClient based on configurations specified.
		/// </summary>
		IFlurlClient Build();
	}

	/// <summary>
	/// Default implementation of IFlurlClientBuilder.
	/// </summary>
	public class FlurlClientBuilder : IFlurlClientBuilder
	{
		private readonly IFlurlClientFactory _factory;
		private readonly string _baseUrl;
		private readonly List<Func<DelegatingHandler>> _addMiddleware = new();
		private readonly List<Action<FlurlHttpSettings>> _configSettings = new();
		private readonly List<Action<HttpClient>> _configClient = new();
#if NETCOREAPP2_1_OR_GREATER
		private readonly HandlerBuilder<SocketsHttpHandler> _handlerBuilder = new();
#else
		private readonly HandlerBuilder<HttpClientHandler> _handlerBuilder = new();
#endif

		/// <summary>
		/// Creates a new FlurlClientBuilder.
		/// </summary>
		public FlurlClientBuilder(string baseUrl = null) : this(new DefaultFlurlClientFactory(),  baseUrl) { }

		/// <summary>
		/// Creates a new FlurlClientBuilder.
		/// </summary>
		internal FlurlClientBuilder(IFlurlClientFactory factory, string baseUrl) {
			_factory = factory;
			_baseUrl = baseUrl;
		}

		/// <inheritdoc />
		public IFlurlClientBuilder WithSettings(Action<FlurlHttpSettings> configAction) {
			_configSettings.Add(configAction);
			return this;
		}

		/// <inheritdoc />
		public IFlurlClientBuilder AddMiddleware(Func<DelegatingHandler> create) {
			_addMiddleware.Add(create);
			return this;
		}

		/// <inheritdoc />
		public IFlurlClientBuilder ConfigureHttpClient(Action<HttpClient> configAction) {
			_configClient.Add(configAction);
			return this;
		}

		/// <inheritdoc />
#if NETCOREAPP2_1_OR_GREATER
		public IFlurlClientBuilder ConfigureInnerHandler(Action<SocketsHttpHandler> configAction) {
#else
		public IFlurlClientBuilder ConfigureInnerHandler(Action<HttpClientHandler> configAction) {
#endif
			_handlerBuilder.Configs.Add(configAction);
			return this;
		}

		/// <inheritdoc />
		public IFlurlClient Build() {
			var outerHandler = _handlerBuilder.Build(_factory);

			foreach (var middleware in Enumerable.Reverse(_addMiddleware).Select(create => create())) {
				middleware.InnerHandler = outerHandler;
				outerHandler = middleware;
			}

			var httpCli = _factory.CreateHttpClient(outerHandler);
			foreach (var config in _configClient)
				config(httpCli);

			var flurlCli = new FlurlClient(httpCli, _baseUrl);
			foreach (var config in _configSettings)
				config(flurlCli.Settings);

			return flurlCli;
		}

		// helper class to keep those compiler switches from getting too messy
		private class HandlerBuilder<T> where T : HttpMessageHandler
		{
			public List<Action<T>> Configs { get; } = new();

			public HttpMessageHandler Build(IFlurlClientFactory fac) {
				var handler = fac.CreateInnerHandler();
				foreach (var config in Configs) {
					if (handler is T h)
						config(h);
					else
						throw new Exception($"ConfigureInnerHandler expected an instance of {typeof(T).Name} but received an instance of {handler.GetType().Name}.");
				}
				return handler;
			}
		}
	}
}
