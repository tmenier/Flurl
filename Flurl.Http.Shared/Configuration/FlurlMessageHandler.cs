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
		public FlurlMessageHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			var call = new HttpCall {
				Request = request
			};

			var stringContent = request.Content as CapturedStringContent;
			if (stringContent != null)
				call.RequestBody = stringContent.Content;

			await RaiseGlobalEventAsync(FlurlHttp.Configuration.BeforeCall, FlurlHttp.Configuration.BeforeCallAsync, call);

			call.StartedUtc = DateTime.UtcNow;

			try {
				call.Response = await base.SendAsync(request, cancellationToken);
				call.EndedUtc = DateTime.UtcNow;
			}
			catch (TaskCanceledException ex) {
				if (!cancellationToken.IsCancellationRequested)
					call.Exception = new FlurlHttpTimeoutException(call, ex);
			}
			catch (Exception ex) {
				call.Exception = new FlurlHttpException(call, ex);
			}

			if (call.Response != null && !call.Succeeded) {
				if (call.Response.Content != null)
					call.ErrorResponseBody = await call.Response.Content.ReadAsStringAsync();

				call.Exception = new FlurlHttpException(call, null);
			}

			if (call.Exception != null)
				await RaiseGlobalEventAsync(FlurlHttp.Configuration.OnError, FlurlHttp.Configuration.OnErrorAsync, call);

			await RaiseGlobalEventAsync(FlurlHttp.Configuration.AfterCall, FlurlHttp.Configuration.AfterCallAsync, call);

			if (call.Exception != null && !call.ExceptionHandled)
				throw call.Exception;

			return call.Response;
		}

		private async Task RaiseGlobalEventAsync(Action<HttpCall> syncVersion, Func<HttpCall, Task> asyncVersion, HttpCall call) {
			if (syncVersion != null) syncVersion(call);
			if (asyncVersion != null) await asyncVersion(call);
		}
	}
}
