using System;
using Flurl.Http.Configuration;
using Flurl.Http.Testing;

namespace Flurl.Http
{
	/// <summary>
	/// A static container for global configuration settings affecting Flurl.Http behavior.
	/// </summary>
	public static class FlurlHttp
	{
		private static readonly object _configLock = new object();

		private static Lazy<GlobalFlurlHttpSettings> _settings =
			new Lazy<GlobalFlurlHttpSettings>(() => new GlobalFlurlHttpSettings());

		/// <summary>
		/// Globally configured Flurl.Http settings. Should normally be written to by calling FlurlHttp.Configure once application at startup.
		/// </summary>
		public static GlobalFlurlHttpSettings GlobalSettings => HttpTest.Current?.Settings ?? _settings.Value;

		/// <summary>
		/// Provides thread-safe access to Flurl.Http's global configuration settings. Should only be called once at application startup.
		/// </summary>
		/// <param name="configAction"></param>
		/// <exception cref="Exception">A delegate callback throws an exception.</exception>
		public static void Configure(Action<GlobalFlurlHttpSettings> configAction) {
			lock (_configLock) {
				configAction(GlobalSettings);
			}
		}
	}
}