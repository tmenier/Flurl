using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Versioning;
using Flurl.Util;

namespace Flurl.Http.Configuration
{
	/// <summary>
	/// A builder for configuring IFlurlClient instances.
	/// </summary>
	public interface IFlurlClientBuilder : ISettingsContainer, IHeadersContainer
	{
		/// <summary>
		/// Configure the HttpClient wrapped by this IFlurlClient.
		/// </summary>
		IFlurlClientBuilder ConfigureHttpClient(Action<HttpClient> configure);

		/// <summary>
		/// Configure the inner-most HttpMessageHandler (an instance of HttpClientHandler) associated with this IFlurlClient.
		/// </summary>
		IFlurlClientBuilder ConfigureInnerHandler(Action<HttpClientHandler> configure);

#if NET
		/// <summary>
		/// Configure a SocketsHttpHandler instead of HttpClientHandler as the inner-most HttpMessageHandler.
		/// Note that HttpClientHandler has broader platform support and defers its work to SocketsHttpHandler
		/// on supported platforms. It is recommended to explicitly use SocketsHttpHandler ONLY if you
		/// need to directly configure its properties that aren't available on HttpClientHandler.
		/// </summary>
		[UnsupportedOSPlatform("browser")]
		IFlurlClientBuilder UseSocketsHttpHandler(Action<SocketsHttpHandler> configure);
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
		private IFlurlClientFactory _factory = new DefaultFlurlClientFactory();

		private readonly string _baseUrl;
		private readonly List<Func<DelegatingHandler>> _addMiddleware = new();
		private readonly List<Action<HttpClient>> _clientConfigs = new();
		private readonly List<Action<HttpMessageHandler>> _handlerConfigs = new();

		/// <inheritdoc />
		public FlurlHttpSettings Settings { get; } = new();

		/// <inheritdoc />
		public INameValueList<string> Headers { get; } = new NameValueList<string>(false); // header names are case-insensitive https://stackoverflow.com/a/5259004/62600

		/// <summary>
		/// Creates a new FlurlClientBuilder.
		/// </summary>
		public FlurlClientBuilder(string baseUrl = null) {
			_baseUrl = baseUrl;
		}

		/// <inheritdoc />
		public IFlurlClientBuilder AddMiddleware(Func<DelegatingHandler> create) {
			_addMiddleware.Add(create);
			return this;
		}

		/// <inheritdoc />
		public IFlurlClientBuilder ConfigureHttpClient(Action<HttpClient> configure) {
			_clientConfigs.Add(configure);
			return this;
		}

		/// <inheritdoc />
		public IFlurlClientBuilder ConfigureInnerHandler(Action<HttpClientHandler> configure) {
#if NET
			if (_factory is SocketsHandlerFlurlClientFactory && _handlerConfigs.Any())
				throw new FlurlConfigurationException("ConfigureInnerHandler and UseSocketsHttpHandler cannot be used together. The former configures and instance of HttpClientHandler and would be ignored when switching to SocketsHttpHandler.");
#endif
			_handlerConfigs.Add(h => configure(h as HttpClientHandler));
			return this;
		}

#if NET
		/// <inheritdoc />
		public IFlurlClientBuilder UseSocketsHttpHandler(Action<SocketsHttpHandler> configure) {
			if (!SocketsHttpHandler.IsSupported)
				throw new PlatformNotSupportedException("SocketsHttpHandler is not supported on one or more target platforms.");

			if (_factory is DefaultFlurlClientFactory && _handlerConfigs.Any())
				throw new FlurlConfigurationException("ConfigureInnerHandler and UseSocketsHttpHandler cannot be used together. The former configures and instance of HttpClientHandler and would be ignored when switching to SocketsHttpHandler.");

			if (!(_factory is SocketsHandlerFlurlClientFactory))
				_factory = new SocketsHandlerFlurlClientFactory();

			_handlerConfigs.Add(h => configure(h as SocketsHttpHandler));
			return this;
		}
#endif

		/// <inheritdoc />
		public IFlurlClient Build() {
			var outerHandler = _factory.CreateInnerHandler();
			foreach (var config in _handlerConfigs)
				config(outerHandler);

			foreach (var middleware in Enumerable.Reverse(_addMiddleware).Select(create => create())) {
				middleware.InnerHandler = outerHandler;
				outerHandler = middleware;
			}

			var httpCli = _factory.CreateHttpClient(outerHandler);
			foreach (var config in _clientConfigs)
				config(httpCli);

			return new FlurlClient(httpCli, _baseUrl, Settings, Headers);
		}
	}
}
