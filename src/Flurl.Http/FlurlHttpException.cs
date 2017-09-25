using System;
using System.Dynamic;
using System.Text;

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
		public HttpCall Call { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="FlurlHttpException"/> class.
		/// </summary>
		/// <param name="call">The call.</param>
		/// <param name="message">The message.</param>
		/// <param name="inner">The inner.</param>
		public FlurlHttpException(HttpCall call, string message, Exception inner) : base(message, inner) {
			Call = call;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="FlurlHttpException"/> class.
		/// </summary>
		/// <param name="call">The call.</param>
		/// <param name="inner">The inner.</param>
		public FlurlHttpException(HttpCall call, Exception inner) : this(call, BuildMessage(call, inner), inner) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="FlurlHttpException"/> class.
		/// </summary>
		/// <param name="call">The call.</param>
		public FlurlHttpException(HttpCall call) : this(call, BuildMessage(call, null), null) { }

		private static string BuildMessage(HttpCall call, Exception inner) {
			var sb = new StringBuilder();

			if (call.Response != null && !call.Succeeded)
				sb.AppendLine($"{call} failed with status code {(int)call.Response.StatusCode} ({call.Response.ReasonPhrase}).");
			else if (inner != null)
				sb.AppendLine($"{call} failed. {inner.Message}");
			else // in theory we should never get here.
				sb.AppendLine($"{call} failed.");

			if (!string.IsNullOrWhiteSpace(call.RequestBody))
				sb.AppendLine("Request body:").AppendLine(call.RequestBody);

			if (!string.IsNullOrWhiteSpace(call.ErrorResponseBody))
				sb.AppendLine("Response body:").AppendLine(call.ErrorResponseBody);

			return sb.ToString().Trim();
		}

		/// <summary>
		/// Gets the response body of the failed call.
		/// </summary>
		public string GetResponseString() {
			return Call?.ErrorResponseBody;
		}

		/// <summary>
		/// Deserializes the JSON response body to an object of the given type.
		/// </summary>
		/// <typeparam name="T">A type whose structure matches the expected JSON response.</typeparam>
		/// <returns>An object containing data in the response body.</returns>
		public T GetResponseJson<T>() {
			return
				Call?.ErrorResponseBody == null ? default(T) :
				Call?.FlurlRequest?.Settings?.JsonSerializer == null ? default(T) :
				Call.FlurlRequest.Settings.JsonSerializer.Deserialize<T>(Call.ErrorResponseBody);
		}

		/// <summary>
		/// Deserializes the JSON response body to a dynamic object.
		/// </summary>
		/// <returns>An object containing data in the response body.</returns>
		public dynamic GetResponseJson() {
			return GetResponseJson<ExpandoObject>();
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
		/// <param name="call">The call.</param>
		/// <param name="inner">The inner.</param>
		public FlurlHttpTimeoutException(HttpCall call, Exception inner) : base(call, BuildMessage(call), inner) { }

		private static string BuildMessage(HttpCall call) {
			return $"{call} timed out.";
		}
	}
}