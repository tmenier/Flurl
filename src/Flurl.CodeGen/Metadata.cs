﻿using System.Collections.Generic;
using System.Linq;

namespace Flurl.CodeGen
{
	public static class Metadata
	{
		/// <summary>
		/// Mirrors methods defined on Url. We'll auto-gen them for Uri and string. 
		/// </summary>
		public static IEnumerable<ExtensionMethod> GetUrlReturningExtensions(MethodArg extendedArg) {
			ExtensionMethod Create(string name, string descrip) => new ExtensionMethod(name, descrip)
				.Extends(extendedArg)
				.Returns("Url", "A new Flurl.Url object.");

			yield return Create("AppendPathSegment", "Creates a new Url object from the string and appends a segment to the URL path, ensuring there is one and only one '/' character as a separator.")
				.AddArg("segment", "object", "The segment to append")
				.AddArg("fullyEncode", "bool", "If true, URL-encodes reserved characters such as '/', '+', and '%'. Otherwise, only encodes strictly illegal characters (including '%' but only when not followed by 2 hex characters).", "false");

			yield return Create("AppendPathSegments", "Appends multiple segments to the URL path, ensuring there is one and only one '/' character as a separator.")
				.AddArg("segments", "params object[]", "The segments to append");

			yield return Create("AppendPathSegments", "Appends multiple segments to the URL path, ensuring there is one and only one '/' character as a separator.")
				.AddArg("segments", "IEnumerable<object>", "The segments to append");

			yield return Create("RemovePathSegment", "Removes the last path segment from the URL.");

			yield return Create("RemovePath", "Removes the entire path component of the URL.");

			yield return Create("SetQueryParam", "Creates a new Url object from the string and adds a parameter to the query, overwriting the value if name exists.")
				.AddArg("name", "string", "Name of query parameter")
				.AddArg("value", "object", "Value of query parameter")
				.AddArg("nullValueHandling", "NullValueHandling", "Indicates how to handle null values. Defaults to Remove (any existing)", "NullValueHandling.Remove");

			yield return Create("SetQueryParam", "Creates a new Url object from the string and adds a parameter to the query, overwriting the value if name exists.")
				.AddArg("name", "string", "Name of query parameter")
				.AddArg("value", "string", "Value of query parameter")
				.AddArg("isEncoded", "bool", "Set to true to indicate the value is already URL-encoded. Defaults to false.", "false")
				.AddArg("nullValueHandling", "NullValueHandling", "Indicates how to handle null values. Defaults to Remove (any existing).", "NullValueHandling.Remove");

			yield return Create("SetQueryParam", "Creates a new Url object from the string and adds a parameter without a value to the query, removing any existing value.")
				.AddArg("name", "string", "Name of query parameter");

			yield return Create("SetQueryParams", "Creates a new Url object from the string, parses values object into name/value pairs, and adds them to the query, overwriting any that already exist.")
				.AddArg("values", "object", "Typically an anonymous object, ie: new { x = 1, y = 2 }")
				.AddArg("nullValueHandling", "NullValueHandling", "Indicates how to handle null values. Defaults to Remove (any existing)", "NullValueHandling.Remove");

			yield return Create("SetQueryParams", "Creates a new Url object from the string and adds multiple parameters without values to the query.")
				.AddArg("names", "IEnumerable<string>", "Names of query parameters.");

			yield return Create("SetQueryParams", "Creates a new Url object from the string and adds multiple parameters without values to the query.")
				.AddArg("names", "params string[]", "Names of query parameters");

			yield return Create("AppendQueryParam", "Creates a new Url object from the string and adds a parameter to the query.")
				.AddArg("name", "string", "Name of query parameter")
				.AddArg("value", "object", "Value of query parameter")
				.AddArg("nullValueHandling", "NullValueHandling", "Indicates how to handle null values. Defaults to Remove (any existing)", "NullValueHandling.Remove");

			yield return Create("AppendQueryParam", "Creates a new Url object from the string and adds a parameter to the query.")
				.AddArg("name", "string", "Name of query parameter")
				.AddArg("value", "string", "Value of query parameter")
				.AddArg("isEncoded", "bool", "Set to true to indicate the value is already URL-encoded. Defaults to false.", "false")
				.AddArg("nullValueHandling", "NullValueHandling", "Indicates how to handle null values. Defaults to Remove (any existing).", "NullValueHandling.Remove");

			yield return Create("AppendQueryParam", "Creates a new Url object from the string and adds a parameter without a value to the query.")
				.AddArg("name", "string", "Name of query parameter");

			yield return Create("AppendQueryParam", "Creates a new Url object from the string, parses values object into name/value pairs, and adds them to the query, overwriting any that already exist.")
				.AddArg("values", "object", "Typically an anonymous object, ie: new { x = 1, y = 2 }")
				.AddArg("nullValueHandling", "NullValueHandling", "Indicates how to handle null values. Defaults to Remove (any existing)", "NullValueHandling.Remove");

			yield return Create("AppendQueryParam", "Creates a new Url object from the string and adds multiple parameters without values to the query.")
				.AddArg("names", "IEnumerable<string>", "Names of query parameters.");

			yield return Create("AppendQueryParam", "Creates a new Url object from the string and adds multiple parameters without values to the query.")
				.AddArg("names", "params string[]", "Names of query parameters");


			yield return Create("RemoveQueryParam", "Creates a new Url object from the string and removes a name/value pair from the query by name.")
				.AddArg("name", "string", "Query string parameter name to remove");

			yield return Create("RemoveQueryParams", "Creates a new Url object from the string and removes multiple name/value pairs from the query by name.")
				.AddArg("names", "params string[]", "Query string parameter names to remove");

			yield return Create("RemoveQueryParams", "Creates a new Url object from the string and removes multiple name/value pairs from the query by name.")
				.AddArg("names", "IEnumerable<string>", "Query string parameter names to remove");

			yield return Create("RemoveQuery", "Removes the entire query component of the URL.");

			yield return Create("SetFragment", "Set the URL fragment fluently.")
				.AddArg("fragment", "string", "The part of the URL after #");

			yield return Create("RemoveFragment", "Removes the URL fragment including the #.");
			yield return Create("ResetToRoot", "Trims the URL to its root, including the scheme, any user info, host, and port (if specified).");
		}

