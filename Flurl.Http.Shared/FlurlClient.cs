using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http.Configuration;

namespace Flurl.Http
{
	/// <summary>
	/// A chainable wrapper around HttpClient and Flurl.Url.
	/// </summary>
	public class FlurlClient : IDisposable
	{
		public FlurlClient(Url url, bool autoDispose) {
			this.Url = url;
			this.AutoDispose = autoDispose;
			this.Settings = FlurlHttp.GlobalSettings.Clone();
		}

		public FlurlClient(string url, bool autoDispose) : this(new Url(url), autoDispose) { }
		public FlurlClient(Url url) : this(url, false) { }
		public FlurlClient(string url) : this(new Url(url), false) { }
		public FlurlClient() : this((Url)null, false) { }

		private HttpClient _httpClient;
		private HttpMessageHandler _httpMessageHandler;

		/// <summary>
		/// Gets or sets the FlurlHttpSettings object used by this client.
		/// </summary>
		public FlurlHttpSettings Settings { get; set; }

		/// <summary>
		/// Gets or sets the URL to be called.
		/// </summary>
		public Url Url { get; set; }

		/// <summary>
		/// Gets a value indicating whether the underlying HttpClient
		/// should be disposed immediately after the first HTTP call is made.
		/// </summary>
		public bool AutoDispose { get; private set; }

		/// <summary>
		/// Gets the HttpClient to be used in subsequent HTTP calls. Creation (when necessary) is delegated
		/// to FlurlHttp.HttpClientFactory. Reused for the life of the FlurlClient.
		/// </summary>
		public HttpClient HttpClient {
			get {
				if (_httpClient == null) {
					_httpClient = Settings.HttpClientFactory.CreateClient(Url, HttpMessageHandler);
					_httpClient.Timeout = Settings.DefaultTimeout;
				}
				return _httpClient;
			}
		}

		/// <summary>
		/// Creates and asynchronously sends an HttpRequestMethod, disposing HttpClient if AutoDispose it true.
		/// Mainly used to implement higher-level extension methods (GetJsonAsync, etc).
		/// </summary>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public async Task<HttpResponseMessage> SendAsync(HttpMethod verb, HttpContent content = null, CancellationToken? cancellationToken = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead) {
			try {
				var request = new HttpRequestMessage(verb, this.Url) { Content = content };
				request.SetFlurlSettings(this.Settings);
				return await HttpClient.SendAsync(request, completionOption, cancellationToken ?? CancellationToken.None);
			}
			finally {
				if (AutoDispose) Dispose();
			}
		}

		/// <summary>
		/// Gets the HttpMessageHandler to be used in subsequent HTTP calls. Creation (when necessary) is delegated
		/// to FlurlHttp.HttpClientFactory.
		/// </summary>
		public HttpMessageHandler HttpMessageHandler {
			get {
				if (_httpMessageHandler == null)
					_httpMessageHandler = Settings.HttpClientFactory.CreateMessageHandler();

				return _httpMessageHandler;
			}			
		}

		/// <summary>
		/// Disposes the underlying HttpClient and HttpMessageHandler, setting both properties to null.
		/// This FlurlClient can still be reused, but those underlying objects will be re-created as needed. Previously set headers, etc, will be lost.
		/// </summary>
		public void Dispose() {
			if (_httpMessageHandler != null)
				_httpMessageHandler.Dispose();

			if (_httpClient != null)
				_httpClient.Dispose();

			_httpMessageHandler = null;
			_httpClient = null;
		}
	}
}
