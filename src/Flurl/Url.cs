using Flurl.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Flurl
{
	/// <summary>
	/// A mutable object for fluently building and parsing URLs.
	/// </summary>
	public class Url
	{
		private bool _leadingSlash = false;
		private bool _trailingSlash = false;

		#region properties
		/// <summary>
		/// i.e. "/path" in "https://www.site.com/path". Empty string if not present. Leading and trailing "/" retained exactly as specified by user.
		/// </summary>
		public string Path {
			get {
				var sb = new StringBuilder();
				if (_leadingSlash) sb.Append("/");
				sb.Append(string.Join("/", PathSegments));
				if (_trailingSlash) sb.Append("/");
				return sb.ToString();
			}
			set {
				PathSegments.Clear();
				AppendPathSegment(value);
			}
		}

		/// <summary>
		/// The "/"-delimited segments of the path, not including leading or trailing "/" characters.
		/// </summary>
		public IList<string> PathSegments { get; } = new List<string>();

		/// <summary>
		/// The scheme of the URL, i.e. "http". Does not include ":" delimiter. Empty string if the URL is relative.
		/// </summary>
		public string Scheme { get; set; }

		/// <summary>
		/// i.e. "user:pass" in "https://user:pass@www.site.com". Empty string if not present.
		/// </summary>
		public string UserInfo { get; set; }

		/// <summary>
		/// i.e. "www.site.com" in "https://www.site.com:8080/path". Does not include user info or port.
		/// </summary>
		public string Host { get; set; }

		/// <summary>
		/// Port number of the URL. Null if not explicitly specified.
		/// </summary>
		public int? Port { get; set; }

		/// <summary>
		/// i.e. "www.site.com:8080" in "https://www.site.com:8080/path". Includes both user info and port, if included.
		/// </summary>
		public string Authority {
			get {
				var sb = new StringBuilder();
				if (!string.IsNullOrEmpty(UserInfo))
					sb.Append(UserInfo).Append("@");
				sb.Append(Host);
				if (Port.HasValue)
					sb.Append(":").Append(Port);
				return sb.ToString();
			}
		}

		/// <summary>
		/// i.e. "https://www.site.com:8080" in "https://www.site.com:8080/path" (everything before the path).
		/// </summary>
		public string Root {
			get {
				var sb = new StringBuilder();
				if (!string.IsNullOrEmpty(Scheme))
					sb.Append(Scheme).Append(":");
				var a = Authority; // avoid parsing it twice
				if (!string.IsNullOrEmpty(a))
					sb.Append("//").Append(a);
				return sb.ToString();
			}
		}

		/// <summary>
		/// i.e. "x=1&y=2" in "https://www.site.com/path?x=1&y=2". Does not include "?".
		/// </summary>
		public string Query {
			get => QueryParams.ToString();
			set => QueryParams = ParseQueryParams(value);
		}

		/// <summary>
		/// i.e. "frag" in "https://www.site.com/path?x=y#frag". Does not include "#".
		/// </summary>
		public string Fragment { get; set; }

		/// <summary>
		/// Query parsed to name/value pairs.
		/// </summary>
		public QueryParamCollection QueryParams { get; private set; }

		/// <summary>
		/// True if URL does not start with a non-empty scheme. i.e. true for "https://www.site.com", false for "//www.site.com".
		/// </summary>
		public bool IsRelative => string.IsNullOrEmpty(Scheme);
		#endregion

		#region ctors and parsing methods
		/// <summary>
		/// Constructs a Url object from a string.
		/// </summary>
		/// <param name="baseUrl">The URL to use as a starting point (required)</param>
		/// <exception cref="ArgumentNullException"><paramref name="baseUrl"/> is <see langword="null" />.</exception>
		public Url(string baseUrl) {
			if (baseUrl == null)
				throw new ArgumentNullException(nameof(baseUrl));

			ParseInternal(new Uri(baseUrl.Trim(), UriKind.RelativeOrAbsolute));
		}

		/// <summary>
		/// Constructs a Url object from a System.Uri.
		/// </summary>
		/// <param name="uri">The System.Uri (required)</param>
		/// <exception cref="ArgumentNullException"><paramref name="uri"/> is <see langword="null" />.</exception>
		public Url(Uri uri) {
			if (uri == null)
				throw new ArgumentNullException(nameof(uri));

			ParseInternal(uri);
		}
		
		/// <summary>
		/// Parses a URL string into a Flurl.Url object.
		/// </summary>
		public static Url Parse(string url) {
			return new Url(url);
		}

		private void ParseInternal(Uri uri) {
			if (uri.IsAbsoluteUri) {
				Scheme = uri.Scheme;
				UserInfo = uri.UserInfo;
				Host = uri.Host;
				Port = uri.Authority.EndsWith($":{uri.Port}") ? uri.Port : (int?)null; // don't default Port if not included
				if (uri.AbsolutePath.Length > 1)
					AppendPathSegment(uri.AbsolutePath);
				Query = uri.Query;
				Fragment = uri.Fragment.TrimStart('#'); // quirk - formal def of fragment does not include the #

				_leadingSlash = uri.OriginalString.StartsWith(Root + "/");
				_trailingSlash = PathSegments.Any() && uri.AbsolutePath.EndsWith("/");

				// more quirk fixes
				var hasAuthority = uri.OriginalString.StartsWith($"{Scheme}://");
				if (hasAuthority && Authority.Length == 0 && PathSegments.Any()) {
					// Uri didn't parse Authority when it should have
					Host = PathSegments[0];
					PathSegments.RemoveAt(0);
				}
				else if (!hasAuthority && Authority.Length > 0) {
					// Uri parsed Authority when it should not have
					PathSegments.Insert(0, Authority);
					UserInfo = "";
					Host = "";
					Port = null;
				}
			}
			// if it's relative, System.Uri refuses to parse any of it. these hacks will force the matter
			else if (uri.OriginalString.StartsWith("//")) {
				ParseInternal(new Uri("http:" + uri.OriginalString));
				Scheme = "";
			}
			else if (uri.OriginalString.StartsWith("/")) {
				ParseInternal(new Uri("http://temp.com" + uri.OriginalString));
				Scheme = "";
				Host = "";
				_leadingSlash = true;
			}
			else {
				ParseInternal(new Uri("http://temp.com/" + uri.OriginalString));
				Scheme = "";
				Host = "";
				_leadingSlash = false;
			}
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
				let pair = p.SplitOnFirstOccurence("=")
				let name = pair[0]
				let value = (pair.Length == 1) ? null : pair[1]
				select new QueryParameter(name, value, true));

			return result;
		}
		#endregion

		#region fluent builder methods
		/// <summary>
		/// Appends a segment to the URL path, ensuring there is one and only one '/' character as a separator.
		/// </summary>
		/// <param name="segment">The segment to append</param>
		/// <param name="fullyEncode">If true, URL-encodes reserved characters such as '/', '+', and '%'. Otherwise, only encodes strictly illegal characters (including '%' but only when not followed by 2 hex characters).</param>
		/// <returns>the Url object with the segment appended</returns>
		/// <exception cref="ArgumentNullException"><paramref name="segment"/> is <see langword="null" />.</exception>
		public Url AppendPathSegment(object segment, bool fullyEncode = false) {
			if (segment == null)
				throw new ArgumentNullException(nameof(segment));

			if (fullyEncode)
				PathSegments.Add(Uri.EscapeDataString(segment.ToInvariantString()));
			else {
				var encoded = EncodeIllegalCharacters(segment.ToInvariantString()).Replace("?", "%3F");
				foreach (var s in encoded.Trim('/').Split('/'))
					PathSegments.Add(s);
				_trailingSlash = encoded.EndsWith("/");
			}

			_leadingSlash = true;
			return this;
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

			foreach (var kv in values.ToKeyValuePairs())
				QueryParams.Merge(kv.Key, kv.Value, false, nullValueHandling);

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
		public Url SetQueryParams(params string[] names) => SetQueryParams(names as IEnumerable<string>);

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
		public Url RemoveFragment() => SetFragment("");

		/// <summary>
		/// Resets the URL to its root, including the scheme, any user info, host, and port (if specified).
		/// </summary>
		/// <returns>The Url object trimmed to its root.</returns>
		public Url ResetToRoot() {
			PathSegments.Clear();
			QueryParams.Clear();
			Fragment = "";
			_leadingSlash = _trailingSlash = false;
			return this;
		}

		/// <summary>
		/// Creates a copy of this Url.
		/// </summary>
		public Url Clone() => new Url(this);
		#endregion

		#region conversion, equality, etc.
		/// <summary>
		/// Converts this Url object to its string representation.
		/// </summary>
		/// <param name="encodeSpaceAsPlus">Indicates whether to encode spaces with the "+" character instead of "%20"</param>
		/// <returns></returns>
		public string ToString(bool encodeSpaceAsPlus) {
			var sb = new StringBuilder(Root);
			sb.Append(encodeSpaceAsPlus ? Path.Replace("%20", "+") : Path);
			if (Query.Length > 0)
				sb.Append("?").Append(QueryParams.ToString(encodeSpaceAsPlus));
			if (Fragment.Length > 0)
				sb.Append("#").Append(Fragment);
			return sb.ToString();
		}

		/// <summary>
		/// Converts this Url object to its string representation.
		/// </summary>
		/// <returns></returns>
		public override string ToString() => ToString(false);

		/// <summary>
		/// Converts this Url object to System.Uri
		/// </summary>
		/// <returns>The System.Uri object</returns>
		public Uri ToUri() => new Uri(this, UriKind.RelativeOrAbsolute);

		/// <summary>
		/// Implicit conversion from Url to String.
		/// </summary>
		/// <param name="url">The Url object</param>
		/// <returns>The string</returns>
		public static implicit operator string(Url url) => url?.ToString();

		/// <summary>
		/// Implicit conversion from String to Url.
		/// </summary>
		/// <param name="url">The String representation of the URL</param>
		/// <returns>The string</returns>
		public static implicit operator Url(string url) => new Url(url);

		/// <summary>
		/// Implicit conversion from System.Uri to Flurl.Url.
		/// </summary>
		/// <returns>The string</returns>
		public static implicit operator Url(Uri uri) => new Url(uri.ToString());

		/// <summary>
		/// True if obj is an instance of Url and its string representation is equal to this instance's string representation.
		/// </summary>
		/// <param name="obj">The object to compare to this instance.</param>
		/// <returns></returns>
		public override bool Equals(object obj) => obj is Url url && this.ToString().Equals(url.ToString());

		/// <summary>
		/// Returns the hashcode for this Url.
		/// </summary>
		public override int GetHashCode() => this.ToString().GetHashCode();
		#endregion

		#region static utility methods
		/// <summary>
		/// Basically a Path.Combine for URLs. Ensures exactly one '/' separates each segment,
		/// and exactly on '&amp;' separates each query parameter.
		/// URL-encodes illegal characters but not reserved characters.
		/// </summary>
		/// <param name="parts">URL parts to combine.</param>
		public static string Combine(params string[] parts) {
			if (parts == null)
				throw new ArgumentNullException(nameof(parts));

			string result = "";
			bool inQuery = false, inFragment = false;

			string CombineEnsureSingleSeparator(string a, string b, char separator) {
				if (string.IsNullOrEmpty(a)) return b;
				if (string.IsNullOrEmpty(b)) return a;
				return a.TrimEnd(separator) + separator + b.TrimStart(separator);
			}

			foreach (var part in parts) {
				if (string.IsNullOrEmpty(part))
					continue;

				if (result.EndsWith("?") || part.StartsWith("?"))
					result = CombineEnsureSingleSeparator(result, part, '?');
				else if (result.EndsWith("#") || part.StartsWith("#"))
					result = CombineEnsureSingleSeparator(result, part, '#');
				else if (inFragment)
					result += part;
				else if (inQuery)
					result = CombineEnsureSingleSeparator(result, part, '&');
				else
					result = CombineEnsureSingleSeparator(result, part, '/');

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
		/// Decodes a URL-encoded string.
		/// </summary>
		/// <param name="s">The URL-encoded string.</param>
		/// <param name="interpretPlusAsSpace">If true, any '+' character will be decoded to a space.</param>
		/// <returns></returns>
		public static string Decode(string s, bool interpretPlusAsSpace) {
			if (string.IsNullOrEmpty(s))
				return s;

			return Uri.UnescapeDataString(interpretPlusAsSpace ? s.Replace("+", " ") : s);
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
		/// Checks if a string is a well-formed absolute URL.
		/// </summary>
		/// <param name="url">The string to check</param>
		/// <returns>true if s is a well-formed absolute URL</returns>
		public static bool IsValid(string url) => url != null && Uri.IsWellFormedUriString(url, UriKind.Absolute);
		#endregion
	}
}