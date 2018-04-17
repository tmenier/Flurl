using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Flurl.Http
{
    internal static class HttpRequestMessageExtensions
    {
		/// <summary>
		/// Associate an HttpCall object with this request
		/// </summary>
	    internal static void SetHttpCall(this HttpRequestMessage request, HttpCall call) {
		    if (request?.Properties != null)
			    request.Properties["FlurlHttpCall"] = call;
	    }

		/// <summary>
		/// Get the HttpCall assocaited with this request, if any.
		/// </summary>
		internal static HttpCall GetHttpCall(this HttpRequestMessage request) {
		    if (request?.Properties != null && request.Properties.TryGetValue("FlurlHttpCall", out var obj) && obj is HttpCall)
			    return (HttpCall)obj;
		    return null;
	    }
	}
}
