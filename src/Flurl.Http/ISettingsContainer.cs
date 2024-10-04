using System;
using System.Linq;
using System.Net;
using Flurl.Http.Authentication;
using Flurl.Http.Configuration;

namespace Flurl.Http
{
	/// <summary>
	/// A common interface for Flurl.Http objects that are configurable via a Settings property.
	/// </summary>
	public interface ISettingsContainer
	{
		/// <summary>
		/// A collection request settings.
		/// </summary>
		FlurlHttpSettings Settings { get; }
	}

	/// <summary>
	/// Fluent extension methods for tweaking FlurlHttpSettings
	/// </summary>
	public static class SettingsExtensions
	{
		/// <summary>
		/// Change FlurlHttpSettings for this request, client, or test context.
		/// </summary>
		/// <param name="obj">Object containing settings.</param>
		/// <param name="action">Action defining the settings changes.</param>
		/// <returns>This settings container.</returns>
		public static T WithSettings<T>(this T obj, Action<FlurlHttpSettings> action) where T : ISettingsContainer {
			action(obj.Settings);
			return obj;
		}

		/// <summary>
		/// Sets the timeout for this request, client, or test context.
		/// </summary>
		/// <param name="obj">Object containing settings.</param>
		/// <param name="timespan">Time to wait before the request times out.</param>
		/// <returns>This settings container.</returns>
		public static T WithTimeout<T>(this T obj, TimeSpan timespan) where T : ISettingsContainer {
			obj.Settings.Timeout = timespan;
			return obj;
		}

		/// <summary>
		/// Sets the timeout for this request, client, or test context.
		/// </summary>
		/// <param name="obj">Object containing settings.</param>
		/// <param name="seconds">Seconds to wait before the request times out.</param>
		/// <returns>This settings container.</returns>
		public static T WithTimeout<T>(this T obj, int seconds) where T : ISettingsContainer {
			obj.Settings.Timeout = TimeSpan.FromSeconds(seconds);
			return obj;
		}

		/// <summary>
		/// Adds a pattern representing an HTTP status code or range of codes which (in addition to 2xx) will NOT result in a FlurlHttpException being thrown.
		/// </summary>
		/// <param name="obj">Object containing settings.</param>
		/// <param name="pattern">Examples: "3xx", "100,300,600", "100-299,6xx"</param>
		/// <returns>This settings container.</returns>
		public static T AllowHttpStatus<T>(this T obj, string pattern) where T : ISettingsContainer {
			if (!string.IsNullOrWhiteSpace(pattern)) {
				var current = obj.Settings.AllowedHttpStatusRange;
				if (string.IsNullOrWhiteSpace(current))
					obj.Settings.AllowedHttpStatusRange = pattern;
				else
					obj.Settings.AllowedHttpStatusRange += "," + pattern;
			}
			return obj;
		}

		/// <summary>
		/// Adds one or more response status codes which (in addition to 2xx) will NOT result in a FlurlHttpException being thrown.
		/// </summary>
		/// <param name="obj">Object containing settings.</param>
		/// <param name="statusCodes">One or more response status codes that, when received, will not cause an exception to be thrown.</param>
		/// <returns>This settings container.</returns>
		public static T AllowHttpStatus<T>(this T obj, params int[] statusCodes) where T : ISettingsContainer {
			var pattern = string.Join(",", statusCodes);
			return AllowHttpStatus(obj, pattern);
		}

		/// <summary>
		/// Prevents a FlurlHttpException from being thrown on any completed response, regardless of the HTTP status code.
		/// </summary>
		/// <param name="obj">Object containing settings.</param>
		/// <returns>This settings container.</returns>
		public static T AllowAnyHttpStatus<T>(this T obj) where T : ISettingsContainer {
			obj.Settings.AllowedHttpStatusRange = "*";
			return obj;
		}

		/// <summary>
		/// Configures whether redirects are automatically followed.
		/// </summary>
		/// <param name="obj">Object containing settings.</param>
		/// <param name="enabled">true if Flurl should automatically send a new request to the redirect URL, false if it should not.</param>
		/// <returns>This settings container.</returns>
		public static T WithAutoRedirect<T>(this T obj, bool enabled) where T : ISettingsContainer {
			obj.Settings.Redirects.Enabled = enabled;
			return obj;
		}

		/// <summary>
		/// Configures the OAuth token provider which authenticates the request
		/// </summary>
		/// <param name="obj">Object containing settings.</param>
		/// <param name="tokenProvider">The token provider</param>
		/// <returns>this settings container.</returns>
		public static T WithOAuthTokenProvider<T>(this T obj, IOAuthTokenProvider tokenProvider) where T : ISettingsContainer {
			obj.Settings.OAuthTokenProvider = tokenProvider;
			return obj;
		}
	}
}