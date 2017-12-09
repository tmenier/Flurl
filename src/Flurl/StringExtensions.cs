using System.Collections.Generic;

namespace Flurl
{
	/// <summary>
	/// A set of string extension methods for working with Flurl URLs
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Creates a new Url object from the string and appends a segment to the URL path,
		/// ensuring there is one and only one '/' character as a separator.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="segment">The segment to append</param>
		/// <param name="fullyEncode">If true, URL-encodes reserved characters such as '/', '+', and '%'. Otherwise, only encodes strictly illegal characters (including '%' but only when not followed by 2 hex characters).</param>
		/// <returns>
		/// the resulting Url object
		/// </returns>
		public static Url AppendPathSegment(this string url, object segment, bool fullyEncode = false) {
			return new Url(url).AppendPathSegment(segment, fullyEncode);
		}

		/// <summary>
		/// Appends multiple segments to the URL path, ensuring there is one and only one '/' character as a seperator.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="segments">The segments to append</param>
		/// <returns>
		/// the Url object with the segments appended
		/// </returns>
		public static Url AppendPathSegments(this string url, params object[] segments) {
			return new Url(url).AppendPathSegments(segments);
		}

		/// <summary>
		/// Appends multiple segments to the URL path, ensuring there is one and only one '/' character as a seperator.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="segments">The segments to append</param>
		/// <returns>
		/// the Url object with the segments appended
		/// </returns>
		public static Url AppendPathSegments(this string url, IEnumerable<object> segments) {
			return new Url(url).AppendPathSegments(segments);
		}

		/// <summary>
		/// Creates a new Url object from the string and adds a parameter to the query, overwriting the value if name exists.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="name">Name of query parameter</param>
		/// <param name="value">Value of query parameter</param>
		/// <param name="nullValueHandling">Indicates how to handle null values. Defaults to Remove (any existing)</param>
		/// <returns>The Url object with the query parameter added</returns>
		public static Url SetQueryParam(this string url, string name, object value, NullValueHandling nullValueHandling = NullValueHandling.Remove) {
			return new Url(url).SetQueryParam(name, value, nullValueHandling);
		}

		/// <summary>
		/// Creates a new Url object from the string and adds a parameter to the query, overwriting the value if name exists.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="name">Name of query parameter</param>
		/// <param name="value">Value of query parameter</param>
		/// <param name="isEncoded">Set to true to indicate the value is already URL-encoded. Defaults to false.</param>
		/// <param name="nullValueHandling">Indicates how to handle null values. Defaults to Remove (any existing).</param>
		/// <returns>
		/// The Url object with the query parameter added
		/// </returns>
		public static Url SetQueryParam(this string url, string name, string value, bool isEncoded = false, NullValueHandling nullValueHandling = NullValueHandling.Remove) {
			return new Url(url).SetQueryParam(name, value, isEncoded, nullValueHandling);
		}

		/// <summary>
		/// Creates a new Url object from the string and adds a parameter without a value to the query, removing any existing value.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="name">Name of query parameter</param>
		/// <returns>The Url object with the query parameter added</returns>
		public static Url SetQueryParam(this string url, string name) {
			return new Url(url).SetQueryParam(name);
		}

		/// <summary>
		/// Creates a new Url object from the string, parses values object into name/value pairs, and adds them to the query,
		/// overwriting any that already exist.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="values">Typically an anonymous object, ie: new { x = 1, y = 2 }</param>
		/// <param name="nullValueHandling">Indicates how to handle null values. Defaults to Remove (any existing)</param>
		/// <returns>
		/// The Url object with the query parameters added
		/// </returns>
		public static Url SetQueryParams(this string url, object values, NullValueHandling nullValueHandling = NullValueHandling.Remove) {
			return new Url(url).SetQueryParams(values, nullValueHandling);
		}

		/// <summary>
		/// Creates a new Url object from the string and adds multiple parameters without values to the query.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="names">Names of query parameters.</param>
		/// <returns>The Url object with the query parameter added</returns>
		public static Url SetQueryParams(this string url, IEnumerable<string> names) {
			return new Url(url).SetQueryParams(names);
		}

		/// <summary>
		/// Creates a new Url object from the string and adds multiple parameters without values to the query.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="names">Names of query parameters</param>
		/// <returns>The Url object with the query parameter added.</returns>
		public static Url SetQueryParams(this string url, params string[] names) {
			return new Url(url).SetQueryParams(names);
		}

		/// <summary>
		/// Creates a new Url object from the string and removes a name/value pair from the query by name.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="name">Query string parameter name to remove</param>
		/// <returns>
		/// The Url object with the query parameter removed
		/// </returns>
		public static Url RemoveQueryParam(this string url, string name) {
			return new Url(url).RemoveQueryParam(name);
		}

		/// <summary>
		/// Creates a new Url object from the string and removes multiple name/value pairs from the query by name.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="names">Query string parameter names to remove</param>
		/// <returns>
		/// The Url object with the query parameters removed
		/// </returns>
		public static Url RemoveQueryParams(this string url, params string[] names) {
			return new Url(url).RemoveQueryParams(names);
		}

		/// <summary>
		/// Creates a new Url object from the string and removes multiple name/value pairs from the query by name.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="names">Query string parameter names to remove</param>
		/// <returns>
		/// The Url object with the query parameters removed
		/// </returns>
		public static Url RemoveQueryParams(this string url, IEnumerable<string> names) {
			return new Url(url).RemoveQueryParams(names);
		}

		/// <summary>
		/// Set the URL fragment fluently.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <param name="fragment">The part of the URL afer #</param>
		/// <returns>
		/// The Url object with the new fragment set
		/// </returns>
		public static Url SetFragment(this string url, string fragment) {
			return new Url(url).SetFragment(fragment);
		}

		/// <summary>
		/// Removes the URL fragment including the #.
		/// </summary>
		/// <returns>The Url object with the fragment removed</returns>
		public static Url RemoveFragment(this string url) {
			return new Url(url).RemoveFragment();
		}

		/// <summary>
		/// Trims the URL to its root, including the scheme, any user info, host, and port (if specified).
		/// </summary>
		/// <returns>A Url object.</returns>
		public static Url ResetToRoot(this string url) {
			return new Url(url).ResetToRoot();
		}
	}
}