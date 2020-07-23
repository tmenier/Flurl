using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Flurl.Http
{
	/// <summary>
	/// A context where multiple requests use a common CookieJar. Created using FlurlClient.StartCookieSession.
	/// </summary>
	public class CookieSession : IDisposable
	{
		private readonly IFlurlClient _client;

		internal CookieSession(IFlurlClient client) {
			_client = client;
		}

		/// <summary>
		/// The CookieJar used by all requests sent with this CookieSession.
		/// </summary>
		public CookieJar Cookies { get; } = new CookieJar();

		/// <summary>
		/// Creates a new IFlurlRequest with this session's CookieJar that can be further built and sent fluently.
		/// </summary>
		/// <param name="urlSegments">The URL or URL segments for the request.</param>
		public IFlurlRequest Request(params object[] urlSegments) => _client.Request(urlSegments).WithCookies(Cookies);

		/// <summary>
		/// Not necessary to call. IDisposable is implemented mainly for the syntactic sugar of using statements.
		/// </summary>
		public void Dispose() { }
	}
}
