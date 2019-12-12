using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using Flurl.Http.Configuration;
using Flurl.Http.Content;
using Flurl.Util;

namespace Flurl.Http.Testing
{
	/// <summary>
	/// Represents a set of request conditions and fake responses for faking HTTP calls in tests.
	/// Usually created fluently via HttpTest.ForCallsTo, rather than instantiated directly.
	/// </summary>
	public class HttpTestSetup
	{
		private readonly List<Func<HttpCall, bool>> _filters = new List<Func<HttpCall, bool>>();
		private readonly List<Func<HttpResponseMessage>> _responses = new List<Func<HttpResponseMessage>>();

		private int _respIndex = 0;
		private bool _allowRealHttp = false;

		/// <summary>
		/// Constructs a new instance of HttpTestSetup.
		/// </summary>
		/// <param name="settings">FlurlHttpSettings used in fake calls.</param>
		/// <param name="urlPatterns">URL(s) or URL pattern(s) that this HttpTestSetup applies to. Can contain * wildcard.</param>
		public HttpTestSetup(TestFlurlHttpSettings settings, params string[] urlPatterns) {
			Settings = settings;
			if (urlPatterns.Any())
				With(call => urlPatterns.Any(p => Util.MatchesPattern(call.FlurlRequest.Url, p)));
		}

		/// <summary>
		/// The FlurlHttpSettings used in fake calls.
		/// </summary>
		public TestFlurlHttpSettings Settings { get; }

		internal bool FakeRequest => !_allowRealHttp;

		/// <summary>
		/// Returns true if the given HttpCall matches one of the URL patterns and all other criteria defined for this HttpTestSetup.
		/// </summary>
		internal bool IsMatch(HttpCall call) => _filters.All(f => f(call));

		internal HttpResponseMessage GetNextResponse() {
			if (!_responses.Any())
				return null;

			// atomically get the next response in the list, or the last one if we're past the end
			return _responses[Math.Min(Interlocked.Increment(ref _respIndex), _responses.Count) - 1]();
		}

		#region filtering
		/// <summary>
		/// Defines a condition for which this HttpTestSetup applies.
		/// </summary>
		public HttpTestSetup With(Func<HttpCall, bool> condition) {
			_filters.Add(condition);
			return this;
		}

		/// <summary>
		/// Defines a condition for which this HttpTestSetup does NOT apply.
		/// </summary>
		public HttpTestSetup Without(Func<HttpCall, bool> condition) {
			return With(c => !condition(c));
		}

		/// <summary>
		/// Defines one or more HTTP verbs, any of which a call must match in order for this HttpTestSetup to apply.
		/// </summary>
		public HttpTestSetup WithVerb(params HttpMethod[] verbs) {
			return With(call => call.HasAnyVerb(verbs));
		}

		/// <summary>
		/// Defines one or more HTTP verbs, any of which a call must match in order for this HttpTestSetup to apply.
		/// </summary>
		public HttpTestSetup WithVerb(params string[] verbs) {
			return With(call => call.HasAnyVerb(verbs));
		}

		/// <summary>
		/// Defines a query parameter and (optionally) its value that a call must contain in order for this HttpTestSetup to apply.
		/// </summary>
		public HttpTestSetup WithQueryParam(string name, object value = null) {
			return With(c => c.HasQueryParam(name, value));
		}

		/// <summary>
		/// Defines a query parameter and (optionally) its value that a call must NOT contain in order for this HttpTestSetup to apply.
		/// </summary>
		public HttpTestSetup WithoutQueryParam(string name, object value = null) {
			return Without(c => c.HasQueryParam(name, value));
		}

		/// <summary>
		/// Defines query parameter names, ALL of which a call must contain in order for this HttpTestSetup to apply.
		/// </summary>
		public HttpTestSetup WithQueryParams(params string[] names) {
			return With(c => c.HasAllQueryParams(names));
		}

		/// <summary>
		/// Defines query parameter names, NONE of which a call must contain in order for this HttpTestSetup to apply.
		/// If no names are provided, call must not contain any query parameters.
		/// </summary>
		public HttpTestSetup WithoutQueryParams(params string[] names) {
			return Without(c => c.HasAnyQueryParam(names));
		}

