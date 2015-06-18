using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Flurl.Http.Configuration
{
	/// <summary>
	/// A set of properties that affect Flurl.Http behavior. Generally set via static FlurlHttp.Configure method.
	/// </summary>
	public class FlurlHttpConfigurationOptions
	{
		internal FlurlHttpConfigurationOptions() {
			ResetDefaults();
		}

		/// <summary>
		/// Gets or sets the default timeout for every HTTP request.
		/// </summary>
		public TimeSpan DefaultTimeout { get; set; }

		/// <summary>
		/// Gets or sets a pattern representing a range of HTTP status codes which (in addtion to 2xx) will NOT result in Flurl.Http throwing an Exception.
		/// Examples: "3xx", "100,300,600", "100-299,6xx", "*" (allow everything)
		/// 2xx will never throw regardless of this setting.
		/// </summary>
		public string AllowedHttpStatusRange { get; set; }

		/// <summary>
		/// Gets or sets a factory used to create HttpClient object used in Flurl HTTP calls. Default value
		/// is an instance of DefaultHttpClientFactory. Custom factory implementations should generally
		/// inherit from DefaultHttpClientFactory, call base.CreateClient, and manipulate the returned HttpClient,
		/// otherwise functionality such as callbacks and most testing features will be lost.
		/// </summary>
		public IHttpClientFactory HttpClientFactory { get; set; }

		/// <summary>
		/// Allows global customization of the JSON.Net serializer settings. These allow you to add custom
		/// JsonConverters and alter other behaviors like date formatting and null handling. For more information,
		/// see: http://www.newtonsoft.com/json/help/html/SerializationSettings.htm
		/// </summary>
		public JsonSerializerSettings JsonSerializerSettings { get; set; }

		/// <summary>
		/// Gets or sets a callback that is called immediately before every HTTP request is sent.
		/// </summary>
		public Action<HttpCall> BeforeCall { get; set; }

		/// <summary>
		/// Gets or sets a callback that is asynchronously called immediately before every HTTP request is sent.
		/// </summary>
		public Func<HttpCall, Task> BeforeCallAsync { get; set; }

		/// <summary>
		/// Gets or sets a callback that is called immediately after every HTTP response is received.
		/// </summary>
		public Action<HttpCall> AfterCall { get; set; }

		/// <summary>
		/// Gets or sets a callback that is asynchronously called immediately after every HTTP response is received.
		/// </summary>
		public Func<HttpCall, Task> AfterCallAsync { get; set; }

		/// <summary>
		/// Gets or sets a callback that is called when an error occurs during any HTTP call, including when any non-success
		/// HTTP status code is returned in the response. Response should be null-checked if used in the event handler.
		/// </summary>
		public Action<HttpCall> OnError { get; set; }

		/// <summary>
		/// Gets or sets a callback that is asynchronously called when an error occurs during any HTTP call, including when any non-success
		/// HTTP status code is returned in the response. Response should be null-checked if used in the event handler.
		/// </summary>
		public Func<HttpCall, Task> OnErrorAsync { get; set; }

		/// <summary>
		/// Clear all custom global options and set default values.
		/// </summary>
		public void ResetDefaults() {
			DefaultTimeout = new HttpClient().Timeout;
			AllowedHttpStatusRange = null;
			HttpClientFactory = new DefaultHttpClientFactory();
			JsonSerializerSettings = new JsonSerializerSettings();
			BeforeCall = null;
			BeforeCallAsync = null;
			AfterCall = null;
			AfterCallAsync = null;
			OnError = null;
			OnErrorAsync = null;
		}
	}
}