		/// <summary>
		/// Mirrors methods defined on IFlurlRequest and IFlurlClient. We'll auto-gen them for Url, Uri, and string. 
		/// </summary>
		public static IEnumerable<ExtensionMethod> GetRequestReturningExtensions(MethodArg extendedArg) {
			ExtensionMethod Create(string name, string descrip) => new ExtensionMethod(name, descrip)
				.Extends(extendedArg)
				.Returns("IFlurlRequest", "A new IFlurlRequest.");

			// header extensions
			yield return Create("WithHeader", "Creates a new FlurlRequest and sets a request header.")
				.AddArg("name", "string", "The header name.")
				.AddArg("value", "object", "The header value.");
			yield return Create("WithHeaders", "Creates a new FlurlRequest and sets request headers based on property names/values of the provided object, or keys/values if object is a dictionary, to be sent.")
				.AddArg("headers", "object", "Names/values of HTTP headers to set. Typically an anonymous object or IDictionary.")
				.AddArg("replaceUnderscoreWithHyphen", "bool", "If true, underscores in property names will be replaced by hyphens. Default is true.", "true");
			yield return Create("WithBasicAuth", "Creates a new FlurlRequest and sets the Authorization header according to Basic Authentication protocol.")
				.AddArg("username", "string", "Username of authenticating user.")
				.AddArg("password", "string", "Password of authenticating user.");
			yield return Create("WithOAuthBearerToken", "Creates a new FlurlRequest and sets the Authorization header with a bearer token according to OAuth 2.0 specification.")
				.AddArg("token", "string", "The acquired oAuth bearer token.");

			// cookie extensions
			yield return Create("WithCookie", "Creates a new FlurlRequest and adds a name-value pair to its Cookie header. " +
			                                  "To automatically maintain a cookie \"session\", consider using a CookieJar or CookieSession instead.")
				.AddArg("name", "string", "The cookie name.")
				.AddArg("value", "object", "The cookie value.");
			yield return Create("WithCookies", "Creates a new FlurlRequest and adds name-value pairs to its Cookie header based on property names/values of the provided object, or keys/values if object is a dictionary. " +
											   "To automatically maintain a cookie \"session\", consider using a CookieJar or CookieSession instead.")
				.AddArg("values", "object", "Names/values of HTTP cookies to set. Typically an anonymous object or IDictionary.");
			yield return Create("WithCookies", "Creates a new FlurlRequest and sets the CookieJar associated with this request, which will be updated with any Set-Cookie headers present in the response and is suitable for reuse in subsequent requests.")
				.AddArg("cookieJar", "CookieJar", "The CookieJar.");
			yield return Create("WithCookies", "Creates a new FlurlRequest and associates it with a new CookieJar, which will be updated with any Set-Cookie headers present in the response and is suitable for reuse in subsequent requests.")
				.AddArg("cookieJar", "CookieJar", "The created CookieJar, which can be reused in subsequent requests.", isOut: true);

			// settings extensions
			yield return Create("WithSettings", "Creates a new FlurlRequest and allows changing its Settings inline.")
				.AddArg("action", "Action<FlurlHttpSettings>", "A delegate defining the Settings changes.");
			yield return Create("WithTimeout", "Creates a new FlurlRequest and sets the request timeout.")
				.AddArg("timespan", "TimeSpan", "Time to wait before the request times out.");
			yield return Create("WithTimeout", "Creates a new FlurlRequest and sets the request timeout.")
				.AddArg("seconds", "int", "Seconds to wait before the request times out.");
			yield return Create("AllowHttpStatus", "Creates a new FlurlRequest and adds a pattern representing an HTTP status code or range of codes which (in addition to 2xx) will NOT result in a FlurlHttpException being thrown.")
				.AddArg("pattern", "string", "Examples: \"3xx\", \"100,300,600\", \"100-299,6xx\"");
			yield return Create("AllowHttpStatus", "Creates a new FlurlRequest and adds one or more response status codes which (in addition to 2xx) will NOT result in a FlurlHttpException being thrown.")
				.AddArg("statusCodes", "params int[]", "One or more response status codes that, when received, will not cause an exception to be thrown.");
			yield return Create("AllowAnyHttpStatus", "Creates a new FlurlRequest and configures it to allow any returned HTTP status without throwing a FlurlHttpException.");
			yield return Create("WithAutoRedirect", "Creates a new FlurlRequest and configures whether redirects are automatically followed.")
				.AddArg("enabled", "bool", "true if Flurl should automatically send a new request to the redirect URL, false if it should not.");

			// event handler extensions
			foreach (var name in new[] { "BeforeCall", "AfterCall", "OnError", "OnRedirect" }) {
				yield return Create(name, $"Creates a new FlurlRequest and adds a new {name} event handler.")
					.AddArg("action", "Action<FlurlCall>", $"Action to perform when the {name} event is raised.");
				yield return Create(name, $"Creates a new FlurlRequest and adds a new asynchronous {name} event handler.")
					.AddArg("action", "Func<FlurlCall, Task>", $"Async action to perform when the {name} event is raised.");
			}
		}

