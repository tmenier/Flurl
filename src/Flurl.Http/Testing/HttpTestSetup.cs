using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http.Configuration;
using Flurl.Http.Content;
using Flurl.Util;

namespace Flurl.Http.Testing
{
	/// <summary>
	/// Abstract base class class for HttpTest and FilteredHttpTestSetup. Provides fluent methods for building queue of fake responses.
	/// </summary>
	public abstract class HttpTestSetup
	{
		private readonly List<Func<IFlurlRequest, HttpResponseMessage>> _responses = new();

		private int _respIndex = 0;
		private bool _allowRealHttp = false;

		/// <summary>
		/// Constructs a new instance of HttpTestSetup.
		/// </summary>
		/// <param name="settings">FlurlHttpSettings used in fake calls.</param>
		protected HttpTestSetup(FlurlHttpSettings settings) {
			Settings = settings;
		}

		/// <summary>
		/// The FlurlHttpSettings used in fake calls.
		/// </summary>
		public FlurlHttpSettings Settings { get; }

		internal HttpResponseMessage GetNextResponse(IFlurlRequest request) {
			if (_allowRealHttp)
				return null;

			// atomically get the next response in the list, or the last one if we're past the end
			if (_responses.Any())
				return _responses[Math.Min(Interlocked.Increment(ref _respIndex), _responses.Count) - 1](request);

			return new HttpResponseMessage {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent("")
			};
		}

		/// <summary>
		/// Adds a fake HTTP response to the response queue.
		/// </summary>
		/// <param name="body">The simulated response body string.</param>
		/// <param name="status">The simulated HTTP status. Default is 200.</param>
		/// <param name="headers">The simulated response headers (optional).</param>
		/// <param name="cookies">The simulated response cookies (optional).</param>
		/// <param name="replaceUnderscoreWithHyphen">If true, underscores in property names of headers will be replaced by hyphens. Default is true.</param>
		/// <returns>The current HttpTest object (so more responses can be chained).</returns>
		public HttpTestSetup RespondWith(string body, int status = 200, object headers = null, object cookies = null, bool replaceUnderscoreWithHyphen = true) {
			return RespondWith(() => new CapturedStringContent(body), status, headers, cookies, replaceUnderscoreWithHyphen);
		}


		/// <summary>
		/// Adds a fake HTTP response to the response queue.
		/// </summary>
		/// <param name="buildContent">A function that builds the simulated response body content. Optional.</param>
		/// <param name="status">The simulated HTTP status. Default is 200.</param>
		/// <param name="headers">The simulated response headers (optional).</param>
		/// <param name="cookies">The simulated response cookies (optional).</param>
		/// <param name="replaceUnderscoreWithHyphen">If true, underscores in property names of headers will be replaced by hyphens. Default is true.</param>
		/// <returns>The current HttpTest object (so more responses can be chained).</returns>
		public HttpTestSetup RespondWith(Func<IFlurlRequest, string> buildContent, int status = 200, object headers = null, object cookies = null, bool replaceUnderscoreWithHyphen = true) {
			return RespondWith((request) => new CapturedStringContent(buildContent(request)), status, headers, cookies, replaceUnderscoreWithHyphen);
		}

		/// <summary>
		/// Adds a fake HTTP response to the response queue with the given data serialized to JSON as the content body.
		/// </summary>
		/// <param name="body">The object to be JSON-serialized and used as the simulated response body.</param>
		/// <param name="status">The simulated HTTP status. Default is 200.</param>
		/// <param name="headers">The simulated response headers (optional).</param>
		/// <param name="cookies">The simulated response cookies (optional).</param>
		/// <param name="replaceUnderscoreWithHyphen">If true, underscores in property names of headers will be replaced by hyphens. Default is true.</param>
		/// <returns>The current HttpTest object (so more responses can be chained).</returns>
		public HttpTestSetup RespondWithJson(object body, int status = 200, object headers = null, object cookies = null, bool replaceUnderscoreWithHyphen = true) {
			var s = Settings.JsonSerializer.Serialize(body);
			return RespondWith(() => new CapturedJsonContent(s), status, headers, cookies, replaceUnderscoreWithHyphen);
		}

