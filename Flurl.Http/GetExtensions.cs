using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Flurl.Http
{
	public static class GetExtensions
	{
		/// <summary>
		/// Sends an asynchronous GET request.
		/// </summary>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> GetAsync(this FlurlClient client) {
			return client.HttpClient.GetAsync(client.Url);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous GET request.
		/// </summary>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> GetAsync(this string url) {
			return new FlurlClient(url).GetAsync();
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous GET request.
		/// </summary>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> GetAsync(this Url url) {
			return new FlurlClient(url).GetAsync();
		}

		/// <summary>
		/// Sends an asynchronous GET request and deserializes the JSON-formatted response body to an object of type T.
		/// </summary>
		/// <typeparam name="T">A type whose structure matches the expected JSON response.</typeparam>
		/// <returns>A Task whose result is an object containing data in the response body.</returns>
		public static Task<T> GetJsonAsync<T>(this FlurlClient client) {
			return client.GetAsync().ReceiveJsonAsync<T>();
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous GET request and deserializes the JSON-formatted response body to an object of type T.
		/// </summary>
		/// <typeparam name="T">A type whose structure matches the expected JSON response.</typeparam>
		/// <returns>A Task whose result is an object containing data in the response body.</returns>
		public static Task<T> GetJsonAsync<T>(this string url) {
			return new FlurlClient(url).GetJsonAsync<T>();
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous GET request and deserializes the JSON-formatted response body to an object of type T.
		/// </summary>
		/// <typeparam name="T">A type whose structure matches the expected JSON response.</typeparam>
		/// <returns>A Task whose result is an object containing data in the response body.</returns>
		public static Task<T> GetJsonAsync<T>(this Url url) {
			return new FlurlClient(url).GetJsonAsync<T>();
		}

		/// <summary>
		/// Sends an asynchronous GET request and deserializes the JSON-formatted response body to a dynamic object.
		/// </summary>
		/// <returns>A Task whose result is a dynamic object containing data in the response body.</returns>
		public static Task<dynamic> GetJsonAsync(this FlurlClient client) {
			return client.GetAsync().ReceiveJsonAsync();
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous GET request and deserializes the JSON-formatted response body to a dynamic object.
		/// </summary>
		/// <returns>A Task whose result is a dynamic object containing data in the response body.</returns>
		public static Task<dynamic> GetJsonAsync(this string url) {
			return new FlurlClient(url).GetJsonAsync();
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous GET request and deserializes the JSON-formatted response body to a dynamic object.
		/// </summary>
		/// <returns>A Task whose result is a dynamic object containing data in the response body.</returns>
		public static Task<dynamic> GetJsonAsync(this Url url) {
			return new FlurlClient(url).GetJsonAsync();
		}

		/// <summary>
		/// Sends an asynchronous GET request and deserializes the JSON-formatted response body to a list of dynamic objects.
		/// </summary>
		/// <returns>A Task whose result is a list of dynamic objects containing data in the response body.</returns>
		public static Task<IList<dynamic>> GetJsonListAsync(this FlurlClient client) {
			return client.GetAsync().ReceiveJsonListAsync();
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous GET request and deserializes the JSON-formatted response body to a list of dynamic objects.
		/// </summary>
		/// <returns>A Task whose result is a list of dynamic objects containing data in the response body.</returns>
		public static Task<IList<dynamic>> GetJsonListAsync(this string url) {
			return new FlurlClient(url).GetJsonListAsync();
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous GET request and deserializes the JSON-formatted response body to a list of dynamic objects.
		/// </summary>
		/// <returns>A Task whose result is a list of dynamic objects containing data in the response body.</returns>
		public static Task<IList<dynamic>> GetJsonListAsync(this Url url) {
			return new FlurlClient(url).GetJsonListAsync();
		}

		/// <summary>
		/// Sends an asynchronous GET request and returns the response body as a string.
		/// </summary>
		/// <returns>A Task whose result is the response body.</returns>
		public static Task<string> GetStringAsync(this FlurlClient client) {
			return client.GetAsync().ReceiveStringAsync();
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous GET request and returns the response body as a string.
		/// </summary>
		/// <returns>A Task whose result is the response body.</returns>
		public static Task<string> GetStringAsync(this string url) {
			return new FlurlClient(url).GetStringAsync();
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous GET request and returns the response body as a string.
		/// </summary>
		/// <returns>A Task whose result is the response body.</returns>
		public static Task<string> GetStringAsync(this Url url) {
			return new FlurlClient(url).GetStringAsync();
		}
	}
}
