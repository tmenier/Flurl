using System;
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
		/// Gets or sets the HTTP method of the request. Normally you don't need to set this explicitly; it will be set
		/// when you call the sending method, such as GetAsync, PostAsync, etc.
		/// </summary>
		HttpMethod Verb { get; set; }

		/// <summary>
		/// Gets or sets the URL to be called.
		/// </summary>
		Url Url { get; set; }

		/// <summary>
		/// Gets Name/Value pairs parsed from the Cookie request header.
		/// </summary>
		IEnumerable<(string Name, string Value)> Cookies { get; }

		/// <summary>
		/// Gets or sets the collection of HTTP cookies that can be shared between multiple requests. When set, values that
		/// should be sent with this request (based on Domain, Path, and other rules) are immediately copied to the Cookie
		/// request header, and any Set-Cookie headers received in the response will be written to the CookieJar.
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
		private FlurlCall _redirectedFrom;
		private CookieJar _jar;

		/// <summary>
		/// Initializes a new instance of the <see cref="FlurlRequest"/> class.
		/// </summary>
		/// <param name="url">The URL to call with this FlurlRequest instance.</param>
		public FlurlRequest(Url url = null) {
			_url = url;
		}

		/// <summary>
		/// Used internally by FlurlClient.Request and CookieSession.Request
		/// </summary>
		internal FlurlRequest(string baseUrl, object[] urlSegments) {
			var parts = new List<string>(urlSegments.Select(s => s.ToInvariantString()));
			if (!Url.IsValid(parts.FirstOrDefault()) && !string.IsNullOrEmpty(baseUrl))
				parts.Insert(0, baseUrl);

			if (!parts.Any())
				throw new ArgumentException("Cannot create a Request. BaseUrl is not defined and no segments were passed.");
			if (!Url.IsValid(parts[0]))
				throw new ArgumentException("Cannot create a Request. Neither BaseUrl nor the first segment passed is a valid URL.");

			_url = Url.Combine(parts.ToArray());
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
		public INameValueList<string> Headers { get; } = new NameValueList<string>();

		/// <inheritdoc />
		public IEnumerable<(string Name, string Value)> Cookies =>
			CookieCutter.ParseRequestHeader(Headers.FirstOrDefault("Cookie"));

		/// <inheritdoc />
		public CookieJar CookieJar {
			get => _jar;
			set {
				_jar = value;
				this.WithCookies(
					from c in CookieJar
					where c.ShouldSendTo(this.Url, out _)
					// sort by longest path, then earliest creation time, per #2: https://tools.ietf.org/html/rfc6265#section-5.4
					orderby (c.Path ?? c.OriginUrl.Path).Length descending, c.DateReceived
					select (c.Name, c.Value));
			}
		}

		/// <inheritdoc />
		public async Task<IFlurlResponse> SendAsync(HttpMethod verb, HttpContent content = null, CancellationToken cancellationToken = default, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead) {
			_client = Client; // "freeze" the client at this point to avoid excessive calls to FlurlClientFactory.Get (#374)
			Verb = verb;

			var request = new HttpRequestMessage(verb, Url) { Content = content };
			SyncHeaders(request);
			var call = new FlurlCall {
				Request = this,
				RedirectedFrom = _redirectedFrom,
				HttpRequestMessage = request
			};
			request.SetFlurlCall(call);

			await RaiseEventAsync(Settings.BeforeCall, Settings.BeforeCallAsync, call).ConfigureAwait(false);

			// in case URL or headers were modified in the handler above
			request.RequestUri = Url.ToUri();
			SyncHeaders(request);

			call.StartedUtc = DateTime.UtcNow;
			var ct = GetCancellationTokenWithTimeout(cancellationToken, out var cts);

			try {

				var response = await Client.HttpClient.SendAsync(request, completionOption, ct).ConfigureAwait(false);
				call.HttpResponseMessage = response;
				call.HttpResponseMessage.RequestMessage = request;
				call.Response = new FlurlResponse(call.HttpResponseMessage, CookieJar);

				if (call.Succeeded) {
					var redirResponse = await ProcessRedirectAsync(call, cancellationToken, completionOption).ConfigureAwait(false);
					return redirResponse ?? call.Response;
				}
				else
					throw new FlurlHttpException(call, null);
			}
			catch (Exception ex) {
				return await HandleExceptionAsync(call, ex, cancellationToken).ConfigureAwait(false);
			}
			finally {
				request.Dispose();
				cts?.Dispose();
				call.EndedUtc = DateTime.UtcNow;
				await RaiseEventAsync(Settings.AfterCall, Settings.AfterCallAsync, call).ConfigureAwait(false);
			}
		}

		private void SyncHeaders(HttpRequestMessage request) {
			// copy any client-level (default) headers to this request
			foreach (var header in Client.Headers.Where(h => !this.Headers.Contains(h.Name)))
				this.Headers.Add(header.Name, header.Value);

			// copy headers from FlurlRequest to HttpRequestMessage
			foreach (var header in Headers)
				request.SetHeader(header.Name, header.Value);

			// copy headers from HttpContent to FlurlRequest
			if (request.Content != null) {
				foreach (var header in request.Content.Headers)
					Headers.AddOrReplace(header.Key, string.Join(",", header.Value));
			}
		}

		private CancellationToken GetCancellationTokenWithTimeout(CancellationToken original, out CancellationTokenSource timeoutTokenSource) {
			timeoutTokenSource = null;
			if (Settings.Timeout.HasValue) {
				timeoutTokenSource = CancellationTokenSource.CreateLinkedTokenSource(original);
				timeoutTokenSource.CancelAfter(Settings.Timeout.Value);
				return timeoutTokenSource.Token;
			}
			else {
				return original;
			}
		}

		private async Task<IFlurlResponse> ProcessRedirectAsync(FlurlCall call, CancellationToken cancellationToken, HttpCompletionOption completionOption) {
			if (Settings.Redirects.Enabled)
				call.Redirect = GetRedirect(call);

			if (call.Redirect == null)
				return null;

			await RaiseEventAsync(Settings.OnRedirect, Settings.OnRedirectAsync, call).ConfigureAwait(false);

			if (call.Redirect.Follow != true)
				return null;

			CheckForCircularRedirects(call);

			var redir = new FlurlRequest(call.Redirect.Url);
			redir.Client = Client;
			redir._redirectedFrom = call;
			redir.Settings.Defaults = Settings;
			redir.WithHeaders(this.Headers).WithCookies(call.Response.Cookies);

			var changeToGet = call.Redirect.ChangeVerbToGet;

			if (!Settings.Redirects.ForwardAuthorizationHeader)
				redir.Headers.Remove("Authorization");
			if (changeToGet)
				redir.Headers.Remove("Transfer-Encoding");

			var ct = GetCancellationTokenWithTimeout(cancellationToken, out var cts);
			try {
				return await redir.SendAsync(
					changeToGet ? HttpMethod.Get : call.HttpRequestMessage.Method,
					changeToGet ? null : call.HttpRequestMessage.Content,
					cancellationToken,
					completionOption).ConfigureAwait(false);
			}
			finally {
				cts?.Dispose();
			}
		}

		// partially lifted from https://github.com/dotnet/runtime/blob/master/src/libraries/System.Net.Http/src/System/Net/Http/SocketsHttpHandler/RedirectHandler.cs
		private FlurlRedirect GetRedirect(FlurlCall call) {
			if (call.Response.StatusCode < 300 || call.Response.StatusCode > 399)
				return null;

			if (!call.Response.Headers.TryGetFirst("Location", out var location))
				return null;

			var redir = new FlurlRedirect();

			if (Url.IsValid(location))
				redir.Url = new Url(location);
			else if (location.OrdinalStartsWith("/"))
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