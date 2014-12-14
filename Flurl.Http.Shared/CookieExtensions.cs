using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using Flurl.Util;

namespace Flurl.Http
{
    public static class CookieExtensions
    {
		/// <summary>
		/// Gets a collection of cookies that will be sent in calls using this client. (Use FlurlClient.WithCookie/WithCookies to set cookies.)
		/// </summary>
		public static Dictionary<string, Cookie> GetCookies(this FlurlClient client) {
			var jar = GetCookieContainer(client);
			if (jar == null)
				return null;

			var uri = new Uri(Url.GetRoot(client.Url));
			return jar.GetCookies(uri).Cast<Cookie>().ToDictionary(c => c.Name, c => c);
		}

		/// <summary>
		/// Allows cookies to be sent and received in calls made with this client. Not necessary to call when setting cookies via WithCookie/WithCookies.
		/// </summary>
		public static FlurlClient EnableCookies(this FlurlClient client) {
			GetCookieContainer(client); // ensures the container has been created
			return client;
		}

		/// <summary>
		/// Allows cookies to be sent and received in calls made to this Url. Not necessary to call when setting cookies via WithCookie/WithCookies.
		/// </summary>
		public static FlurlClient EnableCookies(this Url url) {
			return new FlurlClient(url).EnableCookies();
		}

		/// <summary>
		/// Allows cookies to be sent and received in calls made to this Url. Not necessary to call when setting cookies via WithCookie/WithCookies.
		/// </summary>
		public static FlurlClient EnableCookies(this string url) {
			return new FlurlClient(url).EnableCookies();
		}

		/// <summary>
		/// Sets an HTTP cookie to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="cookie">the cookie to set.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithCookie(this FlurlClient client, Cookie cookie) {
			var uri = new Uri(Url.GetRoot(client.Url));
			GetCookieContainer(client).Add(uri, cookie);
			return client;
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets an HTTP cookie to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="cookie">the cookie to set.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithCookie(this string url, Cookie cookie) {
			return new FlurlClient(url, true).WithCookie(cookie);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets an HTTP cookie to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="cookie">the cookie to set.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithCookie(this Url url, Cookie cookie) {
			return new FlurlClient(url, true).WithCookie(cookie);
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
			return new FlurlClient(url, true).WithCookie(name, value, expires);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets an HTTP cookie to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="name">cookie name.</param>
		/// <param name="value">cookie value.</param>
		/// <param name="expires">cookie expiration (optional). If excluded, cookie only lives for duration of session.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithCookie(this Url url, string name, object value, DateTime? expires = null) {
			return new FlurlClient(url, true).WithCookie(name, value, expires);
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
			return new FlurlClient(url, true).WithCookies(cookies);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets HTTP cookies based on property names/values of the provided object, or keys/values if object is a dictionary, to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="cookies">Names/values of HTTP cookies to set. Typically an anonymous object or IDictionary.</param>
		/// <param name="expires">Expiration for all cookies (optional). If excluded, cookies only live for duration of session.</param>
		/// <returns>The modified FlurlClient.</returns>
		public static FlurlClient WithCookies(this string url, object cookies, DateTime? expires = null) {
			return new FlurlClient(url, true).WithCookies(cookies);
		}

		private static CookieContainer GetCookieContainer(FlurlClient client) {
			var handler = client.HttpMessageHandler as HttpClientHandler;
			if (handler == null)
				return null;

			return handler.CookieContainer ?? (handler.CookieContainer = new CookieContainer());
		}
	}
}
