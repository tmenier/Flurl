using System;
using System.Net;
using System.Net.Http;

namespace Flurl.Http.Configuration
{
	/// <summary>
	/// Interface for methods used to build and cache IFlurlClient instances.
	/// </summary>
	public interface IFlurlClientFactory
	{
		/// <summary>
		/// Defines how HttpClient should be instantiated and configured by default. Do NOT attempt
		/// to cache/reuse HttpClient instances here - that should be done at the FlurlClient level
		/// via a custom FlurlClientFactory that gets registered globally.
		/// </summary>
		/// <param name="handler">The HttpMessageHandler used to construct the HttpClient.</param>
		/// <returns></returns>
		HttpClient CreateHttpClient(HttpMessageHandler handler);

		/// <summary>
		/// Defines how the HttpMessageHandler used by HttpClients that are created by
		/// this factory should be instantiated and configured. 
		/// </summary>
		/// <returns></returns>
		HttpMessageHandler CreateInnerHandler();
	}

	/// <summary>
	/// Extension methods on IFlurlClientFactory
	/// </summary>
	public static class FlurlClientFactoryExtensions
	{
		/// <summary>
		/// Creates an HttpClient with the HttpMessageHandler returned from this factory's CreateInnerHandler method.
		/// </summary>
		public static HttpClient CreateHttpClient(this IFlurlClientFactory fac) => fac.CreateHttpClient(fac.CreateInnerHandler());
	}

	/// <summary>
	/// Default implementation of IFlurlClientFactory, used to build and cache IFlurlClient instances.
	/// </summary>
	public class DefaultFlurlClientFactory : IFlurlClientFactory
	{
		/// <summary>
		/// Override in custom factory to customize the creation of HttpClient used in all Flurl HTTP calls
		/// (except when one is passed explicitly to the FlurlClient constructor). In order not to lose
		/// Flurl.Http functionality, it is recommended to call base.CreateClient and customize the result.
		/// </summary>
		public virtual HttpClient CreateHttpClient(HttpMessageHandler handler) {
			return new HttpClient(handler);
		}

		/// <summary>
		/// Override in custom factory to customize the creation of the top-level HttpMessageHandler used in all
		/// Flurl HTTP calls (except when an HttpClient is passed explicitly to the FlurlClient constructor).
		/// In order not to lose Flurl.Http functionality, it is recommended to call base.CreateMessageHandler, and
		/// either customize the returned HttpClientHandler, or set it as the InnerHandler of a DelegatingHandler.
		/// </summary>
		public virtual HttpMessageHandler CreateInnerHandler() {
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
