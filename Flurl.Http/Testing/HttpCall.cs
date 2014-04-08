using System.Net.Http;

namespace Flurl.Http.Testing
{
	public class CallLogEntry
	{
		public HttpRequestMessage Request { get; set; }
		public string RequestBody { get; set; }
		public HttpResponseMessage Response { get; set; }
		public string ResponseBody { get; set; }
	}
}
