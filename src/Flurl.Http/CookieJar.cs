using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
			foreach (var cookie in this.Values.Where(c => ShouldSend(c, req.Url, out _)))
				req.Cookies[cookie.Name] = cookie.Value;
			_syncdRequests.Add(req);
		}

		/// <summary>
		/// Stops synchronization of changes to the CookieJar with the Cookies collection of the FlurlRequest
		/// </summary>
		internal void UnsyncWith(IFlurlRequest req) => _syncdRequests.Remove(req);

		private void SyncToRequests(FlurlCookie cookie, bool removed) {
			foreach (var req in _syncdRequests) {
				if (removed || !ShouldSend(cookie, req.Url, out _))
					req.Cookies.Remove(cookie.Name);
				else
					req.Cookies[cookie.Name] = cookie.Value;
			}
		}

		/// <summary>
		/// True if the given cookie should be sent in a request to the given URL. If false, a descriptive reason is provided.
		/// </summary>
		public static bool ShouldSend(FlurlCookie cookie, Url requestUrl, out string reason) {
			if (cookie.Secure && !requestUrl.IsSecureScheme) {
				reason = $"Cookie is marked Secure and request URL is insecure ({requestUrl.Scheme}).";
				return false;
			}

			return
				ValidateOrigin(cookie, out var originUrl, out reason) &&
				IsDomainMatch(cookie, originUrl, requestUrl, out reason) &&
				IsPathMatch(cookie, originUrl, requestUrl, out reason) &&
				!IsExpired(cookie, out reason);
		}

		private static bool ValidateOrigin(FlurlCookie cookie, out Url originUrl, out string reason) {
			if (!string.IsNullOrEmpty(cookie.OriginUrl)) {
				originUrl = new Url(cookie.OriginUrl);
				reason = "ok";
				return true;
			}

			if (!string.IsNullOrEmpty(cookie.Domain) && !string.IsNullOrEmpty(cookie.Path)) {
				originUrl = $"{(cookie.Secure ? "https" : "http")}://{cookie.Domain.Trim().TrimStart('.')}"
					.AppendPathSegment(cookie.Path);
				reason = "ok";
				return true;
			}

			// CookieJar.AddOrUpdate will catch this in validation. Should this throw instead?
			originUrl = null;
			reason = "Either OriginUrl, or both Domain and Path, must be specified to determine whether to send this cooke.";
			return false;
		}

		private static bool IsDomainMatch(FlurlCookie cookie, Url originUrl, Url requestUrl, out string reason) {
			reason = "ok";

			if (requestUrl.Host.Equals(originUrl.Host, StringComparison.OrdinalIgnoreCase))
				return true;

			if (string.IsNullOrEmpty(cookie.Domain)) {
				reason = $"Cookie set from {originUrl.Host} without Domain specified should only be sent to that specific host, not {requestUrl.Host}.";
				return false;
			}

			var domain = cookie.Domain.TrimStart('.');
			if (requestUrl.Host.Equals(domain, StringComparison.OrdinalIgnoreCase))
				return true;

			if (requestUrl.Host.EndsWith("." + domain, StringComparison.OrdinalIgnoreCase))
				return true;

			reason = $"Cookie with Domain={cookie.Domain} should not be sent to {requestUrl.Host}.";
			return false;
		}

		// https://tools.ietf.org/html/rfc6265#section-5.1.4
		private static bool IsPathMatch(FlurlCookie cookie, Url originUrl, Url requestUrl, out string reason) {
			reason = "ok";

			if (cookie.Path == "/")
				return true;
			
			var cookiePath = (cookie.Path?.StartsWith("/") == true) ? cookie.Path : originUrl.Path;
			if (cookiePath == "")
				cookiePath = "/";
			else if (cookiePath.Length > 1 && cookiePath.EndsWith("/"))
				cookiePath = cookiePath.TrimEnd('/');

			if (cookiePath == "/")
				return true;

			var requestPath = (requestUrl.Path.Length > 0) ? requestUrl.Path : "/";

			if (requestPath.Equals(cookiePath, StringComparison.Ordinal)) // Path is case-sensitive, unlike Domain
				return true;

			if (requestPath.StartsWith(cookiePath, StringComparison.Ordinal) && requestPath[cookiePath.Length] == '/')
				return true;

			reason = string.IsNullOrEmpty(cookie.Path) ?
				$"Cookie from path {cookiePath} should not be sent to path {requestUrl.Path}." :
				$"Cookie with Path={cookie.Path} should not be sent to path {requestUrl.Path}.";

			return false;
		}

		private static bool IsExpired(FlurlCookie cookie, out string reason) {
			// Max-Age takes precedence over Expires
			if (cookie.MaxAge.HasValue) {
				if (cookie.DateReceived.AddSeconds(cookie.MaxAge.Value) < DateTimeOffset.UtcNow) {
					reason = $"Cookie's Max-Age={cookie.MaxAge} (seconds) has expired.";
					return true;
				}
			}
			else if (cookie.Expires.HasValue && cookie.Expires < DateTimeOffset.UtcNow) {
				reason = $"Cookie with Expires={cookie.Expires} has expired.";
				return true;
			}
			reason = "ok";
			return false;
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

		// Possible future enhancement: https://github.com/tmenier/Flurl/issues/538
		// This method works, but the feature still needs caching of some kind and an opt-in config setting.
		private async Task<bool> IsPublicSuffixesAsync(string domain) {
			using (var stream = await "https://publicsuffix.org/list/public_suffix_list.dat".GetStreamAsync())
			using (var reader = new StreamReader(stream)) {
				while (true) {
					var line = await reader.ReadLineAsync();
					if (line == null) break;
					if (line.Trim() == "") continue;
					if (line.StartsWith("//")) continue;
					if (line == domain) return true;
				}
			}
			return false;
		}
	}
}
