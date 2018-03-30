using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Flurl.Util;

namespace Flurl.Http.Testing
{
	/// <summary>
	/// Provides fluent helpers for asserting against (faked) HTTP calls. This class is normally not
	/// instantiated directly; you can get an instance via HttpTest.ShouldHaveCalled or
	/// HttpTest.ShouldNotHaveCalled
	/// </summary>
	public class HttpCallAssertion
	{
		private readonly bool _negate;
		private readonly IList<string> _expectedConditions = new List<string>();

		private IList<HttpCall> _calls;

		/// <param name="loggedCalls">Set of calls (usually from HttpTest.CallLog) to assert against.</param>
		/// <param name="negate">if true, assertions pass when calls matching criteria were NOT made.</param>
		public HttpCallAssertion(IEnumerable<HttpCall> loggedCalls, bool negate = false) {
			_calls = loggedCalls.ToList();
			_negate = negate;
		}

	    /// <summary>
	    /// Assert whether calls matching specified criteria were made a specific number of times. (When not specified,
	    /// assertions verify whether any calls matching criteria were made.)
	    /// </summary>
	    /// <param name="expectedCount">Exact number of expected calls</param>
	    /// <exception cref="ArgumentException"><paramref name="expectedCount"/> must be greater than or equal to 0.</exception>
	    public void Times(int expectedCount) {
			if (expectedCount < 0)
				throw new ArgumentException("expectedCount must be greater than or equal to 0.");

			Assert(expectedCount);
		}

		/// <summary>
		/// Asserts whether calls were made matching given URL or URL pattern.
		/// </summary>
		/// <param name="urlPattern">Can contain * wildcard.</param>
		public HttpCallAssertion WithUrlPattern(string urlPattern) {
			if (urlPattern == "*") {
				Assert();
				return this;
			}
			_expectedConditions.Add($"URL pattern {urlPattern}");
			return With(c => MatchesPattern(c.FlurlRequest.Url, urlPattern));
		}

		/// <summary>
		/// Asserts whether calls were made containing the given query parameter (regardless of its value).
		/// </summary>
		/// <param name="name">The query parameter name.</param>
		/// <returns></returns>
		public HttpCallAssertion WithQueryParam(string name) {
			_expectedConditions.Add($"query parameter {name}");
			return With(c => c.FlurlRequest.Url.QueryParams.Any(q => q.Name == name));
		}

		/// <summary>
		/// Asserts whether calls were made NOT containing the given query parameter.
		/// </summary>
		/// <param name="name">The query parameter name.</param>
		/// <returns></returns>
		public HttpCallAssertion WithoutQueryParam(string name) {
			_expectedConditions.Add($"no query parameter {name}");
			return Without(c => c.FlurlRequest.Url.QueryParams.Any(q => q.Name == name));
		}

		/// <summary>
		/// Asserts whether calls were made containing all the given query parameters (regardless of their values).
		/// </summary>
		/// <param name="names">The query parameter names.</param>
		/// <returns></returns>
		public HttpCallAssertion WithQueryParams(params string[] names) {
			if (!names.Any()) {
				_expectedConditions.Add("any query parameters");
				return With(c => c.FlurlRequest.Url.QueryParams.Any());
			}
			return names.Select(WithQueryParam).LastOrDefault() ?? this;
		}

		/// <summary>
		/// Asserts whether calls were made NOT containing ANY of the given query parameters.
		/// </summary>
		/// <param name="names">The query parameter names.</param>
		/// <returns></returns>
		public HttpCallAssertion WithoutQueryParams(params string[] names) {
			if (!names.Any()) {
				_expectedConditions.Add("no query parameters");
				return Without(c => c.FlurlRequest.Url.QueryParams.Any());
			}
			return names.Select(WithoutQueryParam).LastOrDefault() ?? this;
		}

		/// <summary>
		/// Asserts whether calls were made containing the given query parameter name and value.
		/// </summary>
		/// <param name="name">The query parameter name.</param>
		/// <param name="value">The query parameter value. Can contain * wildcard.</param>
		/// <returns></returns>
		public HttpCallAssertion WithQueryParamValue(string name, object value) {
			if (value is IEnumerable && !(value is string)) {
				foreach (var val in (IEnumerable)value)
					WithQueryParamValue(name, val);
				return this;
			}
			_expectedConditions.Add($"query parameter {name}={value}");
			return With(c => c.FlurlRequest.Url.QueryParams.Any(qp => QueryParamMatches(qp, name, value)));
		}

		/// <summary>
		/// Asserts whether calls were made NOT containing the given query parameter name and value.
		/// </summary>
		/// <param name="name">The query parameter name.</param>
		/// <param name="value">The query parameter value. Can contain * wildcard.</param>
		/// <returns></returns>
		public HttpCallAssertion WithoutQueryParamValue(string name, object value) {
			if (value is IEnumerable && !(value is string)) {
				foreach (var val in (IEnumerable)value)
					WithoutQueryParamValue(name, val);
				return this;
			}
			_expectedConditions.Add($"no query parameter {name}={value}");
			return Without(c => c.FlurlRequest.Url.QueryParams.Any(qp => QueryParamMatches(qp, name, value)));
		}

		/// <summary>
		/// Asserts whether calls were made containing all of the given query parameter values.
		/// </summary>
		/// <param name="values">Object (usually anonymous) or dictionary that is parsed to name/value query parameters to check for.</param>
		/// <returns></returns>
		public HttpCallAssertion WithQueryParamValues(object values) {
			return values.ToKeyValuePairs().Select(kv => WithQueryParamValue(kv.Key, kv.Value)).LastOrDefault() ?? this;
		}

