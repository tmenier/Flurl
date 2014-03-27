using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Flurl.Http
{
	internal class FlurlMesageHandler : HttpClientHandler
	{
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
			FlurlHttp.BeforeCall(request);

			if (FlurlHttp.TestMode) {
				FlurlHttp.Testing.LastRequest = request;
				FlurlHttp.Testing.LastRequestBody = await request.Content.ReadAsStringAsync();
			}

			HttpResponseMessage response = null;
			try {
				response = FlurlHttp.TestMode ? GetTestResponse() : await base.SendAsync(request, cancellationToken);
			}
			catch (TaskCanceledException) {
				throw new TimeoutException(string.Format("Request to {0} timed out.", request.RequestUri.AbsoluteUri));
			}
			catch (Exception ex) {
				throw new FlurlHttpException(request, response, ex);
			}
			finally {
				FlurlHttp.AfterCall(request, response);
			}

			if (!response.IsSuccessStatusCode)
				throw new FlurlHttpException(request, response);

			return response;
		}

		private static HttpResponseMessage GetTestResponse() {
			return new HttpResponseMessage {
				StatusCode = HttpStatusCode.OK,
				Content = new StringContent("{ message: 'Hello!' }")
			};
		}
	}
}
