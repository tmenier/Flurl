using System;
using System.Collections.Concurrent;
using System.Net.Http;

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
		/// Override in custom factory to customize the creation of HttpClient used in all Flurl HTTP calls.
		/// In order not to lose Flurl.Http functionality, it is recommended to call base.CreateClient and
		/// customize the result.
		/// </summary>
		public virtual HttpClient CreateHttpClient(HttpMessageHandler handler) {
			return new HttpClient(new FlurlMessageHandler(handler));
		}

		/// <summary>
		/// Override in custom factory to customize the creation of HttpClientHandler used in all Flurl HTTP calls.
		/// In order not to lose Flurl.Http functionality, it is recommended to call base.CreateMessageHandler and
		/// customize the result.
		/// </summary>
		public virtual HttpMessageHandler CreateMessageHandler() {
			return new HttpClientHandler();
		}

		/// <summary>
		/// Uses a caching strategy of one FlurlClient per host. This maximizes reuse of underlying
		/// HttpClient/Handler while allowing things like cookies to be host-specific.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <returns>The FlurlClient instance.</returns>
		public virtual IFlurlClient GetClient(Url url) {
			var key = new Uri(url).Host;
			return _clients.GetOrAdd(key, _ => new FlurlClient());
		}
	}
}
