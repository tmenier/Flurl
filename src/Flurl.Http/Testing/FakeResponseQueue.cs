using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Flurl.Http.Testing
{
	/// <summary>
	/// A queue of fake responses used by HttpTest for a specific URL pattern / verb.
	/// </summary>
	public class FakeResponseQueue
	{
		/// <summary>
		/// The URL pattern to return these fake responses from. Use * as wildcard.
		/// </summary>
		public string UrlPattern { get; set; }

		/// <summary>
		/// The HTTP verb to return these fake responses for. If null, applies to any verb.
		/// </summary>
		public HttpMethod Verb { get; set; }

		/// <summary>
		/// The fake responses to return.
		/// </summary>
		public ConcurrentQueue<HttpResponseMessage> Queue { get; } = new ConcurrentQueue<HttpResponseMessage>();
	}
}
