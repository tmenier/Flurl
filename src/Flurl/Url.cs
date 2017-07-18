using Flurl.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Flurl
{
	/// <summary>
	/// Represents a URL that can be built fluently
	/// </summary>
	public class Url
	{
		/// <summary>
		/// The full absolute path part of the URL (everthing except the query and fragment).
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// The query part of the URL (after the ?, RFC 3986).
		/// </summary>
		public string Query {
			get { return QueryParams.ToString(); }
			set { QueryParams = ParseQueryParams(value); }
		}

		/// <summary>
		/// The fragment part of the URL (after the #, RFC 3986).
		/// </summary>
		public string Fragment { get; set; }

		/// <summary>
		/// Query parsed to name/value pairs.
		/// </summary>
		public QueryParamCollection QueryParams { get; private set; }

	    /// <summary>
	    /// Constructs a Url object from a string.
	    /// </summary>
	    /// <param name="baseUrl">The URL to use as a starting point (required)</param>
	    /// <exception cref="ArgumentNullException"><paramref name="baseUrl"/> is <see langword="null" />.</exception>
	    public Url(string baseUrl) {
			if (baseUrl == null)
				throw new ArgumentNullException(nameof(baseUrl));

			var parts = baseUrl.SplitOnFirstOccurence('#');
			Fragment = (parts.Length == 2) ? parts[1] : "";
			parts = parts[0].SplitOnFirstOccurence('?');
			Query = (parts.Length == 2) ? parts[1] : "";
			Path = parts[0];
		}

		/// <summary>
		/// Parses a URL query to a QueryParamCollection dictionary.
		/// </summary>
		/// <param name="query">The URL query to parse.</param>
		public static QueryParamCollection ParseQueryParams(string query) {
			var result = new QueryParamCollection();

			query = query?.TrimStart('?');
			if (string.IsNullOrEmpty(query))
				return result;

			result.AddRange(
				from p in query.Split('&')
				let pair = p.SplitOnFirstOccurence('=')
				let name = pair[0]
				let value = (pair.Length == 1) ? null : pair[1]
				select new QueryParameter(name, value, true));

			return result;
		}

		/// <summary>
		/// Basically a Path.Combine for URLs. Ensures exactly one '/' seperates each segment,
		/// and exactly on '&amp;' seperates each query parameter.
		/// URL-encodes illegal characters but not reserved characters.
		/// </summary>
		/// <param name="parts">URL parts to combine.</param>
		public static string Combine(params string[] parts) {
			if (parts == null)
				throw new ArgumentNullException(nameof(parts));

			string result = "";
			bool inQuery = false, inFragment = false;

		    foreach (var part in parts) {
			    if (string.IsNullOrEmpty(part))
				    continue;

				if (result.EndsWith("?") || part.StartsWith("?"))
					result = CombineEnsureSingleSeperator(result, part, '?');
				else if (result.EndsWith("#") || part.StartsWith("#"))
					result = CombineEnsureSingleSeperator(result, part, '#');
				else if (inFragment)
					result += part;
				else if (inQuery)
					result = CombineEnsureSingleSeperator(result, part, '&');
				else
					result = CombineEnsureSingleSeperator(result, part, '/');

			    if (part.Contains("#")) {
					inQuery = false;
					inFragment = true;
			    }
				else if (!inFragment && part.Contains("?")) {
					inQuery = true;
				}
			}
			return EncodeIllegalCharacters(result);
		}

		/// <summary>
		/// Returns the root URL of the given full URL, including the scheme, any user info, host, and port (if specified).
		/// </summary>
		public static string GetRoot(string url) {
			// http://stackoverflow.com/a/27473521/62600
			return new Uri(url).GetComponents(UriComponents.SchemeAndServer | UriComponents.UserInfo, UriFormat.Unescaped);
		}

		/// <summary>
		/// Decodes a URL-encoded query parameter value.
		/// </summary>
		/// <param name="value">The encoded query parameter value.</param>
		/// <returns></returns>
		public static string DecodeQueryParamValue(string value) {
			// Uri.UnescapeDataString comes closest to doing it right, but famously stumbles on the + sign
			// http://weblog.west-wind.com/posts/2009/Feb/05/Html-and-Uri-String-Encoding-without-SystemWeb
			return Uri.UnescapeDataString((value ?? "").Replace("+", " "));
		}

		/// <summary>
		/// URL-encodes a query parameter value.
		/// </summary>
		/// <param name="value">The query parameter value to encode.</param>
		/// <param name="encodeSpaceAsPlus">If true, spaces will be encoded as + signs. Otherwise, they'll be encoded as %20.</param>
		/// <returns></returns>
		public static string EncodeQueryParamValue(object value, bool encodeSpaceAsPlus) {
			var result = Uri.EscapeDataString((value ?? "").ToInvariantString());
			return encodeSpaceAsPlus ? result.Replace("%20", "+") : result;
		}

		/// <summary>
		/// Encodes characters that are illegal in a URL. Does not encode reserved characters, i.e. '/', '+', etc.
		/// </summary>
		/// <param name="urlPart">The URL or URL part.</param>
		public static string EncodeIllegalCharacters(string urlPart) {
			// http://stackoverflow.com/questions/4669692/valid-characters-for-directory-part-of-a-url-for-short-links
			var unescaped = Uri.UnescapeDataString(urlPart);
			return Uri.EscapeUriString(unescaped);
		}

	    /// <summary>
	    /// Appends a segment to the URL path, ensuring there is one and only one '/' character as a seperator.
	    /// </summary>
	    /// <param name="segment">The segment to append</param>
	    /// <returns>the Url object with the segment appended</returns>
	    /// <exception cref="ArgumentNullException"><paramref name="segment"/> is <see langword="null" />.</exception>
	    public Url AppendPathSegment(object segment) {
			if (segment == null)
				throw new ArgumentNullException(nameof(segment));

			Path = CombineEnsureSingleSeperator(Path, EncodeIllegalCharacters(segment.ToInvariantString()).Replace("?", "%3F"), '/');
			return this;
		}

		private static string CombineEnsureSingleSeperator(string a, string b, char seperator) {
			if (string.IsNullOrEmpty(a)) return b;
			if (string.IsNullOrEmpty(b)) return a;
			return a.TrimEnd(seperator) + seperator + b.TrimStart(seperator);
		}

		/// <summary>
		/// Appends multiple segments to the URL path, ensuring there is one and only one '/' character as a seperator.
		/// </summary>
		/// <param name="segments">The segments to append</param>
		/// <returns>the Url object with the segments appended</returns>
		public Url AppendPathSegments(params object[] segments) {
			foreach(var segment in segments)
				AppendPathSegment(segment);

			return this;
		}

		/// <summary>
		/// Appends multiple segments to the URL path, ensuring there is one and only one '/' character as a seperator.
		/// </summary>
		/// <param name="segments">The segments to append</param>
		/// <returns>the Url object with the segments appended</returns>
		public Url AppendPathSegments(IEnumerable<object> segments) {
			foreach(var s in segments)
				AppendPathSegment(s);

			return this;
		}

		/// <summary>
		/// Adds a parameter to the query, overwriting the value if name exists.
		/// </summary>
		/// <param name="name">Name of query parameter</param>
		/// <param name="value">Value of query parameter</param>
		/// <param name="nullValueHandling">Indicates how to handle null values. Defaults to Remove (any existing)</param>
		/// <returns>The Url object with the query parameter added</returns>
		public Url SetQueryParam(string name, object value, NullValueHandling nullValueHandling = NullValueHandling.Remove) {
			SetQueryParamInternal(name, value, false, nullValueHandling);
			return this;
		}

		/// <summary>
		/// Adds a parameter to the query, overwriting the value if name exists.
		/// </summary>
		/// <param name="name">Name of query parameter</param>
		/// <param name="value">Value of query parameter</param>
		/// <param name="isEncoded">Set to true to indicate the value is already URL-encoded</param>
		/// <param name="nullValueHandling">Indicates how to handle null values. Defaults to Remove (any existing)</param>
		/// <returns>The Url object with the query parameter added</returns>
		/// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null" />.</exception>
		public Url SetQueryParam(string name, string value, bool isEncoded = false, NullValueHandling nullValueHandling = NullValueHandling.Remove) {
			SetQueryParamInternal(name, value, isEncoded, nullValueHandling);
			return this;
		}

		/// <summary>
		/// Adds a parameter without a value to the query, removing any existing value.
		/// </summary>
		/// <param name="name">Name of query parameter</param>
		/// <returns>The Url object with the query parameter added</returns>
		public Url SetQueryParam(string name) {
			SetQueryParamInternal(name, null, false, NullValueHandling.NameOnly);
			return this;
		}

		private void SetQueryParamInternal(string name, object value, bool isEncoded, NullValueHandling nullValueHandling) {
			if (value != null || nullValueHandling == NullValueHandling.NameOnly)
				QueryParams[name] = new QueryParameter(name, value, isEncoded);
			else if (nullValueHandling == NullValueHandling.Remove)
				RemoveQueryParam(name);
		}

		/// <summary>
		/// Parses values (usually an anonymous object or dictionary) into name/value pairs and adds them to the query, overwriting any that already exist.
		/// </summary>
		/// <param name="values">Typically an anonymous object, ie: new { x = 1, y = 2 }</param>
		/// <param name="nullValueHandling">Indicates how to handle null values. Defaults to Remove (any existing)</param>
		/// <returns>The Url object with the query parameters added</returns>
		public Url SetQueryParams(object values, NullValueHandling nullValueHandling = NullValueHandling.Remove) {
			if (values == null)
				return this;

			foreach (var kv in values.ToKeyValuePairs())
				SetQueryParamInternal(kv.Key, kv.Value, false, nullValueHandling);

			return this;
		}

		/// <summary>
		/// Adds multiple parameters without values to the query.
		/// </summary>
		/// <param name="names">Names of query parameters.</param>
		/// <returns>The Url object with the query parameter added</returns>
		public Url SetQueryParams(IEnumerable<string> names) {
			foreach (var name in names.Where(n => n != null))
				SetQueryParam(name);
			return this;
		}

		/// <summary>
		/// Adds multiple parameters without values to the query.
		/// </summary>
		/// <param name="names">Names of query parameters</param>
		/// <returns>The Url object with the query parameter added.</returns>
		public Url SetQueryParams(params string[] names) {
			return SetQueryParams(names as IEnumerable<string>);
		}

		/// <summary>
		/// Removes a name/value pair from the query by name.
		/// </summary>
		/// <param name="name">Query string parameter name to remove</param>
		/// <returns>The Url object with the query parameter removed</returns>
		public Url RemoveQueryParam(string name) {
			QueryParams.Remove(name);
			return this;
		}

		/// <summary>
		/// Removes multiple name/value pairs from the query by name.
		/// </summary>
		/// <param name="names">Query string parameter names to remove</param>
		/// <returns>The Url object with the query parameters removed</returns>
		public Url RemoveQueryParams(params string[] names) {
			foreach(var name in names)
				QueryParams.Remove(name);
			return this;
		}

		/// <summary>
		/// Removes multiple name/value pairs from the query by name.
		/// </summary>
		/// <param name="names">Query string parameter names to remove</param>
		/// <returns>The Url object with the query parameters removed</returns>
		public Url RemoveQueryParams(IEnumerable<string> names) {
			foreach(var name in names)
				QueryParams.Remove(name);

			return this;
		}

		/// <summary>
		/// Set the URL fragment fluently.
		/// </summary>
		/// <param name="fragment">The part of the URL afer #</param>
		/// <returns>The Url object with the new fragment set</returns>
		public Url SetFragment(string fragment) {
			Fragment = fragment ?? "";
			return this;
		}

		/// <summary>
		/// Removes the URL fragment including the #.
		/// </summary>
		/// <returns>The Url object with the fragment removed</returns>
		public Url RemoveFragment() {
			return SetFragment("");
		}

		/// <summary>
		/// Checks if this URL is a well-formed.
		/// </summary>
		/// <returns>true if this is a well-formed URL</returns>
		public bool IsValid() => IsValid(ToString());

		/// <summary>
		/// Checks if a string is a well-formed URL.
		/// </summary>
		/// <param name="url">The string to check</param>
		/// <returns>true if s is a well-formed URL</returns>
		public static bool IsValid(string url) {
			return url != null && Uri.IsWellFormedUriString(url, UriKind.Absolute);
		}

		/// <summary>
		/// Resets the URL to its root, including the scheme, any user info, host, and port (if specified).
		/// </summary>
		/// <returns>The Url object trimmed to its root.</returns>
		public Url ResetToRoot() {
			Path = GetRoot(Path);
			QueryParams.Clear();
			Fragment = "";
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
		/// <param name="encodeSpaceAsPlus">Indicates whether to encode spaces with the "+" character instead of "%20"</param>
		/// <returns></returns>
		public string ToString(bool encodeSpaceAsPlus) {
			var sb = new System.Text.StringBuilder(Path);
			if (Query.Length > 0)
				sb.Append("?").Append(QueryParams.ToString(encodeSpaceAsPlus));
			if (Fragment.Length > 0)
				sb.Append("#").Append(Fragment);
			return sb.ToString();
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

		/// <summary>
		/// Implicit conversion from System.Uri to Flurl.Url.
		/// </summary>
		/// <returns>The string</returns>
		public static implicit operator Url(Uri uri) {
			return new Url(uri.ToString());
		}
	}
}