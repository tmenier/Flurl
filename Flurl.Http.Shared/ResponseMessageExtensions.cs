using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Flurl.Http
{
	/// <summary>
	/// Async extension methods that can be chained off Task&lt;HttpResponseMessage&gt;, avoiding nested awaits.
	/// </summary>
	public static class ResponseMessageExtensions
	{
		/// <summary>
		/// Deserializes JSON-formatted HTTP response body to object of type T. Intended to chain off an async HTTP.
		/// </summary>
 		/// <typeparam name="T">A type whose structure matches the expected JSON response.</typeparam>
		/// <returns>A Task whose result is an object containing data in the response body.</returns>
		/// <example>x = await url.PostAsync(data).ReceiveJson&lt;T&gt;()</example>
		public static async Task<T> ReceiveJson<T>(this Task<HttpResponseMessage> response) {
			using (var stream = await response.ReceiveStream())
				return JsonSerializer.CreateDefault().Deserialize<T>(stream);
		}

		/// <summary>
		/// Deserializes JSON-formatted HTTP response body to a dynamic object. Intended to chain off an async call.
		/// </summary>
		/// <returns>A Task whose result is a dynamic object containing data in the response body.</returns>
		/// <example>d = await url.PostAsync(data).ReceiveJson()</example>
		public static async Task<dynamic> ReceiveJson(this Task<HttpResponseMessage> response) {
			return await response.ReceiveJson<ExpandoObject>();
		}

		/// <summary>
		/// Deserializes JSON-formatted HTTP response body to a list of dynamic objects. Intended to chain off an async call.
		/// </summary>
		/// <returns>A Task whose result is a list of dynamic objects containing data in the response body.</returns>
		/// <example>d = await url.PostAsync(data).ReceiveJsonList()</example>
		public static async Task<IList<dynamic>> ReceiveJsonList(this Task<HttpResponseMessage> response) {
			dynamic[] d = await response.ReceiveJson<ExpandoObject[]>();
			return d;
		}

		/// <summary>
		/// Returns HTTP response body as a string. Intended to chain off an async call.
		/// </summary>
		/// <returns>A Task whose result is the response body as a string.</returns>
		/// <example>s = await url.PostAsync(data).ReceiveString()</example>
		public static async Task<string> ReceiveString(this Task<HttpResponseMessage> response) {
			return await (await response).Content.ReadAsStringAsync();
		}

		/// <summary>
		/// Returns HTTP response body as a stream. Intended to chain off an async call.
		/// </summary>
		/// <returns>A Task whose result is the response body as a stream.</returns>
		/// <example>stream = await url.PostAsync(data).ReceiveStream()</example>
		public static async Task<Stream> ReceiveStream(this Task<HttpResponseMessage> response) {
			return await (await response).Content.ReadAsStreamAsync();
		}

		/// <summary>
		/// Returns HTTP response body as a byte array. Intended to chain off an async call.
		/// </summary>
		/// <returns>A Task whose result is the response body as a byte array.</returns>
		/// <example>bytes = await url.PostAsync(data).ReceiveBytes()</example>
		public static async Task<byte[]> ReceiveBytes(this Task<HttpResponseMessage> response) {
			return await (await response).Content.ReadAsByteArrayAsync();
		}
	}
}
