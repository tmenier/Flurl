using System;
using System.Dynamic;
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
			return (call.Response != null && !call.Succeeded) ?
				$"{call} failed with status code {(int)call.Response.StatusCode} ({call.Response.ReasonPhrase}).":
				$"{call} failed. {inner?.Message}".Trim();
		}

		/// <summary>
		/// Gets the response body of the failed call.
		/// </summary>
		/// <returns>A task whose result is the string contents of the response body.</returns>
		public async Task<string> GetResponseStringAsync() {
			var task = Call?.Response?.Content?.ReadAsStringAsync();
			return (task == null) ? null : await task.ConfigureAwait(false);
		}

		/// <summary>
		/// Deserializes the JSON response body to an object of the given type.
		/// </summary>
		/// <typeparam name="T">A type whose structure matches the expected JSON response.</typeparam>
		/// <returns>A task whose result is an object containing data in the response body.</returns>
		public async Task<T> GetResponseJsonAsync<T>() {
			var task = Call?.Response?.Content?.ReadAsStreamAsync();
			if (task == null) return default(T);
			var ser = Call.FlurlRequest?.Settings?.JsonSerializer;
			if (ser == null) return default(T);
			return ser.Deserialize<T>(await task.ConfigureAwait(false));
		}

		/// <summary>
		/// Deserializes the JSON response body to a dynamic object.
		/// </summary>
		/// <returns>A task whose result is an object containing data in the response body.</returns>
		public async Task<dynamic> GetResponseJsonAsync() => await GetResponseJsonAsync<ExpandoObject>().ConfigureAwait(false);
	}

	/// <summary>
	/// An exception that is thrown when an HTTP call made by Flurl.Http times out.
	/// </summary>
	public class FlurlHttpTimeoutException : FlurlHttpException
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FlurlHttpTimeoutException"/> class.
		/// </summary>
		/// <param name="call">The HttpCall instance.</param>
		/// <param name="inner">The inner exception.</param>
		public FlurlHttpTimeoutException(HttpCall call, Exception inner) : base(call, BuildMessage(call), inner) { }

		private static string BuildMessage(HttpCall call) {
			return $"{call} timed out.";
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
		/// <param name="call">The HttpCall instance.</param>
		/// <param name="expectedFormat">The format that could not be parsed to, i.e. JSON.</param>
		/// <param name="inner">The inner exception.</param>
		public FlurlParsingException(HttpCall call, string expectedFormat, Exception inner) : base(call, BuildMessage(call, expectedFormat), inner) {
			ExpectedFormat = expectedFormat;
		}

		/// <summary>
		/// The format that could not be parsed to, i.e. JSON.
		/// </summary>
		public string ExpectedFormat { get; }

		private static string BuildMessage(HttpCall call, string expectedFormat) {
			return $"Response from {call} could not be deserialized to {expectedFormat}.";
		}
	}

}