		/// <summary>
		/// Defines query parameters, ALL of which a call must contain in order for this HttpTestSetup to apply.
		/// </summary>
		/// <param name="values">Object (usually anonymous) or dictionary that is parsed to name/value query parameters to check for. Values may contain * wildcard.</param>
		public HttpTestSetup WithQueryParams(object values) {
			return With(c => c.HasQueryParams(values));
		}

		/// <summary>
		/// Defines query parameters, NONE of which a call must contain in order for this HttpTestSetup to apply.
		/// </summary>
		/// <param name="values">Object (usually anonymous) or dictionary that is parsed to name/value query parameters to check for. Values may contain * wildcard.</param>
		public HttpTestSetup WithoutQueryParams(object values) {
			return Without(c => c.HasQueryParams(values));
		}

		/// <summary>
		/// Defines query parameter names, ANY of which a call must contain in order for this HttpTestSetup to apply.
		/// If no names are provided, call must contain at least one query parameter with any name.
		/// </summary>
		public HttpTestSetup WithAnyQueryParam(params string[] names) {
			return With(c => c.HasAnyQueryParam(names));
		}

		/// <summary>
		/// Defines a request header and (optionally) its value that a call must contain in order for this HttpTestSetup to apply.
		/// </summary>
		public HttpTestSetup WithHeader(string name, string valuePattern = null) {
			return With(c => c.HasHeader(name, valuePattern));
		}

		/// <summary>
		/// Defines a request header and (optionally) its value that a call must NOT contain in order for this HttpTestSetup to apply.
		/// </summary>
		public HttpTestSetup WithoutHeader(string name, string valuePattern = null) {
			return Without(c => c.HasHeader(name, valuePattern));
		}

		/// <summary>
		/// Defines a request body that must exist in order for this HttpTestSetup to apply.
		/// The * wildcard can be used.
		/// </summary>
		public HttpTestSetup WithRequestBody(string pattern) {
			return With(call => Util.MatchesPattern(call.RequestBody, pattern));
		}

		/// <summary>
		/// Defines an object that, when serialized to JSON, must match the request body in order for this HttpTestSetup to apply.
		/// </summary>
		public HttpTestSetup WithRequestJson(object body) {
			return WithRequestBody(Settings.JsonSerializer.Serialize(body));
		}
		#endregion

		#region responding
		/// <summary>
		/// Adds an HttpResponseMessage to the response queue.
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
		/// Adds an HttpResponseMessage to the response queue with the given data serialized to JSON as the content body.
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
		/// Adds an HttpResponseMessage to the response queue.
		/// </summary>
		/// <param name="buildContent">A function that builds the simulated response body content. Optional.</param>
		/// <param name="status">The simulated HTTP status. Optional. Default is 200.</param>
		/// <param name="headers">The simulated response headers. Optional.</param>
		/// <param name="cookies">The simulated response cookies. Optional.</param>
		/// <param name="replaceUnderscoreWithHyphen">If true, underscores in property names of headers will be replaced by hyphens. Default is true.</param>
		/// <returns>The current HttpTest object (so more responses can be chained).</returns>
		public HttpTestSetup RespondWith(Func<HttpContent> buildContent = null, int status = 200, object headers = null, object cookies = null, bool replaceUnderscoreWithHyphen = true) {
			_responses.Add(() => {
				var response = new HttpResponseMessage {
					StatusCode = (HttpStatusCode)status,
					Content = buildContent?.Invoke()
				};
				if (headers != null) {
					foreach (var kv in headers.ToKeyValuePairs()) {
						var key = replaceUnderscoreWithHyphen ? kv.Key.Replace("_", "-") : kv.Key;
						response.SetHeader(key, kv.Value.ToInvariantString());
					}
				}

				if (cookies != null) {
					foreach (var kv in cookies.ToKeyValuePairs()) {
						var value = new Cookie(kv.Key, kv.Value.ToInvariantString()).ToString();
						response.Headers.Add("Set-Cookie", value);
					}
				}
				return response;
			});
			return this;
		}

		/// <summary>
		/// Adds a simulated timeout response to the response queue.
		/// </summary>
		public HttpTestSetup SimulateTimeout() {
			_responses.Add(() => new TimeoutResponseMessage());
			return this;
		}

		/// <summary>
		/// Do NOT fake requests for this setup. Typically called on a filtered setup, i.e. HttpTest.ForCallsTo(urlPattern).AllowRealHttp();
		/// </summary>
		public void AllowRealHttp() {
			_responses.Clear();
			_allowRealHttp = true;
		}
		#endregion
	}
}
