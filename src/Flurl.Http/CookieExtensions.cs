using System.Linq;
using Flurl.Util;

namespace Flurl.Http
{
	/// <summary>
	/// Fluent extension methods for working with HTTP cookies.
	/// </summary>
	public static class CookieExtensions
	{
		/// <summary>
		/// Adds or updates a name-value pair in this request's Cookie header.
		/// To automatically maintain a cookie "session", consider using a CookieJar or CookieSession instead.
		/// </summary>
		/// <param name="request">The IFlurlRequest.</param>
		/// <param name="name">The cookie name.</param>
		/// <param name="value">The cookie value.</param>
		/// <returns>This IFlurlClient instance.</returns>
		public static IFlurlRequest WithCookie(this IFlurlRequest request, string name, object value) {
			var cookies = new NameValueList<string>(request.Cookies);
			cookies.AddOrReplace(name, value.ToInvariantString());
			return request.WithHeader("Cookie", CookieCutter.ToRequestHeader(cookies));
		}

		/// <summary>
		/// Adds or updates name-value pairs in this request's Cookie header, based on property names/values
		/// of the provided object, or keys/values if object is a dictionary.
		/// To automatically maintain a cookie "session", consider using a CookieJar or CookieSession instead.
		/// </summary>
		/// <param name="request">The IFlurlRequest.</param>
		/// <param name="values">Names/values of HTTP cookies to set. Typically an anonymous object or IDictionary.</param>
		/// <returns>This IFlurlClient.</returns>
		public static IFlurlRequest WithCookies(this IFlurlRequest request, object values) {
			var cookies = new NameValueList<string>(request.Cookies);
			// although rare, we need to accommodate the possibility of multiple cookies with the same name
			foreach (var group in values.ToKeyValuePairs().GroupBy(x => x.Key)) {
				// add or replace the first one (by name)
				cookies.AddOrReplace(group.Key, group.First().Value.ToInvariantString());
				// append the rest
				foreach (var kv in group.Skip(1))
					cookies.Add(kv.Key, kv.Value.ToInvariantString());
			}
			return request.WithHeader("Cookie", CookieCutter.ToRequestHeader(cookies));
		}

		/// <summary>
		/// Sets the CookieJar associated with this request, which will be updated with any Set-Cookie headers present
		/// in the response and is suitable for reuse in subsequent requests.
		/// </summary>
		/// <param name="request">The IFlurlRequest.</param>
		/// <param name="cookieJar">The CookieJar.</param>
		/// <returns>This IFlurlClient instance.</returns>
		public static IFlurlRequest WithCookies(this IFlurlRequest request, CookieJar cookieJar) {
			request.CookieJar = cookieJar;
			return request;
		}

		/// <summary>
		/// Creates a new CookieJar and associates it with this request, which will be updated with any Set-Cookie
		/// headers present in the response and is suitable for reuse in subsequent requests.
		/// </summary>
		/// <param name="request">The IFlurlRequest.</param>
		/// <param name="cookieJar">The created CookieJar, which can be reused in subsequent requests.</param>
		/// <returns>This IFlurlClient instance.</returns>
		public static IFlurlRequest WithCookies(this IFlurlRequest request, out CookieJar cookieJar) {
			cookieJar = new CookieJar();
			return request.WithCookies(cookieJar);
		}
	}
}
