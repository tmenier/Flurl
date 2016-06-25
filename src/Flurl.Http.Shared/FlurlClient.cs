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
		/// <summary>
		/// Initializes a new instance of the <see cref="FlurlClient"/> class.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="autoDispose">if set to <c>true</c> [automatic dispose].</param>
		public FlurlClient(Url url, bool autoDispose) {
			Url = url;
			AutoDispose = autoDispose;
			Settings = FlurlHttp.GlobalSettings.Clone();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FlurlClient"/> class.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="autoDispose">if set to <c>true</c> [automatic dispose].</param>
		/// <exception cref="ArgumentNullException"><paramref name="url" /> is <see langword="null" />.</exception>
		public FlurlClient(string url, bool autoDispose) : this(new Url(url), autoDispose) { }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="FlurlClient"/> class.
		/// </summary>
		/// <param name="url">The URL.</param>
		public FlurlClient(Url url) : this(url, false) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="FlurlClient"/> class.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <exception cref="ArgumentNullException"><paramref name="url" /> is <see langword="null" />.</exception>
		public FlurlClient(string url) : this(new Url(url), false) { }
		
		/// <summary>
		/// Initializes a new instance of the <see cref="FlurlClient"/> class.
		/// </summary>
		public FlurlClient() : this((Url)null, false) { }

		/// <summary>
		/// Creates a copy of this FlurlClient with a shared instance of HttpClient and HttpMessageHandler
		/// </summary>

		public FlurlClient Clone() {
			return new FlurlClient {
				_httpClient = _httpClient,
				_httpMessageHandler = _httpMessageHandler,
				_parent = this,
				Settings = Settings,
				Url = Url,
				Cookies = Cookies,
				AutoDispose = AutoDispose
			};
		}

		private HttpClient _httpClient;
		private HttpMessageHandler _httpMessageHandler;
		private FlurlClient _parent;

		/// <summary>
		/// Gets or sets the FlurlHttpSettings object used by this client.
		/// </summary>
		public FlurlHttpSettings Settings { get; set; }

		/// <summary>
		/// Gets or sets the URL to be called.
		/// </summary>
		public Url Url { get; set; }

		/// <summary>
		/// Collection of HttpCookies sent and received.
		/// </summary>
		public IDictionary<string, Cookie> Cookies { get; private set; } = new Dictionary<string, Cookie>();

		/// <summary>
		/// Gets a value indicating whether the underlying HttpClient
		/// should be disposed immediately after the first HTTP call is made.
		/// </summary>
		public bool AutoDispose { get; set; }

		/// <summary>
		/// Gets the HttpClient to be used in subsequent HTTP calls. Creation (when necessary) is delegated
		/// to FlurlHttp.HttpClientFactory. Reused for the life of the FlurlClient.
		/// </summary>
		public HttpClient HttpClient => EnsureHttpClient();

		private HttpClient EnsureHttpClient(HttpClient hc = null) {
			if (_httpClient == null) {
				if (hc == null) {
					hc = Settings.HttpClientFactory.CreateClient(Url, HttpMessageHandler);
					hc.Timeout = Settings.DefaultTimeout;
				}
				_httpClient = hc;
				_parent?.EnsureHttpClient(hc);
			}
			return _httpClient;
		}

	    /// <summary>
	    /// Creates and asynchronously sends an HttpRequestMethod, disposing HttpClient if AutoDispose it true.
	    /// Mainly used to implement higher-level extension methods (GetJsonAsync, etc).
	    /// </summary>
	    /// <returns>A Task whose result is the received HttpResponseMessage.</returns>
	    public async Task<HttpResponseMessage> SendAsync(HttpMethod verb, HttpContent content = null, CancellationToken? cancellationToken = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead) {
			try {
				var request = new HttpRequestMessage(verb, Url) { Content = content };
				if (Settings.CookiesEnabled)
					WriteRequestCookies(request);
				HttpCall.Set(request, Settings);
				var resp = await HttpClient.SendAsync(request, completionOption, cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
				if (Settings.CookiesEnabled)
					ReadResponseCookies(resp);
				return resp;
			}
			finally {
				if (AutoDispose) Dispose();
			}
		}

		private void WriteRequestCookies(HttpRequestMessage request) {
			if (!Cookies.Any()) return;
			var uri = request.RequestUri;
			var cookieHandler = HttpMessageHandler as HttpClientHandler;

			// if the inner handler is an HttpClientHandler (which it usually is), put the cookies in the CookieContainer.
			if (cookieHandler != null && cookieHandler.UseCookies) {
				if (cookieHandler.CookieContainer == null)
					cookieHandler.CookieContainer = new CookieContainer();
				foreach (var cookie in Cookies.Values)
					cookieHandler.CookieContainer.Add(uri, cookie);
			}
			else {
				// http://stackoverflow.com/a/15588878/62600
				request.Headers.TryAddWithoutValidation("Cookie", string.Join("; ", Cookies.Values));
			}
		}

		private void ReadResponseCookies(HttpResponseMessage response) {
			var uri = response.RequestMessage.RequestUri;

			// if the inner handler is an HttpClientHandler (which it usually is), it's already plucked the
			// cookies out of the headers and put them in the CookieContainer.
			var jar = (HttpMessageHandler as HttpClientHandler)?.CookieContainer;
			if (jar == null) {
				// http://stackoverflow.com/a/15588878/62600
				IEnumerable<string> cookieHeaders;
				if (!response.Headers.TryGetValues("Set-Cookie", out cookieHeaders))
					return;

				jar = new CookieContainer();
				foreach (string header in cookieHeaders) {
					jar.SetCookies(uri, header);
				}
			}

			foreach (var cookie in jar.GetCookies(uri).Cast<Cookie>())
				Cookies[cookie.Name] = cookie;
		}

		/// <summary>
		/// Gets the HttpMessageHandler to be used in subsequent HTTP calls. Creation (when necessary) is delegated
		/// to FlurlHttp.HttpClientFactory.
		/// </summary>
		public HttpMessageHandler HttpMessageHandler => EnsureHttpMessageHandler();

		private HttpMessageHandler EnsureHttpMessageHandler(HttpMessageHandler hmh = null) {
			if (_httpMessageHandler == null) {
				if (hmh == null)
					hmh = Settings.HttpClientFactory.CreateMessageHandler();
				_httpMessageHandler = hmh;
				_parent?.EnsureHttpMessageHandler(hmh);
			}
			return _httpMessageHandler;
		}

		/// <summary>
		/// Disposes the underlying HttpClient and HttpMessageHandler, setting both properties to null.
		/// This FlurlClient can still be reused, but those underlying objects will be re-created as needed. Previously set headers, etc, will be lost.
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