		/// <summary>
		/// Adds a fake HTTP response to the response queue with the given data serialized to JSON as the content body.
		/// </summary>
		/// <param name="buildContent">A function that builds the simulated response body content. Optional.</param>
		/// <param name="status">The simulated HTTP status. Default is 200.</param>
		/// <param name="headers">The simulated response headers (optional).</param>
		/// <param name="cookies">The simulated response cookies (optional).</param>
		/// <param name="replaceUnderscoreWithHyphen">If true, underscores in property names of headers will be replaced by hyphens. Default is true.</param>
		/// <returns>The current HttpTest object (so more responses can be chained).</returns>
		public HttpTestSetup RespondWithJson(Func<IFlurlRequest, object> buildContent, int status = 200, object headers = null, object cookies = null, bool replaceUnderscoreWithHyphen = true) {
			return RespondWith((request) =>
			{
				var s = Settings.JsonSerializer.Serialize(buildContent(request));
				return new CapturedJsonContent(s);
			}, status, headers, cookies, replaceUnderscoreWithHyphen);
		}

		/// <summary>
		/// Adds a fake HTTP response to the response queue.
		/// </summary>
		/// <param name="buildContent">A function that builds the simulated response body content. Optional.</param>
		/// <param name="status">The simulated HTTP status. Optional. Default is 200.</param>
		/// <param name="headers">The simulated response headers. Optional.</param>
		/// <param name="cookies">The simulated response cookies. Optional.</param>
		/// <param name="replaceUnderscoreWithHyphen">If true, underscores in property names of headers will be replaced by hyphens. Default is true.</param>
		/// <returns>The current HttpTest object (so more responses can be chained).</returns>
		public HttpTestSetup RespondWith(Func<HttpContent> buildContent, int status = 200, object headers = null, object cookies = null, bool replaceUnderscoreWithHyphen = true)
		{
			return RespondWith(request => buildContent?.Invoke(), status, headers, cookies, replaceUnderscoreWithHyphen);
		}

		/// <summary>
		/// Adds a fake HTTP response to the response queue.
		/// </summary>
		/// <param name="buildContent">A function that builds the simulated response body content. Optional.</param>
		/// <param name="status">The simulated HTTP status. Optional. Default is 200.</param>
		/// <param name="headers">The simulated response headers. Optional.</param>
		/// <param name="cookies">The simulated response cookies. Optional.</param>
		/// <param name="replaceUnderscoreWithHyphen">If true, underscores in property names of headers will be replaced by hyphens. Default is true.</param>
		/// <returns>The current HttpTest object (so more responses can be chained).</returns>
		public HttpTestSetup RespondWith(Func<IFlurlRequest, HttpContent> buildContent = null, int status = 200, object headers = null, object cookies = null, bool replaceUnderscoreWithHyphen = true) {
			_responses.Add((request) => {
				var response = new HttpResponseMessage {
					StatusCode = (HttpStatusCode)status,
					Content = buildContent?.Invoke(request)
				};

				if (headers != null) {
					foreach (var kv in headers.ToKeyValuePairs()) {
						var key = replaceUnderscoreWithHyphen ? kv.Key.Replace("_", "-") : kv.Key;
						response.SetHeader(key, kv.Value.ToInvariantString());
					}
				}

				if (cookies != null) {
					foreach (var kv in cookies.ToKeyValuePairs())
						response.Headers.Add("Set-Cookie", $"{kv.Key}={kv.Value}");
				}
				return response;
			});
			return this;
		}

		/// <summary>
		/// Adds a simulated timeout response to the response queue.
		/// </summary>
		public HttpTestSetup SimulateTimeout() =>
			SimulateException(new TaskCanceledException(null, new TimeoutException())); // inner exception covers the .net5+ case https://stackoverflow.com/a/65989456/62600

		/// <summary>
		/// Adds the throwing of an exception to the response queue.
		/// </summary>
		/// <param name="exception">The exception to throw when the call is simulated.</param>
		public HttpTestSetup SimulateException(Exception exception) {
			_responses.Add((request) => throw exception);
			return this;
		}

		/// <summary>
		/// Do NOT fake requests for this setup. Typically called on a filtered setup, i.e. HttpTest.ForCallsTo(urlPattern).AllowRealHttp();
		/// </summary>
		public void AllowRealHttp() {
			_responses.Clear();
			_allowRealHttp = true;
		}
	}
}
