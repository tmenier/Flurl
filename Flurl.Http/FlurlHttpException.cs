using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Flurl.Http
{
	/// <summary>
	/// An exception that is thrown when an HTTP call made by Flurl.Http fails, including when the response
	/// indicates an unsuccessful HTTP status code.
	/// </summary>
	public class FlurlHttpException : HttpRequestException
	{
		/// <summary>
		/// An object containing details about the failed HTTP call
		/// </summary>
		public HttpCall Call { get; private set; }

		public FlurlHttpException(HttpCall call, string message, Exception inner) : base(message, inner) {
			this.Call = call;
		}

		public FlurlHttpException(HttpCall call, Exception inner) : this(call, BuildMessage(call, inner), inner) { }

		public FlurlHttpException(HttpCall call) : this(call, BuildMessage(call, null), null) { }

		private static string BuildMessage(HttpCall call, Exception inner) {
			if (call.Response != null && !call.Response.IsSuccessStatusCode) {
				return string.Format("Request to {0} failed with status code {1} ({2}).",
					call.Request.RequestUri.AbsoluteUri,
					(int) call.Response.StatusCode,
					call.Response.ReasonPhrase);
			}
			else if (inner != null) {
				return string.Format("Request to {0} failed. {1}",
					call.Request.RequestUri.AbsoluteUri, inner.Message);
			}

			// in theory we should never get here.
			return string.Format("Request to {0} failed.", call.Request.RequestUri.AbsoluteUri);
		}
	}

	/// <summary>
	/// An exception that is thrown when an HTTP call made by Flurl.Http times out.
	/// </summary>
	public class FlurlHttpTimeoutException : FlurlHttpException
	{
		public FlurlHttpTimeoutException(HttpCall call, Exception inner) : base(call, BuildMessage(call), inner) { }

		private static string BuildMessage(HttpCall call) {
			return string.Format("Request to {0} timed out.", call);
		}
	}
}
