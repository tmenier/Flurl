using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Flurl.Http.Configuration;
using Flurl.Http.Content;
using Flurl.Util;

namespace Flurl.Http.Testing
{
	/// <summary>
	/// An object whose existence puts Flurl.Http into test mode where actual HTTP calls are faked. Provides a response
	/// queue, call log, and assertion helpers for use in Arrange/Act/Assert style tests.
	/// </summary>
	public class HttpTest : IDisposable
	{
	    /// <summary>
	    /// Initializes a new instance of the <see cref="HttpTest"/> class.
	    /// </summary>
	    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
	    public HttpTest() {
		    Settings = new GlobalFlurlHttpSettings {
			    HttpClientFactory = new TestHttpClientFactory(),
				FlurlClientFactory = new TestFlurlClientFactory()
		    };
			ResponseQueue = new Queue<HttpResponseMessage>();
			CallLog = new List<HttpCall>();
		    SetCurrentTest(this);
	    }

		/// <summary>
		/// Gets or sets the FlurlHttpSettings object used by this test.
		/// </summary>
		public GlobalFlurlHttpSettings Settings { get; set; }

		/// <summary>
		/// Gets the current HttpTest from the logical (async) call context
		/// </summary>
		public static HttpTest Current => GetCurrentTest();

		/// <summary>
		/// Queue of HttpResponseMessages to be returned in place of real responses during testing.
		/// </summary>
		public Queue<HttpResponseMessage> ResponseQueue { get; set; }

		/// <summary>
		/// List of all (fake) HTTP calls made since this HttpTest was created.
		/// </summary>
		public List<HttpCall> CallLog { get; }

		/// <summary>
		/// Adds an HttpResponseMessage to the response queue.
		/// </summary>
		/// <param name="body">The simulated response body string.</param>
		/// <param name="status">The simulated HTTP status. Default is 200.</param>
		/// <param name="headers">The simulated response headers (optional).</param>
		/// <param name="cookies">The simulated response cookies (optional).</param>
		/// <returns>The current HttpTest object (so more responses can be chained).</returns>
		public HttpTest RespondWith(string body, int status = 200, object headers = null, object cookies = null) {
			return RespondWith(new StringContent(body), status, headers, cookies);
		}

		/// <summary>
		/// Adds an HttpResponseMessage to the response queue with the given data serialized to JSON as the content body.
		/// </summary>
		/// <param name="body">The object to be JSON-serialized and used as the simulated response body.</param>
		/// <param name="status">The simulated HTTP status. Default is 200.</param>
		/// <param name="headers">The simulated response headers (optional).</param>
		/// <param name="cookies">The simulated response cookies (optional).</param>
		/// <returns>The current HttpTest object (so more responses can be chained).</returns>
		public HttpTest RespondWithJson(object body, int status = 200, object headers = null, object cookies = null) {
			var content = new CapturedJsonContent(FlurlHttp.GlobalSettings.JsonSerializer.Serialize(body));
			return RespondWith(content, status, headers, cookies);
		}

		/// <summary>
		/// Adds an HttpResponseMessage to the response queue.
		/// </summary>
		/// <param name="content">The simulated response body content (optional).</param>
		/// <param name="status">The simulated HTTP status. Default is 200.</param>
		/// <param name="headers">The simulated response headers (optional).</param>
		/// <param name="cookies">The simulated response cookies (optional).</param>
		/// <returns>The current HttpTest object (so more responses can be chained).</returns>
		public HttpTest RespondWith(HttpContent content = null, int status = 200, object headers = null, object cookies = null) {
			var response = new HttpResponseMessage {
				StatusCode = (HttpStatusCode)status,
				Content = content
			};
			if (headers != null) {
				foreach (var kv in headers.ToKeyValuePairs())
					response.Headers.Add(kv.Key, kv.Value.ToInvariantString());
			}
			if (cookies != null) {
				foreach (var kv in cookies.ToKeyValuePairs()) {
					var value = new Cookie(kv.Key, kv.Value.ToInvariantString()).ToString();
					response.Headers.Add("Set-Cookie", value);
				}
			}
			ResponseQueue.Enqueue(response);
			return this;
		}

		/// <summary>
		/// Adds a simulated timeout response to the response queue.
		/// </summary>
		public HttpTest SimulateTimeout() {
			ResponseQueue.Enqueue(new TimeoutResponseMessage());
			return this;
		}

		internal HttpResponseMessage GetNextResponse() {
			return ResponseQueue.Any() ? ResponseQueue.Dequeue() : new HttpResponseMessage {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent("")
			};
		}

		/// <summary>
		/// Asserts whether matching URL was called, throwing HttpCallAssertException if it wasn't.
		/// </summary>
		/// <param name="urlPattern">URL that should have been called. Can include * wildcard character.</param>
		public HttpCallAssertion ShouldHaveCalled(string urlPattern) {
			return new HttpCallAssertion(CallLog).WithUrlPattern(urlPattern);
		}

		/// <summary>
		/// Asserts whether matching URL was NOT called, throwing HttpCallAssertException if it was.
		/// </summary>
		/// <param name="urlPattern">URL that should not have been called. Can include * wildcard character.</param>
		public void ShouldNotHaveCalled(string urlPattern) {
			new HttpCallAssertion(CallLog, true).WithUrlPattern(urlPattern);
		}

		/// <summary>
		/// Asserts whether any HTTP call was made, throwing HttpCallAssertException if none were.
		/// </summary>
		public HttpCallAssertion ShouldHaveMadeACall() {
			return new HttpCallAssertion(CallLog).WithUrlPattern("*");
		}

		/// <summary>
		/// Asserts whether no HTTP calls were made, throwing HttpCallAssertException if any were.
		/// </summary>
		public void ShouldNotHaveMadeACall() {
			new HttpCallAssertion(CallLog, true).WithUrlPattern("*");
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		public void Dispose() {
			SetCurrentTest(null);
		}

#if NET45
		private static void SetCurrentTest(HttpTest test) => System.Runtime.Remoting.Messaging.CallContext.LogicalSetData("FlurlHttpTest", test);
		private static HttpTest GetCurrentTest() => System.Runtime.Remoting.Messaging.CallContext.LogicalGetData("FlurlHttpTest") as HttpTest;
#elif NETSTANDARD1_3
		private static System.Threading.AsyncLocal<HttpTest> _test = new System.Threading.AsyncLocal<HttpTest>();
		private static void SetCurrentTest(HttpTest test) => _test.Value = test;
		private static HttpTest GetCurrentTest() => _test.Value;
#elif NETSTANDARD1_1
		private static HttpTest _test;
		private static void SetCurrentTest(HttpTest test) => _test = test;
		private static HttpTest GetCurrentTest() => _test;
#endif
	}
}