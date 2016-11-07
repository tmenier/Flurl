using Flurl.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flurl
{
	/// <summary>
	/// Represents a Single Page Application URL that can be built fluently
	/// </summary>
	public class SpaUrl : Url
	{
		/// <summary>
		/// Constructs a Single Page Application style URL from a string.
		/// </summary>
		/// <param name="baseUrl">The URL to use as a starting point (required)</param>
		/// <exception cref="ArgumentNullException"><paramref name="baseUrl"/> is <see langword="null" />.</exception>
		public SpaUrl(string baseUrl) : base(baseUrl)
		{
			if (baseUrl == null) { throw new ArgumentNullException(nameof(baseUrl)); }

			var separatorCount = baseUrl.Length - baseUrl.Replace("#", "").Length;

			var parts = new string[] { baseUrl };

			// Treat the last occurence of '#' as an anchor tag
			if (separatorCount > 1)
				parts = baseUrl.SplitOnLastOccurence('#');

			Fragment = (parts.Length == 2) ? parts[1] : "";
			parts = parts[0].SplitOnFirstOccurence('?');
			Query = (parts.Length == 2) ? parts[1] : "";
			Path = parts[0];
		}

		/// <summary>
		/// Basically a Path.Combine for Single Page ApplicationURLs. Ensure exactly one '/'
		/// separates each segment and exactly one '&amp;' separates each query parameter.
		/// Differs from <see cref="Url.Combine(string[])"/> by ignoring fragments. 
		/// </summary>
		/// <param name="parts">URL parts to combine.</param>
		/// <returns></returns>
		public new static string Combine(params string[] parts)
		{
			if (parts == null)
				throw new ArgumentNullException(nameof(parts));

			string result = "";
			bool inQuery = false, inFragment = false;

			// When more than one fragment part, treat the last part with a '#'
			// as the true fragment
			var fragmentIndex = -1;
			if (parts.Where(p => p != null).Count(p => p.Contains("#")) > 1)
				fragmentIndex = Array.LastIndexOf(parts, parts.Last(p => p.Contains("#")));
			
			for (var partIndex = 0; partIndex < parts.Length; partIndex ++)
			{
				var part = parts[partIndex];

				if (string.IsNullOrEmpty(part))
					continue;

				if (partIndex == fragmentIndex)
					result = CombineEnsureSingleSeperator(result, part, '#');
				else if (result.EndsWith("?") || part.StartsWith("?"))
					result = CombineEnsureSingleSeperator(result, part, '?');
				else if (inFragment)
					result += part;
				else if (inQuery)
					result = CombineEnsureSingleSeperator(result, part, '&');
				else
					result = CombineEnsureSingleSeperator(result, part, '/');

				if (partIndex == fragmentIndex)
				{
					inQuery = false;
					inFragment = true;
				}
				else if (!inFragment && part.Contains("?"))
				{
					inQuery = true;
				}
			}
			return EncodeIllegalCharacters(result);
		}

		/// <summary>
		/// Checks if a string is a well-formed single page application URL.
		/// </summary>
		/// <remarks>
		/// This check is very opiniated on what an SPA url might look like.
		/// </remarks>
		/// <param name="url">The string to check</param>
		/// <returns>True is string is a well-formed URL</returns>
		public new static bool IsValid(string url)
		{
			// Very opinionated removal of # symbols used for routing
			// in single page applications
			var spaUrl = url.Replace("/#/", "/");
			return Url.IsValid(spaUrl);
		}

		/// <summary>
		/// Checks if this URL is well formed.
		/// </summary>
		/// <returns>true if the is a well formed URL.</returns>
		public new bool IsValid() => IsValid(ToString());
	}
}
