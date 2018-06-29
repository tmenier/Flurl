using Flurl.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

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
			get => QueryParams.ToString();
			set => QueryParams = ParseQueryParams(value);
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
		/// Decodes a URL-encoded string.
		/// </summary>
		/// <param name="s">The URL-encoded string.</param>
		/// <param name="interpretPlusAsSpace">If true, any '+' character will be decoded to a space.</param>
		/// <returns></returns>
		public static string Decode(string s, bool interpretPlusAsSpace) {
			if (string.IsNullOrEmpty(s))
				return s;

			s = Uri.UnescapeDataString(s);
			return interpretPlusAsSpace ? s.Replace("+", " ") : s;
		}

		private const int MAX_URL_LENGTH = 65519;

		/// <summary>
		/// URL-encodes a string, including reserved characters such as '/' and '?'.
		/// </summary>
		/// <param name="s">The string to encode.</param>
		/// <param name="encodeSpaceAsPlus">If true, spaces will be encoded as + signs. Otherwise, they'll be encoded as %20.</param>
		/// <returns>The encoded URL.</returns>
		public static string Encode(string s, bool encodeSpaceAsPlus = false) {
			if (string.IsNullOrEmpty(s))
				return s;

			if (s.Length > MAX_URL_LENGTH) {
				// Uri.EscapeDataString is going to throw because the string is "too long", so break it into pieces and concat them
				var parts = new string[(int)Math.Ceiling((double)s.Length / MAX_URL_LENGTH)];
				for (var i = 0; i < parts.Length; i++) {
					var start = i * MAX_URL_LENGTH;
					var len = Math.Min(MAX_URL_LENGTH, s.Length - start);
					parts[i] = Uri.EscapeDataString(s.Substring(start, len));
				}
				s = string.Concat(parts);
			}
			else {
				s = Uri.EscapeDataString(s);
			}
			return encodeSpaceAsPlus ? s.Replace("%20", "+") : s;
		}

		/// <summary>
		/// URL-encodes characters in a string that are neither reserved nor unreserved. Avoids encoding reserved characters such as '/' and '?'. Avoids encoding '%' if it begins a %-hex-hex sequence (i.e. avoids double-encoding).
		/// </summary>
		/// <param name="s">The string to encode.</param>
		/// <param name="encodeSpaceAsPlus">If true, spaces will be encoded as + signs. Otherwise, they'll be encoded as %20.</param>
		/// <returns>The encoded URL.</returns>
		public static string EncodeIllegalCharacters(string s, bool encodeSpaceAsPlus = false) {
			if (string.IsNullOrEmpty(s))
				return s;

			if (encodeSpaceAsPlus)
				s = s.Replace(" ", "+");

			// Uri.EscapeUriString mostly does what we want - encodes illegal characters only - but it has a quirk
			// in that % isn't illegal if it's the start of a %-encoded sequence https://stackoverflow.com/a/47636037/62600

			// no % characters, so avoid the regex overhead
			if (!s.Contains("%"))
				return Uri.EscapeUriString(s);

			// pick out all %-hex-hex matches and avoid double-encoding 
			return Regex.Replace(s, "(.*?)((%[0-9A-Fa-f]{2})|$)", c => {
				var a = c.Groups[1].Value; // group 1 is a sequence with no %-encoding - encode illegal characters
				var b = c.Groups[2].Value; // group 2 is a valid 3-character %-encoded sequence - leave it alone!
				return Uri.EscapeUriString(a) + b;
			});
		}

		/// <summary>
		/// Appends a segment to the URL path, ensuring there is one and only one '/' character as a seperator.
		/// </summary>
		/// <param name="segment">The segment to append</param>
		/// <param name="fullyEncode">If true, URL-encodes reserved characters such as '/', '+', and '%'. Otherwise, only encodes strictly illegal characters (including '%' but only when not followed by 2 hex characters).</param>
		/// <returns>the Url object with the segment appended</returns>
		/// <exception cref="ArgumentNullException"><paramref name="segment"/> is <see langword="null" />.</exception>
		public Url AppendPathSegment(object segment, bool fullyEncode = false) {
			if (segment == null)
				throw new ArgumentNullException(nameof(segment));

			var encoded = fullyEncode ? 
				Uri.EscapeDataString(segment.ToInvariantString()) :
				EncodeIllegalCharacters(segment.ToInvariantString());

			Path = CombineEnsureSingleSeperator(Path, encoded.Replace("?", "%3F"), '/');
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
			QueryParams.Merge(name, value, false, nullValueHandling);
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
			QueryParams.Merge(name, value, isEncoded, nullValueHandling);
			return this;
		}

		/// <summary>
		/// Adds a parameter without a value to the query, removing any existing value.
		/// </summary>
		/// <param name="name">Name of query parameter</param>
		/// <returns>The Url object with the query parameter added</returns>
		public Url SetQueryParam(string name) {
			QueryParams.Merge(name, null, false, NullValueHandling.NameOnly);
			return this;
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

            var type = values.GetType();            
            foreach (var kv in values.ToKeyValuePairs()) {
                var paramName = kv.Key;
#if NETSTANDARD1_0
                var prop = type.GetRuntimeProperty(kv.Key);
#else
                var prop = type.GetProperty(kv.Key);
#endif
                if (prop != null) {
                    var attributes = prop.GetCustomAttributes(typeof(JsonPropertyAttribute), false);
                    if (attributes != null) {
                        if (attributes.Any() && attributes.First() != null) paramName = ((JsonPropertyAttribute)attributes.First()).PropertyName;
                    }
                }
                QueryParams.Merge(paramName, kv.Value, false, nullValueHandling);
            }

            return this;
		}

        private static bool TryGetAttribute<T>(MemberInfo memberInfo, out T customAttribute) where T : Attribute
        {
            var attributes = memberInfo.GetCustomAttributes(typeof(T), false).FirstOrDefault();
            if (attributes == null)
            {
                customAttribute = null;
                return false;
            }
            customAttribute = (T)attributes;
            return true;
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
			var sb = new System.Text.StringBuilder(encodeSpaceAsPlus ? Path.Replace("%20", "+") : Path);
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
			return url?.ToString();
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