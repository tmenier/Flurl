using System;
using System.Linq;
using System.Net;
using Flurl.Http.Configuration;

namespace Flurl.Http
{
	/// <summary>
	/// Fluent extension methods for tweaking FlurlHttpSettings
	/// </summary>
	public static class SettingsExtensions
	{
		/// <summary>
		/// Fluently specify the IFlurlClient to use with this IFlurlRequest.
		/// </summary>
		/// <param name="request">The IFlurlRequest.</param>
		/// <param name="client">The IFlurlClient to use when sending the request.</param>
		/// <returns>A new IFlurlRequest to use in calling the Url</returns>
		public static IFlurlRequest WithClient(this IFlurlRequest request, IFlurlClient client) {
			request.Client = client;
			return request;
		}

		/// <summary>
		/// Fluently returns a new IFlurlRequest that can be used to call this Url with the given client.
		/// </summary>
		/// <param name="url"></param>
		/// <param name="client">The IFlurlClient to use to call the Url.</param>
		/// <returns>A new IFlurlRequest to use in calling the Url</returns>
		public static IFlurlRequest WithClient(this Url url, IFlurlClient client) {
			return client.Request(url);
		}

		/// <summary>
		/// Fluently returns a new IFlurlRequest that can be used to call this Url with the given client.
		/// </summary>
		/// <param name="url"></param>
		/// <param name="client">The IFlurlClient to use to call the Url.</param>
		/// <returns>A new IFlurlRequest to use in calling the Url</returns>
		public static IFlurlRequest WithClient(this string url, IFlurlClient client) {
			return client.Request(url);
		}

		/// <summary>
		/// Change FlurlHttpSettings for this IFlurlClient or IFlurlRequest.
		/// </summary>
		/// <param name="obj">The IFlurlClient or IFlurlRequest.</param>
		/// <param name="action">Action defining the settings changes.</param>
		/// <returns>The T with the modified HttpClient</returns>
		public static T Configure<T>(this T obj, Action<FlurlHttpSettings> action) where T : IHttpSettingsContainer {
			action(obj.Settings);
			return obj;
		}

		/// <summary>
		/// Sets the timeout for this IFlurlRequest or all requests made with this IFlurlClient.
		/// </summary>
		/// <param name="obj">The IFlurlClient or IFlurlRequest.</param>
		/// <param name="timespan">Time to wait before the request times out.</param>
		/// <returns>This IFlurlClient or IFlurlRequest.</returns>
		public static T WithTimeout<T>(this T obj, TimeSpan timespan) where T : IHttpSettingsContainer {
			obj.Settings.Timeout = timespan;
			return obj;
		}

		/// <summary>
		/// Sets the timeout for this IFlurlRequest or all requests made with this IFlurlClient.
		/// </summary>
		/// <param name="obj">The IFlurlClient or IFlurlRequest.</param>
		/// <param name="seconds">Seconds to wait before the request times out.</param>
		/// <returns>This IFlurlClient or IFlurlRequest.</returns>
		public static T WithTimeout<T>(this T obj, int seconds) where T : IHttpSettingsContainer {
			obj.Settings.Timeout = TimeSpan.FromSeconds(seconds);
			return obj;
		}

		/// <summary>
		/// Adds a pattern representing an HTTP status code or range of codes which (in addition to 2xx) will NOT result in a FlurlHttpException being thrown.
		/// </summary>
		/// <param name="obj">The IFlurlClient or IFlurlRequest.</param>
		/// <param name="pattern">Examples: "3xx", "100,300,600", "100-299,6xx"</param>
		/// <returns>This IFlurlClient or IFlurlRequest.</returns>
		public static T AllowHttpStatus<T>(this T obj, string pattern) where T : IHttpSettingsContainer {
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
		/// Adds an <see cref="HttpStatusCode" /> which (in addition to 2xx) will NOT result in a FlurlHttpException being thrown.
		/// </summary>
		/// <param name="obj">The IFlurlClient or IFlurlRequest.</param>
		/// <param name="statusCodes">Examples: HttpStatusCode.NotFound</param>
		/// <returns>This IFlurlClient or IFlurlRequest.</returns>
		public static T AllowHttpStatus<T>(this T obj, params HttpStatusCode[] statusCodes) where T : IHttpSettingsContainer {
			var pattern = string.Join(",", statusCodes.Select(c => (int)c));
			return AllowHttpStatus(obj, pattern);
		}

		/// <summary>
		/// Prevents a FlurlHttpException from being thrown on any completed response, regardless of the HTTP status code.
		/// </summary>
		/// <returns>This IFlurlClient or IFlurlRequest.</returns>
		public static T AllowAnyHttpStatus<T>(this T obj) where T : IHttpSettingsContainer {
			obj.Settings.AllowedHttpStatusRange = "*";
			return obj;
		}
	}
}