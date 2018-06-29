using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flurl.Http.Configuration
{
	/// <summary>
	/// An IFlurlClientFactory implementation that caches and reuses the same one instance of
	/// FlurlClient per host being called. Maximizes reuse of underlying HttpClient/Handler
	/// while allowing things like cookies to be host-specific. This is the default
	/// implementation used when calls are made fluently off Urls/strings.
	/// </summary>
	public class PerHostFlurlClientFactory : FlurlClientFactoryBase
	{
		/// <summary>
		/// Returns the host part of the URL (i.e. www.api.com) so that all calls to the same
		/// host use the same FlurlClient (and HttpClient/HttpMessageHandler) instance.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <returns>The cache key</returns>
		protected override string GetCacheKey(Url url) => new Uri(url).Host;
	}
}
