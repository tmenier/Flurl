using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl.Util;

namespace Flurl.Http
{
	/// <summary>
	/// Fluent extension methods for working with HTTP request headers.
	/// </summary>
    public static class HeaderExtensions
    {
	    /// <summary>
	    /// Sets an HTTP header to be sent with this IFlurlRequest or all requests made with this IFlurlClient.
	    /// </summary>
	    /// <param name="obj">The IFlurlClient or IFlurlRequest.</param>
	    /// <param name="name">HTTP header name.</param>
	    /// <param name="value">HTTP header value.</param>
	    /// <returns>This IFlurlClient or IFlurlRequest.</returns>
	    public static T WithHeader<T>(this T obj, string name, object value) where T : IHttpSettingsContainer {
		    obj.Headers[name] = value;
		    return obj;
	    }

	    /// <summary>
	    /// Sets HTTP headers based on property names/values of the provided object, or keys/values if object is a dictionary, to be sent with this IFlurlRequest or all requests made with this IFlurlClient.
	    /// </summary>
	    /// <param name="obj">The IFlurlClient or IFlurlRequest.</param>
	    /// <param name="headers">Names/values of HTTP headers to set. Typically an anonymous object or IDictionary.</param>
	    /// <returns>This IFlurlClient or IFlurlRequest.</returns>
	    public static T WithHeaders<T>(this T obj, object headers) where T : IHttpSettingsContainer {
		    if (headers == null)
			    return obj;

		    foreach (var kv in headers.ToKeyValuePairs()) {
			    obj.WithHeader(kv.Key, kv.Value);
		    }

		    return obj;
	    }

	    /// <summary>
	    /// Sets HTTP authorization header according to Basic Authentication protocol to be sent with this IFlurlRequest or all requests made with this IFlurlClient.
	    /// </summary>
	    /// <param name="obj">The IFlurlClient or IFlurlRequest.</param>
	    /// <param name="username">Username of authenticating user.</param>
	    /// <param name="password">Password of authenticating user.</param>
	    /// <returns>This IFlurlClient or IFlurlRequest.</returns>
	    public static T WithBasicAuth<T>(this T obj, string username, string password) where T : IHttpSettingsContainer {
		    // http://stackoverflow.com/questions/14627399/setting-authorization-header-of-httpclient
		    var encodedCreds = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
		    return obj.WithHeader("Authorization", $"Basic {encodedCreds}");
	    }

	    /// <summary>
	    /// Sets HTTP authorization header with acquired bearer token according to OAuth 2.0 specification to be sent with this IFlurlRequest or all requests made with this IFlurlClient.
	    /// </summary>
	    /// <param name="obj">The IFlurlClient or IFlurlRequest.</param>
	    /// <param name="token">The acquired bearer token to pass.</param>
	    /// <returns>This IFlurlClient or IFlurlRequest.</returns>
	    public static T WithOAuthBearerToken<T>(this T obj, string token) where T : IHttpSettingsContainer {
		    return obj.WithHeader("Authorization", $"Bearer {token}");
	    }
    }
}
