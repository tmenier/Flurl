using System;
using System.Collections;
using System.Text;
using Flurl.Util;

namespace Flurl.Http
{
	/// <summary>
	/// A common interface for Flurl.Http objects that contain a collection of request headers.
	/// </summary>
	public interface IHeadersContainer
	{
		/// <summary>
		/// A collection of request headers.
		/// </summary>
		INameValueList<string> Headers { get; }
	}

	/// <summary>
	/// Fluent extension methods for working with HTTP request headers.
	/// </summary>
	public static class HeaderExtensions
    {
		/// <summary>
		/// Sets an HTTP header associated with this request or client.
		/// </summary>
		/// <param name="obj">Object containing request headers.</param>
		/// <param name="name">HTTP header name.</param>
		/// <param name="value">HTTP header value.</param>
		/// <returns>This headers container.</returns>
		public static T WithHeader<T>(this T obj, string name, object value) where T : IHeadersContainer {
		    if (value == null)
			    obj.Headers.Remove(name);
			else
			    obj.Headers.AddOrReplace(name, value.ToInvariantString().Trim());
		    return obj;
	    }

		/// <summary>
		/// Sets HTTP headers based on property names/values of the provided object, or keys/values if object is a dictionary, associated with this request or client.
		/// </summary>
		/// <param name="obj">Object containing request headers.</param>
		/// <param name="headers">Names/values of HTTP headers to set. Typically an anonymous object or IDictionary.</param>
		/// <param name="replaceUnderscoreWithHyphen">If true, underscores in property names will be replaced by hyphens. Default is true.</param>
		/// <returns>This headers container.</returns>
		public static T WithHeaders<T>(this T obj, object headers, bool replaceUnderscoreWithHyphen = true) where T : IHeadersContainer {
		    if (headers == null)
			    return obj;

			// underscore replacement only applies when object properties are parsed to kv pairs
		    replaceUnderscoreWithHyphen = replaceUnderscoreWithHyphen && headers is not string && headers is not IEnumerable;

		    foreach (var kv in headers.ToKeyValuePairs()) {
			    var key = replaceUnderscoreWithHyphen ? kv.Key.Replace("_", "-") : kv.Key;
			    obj.WithHeader(key, kv.Value);
		    }

		    return obj;
	    }

		/// <summary>
		/// Sets HTTP authorization header according to Basic Authentication protocol associated with this request or client.
		/// </summary>
		/// <param name="obj">Object containing request headers.</param>
		/// <param name="username">Username of authenticating user.</param>
		/// <param name="password">Password of authenticating user.</param>
		/// <returns>This headers container.</returns>
		public static T WithBasicAuth<T>(this T obj, string username, string password) where T : IHeadersContainer {
		    // http://stackoverflow.com/questions/14627399/setting-authorization-header-of-httpclient
		    var encodedCreds = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
		    return obj.WithHeader("Authorization", $"Basic {encodedCreds}");
	    }

		/// <summary>
		/// Sets HTTP authorization header with acquired bearer token according to OAuth 2.0 specification associated with this request or client.
		/// </summary>
		/// <param name="obj">Object containing request headers.</param>
		/// <param name="token">The acquired bearer token to pass.</param>
		/// <returns>This headers container.</returns>
		public static T WithOAuthBearerToken<T>(this T obj, string token) where T : IHeadersContainer {
		    return obj.WithHeader("Authorization", $"Bearer {token}");
	    }
    }
}