		/// <summary>
		/// Asserts whether calls were made NOT containing ANY of the given query parameter values.
		/// </summary>
		/// <param name="values">Object (usually anonymous) or dictionary that is parsed to name/value query parameters to check for.</param>
		/// <returns></returns>
		public HttpCallAssertion WithoutQueryParamValues(object values) {
			return values.ToKeyValuePairs().Select(kv => WithoutQueryParamValue(kv.Key, kv.Value)).LastOrDefault() ?? this;
		}

		private bool QueryParamMatches(QueryParameter qp, string name, object value) {
			if (qp.Name != name)
				return false;
			if (value is string)
				return MatchesPattern(qp.Value?.ToString(), value?.ToString());
			return qp.Value?.ToString() == value?.ToString();
		}

		/// <summary>
		/// Asserts whether calls were made containing given request body or request body pattern.
		/// </summary>
		/// <param name="bodyPattern">Can contain * wildcard.</param>
		public HttpCallAssertion WithRequestBody(string bodyPattern) {
			_expectedConditions.Add($"body matching pattern {bodyPattern}");
			return With(c => MatchesPattern(c.RequestBody, bodyPattern));
		}

		/// <summary>
		/// Asserts whether calls were made containing given request body.
		/// </summary>
		/// <param name="body"></param>
		public HttpCallAssertion WithRequestJson(object body) {
			var serializedBody = FlurlHttp.GlobalSettings.JsonSerializer.Serialize(body);
			return WithRequestBody(serializedBody);
		}

		/// <summary>
		/// Asserts whether calls were made with given HTTP verb.
		/// </summary>
		public HttpCallAssertion WithVerb(HttpMethod httpMethod) {
			_expectedConditions.Add("verb " + httpMethod);
			return With(c => c.Request.Method == httpMethod);
		}

		/// <summary>
		/// Asserts whether calls were made with a request body of the given content (MIME) type.
		/// </summary>
		public HttpCallAssertion WithContentType(string mediaType) {
			_expectedConditions.Add("content type " + mediaType);
			return With(c => c.Request.Content?.Headers?.ContentType?.MediaType == mediaType);
		}

		/// <summary>
		/// Asserts whether the Authorization header was set with OAuth.
		/// </summary>
		/// <param name="token">Expected token value</param>
		/// <returns></returns>
		public HttpCallAssertion WithOAuthBearerToken(string token) {
			_expectedConditions.Add("OAuth bearer token " + token);
			return With(c => c.Request.Headers.Authorization?.Scheme == "Bearer"
				&& c.Request.Headers.Authorization?.Parameter == token);
		}

		/// <summary>
		/// Asserts whther the calls were made containing the given request header.
		/// </summary>
		/// <param name="name">Expected header name</param>
		/// <param name="valuePattern">Expected header value pattern</param>
		/// <returns></returns>
		public HttpCallAssertion WithHeader(string name, string valuePattern = "*") {
			_expectedConditions.Add($"header {name}: {valuePattern}");
			return With(c => 
				c.Request.Headers.TryGetValues(name, out var vals) &&
				vals.Any(v => MatchesPattern(v, valuePattern)));
		}

		/// <summary>
		/// Asserts whther the calls were made that do not contain the given request header.
		/// </summary>
		/// <param name="name">Expected header name</param>
		/// <param name="valuePattern">Expected header value pattern</param>
		/// <returns></returns>
		public HttpCallAssertion WithoutHeader(string name, string valuePattern = "*") {
			_expectedConditions.Add($"no header {name}: {valuePattern}");
			return Without(c =>
				c.Request.Headers.TryGetValues(name, out var vals) &&
				vals.Any(v => MatchesPattern(v, valuePattern)));
		}

		/// <summary>
		/// Asserts whether the Authorization header was set with basic auth.
		/// </summary>
		/// <param name="username">Expected username</param>
		/// <param name="password">Expected password</param>
		/// <returns></returns>
		public HttpCallAssertion WithBasicAuth(string username, string password) {
			_expectedConditions.Add($"basic auth credentials {username}/{password}");
			var value = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
			return With(c => c.Request.Headers.Authorization?.Scheme == "Basic"
				&& c.Request.Headers.Authorization?.Parameter == value);
		}

		/// <summary>
		/// Asserts whether calls were made matching the given predicate function.
		/// </summary>
		/// <param name="match">Predicate (usually a lambda expression) that tests an HttpCall and returns a bool.</param>
		public HttpCallAssertion With(Func<HttpCall, bool> match) {
			_calls = _calls.Where(match).ToList();
			Assert();
			return this;
		}

		/// <summary>
		/// Asserts whether calls were made that do NOT match the given predicate function.
		/// </summary>
		/// <param name="match">Predicate (usually a lambda expression) that tests an HttpCall and returns a bool.</param>
		public HttpCallAssertion Without(Func<HttpCall, bool> match) {
			_calls = _calls.Where(c => !match(c)).ToList();
			Assert();
			return this;
		}

		private void Assert(int? count = null) {
			var pass = count.HasValue ? (_calls.Count == count.Value) : _calls.Any();
			if (_negate) pass = !pass;

			if (!pass)
				throw new HttpCallAssertException(_expectedConditions, count, _calls.Count);
		}

		private bool MatchesPattern(string textToCheck, string pattern) {
			var regex = Regex.Escape(pattern).Replace("\\*", "(.*)");
			return Regex.IsMatch(textToCheck ?? "", regex);
		}
	}
}
