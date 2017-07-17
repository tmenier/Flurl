using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace Flurl.Http
{
	/// <summary>
	/// The status range parser class.
	/// </summary>
	public static class HttpStatusRangeParser
	{
		/// <summary>
		/// Determines whether the specified pattern is match.
		/// </summary>
		/// <param name="pattern">The pattern.</param>
		/// <param name="value">The value.</param>
		/// <exception cref="ArgumentException">pattern is invalid.</exception>
		public static bool IsMatch(string pattern, HttpStatusCode value) {
			return IsMatch(pattern, (int)value);
		}

		/// <summary>
		/// Determines whether the specified pattern is match.
		/// </summary>
		/// <param name="pattern">The pattern.</param>
		/// <param name="value">The value.</param>
		/// <exception cref="ArgumentException"><paramref name="pattern"/> is invalid.</exception>
		public static bool IsMatch(string pattern, int value) {
			if (pattern == null)
				return false;

			foreach (var range in pattern.Split(',').Select(p => p.Trim())) {
				if (range == "")
					continue;

				if (range == "*")
					return true; // special case - allow everything

				var bounds = range.Split('-');
				int lower = 0, upper = 0;

				var valid =
					bounds.Length <= 2 &&
					int.TryParse(Regex.Replace(bounds.First().Trim(), "[*xX]", "0"), out lower) &&
					int.TryParse(Regex.Replace(bounds.Last().Trim(), "[*xX]", "9"), out upper);

				if (!valid) {
					throw new ArgumentException(
						$"Invalid range pattern: \"{pattern}\". Examples of allowed patterns: \"400\", \"4xx\", \"300,400-403\", \"*\".");
				}

				if (value >= lower && value <= upper)
					return true;
			}
			return false;
		}
	}
}