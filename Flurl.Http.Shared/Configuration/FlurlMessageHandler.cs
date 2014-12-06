using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http.Content;
using Rackspace.Threading;

namespace Flurl.Http.Configuration
{
	/// <summary>
	/// HTTP message handler used by default in all Flurl-created HttpClients.
	/// </summary>
	internal class FlurlMessageHandler : DelegatingHandler
	{
		public FlurlMessageHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

		public FlurlMessageHandler() : base(new HttpClientHandler()) { }

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			var call = new HttpCall {
				Request = request
			};

			var stringContent = request.Content as CapturedStringContent;
			if (stringContent != null)
				call.RequestBody = stringContent.Content;

			Task t1 = RaiseGlobalEventAsync(FlurlHttp.Configuration.BeforeCall, FlurlHttp.Configuration.BeforeCallAsync, call);

			Func<Task, Task> sendContinuation =
				_ => {
					call.StartedUtc = DateTime.UtcNow;
					return base.SendAsync(request, cancellationToken)
					.Select(task => {
						if (task.Status == TaskStatus.RanToCompletion) {
							call.Response = task.Result;
							call.EndedUtc = DateTime.UtcNow;
						} else {
							call.Exception = task.Exception;
						}
					}, supportsErrors: true);
				};

			Task t2 = t1.Then(sendContinuation);

			Func<Task, Task> exceptionContinuation =
				_ => {
					if (call.Exception != null)
						return RaiseGlobalEventAsync(FlurlHttp.Configuration.OnError, FlurlHttp.Configuration.OnErrorAsync, call);

					return CompletedTask.Default;
				};

			Task t3 = t2.Then(exceptionContinuation);

			Task t4 = t3.Then(_ => RaiseGlobalEventAsync(FlurlHttp.Configuration.AfterCall, FlurlHttp.Configuration.AfterCallAsync, call));

			Task<HttpResponseMessage> resultTask = t4.Select(_ => {
					if (IsErrorCondition(call)) {
						throw IsTimeout(call, cancellationToken) ?
							new FlurlHttpTimeoutException(call, call.Exception) :
							new FlurlHttpException(call, call.Exception);
					}

					return call.Response;
				});

			return resultTask;
		}

		private Task RaiseGlobalEventAsync(Action<HttpCall> syncVersion, Func<HttpCall, Task> asyncVersion, HttpCall call) {
			if (syncVersion != null) syncVersion(call);
			if (asyncVersion != null) return asyncVersion(call);
			return CompletedTask.Default;
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
