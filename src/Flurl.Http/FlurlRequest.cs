using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
		/// The HTTP method of the request. Normally you don't need to set this explicitly; it will be set
		/// when you call the sending method (GetAsync, PostAsync, etc.)
		/// </summary>
		HttpMethod Verb { get; set; }

		/// <summary>
		/// Gets or sets the URL to be called.
		/// </summary>
		Url Url { get; set; }

		/// <summary>
		/// Collection of HTTP cookie values to be sent in this request's Cookie header. If a CookieJar is used, values
		/// from the jar that will be sent in this request will be sync'd to this collection automatically, but NOT
		/// vice-versa. Therefore, you can use this collection to override values set by the jar for this request only,
		/// but for a multi-request cookie "session" it is better to set values in the CookieJar and reuse it.
		/// </summary>
		IDictionary<string, object> Cookies { get; }

		/// <summary>
		/// Collection of HTTP cookies that can be shared between multiple requests. Automatically adds/updates cookies
		/// received via Set-Cookie headers in this response. Processes rules based on attributes (Domain, Path, Expires, etc.)
		/// to determine which cookies to send with this specific request, and synchronizes those with the Cookies collection.
		/// </summary>
		CookieJar CookieJar { get; set; }

		/// <summary>
		/// Asynchronously sends the HTTP request. Mainly used to implement higher-level extension methods (GetJsonAsync, etc).
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
		private CookieJar _cookieJar;
		private FlurlCall _redirectedFrom;

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
		public HttpMethod Verb { get; set; }

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

		/// <inheritdoc />
		public IDictionary<string, object> Headers { get; } = new ConcurrentDictionary<string, object>();

		/// <inheritdoc />
		public IDictionary<string, object> Cookies { get; } = new ConcurrentDictionary<string, object>();

		/// <inheritdoc />
		public CookieJar CookieJar {
			get => _cookieJar;
			set {
				_cookieJar?.UnsyncWith(this);
				_cookieJar = value;
				_cookieJar?.SyncWith(this);
			}
		}
		
		/// <inheritdoc />
		public async Task<IFlurlResponse> SendAsync(HttpMethod verb, HttpContent content = null, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead) {
			_client = Client; // "freeze" the client at this point to avoid excessive calls to FlurlClientFactory.Get (#374)
			Verb = verb;

			var request = new HttpRequestMessage(verb, Url) { Content = content };
			var call = new FlurlCall {
				Request = this,
				RedirectedFrom = _redirectedFrom,
				HttpRequestMessage = request
			};
			request.SetHttpCall(call);

			await RaiseEventAsync(Settings.BeforeCall, Settings.BeforeCallAsync, call).ConfigureAwait(false);
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
				SyncHeadersAndCookies(request);
				_cookieJar?.UnsyncWith(this);

				var response = await Client.HttpClient.SendAsync(request, completionOption, cancellationTokenWithTimeout).ConfigureAwait(false);
				call.HttpResponseMessage = response;
				call.HttpResponseMessage.RequestMessage = request;
				call.Response = new FlurlResponse(call.HttpResponseMessage, _cookieJar);

				if (Settings.Redirects.Enabled)
					call.Redirect = GetRedirect(call);

				if (call.Redirect != null)
					await RaiseEventAsync(Settings.OnRedirect, Settings.OnRedirectAsync, call).ConfigureAwait(false);

				if (call.Redirect?.Follow == true) {
					CheckForCircularRedirects(call);

					var redir = new FlurlRequest(call.Redirect.Url)
						.WithHeaders(this.Headers)
						.WithCookies(call.Response.Cookies) as FlurlRequest;

					redir.Client = Client;
					redir._redirectedFrom = call;
					redir.Settings.Defaults = Settings;

					var changeToGet = call.Redirect.ChangeVerbToGet;

					if (!Settings.Redirects.ForwardAuthorizationHeader)
						redir.Headers.Remove("Authorization");
					if (changeToGet)
						redir.Headers.Remove("Transfer-Encoding");

					return await redir.SendAsync(
						changeToGet ? HttpMethod.Get : verb,
						changeToGet ? null : content,
						cancellationToken,
						completionOption).ConfigureAwait(false);
				}
				else if (call.Succeeded)
					return call.Response;
				else
					throw new FlurlHttpException(call, null);
			}
			catch (Exception ex) {
				return await HandleExceptionAsync(call, ex, cancellationToken).ConfigureAwait(false);
			}
			finally {
				request.Dispose();
				timeoutTokenSource?.Dispose();

				call.EndedUtc = DateTime.UtcNow;
				await RaiseEventAsync(Settings.AfterCall, Settings.AfterCallAsync, call).ConfigureAwait(false);
			}
		}

		private void SyncHeadersAndCookies(HttpRequestMessage request) {
			// copy any client-level (default) to FlurlRequest
			Headers.Merge(Client.Headers);
			//Cookies.Merge(Client.Cookies);

			if (Cookies.Any())
				Headers["Cookie"] = CookieCutter.ToRequestHeader(Cookies);

			// copy headers from FlurlRequest to HttpRequestMessage
			foreach (var header in Headers)
				request.SetHeader(header.Key, header.Value);

			// copy headers from HttpContent to FlurlRequest
			if (request.Content != null) {
				foreach (var header in request.Content.Headers)
					Headers[header.Key] = string.Join(",", header.Value);
			}
		}

		// partially lifted from https://github.com/dotnet/runtime/blob/master/src/libraries/System.Net.Http/src/System/Net/Http/SocketsHttpHandler/RedirectHandler.cs
		private FlurlRedirect GetRedirect(FlurlCall call) {
			if (call.Response.StatusCode < 300 || call.Response.StatusCode > 399)
				return null;

			if (!call.Response.Headers.TryGetValue("Location", out var location))
				return null;

			var redir = new FlurlRedirect();

			if (Url.IsValid(location))
				redir.Url = new Url(location);
			else if (location.StartsWith("/"))
				redir.Url = new Url(this.Url.Root).AppendPathSegment(location);
			else
				redir.Url = new Url(this.Url.Root).AppendPathSegments(this.Url.Path, location);

			// Per https://tools.ietf.org/html/rfc7231#section-7.1.2, a redirect location without a
			// fragment should inherit the fragment from the original URI.
			redir.Url.Fragment = this.Url.Fragment;

			redir.Count = 1 + (call.RedirectedFrom?.Redirect?.Count ?? 0);

			var isSecureToInsecure = (this.Url.IsSecureScheme && !redir.Url.IsSecureScheme);

			redir.Follow =
				new[] { 301, 302, 303, 307, 308 }.Contains(call.Response.StatusCode) &&
				redir.Count <= Settings.Redirects.MaxAutoRedirects &&
				(Settings.Redirects.AllowSecureToInsecure || !isSecureToInsecure);

			bool ChangeVerbToGetOn(int statusCode, HttpMethod verb) {
				switch (statusCode) {
					// 301 and 302 are a bit ambiguous. The spec says to preserve the verb
					// but most browsers rewrite it to GET. HttpClient stack changes it if
					// only it's a POST, presumably since that's a browser-friendly verb.
					// Seems odd, but sticking with that is probably the safest bet.
					// https://github.com/dotnet/runtime/blob/master/src/libraries/System.Net.Http/src/System/Net/Http/SocketsHttpHandler/RedirectHandler.cs#L140
					case 301:
					case 302:
						return verb == HttpMethod.Post;
					case 303:
						return true;
					default: // 307 & 308 mainly
						return false;
				}
			}

			redir.ChangeVerbToGet =
				redir.Follow &&
				ChangeVerbToGetOn(call.Response.StatusCode, call.Request.Verb);

			return redir;
		}

		private void CheckForCircularRedirects(FlurlCall call, HashSet<string> visited = null) {
			if (call == null) return;
			visited = visited ?? new HashSet<string>();
			if (visited.Contains(call.Request.Url))
				throw new FlurlHttpException(call, "Circular redirects detected.", null);
			visited.Add(call.Request.Url);
			CheckForCircularRedirects(call.RedirectedFrom, visited);
		}

		internal static async Task<IFlurlResponse> HandleExceptionAsync(FlurlCall call, Exception ex, CancellationToken token) {
			call.Exception = ex;
			await RaiseEventAsync(call.Request.Settings.OnError, call.Request.Settings.OnErrorAsync, call).ConfigureAwait(false);

			if (call.ExceptionHandled)
				return call.Response;

			if (ex is OperationCanceledException && !token.IsCancellationRequested)
				throw new FlurlHttpTimeoutException(call, ex);

			if (ex is FlurlHttpException)
				throw ex;

			throw new FlurlHttpException(call, ex);
		}

		private static Task RaiseEventAsync(Action<FlurlCall> syncHandler, Func<FlurlCall, Task> asyncHandler, FlurlCall call) {
			syncHandler?.Invoke(call);
			if (asyncHandler != null)
				return asyncHandler(call);
			return Task.FromResult(0);
		}
	}
}