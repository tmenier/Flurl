using System;
using System.Net.Http;
using System.Threading.Tasks;
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
		/// Provides thread-safe access to Flurl.Http's global configuration settings. Should only be called once at application startup.
		/// </summary>
		/// <param name="configAction"></param>
		/// <exception cref="Exception">A delegate callback throws an exception.</exception>
		public static void Configure(Action<FlurlHttpSettings> configAction) {
			lock (_configLock) {
				configAction(GlobalSettings);
			}
		}

		/// <summary>
		/// Triggers the specified sync and async event handlers, usually defined on 
		/// </summary>
		public static Task RaiseEventAsync(HttpRequestMessage request, FlurlEventType eventType) {
			var call = HttpCall.Get(request);
			if (call == null)
				return NoOpTask.Instance;

			var settings = call.Settings;
			if (settings == null)
				return NoOpTask.Instance;

			switch (eventType) {
				case FlurlEventType.BeforeCall:
					return HandleEventAsync(settings.BeforeCall, settings.BeforeCallAsync, call);
				case FlurlEventType.AfterCall:
					return HandleEventAsync(settings.AfterCall, settings.AfterCallAsync, call);
				case FlurlEventType.OnError:
					return HandleEventAsync(settings.OnError, settings.OnErrorAsync, call);
				default:
					return NoOpTask.Instance;
			}
		}

		private static Task HandleEventAsync(Action<HttpCall> syncHandler, Func<HttpCall, Task> asyncHandler, HttpCall call) {
			if (syncHandler != null)
				syncHandler(call);
			if (asyncHandler != null)
				return asyncHandler(call);
			return NoOpTask.Instance;
		}
	}

	/// <summary>
	/// Flurl event types/
	/// </summary>
	public enum FlurlEventType {
		/// <summary>
		/// The before call
		/// </summary>
		BeforeCall,
		/// <summary>
		/// The after call
		/// </summary>
		AfterCall,
		/// <summary>
		/// The on error
		/// </summary>
		OnError
	}
}