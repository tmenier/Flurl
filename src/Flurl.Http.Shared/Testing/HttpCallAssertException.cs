using System;
using System.Text;

namespace Flurl.Http.Testing
{
	/// <summary>
	/// An exception thrown by HttpTest's assertion methods to indicate that the assertion failed.
	/// </summary>
	public class HttpCallAssertException : Exception
	{
		public HttpCallAssertException(string urlPattern, int? expectedCalls, int actualCalls) : base(BuildMessage(urlPattern, expectedCalls, actualCalls)) { }

		private static string BuildMessage(string urlPattern, int? expectedCalls, int actualCalls) {
			if (expectedCalls == null)
				return string.Format("Expected call to {0} was not made.", urlPattern);

			return new StringBuilder()
				.Append("Expected ").Append(expectedCalls.Value)
				.Append(expectedCalls == 1 ? " call" : " calls")
				.Append(" to ").Append(urlPattern).Append(" but ").Append(actualCalls)
				.Append(actualCalls == 1 ? " call was made." : " calls were made.")
				.ToString();
		}
	}
}
