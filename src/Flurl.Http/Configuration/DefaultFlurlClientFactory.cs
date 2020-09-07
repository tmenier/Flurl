using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flurl.Http.Configuration
{
	/// <summary>
	/// An IFlurlClientFactory implementation that caches and reuses the same one instance of
	/// FlurlClient per combination of scheme, host, and port. This is the default
	/// implementation used when calls are made fluently off Urls/strings.
	/// </summary>
	public class DefaultFlurlClientFactory : FlurlClientFactoryBase
	{
		/// <summary>
		/// Returns a unique cache key based on scheme, host, and port of the given URL.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <returns>The cache key</returns>
		protected override string GetCacheKey(Url url) => $"{url.Scheme}|{url.Host}|{url.Port}";
	}
}