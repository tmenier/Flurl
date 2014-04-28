using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using Flurl.Common;
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
			return client.HttpClient.PostAsync(client.Url, new CapturedJsonContent(data));
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous POST request of specified data (usually an anonymous object or dictionary) formatted as JSON.
		/// </summary>
		/// <param name="data">Data to be serialized and posted.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PostJsonAsync(this string url, object data) {
			return new FlurlClient(url).PostJsonAsync(data);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous POST request of specified data (usually an anonymous object or dictionary) formatted as JSON.
		/// </summary>
		/// <param name="data">Data to be serialized and posted.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PostJsonAsync(this Url url, object data) {
			return new FlurlClient(url).PostJsonAsync(data);
		}

		/// <summary>
		/// Sends an asynchronous POST request of specified data (usually an anonymous object or dictionary) serialized as URL-encoded key/value pair (simulating a form post).
		/// </summary>
		/// <param name="data">Data to be serialized and posted.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PostUrlEncodedAsync(this FlurlClient client, object data) {
			return client.HttpClient.PostAsync(client.Url, new CapturedFormUrlEncodedContent(data));
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous POST request of specified data (usually an anonymous object or dictionary) serialized as URL-encoded key/value pair (simulating a form post).
		/// </summary>
		/// <param name="data">Data to be serialized and posted.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PostUrlEncodedAsync(this string url, object data) {
			return new FlurlClient(url).PostUrlEncodedAsync(data);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous POST request of specified data (usually an anonymous object or dictionary) serialized as URL-encoded key/value pair (simulating a form post).
		/// </summary>
		/// <param name="data">Data to be serialized and posted.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PostUrlEncodedAsync(this Url url, object data) {
			return new FlurlClient(url).PostUrlEncodedAsync(data);
		}
	}
}
