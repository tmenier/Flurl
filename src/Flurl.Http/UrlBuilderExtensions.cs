using System;
using System.Collections.Generic;

namespace Flurl.Http
{
	/// <summary>
	/// URL builder extension methods on FlurlRequest
	/// </summary>
	public static class UrlBuilderExtensions
	{
		/// <summary>
		/// Appends a segment to the URL path, ensuring there is one and only one '/' character as a seperator.
		/// </summary>
		/// <param name="request">The IFlurlRequest associated with the URL</param>
		/// <param name="segment">The segment to append</param>
		/// <returns>This IFlurlRequest</returns>
		/// <exception cref="ArgumentNullException"><paramref name="segment"/> is <see langword="null" />.</exception>
		public static IFlurlRequest AppendPathSegment(this IFlurlRequest request, object segment) {
			request.Url.AppendPathSegment(segment);
			return request;
		}

		/// <summary>
		/// Appends multiple segments to the URL path, ensuring there is one and only one '/' character as a seperator.
		/// </summary>
		/// <param name="request">The IFlurlRequest associated with the URL</param>
		/// <param name="segments">The segments to append</param>
		/// <returns>This IFlurlRequest</returns>
		public static IFlurlRequest AppendPathSegments(this IFlurlRequest request, params object[] segments) {
			request.Url.AppendPathSegments(segments);
			return request;
		}

		/// <summary>
		/// Appends multiple segments to the URL path, ensuring there is one and only one '/' character as a seperator.
		/// </summary>
		/// <param name="request">The IFlurlRequest associated with the URL</param>
		/// <param name="segments">The segments to append</param>
		/// <returns>This IFlurlRequest</returns>
		public static IFlurlRequest AppendPathSegments(this IFlurlRequest request, IEnumerable<object> segments) {
			request.Url.AppendPathSegments(segments);
			return request;
		}

		/// <summary>
		/// Adds a parameter to the URL query, overwriting the value if name exists.
		/// </summary>
		/// <param name="request">The IFlurlRequest associated with the URL</param>
		/// <param name="name">Name of query parameter</param>
		/// <param name="value">Value of query parameter</param>
		/// <param name="nullValueHandling">Indicates how to handle null values. Defaults to Remove (any existing)</param>
		/// <returns>This IFlurlRequest</returns>
		public static IFlurlRequest SetQueryParam(this IFlurlRequest request, string name, object value, NullValueHandling nullValueHandling = NullValueHandling.Remove) {
			request.Url.SetQueryParam(name, value, nullValueHandling);
			return request;
		}

		/// <summary>
		/// Adds a parameter to the URL query, overwriting the value if name exists.
		/// </summary>
		/// <param name="request">The IFlurlRequest associated with the URL</param>
		/// <param name="name">Name of query parameter</param>
		/// <param name="value">Value of query parameter</param>
		/// <param name="isEncoded">Set to true to indicate the value is already URL-encoded</param>
		/// <param name="nullValueHandling">Indicates how to handle null values. Defaults to Remove (any existing)</param>
		/// <returns>This IFlurlRequest</returns>
		/// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null" />.</exception>
		public static IFlurlRequest SetQueryParam(this IFlurlRequest request, string name, string value, bool isEncoded = false, NullValueHandling nullValueHandling = NullValueHandling.Remove) {
			request.Url.SetQueryParam(name, value, isEncoded, nullValueHandling);
			return request;
		}

		/// <summary>
		/// Adds a parameter without a value to the URL query, removing any existing value.
		/// </summary>
		/// <param name="request">The IFlurlRequest associated with the URL</param>
		/// <param name="name">Name of query parameter</param>
		/// <returns>This IFlurlRequest</returns>
		public static IFlurlRequest SetQueryParam(this IFlurlRequest request, string name) {
			request.Url.SetQueryParam(name);
			return request;
		}

		/// <summary>
		/// Parses values (usually an anonymous object or dictionary) into name/value pairs and adds them to the URL query, overwriting any that already exist.
		/// </summary>
		/// <param name="request">The IFlurlRequest associated with the URL</param>
		/// <param name="values">Typically an anonymous object, ie: new { x = 1, y = 2 }</param>
		/// <param name="nullValueHandling">Indicates how to handle null values. Defaults to Remove (any existing)</param>
		/// <returns>This IFlurlRequest</returns>
		public static IFlurlRequest SetQueryParams(this IFlurlRequest request, object values, NullValueHandling nullValueHandling = NullValueHandling.Remove) {
			request.Url.SetQueryParams(values, nullValueHandling);
			return request;
		}

		/// <summary>
		/// Adds multiple parameters without values to the URL query.
		/// </summary>
		/// <param name="request">The IFlurlRequest associated with the URL</param>
		/// <param name="names">Names of query parameters.</param>
		/// <returns>This IFlurlRequest</returns>
		public static IFlurlRequest SetQueryParams(this IFlurlRequest request, IEnumerable<string> names) {
			request.Url.SetQueryParams(names);
			return request;
		}

		/// <summary>
		/// Adds multiple parameters without values to the URL query.
		/// </summary>
		/// <param name="request">The IFlurlRequest associated with the URL</param>
		/// <param name="names">Names of query parameters</param>
		/// <returns>This IFlurlRequest</returns>
		public static IFlurlRequest SetQueryParams(this IFlurlRequest request, params string[] names) {
			request.Url.SetQueryParams(names as IEnumerable<string>);
			return request;
		}

		/// <summary>
		/// Removes a name/value pair from the URL query by name.
		/// </summary>
		/// <param name="request">The IFlurlRequest associated with the URL</param>
		/// <param name="name">Query string parameter name to remove</param>
		/// <returns>This IFlurlRequest</returns>
		public static IFlurlRequest RemoveQueryParam(this IFlurlRequest request, string name) {
			request.Url.RemoveQueryParam(name);
			return request;
		}

		/// <summary>
		/// Removes multiple name/value pairs from the URL query by name.
		/// </summary>
		/// <param name="request">The IFlurlRequest associated with the URL</param>
		/// <param name="names">Query string parameter names to remove</param>
		/// <returns>This IFlurlRequest</returns>
		public static IFlurlRequest RemoveQueryParams(this IFlurlRequest request, params string[] names) {
			request.Url.RemoveQueryParams(names);
			return request;
		}

		/// <summary>
		/// Removes multiple name/value pairs from the URL query by name.
		/// </summary>
		/// <param name="request">The IFlurlRequest associated with the URL</param>
		/// <param name="names">Query string parameter names to remove</param>
		/// <returns>This IFlurlRequest</returns>
		public static IFlurlRequest RemoveQueryParams(this IFlurlRequest request, IEnumerable<string> names) {
			request.Url.RemoveQueryParams(names);
			return request;
		}

		/// <summary>
		/// Set the URL fragment fluently.
		/// </summary>
		/// <param name="request">The IFlurlRequest associated with the URL</param>
		/// <param name="fragment">The part of the URL afer #</param>
		/// <returns>This IFlurlRequest</returns>
		public static IFlurlRequest SetFragment(this IFlurlRequest request, string fragment) {
			request.Url.SetFragment(fragment);
			return request;
		}

		/// <summary>
		/// Removes the URL fragment including the #.
		/// </summary>
		/// <param name="request">The IFlurlRequest associated with the URL</param>
		/// <returns>This IFlurlRequest</returns>
		public static IFlurlRequest RemoveFragment(this IFlurlRequest request) {
			request.Url.RemoveFragment();
			return request;
		}
	}
}
