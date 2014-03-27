using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace Flurl.Http
{
	public class FlurlHttpException : HttpRequestException
	{
		public HttpRequestMessage Request { get; private set; }
		public HttpResponseMessage Response { get; private set; }

		public FlurlHttpException(HttpRequestMessage request, HttpResponseMessage response, Exception inner)
			: base(BuildMessage(request, response, inner), inner)
		{
			this.Request = request;
			this.Response = response;
		}

		public FlurlHttpException(HttpRequestMessage request, HttpResponseMessage response)
			: this(request, response, null) { }

		private static string BuildMessage(HttpRequestMessage request, HttpResponseMessage response, Exception inner) {
			if (!response.IsSuccessStatusCode)
				return string.Format("Request to {0} failed with status {1} ({2}).", request.RequestUri.AbsoluteUri, (int)response.StatusCode, response.ReasonPhrase);
			else if (inner != null)
				return string.Format("Request to {0} failed. {1}", request.RequestUri.AbsoluteUri, inner.Message);

			// in theory we should never get here.
			return string.Format("Request to {0} failed.", request.RequestUri.AbsoluteUri);
		}
	}
}
