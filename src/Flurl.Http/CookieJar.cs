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
		/// Add a cookie to the jar or update if one with the same Name already exists.
		/// </summary>
		public CookieJar AddOrUpdate(FlurlCookie cookie) {
			if (string.IsNullOrEmpty(cookie.OriginUrl)) {
				if (string.IsNullOrEmpty(cookie.Domain) || string.IsNullOrEmpty(cookie.Path))
					throw new ArgumentException("OriginUrl must have a value unless both Domain and Path are specified. This is necessary to determine whether to send the cookie in subsequent requests.");
			}

			cookie.Changed += (_, name) => SyncToRequests(cookie, false);
			_dict[cookie.Name] = cookie;
			SyncToRequests(cookie, false);

			return this;
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
			foreach (var cookie in this.Values.Where(c => ShouldSend(c, req.Url)))
				req.Cookies[cookie.Name] = cookie.Value;
			_syncdRequests.Add(req);
		}

		/// <summary>
		/// Stops synchronization of changes to the CookieJar with the Cookies collection of the FlurlRequest
		/// </summary>
		internal void UnsyncWith(IFlurlRequest req) => _syncdRequests.Remove(req);

		private void SyncToRequests(FlurlCookie cookie, bool removed) {
			foreach (var req in _syncdRequests) {
				if (removed || !ShouldSend(cookie, req.Url))
					req.Cookies.Remove(cookie.Name);
				else
					req.Cookies[cookie.Name] = cookie.Value;
			}
		}

		/// <summary>
		/// True the given cookie should be sent in requests to the given URL.
		/// </summary>
		private static bool ShouldSend(FlurlCookie cookie, string url) {
			var origin = cookie.OriginUrl;
			if (string.IsNullOrEmpty(origin)) {
				if (string.IsNullOrEmpty(cookie.Domain) || string.IsNullOrEmpty(cookie.Path))
					return false; // CookieJar.AddOrUpdate will catch this in validation
				origin = $"{(cookie.Secure ? "https" : "http")}://{cookie.Domain.Trim().TrimStart('.')}".AppendPathSegment(cookie.Path);
			}

			// enlist the help of CookieContainer here, which feels really awful, but so does re-inventing the wheel.
			var cc = new System.Net.CookieContainer();
			var ccCookie = new System.Net.Cookie(cookie.Name, cookie.Value);
			if (cookie.MaxAge.HasValue) ccCookie.Expires = cookie.DateReceived.UtcDateTime.AddSeconds(cookie.MaxAge.Value);
			else if (cookie.Expires.HasValue) ccCookie.Expires = cookie.Expires.Value.UtcDateTime;
			if (!string.IsNullOrEmpty(cookie.Domain)) ccCookie.Domain = cookie.Domain;
			if (!string.IsNullOrEmpty(cookie.Path)) ccCookie.Path = cookie.Path;
			ccCookie.Secure = cookie.Secure;
			ccCookie.HttpOnly = cookie.HttpOnly;
			cc.Add(new Uri(origin), ccCookie);

			return cc.GetCookies(new Uri(url)).Count > 0;
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
}
