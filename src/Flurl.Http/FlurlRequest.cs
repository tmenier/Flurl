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
		/// The body content of this request.
		/// </summary>
		HttpContent Content { get; set; }

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
		/// The FlurlCall that received a 3xx response and automatically triggered this request.
		/// </summary>
		FlurlCall RedirectedFrom { get; set; }

		/// <summary>
		/// Asynchronously sends the HTTP request. Mainly used to implement higher-level extension methods (GetJsonAsync, etc).
		/// </summary>
		/// <param name="verb">The HTTP method used to make the request.</param>
		/// <param name="content">Contents of the request body.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
		/// <param name="completionOption">The HttpCompletionOption used in the request. Optional.</param>
		/// <returns>A Task whose result is the received IFlurlResponse.</returns>
		Task<IFlurlResponse> SendAsync(HttpMethod verb, HttpContent content = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead, CancellationToken cancellationToken = default);
	}

	/// <inheritdoc />
	public class FlurlRequest : IFlurlRequest
	{
		private IFlurlClient _client;
		private CookieJar _jar;

		/// <summary>
		/// Initializes a new instance of the <see cref="FlurlRequest"/> class.
		/// </summary>
		/// <param name="url">The URL to call with this FlurlRequest instance.</param>
		public FlurlRequest(Url url = null) {
			Url = url;
		}

		/// <summary>
		/// Used internally by FlurlClient.Request
		/// </summary>
		internal FlurlRequest(IFlurlClient client, params object[] urlSegments) : this(client?.BaseUrl, urlSegments) {
			Client = client;
		}

		/// <summary>
		/// Used internally by FlurlClient.Request and CookieSession.Request
		/// </summary>
		internal FlurlRequest(string baseUrl, params object[] urlSegments) {
			var parts = new List<string>(urlSegments.Select(s => s.ToInvariantString()));
			if (!Url.IsValid(parts.FirstOrDefault()) && !string.IsNullOrEmpty(baseUrl))
				parts.Insert(0, baseUrl);

			if (parts.Any())
				Url = Url.Combine(parts.ToArray());
		}

		/// <inheritdoc />
		public FlurlHttpSettings Settings { get; } = new FlurlHttpSettings();

		/// <inheritdoc />
		public IFlurlClient Client {
			get => _client;
			set {
				_client = value;
				Settings.Defaults = _client?.Settings;
			}
		}

		/// <inheritdoc />
		public HttpMethod Verb { get; set; }

		/// <inheritdoc />
		public Url Url { get; set; }

		/// <inheritdoc />
		public HttpContent Content { get; set; }

		/// <inheritdoc />
		public FlurlCall RedirectedFrom { get; set; }

		/// <inheritdoc />
		public INameValueList<string> Headers { get; } = new NameValueList<string>(false); // header names are case-insensitive https://stackoverflow.com/a/5259004/62600

		/// <inheritdoc />
		public IEnumerable<(string Name, string Value)> Cookies =>
			CookieCutter.ParseRequestHeader(Headers.FirstOrDefault("Cookie"));

		/// <inheritdoc />
		public CookieJar CookieJar {
			get => _jar;
			set => ApplyCookieJar(value);
		}

		/// <inheritdoc />
		public Task<IFlurlResponse> SendAsync(HttpMethod verb, HttpContent content = null, HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead, CancellationToken cancellationToken = default) {
			Verb = verb;
			Content = content;
			Client ??= FlurlHttp.GlobalSettings.FlurlClientFactory.Get(Url);
			return Client.SendAsync(this, completionOption, cancellationToken);
		}

		private void ApplyCookieJar(CookieJar jar) {
			_jar = jar;
			if (jar == null)
				return;

			this.WithCookies(
				from c in CookieJar
				where c.ShouldSendTo(this.Url, out _)
				// sort by longest path, then earliest creation time, per #2: https://tools.ietf.org/html/rfc6265#section-5.4
				orderby (c.Path ?? c.OriginUrl.Path).Length descending, c.DateReceived
				select (c.Name, c.Value));
		}
	}
}