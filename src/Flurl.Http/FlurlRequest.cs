using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http.Configuration;
using Flurl.Util;

namespace Flurl.Http
{
	/// <summary>
	/// Interface defining FlurlRequest's contract (useful for mocking and DI)
	/// </summary>
	public interface IFlurlRequest : IHttpSettingsContainer
	{
		/// <summary>
		/// Gets or sets the IFlurlClient to use when sending the request.
		/// </summary>
		IFlurlClient Client { get; set; }

		/// <summary>
		/// Gets or sets the URL to be called.
		/// </summary>
		Url Url { get; set; }

		/// <summary>
		/// Creates and asynchronously sends an HttpRequestMethod.
		/// Mainly used to implement higher-level extension methods (GetJsonAsync, etc).
		/// </summary>
		/// <param name="verb">The HTTP method used to make the request.</param>
		/// <param name="content">Contents of the request body.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
		/// <param name="completionOption">The HttpCompletionOption used in the request. Optional.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		Task<HttpResponseMessage> SendAsync(HttpMethod verb, HttpContent content = null, CancellationToken cancellationToken = default(CancellationToken), HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead);
	}

	/// <summary>
	/// A chainable wrapper around HttpClient and Flurl.Url.
	/// </summary>
	public class FlurlRequest : IFlurlRequest
	{
		private FlurlHttpSettings _settings;
		private IFlurlClient _client;
		private Url _url;

		/// <summary>
		/// Initializes a new instance of the <see cref="FlurlRequest"/> class.
		/// </summary>
		/// <param name="url">The URL to call with this FlurlRequest instance.</param>
		public FlurlRequest(Url url = null) {
			_url = url;
		}

		/// <summary>
		/// Gets or sets the FlurlHttpSettings used by this request.
		/// </summary>
		public FlurlHttpSettings Settings {
			get {
				if (_settings == null) {
					_settings = new FlurlHttpSettings();
					ResetDefaultSettings();
				}
				return _settings;
			}
			set {
				_settings = value;
				ResetDefaultSettings();
			}
		}

		/// <inheritdoc />
		public IFlurlClient Client {
			get => 
				(_client != null) ? _client :
				(Url != null) ? FlurlHttp.GlobalSettings.FlurlClientFactory.Get(Url) :
				null;
			set {
				_client = value;
				ResetDefaultSettings();
			}
		}

		/// <inheritdoc />
		public Url Url {
			get => _url;
			set {
				_url = value;
				ResetDefaultSettings();
			}
		}

		private void ResetDefaultSettings() {
			if (_settings != null)
				_settings.Defaults = Client?.Settings;
		}

		/// <summary>
		/// Collection of headers sent on this request.
		/// </summary>
		public IDictionary<string, object> Headers { get; } = new Dictionary<string, object>();

		/// <summary>
		/// Collection of HttpCookies sent and received by the IFlurlClient associated with this request.
		/// </summary>
		public IDictionary<string, Cookie> Cookies => Client.Cookies;

		/// <inheritdoc />
		public async Task<HttpResponseMessage> SendAsync(HttpMethod verb, HttpContent content = null, CancellationToken cancellationToken = default(CancellationToken), HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead) {
			var request = new HttpRequestMessage(verb, Url) { Content = content };
			var call = new HttpCall { FlurlRequest = this, Request = request };
			request.SetHttpCall(call);

			await HandleEventAsync(Settings.BeforeCall, Settings.BeforeCallAsync, call).ConfigureAwait(false);
			request.RequestUri = Url.ToUri(); // in case it was modifed in the handler above

			var cancellationTokenWithTimeout = cancellationToken;

			if (Settings.Timeout.HasValue) {
				var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
				cts.CancelAfter(Settings.Timeout.Value);
				cancellationTokenWithTimeout = cts.Token;
			}

			call.StartedUtc = DateTime.UtcNow;
			try {
				Headers.Merge(Client.Headers);
				foreach (var header in Headers)
					request.SetHeader(header.Key, header.Value);

				if (Settings.CookiesEnabled)
					WriteRequestCookies(request);

				call.Response = await Client.HttpClient.SendAsync(request, completionOption, cancellationTokenWithTimeout).ConfigureAwait(false);
				call.Response.RequestMessage = request;

				if (call.Succeeded)
					return call.Response;

				throw new FlurlHttpException(call, null);
			}
			catch (Exception ex) {
				return await HandleExceptionAsync(call, ex, cancellationToken);
			}
			finally {
				request.Dispose();
				if (Settings.CookiesEnabled)
					ReadResponseCookies(call.Response);

				call.EndedUtc = DateTime.UtcNow;
				await HandleEventAsync(Settings.AfterCall, Settings.AfterCallAsync, call).ConfigureAwait(false);
			}
		}

		private void WriteRequestCookies(HttpRequestMessage request) {
			if (!Cookies.Any()) return;
			var uri = request.RequestUri;
			var cookieHandler = FindHttpClientHandler(Client.HttpMessageHandler);

			// if the handler is an HttpClientHandler (which it usually is), put the cookies in the CookieContainer.
			if (cookieHandler != null && cookieHandler.UseCookies) {
				if (cookieHandler.CookieContainer == null)
					cookieHandler.CookieContainer = new CookieContainer();

				Cookies.Merge(Client.Cookies);
				foreach (var cookie in Cookies.Values)
					cookieHandler.CookieContainer.Add(uri, cookie);
			}
			else {
				// http://stackoverflow.com/a/15588878/62600
				request.Headers.TryAddWithoutValidation("Cookie", string.Join("; ", Cookies.Values));
			}
		}

		private void ReadResponseCookies(HttpResponseMessage response) {
			var uri = response?.RequestMessage?.RequestUri;
			if (uri == null)
				return;

			// if the handler is an HttpClientHandler (which it usually is), it's already plucked the
			// cookies out of the headers and put them in the CookieContainer.
			var jar = FindHttpClientHandler(Client.HttpMessageHandler)?.CookieContainer;
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

		private HttpClientHandler FindHttpClientHandler(HttpMessageHandler handler) {
			// if it's an HttpClientHandler, return it
			var httpClientHandler = handler as HttpClientHandler;
			if (httpClientHandler != null)
				return httpClientHandler;

			// if it's a DelegatingHandler, check the InnerHandler recursively
			var delegatingHandler = handler as DelegatingHandler;
			if (delegatingHandler != null)
				return FindHttpClientHandler(delegatingHandler.InnerHandler);

			// it's neither
			return null;
		}

		internal static async Task<HttpResponseMessage> HandleExceptionAsync(HttpCall call, Exception ex, CancellationToken token) {
			call.Exception = ex;
			await HandleEventAsync(call.FlurlRequest.Settings.OnError, call.FlurlRequest.Settings.OnErrorAsync, call).ConfigureAwait(false);

			if (call.ExceptionHandled)
				return call.Response;

			if (ex is OperationCanceledException && !token.IsCancellationRequested)
				throw new FlurlHttpTimeoutException(call, ex);

			if (ex is FlurlHttpException)
				throw ex;

			throw new FlurlHttpException(call, ex);
		}

		private static Task HandleEventAsync(Action<HttpCall> syncHandler, Func<HttpCall, Task> asyncHandler, HttpCall call) {
			syncHandler?.Invoke(call);
			if (asyncHandler != null)
				return asyncHandler(call);
			return Task.FromResult(0);
		}
	}
}