using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
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

		public HttpTest() {
			FlurlHttp.Configure(opts => {
				opts.HttpClientFactory = new TestHttpClientFactory(this);
				opts.AfterCall = call => CallLog.Add(call);
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
				Content = new CapturedJsonContent(data)
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
		/// <returns></returns>
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
		/// Throws an HttpCallAssertException if a URL matching the given pattern (and other optional criteria) was not called.
		/// </summary>
		/// <param name="urlPattern">URL that should have been called. Can include * wildcard character.</param>
		/// <param name="times">Exact number of times URL should have been called (optional).</param>
		/// <param name="verb">HTTP verb that should have been used in matched HTTP call(s).</param>
		/// <param name="contentType">MIME type that should have been specified in content-type header of matched HTTP call(s).</param>
		/// <param name="bodyPattern">Request body that should have been sent in matched HTTP call(s). Can include * wildcard character.</param>
		public void ShouldHaveCalled(string urlPattern, int? times = null, HttpMethod verb = null, string contentType = null, string bodyPattern = null) {
			var calls = CallLog.Where(c => MatchesPattern(c.Request.RequestUri.AbsoluteUri, urlPattern));

			if (verb != null)
				calls = calls.Where(c => c.Request.Method == verb);

			if (contentType != null)
				calls = calls.Where(c => c.Request.Content.Headers.ContentType.MediaType == contentType);

			if (bodyPattern != null)
				calls = calls.Where(c => MatchesPattern(c.RequestBody, bodyPattern));

			var count = calls.Count();
			var pass = times.HasValue ? count == times.Value : count > 0;
			if (!pass)
				throw new HttpCallAssertException(urlPattern, times, count);
		}

		private bool MatchesPattern(string textToCheck, string pattern) {
			var regex = Regex.Escape(pattern).Replace("\\*", "(.*)");
			return Regex.IsMatch(textToCheck, regex);
		}

		/// <summary>
		/// Throws an HttpCallAssertException if a URL matching the given pattern was called.
		/// </summary>
		/// <param name="urlPattern">URL that should not have been called. Can include * wildcard character.</param>
		public void ShouldNotHaveCalled(string urlPattern) {
			ShouldHaveCalled(urlPattern, times: 0);
		}

		public void Dispose() {
			FlurlHttp.Configuration.ResetDefaults();
		}
	}
}
