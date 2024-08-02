using System;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;

namespace Flurl.Http.Configuration
{
	/// <summary>
	/// Interface for helper methods used to construct IFlurlClient instances.
	/// </summary>
	public interface IFlurlClientFactory
	{
		/// <summary>
		/// Creates and configures a new HttpClient as needed when a new IFlurlClient instance is created.
		/// Implementors should NOT attempt to cache or reuse HttpClient instances here - their lifetime is
		/// bound one-to-one with an IFlurlClient, whose caching and reuse is managed by IFlurlClientCache.
		/// </summary>
		/// <param name="handler">The HttpMessageHandler passed to the constructor of the HttpClient.</param>
		HttpClient CreateHttpClient(HttpMessageHandler handler);

		/// <summary>
		/// Creates and configures a new HttpMessageHandler as needed when a new IFlurlClient instance is created.
		/// The default implementation creates an instance of SocketsHttpHandler for platforms that support it,
		/// otherwise HttpClientHandler.
		/// </summary>
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
		// cached Blazor/WASM check (#543, #823)
		private readonly bool _isBrowser =
#if NET
			OperatingSystem.IsBrowser();
#else
			false;
#endif

		/// <inheritdoc />
		public virtual HttpClient CreateHttpClient(HttpMessageHandler handler) {
			return new HttpClient(handler);
		}

		/// <summary>
		/// Creates and configures a new HttpMessageHandler as needed when a new IFlurlClient instance is created.
		/// </summary>
		public virtual HttpMessageHandler CreateInnerHandler() {
			// Flurl has its own mechanisms for managing cookies and redirects, so we need to disable them in the inner handler.
			var handler = new HttpClientHandler();

			if (handler.SupportsRedirectConfiguration)
				handler.AllowAutoRedirect = false;

			// #266
			// deflate not working? see #474
			if (handler.SupportsAutomaticDecompression)
				handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

			if (!_isBrowser) {
				try { handler.UseCookies = false; }
				catch (PlatformNotSupportedException) { } // already checked for Blazor, but just in case any other surprises pop up
			}

			return handler;
		}
	}

#if NET
	/// <summary>
	/// An implementation of IFlurlClientFactory that uses SocketsHttpHandler on supported platforms.
	/// </summary>
	public class SocketsHandlerFlurlClientFactory : DefaultFlurlClientFactory
	{
		/// <summary>
		/// Creates and configures a new SocketsHttpHandler as needed when a new IFlurlClient instance is created.
		/// </summary>
		public override HttpMessageHandler CreateInnerHandler() => new SocketsHttpHandler {
			// Flurl has its own mechanisms for managing cookies and redirects, so we need to disable them in the inner handler.
			UseCookies = false,
			AllowAutoRedirect = false,
			AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
		};
	}
#endif
}
