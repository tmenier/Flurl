using System;
using System.Collections.Concurrent;
using Flurl.Util;

namespace Flurl.Http.Configuration
{
    /// <summary>
    /// Encapsulates a creation/caching strategy for IFlurlClient instances. Custom factories looking to extend
    /// Flurl's behavior should inherit from this class, rather than implementing IFlurlClientFactory directly.
    /// </summary>
    public abstract class FlurlClientFactoryBase : DisposableBase, IFlurlClientFactory
    {
        private readonly ConcurrentDictionary<string, IFlurlClient> _clients = new ConcurrentDictionary<string, IFlurlClient>();

        /// <summary>
        /// By default, uses a caching strategy of one FlurlClient per host. This maximizes reuse of
        /// underlying HttpClient/Handler while allowing things like cookies to be host-specific.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <returns>The FlurlClient instance.</returns>
        public virtual IFlurlClient Get(Url url)
        {
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
        protected override void DisposeResources()
        {
            foreach (var kv in _clients)
            {
                using (kv.Value) { }
            }

            _clients.Clear();
        }
    }
}
