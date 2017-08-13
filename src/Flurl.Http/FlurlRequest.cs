using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http.Configuration;
using Flurl.Http.Testing;
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
		private IFlurlClient _client;

		/// <summary>
		/// Initializes a new instance of the <see cref="FlurlRequest"/> class.
		/// </summary>
		/// <param name="url">The URL to call with this FlurlRequest instance.</param>
		/// <param name="settings">The FlurlHttpSettings object used by this request.</param>
		public FlurlRequest(Url url, FlurlHttpSettings settings = null) {
			Settings = settings ?? new FlurlHttpSettings().Merge(HttpTest.Current?.Settings ?? FlurlHttp.GlobalSettings);
			Url = url;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FlurlRequest"/> class.
		/// </summary>
		/// <param name="url">The URL to call with this FlurlRequest instance.</param>
		/// <param name="settings">The FlurlHttpSettings object used by this request.</param>
		public FlurlRequest(string url, FlurlHttpSettings settings = null) : this(new Url(url), settings) { }

		/// <summary>
		/// Gets or sets the FlurlHttpSettings used by this request.
		/// </summary>
		public FlurlHttpSettings Settings { get; set; }

		/// <summary>
		/// Gets or sets the IFlurlClient to use when sending the request.
		/// </summary>
		public IFlurlClient Client {
			get => _client = _client ?? Settings.FlurlClientFactory.GetClient(Url);
			set {
				_client = value;
				Settings.Merge(_client.Settings);
			}
		}

		/// <summary>
		/// Gets or sets the URL to be called.
		/// </summary>
		public Url Url { get; set; }

		/// <summary>
		/// Collection of headers sent on this request.
		/// </summary>
		public IDictionary<string, object> Headers { get; } = new Dictionary<string, object>();

		/// <summary>
		/// Collection of HttpCookies sent and received by the IFlurlClient associated with this request.
		/// </summary>
		public IDictionary<string, Cookie> Cookies => Client.Cookies;

		/// <summary>
		/// Creates and asynchronously sends an HttpRequestMessage.
		/// Mainly used to implement higher-level extension methods (GetJsonAsync, etc).
		/// </summary>
		/// <param name="verb">The HTTP method used to make the request.</param>
		/// <param name="content">Contents of the request body.</param>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation. Optional.</param>
		/// <param name="completionOption">The HttpCompletionOption used in the request. Optional.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public async Task<HttpResponseMessage> SendAsync(HttpMethod verb, HttpContent content = null, CancellationToken? cancellationToken = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead) {
			Settings.Merge(Client.Settings);
			if (Settings.Timeout.HasValue)
				Client.HttpClient.Timeout = Settings.Timeout.Value;
			var request = new HttpRequestMessage(verb, Url) { Content = content };
			var call = new HttpCall(request, Settings);

			try {
				WriteHeaders(request);
				if (Settings.CookiesEnabled)
					WriteRequestCookies(request);
				return await Client.HttpClient.SendAsync(request, completionOption, cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
			}
			catch (Exception) when (call.ExceptionHandled) {
				return call.Response;
			}
			finally {
				request.Dispose();
				if (Settings.CookiesEnabled)
					ReadResponseCookies(call.Response);
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
			var cookieHandler = Client.HttpMessageHandler as HttpClientHandler;

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
			if (response?.RequestMessage == null) return;

			var uri = response.RequestMessage.RequestUri;

			// if the handler is an HttpClientHandler (which it usually is), it's already plucked the
			// cookies out of the headers and put them in the CookieContainer.
			var jar = (Client.HttpMessageHandler as HttpClientHandler)?.CookieContainer;
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
	}
}