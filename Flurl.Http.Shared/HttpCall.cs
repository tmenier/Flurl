using System;
using System.Net.Http;

namespace Flurl.Http
{
	public class HttpCall
	{
		public HttpRequestMessage Request { get; set; }
		public string RequestBody { get; set; }
		public HttpResponseMessage Response { get; set; }
		public Exception Exception { get; set; }
		public bool ExceptionHandled { get; set; }
		public DateTime StartedUtc { get; set; }
		public DateTime? EndedUtc { get; set; }

		public TimeSpan? Duration {
			get {
				return EndedUtc.HasValue ? EndedUtc - StartedUtc : null;
			}
		}
	}
}
