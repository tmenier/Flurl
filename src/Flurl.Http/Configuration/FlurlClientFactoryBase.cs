using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net;

namespace Flurl.Http.Configuration
{
	/// <summary>
	/// Encapsulates a creation/caching strategy for IFlurlClient instances. Custom factories looking to extend
	/// Flurl's behavior should inherit from this class, rather than implementing IFlurlClientFactory directly.
	/// </summary>
	public abstract class FlurlClientFactoryBase : IFlurlClientFactory
	{
		private readonly ConcurrentDictionary<string, IFlurlClient> _clients = new ConcurrentDictionary<string, IFlurlClient>();

		/// <summary>
		/// By default, uses a caching strategy of one FlurlClient per host. This maximizes reuse of
		/// underlying HttpClient/Handler while allowing things like cookies to be host-specific.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <returns>The FlurlClient instance.</returns>
		public virtual IFlurlClient Get(Url url) {
			if (url == null)
				throw new ArgumentNullException(nameof(url));

			return _clients.AddOrUpdate(
				GetCacheKey(url),
				u => Create(u),
				(u, client) => client.IsDisposed ? Create(u) : client);
		}

		/// <summary>
		/// Defines a strategy for getting a cache key based on a Url. Default implementation
		/// returns the host part (i.e www.api.com) so that all calls to the same host use the
		/// same FlurlClient (and HttpClient/HttpMessageHandler) instance.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <returns>The cache key</returns>
		protected abstract string GetCacheKey(Url url);

		/// <summary>
		/// Creates a new FlurlClient
		/// </summary>
		/// <param name="url">The URL (not used)</param>
		/// <returns></returns>
		protected virtual IFlurlClient Create(Url url) => new FlurlClient();

		/// <summary>
		/// Disposes all cached IFlurlClient instances and clears the cache.
		/// </summary>
		public void Dispose() {
			foreach (var kv in _clients) {
				if (!kv.Value.IsDisposed)
					kv.Value.Dispose();
			}
			_clients.Clear();
		}

		/// <summary>
		/// Override in custom factory to customize the creation of HttpClient used in all Flurl HTTP calls
		/// (except when one is passed explicitly to the FlurlClient constructor). In order not to lose
		/// Flurl.Http functionality, it is recommended to call base.CreateClient and customize the result.
		/// </summary>
		public virtual HttpClient CreateHttpClient(HttpMessageHandler handler) {
			return new HttpClient(handler) {
				// Timeouts handled per request via FlurlHttpSettings.Timeout
				Timeout = System.Threading.Timeout.InfiniteTimeSpan
			};
		}

		/// <summary>
		/// Override in custom factory to customize the creation of the top-level HttpMessageHandler used in all
		/// Flurl HTTP calls (except when an HttpClient is passed explicitly to the FlurlClient constructor).
		/// In order not to lose Flurl.Http functionality, it is recommended to call base.CreateMessageHandler, and
		/// either customize the returned HttpClientHandler, or set it as the InnerHandler of a DelegatingHandler.
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
