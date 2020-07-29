using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Flurl.Util;

namespace Flurl.Http
{
	/// <summary>
	/// A collection of FlurlCookies that can be passed to one or more FlurlRequests, either
	/// explicitly via WithCookies or implicitly via FlurlClient.StartCookieSession. Automatically
	/// populated/synchronized with cookies received via Set-Cookie response headers. Chooses
	/// which cookies to send in Cookie request per RFC 6265.
	/// </summary>
	public class CookieJar : IReadOnlyDictionary<string, FlurlCookie>
	{
		private readonly ConcurrentDictionary<string, FlurlCookie> _dict = new ConcurrentDictionary<string, FlurlCookie>();

		// requests whose Cookies collection should be kept in sync with changes to this CookieJar
		private readonly HashSet<IFlurlRequest> _syncdRequests = new HashSet<IFlurlRequest>();

		/// <summary>
		/// Add a cookie to the jar or update if one with the same Name already exists.
		/// </summary>
		/// <param name="name">Name of the cookie.</param>
		/// <param name="value">Value of the cookie.</param>
		/// <param name="originUrl">URL of request that sent the original Set-Cookie header.</param>
		/// <param name="dateReceived">Date/time that original Set-Cookie header was received. Defaults to current date/time. Important for Max-Age to be enforced correctly.</param>
		public CookieJar AddOrUpdate(string name, object value, string originUrl, DateTimeOffset? dateReceived = null) =>
			AddOrUpdate(new FlurlCookie(name, value.ToInvariantString(), originUrl, dateReceived));

		/// <summary>
		/// Adds a cookie to the jar or updates if one with the same Name already exists.
		/// Throws FlurlHttpException if cookie is invalid.
		/// </summary>
		public CookieJar AddOrUpdate(FlurlCookie cookie) {
			if (!TryAddOrUpdate(cookie, out var reason))
				throw new InvalidCookieException(reason);

			return this;
		}

		/// <summary>
		/// Adds a cookie to the jar or updates if one with the same Name already exists, if it is valid.
		/// Returns true if cookie is valid and was added. If false, provides descriptive reason.
		/// </summary>
		public bool TryAddOrUpdate(FlurlCookie cookie, out string reason) {
			if (!cookie.IsValid(out reason) || cookie.IsExpired(out reason))
				return false;

			cookie.Changed += (_, name) => SyncToRequests(cookie, false);
			_dict[cookie.Name] = cookie;
			SyncToRequests(cookie, false);

			return true;
		}

		/// <summary>
		/// Removes a cookie from the CookieJar.
		/// </summary>
		/// <param name="name">The cookie name.</param>
		public CookieJar Remove(string name) {
			if (_dict.TryRemove(name, out var cookie))
				SyncToRequests(cookie, true);

			return this;
		}

		/// <summary>
		/// Removes all cookies from this CookieJar
		/// </summary>
		public CookieJar Clear() {
			var all = _dict.Values;
			_dict.Clear();
			foreach (var cookie in all)
				SyncToRequests(cookie, true);
			return this;
		}

		/// <summary>
		/// Ensures changes to the CookieJar are kept in sync with the Cookies collection of the FlurlRequest
		/// </summary>
		internal void SyncWith(IFlurlRequest req) {
			foreach (var cookie in this.Values.Where(c => c.ShouldSendTo(req.Url, out _)))
				req.Cookies[cookie.Name] = cookie.Value;
			_syncdRequests.Add(req);
		}

		/// <summary>
		/// Stops synchronization of changes to the CookieJar with the Cookies collection of the FlurlRequest
		/// </summary>
		internal void UnsyncWith(IFlurlRequest req) => _syncdRequests.Remove(req);

		private void SyncToRequests(FlurlCookie cookie, bool removed) {
			foreach (var req in _syncdRequests) {
				if (removed || !cookie.ShouldSendTo(req.Url, out _))
					req.Cookies.Remove(cookie.Name);
				else
					req.Cookies[cookie.Name] = cookie.Value;
			}
		}

		/// <inheritdoc/>
		public IEnumerator<KeyValuePair<string, FlurlCookie>> GetEnumerator() => _dict.GetEnumerator();

		/// <inheritdoc/>
		IEnumerator IEnumerable.GetEnumerator() => _dict.GetEnumerator();

		/// <inheritdoc/>
		public int Count => _dict.Count;

		/// <inheritdoc/>
		public bool ContainsKey(string key) => _dict.ContainsKey(key);

		/// <inheritdoc/>
		public bool TryGetValue(string key, out FlurlCookie value) => _dict.TryGetValue(key, out value);

		/// <inheritdoc/>
		public FlurlCookie this[string key] => _dict[key];

		/// <inheritdoc/>
		public IEnumerable<string> Keys => _dict.Keys;

		/// <inheritdoc/>
		public IEnumerable<FlurlCookie> Values => _dict.Values;
	}

	/// <summary>
	/// Exception thrown when attempting to add or update an invalid FlurlCookie to a CookieJar.
	/// </summary>
	public class InvalidCookieException : Exception
	{
		/// <summary>
		/// Creates a new InvalidCookieException.
		/// </summary>
		public InvalidCookieException(string reason) : base(reason) { }
	}
}
