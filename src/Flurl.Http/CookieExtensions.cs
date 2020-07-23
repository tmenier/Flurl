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
		/// Sets an HTTP cookie to be sent with this request only.
		/// To maintain a cookie "session", consider using WithCookies(CookieJar) or FlurlClient.StartCookieSession instead.
		/// </summary>
		/// <param name="request">The IFlurlRequest.</param>
		/// <param name="name">The cookie name.</param>
		/// <param name="value">The cookie value.</param>
		/// <returns>This IFlurlClient instance.</returns>
		public static IFlurlRequest WithCookie(this IFlurlRequest request, string name, object value) {
			request.Cookies[name] = value;
			return request;
		}

		/// <summary>
		/// Sets HTTP cookies to be sent with this request only, based on property names/values of the provided object, or
		/// keys/values if object is a dictionary. To maintain a cookie "session", consider using WithCookies(CookieJar)
		/// or FlurlClient.StartCookieSession instead.
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

		/// <summary>
		/// Creates a new CookieSession, under which all requests automatically share a common CookieJar.
		/// </summary>
		public static CookieSession StartCookieSession(this IFlurlClient client) => new CookieSession(client);
	}
}
