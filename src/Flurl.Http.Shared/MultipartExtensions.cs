using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http.Content;

namespace Flurl.Http
{
	/// <summary>
	/// MultipartExtensions
	/// </summary>
	public static class MultipartExtensions
	{
		/// <summary>
		/// Sends an asynchronous multipart/form-data POST request.
		/// </summary>
		/// <param name="buildContent">A delegate for building the content parts.</param>
		/// <param name="client">The Flurl client.</param>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PostMultipartAsync(this FlurlClient client, Action<CapturedMultipartContent> buildContent, CancellationToken cancellationToken = default(CancellationToken)) {
			var cmc = new CapturedMultipartContent(client.Settings);
			buildContent(cmc);
			return client.SendAsync(HttpMethod.Post, cmc, cancellationToken);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous multipart/form-data POST request.
		/// </summary>
		/// <param name="buildContent">A delegate for building the content parts.</param>
		/// <param name="url">The URL.</param>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PostMultipartAsync(this Url url, Action<CapturedMultipartContent> buildContent, CancellationToken cancellationToken = default(CancellationToken)) {
			return new FlurlClient(url, false).PostMultipartAsync(buildContent, cancellationToken);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous multipart/form-data POST request.
		/// </summary>
		/// <param name="buildContent">A delegate for building the content parts.</param>
		/// <param name="url">The URL.</param>
		/// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> PostMultipartAsync(this string url, Action<CapturedMultipartContent> buildContent, CancellationToken cancellationToken = default(CancellationToken)) {
			return new FlurlClient(url, false).PostMultipartAsync(buildContent, cancellationToken);
		}
	}
}
