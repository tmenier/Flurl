using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using Flurl.Http.Testing;

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
		/// Gets or sets a factory used to create HttpClient object used in Flurl HTTP calls. Default value
		/// is an instance of DefaultHttpClientFactory. Custom factory implementations should generally
		/// inherit from DefaultHttpClientFactory, call base.CreateClient, and manipulate the returned HttpClient,
		/// otherwise functionality such as callbacks and most testing features will be lost.
		/// </summary>
		public static IHttpClientFactory HttpClientFactory { get; set; }

		/// <summary>
		/// Gets or sets a callback function that is fired immediately before every HTTP request is sent.
		/// </summary>
		public static Action<HttpRequestMessage> BeforeCall { get; set; }

		/// <summary>
		/// Register a callback to occur immediately after every HTTP response is received.
		/// </summary>
		public static Action<HttpRequestMessage, HttpResponseMessage> AfterCall { get; set; }

		static FlurlHttp() {
			ResetDefaults();
		}

		/// <summary>
		/// Sets all FlurlHttp static configuration options back to their default values.
		/// </summary>
		public static void ResetDefaults() {
			DefaultTimeout = new HttpClient().Timeout;
			HttpClientFactory = new DefaultHttpClientFactory();
			BeforeCall = req => { };
			AfterCall = (req, resp) => { };			
		}

		private static readonly Lazy<HttpTester> _tester = new Lazy<HttpTester>();

		/// <summary>
		/// A container for objects and methods useful in automated testing scenarios.
		/// </summary>
		public static HttpTester Testing {
			get { return _tester.Value; }
		}
	}
}
