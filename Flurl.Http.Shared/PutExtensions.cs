using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http.Content;

namespace Flurl.Http
{
	public static class PutExtensions
	{
		/// <summary>
		/// Sends an asynchronous PUT request of specified data (usually an anonymous object or dictionary) formatted as JSON.
		/// </summary>
		/// <param name="data">Data to be serialized and putted.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PutJsonAsync(this FlurlClient client, object data) {
			return client.SendAsync(HttpMethod.Put, new CapturedJsonContent(data));
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous PUT request of specified data (usually an anonymous object or dictionary) formatted as JSON.
		/// </summary>
		/// <param name="data">Data to be serialized and putted.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PutJsonAsync(this string url, object data) {
			return new FlurlClient(url, true).PutJsonAsync(data);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous PUT request of specified data (usually an anonymous object or dictionary) formatted as JSON.
		/// </summary>
		/// <param name="data">Data to be serialized and putted.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PutJsonAsync(this Url url, object data) {
			return new FlurlClient(url, true).PutJsonAsync(data);
		}

		/// <summary>
		/// Sends an asynchronous PUT request of specified data (usually an anonymous object or dictionary) serialized as URL-encoded key/value pair.
		/// </summary>
		/// <param name="data">Data to be serialized and putted.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PutUrlEncodedAsync(this FlurlClient client, object data) {
			return client.SendAsync(HttpMethod.Put, new CapturedFormUrlEncodedContent(data));
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous PUT request of specified data (usually an anonymous object or dictionary) serialized as URL-encoded key/value pair.
		/// </summary>
		/// <param name="data">Data to be serialized and putted.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PutUrlEncodedAsync(this string url, object data) {
			return new FlurlClient(url, true).PutUrlEncodedAsync(data);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous PUT request of specified data (usually an anonymous object or dictionary) serialized as URL-encoded key/value pair.
		/// </summary>
		/// <param name="data">Data to be serialized and putted.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PutUrlEncodedAsync(this Url url, object data) {
			return new FlurlClient(url, true).PutUrlEncodedAsync(data);
		}
	}
}
