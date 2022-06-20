using System;
using System.Net.Http;
using Flurl.Http.Configuration;
using Flurl.Http.Testing;
using Flurl.Util;

namespace Flurl.Http
{
	/// <summary>
	/// Interface defining FlurlClient's contract (useful for mocking and DI)
	/// </summary>
	public interface IFlurlClient : IHttpSettingsContainer, IDisposable {
		/// <summary>
		/// Gets or sets the FlurlHttpSettings object used by this client.
		/// </summary>
		new ClientFlurlHttpSettings Settings { get; set; }

		/// <summary>
		/// Gets the HttpClient to be used in subsequent HTTP calls. Creation (when necessary) is delegated
		/// to FlurlHttp.FlurlClientFactory. Reused for the life of the FlurlClient.
		/// </summary>
		HttpClient HttpClient { get; }

		/// <summary>
		/// Gets the HttpMessageHandler to be used in subsequent HTTP calls. Creation (when necessary) is delegated
		/// to FlurlHttp.FlurlClientFactory.
		/// </summary>
		HttpMessageHandler HttpMessageHandler { get; }

		/// <summary>
		/// Gets or sets base URL associated with this client.
		/// </summary>
		string BaseUrl { get; set; }

		/// <summary>
		/// Creates a new IFlurlRequest that can be further built and sent fluently.
		/// </summary>
		/// <param name="urlSegments">The URL or URL segments for the request. If BaseUrl is defined, it is assumed that these are path segments off that base.</param>
		/// <returns>A new IFlurlRequest</returns>
		IFlurlRequest Request(params object[] urlSegments);

		/// <summary>
		/// Gets a value indicating whether this instance (and its underlying HttpClient) has been disposed.
		/// </summary>
		bool IsDisposed { get; }
	}

	/// <summary>
	/// A reusable object for making HTTP calls.
	/// </summary>
	public class FlurlClient : IFlurlClient
	{
		private ClientFlurlHttpSettings _settings;
		private Lazy<HttpClient> _httpClient;
		private Lazy<HttpMessageHandler> _httpMessageHandler;

		// if an existing HttpClient is provided on construction, skip the lazy logic and just use that.
		private readonly HttpClient _injectedClient;

		/// <summary>
		/// Initializes a new instance of the <see cref="FlurlClient"/> class.
		/// </summary>
		/// <param name="baseUrl">The base URL associated with this client.</param>
		public FlurlClient(string baseUrl = null) {
			_httpClient = new Lazy<HttpClient>(() => Settings.HttpClientFactory.CreateHttpClient(HttpMessageHandler));
			_httpMessageHandler = new Lazy<HttpMessageHandler>(() => Settings.HttpClientFactory.CreateMessageHandler());
			BaseUrl = baseUrl;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FlurlClient"/> class, wrapping an existing HttpClient.
		/// Generally you should let Flurl create and manage HttpClient instances for you, but you might, for
		/// example, have an HttpClient instance that was created by a 3rd-party library and you want to use
		/// Flurl to build and send calls with it.
		/// </summary>
		/// <param name="httpClient">The instantiated HttpClient instance.</param>
		public FlurlClient(HttpClient httpClient) {
			_injectedClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			BaseUrl = httpClient.BaseAddress?.ToString();
		}

		/// <inheritdoc />
		public string BaseUrl { get; set; }

		/// <inheritdoc />
		public ClientFlurlHttpSettings Settings {
			get => _settings ??= new ClientFlurlHttpSettings();
			set => _settings = value;
		}

		/// <inheritdoc />
		public INameValueList<string> Headers { get; } = new NameValueList<string>(false); // header names are case-insensitive https://stackoverflow.com/a/5259004/62600

		/// <inheritdoc />
		public HttpClient HttpClient => HttpTest.Current?.HttpClient ?? _injectedClient ?? _httpClient.Value;

		/// <inheritdoc />
		public HttpMessageHandler HttpMessageHandler => HttpTest.Current?.HttpMessageHandler ?? _httpMessageHandler?.Value;

		/// <inheritdoc />
		public IFlurlRequest Request(params object[] urlSegments) => new FlurlRequest(BaseUrl, urlSegments) { Client = this };

		FlurlHttpSettings IHttpSettingsContainer.Settings {
			get => Settings;
			set => Settings = value as ClientFlurlHttpSettings;
		}

		/// <inheritdoc />
		public bool IsDisposed { get; private set; }

		/// <summary>
		/// Disposes the underlying HttpClient and HttpMessageHandler.
		/// </summary>
		public virtual void Dispose() {
			if (IsDisposed)
				return;

			_injectedClient?.Dispose();
			if (_httpMessageHandler?.IsValueCreated == true)
				_httpMessageHandler.Value.Dispose();
			if (_httpClient?.IsValueCreated == true)
				_httpClient.Value.Dispose();

			IsDisposed = true;
		}
	}
}