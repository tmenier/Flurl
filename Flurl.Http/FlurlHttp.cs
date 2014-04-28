using System;
using System.Net.Http;
using Flurl.Http.Configuration;

namespace Flurl.Http
{
	/// <summary>
	/// A static container for global configuration settings affecting Flurl.Http behavior.
	/// </summary>
	public static class FlurlHttp
	{
		private static readonly object _configLock = new object();

		private static Lazy<FlurlHttpConfigurationOptions> _config = 
			new Lazy<FlurlHttpConfigurationOptions>(() => new FlurlHttpConfigurationOptions());

		public static FlurlHttpConfigurationOptions Configuration {
			get { return _config.Value; }
		}

		/// <summary>
		/// Provides thread-safe accesss to Flurl.Http's global configuration options. Should only be
		/// called once at application startup.
		/// </summary>
		/// <param name="configAction"></param>
		public static void Configure(Action<FlurlHttpConfigurationOptions> configAction) {
			lock (_configLock) {
				configAction(Configuration);
			}
		}
	}
}