		/// <summary>
		/// HTTP-calling methods for all valid verb/content type combinations (PutStringAsync, PatchJsonAsync, etc),
		/// with generic and non-generic overloads.
		/// </summary>
		public static IEnumerable<HttpExtensionMethod> GetHttpCallingExtensions(MethodArg extendedArg) =>
			from verb in new[] { null, "Get", "Post", "Head", "Put", "Delete", "Patch", "Options" }
			from reqType in new[] { null, "Json", "String", "UrlEncoded" }
			from respType in new[] { null, "Json", "String", "Stream", "Bytes" }
			where IsSupportedCombo(verb, reqType, respType, extendedArg.Type)
			let isGenenric = (respType == "Json")
			select new HttpExtensionMethod(verb, isGenenric, reqType, respType) { ExtendedTypeArg = extendedArg };

		/// <summary>
		/// Additional HTTP-calling methods that return dynamic or IList&lt;dynamic&gt;, supported only with Flurl.Http.Newtonsoft.
		/// </summary>
		public static IEnumerable<HttpExtensionMethod> GetDynamicReturningExtensions(MethodArg extendedArg) =>
			from respType in new[] { "Json", "JsonList" }
			select new HttpExtensionMethod("Get", false, null, respType) { ExtendedTypeArg = extendedArg };

		public static IEnumerable<ExtensionMethod> GetMiscAsyncExtensions(MethodArg extendedArg) {
			// a couple oddballs

			yield return new ExtensionMethod("DownloadFileAsync", "Creates a new FlurlRequest and asynchronously downloads a file.")
				.Extends(extendedArg)
				.Returns("Task<string>", "A Task whose result is the local path of the downloaded file.")
				.AddArg("localFolderPath", "string", "Path of local folder where file is to be downloaded.")
				.AddArg("localFileName", "string", "Name of local file. If not specified, the source filename (last segment of the URL) is used.", "null")
				.AddArg("bufferSize", "int", "Buffer size in bytes. Default is 4096.", "4096")
				.AddArg("completionOption", "HttpCompletionOption", "The HttpCompletionOption used in the request. Optional.", "HttpCompletionOption.ResponseHeadersRead")
				.AddArg("cancellationToken", "CancellationToken", "The token to monitor for cancellation requests.", "default");

			yield return new ExtensionMethod("PostMultipartAsync", "Creates a FlurlRequest and sends an asynchronous multipart/form-data POST request.")
				.Extends(extendedArg)
				.Returns("Task<IFlurlResponse>", "A Task whose result is the received IFlurlResponse.")
				.AddArg("buildContent", "Action<CapturedMultipartContent>", "A delegate for building the content parts.")
				.AddArg("completionOption", "HttpCompletionOption", "The HttpCompletionOption used in the request. Optional.", "HttpCompletionOption.ResponseContentRead")
				.AddArg("cancellationToken", "CancellationToken", "The token to monitor for cancellation requests.", "default");
		}

		private static bool IsSupportedCombo(string verb, string reqType, string respType, string extensionType) {
			if (respType != null && verb != "Get")
				return false;

			switch (verb) {
				case "Post":
					return true;
				case "Put":
				case "Patch":
					return reqType != "UrlEncoded";
				case null: // Send
					return reqType != null || extensionType != "IFlurlRequest";
				default: // Get, Head, Delete, Options
					return reqType == null;
			}
		}
	}
}
