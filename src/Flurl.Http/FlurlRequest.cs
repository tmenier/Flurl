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
	/// Represents an HTTP request. Can be created explicitly via new FlurlRequest(), fluently via Url.Request(),
	/// or implicitly when a call is made via methods like Url.GetAsync().
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
		/// Asynchronously sends the HTTP request.
		/// Mainly used to implement higher-level extension methods (GetJsonAsync, etc).
		/// </summary>
		/// <param name="verb">The HTTP method used to make the request.</param>
		/// <param name="content">Contents of the request body.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
		/// <param name="completionOption">The HttpCompletionOption used in the request. Optional.</param>
		/// <returns>A Task whose result is the received IFlurlResponse.</returns>
		Task<IFlurlResponse> SendAsync(HttpMethod verb, HttpContent content = null, CancellationToken cancellationToken = default(CancellationToken), HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead);
	}

	/// <inheritdoc />
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
		public IDictionary<string, Cookie> Cookies { get; } = new Dictionary<string, Cookie>();

		/// <inheritdoc />
		public async Task<IFlurlResponse> SendAsync(HttpMethod verb, HttpContent content = null, CancellationToken cancellationToken = default(CancellationToken), HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead) {
			_client = Client; // "freeze" the client at this point to avoid excessive calls to FlurlClientFactory.Get (#374)

			while (true) { // loop for redirects
				var request = new HttpRequestMessage(verb, Url) { Content = content };
				var call = new FlurlCall { Request = this, HttpRequestMessage = request };
				request.SetHttpCall(call);

				await HandleEventAsync(Settings.BeforeCall, Settings.BeforeCallAsync, call).ConfigureAwait(false);
				request.RequestUri = Url.ToUri(); // in case it was modified in the handler above

				var cancellationTokenWithTimeout = cancellationToken;
				CancellationTokenSource timeoutTokenSource = null;

				if (Settings.Timeout.HasValue) {
					timeoutTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
					timeoutTokenSource.CancelAfter(Settings.Timeout.Value);
					cancellationTokenWithTimeout = timeoutTokenSource.Token;
				}

				call.StartedUtc = DateTime.UtcNow;
				try {
					Headers.Merge(Client.Headers);
					foreach (var header in Headers)
						request.SetHeader(header.Key, header.Value);

					if (Settings.CookiesEnabled)
						WriteRequestCookies(call);

					call.HttpResponseMessage = await Client.HttpClient.SendAsync(request, completionOption, cancellationTokenWithTimeout).ConfigureAwait(false);
					call.HttpResponseMessage.RequestMessage = request;
					call.Response = new FlurlResponse(call.HttpResponseMessage);

					if (Settings.CookiesEnabled)
						ReadResponseCookies(call);

					// TODO: need a flurl-level auto-redirect setting (default true)
					if (IsRedirecting(call, out var redirectUrl)) {
						this.Url = redirectUrl;
						this.WithCookies(call.Response.Cookies);
					}
					else if (call.Succeeded)
						return call.Response;
					else
						throw new FlurlHttpException(call, null);
				}
				catch (Exception ex) {
					return await HandleExceptionAsync(call, ex, cancellationToken);
				}
				finally {
					request.Dispose();
					timeoutTokenSource?.Dispose();

					call.EndedUtc = DateTime.UtcNow;
					await HandleEventAsync(Settings.AfterCall, Settings.AfterCallAsync, call).ConfigureAwait(false);
				}
			}
		}

		// largely lifted from https://github.com/dotnet/runtime/blob/master/src/libraries/System.Net.Http/src/System/Net/Http/SocketsHttpHandler/RedirectHandler.cs
		private bool IsRedirecting(FlurlCall call, out Url redirectUrl) {
			redirectUrl = null;

			if (!call.IsRedirect)
				return false;

			if (!call.Response.Headers.TryGetValue("Location", out var location))
				return false;

			if (Url.IsValid(location))
				redirectUrl = new Url(location);
			else if (location.StartsWith("/"))
				redirectUrl = new Url(this.Url.Root).AppendPathSegment(location);
			else
				redirectUrl = new Url(this.Url.Root).AppendPathSegments(this.Url.Path, location);

			// Per https://tools.ietf.org/html/rfc7231#section-7.1.2, a redirect location without a
			// fragment should inherit the fragment from the original URI.
			redirectUrl.Fragment = this.Url.Fragment;

			// Disallow automatic redirection from secure to non-secure schemes
			if (this.Url.IsSecureScheme && !redirectUrl.IsSecureScheme)
				return false;

			return true;
		}

		private void WriteRequestCookies(FlurlCall call) {
			Cookies.Merge(Client.Cookies);

			if (!Cookies.Any()) return;
			var uri = call.HttpRequestMessage.RequestUri;

			//var jar = GetCookieJar(Client.HttpMessageHandler);

			//if (jar != null) {
			//	Cookies.Merge(Client.Cookies);
			//	foreach (var cookie in Cookies.Values)
			//		jar.Add(uri, cookie);
			//}
			//else {
				// no CookieContainer in play, add cookie headers manually
				// http://stackoverflow.com/a/15588878/62600
				call.HttpRequestMessage.Headers.TryAddWithoutValidation("Cookie", string.Join("; ", Cookies.Values));
			//}
		}

		private void ReadResponseCookies(FlurlCall call) {
			var uri = call.HttpRequestMessage.RequestUri;
			if (uri == null)
				return;

			// if there's a CookieContainer in play, cookies have probably been removed from the headers and put there.
			//var jar = GetCookieJar(Client.HttpMessageHandler) ?? new CookieContainer();

			// enlist CookieContainer to help with parsing
			var jar = new CookieContainer();

			// http://stackoverflow.com/a/15588878/62600
			if (call.HttpResponseMessage.Headers.TryGetValues("Set-Cookie", out var cookieHeaders)) {
				foreach (string header in cookieHeaders)
					jar.SetCookies(uri, header);
			}

			foreach (var cookie in jar.GetCookies(uri).Cast<Cookie>())
				call.Response.Cookies[cookie.Name] = cookie;
		}

		private CookieContainer GetCookieJar(HttpMessageHandler handler) {
			// if it's an HttpClientHandler, return its CookieContainer
			if (handler is HttpClientHandler hch) {
				if (hch.UseCookies && hch.CookieContainer == null)
					hch.CookieContainer = new CookieContainer();
				return hch.CookieContainer;
			}

			// if it's a DelegatingHandler, check the InnerHandler recursively
			if (handler is DelegatingHandler dh)
				return GetCookieJar(dh.InnerHandler);

			// it's neither
			return null;
		}

		internal static async Task<IFlurlResponse> HandleExceptionAsync(FlurlCall call, Exception ex, CancellationToken token) {
			call.Exception = ex;
			await HandleEventAsync(call.Request.Settings.OnError, call.Request.Settings.OnErrorAsync, call).ConfigureAwait(false);

			if (call.ExceptionHandled)
				return call.Response;

			if (ex is OperationCanceledException && !token.IsCancellationRequested)
				throw new FlurlHttpTimeoutException(call, ex);

			if (ex is FlurlHttpException)
				throw ex;

			throw new FlurlHttpException(call, ex);
		}

		private static Task HandleEventAsync(Action<FlurlCall> syncHandler, Func<FlurlCall, Task> asyncHandler, FlurlCall call) {
			syncHandler?.Invoke(call);
			if (asyncHandler != null)
				return asyncHandler(call);
			return Task.FromResult(0);
		}
	}
}