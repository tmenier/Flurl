using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Flurl.Http.Configuration
{
	/// <summary>
	/// Interface for a cache of IFlurlClient instances.
	/// </summary>
	public interface IFlurlClientCache
	{
		/// <summary>
		/// Adds a new IFlurlClient to this cache. Call once per client at startup to register and configure a named client.
		/// </summary>
		/// <param name="name">Name of the IFlurlClient. Serves as a cache key. Subsequent calls to Get will return this client.</param>
		/// <param name="baseUrl">Optional. The base URL associated with the new client.</param>
		/// <returns>A builder to further configure the new client.</returns>
		IFlurlClientBuilder Add(string name, string baseUrl = null);

		/// <summary>
		/// Gets a preconfigured named IFlurlClient.
		/// </summary>
		/// <param name="name">The client name.</param>
		/// <returns>The cached IFlurlClient.</returns>
		IFlurlClient Get(string name);

		/// <summary>
		/// Gets a named IFlurlClient, creating and (optionally) configuring one if it doesn't exist or has been disposed.
		/// </summary>
		/// <param name="name">The client name.</param>
		/// <param name="baseUrl">The base URL associated with the new client, if it doesn't exist.</param>
		/// <param name="configure">Configure the builder associated with the new client, if it doesn't exist.</param>
		/// <returns>The cached IFlurlClient.</returns>
		IFlurlClient GetOrAdd(string name, string baseUrl = null, Action<IFlurlClientBuilder> configure = null);

		/// <summary>
		/// Configuration logic that gets executed for every new IFlurlClient added this case. Good place for things like default
		/// settings. Executes before client-specific builder logic.
		/// </summary>
		/// <returns>This IFlurlCache.</returns>
		IFlurlClientCache ConfigureAll(Action<IFlurlClientBuilder> configure);

		/// <summary>
		/// Removes a named client from this cache.
		/// </summary>
		/// <returns>This IFlurlCache.</returns>
		IFlurlClientCache Remove(string name);

		/// <summary>
		/// Disposes and removes all cached IFlurlClient instances.
		/// </summary>
		/// <returns>This IFlurlCache.</returns>
		IFlurlClientCache Clear();
	}

	/// <summary>
	/// Extension methods on IFlurlClientCache.
	/// </summary>
	public static class IFlurlClientCacheExtensions
	{
		/// <summary>
		/// Adds a new IFlurlClient to this cache. Call once per client at startup to register and configure a named client.
		/// Allows configuring via a nested lambda, rather than returning a builder, so multiple Add calls can be fluently chained.
		/// </summary>
		/// <param name="cache">This IFlurlCache</param>
		/// <param name="name">Name of the IFlurlClient. Serves as a cache key. Subsequent calls to Get will return this client.</param>
		/// <param name="baseUrl">The base URL associated with the new client.</param>
		/// <param name="configure">Configure the builder associated with the added client.</param>
		/// <returns>This IFlurlCache.</returns>
		public static IFlurlClientCache Add(this IFlurlClientCache cache, string name, string baseUrl, Action<IFlurlClientBuilder> configure) {
			var builder = cache.Add(name, baseUrl);
			configure?.Invoke(builder);
			return cache;
		}
	}

	/// <summary>
	/// Default implementation of IFlurlClientCache.
	/// </summary>
	public class FlurlClientCache : IFlurlClientCache
	{
		private readonly ConcurrentDictionary<string, Lazy<IFlurlClient>> _clients = new();
		private readonly List<Action<IFlurlClientBuilder>> _configureAll = new();

		/// <inheritdoc />
		public IFlurlClientBuilder Add(string name, string baseUrl = null) {
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			var builder = CreateBuilder(baseUrl);
			if (!_clients.TryAdd(name, new Lazy<IFlurlClient>(builder.Build)))
				throw new ArgumentException($"A client named '{name}' was already registered. Add should be called just once per client at startup.");

			return builder;
		}

		/// <inheritdoc />
		public virtual IFlurlClient Get(string name) {
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			if (!_clients.TryGetValue(name, out var cli))
				throw new ArgumentException($"A client named '{name}' was not found. Either preconfigure the client using Add (typically at startup), or use GetOrAdd to add/configure one on demand when needed.");

			if (cli.Value.IsDisposed)
				throw new Exception($"A client named '{name}' was not found but has been disposed and cannot be reused.");

			return cli.Value;
		}

		/// <inheritdoc />
		public IFlurlClient GetOrAdd(string name, string baseUrl = null, Action<IFlurlClientBuilder> configure = null) {
			if (name == null)
				throw new ArgumentNullException(nameof(name));

			Lazy<IFlurlClient> Create() {
				var builder = CreateBuilder(baseUrl);
				configure?.Invoke(builder);
				return new Lazy<IFlurlClient>(builder.Build);
			}

			return _clients.AddOrUpdate(name, _ => Create(), (_, existing) => existing.Value.IsDisposed ? Create() : existing).Value;
		}

		/// <inheritdoc />
		public IFlurlClientCache ConfigureAll(Action<IFlurlClientBuilder> configure) {
			if (configure != null)
				_configureAll.Add(configure);
			return this;
		}

		/// <inheritdoc />
		public IFlurlClientCache Remove(string name) {
			if (_clients.TryRemove(name, out var cli) && cli.IsValueCreated && !cli.Value.IsDisposed)
				cli.Value.Dispose();
			return this;
		}

		/// <inheritdoc />
		public IFlurlClientCache Clear() {
			// Remove takes care of disposing too, which is why we don't simply call _clients.Clear
			foreach (var key in _clients.Keys)
				Remove(key);
			return this;
		}

		private IFlurlClientBuilder CreateBuilder(string baseUrl) {
			var builder = new FlurlClientBuilder(baseUrl);
			foreach (var config in _configureAll)
				config(builder);
			return builder;
		}
	}
}
