using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Flurl.Http.Configuration
{
	/// <summary>
	/// A set of properties that affect Flurl.Http behavior
	/// </summary>
	public class FlurlHttpSettings
	{
		// We need to maintain order of precedence (request > client > global) in some tricky scenarios.
		// e.g. if we explicitly set some FlurlRequest.Settings, then set the FlurlClient, we want the
		// client-level settings to override the global settings but not the request-level settings.
		private FlurlHttpSettings _defaults;

		// Values are dictionary-backed so we can check for key existence. Can't do null-coalescing
		// because if a setting is set to null at the request level, that should stick.
		private IDictionary<string, object> _vals = new Dictionary<string, object>();

		/// <summary>
		/// Creates a new FlurlHttpSettings object using another FlurlHttpSettings object as its default values.
		/// </summary>
		public FlurlHttpSettings(FlurlHttpSettings defaults) {
			_defaults = defaults;
		}

		/// <summary>
		/// Creates a new FlurlHttpSettings object.
		/// </summary>
		public FlurlHttpSettings() : this(FlurlHttp.GlobalSettings) { }

		/// <summary>
		/// Gets or sets the HTTP request timeout.
		/// </summary>
		public TimeSpan? Timeout {
			get => Get(() => Timeout);
			set => Set(() => Timeout, value);
		}

		/// <summary>
		/// Gets or sets a pattern representing a range of HTTP status codes which (in addtion to 2xx) will NOT result in Flurl.Http throwing an Exception.
		/// Examples: "3xx", "100,300,600", "100-299,6xx", "*" (allow everything)
		/// 2xx will never throw regardless of this setting.
		/// </summary>
		public string AllowedHttpStatusRange {
			get => Get(() => AllowedHttpStatusRange);
			set => Set(() => AllowedHttpStatusRange, value);
		}

		/// <summary>
		/// Gets or sets a value indicating whether cookies should be sent/received with each HTTP request.
		/// </summary>
		public bool CookiesEnabled {
			get => Get(() => CookiesEnabled);
			set => Set(() => CookiesEnabled, value);
		}

		/// <summary>
		/// Gets or sets object used to serialize and deserialize JSON. Default implementation uses Newtonsoft Json.NET.
		/// </summary>
		public ISerializer JsonSerializer {
			get => Get(() => JsonSerializer);
			set => Set(() => JsonSerializer, value);
		}

		/// <summary>
		/// Gets or sets object used to serialize URL-encoded data. (Deserialization not supported in default implementation.)
		/// </summary>
		public ISerializer UrlEncodedSerializer {
			get => Get(() => UrlEncodedSerializer);
			set => Set(() => UrlEncodedSerializer, value);
		}

		/// <summary>
		/// Gets or sets a callback that is called immediately before every HTTP request is sent.
		/// </summary>
		public Action<HttpCall> BeforeCall {
			get => Get(() => BeforeCall);
			set => Set(() => BeforeCall, value);
		}

		/// <summary>
		/// Gets or sets a callback that is asynchronously called immediately before every HTTP request is sent.
		/// </summary>
		public Func<HttpCall, Task> BeforeCallAsync {
			get => Get(() => BeforeCallAsync);
			set => Set(() => BeforeCallAsync, value);
		}

		/// <summary>
		/// Gets or sets a callback that is called immediately after every HTTP response is received.
		/// </summary>
		public Action<HttpCall> AfterCall {
			get => Get(() => AfterCall);
			set => Set(() => AfterCall, value);
		}

		/// <summary>
		/// Gets or sets a callback that is asynchronously called immediately after every HTTP response is received.
		/// </summary>
		public Func<HttpCall, Task> AfterCallAsync {
			get => Get(() => AfterCallAsync);
			set => Set(() => AfterCallAsync, value);
		}

		/// <summary>
		/// Gets or sets a callback that is called when an error occurs during any HTTP call, including when any non-success
		/// HTTP status code is returned in the response. Response should be null-checked if used in the event handler.
		/// </summary>
		public Action<HttpCall> OnError {
			get => Get(() => OnError);
			set => Set(() => OnError, value);
		}

		/// <summary>
		/// Gets or sets a callback that is asynchronously called when an error occurs during any HTTP call, including when any non-success
		/// HTTP status code is returned in the response. Response should be null-checked if used in the event handler.
		/// </summary>
		public Func<HttpCall, Task> OnErrorAsync {
			get => Get(() => OnErrorAsync);
			set => Set(() => OnErrorAsync, value);
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
		protected T Get<T>(Expression<Func<T>> property) {
			var p = (property.Body as MemberExpression).Member as PropertyInfo;
			return
				_vals.ContainsKey(p.Name) ? (T)_vals[p.Name] :
				_defaults != null ? (T)p.GetValue(_defaults) :
				default(T);
		}

		/// <summary>
		/// Sets a settings value for this instance.
		/// </summary>
		protected void Set<T>(Expression<Func<T>> property, T value) {
			var p = (property.Body as MemberExpression).Member as PropertyInfo;
			_vals[p.Name] = value;
		}

		/// <summary>
		/// Merges other settings with this one. Overrides defaults, but does NOT override
		/// this settings' explicitly set values.
		/// </summary>
		/// <param name="other">The settings to merge.</param>
		public FlurlHttpSettings Merge(FlurlHttpSettings other) {
			_defaults = other;
			return this;
		}
	}

	/// <summary>
	/// Client-level settings for Flurl.Http
	/// </summary>
	public class ClientFlurlHttpSettings : FlurlHttpSettings
	{
		/// <summary>
		/// Creates a new FlurlHttpSettings object using another FlurlHttpSettings object as its default values.
		/// </summary>
		public ClientFlurlHttpSettings(FlurlHttpSettings defaults) : base(defaults) { }

		/// <summary>
		/// Gets or sets a factory used to create the HttpClient and HttpMessageHandler used for HTTP calls.
		/// Whenever possible, custom factory implementations should inherit from DefaultHttpClientFactory,
		/// only override the method(s) needed, call the base method, and modify the result.
		/// </summary>
		public IHttpClientFactory HttpClientFactory {
			get => Get(() => HttpClientFactory);
			set => Set(() => HttpClientFactory, value);
		}
	}

	/// <summary>
	/// Global default settings for Flurl.Http
	/// </summary>
	public class GlobalFlurlHttpSettings : ClientFlurlHttpSettings
	{
		internal GlobalFlurlHttpSettings() : base(null) {
			ResetDefaults();
		}

		/// <summary>
		/// Gets or sets the factory that defines creating, caching, and reusing FlurlClient instances and,
		/// by proxy, HttpClient instances.
		/// </summary>
		public IFlurlClientFactory FlurlClientFactory { get; set; }

		/// <summary>
		/// Resets all global settings to their Flurl.Http-defined default values.
		/// </summary>
		public override void ResetDefaults() {
			base.ResetDefaults();
			Timeout = TimeSpan.FromSeconds(100); // same as HttpClient
			CookiesEnabled = false;
			JsonSerializer = new NewtonsoftJsonSerializer(null);
			UrlEncodedSerializer = new DefaultUrlEncodedSerializer();
			FlurlClientFactory = new DefaultFlurlClientFactory();
			HttpClientFactory = new DefaultHttpClientFactory();
		}
	}
}
