using System;
using System.Collections.Generic;
using System.Linq;
using Flurl.Util;

namespace Flurl.Http
{
	/// <summary>
	/// Utility and extension methods for parsing and validating cookies.
	/// </summary>
	public static class CookieCutter
	{
		/// <summary>
		/// Parses a Cookie request header to name-value pairs.
		/// </summary>
		/// <param name="headerValue">Value of the Cookie request header.</param>
		/// <returns></returns>
		public static IEnumerable<(string Name, string Value)> ParseRequestHeader(string headerValue) {
			if (string.IsNullOrEmpty(headerValue)) yield break;

			foreach (var pair in GetPairs(headerValue))
				yield return (pair.Name, pair.Value);
		}

		/// <summary>
		/// Parses a Set-Cookie response header to a FlurlCookie.
		/// </summary>
		/// <param name="url">The URL that sent the response.</param>
		/// <param name="headerValue">Value of the Set-Cookie header.</param>
		/// <returns></returns>
		public static FlurlCookie ParseResponseHeader(string url, string headerValue) {
			if (string.IsNullOrEmpty(headerValue)) return null;

			FlurlCookie cookie = null;
			foreach (var pair in GetPairs(headerValue)) {
				if (cookie == null)
					cookie = new FlurlCookie(pair.Name, pair.Value.Trim('"'), url, DateTimeOffset.UtcNow);

				// ordinal string compare is both safest and fastest
				// https://docs.microsoft.com/en-us/dotnet/standard/base-types/best-practices-strings#recommendations-for-string-usage
				else if (pair.Name.OrdinalEquals("Expires", true))
					cookie.Expires = DateTimeOffset.TryParse(pair.Value, out var d) ? d : (DateTimeOffset?)null;
				else if (pair.Name.OrdinalEquals("Max-Age", true))
					cookie.MaxAge = int.TryParse(pair.Value, out var i) ? i : (int?)null;
				else if (pair.Name.OrdinalEquals("Domain", true))
					cookie.Domain = pair.Value;
				else if (pair.Name.OrdinalEquals("Path", true))
					cookie.Path = pair.Value;
				else if (pair.Name.OrdinalEquals("HttpOnly", true))
					cookie.HttpOnly = true;
				else if (pair.Name.OrdinalEquals("Secure", true))
					cookie.Secure = true;
				else if (pair.Name.OrdinalEquals("SameSite", true))
					cookie.SameSite = Enum.TryParse<SameSite>(pair.Value, true, out var val) ? val : (SameSite?)null;
			}
			return cookie;
		}

		/// <summary>
		/// Parses list of semicolon-delimited "name=value" pairs.
		/// </summary>
		private static IEnumerable<(string Name, string Value)> GetPairs(string list) =>
			from part in list.Split(';')
			let pair = part.SplitOnFirstOccurence("=")
			select (pair[0].Trim(), pair.Last().Trim());

		/// <summary>
		/// Creates a Cookie request header value from a list of cookie name-value pairs.
		/// </summary>
		/// <returns>A header value if cookie values are present, otherwise null.</returns>
		public static string ToRequestHeader(IEnumerable<(string Name, string Value)> cookies) {
			if (cookies?.Any() != true) return null;

			return string.Join("; ", cookies.Select(c =>
				$"{c.Name}={c.Value}"));
		}

		/// <summary>
		/// True if this cookie passes well-accepted rules for the Set-Cookie header. If false, provides a descriptive reason.
		/// </summary>
		public static bool IsValid(this FlurlCookie cookie, out string reason) {
			// TODO: validate name and value? https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Set-Cookie

			if (cookie.OriginUrl == null) {
				reason = "OriginUrl (URL that returned the original Set-Cookie header) is required in order to validate this cookie.";
				return false;
			}
			if (!Url.IsValid(cookie.OriginUrl)) {
				reason = $"OriginUrl {cookie.OriginUrl} is not a valid absolute URL.";
				return false;
			}
			if (cookie.Secure && !cookie.OriginUrl.IsSecureScheme) {
				reason = $"Secure cannot be true unless OriginUrl ({cookie.OriginUrl}) has a secure scheme (https).";
				return false;
			}

			if (!string.IsNullOrEmpty(cookie.Domain)) {
				if (cookie.Domain.IsIP()) {
					reason = "Domain cannot be an IP address.";
					return false;
				}
				if (cookie.OriginUrl.Host.IsIP()) {
					reason = "Domain cannot be set when origin URL is an IP address.";
					return false;
				}
				if (!cookie.Domain.Trim('.').OrdinalContains(".")) {
					reason = $"{cookie.Domain} is not a valid value for Domain.";
					return false;
				}
				var host = cookie.Domain.OrdinalStartsWith(".") ? cookie.Domain.Substring(1) : cookie.Domain;
				var fakeUrl = new Url("https://" + host);
				if (fakeUrl.IsRelative || fakeUrl.Host != host) {
					reason = $"{cookie.Domain} is not a valid Domain. A non-empty Domain must be a valid URI host (no scheme, path, port, etc).";
					return false;
				}
				if (!cookie.IsDomainMatch(cookie.OriginUrl, out reason)) {
					return false;
				}
			}

			// https://developer.mozilla.org/en-US/docs/Web/HTTP/Cookies#Cookie_prefixes

			if (cookie.Name.OrdinalStartsWith("__Host-")) {
				if (!cookie.OriginUrl.IsSecureScheme) {
					reason = "Cookie named with __Host- prefix must originate from a secure (https) domain.";
					return false;
				}
				if (!cookie.Secure) {
					reason = "Cookie named with __Host- prefix must contain the Secure attribute.";
					return false;
				}
				if (!string.IsNullOrEmpty(cookie.Domain)) {
					reason = "Cookie named with __Host- prefix must not contain the Domain attribute.";
					return false;
				}
				if (cookie.Path != "/") {
					reason = "Cookie named with __Host- prefix must contain the Path attribute with a value of '/'.";
					return false;
				}
			}
			if (cookie.Name.OrdinalStartsWith("__Secure-")) {
				if (!cookie.OriginUrl.IsSecureScheme) {
					reason = "Cookie named with __Secure- prefix must originate from a secure (https) domain.";
					return false;
				}
				if (!cookie.Secure) {
					reason = "Cookie named with __Secure- prefix must contain the Secure attribute.";
					return false;
				}
			}

			// it seems intuitive tht a non-empty path should start with /, but I can't find this in any spec
			//if (!string.IsNullOrEmpty(Path) && !Path.OrdinalStartsWith("/")) {
			//	reason = $"{Path} is not a valid Path. A non-empty Path must start with a / character.";
			//	return false;
			//}

			reason = "ok";
			return true;
		}

