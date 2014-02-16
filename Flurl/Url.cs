using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Web;

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
		public NameValueCollection QueryParams { get; private set; }

		/// <summary>
		/// Constructs a Url object from a string.
		/// </summary>
		/// <param name="baseUrl">The URL to use as a starting point (required)</param>
		public Url(string baseUrl) {
			if (baseUrl == null)
				throw new ArgumentNullException("baseUrl");

			var parts = baseUrl.Split('?');
			Path = parts[0];
			// nice tip from John Bledsoe: http://stackoverflow.com/a/1877016/62600
			QueryParams = HttpUtility.ParseQueryString(parts.Length > 1 ? parts[1] : "");
		}

		/// <summary>
		/// Basically a Path.Combine for URLs. Ensures exactly one '/' character is used to seperate each segment.
		/// URL-encodes illegal characters but not reserved characters.
		/// </summary>
		/// <param name="baseUrl">The URL to use as a starting point (required). NOT URL-encoded.</param>
		/// <param name="segments">Paths to combine.</param>
		/// <returns></returns>
		public static string Combine(string baseUrl, params string[] segments) {
			if (baseUrl == null)
				throw new ArgumentNullException("baseUrl");

			return new Url(baseUrl).AppendPathSegments(segments).ToString();
		}

		/// <summary>
		/// Encodes characters that are strictly illegal in a URL. Does not encode reserved characters, i.e. '/', '+', etc.
		/// </summary>
		/// <param name="url"></param>
		/// <returns></returns>
		public static string Cleanse(string url) {
			return Uri.EscapeUriString(url);
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
			Path += HttpUtility.UrlPathEncode(segment.TrimStart('/').TrimEnd('/'));
			return this;
		}

		/// <summary>
		/// Appends multiple segments to the URL path, ensuring there is one and only one '/' character as a seperator.
		/// </summary>
		/// <param name="segments">The segments to append</param>
		/// <returns>the Url object with the segments appended</returns>
		public Url AppendPathSegments(params string[] segments) {
			Array.ForEach(segments, s => AppendPathSegment(s));
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
		public Url AddQueryParam(string name, object value) {
			QueryParams[name] = (value == null) ? null : value.ToString();
			return this;
		}

		/// <summary>
		/// Parses object into name/value pairs and adds them to the query string, overwriting any that already exist.
		/// </summary>
		/// <param name="values">Typically an anonymous object, ie: new { x = 1, y = 2 }</param>
		/// <returns>The Url object with the query string parameters added</returns>
		public Url AddQueryParams(object values) {
			if (values == null)
				return this;

			foreach (PropertyDescriptor prop in TypeDescriptor.GetProperties(values))
				AddQueryParam(prop.Name, prop.GetValue(values));

			return this;
		}

		/// <summary>
		/// Adds key/value pairs and to the query string, overwriting any that already exist.
		/// </summary>
		/// <param name="values">Dictionary of key/value pairs to add to the query string</param>
		/// <returns>The Url object with the query string parameters added</returns>
		public Url AddQueryParams(IDictionary values) {
			if (values == null)
				return this;

			foreach (var key in values.Keys)
				AddQueryParam(key.ToString(), values[key]);

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
			Array.ForEach(names, QueryParams.Remove);
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
		/// Converts this Url object to its string representation.
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			var url = Path;
			if (QueryParams.Count > 0)
				url += "?" + QueryParams;
			return url;
		}

		/// <summary>
		/// Implicit conversion to string.
		/// </summary>
		/// <param name="url">the Url object</param>
		/// <returns>The string</returns>
		public static implicit operator string(Url url) {
			return url.ToString();
		}
	}
}
