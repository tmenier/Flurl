using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Util;

namespace Flurl.Http
{
	/// <summary>
	/// Extension methods off HttpResponseMessage, and async extension methods off Task&lt;IFlurlResponse&gt;
	/// that allow chaining off methods like SendAsync without the need for nested awaits.
	/// </summary>
	public static class HttpResponseMessageExtensions
	{
		/// <summary>
		/// Deserializes JSON-formatted HTTP response body to object of type T. Intended to chain off an async HTTP.
		/// </summary>
		/// <typeparam name="T">A type whose structure matches the expected JSON response.</typeparam>
		/// <returns>A Task whose result is an object containing data in the response body.</returns>
		/// <example>x = await url.PostAsync(data).ReceiveJson&lt;T&gt;()</example>
		/// <exception cref="FlurlHttpException">Condition.</exception>
		public static async Task<T> ReceiveJson<T>(this Task<IFlurlResponse> response) {
			using (var resp = await response.ConfigureAwait(false)) {
				if (resp == null) return default(T);
				return await resp.GetJsonAsync<T>().ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Deserializes JSON-formatted HTTP response body to a dynamic object. Intended to chain off an async call.
		/// </summary>
		/// <returns>A Task whose result is a dynamic object containing data in the response body.</returns>
		/// <example>d = await url.PostAsync(data).ReceiveJson()</example>
		/// <exception cref="FlurlHttpException">Condition.</exception>
		public static async Task<dynamic> ReceiveJson(this Task<IFlurlResponse> response) {
			using (var resp = await response.ConfigureAwait(false)) {
				if (resp == null) return null;
				return await resp.GetJsonAsync().ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Deserializes JSON-formatted HTTP response body to a list of dynamic objects. Intended to chain off an async call.
		/// </summary>
		/// <returns>A Task whose result is a list of dynamic objects containing data in the response body.</returns>
		/// <example>d = await url.PostAsync(data).ReceiveJsonList()</example>
		/// <exception cref="FlurlHttpException">Condition.</exception>
		public static async Task<IList<dynamic>> ReceiveJsonList(this Task<IFlurlResponse> response) {
			using (var resp = await response.ConfigureAwait(false)) {
				if (resp == null) return null;
				return await resp.GetJsonListAsync().ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Returns HTTP response body as a string. Intended to chain off an async call.
		/// </summary>
		/// <returns>A Task whose result is the response body as a string.</returns>
		/// <example>s = await url.PostAsync(data).ReceiveString()</example>
		public static async Task<string> ReceiveString(this Task<IFlurlResponse> response) {
			using (var resp = await response.ConfigureAwait(false)) {
				if (resp == null) return null;
				return await resp.GetStringAsync().ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Returns HTTP response body as a stream. Intended to chain off an async call.
		/// </summary>
		/// <returns>A Task whose result is the response body as a stream.</returns>
		/// <example>stream = await url.PostAsync(data).ReceiveStream()</example>
		public static async Task<Stream> ReceiveStream(this Task<IFlurlResponse> response) {
			// don't wrap in a using, otherwise we'll dispose the stream too early.
			// we can dispose it if there's an error, otherwise the user is on the hook for it.
			var resp = await response.ConfigureAwait(false);
			if (resp == null) return null;
			try {
				return await resp.GetStreamAsync().ConfigureAwait(false);
			}
			catch (Exception) {
				resp.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Returns HTTP response body as a byte array. Intended to chain off an async call.
		/// </summary>
		/// <returns>A Task whose result is the response body as a byte array.</returns>
		/// <example>bytes = await url.PostAsync(data).ReceiveBytes()</example>
		public static async Task<byte[]> ReceiveBytes(this Task<IFlurlResponse> response) {
			using (var resp = await response.ConfigureAwait(false)) {
				if (resp == null) return null;
				return await resp.GetBytesAsync().ConfigureAwait(false);
			}
		}

		/// <summary>
		/// Set a header on this HttpResponseMessage (default), or its Content property if it's a known content-level header.
		/// No validation. Overwrites any existing value(s) for the header. 
		/// </summary>
		/// <param name="response">The HttpResponseMessage.</param>
		/// <param name="name">The header name.</param>
		/// <param name="value">The header value.</param>
		/// <param name="createContentIfNecessary">If it's a content-level header and there is no content, this determines whether to create an empty HttpContent or just ignore the header.</param>
		public static void SetHeader(this HttpResponseMessage response, string name, object value, bool createContentIfNecessary = true) {
			new HttpMessage(response).SetHeader(name, value, createContentIfNecessary);
		}

		/// <summary>
		/// Gets the value of a header on this HttpResponseMessage (default), or its Content property.
		/// Returns null if the header doesn't exist.
		/// </summary>
		/// <param name="response">The HttpResponseMessage.</param>
		/// <param name="name">The header name.</param>
		/// <returns>The header value.</returns>
		public static string GetHeaderValue(this HttpResponseMessage response, string name) {
			return new HttpMessage(response).GetHeaderValue(name);
		}

		// https://github.com/tmenier/Flurl/pull/76, https://github.com/dotnet/corefx/issues/5014
		internal static HttpContent StripCharsetQuotes(this HttpContent content) {
			var header = content?.Headers?.ContentType;
			if (header?.CharSet != null)
				header.CharSet = header.CharSet.StripQuotes();
			return content;
		}
	}
}