		/// <summary>
		/// True if this cookie is expired. If true, provides a descriptive reason (Expires or Max-Age).
		/// </summary>
		public static bool IsExpired(this FlurlCookie cookie, out string reason) {
			// Max-Age takes precedence over Expires
			if (cookie.MaxAge.HasValue) {
				if (cookie.MaxAge.Value <= 0 || cookie.DateReceived.AddSeconds(cookie.MaxAge.Value) < DateTimeOffset.UtcNow) {
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

		/// <summary>
		/// True if this cookie should be sent in a request to the given URL. If false, provides a descriptive reason.
		/// </summary>
		public static bool ShouldSendTo(this FlurlCookie cookie, Url requestUrl, out string reason) {
			if (cookie.Secure && !requestUrl.IsSecureScheme) {
				reason = $"Cookie is marked Secure and request URL is insecure ({requestUrl.Scheme}).";
				return false;
			}

			return
				cookie.IsValid(out reason) &&
				!cookie.IsExpired(out reason) &&
				IsDomainMatch(cookie, requestUrl, out reason) &&
				IsPathMatch(cookie, requestUrl, out reason);
		}

		private static bool IsDomainMatch(this FlurlCookie cookie, Url requestUrl, out string reason) {
			reason = "ok";

			if (!string.IsNullOrEmpty(cookie.Domain)) {
				var domain = cookie.Domain.OrdinalStartsWith(".") ? cookie.Domain.Substring(1) : cookie.Domain;
				if (requestUrl.Host.OrdinalEquals(domain, true))
					return true;

				if (requestUrl.Host.OrdinalEndsWith("." + domain, true))
					return true;

				reason = $"Cookie with Domain={cookie.Domain} should not be sent to {requestUrl.Host}.";
				return false;
			}
			else {
				if (requestUrl.Host.OrdinalEquals(cookie.OriginUrl.Host, true))
					return true;

				reason = $"Cookie set from {cookie.OriginUrl.Host} without Domain specified should only be sent to that specific host, not {requestUrl.Host}.";
				return false;
			}
		}

		private static bool IsPathMatch(this FlurlCookie cookie, Url requestUrl, out string reason) {
			reason = "ok";

			// implementation of default-path algorithm https://tools.ietf.org/html/rfc6265#section-5.1.4
			string GetDefaultPath() {
				var origPath = cookie.OriginUrl.Path;
				if (origPath == "" || origPath[0] != '/') return "/";
				if (origPath.Count(c => c == '/') <= 1) return "/";
				return origPath.Substring(0, origPath.LastIndexOf('/'));
			}

			// https://tools.ietf.org/html/rfc6265#section-5.2.4
			var cookiePath = (cookie.Path?.OrdinalStartsWith("/") == true) ? cookie.Path : GetDefaultPath();

			if (cookiePath.Length > 1 && cookiePath.OrdinalEndsWith("/"))
				cookiePath = cookiePath.TrimEnd('/');

			if (cookiePath == "/")
				return true;

			var requestPath = (requestUrl.Path.Length > 0) ? requestUrl.Path : "/";

			if (requestPath.OrdinalEquals(cookiePath)) // Path is case-sensitive, unlike Domain
				return true;

			if (requestPath.OrdinalStartsWith(cookiePath) && requestPath[cookiePath.Length] == '/')
				return true;

			reason = string.IsNullOrEmpty(cookie.Path) ?
				$"Cookie from path {cookiePath} should not be sent to path {requestUrl.Path}." :
				$"Cookie with Path={cookie.Path} should not be sent to path {requestUrl.Path}.";

			return false;
		}

		// Possible future enhancement: https://github.com/tmenier/Flurl/issues/538
		// This method works, but the feature still needs caching of some kind and an opt-in config setting.
		//private static async Task<bool> IsPublicSuffixesAsync(string domain) {
		//	using (var stream = await "https://publicsuffix.org/list/public_suffix_list.dat".GetStreamAsync())
		//	using (var reader = new StreamReader(stream)) {
		//		while (true) {
		//			var line = await reader.ReadLineAsync();
		//			if (line == null) break;
		//			if (line.Trim() == "") continue;
		//			if (line.OrdinalStartsWith("//")) continue;
		//			if (line == domain) return true;
		//		}
		//	}
		//	return false;
		//}
	}
}
