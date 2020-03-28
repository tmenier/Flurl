using System;
using System.Collections.Concurrent;
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
		/// <param name="clientOrRequest">The IFlurlClient or IFlurlRequest.</param>
		/// <returns>This IFlurlClient.</returns>
		public static T EnableCookies<T>(this T clientOrRequest) where T : IHttpSettingsContainer {
			clientOrRequest.Settings.CookiesEnabled = true;
			return clientOrRequest;
		}

		/// <summary>
		/// Sets an HTTP cookie to be sent with this IFlurlRequest or all requests made with this IFlurlClient.
		/// </summary>
		/// <param name="clientOrRequest">The IFlurlClient or IFlurlRequest.</param>
		/// <param name="cookie">The cookie to set.</param>
		/// <returns>This IFlurlClient.</returns>
		public static T WithCookie<T>(this T clientOrRequest, Cookie cookie) where T : IHttpSettingsContainer {
			clientOrRequest.Settings.CookiesEnabled = true;
			clientOrRequest.Cookies[cookie.Name] = cookie.Value;
			return clientOrRequest;
		}

		/// <summary>
		/// Sets an HTTP cookie to be sent with this IFlurlRequest or all requests made with this IFlurlClient.
		/// </summary>
		/// <param name="clientOrRequest">The IFlurlClient or IFlurlRequest.</param>
		/// <param name="name">The cookie name.</param>
		/// <param name="value">The cookie value.</param>
		/// <returns>This IFlurlClient.</returns>
		public static T WithCookie<T>(this T clientOrRequest, string name, object value) where T : IHttpSettingsContainer {
			clientOrRequest.Cookies[name] = value?.ToInvariantString();
			return clientOrRequest;
		}

		/// <summary>
		/// Sets HTTP cookies to be sent with this IFlurlRequest, based on property names/values of the provided object, or keys/values if object is a dictionary.
		/// </summary>
		/// <param name="request">The IFlurlRequest.</param>
		/// <param name="values">Names/values of HTTP cookies to set. Typically an anonymous object or IDictionary.</param>
		/// <param name="cookies">The collection of cookies that will be initialized with the given values, possibly modified by the response, and pass-able to subsequent requests.</param>
		/// <returns>This IFlurlClient.</returns>
		public static IFlurlRequest WithCookies(this IFlurlRequest request, object values, out IDictionary<string, Cookie> cookies) {
			cookies = new ConcurrentDictionary<string, Cookie>(values == null ?
				Enumerable.Empty<KeyValuePair<string, Cookie>>() :
				values
					.ToKeyValuePairs()
					.Select(kv => new KeyValuePair<string, Cookie>(kv.Key, new Cookie(kv.Key, kv.Value?.ToInvariantString()))));

			return request.WithCookies(cookies);
		}

		/// <summary>
		/// Sets HTTP cookies to be sent with this IFlurlRequest, based on property names/values of the provided object, or keys/values if object is a dictionary.
		/// </summary>
		/// <param name="request">The IFlurlRequest.</param>
		/// <param name="values">Names/values of HTTP cookies to set. Typically an anonymous object or IDictionary.</param>
		/// <returns>This IFlurlClient.</returns>
		public static IFlurlRequest WithCookies(this IFlurlRequest request, object values) {
			foreach (var kv in values.ToKeyValuePairs())
				request.Cookies[kv.Key] = kv.Value?.ToInvariantString();

			return request;
		}

		/// <summary>
		/// Creates a new CookieSession, under which all requests and responses share a cookie collection.
		/// </summary>
		public static CookieSession StartCookieSession(this IFlurlClient client) => new CookieSession(client);
	}
}
