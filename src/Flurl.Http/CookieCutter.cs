using System;
using System.Collections.Generic;
using System.Linq;
using Flurl.Util;

namespace Flurl.Http
{
	/// <summary>
	/// Utility class for parsing cookies from headers and vice-versa.
	/// </summary>
	public static class CookieCutter
	{
		/// <summary>
		/// Parses a Set-Cookie response header to a FlurlCookie.
		/// </summary>
		/// <param name="url">The URL that sent the response.</param>
		/// <param name="headerValue">Value of the Set-Cookie header.</param>
		/// <returns></returns>
		public static FlurlCookie FromResponseHeader(string url, string headerValue) {
			if (string.IsNullOrEmpty(headerValue)) return null;
			var pairs = (
				from part in headerValue.Split(';')
				let pair = part.SplitOnFirstOccurence("=")
				select new { Name = pair[0].Trim(), Value = pair.Last().Trim() });

			FlurlCookie cookie = null;
			foreach (var pair in pairs) {
				if (cookie == null)
					cookie = new FlurlCookie(pair.Name, pair.Value.Trim('"'), url, DateTimeOffset.UtcNow);

				// ordinal string compare is both safest and fastest
				// https://docs.microsoft.com/en-us/dotnet/standard/base-types/best-practices-strings#recommendations-for-string-usage
				else if (pair.Name.Equals("Expires", StringComparison.OrdinalIgnoreCase))
					cookie.Expires = DateTimeOffset.TryParse(pair.Value, out var d) ? d : (DateTimeOffset?)null;
				else if (pair.Name.Equals("Max-Age", StringComparison.OrdinalIgnoreCase))
					cookie.MaxAge = int.TryParse(pair.Value, out var i) ? i : (int?)null;
				else if (pair.Name.Equals("Domain", StringComparison.OrdinalIgnoreCase))
					cookie.Domain = pair.Value;
				else if (pair.Name.Equals("Path", StringComparison.OrdinalIgnoreCase))
					cookie.Path = pair.Value;
				else if (pair.Name.Equals("HttpOnly", StringComparison.OrdinalIgnoreCase))
					cookie.HttpOnly = true;
				else if (pair.Name.Equals("Secure", StringComparison.OrdinalIgnoreCase))
					cookie.Secure = true;
				else if (pair.Name.Equals("SameSite", StringComparison.OrdinalIgnoreCase))
					cookie.SameSite = pair.Value;
			}
			return cookie;
		}

		/// <summary>
		/// Creates a Cookie request header value from a key-value dictionary.
		/// </summary>
		/// <param name="values">Cookie values.</param>
		/// <returns>a header value if cookie values are present, otherwise null.</returns>
		public static string ToRequestHeader(IDictionary<string, object> values) {
			if (values?.Any() != true) return null;

			return string.Join("; ", values.Select(c =>
				$"{Url.Encode(c.Key)}={Url.Encode(c.Value.ToInvariantString())}"));
		}
	}
}
