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

		private static FlurlHttpSettings _baseDefaults = new FlurlHttpSettings(null) {
			FlurlClientFactory = new DefaultFlurlClientFactory(),
			CookiesEnabled = false,
			JsonSerializer = new NewtonsoftJsonSerializer(null),
			UrlEncodedSerializer = new DefaultUrlEncodedSerializer()
		};

		/// <summary>
		/// Creates a new FlurlHttpSettings object using another FlurlHttpSettings object as its default values.
		/// </summary>
		public FlurlHttpSettings(FlurlHttpSettings defaults) {
			_defaults = defaults;
		}

		/// <summary>
		/// Creates a new FlurlHttpSettings object.
		/// </summary>
		public FlurlHttpSettings() {
			_defaults = _baseDefaults;
		}

		/// <summary>
		/// Gets or sets a factory used to create HttpClient object used in Flurl HTTP calls. Default value
		/// is an instance of DefaultFlurlClientFactory. Custom factory implementations should generally
		/// inherit from DefaultFlurlClientFactory, call base.CreateClient, and manipulate the returned HttpClient,
		/// otherwise functionality such as callbacks and most testing features will be lost.
		/// </summary>
		public IFlurlClientFactory FlurlClientFactory {
			get => Get(() => FlurlClientFactory);
			set => Set(() => FlurlClientFactory, value);
		}

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
		/// Clears all custom options and resets them to their default values.
		/// </summary>
		public void ResetDefaults() {
			_vals.Clear();
		}

		private T Get<T>(Expression<Func<T>> property) {
			var p = (property.Body as MemberExpression).Member as PropertyInfo;
			return
				_vals.ContainsKey(p.Name) ? (T)_vals[p.Name] :
				_defaults != null ? (T)p.GetValue(_defaults) :
				default(T);
		}

		private void Set<T>(Expression<Func<T>> property, T value) {
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
}
