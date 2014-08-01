using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Rackspace.Threading;

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
		public static Task<T> ReceiveJson<T>(this Task<HttpResponseMessage> response) {
			return TaskBlocks.Using(() => response.Then(task => task.Result.Content.ReadAsStreamAsync()),
				streamTask => CompletedTask.FromResult(JsonHelper.ReadJsonFromStream<T>(streamTask.Result)));
		}

		/// <summary>
		/// Deserializes JSON-formatted HTTP response body to a dynamic object. Intended to chain off an async call.
		/// </summary>
		/// <returns>A Task whose result is a dynamic object containing data in the response body.</returns>
		/// <example>d = await url.PostAsync(data).ReceiveJson()</example>
		public static Task<dynamic> ReceiveJson(this Task<HttpResponseMessage> response) {
			return response.ReceiveJson<ExpandoObject>().Select(task => (dynamic)task.Result);
		}

		/// <summary>
		/// Deserializes JSON-formatted HTTP response body to a list of dynamic objects. Intended to chain off an async call.
		/// </summary>
		/// <returns>A Task whose result is a list of dynamic objects containing data in the response body.</returns>
		/// <example>d = await url.PostAsync(data).ReceiveJsonList()</example>
		public static Task<IList<dynamic>> ReceiveJsonList(this Task<HttpResponseMessage> response) {
			return response.ReceiveJson<ExpandoObject[]>().Select(task => {
				dynamic[] d = task.Result;
				return (IList<dynamic>)d;
			});
		}

		/// <summary>
		/// Returns HTTP response body as a string. Intended to chain off an async call.
		/// </summary>
		/// <returns>A Task whose result is the response body as a string.</returns>
		/// <example>s = await url.PostAsync(data).ReceiveString()</example>
		public static Task<string> ReceiveString(this Task<HttpResponseMessage> response) {
			return response.Then(task => task.Result.Content.ReadAsStringAsync());
		}

		/// <summary>
		/// Returns HTTP response body as a stream. Intended to chain off an async call.
		/// </summary>
		/// <returns>A Task whose result is the response body as a stream.</returns>
		/// <example>stream = await url.PostAsync(data).ReceiveStream()</example>
		public static Task<Stream> ReceiveStream(this Task<HttpResponseMessage> response) {
			return response.Then(task => task.Result.Content.ReadAsStreamAsync());
		}

		/// <summary>
		/// Returns HTTP response body as a byte array. Intended to chain off an async call.
		/// </summary>
		/// <returns>A Task whose result is the response body as a byte array.</returns>
		/// <example>bytes = await url.PostAsync(data).ReceiveBytes()</example>
		public static Task<byte[]> ReceiveBytes(this Task<HttpResponseMessage> response) {
			return response.Then(task => task.Result.Content.ReadAsByteArrayAsync());
		}
	}
}
