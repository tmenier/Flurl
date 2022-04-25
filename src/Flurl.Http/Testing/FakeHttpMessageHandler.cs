using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Flurl.Http.Testing
{
	/// <summary>
	/// An HTTP message handler that logs calls so they can be asserted in tests. If the corresponding test setup is
	/// configured to fake HTTP calls (the default behavior), blocks the call from being made and provides a fake
	/// response as configured in the test.
	/// </summary>
	public class FakeHttpMessageHandler : DelegatingHandler
	{
		/// <summary>
		/// Creates a new instance of FakeHttpMessageHandler with the given inner handler.
		/// </summary>
		public FakeHttpMessageHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }

		/// <summary>
		/// If there is an HttpTest context, logs the call so it can be asserted. If the corresponding test setup is
		/// configured to fake HTTP calls (the default behavior), blocks the call from being made and provides a fake
		/// response as configured in the test.
		/// </summary>
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			if (HttpTest.Current != null) {
				var call = request.GetFlurlCall();
				if (call != null) {
					HttpTest.Current.LogCall(call);
					var setup = HttpTest.Current.FindSetup(call);
					if (setup?.FakeRequest == true) {
						var resp = setup.GetNextResponse() ?? new HttpResponseMessage {
							StatusCode = HttpStatusCode.OK,
							Content = new StringContent("")
						};

						return Task.FromResult(resp);
					}
				}
			}

			return base.SendAsync(request, cancellationToken);
		}
	}
}