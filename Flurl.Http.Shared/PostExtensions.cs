using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http.Content;

namespace Flurl.Http
{
	public static class PostExtensions
	{
		/// <summary>
		/// Sends an asynchronous POST request of specified data (usually an anonymous object or dictionary) formatted as JSON.
		/// </summary>
		/// <param name="data">Data to be serialized and posted.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PostJsonAsync(this FlurlClient client, object data) {
			return client.SendAsync(HttpMethod.Post, new CapturedJsonContent(data));
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous POST request of specified data (usually an anonymous object or dictionary) formatted as JSON.
		/// </summary>
		/// <param name="data">Data to be serialized and posted.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PostJsonAsync(this string url, object data) {
			return new FlurlClient(url, true).PostJsonAsync(data);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous POST request of specified data (usually an anonymous object or dictionary) formatted as JSON.
		/// </summary>
		/// <param name="data">Data to be serialized and posted.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PostJsonAsync(this Url url, object data) {
			return new FlurlClient(url, true).PostJsonAsync(data);
		}

		/// <summary>
		/// Sends an asynchronous POST request of specified data (usually an anonymous object or dictionary) serialized as URL-encoded key/value pair (simulating a form post).
		/// </summary>
		/// <param name="data">Data to be serialized and posted.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PostUrlEncodedAsync(this FlurlClient client, object data) {
			return client.SendAsync(HttpMethod.Post, new CapturedFormUrlEncodedContent(data));
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous POST request of specified data (usually an anonymous object or dictionary) serialized as URL-encoded key/value pair (simulating a form post).
		/// </summary>
		/// <param name="data">Data to be serialized and posted.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PostUrlEncodedAsync(this string url, object data) {
			return new FlurlClient(url, true).PostUrlEncodedAsync(data);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous POST request of specified data (usually an anonymous object or dictionary) serialized as URL-encoded key/value pair (simulating a form post).
		/// </summary>
		/// <param name="data">Data to be serialized and posted.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PostUrlEncodedAsync(this Url url, object data) {
			return new FlurlClient(url, true).PostUrlEncodedAsync(data);
		}
	}
}
