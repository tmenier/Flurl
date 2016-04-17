using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Flurl.Http.Configuration;
using Flurl.Util;

namespace Flurl.Http
{
	public static class ClientConfigExtensions
	{
		/// <summary>
		/// Fluently specify that an existing FlurlClient should be used to call the Url, rather than creating a new one.
		/// Enables re-using the underlying HttpClient.
		/// </summary>
		/// <param name="fc">The FlurlClient to use in calling the Url</param>
		/// <returns></returns>
		public static FlurlClient WithClient(this Url url, FlurlClient fc) {
			fc.Url = url;
			return fc;
		}

		/// <summary>
		/// Fluently specify that an existing FlurlClient should be used to call the Url, rather than creating a new one.
		/// Enables re-using the underlying HttpClient.
		/// </summary>
		/// <param name="fc">The FlurlClient to use in calling the Url</param>
		/// <returns></returns>
		public static FlurlClient WithClient(this string url, FlurlClient fc) {
			return new Url(url).WithClient(fc);
		}

		/// <summary>
		/// Returns a new FlurlClient where all state (HttpClient, etc) is shared but with a different URL.
		/// Allows you to re-use the underlying HttpClient instance (such as to share cookies, etc) with
		/// different URLs in a thread-safe way.
		/// </summary>
		/// <param name="url">The Url to call.</param>
		/// <returns></returns>
		public static FlurlClient WithUrl(this FlurlClient client, Url url) {
			var fc = client.Clone();
			fc.Url = url;
			// prevent the new client from automatically disposing the parent's HttpClient
			fc.AutoDispose = false;
			return fc;
		}

