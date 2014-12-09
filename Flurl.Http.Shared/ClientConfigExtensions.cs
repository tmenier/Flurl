using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Flurl.Util;

namespace Flurl.Http
{
	public static class ClientConfigExtensions
	{
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
		public static FlurlClient ConfigureHttpClient(this string url, Action<HttpClient> action) {
			return new FlurlClient(url).ConfigureHttpClient(action);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and provides access to modifying the underlying HttpClient.
		/// </summary>
		/// <param name="action">Action to perform on the HttpClient.</param>
		/// <returns>The FlurlClient with the modified HttpClient</returns>
		public static FlurlClient ConfigureHttpClient(this Url url, Action<HttpClient> action) {
			return new FlurlClient(url).ConfigureHttpClient(action);
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
		public static FlurlClient WithTimeout(this string url, TimeSpan timespan) {
			return new FlurlClient(url).WithTimeout(timespan);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets the client timout to the specified timespan.
		/// </summary>
		/// <param name="timespan">Time to wait before the request times out.</param>
		/// <returns>The created FlurlClient.</returns>
		public static FlurlClient WithTimeout(this Url url, TimeSpan timespan) {
			return new FlurlClient(url).WithTimeout(timespan);
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
		public static FlurlClient WithTimeout(this string url, int seconds) {
			return new FlurlClient(url).WithTimeout(seconds);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets the client timout to the specified number of seconds.
		/// </summary>
		/// <param name="seconds">Number of seconds to wait before the request times out.</param>
		/// <returns>The created FlurlClient.</returns>
		public static FlurlClient WithTimeout(this Url url, int seconds) {
			return new FlurlClient(url).WithTimeout(seconds);
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
		public static FlurlClient WithHeader(this string url, string name, object value) {
			return new FlurlClient(url).WithHeader(name, value);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets an HTTP header to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="name">HTTP header name.</param>
		/// <param name="value">HTTP header value.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithHeader(this Url url, string name, object value) {
			return new FlurlClient(url).WithHeader(name, value);
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
			return new FlurlClient(url).WithHeaders(headers);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets HTTP headers based on property names/values of the provided object, or keys/values if object is a dictionary, to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="headers">Names/values of HTTP headers to set. Typically an anonymous object or IDictionary.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithHeaders(this string url, object headers) {
			return new FlurlClient(url).WithHeaders(headers);
		}

		/// <summary>
		/// Sets an HTTP cookie to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="cookie">the cookie to set.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithCookie(this FlurlClient client, Cookie cookie) {
			var handler = client.HttpMessageHandler as HttpClientHandler;
			if (handler != null) {
				if (handler.CookieContainer == null)
					handler.CookieContainer = new CookieContainer();
				handler.CookieContainer.Add(new Uri(client.Url), cookie);
			}
			return client;
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets an HTTP cookie to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="cookie">the cookie to set.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithCookie(this string url, Cookie cookie) {
			return new FlurlClient(url).WithCookie(cookie);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets an HTTP cookie to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="cookie">the cookie to set.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithCookie(this Url url, Cookie cookie) {
			return new FlurlClient(url).WithCookie(cookie);
		}


		/// <summary>
		/// Sets an HTTP cookie to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="name">cookie name.</param>
		/// <param name="value">cookie value.</param>
		/// <param name="expires">cookie expiration (optional). If excluded, cookie only lives for duration of session.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithCookie(this FlurlClient client, string name, object value, DateTime? expires = null) {
			return client.WithCookie(new Cookie(name, (value == null) ? null : value.ToString()) { Expires = expires ?? DateTime.MinValue });
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets an HTTP cookie to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="name">cookie name.</param>
		/// <param name="value">cookie value.</param>
		/// <param name="expires">cookie expiration (optional). If excluded, cookie only lives for duration of session.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithCookie(this string url, string name, object value, DateTime? expires = null) {
			return new FlurlClient(url).WithCookie(name, value, expires);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets an HTTP cookie to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="name">cookie name.</param>
		/// <param name="value">cookie value.</param>
		/// <param name="expires">cookie expiration (optional). If excluded, cookie only lives for duration of session.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithCookie(this Url url, string name, object value, DateTime? expires = null) {
			return new FlurlClient(url).WithCookie(name, value, expires);
		}

		/// <summary>
		/// Sets HTTP cookies based on property names/values of the provided object, or keys/values if object is a dictionary, to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="cookies">Names/values of HTTP cookies to set. Typically an anonymous object or IDictionary.</param>
		/// <param name="expires">Expiration for all cookies (optional). If excluded, cookies only live for duration of session.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithCookies(this FlurlClient client, object cookies, DateTime? expires = null) {
			if (cookies == null)
				return client;

			foreach (var kv in cookies.ToKeyValuePairs())
				client.WithCookie(kv.Key, kv.Value, expires);

			return client;
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets HTTP cookies based on property names/values of the provided object, or keys/values if object is a dictionary, to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="cookies">Names/values of HTTP cookies to set. Typically an anonymous object or IDictionary.</param>
		/// <param name="expires">Expiration for all cookies (optional). If excluded, cookies only live for duration of session.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithCookies(this Url url, object cookies, DateTime? expires = null) {
			return new FlurlClient(url).WithCookies(cookies);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets HTTP cookies based on property names/values of the provided object, or keys/values if object is a dictionary, to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="cookies">Names/values of HTTP cookies to set. Typically an anonymous object or IDictionary.</param>
		/// <param name="expires">Expiration for all cookies (optional). If excluded, cookies only live for duration of session.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithCookies(this string url, object cookies, DateTime? expires = null) {
			return new FlurlClient(url).WithCookies(cookies);
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
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithBasicAuth(this Url url, string username, string password) {
			return new FlurlClient(url).WithBasicAuth(username, password);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets HTTP authorization header according to Basic Authentication protocol to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="username">Username of authenticating user.</param>
		/// <param name="password">Password of authenticating user.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithBasicAuth(this string url, string username, string password) {
			return new FlurlClient(url).WithBasicAuth(username, password);
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
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithOAuthBearerToken(this Url url, string token) {
			return new FlurlClient(url).WithOAuthBearerToken(token);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets HTTP authorization header with acquired bearer token according to OAuth 2.0 specification to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="token">The acquired bearer token to pass.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithOAuthBearerToken(this string url, string token) {
			return new FlurlClient(url).WithOAuthBearerToken(token);
		}
	}
}
