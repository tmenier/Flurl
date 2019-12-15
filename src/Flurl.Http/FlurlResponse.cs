using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Flurl.Http
{
	/// <summary>
	/// Represents an HTTP response.
	/// </summary>
	public interface IFlurlResponse : IDisposable
	{
		/// <summary>
		/// Gets the collection of response headers received.
		/// </summary>
		IDictionary<string, string> Headers { get; }

		/// <summary>
		/// Gets the collection of HttpCookies received from the server.
		/// </summary>
		IDictionary<string, Cookie> Cookies { get; }

		/// <summary>
		/// Gets the raw HttpResponseMessage that this IFlurlResponse wraps.
		/// </summary>
		HttpResponseMessage ResponseMessage { get; }

		/// <summary>
		/// Gets the status code of the response.
		/// </summary>
		int StatusCode { get; }

		/// <summary>
		/// Deserializes JSON-formatted HTTP response body to object of type T.
		/// </summary>
		/// <typeparam name="T">A type whose structure matches the expected JSON response.</typeparam>
		/// <returns>A Task whose result is an object containing data in the response body.</returns>
		/// <example>x = await url.PostAsync(data).GetJson&lt;T&gt;()</example>
		/// <exception cref="FlurlHttpException">Condition.</exception>
		Task<T> GetJsonAsync<T>();

		/// <summary>
		/// Deserializes JSON-formatted HTTP response body to a dynamic object.
		/// </summary>
		/// <returns>A Task whose result is a dynamic object containing data in the response body.</returns>
		/// <example>d = await url.PostAsync(data).GetJson()</example>
		/// <exception cref="FlurlHttpException">Condition.</exception>
		Task<dynamic> GetJsonAsync();

		/// <summary>
		/// Deserializes JSON-formatted HTTP response body to a list of dynamic objects.
		/// </summary>
		/// <returns>A Task whose result is a list of dynamic objects containing data in the response body.</returns>
		/// <example>d = await url.PostAsync(data).GetJsonList()</example>
		/// <exception cref="FlurlHttpException">Condition.</exception>
		Task<IList<dynamic>> GetJsonListAsync();

		/// <summary>
		/// Returns HTTP response body as a string.
		/// </summary>
		/// <returns>A Task whose result is the response body as a string.</returns>
		/// <example>s = await url.PostAsync(data).GetString()</example>
		Task<string> GetStringAsync();

		/// <summary>
		/// Returns HTTP response body as a stream.
		/// </summary>
		/// <returns>A Task whose result is the response body as a stream.</returns>
		/// <example>stream = await url.PostAsync(data).GetStream()</example>
		Task<Stream> GetStreamAsync();

		/// <summary>
		/// Returns HTTP response body as a byte array.
		/// </summary>
		/// <returns>A Task whose result is the response body as a byte array.</returns>
		/// <example>bytes = await url.PostAsync(data).GetBytes()</example>
		Task<byte[]> GetBytesAsync();
	}

	/// <inheritdoc />
	public class FlurlResponse : IFlurlResponse
	{
		private readonly Lazy<IDictionary<string, string>> _headers;

		/// <inheritdoc />
		public IDictionary<string, string> Headers => _headers.Value;

		/// <inheritdoc />
		public IDictionary<string, Cookie> Cookies { get; } = new Dictionary<string, Cookie>();

		/// <inheritdoc />
		public HttpResponseMessage ResponseMessage { get; }

		/// <inheritdoc />
		public int StatusCode => (int)ResponseMessage.StatusCode;

		/// <summary>
		/// Creates a new FlurlResponse that wraps the give HttpResponseMessage.
		/// </summary>
		/// <param name="resp"></param>
		public FlurlResponse(HttpResponseMessage resp) {
			ResponseMessage = resp;
			_headers = new Lazy<IDictionary<string, string>>(BuildHeaders);
		}

		private IDictionary<string, string> BuildHeaders() => ResponseMessage.Headers
			.Concat(ResponseMessage.Content?.Headers ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>())
			.GroupBy(h => h.Key)
			.ToDictionary(g => g.Key, g => string.Join(", ", g.SelectMany(h => h.Value)));

		/// <inheritdoc />
		public async Task<T> GetJsonAsync<T>() {
			var call = ResponseMessage.RequestMessage.GetHttpCall();
			using (var stream = await ResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false)) {
				try {
					return call.FlurlRequest.Settings.JsonSerializer.Deserialize<T>(stream);
				}
				catch (Exception ex) {
					var body = await ResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
					call.Exception = new FlurlParsingException(call, "JSON", body, ex);
					await FlurlRequest.HandleExceptionAsync(call, call.Exception, CancellationToken.None).ConfigureAwait(false);
					return default(T);
				}
			}
		}

		/// <inheritdoc />
		public async Task<dynamic> GetJsonAsync() {
			dynamic d = await GetJsonAsync<ExpandoObject>().ConfigureAwait(false);
			return d;
		}

		/// <inheritdoc />
		public async Task<IList<dynamic>> GetJsonListAsync() {
			dynamic[] d = await GetJsonAsync<ExpandoObject[]>().ConfigureAwait(false);
			return d;
		}

		/// <inheritdoc />
		public Task<string> GetStringAsync() {
#if NETSTANDARD1_3 || NETSTANDARD2_0
			// https://stackoverflow.com/questions/46119872/encoding-issues-with-net-core-2 (#86)
			System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
#endif
			return ResponseMessage.Content.StripCharsetQuotes().ReadAsStringAsync();
		}

		/// <inheritdoc />
		public Task<Stream> GetStreamAsync() {
			return ResponseMessage.Content.ReadAsStreamAsync();
		}

		/// <inheritdoc />
		public Task<byte[]> GetBytesAsync() {
			return ResponseMessage.Content.ReadAsByteArrayAsync();
		}

		/// <summary>
		/// Disposes the underlying HttpResponseMessage.
		/// </summary>
		public void Dispose() => ResponseMessage.Dispose();
	}
}
