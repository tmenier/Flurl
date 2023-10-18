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
		IFlurlClientBuilder ConfigureInnerHandler(Action<HttpClientHandler> configAction);

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
		private readonly List<Action<HttpClient>> _configClient = new();
		private readonly List<Action<HttpClientHandler>> _configHandler = new();
		private readonly List<Action<FlurlHttpSettings>> _configSettings = new();

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
		public IFlurlClientBuilder ConfigureInnerHandler(Action<HttpClientHandler> configAction) {
			_configHandler.Add(configAction);
			return this;
		}

		/// <inheritdoc />
		public IFlurlClient Build() {
			var innerHandler = _factory.CreateInnerHandler();
			foreach (var config in _configHandler) {
				if (innerHandler is HttpClientHandler hch)
					config(hch);
				else
					throw new Exception("ConfigureInnerHandler can only be used when IFlurlClientFactory.CreateInnerHandler returns an instance of HttpClientFactory.");
			}

			HttpMessageHandler outerHandler = innerHandler;
			foreach (var createMW in Enumerable.Reverse(_addMiddleware)) {
				var middleware = createMW();
				middleware.InnerHandler = outerHandler;
				outerHandler = middleware;
			}

			var httpCli = _factory.CreateHttpClient(outerHandler);
			foreach (var config in _configClient)
				config(httpCli);

			var flurlCli = new FlurlClient(httpCli, _baseUrl);
			foreach (var config in _configSettings) {
				config(flurlCli.Settings);
			}

			return flurlCli;
		}
	}
}
