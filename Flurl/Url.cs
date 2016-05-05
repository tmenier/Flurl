using Flurl.Util;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Flurl
{
	/// <summary>
	/// Represents a URL that can be built fluently
	/// </summary>
	public class Url
	{
		/// <summary>
		/// The full absolute path part of the URL (everthing except the query string).
		/// </summary>
		public string Path { get; private set; }

		/// <summary>
		/// Collection of all query string parameters.
		/// </summary>
		public QueryParamCollection QueryParams { get; private set; }

		/// <summary>
		/// The fragment part of the url (after the #, RFC 3986)
		/// </summary>
		public string Fragment { get; private set; }

		const string URL_SLICE_REGEXP = @"^([^?#\n]*)([^#\n]*)(.*)$";

		/// <summary>
		/// Constructs a Url object from a string.
		/// </summary>
		/// <param name="baseUrl">The URL to use as a starting point (required)</param>
		public Url(string baseUrl) {
			if(baseUrl == null)
				throw new ArgumentNullException("baseUrl");

			var urlParts = Regex.Match(baseUrl, URL_SLICE_REGEXP, RegexOptions.IgnoreCase | RegexOptions.Singleline);
			Path = urlParts.Groups[1].Value;
			QueryParams = QueryParamCollection.Parse(urlParts.Groups[2].Value);
			Fragment = urlParts.Groups[3].Value;
		}

		/// <summary>
		/// Basically a Path.Combine for URLs. Ensures exactly one '/' character is used to seperate each segment.
		/// URL-encodes illegal characters but not reserved characters.
		/// </summary>
		/// <param name="url">The URL to use as a starting point (required).</param>
		/// <param name="segments">Paths to combine.</param>
		/// <returns></returns>
		public static string Combine(string url, params string[] segments) {
			if (url == null)
				throw new ArgumentNullException("url");

			return new Url(url).AppendPathSegments(segments).ToString();
		}

		/// <summary>
		/// Returns the root URL of the given full URL, including the scheme, any user info, host, and port (if specified).
		/// </summary>
		public static string GetRoot(string url) {
			// http://stackoverflow.com/a/27473521/62600
			var uri = new Uri(url);
			return uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.UserInfo, UriFormat.Unescaped);
		}

		/// <summary>
		/// Decodes a URL-encoded query string value.
		/// </summary>
		/// <param name="value">The encoded query string value.</param>
		/// <returns></returns>
		public static string DecodeQueryParamValue(string value) {
			// Uri.UnescapeDataString comes closest to doing it right, but famously stumbles on the + sign
			// http://weblog.west-wind.com/posts/2009/Feb/05/Html-and-Uri-String-Encoding-without-SystemWeb
			return Uri.UnescapeDataString((value ?? "").Replace("+", " "));
		}

		/// <summary>
		/// URL-encodes a query string value.
		/// </summary>
		/// <param name="value">The query string value to encode.</param>
		/// <param name="value">If true, spaces will be encoded as + signs. Otherwise, they'll be encoded as %20.</param>
		/// <returns></returns>
		public static string EncodeQueryParamValue(object value, bool encodeSpaceAsPlus) {
			var result = Uri.EscapeDataString((value ?? "").ToInvariantString());
			return encodeSpaceAsPlus ? result.Replace("%20", "+") : result;
		}

		/// <summary>
		/// Encodes characters that are illegal in a URL path, including '?'. Does not encode reserved characters, i.e. '/', '+', etc.
		/// </summary>
		/// <param name="segment"></param>
		/// <returns></returns>
		private static string CleanSegment(string segment) {
			// http://stackoverflow.com/questions/4669692/valid-characters-for-directory-part-of-a-url-for-short-links
			var unescaped = Uri.UnescapeDataString(segment);
			return Uri.EscapeUriString(unescaped).Replace("?", "%3F");
		}

		/// <summary>
		/// Appends a segment to the URL path, ensuring there is one and only one '/' character as a seperator.
		/// </summary>
		/// <param name="segment">The segment to append</param>
		/// <param name="encode">If true, URL-encode the segment where necessary</param>
		/// <returns>the Url object with the segment appended</returns>
		public Url AppendPathSegment(string segment) {
			if (segment == null)
				throw new ArgumentNullException("segment");

			if (!Path.EndsWith("/")) Path += "/";
			Path += CleanSegment(segment.TrimStart('/'));
			return this;
		}

		/// <summary>
		/// Appends multiple segments to the URL path, ensuring there is one and only one '/' character as a seperator.
		/// </summary>
		/// <param name="segments">The segments to append</param>
		/// <returns>the Url object with the segments appended</returns>
		public Url AppendPathSegments(params string[] segments) {
			foreach(var segment in segments)
				AppendPathSegment(segment);

			return this;
		}

		/// <summary>
		/// Appends multiple segments to the URL path, ensuring there is one and only one '/' character as a seperator.
		/// </summary>
		/// <param name="segments">The segments to append</param>
		/// <returns>the Url object with the segments appended</returns>
		public Url AppendPathSegments(IEnumerable<string> segments) {
			foreach(var s in segments)
				AppendPathSegment(s);

			return this;
		}

		/// <summary>
		/// Adds a parameter to the query string, overwriting the value if name exists.
		/// </summary>
		/// <param name="name">name of query string parameter</param>
		/// <param name="value">value of query string parameter</param>
		/// <returns>The Url obect with the query string parameter added</returns>
		public Url SetQueryParam(string name, object value) {
			if (name == null)
				throw new ArgumentNullException("name", "Query parameter name cannot be null.");

			QueryParams[name] = value;
			return this;
		}

		/// <summary>
		/// Parses values (usually an anonymous object or dictionary) into name/value pairs and adds them to the query string, overwriting any that already exist.
		/// </summary>
		/// <param name="values">Typically an anonymous object, ie: new { x = 1, y = 2 }</param>
		/// <returns>The Url object with the query string parameters added</returns>
		public Url SetQueryParams(object values) {
			if (values == null)
				return this;

			foreach (var kv in values.ToKeyValuePairs())
				SetQueryParam(kv.Key, kv.Value);

			return this;
		}

		/// <summary>
		/// Removes a name/value pair from the query string by name.
		/// </summary>
		/// <param name="name">Query string parameter name to remove</param>
		/// <returns>The Url object with the query string parameter removed</returns>
		public Url RemoveQueryParam(string name) {
			QueryParams.Remove(name);
			return this;
		}

		/// <summary>
		/// Removes multiple name/value pairs from the query string by name.
		/// </summary>
		/// <param name="names">Query string parameter names to remove</param>
		/// <returns>The Url object with the query string parameters removed</returns>
		public Url RemoveQueryParams(params string[] names) {
			foreach(var name in names)
				QueryParams.Remove(name);

			return this;
		}

		/// <summary>
		/// Removes multiple name/value pairs from the query string by name.
		/// </summary>
		/// <param name="names">Query string parameter names to remove</param>
		/// <returns>The Url object with the query string parameters removed</returns>
		public Url RemoveQueryParams(IEnumerable<string> names) {
			foreach(var name in names)
				QueryParams.Remove(name);

			return this;
		}

		/// <summary>
		/// Resets the URL to its root, including the scheme, any user info, host, and port (if specified).
		/// </summary>
		/// <returns>The Url object trimmed to its root.</returns>
		public Url ResetToRoot() {
			Path = GetRoot(Path);
			QueryParams.Clear();
			return this;
		}

		/// <summary>
		/// Converts this Url object to its string representation.
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			return ToString(false);
		}

		/// <summary>
		/// Converts this Url object to its string representation.
		/// </summary>
		/// <returns></returns>
		public string ToString(bool encodeStringAsPlus) {
			var url = Path;
			var query = QueryParams.ToString(encodeStringAsPlus);
			if(query.Length > 0)
				url += "?" + query;

			url += Fragment;

			return url;
		}

		/// <summary>
		/// Implicit conversion from Url to String.
		/// </summary>
		/// <param name="url">the Url object</param>
		/// <returns>The string</returns>
		public static implicit operator string(Url url) {
			return url.ToString();
		}

		/// <summary>
		/// Implicit conversion from String to Url.
		/// </summary>
		/// <param name="url">the String representation of the URL</param>
		/// <returns>The string</returns>
		public static implicit operator Url(string url) {
			return new Url(url);
		}
	}
}
