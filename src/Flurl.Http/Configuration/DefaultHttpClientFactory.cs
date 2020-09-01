using System;
using System.Net;
using System.Net.Http;

namespace Flurl.Http.Configuration
{
	/// <summary>
	/// Default implementation of IHttpClientFactory used by FlurlHttp. The created HttpClient includes hooks
	/// that enable FlurlHttp's testing features and respect its configuration settings. Therefore, custom factories
	/// should inherit from this class, rather than implementing IHttpClientFactory directly.
	/// </summary>
	public class DefaultHttpClientFactory : IHttpClientFactory
	{
		/// <summary>
		/// Override in custom factory to customize the creation of HttpClient used in all Flurl HTTP calls.
		/// In order not to lose Flurl.Http functionality, it is recommended to call base.CreateClient and
		/// customize the result.
		/// </summary>
		public virtual HttpClient CreateHttpClient(HttpMessageHandler handler) {
			return new HttpClient(handler) {
				// Timeouts handled per request via FlurlHttpSettings.Timeout
				Timeout = System.Threading.Timeout.InfiniteTimeSpan
			};
		}

		/// <summary>
		/// Override in custom factory to customize the creation of HttpClientHandler used in all Flurl HTTP calls.
		/// In order not to lose Flurl.Http functionality, it is recommended to call base.CreateMessageHandler and
		/// customize the result.
		/// </summary>
		public virtual HttpMessageHandler CreateMessageHandler() {
			var httpClientHandler = new HttpClientHandler();

			// flurl has its own mechanisms for managing cookies and redirects

			try { httpClientHandler.UseCookies = false; }
			catch (PlatformNotSupportedException) { } // look out for WASM platforms (#543)

			if (httpClientHandler.SupportsRedirectConfiguration)
				httpClientHandler.AllowAutoRedirect = false;

			if (httpClientHandler.SupportsAutomaticDecompression) {
				// #266
				// deflate not working? see #474
				httpClientHandler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
			}
			return httpClientHandler;
		}
	}
}