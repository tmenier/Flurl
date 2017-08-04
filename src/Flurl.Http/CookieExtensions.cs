using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Flurl.Util;

namespace Flurl.Http
{
	/// <summary>
	/// Fluent extension methods for working with HTTP cookies.
	/// </summary>
	public static class CookieExtensions
	{
		/// <summary>
		/// Allows cookies to be sent and received. Not necessary to call when setting cookies via WithCookie/WithCookies.
		/// </summary>
		/// <param name="client">The IFlurlClient.</param>
		/// <returns>This IFlurlClient.</returns>
		public static IFlurlClient EnableCookies(this IFlurlClient client) {
			client.Settings.CookiesEnabled = true;
			return client;
		}

		/// <summary>
		/// Sets an HTTP cookie to be sent with all requests made with this IFlurlClient.
		/// </summary>
		/// <param name="client">The IFlurlClient.</param>
		/// <param name="cookie">The cookie to set.</param>
		/// <returns>This IFlurlClient.</returns>
		public static IFlurlClient WithCookie(this IFlurlClient client, Cookie cookie) {
			client.Settings.CookiesEnabled = true;
			client.Cookies[cookie.Name] = cookie;
			return client;
		}

		/// <summary>
		/// Sets an HTTP cookie to be sent with all requests made with this IFlurlClient.
		/// </summary>
		/// <param name="client">The IFlurlClient.</param>
		/// <param name="name">The cookie name.</param>
		/// <param name="value">The cookie value.</param>
		/// <param name="expires">The cookie expiration (optional). If excluded, cookie only lives for duration of session.</param>
		/// <returns>This IFlurlClient.</returns>
		public static IFlurlClient WithCookie(this IFlurlClient client, string name, object value, DateTime? expires = null) {
			var cookie = new Cookie(name, value?.ToInvariantString()) { Expires = expires ?? DateTime.MinValue };
			return client.WithCookie(cookie);
		}

		/// <summary>
		/// Sets HTTP cookies to be sent with all requests made with this IFlurlClient, based on property names/values of the provided object, or keys/values if object is a dictionary.
		/// </summary>
		/// <param name="client">The IFlurlClient.</param>
		/// <param name="cookies">Names/values of HTTP cookies to set. Typically an anonymous object or IDictionary.</param>
		/// <param name="expires">Expiration for all cookies (optional). If excluded, cookies only live for duration of session.</param>
		/// <returns>This IFlurlClient.</returns>
		public static IFlurlClient WithCookies(this IFlurlClient client, object cookies, DateTime? expires = null) {
			if (cookies == null)
				return client;

			foreach (var kv in cookies.ToKeyValuePairs())
				client.WithCookie(kv.Key, kv.Value, expires);

			return client;
		}

		/// <summary>
		/// Allows cookies to be sent and received with this request's IFlurlClient. Not necessary to call when setting cookies via WithCookie/WithCookies.
		/// </summary>
		/// <param name="request">The IFlurlRequest.</param>
		/// <returns>This IFlurlRequest.</returns>
		public static IFlurlRequest EnableCookies(this IFlurlRequest request) {
			request.Settings.CookiesEnabled = true; // a little awkward to have this at both the client and request.
			request.Client.EnableCookies();
			return request;
		}

		/// <summary>
		/// Sets an HTTP cookie to be sent with this request's IFlurlClient.
		/// </summary>
		/// <param name="request">The IFlurlRequest.</param>
		/// <param name="cookie">The cookie to set.</param>
		/// <returns>This IFlurlRequest.</returns>
		public static IFlurlRequest WithCookie(this IFlurlRequest request, Cookie cookie) {
			request.Client.WithCookie(cookie);
			return request;
		}

		/// <summary>
		/// Sets an HTTP cookie to be sent with this request's IFlurlClient.
		/// </summary>
		/// <param name="request">The IFlurlRequest.</param>
		/// <param name="name">The cookie name.</param>
		/// <param name="value">The cookie value.</param>
		/// <param name="expires">The cookie expiration (optional). If excluded, cookie only lives for duration of session.</param>
		/// <returns>This IFlurlRequest.</returns>
		public static IFlurlRequest WithCookie(this IFlurlRequest request, string name, object value, DateTime? expires = null) {
			request.Client.WithCookie(name, value, expires);
			return request;
		}

		/// <summary>
		/// Sets HTTP cookies to be sent with this request's IFlurlClient, based on property names/values of the provided object, or keys/values if object is a dictionary.
		/// </summary>
		/// <param name="request">The IFlurlRequest.</param>
		/// <param name="cookies">Names/values of HTTP cookies to set. Typically an anonymous object or IDictionary.</param>
		/// <param name="expires">Expiration for all cookies (optional). If excluded, cookies only live for duration of session.</param>
		/// <returns>This IFlurlRequest.</returns>
		public static IFlurlRequest WithCookies(this IFlurlRequest request, object cookies, DateTime? expires = null) {
			request.Client.WithCookies(cookies, expires);
			return request;
		}
	}
}
