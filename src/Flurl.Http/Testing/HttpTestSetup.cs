using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http.Configuration;
using Flurl.Http.Content;
using Flurl.Util;

namespace Flurl.Http.Testing
{
	/// <summary>
	/// Represents a collection of fake responses and other test configurations for requests that match specific criteria.
	/// Usually created fluently via HttpTest.ForCallsTo(...), rather than instantiated directly.
	/// </summary>
	public class HttpTestSetup
	{
		private List<Func<HttpCall, bool>> _filters = new List<Func<HttpCall, bool>>();

		/// <summary>
		/// Queue of fake responses to return for calls matching this setup.
		/// </summary>
		public ConcurrentQueue<HttpResponseMessage> ResponseQueue { get; } = new ConcurrentQueue<HttpResponseMessage>();

		/// <summary>
		/// Constructs a new instance of HttpTestSetup.
		/// </summary>
		public HttpTestSetup(TestFlurlHttpSettings settings, params string[] urlPatterns) {
			Settings = settings;
			if (urlPatterns.Any())
				With(call => urlPatterns.Any(p => Util.MatchesPattern(call.FlurlRequest.Url, p)));
		}

		/// <summary>
		/// Gets the FlurlHttpSettings object used by this test.
		/// </summary>
		public TestFlurlHttpSettings Settings { get; }

		/// <summary>
		/// Returns true if the giving HttpCall matches one of the URL patterns and all other criteria defined for this HttpTestSetup.
		/// </summary>
		public bool IsMatch(HttpCall call) => _filters.All(f => f(call));

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
			return With(call => verbs.Any(verb => call.Request.Method == verb));
		}

		/// <summary>
		/// Defines one or more HTTP verbs, any of which a call must match in order for this HttpTestSetup to apply.
		/// </summary>
		public HttpTestSetup WithVerb(params string[] verbs) {
			return With(call => verbs.Any(verb => call.Request.Method.Method.Equals(verb, StringComparison.OrdinalIgnoreCase)));
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
			return With(c => c.HasQueryParams(names));
		}

		/// <summary>
		/// Defines query parameter name-value pairs, expressed as properties of an object, ALL of which a call must contain
		/// in order for this HttpTestSetup to apply.
		/// </summary>
		public HttpTestSetup WithQueryParams(object values, NullValueHandling nullValueHandling = NullValueHandling.NameOnly) {
			return With(c => c.HasQueryParams(values, nullValueHandling));
		}

		/// <summary>
		/// Defines query parameter names, ANY of which a call must contain in order for this HttpTestSetup to apply.
		/// If no names are provided, call must contain at least one query parameter with any name.
		/// </summary>
		public HttpTestSetup WithAnyQueryParam(params string[] names) {
			return With(c => c.HasAnyQueryParam(names));
		}

		/// <summary>
		/// Defines query parameter names, NONE of which a call must contain in order for this HttpTestSetup to apply.
		/// If no names are provided, call must not contain any query parameters.
		/// </summary>
		public HttpTestSetup WithoutQueryParams(params string[] names) {
			return Without(c => c.HasAnyQueryParam(names));
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

		#region responses
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
			return RespondWith(new StringContent(body), status, headers, cookies, replaceUnderscoreWithHyphen);
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
			var content = new CapturedJsonContent(Settings.JsonSerializer.Serialize(body));
			return RespondWith(content, status, headers, cookies, replaceUnderscoreWithHyphen);
		}

		/// <summary>
		/// Adds an HttpResponseMessage to the response queue.
		/// </summary>
		/// <param name="content">The simulated response body content (optional).</param>
		/// <param name="status">The simulated HTTP status. Default is 200.</param>
		/// <param name="headers">The simulated response headers (optional).</param>
		/// <param name="cookies">The simulated response cookies (optional).</param>
		/// <param name="replaceUnderscoreWithHyphen">If true, underscores in property names of headers will be replaced by hyphens. Default is true.</param>
		/// <returns>The current HttpTest object (so more responses can be chained).</returns>
		public HttpTestSetup RespondWith(HttpContent content = null, int status = 200, object headers = null, object cookies = null, bool replaceUnderscoreWithHyphen = true) {
			var response = new HttpResponseMessage {
				StatusCode = (HttpStatusCode)status,
				Content = content
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
			ResponseQueue.Enqueue(response);
			return this;
		}

		/// <summary>
		/// Adds a simulated timeout response to the response queue.
		/// </summary>
		public HttpTestSetup SimulateTimeout() {
			ResponseQueue.Enqueue(new TimeoutResponseMessage());
			return this;
		}
		#endregion
	}
}
