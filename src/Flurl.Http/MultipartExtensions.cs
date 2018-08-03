using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http.Content;

namespace Flurl.Http
{
    /// <summary>
    /// Fluent extension menthods for sending multipart/form-data requests.
    /// </summary>
    public static class MultipartExtensions
    {
        /// <summary>
        /// Sends an asynchronous multipart/form-data POST request.
        /// </summary>
        /// <param name="buildContent">A delegate for building the content parts.</param>
        /// <param name="request">The IFlurlRequest.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A Task whose result is the received HttpResponseMessage.</returns>
        public static Task<HttpResponseMessage> PostMultipartAsync(this IFlurlRequest request, Action<CapturedMultipartContent> buildContent, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cmc = new CapturedMultipartContent(request.Settings);
            buildContent(cmc);
            return request.SendAsync(HttpMethod.Post, cmc, cancellationToken);
        }

        /// <summary>
        /// Creates a FlurlRequest from the URL and sends an asynchronous multipart/form-data POST request.
        /// </summary>
        /// <param name="buildContent">A delegate for building the content parts.</param>
        /// <param name="url">The URL.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A Task whose result is the received HttpResponseMessage.</returns>
        public static Task<HttpResponseMessage> PostMultipartAsync(this Url url, Action<CapturedMultipartContent> buildContent, CancellationToken cancellationToken = default(CancellationToken))
        {
            return new FlurlRequest(url).PostMultipartAsync(buildContent, cancellationToken);
        }

        /// <summary>
        /// Creates a FlurlRequest from the URL and sends an asynchronous multipart/form-data POST request.
        /// </summary>
        /// <param name="buildContent">A delegate for building the content parts.</param>
        /// <param name="url">The URL.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A Task whose result is the received HttpResponseMessage.</returns>
        public static Task<HttpResponseMessage> PostMultipartAsync(this string url, Action<CapturedMultipartContent> buildContent, CancellationToken cancellationToken = default(CancellationToken))
        {
            return new FlurlRequest(url).PostMultipartAsync(buildContent, cancellationToken);
        }

        /// <summary>
        /// Sends an asynchronous multipart/form-data PUT request.
        /// </summary>
        /// <param name="buildContent">A delegate for building the content parts.</param>
        /// <param name="request">The IFlurlRequest.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A Task whose result is the received HttpResponseMessage.</returns>
        public static Task<HttpResponseMessage> PutMultipartAsync(this IFlurlRequest request, Action<CapturedMultipartContent> buildContent, CancellationToken cancellationToken = default(CancellationToken))
        {
            var cmc = new CapturedMultipartContent(request.Settings);
            buildContent(cmc);
            return request.SendAsync(HttpMethod.Put, cmc, cancellationToken);
        }

        /// <summary>
        /// Creates a FlurlRequest from the URL and sends an asynchronous multipart/form-data PUT request.
        /// </summary>
        /// <param name="buildContent">A delegate for building the content parts.</param>
        /// <param name="url">The URL.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A Task whose result is the received HttpResponseMessage.</returns>
        public static Task<HttpResponseMessage> PutMultipartAsync(this Url url, Action<CapturedMultipartContent> buildContent, CancellationToken cancellationToken = default(CancellationToken))
        {
            return new FlurlRequest(url).PutMultipartAsync(buildContent, cancellationToken);
        }

        /// <summary>
        /// Creates a FlurlRequest from the URL and sends an asynchronous multipart/form-data PUT request.
        /// </summary>
        /// <param name="buildContent">A delegate for building the content parts.</param>
        /// <param name="url">The URL.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A Task whose result is the received HttpResponseMessage.</returns>
        public static Task<HttpResponseMessage> PutMultipartAsync(this string url, Action<CapturedMultipartContent> buildContent, CancellationToken cancellationToken = default(CancellationToken))
        {
            return new FlurlRequest(url).PutMultipartAsync(buildContent, cancellationToken);
        }
    }
}
