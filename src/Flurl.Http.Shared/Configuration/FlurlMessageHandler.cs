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
			var call = request.GetFlurlHttpCall();

			var stringContent = request.Content as CapturedStringContent;
			if (stringContent != null)
				call.RequestBody = stringContent.Content;

			await FlurlHttp.RaiseEventAsync(request, FlurlEventType.BeforeCall).ConfigureAwait(false);

			call.StartedUtc = DateTime.UtcNow;

			try {
				call.Response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
				call.EndedUtc = DateTime.UtcNow;
			}
			catch (OperationCanceledException ex) {
				call.Exception = (cancellationToken.IsCancellationRequested) ?
					new FlurlHttpException(call, ex) :
					new FlurlHttpTimeoutException(call, ex);
			}
			catch (Exception ex) {
				call.Exception =  new FlurlHttpException(call, ex);
			}

			if (call.Response != null && !call.Succeeded) {
				if (call.Response.Content != null)
					call.ErrorResponseBody = await call.Response.Content.StripCharsetQuotes().ReadAsStringAsync().ConfigureAwait(false);

				call.Exception = new FlurlHttpException(call, null);
			}

			if (call.Exception != null)
				await FlurlHttp.RaiseEventAsync(request, FlurlEventType.OnError).ConfigureAwait(false);

			await FlurlHttp.RaiseEventAsync(request, FlurlEventType.AfterCall).ConfigureAwait(false);

			if (call.Exception != null && !call.ExceptionHandled)
				throw call.Exception;

			if (call.Response != null)
				call.Response.RequestMessage = request;
	
			return call.Response;
		}
	}
}