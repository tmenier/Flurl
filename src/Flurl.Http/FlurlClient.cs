using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http.Configuration;
using Flurl.Http.Testing;

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
	}

	/// <summary>
	/// A chainable wrapper around HttpClient and Flurl.Url.
	/// </summary>
	public class FlurlClient : IFlurlClient
	{
		private HttpClient _httpClient;
		private HttpMessageHandler _httpMessageHandler;

		/// <summary>
		/// Initializes a new instance of the <see cref="FlurlClient"/> class.
		/// </summary>
		/// <param name="settings">The FlurlHttpSettings associated with this instance.</param>
		public FlurlClient(FlurlHttpSettings settings = null) {
			Settings = settings ?? new FlurlHttpSettings(FlurlHttp.GlobalSettings);
			HttpMessageHandler = Settings.HttpClientFactory.CreateMessageHandler();
			HttpClient = Settings.HttpClientFactory.CreateHttpClient(HttpMessageHandler);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FlurlClient"/> class.
		/// </summary>
		/// <param name="configure">Action allowing you to overide default settings inline.</param>
		public FlurlClient(Action<FlurlHttpSettings> configure) : this() {
			configure(Settings);
		}

		/// <summary>
		/// Gets or sets the FlurlHttpSettings object used by this client.
		/// </summary>
		public FlurlHttpSettings Settings { get; set; }

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
		public HttpClient HttpClient { get; }

		/// <summary>
		/// Gets the HttpMessageHandler to be used in subsequent HTTP calls. Creation (when necessary) is delegated
		/// to FlurlHttp.FlurlClientFactory.
		/// </summary>
		public HttpMessageHandler HttpMessageHandler { get; }

		//private HttpClient EnsureHttpClient(HttpClient hc = null) {
		//	if (_httpClient == null) {
		//		if (hc == null) {
		//			hc = Settings.FlurlClientFactory.CreateHttpClient(Url, HttpMessageHandler);
		//			hc.Timeout = Settings.DefaultTimeout;
		//		}
		//		_httpClient = hc;
		//		_parent?.EnsureHttpClient(hc);
		//	}
		//	return _httpClient;
		//}

		//private HttpMessageHandler EnsureHttpMessageHandler(HttpMessageHandler handler = null) {
		//	if (_httpMessageHandler == null) {
		//		if (handler == null) {
		//			handler = (HttpTest.Current == null) ?
		//				Settings.FlurlClientFactory.CreateMessageHandler() :
		//				new FakeHttpMessageHandler();
		//		}
		//		_httpMessageHandler = handler;
		//		_parent?.EnsureHttpMessageHandler(handler);
		//	}
		//	return _httpMessageHandler;
		//}

		/// <summary>
		/// Disposes the underlying HttpClient and HttpMessageHandler, setting both properties to null.
		/// </summary>
		public void Dispose() {
			_httpMessageHandler?.Dispose();
			_httpClient?.Dispose();
			_httpMessageHandler = null;
			_httpClient = null;
			Cookies = new Dictionary<string, Cookie>();
		}
	}
}