using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
			this.AllowedHttpStatusRanges = new List<string>();
			if (FlurlHttp.Configuration.AllowedHttpStatusRange != null)
				this.AllowedHttpStatusRanges.Add(FlurlHttp.Configuration.AllowedHttpStatusRange);
		}

		public FlurlClient(string url, bool autoDispose) : this(new Url(url), autoDispose) { }
		public FlurlClient(Url url) : this(url, false) { }
		public FlurlClient(string url) : this(new Url(url), false) { }
		public FlurlClient() : this((Url)null, false) { }

		private HttpClient _httpClient;
		private HttpMessageHandler _httpMessageHandler;

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
				if (_httpClient == null)
					_httpClient = FlurlHttp.Configuration.HttpClientFactory.CreateClient(Url, HttpMessageHandler);
				return _httpClient;
			}
		}

		/// <summary>
		/// Encapsulates pattern for making an HTTP call and immediately disposing if AutoDispose is true.
		/// Primarily used by FlurlClient extension methods, not directly in application code.
		/// </summary>
		/// <typeparam name="T">Type (wrapped in a Task) returned in underlying async HTTP call.</typeparam>
		/// <param name="func">Underlying async call made against an HttpClient.</param>
		/// <returns></returns>
		public async Task<HttpResponseMessage> SendAsync(HttpMethod verb, HttpContent content = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead) {
			try {
				var request = new HttpRequestMessage(verb, this.Url) { Content = content };
				// mechanism for passing ad-hoc data to the MessageHandler
				request.Properties.Add("AllowedHttpStatusRanges", AllowedHttpStatusRanges);
				return await HttpClient.SendAsync(request, completionOption, CancellationToken.None);
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
					_httpMessageHandler = FlurlHttp.Configuration.HttpClientFactory.CreateMessageHandler();

				return _httpMessageHandler;
			}			
		}

		/// <summary>
		/// Gets a collection of pattern representing HTTP status codes (or ranges) which, in addition to 2xx, will NOT result in FlurlHttpException being thrown.
		/// Examples: "3xx", "100,300,600", "100-299,6xx", "*" (allow everything)
		/// 2xx will never throw regardless of this setting.
		/// </summary>
		public ICollection<string> AllowedHttpStatusRanges { get; private set; }

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
