using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Flurl.Http
{
	/// <summary>
	/// Extension methods off HttpRequestMessage.
	/// </summary>
	public static class HttpRequestMessageExtensions
    {
	    /// <summary>
	    /// Set a header on this HttpRequestMessage (default), or its Content property if it's a known content-level header.
	    /// No validation. Overwrites any existing value(s) for the header. 
	    /// </summary>
	    /// <param name="request">The HttpRequestMessage.</param>
	    /// <param name="name">The header name.</param>
	    /// <param name="value">The header value.</param>
	    /// <param name="createContentIfNecessary">If it's a content-level header and there is no content, this determines whether to create an empty HttpContent or just ignore the header.</param>
	    public static void SetHeader(this HttpRequestMessage request, string name, object value, bool createContentIfNecessary = true) {
		    new HttpMessage(request).SetHeader(name, value, createContentIfNecessary);
	    }

	    /// <summary>
	    /// Gets the value of a header on this HttpRequestMessage (default), or its Content property.
	    /// Returns null if the header doesn't exist.
	    /// </summary>
	    /// <param name="request">The HttpRequestMessage.</param>
	    /// <param name="name">The header name.</param>
	    /// <returns>The header value.</returns>
	    public static string GetHeaderValue(this HttpRequestMessage request, string name) {
		    return new HttpMessage(request).GetHeaderValue(name);
	    }

		/// <summary>
		/// Associate an HttpCall object with this request
		/// </summary>
		internal static void SetHttpCall(this HttpRequestMessage request, HttpCall call) {
		    if (request?.Properties != null)
			    request.Properties["FlurlHttpCall"] = call;
	    }

		/// <summary>
		/// Get the HttpCall associated with this request, if any.
		/// </summary>
		internal static HttpCall GetHttpCall(this HttpRequestMessage request) {
		    if (request?.Properties != null && request.Properties.TryGetValue("FlurlHttpCall", out var obj) && obj is HttpCall call)
			    return call;
		    return null;
	    }
	}
}