		/// <summary>
		/// Change FlurlHttpSettings for this client instance.
		/// </summary>
		/// <param name="action">Action defining the settings changes.</param>
		/// <returns>The FlurlClient with the modified HttpClient</returns>
		public static FlurlClient ConfigureClient(this FlurlClient client, Action<FlurlHttpSettings> action) {
			action(client.Settings);
			return client;
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and allows changing the FlurlHttpSettings associated with the instance.
		/// </summary>
		/// <param name="action">Action defining the settings changes.</param>
		/// <returns>The FlurlClient with the modified HttpClient</returns>
		public static FlurlClient ConfigureClient(this Url url, Action<FlurlHttpSettings> action) {
			return new FlurlClient(url, true).ConfigureClient(action);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and allows changing the FlurlHttpSettings associated with the instance.
		/// </summary>
		/// <param name="action">Action defining the settings changes.</param>
		/// <returns>The FlurlClient with the modified HttpClient</returns>
		public static FlurlClient ConfigureClient(this string url, Action<FlurlHttpSettings> action) {
			return new FlurlClient(url, true).ConfigureClient(action);
		}

		/// <summary>
		/// Provides access to modifying the underlying HttpClient.
		/// </summary>
		/// <param name="action">Action to perform on the HttpClient.</param>
		/// <returns>The FlurlClient with the modified HttpClient</returns>
		public static FlurlClient ConfigureHttpClient(this FlurlClient client, Action<HttpClient> action) {
			action(client.HttpClient);
			return client;
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and provides access to modifying the underlying HttpClient.
		/// </summary>
		/// <param name="action">Action to perform on the HttpClient.</param>
		/// <returns>The FlurlClient with the modified HttpClient</returns>
		public static FlurlClient ConfigureHttpClient(this Url url, Action<HttpClient> action) {
			return new FlurlClient(url, true).ConfigureHttpClient(action);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and provides access to modifying the underlying HttpClient.
		/// </summary>
		/// <param name="action">Action to perform on the HttpClient.</param>
		/// <returns>The FlurlClient with the modified HttpClient</returns>
		public static FlurlClient ConfigureHttpClient(this string url, Action<HttpClient> action) {
			return new FlurlClient(url, true).ConfigureHttpClient(action);
		}

		/// <summary>
		/// Sets the client timout to the specified timespan.
		/// </summary>
		/// <param name="timespan">Time to wait before the request times out.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithTimeout(this FlurlClient client, TimeSpan timespan) {
			client.HttpClient.Timeout = timespan;
			return client;
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets the client timout to the specified timespan.
		/// </summary>
		/// <param name="timespan">Time to wait before the request times out.</param>
		/// <returns>The created FlurlClient.</returns>
		public static FlurlClient WithTimeout(this Url url, TimeSpan timespan) {
			return new FlurlClient(url, true).WithTimeout(timespan);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets the client timout to the specified timespan.
		/// </summary>
		/// <param name="timespan">Time to wait before the request times out.</param>
		/// <returns>The created FlurlClient.</returns>
		public static FlurlClient WithTimeout(this string url, TimeSpan timespan) {
			return new FlurlClient(url, true).WithTimeout(timespan);
		}

		/// <summary>
		/// Sets the client timout to the specified number of seconds.
		/// </summary>
		/// <param name="seconds">Number of seconds to wait before the request times out.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithTimeout(this FlurlClient client, int seconds) {
			return client.WithTimeout(TimeSpan.FromSeconds(seconds));
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets the client timout to the specified number of seconds.
		/// </summary>
		/// <param name="seconds">Number of seconds to wait before the request times out.</param>
		/// <returns>The created FlurlClient.</returns>
		public static FlurlClient WithTimeout(this Url url, int seconds) {
			return new FlurlClient(url, true).WithTimeout(seconds);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets the client timout to the specified number of seconds.
		/// </summary>
		/// <param name="seconds">Number of seconds to wait before the request times out.</param>
		/// <returns>The created FlurlClient.</returns>
		public static FlurlClient WithTimeout(this string url, int seconds) {
			return new FlurlClient(url, true).WithTimeout(seconds);
		}

		/// <summary>
		/// Sets an HTTP header to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="name">HTTP header name.</param>
		/// <param name="value">HTTP header value.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithHeader(this FlurlClient client, string name, object value) {
			var values = new[] { (value == null) ? null : value.ToString() };
			client.HttpClient.DefaultRequestHeaders.Add(name, values);
			return client;
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets an HTTP header to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="name">HTTP header name.</param>
		/// <param name="value">HTTP header value.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithHeader(this Url url, string name, object value) {
			return new FlurlClient(url, true).WithHeader(name, value);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets an HTTP header to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="name">HTTP header name.</param>
		/// <param name="value">HTTP header value.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithHeader(this string url, string name, object value) {
			return new FlurlClient(url, true).WithHeader(name, value);
		}

		/// <summary>
		/// Sets HTTP headers based on property names/values of the provided object, or keys/values if object is a dictionary, to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="headers">Names/values of HTTP headers to set. Typically an anonymous object or IDictionary.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithHeaders(this FlurlClient client, object headers) {
			if (headers == null)
				return client;

			foreach (var kv in headers.ToKeyValuePairs()) {
				if (kv.Value == null)
					continue;

				client.HttpClient.DefaultRequestHeaders.Add(kv.Key, new[] { kv.Value.ToString() });
			}

			return client;
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets HTTP headers based on property names/values of the provided object, or keys/values if object is a dictionary, to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="headers">Names/values of HTTP headers to set. Typically an anonymous object or IDictionary.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithHeaders(this Url url, object headers) {
			return new FlurlClient(url, true).WithHeaders(headers);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets HTTP headers based on property names/values of the provided object, or keys/values if object is a dictionary, to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="headers">Names/values of HTTP headers to set. Typically an anonymous object or IDictionary.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithHeaders(this string url, object headers) {
			return new FlurlClient(url, true).WithHeaders(headers);
		}

		/// <summary>
		/// Sets HTTP authorization header according to Basic Authentication protocol to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="username">Username of authenticating user.</param>
		/// <param name="password">Password of authenticating user.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithBasicAuth(this FlurlClient client, string username, string password) {
			// http://stackoverflow.com/questions/14627399/setting-authorization-header-of-httpclient
			var value = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format("{0}:{1}", username, password)));
			client.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", value);
			return client;
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets HTTP authorization header according to Basic Authentication protocol to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="username">Username of authenticating user.</param>
		/// <param name="password">Password of authenticating user.</param>
		/// <returns>The new FlurlClient.</returns>
		public static FlurlClient WithBasicAuth(this Url url, string username, string password) {
			return new FlurlClient(url, true).WithBasicAuth(username, password);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets HTTP authorization header according to Basic Authentication protocol to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="username">Username of authenticating user.</param>
		/// <param name="password">Password of authenticating user.</param>
		/// <returns>The new FlurlClient.</returns>
		public static FlurlClient WithBasicAuth(this string url, string username, string password) {
			return new FlurlClient(url, true).WithBasicAuth(username, password);
		}

		/// <summary>
		/// Sets HTTP authorization header with acquired bearer token according to OAuth 2.0 specification to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="token">The acquired bearer token to pass.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithOAuthBearerToken(this FlurlClient client, string token) {
			client.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
			return client;
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets HTTP authorization header with acquired bearer token according to OAuth 2.0 specification to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="token">The acquired bearer token to pass.</param>
		/// <returns>The new FlurlClient.</returns>
		public static FlurlClient WithOAuthBearerToken(this Url url, string token) {
			return new FlurlClient(url, true).WithOAuthBearerToken(token);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets HTTP authorization header with acquired bearer token according to OAuth 2.0 specification to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="token">The acquired bearer token to pass.</param>
		/// <returns>The new FlurlClient.</returns>
		public static FlurlClient WithOAuthBearerToken(this string url, string token) {
			return new FlurlClient(url, true).WithOAuthBearerToken(token);
		}

		/// <summary>
		/// Adds a pattern representing an HTTP status code or range of codes which (in addtion to 2xx) will NOT result in a FlurlHttpException being thrown.
		/// </summary>
		/// <param name="pattern">Examples: "3xx", "100,300,600", "100-299,6xx"</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient AllowHttpStatus(this FlurlClient client, string pattern) {
			if (!string.IsNullOrWhiteSpace(pattern)) {
				var current = client.Settings.AllowedHttpStatusRange;
				if (string.IsNullOrWhiteSpace(current))
					client.Settings.AllowedHttpStatusRange = pattern;
				else
					client.Settings.AllowedHttpStatusRange += "," + pattern;
			}
			return client;
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and adds a pattern representing an HTTP status code or range of codes which (in addtion to 2xx) will NOT result in a FlurlHttpException being thrown.
		/// </summary>
		/// <param name="pattern">Examples: "3xx", "100,300,600", "100-299,6xx"</param>
		/// <returns>The new FlurlClient.</returns>
		public static FlurlClient AllowHttpStatus(this Url url, string pattern) {
			return new FlurlClient(url, true).AllowHttpStatus(pattern);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and adds a pattern representing an HTTP status code or range of codes which (in addtion to 2xx) will NOT result in a FlurlHttpException being thrown.
		/// </summary>
		/// <param name="pattern">Examples: "3xx", "100,300,600", "100-299,6xx"</param>
		/// <returns>The new FlurlClient.</returns>
		public static FlurlClient AllowHttpStatus(this string url, string pattern) {
			return new FlurlClient(url, true).AllowHttpStatus(pattern);
		}

		/// <summary>
		/// Adds an <see cref="HttpStatusCode"/> which (in addtion to 2xx) will NOT result in a FlurlHttpException being thrown.
		/// </summary>
		/// <param name="statusCodes">Examples: HttpStatusCode.NotFound</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient AllowHttpStatus(this FlurlClient client, params HttpStatusCode[] statusCodes) {
			var pattern = string.Join(",", statusCodes.Select(c => (int)c));
			return AllowHttpStatus(client, pattern);
		}

		/// <summary>
		/// Adds an <see cref="HttpStatusCode"/> which (in addtion to 2xx) will NOT result in a FlurlHttpException being thrown.
		/// </summary>
		/// <param name="statusCodes">Examples: HttpStatusCode.NotFound</param>
		/// <returns>The new FlurlClient.</returns>
		public static FlurlClient AllowHttpStatus(this Url url, params HttpStatusCode[] statusCodes) {
			return new FlurlClient(url, true).AllowHttpStatus(statusCodes);
		}

		/// <summary>
		/// Adds an <see cref="HttpStatusCode"/> which (in addtion to 2xx) will NOT result in a FlurlHttpException being thrown.
		/// </summary>
		/// <param name="statusCodes">Examples: HttpStatusCode.NotFound</param>
		/// <returns>The new FlurlClient.</returns>
		public static FlurlClient AllowHttpStatus(this string url, params HttpStatusCode[] statusCodes) {
			return new FlurlClient(url, true).AllowHttpStatus(statusCodes);
		}

		/// <summary>
		/// Prevents a FlurlHttpException from being thrown on any completed response, regardless of the HTTP status code.
		/// </summary>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient AllowAnyHttpStatus(this FlurlClient client) {
			client.Settings.AllowedHttpStatusRange = "*";
			return client;
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and prevents a FlurlHttpException from being thrown on any completed response, regardless of the HTTP status code.
		/// </summary>
		/// <returns>The new FlurlClient.</returns>
		public static FlurlClient AllowAnyHttpStatus(this Url url) {
			return new FlurlClient(url, true).AllowAnyHttpStatus();
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and prevents a FlurlHttpException from being thrown on any completed response, regardless of the HTTP status code.
		/// </summary>
		/// <returns>The new FlurlClient.</returns>
		public static FlurlClient AllowAnyHttpStatus(this string url) {
			return new FlurlClient(url, true).AllowAnyHttpStatus();
		}
	}
}
