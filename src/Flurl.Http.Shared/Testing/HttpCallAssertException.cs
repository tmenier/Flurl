using System;
using System.Collections.Generic;
using System.Linq;

namespace Flurl.Http.Testing
{
	/// <summary>
	/// An exception thrown by HttpTest's assertion methods to indicate that the assertion failed.
	/// </summary>
	public class HttpCallAssertException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="HttpCallAssertException"/> class.
		/// </summary>
		/// <param name="conditions">The expected call conditions.</param>
		/// <param name="expectedCalls">The expected number of calls.</param>
		/// <param name="actualCalls">The actual number calls.</param>
		public HttpCallAssertException(IList<string> conditions, int? expectedCalls, int actualCalls) : base(BuildMessage(conditions, expectedCalls, actualCalls)) { }

		private static string BuildMessage(IList<string> conditions, int? expectedCalls, int actualCalls) {
			var expected =
				(expectedCalls == null) ? "any calls to be made" :
				(expectedCalls == 0) ? "no calls to be made" :
				(expectedCalls == 1) ? "1 call to be made" :
				expectedCalls + " calls to be made";
			var actual =
				(actualCalls == 0) ? "no matching calls were made" :
				(actualCalls == 1) ? "1 matching call was made" :
				actualCalls + " matching calls were made";
			if (conditions.Any())
				expected += " with " + string.Join(" and ", conditions);
			else
				actual = actual.Replace(" matching", "");
			return $"Expected {expected}, but {actual}.";
		}
	}
}