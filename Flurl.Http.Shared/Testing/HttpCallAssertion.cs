using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

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

		private IList<HttpCall> _calls;
		private string _urlPattern;

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
		/// <param name="expectedCount">Exact numnber of expected calls</param>
		/// <returns></returns>
		public void Times(int expectedCount) {
			if (expectedCount < 0)
				throw new ArgumentException("expectedCount must be greater than or equal to 0.");

			Assert(expectedCount);
		}

		/// <summary>
		/// Asserts whether calls were made matching given URL or URL pattern.
		/// </summary>
		/// <param name="urlPattern">Can contain * wildcard.</param>
		/// <returns></returns>
		public HttpCallAssertion WithUrlPattern(string urlPattern) {
			_urlPattern = urlPattern; // this'll land in the exception message when we assert, which is the only reason for capturing it
			return With(c => MatchesPattern(c.Request.RequestUri.AbsoluteUri, urlPattern));
		}

		/// <summary>
		/// Asserts wheter calls were made containing given request body or request body pattern.
		/// </summary>
		/// <param name="bodyPattern">Can contain * wildcard.</param>
		/// <returns></returns>
		public HttpCallAssertion WithRequestBody(string bodyPattern) {
			return With(c => MatchesPattern(c.RequestBody, bodyPattern));
		}

		/// <summary>
		/// Asserts whether calls were made with given HTTP verb.
		/// </summary>
		/// <returns></returns>
		public HttpCallAssertion WithVerb(HttpMethod httpMethod) {
			return With(c => c.Request.Method == httpMethod);
		}

		/// <summary>
		/// Asserts whether calls were made with a request body of the given content (MIME) type.
		/// </summary>
		/// <returns></returns>
		public HttpCallAssertion WithContentType(string contentType) {
			return With(c => c.Request.Content.Headers.ContentType.MediaType == contentType);
		}

		/// <summary>
		/// Asserts whether the Authorization header was set with OAuth.
		/// </summary>
		/// <param name="token">Expected token value</param>
		/// <returns></returns>
		public HttpCallAssertion WithOAuthBearerToken(string token) {
			return With(c => c.Request.Headers.Authorization?.Scheme == "Bearer"
							 && c.Request.Headers.Authorization?.Parameter == token);
		}

		/// <summary>
		/// Asserts whether the Authorization header was set with basic auth.
		/// </summary>
		/// <param name="username">Expected username</param>
		/// <param name="password">Expected password</param>
		/// <returns></returns>
		public HttpCallAssertion WithBasicAuth(string username, string password)
		{
			var value = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
			return With(c => c.Request.Headers.Authorization?.Scheme == "Basic"
			                 && c.Request.Headers.Authorization?.Parameter == value);
		}

		/// <summary>
		/// Asserts whether calls were made matching a given predicate function.
		/// </summary>
		/// <param name="match">Predicate (usually a lambda expression) that tests an HttpCall and returns a bool.</param>
		/// <returns></returns>
		public HttpCallAssertion With(Func<HttpCall, bool> match) {
			_calls = _calls.Where(match).ToList();
			Assert();
			return this;
		}

		private void Assert(int? count = null) {
			var pass = count.HasValue ? (_calls.Count == count.Value) : _calls.Any();
			if (_negate) pass = !pass;

			if (!pass)
				throw new HttpCallAssertException(_urlPattern, count, _calls.Count);
		}

		private bool MatchesPattern(string textToCheck, string pattern) {
			var regex = Regex.Escape(pattern).Replace("\\*", "(.*)");
			return Regex.IsMatch(textToCheck, regex);
		}
	}
}
