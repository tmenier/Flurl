using System;
using System.Net;
using System.Net.Http;
using Flurl.Http.Content;

namespace Flurl.Http
{
	/// <summary>
	/// Encapsulates request, response, and other details associated with an HTTP call. Useful for diagnostics and available in
	/// global event handlers and FlurlHttpException.Call.
	/// </summary>
	public class HttpCall
	{
		internal HttpCall(IFlurlRequest flurlRequest, HttpRequestMessage request) {
			FlurlRequest = flurlRequest;
			Request = request;
			if (request?.Properties != null)
				request.Properties["FlurlHttpCall"] = this;
		}

		internal static HttpCall Get(HttpRequestMessage request) {
			object obj;
			if (request?.Properties != null && request.Properties.TryGetValue("FlurlHttpCall", out obj) && obj is HttpCall)
				return (HttpCall)obj;
			return null;
		}

		/// <summary>
		/// The IFlurlRequest associated with this call.
		/// </summary>
		public IFlurlRequest FlurlRequest { get; }

		/// <summary>
		/// The HttpRequestMessage associated with this call.
		/// </summary>
		public HttpRequestMessage Request { get; }

		/// <summary>
		/// Captured request body. Available ONLY if Request.Content is a Flurl.Http.Content.CapturedStringContent.
		/// </summary>
		public string RequestBody => (Request.Content as CapturedStringContent)?.Content;

		/// <summary>
		/// HttpResponseMessage associated with the call if the call completed, otherwise null.
		/// </summary>
		public HttpResponseMessage Response { get; internal set; }

		/// <summary>
		/// Exception that occurred while sending the HttpRequestMessage.
		/// </summary>
		public Exception Exception { get; internal set; }
	
		/// <summary>
		/// User code should set this to true inside global event handlers (OnError, etc) to indicate
		/// that the exception was handled and should not be propagated further.
		/// </summary>
		public bool ExceptionHandled { get; set; }

		/// <summary>
		/// DateTime the moment the request was sent.
		/// </summary>
		public DateTime StartedUtc { get; internal set; }

		/// <summary>
		/// DateTime the moment a response was received.
		/// </summary>
		public DateTime? EndedUtc { get; internal set; }

		/// <summary>
		/// Total duration of the call if it completed, otherwise null.
		/// </summary>
		public TimeSpan? Duration => EndedUtc - StartedUtc;

		/// <summary>
		/// True if a response was received, regardless of whether it is an error status.
		/// </summary>
		public bool Completed => Response != null;

		/// <summary>
		/// True if a response with a successful HTTP status was received.
		/// </summary>
		public bool Succeeded => Completed && 
			(Response.IsSuccessStatusCode || HttpStatusRangeParser.IsMatch(FlurlRequest.Settings.AllowedHttpStatusRange, Response.StatusCode));

		/// <summary>
		/// HttpStatusCode of the response if the call completed, otherwise null.
		/// </summary>
		public HttpStatusCode? HttpStatus => Completed ? (HttpStatusCode?)Response.StatusCode : null;

		/// <summary>
		/// Body of the HTTP response if unsuccessful, otherwise null. (Successful responses are not captured as strings, mainly for performance reasons.)
		/// </summary>
		public string ErrorResponseBody { get; internal set; }

		/// <summary>
		/// Returns the verb and absolute URI associated with this call.
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			return $"{Request.Method:U} {FlurlRequest.Url}";
		}
	}
}
