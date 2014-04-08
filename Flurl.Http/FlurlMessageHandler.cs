using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http.Testing;

namespace Flurl.Http
{
	/// <summary>
	/// HTTP message handler used by default in all Flurl-created HttpClients. Provides call faking and logging in test mode,
	/// BeforeCall/AfterCall global event firing, and enhanced exceptions.
	/// </summary>
	internal class FlurlMessageHandler : HttpClientHandler
	{
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			FlurlHttp.BeforeCall(request);

			if (FlurlHttp.TestMode)
				await CaptureRequestAsync(request);

			HttpResponseMessage response = null;
			try {
				response = FlurlHttp.TestMode ?
					FlurlHttp.Testing.GetNextResponse() :
					await base.SendAsync(request, cancellationToken);
			}
			catch (Exception ex) {
				throw new FlurlHttpException(request, response, ex);
			}
			finally {
				FlurlHttp.AfterCall(request, response);
			}

			if (FlurlHttp.TestMode)
				await CaptureResponseAsync(response);

			if (!response.IsSuccessStatusCode)
				throw new FlurlHttpException(request, response);

			return response;
		}

		private async Task CaptureRequestAsync(HttpRequestMessage request) {
			var call = new CallLogEntry { Request = request };
			if (request.Content != null)
				call.RequestBody = await request.Content.ReadAsStringAsync();
			FlurlHttp.Testing.CallLog.Add(call);
		}

		private async Task CaptureResponseAsync(HttpResponseMessage response) {
			var call = FlurlHttp.Testing.CallLog.LastOrDefault();
			if (call != null) {
				call.Response = response;
				if (response.Content != null)
					call.ResponseBody = await response.Content.ReadAsStringAsync();
			}
		}
	}
}
