using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http.Content;

namespace Flurl.Http
{
    public static class MultipartExtensions
    {
		/// <summary>
		/// Sends an asynchronous POST request.
		/// </summary>
		/// <param name="data">Contents of the request body.</param>
		/// <param name="client">The Flurl client.</param>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PostMultipartAsync(this FlurlClient client, object data, CancellationToken cancellationToken) {
			var content = new CapturedMultipartContent(data);
			return client.SendAsync(HttpMethod.Post, content: content, cancellationToken: cancellationToken);
		}

		/// <summary>
		/// Sends an asynchronous POST request.
		/// </summary>
		/// <param name="data">Contents of the request body.</param>
		/// <param name="client">The Flurl client.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PostMultipartAsync(this FlurlClient client, object data) {
			return client.PostMultipartAsync(data, CancellationToken.None);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous POST request.
		/// </summary>
		/// <param name="data">Contents of the request body.</param>
		/// <param name="url">The URL.</param>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PostMultipartAsync(this Url url, object data, CancellationToken cancellationToken) {
			return new FlurlClient(url, false).PostMultipartAsync(data, cancellationToken);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous POST request.
		/// </summary>
		/// <param name="data">Contents of the request body.</param>
		/// <param name="url">The URL.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PostMultipartAsync(this Url url, object data) {
			return new FlurlClient(url, false).PostMultipartAsync(data);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous POST request.
		/// </summary>
		/// <param name="data">Contents of the request body.</param>
		/// <param name="url">The URL.</param>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PostMultipartAsync(this string url, object data, CancellationToken cancellationToken) {
			return new FlurlClient(url, false).PostMultipartAsync(data, cancellationToken);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous POST request.
		/// </summary>
		/// <param name="data">Contents of the request body.</param>
		/// <param name="url">The URL.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PostMultipartAsync(this string url, object data) {
			return new FlurlClient(url, false).PostMultipartAsync(data);
		}
	}
}
