using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http.Content;

namespace Flurl.Http.Configuration
{
	/// <summary>
	/// HTTP message handler used by default in all Flurl-created HttpClients.
	/// </summary>
	public class FlurlMessageHandler : DelegatingHandler
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FlurlMessageHandler"/> class.
		/// </summary>
		/// <param name="innerHandler">The inner handler.</param>
		public FlurlMessageHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

		/// <summary>
		/// Send request asynchronous.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			var call = HttpCall.Get(request);

			await FlurlHttp.RaiseEventAsync(request, FlurlEventType.BeforeCall).ConfigureAwait(false);
			call.StartedUtc = DateTime.UtcNow;
			try {
				call.Response = await InnerSendAsync(call, request, cancellationToken).ConfigureAwait(false);
				call.Response.RequestMessage = request;
				if (call.Succeeded)
					return call.Response;

				if (call.Response.Content != null)
					call.ErrorResponseBody = await call.Response.Content.StripCharsetQuotes().ReadAsStringAsync().ConfigureAwait(false);
				throw new FlurlHttpException(call, null);
			}
			catch (Exception ex) {
				call.Exception = ex;
				await FlurlHttp.RaiseEventAsync(request, FlurlEventType.OnError).ConfigureAwait(false);
				throw;
			}
			finally {
				call.EndedUtc = DateTime.UtcNow;
				await FlurlHttp.RaiseEventAsync(request, FlurlEventType.AfterCall).ConfigureAwait(false);
			}
		}

		private async Task<HttpResponseMessage> InnerSendAsync(HttpCall call, HttpRequestMessage request, CancellationToken cancellationToken) {
			try {
				return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested) {
				throw new FlurlHttpTimeoutException(call, ex);
			}
			catch (Exception ex) {
				throw new FlurlHttpException(call, ex);
			}
		}
	}
}