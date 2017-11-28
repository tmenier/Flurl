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
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation. Optional.</param>
		/// <param name="completionOption">The HttpCompletionOption used in the request. Optional.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		Task<HttpResponseMessage> SendAsync(HttpMethod verb, HttpContent content = null, CancellationToken? cancellationToken = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead);
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
					MergeDefaultSettings();
				}
				return _settings;
			}
			set {
				_settings = value;
				MergeDefaultSettings();
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
				MergeDefaultSettings();
			}
		}

		/// <inheritdoc />
		public Url Url {
			get => _url;
			set {
				_url = value;
				MergeDefaultSettings();
			}
		}

		private void MergeDefaultSettings() {
			if (_settings != null)
				_settings.Defaults = Client?.Settings ?? FlurlHttp.GlobalSettings;
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
		public async Task<HttpResponseMessage> SendAsync(HttpMethod verb, HttpContent content = null, CancellationToken? cancellationToken = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead) {
			var request = new HttpRequestMessage(verb, Url) { Content = content };
			var call = new HttpCall(this, request);

			await HandleEventAsync(Settings.BeforeCall, Settings.BeforeCallAsync, call).ConfigureAwait(false);
			request.RequestUri = new Uri(Url); // in case it was modifed in the handler above

			var userToken = cancellationToken ?? CancellationToken.None;
			var token = userToken;

			if (Settings.Timeout.HasValue) {
				var cts = CancellationTokenSource.CreateLinkedTokenSource(userToken);
				cts.CancelAfter(Settings.Timeout.Value);
				token = cts.Token;
			}

			call.StartedUtc = DateTime.UtcNow;
			try {
				WriteHeaders(request);
				if (Settings.CookiesEnabled)
					WriteRequestCookies(request);

				if (Client.CheckAndRenewConnectionLease())
					request.Headers.ConnectionClose = true;

				call.Response = await Client.HttpClient.SendAsync(request, completionOption, token).ConfigureAwait(false);
				call.Response.RequestMessage = request;

				if (call.Succeeded)
					return call.Response;

				// response content is only awaited here if the call failed.
				if (call.Response.Content != null)
					call.ErrorResponseBody = await call.Response.Content.StripCharsetQuotes().ReadAsStringAsync().ConfigureAwait(false);

				throw new FlurlHttpException(call, null);
			}
			catch (Exception ex) {
				call.Exception = ex;
				await HandleEventAsync(Settings.OnError, Settings.OnErrorAsync, call).ConfigureAwait(false);

				if (call.ExceptionHandled)
					return call.Response;

				if (ex is OperationCanceledException && !userToken.IsCancellationRequested)
					throw new FlurlHttpTimeoutException(call, ex);

				if (ex is FlurlHttpException)
					throw;

				throw new FlurlHttpException(call, ex);
			}
			finally {
				request.Dispose();
				if (Settings.CookiesEnabled)
					ReadResponseCookies(call.Response);

				call.EndedUtc = DateTime.UtcNow;
				await HandleEventAsync(Settings.AfterCall, Settings.AfterCallAsync, call).ConfigureAwait(false);
			}
		}

		private void WriteHeaders(HttpRequestMessage request) {
			Headers.Merge(Client.Headers);
			foreach (var header in Headers.Where(h => h.Value != null))
				request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToInvariantString());
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

		private static Task HandleEventAsync(Action<HttpCall> syncHandler, Func<HttpCall, Task> asyncHandler, HttpCall call) {
			syncHandler?.Invoke(call);
			if (asyncHandler != null)
				return asyncHandler(call);
			return Task.FromResult(0);
		}
	}
}