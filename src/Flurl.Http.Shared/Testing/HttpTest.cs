using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Flurl.Http.Content;

namespace Flurl.Http.Testing
{
	/// <summary>
	/// An object whose existence puts Flurl.Http into test mode where actual HTTP calls are faked. Provides a response
	/// queue, call log, and assertion helpers for use in Arrange/Act/Assert style tests.
	/// </summary>
	public class HttpTest : IDisposable
	{
		private static readonly HttpResponseMessage _emptyResponse = new HttpResponseMessage {
			StatusCode = HttpStatusCode.OK,
			Content = new StringContent("")
		};

	    /// <summary>
	    /// Initializes a new instance of the <see cref="HttpTest"/> class.
	    /// </summary>
	    /// <exception cref="Exception">A delegate callback throws an exception.</exception>
	    public HttpTest() {
			FlurlHttp.Configure(settings => {
				settings.HttpClientFactory = new TestHttpClientFactory(this);
				settings.AfterCall = call => CallLog.Add(call);
			});
			ResponseQueue = new Queue<HttpResponseMessage>();
			CallLog = new List<HttpCall>();
		}

		/// <summary>
		/// Adds an HttpResponseMessage to the response queue with the given HTTP status code and content body.
		/// </summary>
		public HttpTest RespondWith(int status, string body) {
			ResponseQueue.Enqueue(new HttpResponseMessage {
				StatusCode = (HttpStatusCode)status,
				Content = new StringContent(body)
			});
			return this;
		}

		/// <summary>
		/// Adds an HttpResponseMessage to the response queue with a 200 (OK) status code and the given content body.
		/// </summary>
		public HttpTest RespondWith(string body) {
			return RespondWith(200, body);
		}

		/// <summary>
		/// Adds an HttpResponseMessage to the response queue with the given HTTP status code and the given data serialized to JSON as the content body.
		/// </summary>
		public HttpTest RespondWithJson(int status, object data) {
			ResponseQueue.Enqueue(new HttpResponseMessage {
				StatusCode = (HttpStatusCode)status,
				Content = new CapturedJsonContent(FlurlHttp.GlobalSettings.JsonSerializer.Serialize(data))
			});
			return this;
		}

		/// <summary>
		/// Adds an HttpResponseMessage to the response queue with a 200 (OK) status code and the given data serialized to JSON as the content body.
		/// </summary>
		public HttpTest RespondWithJson(object data) {
			return RespondWithJson(200, data);
		}

		/// <summary>
		/// Adds a simulated timeout response to the response queue.
		/// </summary>
		public HttpTest SimulateTimeout() {
			ResponseQueue.Enqueue(new TimeoutResponseMessage());
			return this;
		}

		/// <summary>
		/// Queue of HttpResponseMessages to be returned in place of real responses during testing.
		/// </summary>
		public Queue<HttpResponseMessage> ResponseQueue { get; set; }

		internal HttpResponseMessage GetNextResponse() {
			return ResponseQueue.Any() ? ResponseQueue.Dequeue() : _emptyResponse;
		}

		/// <summary>
		/// List of all (fake) HTTP calls made since this HttpTest was created.
		/// </summary>
		public List<HttpCall> CallLog { get; private set; }

		/// <summary>
		/// Throws an HttpCallAssertException if a URL matching the given pattern was not called.
		/// </summary>
		/// <param name="urlPattern">URL that should have been called. Can include * wildcard character.</param>
		public HttpCallAssertion ShouldHaveCalled(string urlPattern) {
			return new HttpCallAssertion(CallLog).WithUrlPattern(urlPattern);
		}

		/// <summary>
		/// Throws an HttpCallAssertException if a URL matching the given pattern was called.
		/// </summary>
		/// <param name="urlPattern">URL that should not have been called. Can include * wildcard character.</param>
		public HttpCallAssertion ShouldNotHaveCalled(string urlPattern) {
			return new HttpCallAssertion(CallLog, true).WithUrlPattern(urlPattern);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		public void Dispose() {
			FlurlHttp.GlobalSettings.ResetDefaults();
		}
	}
}