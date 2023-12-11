using System;
using Flurl.Http.Configuration;

namespace Flurl.Http
{
	/// <summary>
	/// A static object for configuring Flurl for "clientless" usage. Provides a default IFlurlClientCache instance primarily
	/// for clientless support, but can be used directly, as an alternative to a DI-managed singleton cache.
	/// </summary>
	public static class FlurlHttp
	{
		private static Func<IFlurlRequest, string> _cachingStrategy = BuildClientNameByHost;

		/// <summary>
		/// A global collection of cached IFlurlClient instances.
		/// </summary>
		public static IFlurlClientCache Clients { get; } = new FlurlClientCache();

		/// <summary>
		/// Gets a builder for configuring the IFlurlClient that would be selected for calling the given URL when the clientless pattern is used.
		/// Note that if you've overridden the caching strategy to vary clients by request properties other than Url, you should instead use
		/// FlurlHttp.Clients.Add(name) to ensure you are configuring the correct client.
		/// </summary>
		public static IFlurlClientBuilder ConfigureClientForUrl(string url) {
			IFlurlClientBuilder builder = null;
			Clients.Add(_cachingStrategy(new FlurlRequest(url)), null, b => builder = b);
			return builder;
		}

		/// <summary>
		/// Gets or creates the IFlurlClient that would be selected for sending the given IFlurlRequest when the clientless pattern is used.
		/// </summary>
		public static IFlurlClient GetClientForRequest(IFlurlRequest req) => Clients.GetOrAdd(_cachingStrategy(req));

		/// <summary>
		/// Sets a global caching strategy for getting or creating an IFlurlClient instance when the clientless pattern is used, e.g. url.GetAsync.
		/// </summary>
		/// <param name="buildClientName">A delegate that returns a cache key used to store and retrieve a client instance based on properties of the request.</param>
		public static void UseClientCachingStrategy(Func<IFlurlRequest, string> buildClientName) => _cachingStrategy = buildClientName;

		/// <summary>
		/// Sets a global caching strategy of one IFlurlClient per scheme/host/port combination when the clientless pattern is used,
		/// e.g. url.GetAsync. This is the default strategy, so you shouldn't need to call this except to revert a previous call to
		/// UseClientCachingStrategy, which would be rare.
		/// </summary>
		public static void UseClientPerHostStrategy() => _cachingStrategy = BuildClientNameByHost;

		/// <summary>
		/// Builds a cache key consisting of URL scheme, host, and port. This is the default client caching strategy.
		/// </summary>
		public static string BuildClientNameByHost(IFlurlRequest req) => $"{req.Url.Scheme}|{req.Url.Host}|{req.Url.Port}";
	}
}