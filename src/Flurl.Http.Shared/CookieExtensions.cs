using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Flurl.Util;

namespace Flurl.Http
{
	/// <summary>
	/// Cookie extension for Flurl Client.
	/// </summary>
	public static class CookieExtensions
	{



		/// <summary>
		/// Allows cookies to be sent and received in calls made with this client. Not necessary to call when setting cookies via WithCookie/WithCookies.
		/// </summary>
		public static IFlurlClient EnableCookies(this IFlurlClient client) {
			client.Settings.CookiesEnabled = true;
			return client;
		}

		/// <summary>
		/// Allows cookies to be sent and received in calls made to this Url. Not necessary to call when setting cookies via WithCookie/WithCookies.
		/// </summary>
		public static IFlurlClient EnableCookies(this Url url) {
			return new FlurlClient(url).EnableCookies();
		}

		/// <summary>
		/// Allows cookies to be sent and received in calls made to this Url. Not necessary to call when setting cookies via WithCookie/WithCookies.
		/// </summary>
		public static IFlurlClient EnableCookies(this string url) {
			return new FlurlClient(url).EnableCookies();
		}

		/// <summary>
		/// Sets an HTTP cookie to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="client">The client.</param>
		/// <param name="cookie">The cookie to set.</param>
		/// <returns>The modified FlurlClient.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="cookie" /> is null.</exception>
		public static IFlurlClient WithCookie(this IFlurlClient client, Cookie cookie) {
			client.Settings.CookiesEnabled = true;
			client.Cookies[cookie.Name] = cookie;
			return client;
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets an HTTP cookie to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="cookie">the cookie to set.</param>
		/// <returns>The modified FlurlClient.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="cookie" /> is null.</exception>
		public static IFlurlClient WithCookie(this string url, Cookie cookie) {
			return new FlurlClient(url, true).WithCookie(cookie);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sets an HTTP cookie to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="cookie">the cookie to set.</param>
		/// <returns>The modified FlurlClient.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="cookie" /> is null.</exception>
		public static IFlurlClient WithCookie(this Url url, Cookie cookie) {
			return new FlurlClient(url, true).WithCookie(cookie);
		}

		/// <summary>
		/// Sets an HTTP cookie to be sent with all requests made with this FlurlClient.
		/// </summary>
		/// <param name="client">The client.</param>
		/// <param name="name">cookie name.</param>
		/// <param name="value">cookie value.</param>
		/// <param name="expires">cookie expiration (optional). If excluded, cookie only lives for duration of session.</param>
		/// <returns>The modified FlurlClient.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="value" /> is null.</exception>
		public static IFlurlClient WithCookie(this IFlurlClient client, string name, object value, DateTime? expires = null) {
			var cookie = new Cookie(name, value?.ToInvariantString()) { Expires = expires ?? DateTime.MinValue };
			return client.WithCookie(cookie);
		}

	    /// <summary>
	    /// Creates a FlurlClient from the URL and sets an HTTP cookie to be sent with all requests made with this FlurlClient.
	    /// </summary>
	    /// <param name="url">The URL.</param>
	    /// <param name="name">cookie name.</param>
	    /// <param name="value">cookie value.</param>
	    /// <param name="expires">cookie expiration (optional). If excluded, cookie only lives for duration of session.</param>
	    /// <returns>The modified FlurlClient.</returns>
	    /// <exception cref="ArgumentNullException"><paramref name="value" /> is null.</exception>
	    public static IFlurlClient WithCookie(this string url, string name, object value, DateTime? expires = null) {
			return new FlurlClient(url, true).WithCookie(name, value, expires);
		}

	    /// <summary>
	    /// Creates a FlurlClient from the URL and sets an HTTP cookie to be sent with all requests made with this FlurlClient.
	    /// </summary>
	    /// <param name="url">The URL.</param>
	    /// <param name="name">cookie name.</param>
	    /// <param name="value">cookie value.</param>
	    /// <param name="expires">cookie expiration (optional). If excluded, cookie only lives for duration of session.</param>
	    /// <returns>The modified FlurlClient.</returns>
	    /// <exception cref="ArgumentNullException"><paramref name="value" /> is null.</exception>
	    public static IFlurlClient WithCookie(this Url url, string name, object value, DateTime? expires = null) {
			return new FlurlClient(url, true).WithCookie(name, value, expires);
		}

	    /// <summary>
	    /// Sets HTTP cookies based on property names/values of the provided object, or keys/values if object is a dictionary, to be sent with all requests made with this FlurlClient.
	    /// </summary>
	    /// <param name="client">The client.</param>
	    /// <param name="cookies">Names/values of HTTP cookies to set. Typically an anonymous object or IDictionary.</param>
	    /// <param name="expires">Expiration for all cookies (optional). If excluded, cookies only live for duration of session.</param>
	    /// <returns>The modified FlurlClient.</returns>
	    /// <exception cref="ArgumentNullException"><paramref name="cookies" /> is null.</exception>
	    public static IFlurlClient WithCookies(this IFlurlClient client, object cookies, DateTime? expires = null) {
			if (cookies == null)
				return client;

			foreach (var kv in cookies.ToKeyValuePairs())
				client.WithCookie(kv.Key, kv.Value, expires);

			return client;
		}

	    /// <summary>
	    /// Creates a FlurlClient from the URL and sets HTTP cookies based on property names/values of the provided object, or keys/values if object is a dictionary, to be sent with all requests made with this FlurlClient.
	    /// </summary>
	    /// <param name="url">The URL.</param>
	    /// <param name="cookies">Names/values of HTTP cookies to set. Typically an anonymous object or IDictionary.</param>
	    /// <param name="expires">Expiration for all cookies (optional). If excluded, cookies only live for duration of session.</param>
	    /// <returns>The modified FlurlClient.</returns>
	    /// <exception cref="ArgumentNullException"><paramref name="cookies" /> is null.</exception>
	    public static IFlurlClient WithCookies(this Url url, object cookies, DateTime? expires = null) {
			return new FlurlClient(url, true).WithCookies(cookies);
		}

	    /// <summary>
	    /// Creates a FlurlClient from the URL and sets HTTP cookies based on property names/values of the provided object, or keys/values if object is a dictionary, to be sent with all requests made with this FlurlClient.
	    /// </summary>
	    /// <param name="url">The URL.</param>
	    /// <param name="cookies">Names/values of HTTP cookies to set. Typically an anonymous object or IDictionary.</param>
	    /// <param name="expires">Expiration for all cookies (optional). If excluded, cookies only live for duration of session.</param>
	    /// <returns>The modified FlurlClient.</returns>
	    /// <exception cref="ArgumentNullException"><paramref name="cookies" /> is null.</exception>
	    public static IFlurlClient WithCookies(this string url, object cookies, DateTime? expires = null) {
			return new FlurlClient(url, true).WithCookies(cookies);
		}
	}
}