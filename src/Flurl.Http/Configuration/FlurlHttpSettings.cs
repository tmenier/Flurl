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
		// Values are dictionary-backed so we can check for key existence. Can't do null-coalescing
		// because if a setting is set to null at the request level, that should stick.
		private readonly IDictionary<string, object> _vals = new Dictionary<string, object>();

		private FlurlHttpSettings _defaults;

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
		public virtual FlurlHttpSettings Defaults {
			get => _defaults ?? FlurlHttp.GlobalSettings;
			set => _defaults = value;
		}

		/// <summary>
		/// Gets or sets the HTTP request timeout.
		/// </summary>
		public TimeSpan? Timeout {
			get => Get<TimeSpan?>();
			set => Set(value);
		}

		/// <summary>
		/// Gets or sets a pattern representing a range of HTTP status codes which (in addtion to 2xx) will NOT result in Flurl.Http throwing an Exception.
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
		public virtual void ResetDefaults() {
			_vals.Clear();
		}

		/// <summary>
		/// Gets a settings value from this instance if explicitly set, otherwise from the default settings that back this instance.
		/// </summary>
		internal T Get<T>([CallerMemberName]string propName = null) {
			var testVals = HttpTest.Current?.Settings._vals;
			return
				testVals?.ContainsKey(propName) == true ? (T)testVals[propName] :
				_vals.ContainsKey(propName) ? (T)_vals[propName] :
				Defaults != null ? (T)Defaults.Get<T>(propName) :
				default;
		}

		/// <summary>
		/// Sets a settings value for this instance.
		/// </summary>
		internal void Set<T>(T value, [CallerMemberName]string propName = null) {
			_vals[propName] = value;
		}
	}

	/// <summary>
	/// Global default settings for Flurl.Http
	/// </summary>
	public class GlobalFlurlHttpSettings : FlurlHttpSettings
	{
		internal GlobalFlurlHttpSettings() {
			ResetDefaults();
		}

		/// <summary>
		/// Defaults at the global level do not make sense and will always be null.
		/// </summary>
		public override FlurlHttpSettings Defaults {
			get => null;
			set => throw new Exception("Global settings cannot be backed by any higher-level defauts.");
		}

		/// <summary>
		/// Gets or sets the factory that defines creating, caching, and reusing FlurlClient instances and,
		/// by proxy, HttpClient instances.
		/// </summary>
		public IFlurlClientFactory FlurlClientFactory {
			get => Get<IFlurlClientFactory>();
			set => Set(value);
		}

		/// <summary>
		/// Resets all global settings to their default values.
		/// </summary>
		public override void ResetDefaults() {
			base.ResetDefaults();
			Timeout = TimeSpan.FromSeconds(100); // same as HttpClient
			JsonSerializer = new DefaultJsonSerializer();
			UrlEncodedSerializer = new DefaultUrlEncodedSerializer();
			FlurlClientFactory = new DefaultFlurlClientFactory();
			Redirects.Enabled = true;
			Redirects.AllowSecureToInsecure = false;
			Redirects.ForwardHeaders = false;
			Redirects.ForwardAuthorizationHeader = false;
			Redirects.MaxAutoRedirects = 10;
		}
	}
}
