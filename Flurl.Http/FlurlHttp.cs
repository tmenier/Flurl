using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Flurl.Http
{
	/// <summary>
	/// A static configuration object affecting global behavior Flurl HTTP methods
	/// </summary>
	public static class FlurlHttp
	{
		/// <summary>
		/// Get or set whether to actually execute HTTP methods
		/// </summary>
		public static bool TestMode { get; set; }

		/// <summary>
		/// Gets or sets the default timeout for every HTTP request.
		/// </summary>
		public static TimeSpan DefaultTimeout { get; set; }

		/// <summary>
		/// Register a callback to occur immediately before every HTTP request is sent.
		/// </summary>
		public static Action<HttpRequestMessage> BeforeCall { get; set; }

		/// <summary>
		/// Register a callback to occur immediately after every HTTP response is received.
		/// </summary>
		public static Action<HttpRequestMessage, HttpResponseMessage> AfterCall { get; set; }

		static FlurlHttp() {
			DefaultTimeout = new HttpClient().Timeout;
			BeforeCall = req => { };
			AfterCall = (req, resp) => { };
		}

		public static class Testing
		{
			/// <summary>
			/// Gets the last-sent HttpRequestMessage ONLY when TestMode = true
			/// </summary>
			public static HttpRequestMessage LastRequest { get; internal set; }

			/// <summary>
			/// Gets the string content of the last-sent HTTP request ONLY when TestMode = true
			/// </summary>
			public static string LastRequestBody { get; internal set; }
		}
	}
}
