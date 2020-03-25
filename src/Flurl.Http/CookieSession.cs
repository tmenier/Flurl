using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Flurl.Http
{
	/// <summary>
	/// A context where multiple requests and responses share the same cookie collection. Created using FlurlClient.StartCookieSession.
	/// </summary>
	public class CookieSession : IDisposable
	{
		private readonly IFlurlClient _client;

		internal CookieSession(IFlurlClient client) {
			_client = client;
		}

		/// <summary>
		/// A collection of cookies sent by all requests and received by all responses within this session.
		/// </summary>
		public IDictionary<string, Cookie> Cookies { get; } = new Dictionary<string, Cookie>();

		/// <summary>
		/// Creates a new IFlurlRequest with this session's cookies that can be further built and sent fluently.
		/// </summary>
		/// <param name="urlSegments">The URL or URL segments for the request.</param>
		public IFlurlRequest Request(params object[] urlSegments) => _client.Request(urlSegments).WithCookies(Cookies);

		/// <summary>
		/// Not necessary to call. IDisposable is implemented mainly for the syntactic sugar of using statements.
		/// </summary>
		public void Dispose() => Cookies.Clear();
	}
}
