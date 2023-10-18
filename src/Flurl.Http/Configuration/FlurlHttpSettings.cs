using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Flurl.Http.Testing;

namespace Flurl.Http.Configuration
{
	/// <summary>
	/// A set of properties that affect Flurl.Http behavior
	/// </summary>
	public class FlurlHttpSettings
	{
		private static readonly FlurlHttpSettings Defaults = new() {
			Timeout = TimeSpan.FromSeconds(100), // same as HttpClient
			HttpVersion = "1.1",
			JsonSerializer = new DefaultJsonSerializer(),
			UrlEncodedSerializer = new DefaultUrlEncodedSerializer(),
			Redirects = {
				Enabled = true,
				AllowSecureToInsecure = false,
				ForwardHeaders = false,
				ForwardAuthorizationHeader = false,
				MaxAutoRedirects = 10
			}
		};

		// Values are dictionary-backed so we can check for key existence. Can't do null-coalescing
		// because if a setting is set to null at the request level, that should stick.
		private IDictionary<string, object> _vals = new Dictionary<string, object>();

		/// <summary>
		/// Creates a new FlurlHttpSettings object.
		/// </summary>
		public FlurlHttpSettings() {
			Redirects = new RedirectSettings(this);
			ResetDefaults();
		}

		/// <summary>
		/// Gets or sets the default values to fall back on when values are not explicitly set on this instance.
		/// </summary>
		internal FlurlHttpSettings Parent { get; set; }

		/// <summary>
		/// Gets or sets the HTTP request timeout.
		/// </summary>
		public TimeSpan? Timeout {
			get => Get<TimeSpan?>();
			set => Set(value);
		}

		/// <summary>
		/// Gets or sets the HTTP version to be used. Default is "1.1".
		/// </summary>
		public string HttpVersion {
			get => Get<Version>()?.ToString();
			set => Set(Version.TryParse(value, out var v) ? v : throw new ArgumentException("Invalid HTTP version: " + value));
		}

		/// <summary>
		/// Gets or sets a pattern representing a range of HTTP status codes which (in addition to 2xx) will NOT result in Flurl.Http throwing an Exception.
		/// Examples: "3xx", "100,300,600", "100-299,6xx", "*" (allow everything)
		/// 2xx will never throw regardless of this setting.
		/// </summary>
		public string AllowedHttpStatusRange {
			get => Get<string>();
			set => Set(value);
		}

		/// <summary>
		/// Gets or sets object used to serialize and deserialize JSON. Default implementation uses Newtonsoft Json.NET.
		/// </summary>
		public ISerializer JsonSerializer {
			get => Get<ISerializer>();
			set => Set(value);
		}

		/// <summary>
		/// Gets or sets object used to serialize URL-encoded data. (Deserialization not supported in default implementation.)
		/// </summary>
		public ISerializer UrlEncodedSerializer {
			get => Get<ISerializer>();
			set => Set(value);
		}

		/// <summary>
		/// Gets object whose properties describe how Flurl.Http should handle redirect (3xx) responses.
		/// </summary>
		public RedirectSettings Redirects { get; }

		/// <summary>
		/// Gets or sets a callback that is invoked immediately before every HTTP request is sent.
		/// </summary>
		public Action<FlurlCall> BeforeCall {
			get => Get<Action<FlurlCall>>();
			set => Set(value);
		}

		/// <summary>
		/// Gets or sets a callback that is invoked asynchronously immediately before every HTTP request is sent.
		/// </summary>
		public Func<FlurlCall, Task> BeforeCallAsync {
			get => Get<Func<FlurlCall, Task>>();
			set => Set(value);
		}

		/// <summary>
		/// Gets or sets a callback that is invoked immediately after every HTTP response is received.
		/// </summary>
		public Action<FlurlCall> AfterCall {
			get => Get<Action<FlurlCall>>();
			set => Set(value);
		}

		/// <summary>
		/// Gets or sets a callback that is invoked asynchronously immediately after every HTTP response is received.
		/// </summary>
		public Func<FlurlCall, Task> AfterCallAsync {
			get => Get<Func<FlurlCall, Task>>();
			set => Set(value);
		}

		/// <summary>
		/// Gets or sets a callback that is invoked when an error occurs during any HTTP call, including when any non-success
		/// HTTP status code is returned in the response. Response should be null-checked if used in the event handler.
		/// </summary>
		public Action<FlurlCall> OnError {
			get => Get<Action<FlurlCall>>();
			set => Set(value);
		}

		/// <summary>
		/// Gets or sets a callback that is invoked asynchronously when an error occurs during any HTTP call, including when any non-success
		/// HTTP status code is returned in the response. Response should be null-checked if used in the event handler.
		/// </summary>
		public Func<FlurlCall, Task> OnErrorAsync {
			get => Get<Func<FlurlCall, Task>>();
			set => Set(value);
		}

		/// <summary>
		/// Gets or sets a callback that is invoked when any 3xx response with a Location header is received.
		/// You can inspect/manipulate the call.Redirect object to determine what will happen next.
		/// An auto-redirect will only happen if call.Redirect.Follow is true upon exiting the callback.
		/// </summary>
		public Action<FlurlCall> OnRedirect {
			get => Get<Action<FlurlCall>>();
			set => Set(value);
		}

		/// <summary>
		/// Gets or sets a callback that is invoked asynchronously when any 3xx response with a Location header is received.
		/// You can inspect/manipulate the call.Redirect object to determine what will happen next.
		/// An auto-redirect will only happen if call.Redirect.Follow is true upon exiting the callback.
		/// </summary>
		public Func<FlurlCall, Task> OnRedirectAsync {
			get => Get<Func<FlurlCall, Task>>();
			set => Set(value);
		}

		/// <summary>
		/// Resets all overridden settings to their default values. For example, on a FlurlRequest,
		/// all settings are reset to FlurlClient-level settings.
		/// </summary>
		public void ResetDefaults() {
			_vals.Clear();
		}

		/// <summary>
		/// Gets a settings value from this instance if explicitly set, otherwise from the default settings that back this instance.
		/// </summary>
		internal T Get<T>([CallerMemberName]string propName = null) {
			IEnumerable<FlurlHttpSettings> prioritize() {
				yield return HttpTest.Current?.Settings;
				yield return this;
				yield return Parent;
				yield return Defaults;
			}

			foreach (var settings in prioritize())
				if (settings?._vals?.TryGetValue(propName, out var val) == true)
					return (T)val;

			return default; // should never get this far assuming Defaults is fully populated
		}

		/// <summary>
		/// Sets a settings value for this instance.
		/// </summary>
		internal void Set<T>(T value, [CallerMemberName]string propName = null) {
			_vals[propName] = value;
		}
	}
}
