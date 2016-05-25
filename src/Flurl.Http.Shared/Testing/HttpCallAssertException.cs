using System;
using System.Text;

namespace Flurl.Http.Testing
{
	/// <summary>
	/// An exception thrown by HttpTest's assertion methods to indicate that the assertion failed.
	/// </summary>
	[Serializable]
	public class HttpCallAssertException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="HttpCallAssertException"/> class.
		/// </summary>
		/// <param name="urlPattern">The URL pattern.</param>
		/// <param name="expectedCalls">The expected calls.</param>
		/// <param name="actualCalls">The actual calls.</param>
		public HttpCallAssertException(string urlPattern, int? expectedCalls, int actualCalls) : base(BuildMessage(urlPattern, expectedCalls, actualCalls)) { }

		private static string BuildMessage(string urlPattern, int? expectedCalls, int actualCalls) {
			if (expectedCalls == null)
				return $"Expected call to {urlPattern} was not made.";

			return new StringBuilder()
				.Append("Expected ").Append(expectedCalls.Value)
				.Append(expectedCalls == 1 ? " call" : " calls")
				.Append(" to ").Append(urlPattern).Append(" but ").Append(actualCalls)
				.Append(actualCalls == 1 ? " call was made." : " calls were made.")
				.ToString();
		}
	}
}