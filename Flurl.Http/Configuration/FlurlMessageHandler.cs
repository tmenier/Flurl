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
	internal class FlurlMessageHandler : DelegatingHandler
	{
		public FlurlMessageHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

		public FlurlMessageHandler() : base(new HttpClientHandler()) { }

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
			}
			catch (Exception ex) {
				call.Exception = ex;
			}

			call.EndedUtc = DateTime.UtcNow;

			if (call.Exception != null)
				await RaiseGlobalEventAsync(FlurlHttp.Configuration.OnError, FlurlHttp.Configuration.OnErrorAsync, call);

			await RaiseGlobalEventAsync(FlurlHttp.Configuration.AfterCall, FlurlHttp.Configuration.AfterCallAsync, call);

			if (IsErrorCondition(call)) {
				throw IsTimeout(call, cancellationToken) ?
					new FlurlHttpTimeoutException(call, call.Exception) :
					new FlurlHttpException(call, call.Exception);
			}

			return call.Response;
		}

		private async Task RaiseGlobalEventAsync(Action<HttpCall> syncVersion, Func<HttpCall, Task> asyncVersion, HttpCall call) {
			if (syncVersion != null) syncVersion(call);
			if (asyncVersion != null) await asyncVersion(call);
		}

		private bool IsErrorCondition(HttpCall call) {
			return
				(call.Exception != null && !call.ExceptionHandled) ||
				(call.Response != null && !call.Response.IsSuccessStatusCode);
		}

		private bool IsTimeout(HttpCall call, CancellationToken token) {
			return call.Exception != null && call.Exception is TaskCanceledException && !token.IsCancellationRequested;
		}
	}
}
