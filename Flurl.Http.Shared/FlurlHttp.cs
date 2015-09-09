using System;
using Flurl.Http.Configuration;

namespace Flurl.Http
{
	/// <summary>
	/// A static container for global configuration settings affecting Flurl.Http behavior.
	/// </summary>
	public static class FlurlHttp
	{
		private static readonly object _configLock = new object();

		private static Lazy<FlurlHttpSettings> _settings = 
			new Lazy<FlurlHttpSettings>(() => new FlurlHttpSettings());

		/// <summary>
		/// Globally configured Flurl.Http settings. Should normally be written to by calling FlurlHttp.Configure once application at startup.
		/// </summary>
		public static FlurlHttpSettings GlobalSettings {
			get { return _settings.Value; }
		}

		/// <summary>
		/// Provides thread-safe accesss to Flurl.Http's global configuration settings. Should only be called once at application startup.
		/// </summary>
		/// <param name="configAction"></param>
		public static void Configure(Action<FlurlHttpSettings> configAction) {
			lock (_configLock) {
				configAction(GlobalSettings);
			}
		}
	}
}
