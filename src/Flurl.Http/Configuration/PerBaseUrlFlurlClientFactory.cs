using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flurl.Http.Configuration
{
	/// <summary>
	/// An IFlurlClientFactory implementation that caches and reuses the same IFlurlClient instance
	/// per URL requested, which it assumes is a "base" URL, and sets the IFlurlClient.BaseUrl property
	/// to that value. Ideal for use with IoC containers - register as a singleton, inject into a service
	/// that wraps some web service, and use to set a private IFlurlClient field in the constructor.
	/// </summary>
	public class PerBaseUrlFlurlClientFactory : FlurlClientFactoryBase
	{
		/// <summary>
		/// Returns the entire URL, which is assumed to be some "base" URL for a service.
		/// </summary>
		/// <param name="url">The URL.</param>
		/// <returns>The cache key</returns>
		protected override string GetCacheKey(Url url) => url.ToString();

		/// <summary>
		/// Returns a new new FlurlClient with BaseUrl set to the URL passed.
		/// </summary>
		/// <param name="url">The URL</param>
		/// <returns></returns>
		protected override IFlurlClient Create(Url url) => new FlurlClient(url);
	}
}
