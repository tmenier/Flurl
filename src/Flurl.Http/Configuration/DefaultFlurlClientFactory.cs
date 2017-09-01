using System;
using System.Collections.Concurrent;

namespace Flurl.Http.Configuration
{
	/// <summary>
	/// Default implementation of IFlurlClientFactory used by Flurl.Http. Custom factories looking to extend
	/// Flurl's behavior should inherit from this class, rather than implementing IFlurlClientFactory directly.
	/// </summary>
	public class DefaultFlurlClientFactory : IFlurlClientFactory
	{
		private static readonly ConcurrentDictionary<string, IFlurlClient> _clients = new ConcurrentDictionary<string, IFlurlClient>();

		/// <summary>
		/// By defaykt, uses a caching strategy of one FlurlClient per host. This maximizes reuse of
		/// underlying HttpClient/Handler while allowing things like cookies to be host-specific.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <returns>The FlurlClient instance.</returns>
		public virtual IFlurlClient Get(Url url) {
			return _clients.AddOrUpdate(
				GetCacheKey(url),
				_ => new FlurlClient(),
				(_, client) => client.IsDisposed ? new FlurlClient() : client);
		}

		/// <summary>
		/// Defines a strategy for getting a cache key based on a Url. Default implementation
		/// returns the host part (i.e www.api.com) so that all calls to the same host use the
		/// same FlurlClient (and HttpClient/HttpMessageHandler) instance.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <returns>The cache key</returns>
		protected virtual string GetCacheKey(Url url) => new Uri(url).Host;
	}
}
