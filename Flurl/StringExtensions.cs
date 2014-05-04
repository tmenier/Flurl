using System;
using System.Collections.Generic;

namespace Flurl
{
	/// <summary>
	/// A set of string extension methods for working with Flurl URLs
	/// </summary>
	public static class StringExtensions
	{
		/// <summary>
		/// Checks if a string is a well-formed URL.
		/// </summary>
		/// <param name="s">The string to check</param>
		/// <returns>true if s is a well-formed URL</returns>
		public static bool IsUrl(this string s) {
			return s != null && Uri.IsWellFormedUriString(s, UriKind.Absolute);
		}

		/// <summary>
		/// Converts string to a Url object and appends a segment to the URL path, 
		/// ensuring there is one and only one '/' character as a seperator.
		/// </summary>
		/// <param name="segment">The segment to append</param>
		/// <returns>the resulting Url object</returns>
		public static Url AppendPathSegment(this string url, string segment) {
			return new Url(url).AppendPathSegment(segment);
		}

		/// <summary>
		/// Appends multiple segments to the URL path, ensuring there is one and only one '/' character as a seperator.
		/// </summary>
		/// <param name="segments">The segments to append</param>
		/// <returns>the Url object with the segments appended</returns>
		public static Url AppendPathSegments(this string url, params string[] segments) {
			return new Url(url).AppendPathSegments(segments);
		}

		/// <summary>
		/// Appends multiple segments to the URL path, ensuring there is one and only one '/' character as a seperator.
		/// </summary>
		/// <param name="segments">The segments to append</param>
		/// <returns>the Url object with the segments appended</returns>
		public static Url AppendPathSegments(this string url, IEnumerable<string> segments) {
			return new Url(url).AppendPathSegments(segments);
		}

		/// <summary>
		/// Converts string to a Url object and adds a parameter to the query string, overwriting the value if name exists.
		/// </summary>
		/// <param name="name">name of query string parameter</param>
		/// <param name="value">value of query string parameter</param>
		/// <returns>The Url obect with the query string parameter added</returns>
		public static Url SetQueryParam(this string url, string name, object value) {
			return new Url(url).SetQueryParam(name, value);
		}

		/// <summary>
		/// Converts string to a Url object, parses values object into name/value pairs, and adds them to the query string,
		/// overwriting any that already exist.
		/// </summary>
		/// <param name="values">Typically an anonymous object, ie: new { x = 1, y = 2 }</param>
		/// <returns>The Url object with the query string parameters added</returns>
		public static Url SetQueryParams(this string url, object values) {
			return new Url(url).SetQueryParams(values);
		}

		/// <summary>
		/// Converts string to a Url object and removes a name/value pair from the query string by name.
		/// </summary>
		/// <param name="name">Query string parameter name to remove</param>
		/// <returns>The Url object with the query string parameter removed</returns>
		public static Url RemoveQueryParam(this string url, string name) {
			return new Url(url).RemoveQueryParam(name);
		}

		/// <summary>
		/// Converts string to a Url object and removes multiple name/value pairs from the query string by name.
		/// </summary>
		/// <param name="names">Query string parameter names to remove</param>
		/// <returns>The Url object with the query string parameters removed</returns>
		public static Url RemoveQueryParams(this string url, params string[] names) {
			return new Url(url).RemoveQueryParams(names);
		}

		/// <summary>
		///  Converts string to a Url object and removes multiple name/value pairs from the query string by name.
		/// </summary>
		/// <param name="names">Query string parameter names to remove</param>
		/// <returns>The Url object with the query string parameters removed</returns>
		public static Url RemoveQueryParams(this string url, IEnumerable<string> names) {
			return new Url(url).RemoveQueryParams(names);
		}
	}
}
