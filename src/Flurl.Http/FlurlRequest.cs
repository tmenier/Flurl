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
		public IDictionary<string, Cookie> Cookies => Client.Cookies;

		/// <inheritdoc />
		public async Task<IFlurlResponse> SendAsync(HttpMethod verb, HttpContent content = null, CancellationToken cancellationToken = default(CancellationToken), HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead) {
			_client = Client; // "freeze" the client at this point to avoid excessive calls to FlurlClientFactory.Get (#374)

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
					WriteRequestCookies(request);

				call.HttpResponseMessage = await Client.HttpClient.SendAsync(request, completionOption, cancellationTokenWithTimeout).ConfigureAwait(false);
				call.HttpResponseMessage.RequestMessage = request;
				call.Response = new FlurlResponse(call.HttpResponseMessage);

				if (call.Succeeded)
					return call.Response;

				throw new FlurlHttpException(call, null);
			}
			catch (Exception ex) {
				return await HandleExceptionAsync(call, ex, cancellationToken);
			}
			finally {
				request.Dispose();
				timeoutTokenSource?.Dispose();

				if (Settings.CookiesEnabled)
					ReadResponseCookies(call);

				call.EndedUtc = DateTime.UtcNow;
				await HandleEventAsync(Settings.AfterCall, Settings.AfterCallAsync, call).ConfigureAwait(false);
			}
		}

		private void WriteRequestCookies(HttpRequestMessage request) {
			if (!Cookies.Any()) return;
			var uri = request.RequestUri;
			var jar = GetCookieJar(Client.HttpMessageHandler);

			if (jar != null) {
				Cookies.Merge(Client.Cookies);
				foreach (var cookie in Cookies.Values)
					try {
						jar.Add(uri, cookie);
					}
					catch (CookieException) {
						// Ignore invalid cookie and continue processing as per corefx https://github.com/dotnet/corefx/blob/v3.1.1/src/System.Net.Http/src/System/Net/Http/SocketsHttpHandler/CookieHelper.cs
					}
			}
			else {
				// no CookieContainer in play, add cookie headers manually
				// http://stackoverflow.com/a/15588878/62600
				request.Headers.TryAddWithoutValidation("Cookie", string.Join("; ", Cookies.Values));
			}
		}

		private void ReadResponseCookies(FlurlCall call) {
			var response = call.HttpResponseMessage;
			var uri = response?.RequestMessage?.RequestUri;
			if (uri == null)
				return;

			// if there's a CookieContainer in play, cookies have probably been removed from the headers and put there.
			var jar = GetCookieJar(Client.HttpMessageHandler) ?? new CookieContainer();

			// but check the headers either way. they won't be in both places, so this should be safe.
			// http://stackoverflow.com/a/15588878/62600
			if (response.Headers.TryGetValues("Set-Cookie", out var cookieHeaders)) {
				foreach (string header in cookieHeaders)
					try {
						jar.SetCookies(uri, header);
					}
					catch (CookieException) {
						// Ignore invalid cookie and continue processing as per corefx https://github.com/dotnet/corefx/blob/v3.1.1/src/System.Net.Http/src/System/Net/Http/SocketsHttpHandler/CookieHelper.cs
					}
			}

			foreach (var cookie in jar.GetCookies(uri).Cast<Cookie>()) {
				Cookies[cookie.Name] = cookie;

				if (call.Response?.Cookies != null)
					call.Response.Cookies[cookie.Name] = cookie;
			}
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