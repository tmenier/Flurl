using System;
using System.Threading.Tasks;

namespace Flurl.Http
{
	/// <summary>
	/// An exception that is thrown when an HTTP call made by Flurl.Http fails, including when the response
	/// indicates an unsuccessful HTTP status code.
	/// </summary>
	public class FlurlHttpException : Exception
	{
		/// <summary>
		/// An object containing details about the failed HTTP call
		/// </summary>
		public FlurlCall Call { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FlurlHttpException"/> class.
		/// </summary>
		/// <param name="call">The call.</param>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner.</param>
		public FlurlHttpException(FlurlCall call, string message, Exception inner) : base(message, inner) {
			Call = call;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FlurlHttpException"/> class.
		/// </summary>
		/// <param name="call">The call.</param>
		/// <param name="inner">The inner.</param>
		public FlurlHttpException(FlurlCall call, Exception inner) : this(call, BuildMessage(call, inner), inner) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="FlurlHttpException"/> class.
		/// </summary>
		/// <param name="call">The call.</param>
		public FlurlHttpException(FlurlCall call) : this(call, BuildMessage(call, null), null) { }

		private static string BuildMessage(FlurlCall call, Exception inner) {
			return
				(call.Response != null && !call.Succeeded) ?
				$"Call failed with status code {call.Response.StatusCode} ({call.HttpResponseMessage.ReasonPhrase}): {call}":
				$"Call failed. {inner?.Message} {call}";
		}
	}

	/// <summary>
	/// An exception that is thrown when an HTTP call made by Flurl.Http times out.
	/// </summary>
	public class FlurlHttpTimeoutException : FlurlHttpException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FlurlHttpTimeoutException"/> class.
		/// </summary>
		/// <param name="call">Details of the HTTP call that caused the exception.</param>
		/// <param name="inner">The inner exception.</param>
		public FlurlHttpTimeoutException(FlurlCall call, Exception inner) : base(call, BuildMessage(call), inner) { }

		private static string BuildMessage(FlurlCall call) {
			return $"Call timed out: {call}";
		}
	}

	/// <summary>
	/// An exception that is thrown when an HTTP response could not be parsed to a particular format.
	/// </summary>
	public class FlurlParsingException : FlurlHttpException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Flurl.Http.FlurlParsingException"/> class.
		/// </summary>
		/// <param name="call">Details of the HTTP call that caused the exception.</param>
		/// <param name="expectedFormat">The format that could not be parsed to, i.e. JSON.</param>
		/// <param name="inner">The inner exception.</param>
		public FlurlParsingException(FlurlCall call, string expectedFormat, Exception inner) : base(call, BuildMessage(call, expectedFormat), inner) {
			ExpectedFormat = expectedFormat;
		}

		/// <summary>
		/// The format that could not be parsed to, i.e. JSON.
		/// </summary>
		public string ExpectedFormat { get; }

		private static string BuildMessage(FlurlCall call, string expectedFormat) {
			return $"Response could not be deserialized to {expectedFormat}: {call}";
		}
	}

}