using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Linq;
using Flurl.Http.Configuration;
using Flurl.Util;

namespace Flurl.Http
{
	/// <summary>
	/// Interface defining FlurlClient's contract (useful for mocking and DI)
	/// </summary>
	public interface IFlurlClient : IHttpSettingsContainer, IDisposable {
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
		/// The base URL associated with this client.
		/// </summary>
		string BaseUrl { get; set; }

		/// <summary>
		/// Instantiates a new IFlurClient, optionally appending path segments to the BaseUrl.
		/// </summary>
		/// <param name="urlSegments">The URL or URL segments for the request. If BaseUrl is defined, it is assumed that these are path segments off that base.</param>
		/// <returns>A new IFlurlRequest</returns>
		IFlurlRequest Request(params object[] urlSegments);
	}

	/// <summary>
	/// A reusable object for making HTTP calls.
	/// </summary>
	public class FlurlClient : IFlurlClient
	{
		private IHttpClientFactory _httpClientFactory;
		private readonly Lazy<HttpClient> _httpClient;
		private readonly Lazy<HttpMessageHandler> _httpMessageHandler;

		/// <summary>
		/// Initializes a new instance of the <see cref="FlurlClient"/> class.
		/// </summary>
		/// <param name="baseUrl">The base URL associated with this client.</param>
		public FlurlClient(string baseUrl = null) {
			BaseUrl = baseUrl;
			Settings = new FlurlHttpSettings(FlurlHttp.GlobalSettings);
			_httpClient = new Lazy<HttpClient>(() => HttpClientFactory.CreateHttpClient(HttpMessageHandler));
			_httpMessageHandler = new Lazy<HttpMessageHandler>(() => HttpClientFactory.CreateMessageHandler());
		}

		/// <summary>
		/// The base URL associated with this client.
		/// </summary>
		public string BaseUrl { get; set; }

		/// <summary>
		/// Gets or sets the FlurlHttpSettings object used by this client.
		/// </summary>
		public FlurlHttpSettings Settings { get; set; }

		/// <summary>
		/// Gets or sets a factory used to create the HttpClient and HttpMessageHandler used for HTTP calls.
		/// Whenever possible, custom factory implementations should inherit from DefaultHttpClientFactory,
		/// only override the method(s) needed, call the base method, and modify the result.
		/// </summary>
		public IHttpClientFactory HttpClientFactory {
			get => _httpClientFactory ?? FlurlHttp.GlobalSettings.HttpClientFactory;
			set => _httpClientFactory = value;
		}

		/// <summary>
		/// Collection of headers sent on all requests using this client.
		/// </summary>
		public IDictionary<string, object> Headers { get; } = new Dictionary<string, object>();

		/// <summary>
		/// Collection of HttpCookies sent and received on all requests using this client.
		/// </summary>
		public IDictionary<string, Cookie> Cookies { get; private set; } = new Dictionary<string, Cookie>();

		/// <summary>
		/// Gets the HttpClient to be used in subsequent HTTP calls. Creation (when necessary) is delegated
		/// to FlurlHttp.FlurlClientFactory. Reused for the life of the FlurlClient.
		/// </summary>
		public HttpClient HttpClient => _httpClient.Value;

		/// <summary>
		/// Gets the HttpMessageHandler to be used in subsequent HTTP calls. Creation (when necessary) is delegated
		/// to FlurlHttp.FlurlClientFactory.
		/// </summary>
		public HttpMessageHandler HttpMessageHandler => _httpMessageHandler.Value;

		/// <summary>
		/// Instantiates a new IFlurClient, optionally appending path segments to the BaseUrl.
		/// </summary>
		/// <param name="urlSegments">The URL or URL segments for the request. If BaseUrl is defined, it is assumed that these are path segments off that base.</param>
		/// <returns>A new IFlurlRequest</returns>
		public IFlurlRequest Request(params object[] urlSegments) {
			if (!urlSegments.Any()) {
				if (string.IsNullOrEmpty(BaseUrl))
					throw new ArgumentException("Cannot create a Request. No URL segments were passed, and this Client does not have a BaseUrl defined.");
				return new FlurlRequest(this, BaseUrl);
			}

			if (!Url.IsValid(urlSegments[0]?.ToString())) {
				if (string.IsNullOrEmpty(BaseUrl))
					throw new ArgumentException("Cannot create a Request. This Client does not have a BaseUrl defined, and the first segment passed is not a valid URL.");
				return new FlurlRequest(this, BaseUrl.AppendPathSegments(urlSegments));
			}

			return new FlurlRequest(this, Url.Combine(urlSegments.Select(s => s.ToInvariantString()).ToArray()));
		}

		/// <summary>
		/// Disposes the underlying HttpClient and HttpMessageHandler.
		/// </summary>
		public void Dispose() {
			if (_httpMessageHandler.IsValueCreated)
				_httpMessageHandler.Value.Dispose();
			if (_httpClient.IsValueCreated)
				_httpClient.Value.Dispose();
		}
	}
}