using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http.Content;

namespace Flurl.Http
{
	/// <summary>
	/// Fluent extension methods for sending multipart/form-data requests.
	/// </summary>
	public static class MultipartExtensions
	{
		/// <summary>
		/// Sends an asynchronous multipart/form-data POST request.
		/// </summary>
		/// <param name="buildContent">A delegate for building the content parts.</param>
		/// <param name="request">The IFlurlRequest.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
		/// <returns>A Task whose result is the received IFlurlResponse.</returns>
		public static Task<IFlurlResponse> PostMultipartAsync(this IFlurlRequest request, Action<CapturedMultipartContent> buildContent, CancellationToken cancellationToken = default(CancellationToken)) {
			var cmc = new CapturedMultipartContent(request.Settings);
			buildContent(cmc);
			return request.SendAsync(HttpMethod.Post, cmc, cancellationToken);
		}
		
		/// <summary>
		/// Sends an asynchronous multipart/form-data PATCH request.
		/// </summary>
		/// <param name="buildContent">A delegate for building the content parts.</param>
		/// <param name="request">The IFlurlRequest.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
		/// <returns>A Task whose result is the received IFlurlResponse.</returns>
		public static Task<IFlurlResponse> PatchMultipartAsync(this IFlurlRequest request, Action<CapturedMultipartContent> buildContent, CancellationToken cancellationToken = default(CancellationToken)) {
			var cmc = new CapturedMultipartContent(request.Settings);
			buildContent(cmc);
			return request.SendAsync(new HttpMethod("PATCH"), cmc, cancellationToken);
		}
	}
}
