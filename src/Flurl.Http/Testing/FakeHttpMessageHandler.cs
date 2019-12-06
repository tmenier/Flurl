using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Flurl.Http.Testing
{
	/// <summary>
	/// An HTTP message handler that prevents actual HTTP calls from being made and instead returns
	/// responses from a provided response factory.
	/// </summary>
	public class FakeHttpMessageHandler : HttpMessageHandler
	{
		/// <summary>
		/// Sends the request asynchronous.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			if (HttpTest.Current != null) {
				var call = request.GetHttpCall();
				if (call != null)
					HttpTest.Current.LogCall(call);
			}

			var tcs = new TaskCompletionSource<HttpResponseMessage>();
			var resp = HttpTest.Current?.GetNextResponse() ?? new HttpResponseMessage();
			if (resp is TimeoutResponseMessage)
				tcs.SetCanceled();
			else
				tcs.SetResult(resp);
			return tcs.Task;
		}
	}
}