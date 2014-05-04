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
		private readonly Func<HttpResponseMessage> _responseFactory;

		public FakeHttpMessageHandler(Func<HttpResponseMessage> responseFactory) {
			_responseFactory = responseFactory;
		}

		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			var resp = _responseFactory();
			var tcs = new TaskCompletionSource<HttpResponseMessage>();
			if (resp is TimeoutResponseMessage)
				tcs.SetCanceled();
			else
				tcs.SetResult(resp);
			return tcs.Task;
		}
	}
}
