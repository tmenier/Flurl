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

		/// <summary>
		/// Initializes a new instance of the <see cref="FakeHttpMessageHandler"/> class.
		/// </summary>
		/// <param name="responseFactory">The response factory.</param>
		public FakeHttpMessageHandler(Func<HttpResponseMessage> responseFactory) {
			_responseFactory = responseFactory;
		}

		/// <summary>
		/// Sends the request asynchronous.
		/// </summary>
		/// <param name="request">The request.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
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