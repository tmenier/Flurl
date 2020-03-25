using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Flurl.Http.CodeGen
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

			yield return Create("AppendPathSegments", "Appends multiple segments to the URL path, ensuring there is one and only one '/' character as a seperator.")
				.AddArg("segments", "params object[]", "The segments to append");

			yield return Create("AppendPathSegments", "Appends multiple segments to the URL path, ensuring there is one and only one '/' character as a seperator.")
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
			yield return Create("WithHeaders", "Creates a new FlurlRequest and sets request headers based on property names/values of the provided object, or keys/values if object is a dictionary, to be sent")
				.AddArg("headers", "object", "Names/values of HTTP headers to set. Typically an anonymous object or IDictionary.")
				.AddArg("replaceUnderscoreWithHyphen", "bool", "If true, underscores in property names will be replaced by hyphens. Default is true.", "true");
			yield return Create("WithBasicAuth", "Creates a new FlurlRequest and sets the Authorization header according to Basic Authentication protocol.")
				.AddArg("username", "string", "Username of authenticating user.")
				.AddArg("password", "string", "Password of authenticating user.");
			yield return Create("WithOAuthBearerToken", "Creates a new FlurlRequest and sets the Authorization header with a bearer token according to OAuth 2.0 specification.")
				.AddArg("token", "string", "The acquired oAuth bearer token.");

			// cookie extensions
			yield return Create("EnableCookies", "Creates a new FlurlRequest and allows cookies to be sent and received. Not necessary to call when setting cookies via WithCookie/WithCookies.");
			yield return Create("WithCookie", "Creates a new FlurlRequest and sets an HTTP cookie to be sent")
				.AddArg("cookie", "Cookie", "");
			yield return Create("WithCookie", "Creates a new FlurlRequest and sets an HTTP cookie to be sent.")
				.AddArg("name", "string", "The cookie name.")
				.AddArg("value", "object", "The cookie value.")
				.AddArg("expires", "DateTime?", "The cookie expiration (optional). If excluded, cookie only lives for duration of session.", "null");
			yield return Create("WithCookies", "Creates a new FlurlRequest and sets HTTP cookies to be sent, based on property names / values of the provided object, or keys / values if object is a dictionary.")
				.AddArg("cookies", "object", "Names/values of HTTP cookies to set. Typically an anonymous object or IDictionary.")
				.AddArg("expires", "DateTime?", "Expiration for all cookies (optional). If excluded, cookies only live for duration of session.", "null");

			yield return Create("WithCookies", "Creates a new FlurlRequest and sets a collection of HTTP cookies that will be sent with it. May be modified when the response is received, if the server returns any cookies.")
				.AddArg("cookies", "IDictionary<string, Cookie>", "The cookies to send.");
			yield return Create("WithCookies", "Creates a new FlurlRequest and provides access to the collection that will receive HTTP cookies from the server, which can then be sent in subsequent requests.")
				.AddArg("cookies", "IDictionary<string, Cookie>", "The cookie collection.", isOut: true);

			// settings extensions
			yield return Create("ConfigureRequest", "Creates a new FlurlRequest and allows changing its Settings inline.")
				.AddArg("action", "Action<FlurlHttpSettings>", "A delegate defining the Settings changes.");
			yield return Create("WithTimeout", "Creates a new FlurlRequest and sets the request timeout.")
				.AddArg("timespan", "TimeSpan", "Time to wait before the request times out.");
			yield return Create("WithTimeout", "Creates a new FlurlRequest and sets the request timeout.")
				.AddArg("seconds", "int", "Seconds to wait before the request times out.");
			yield return Create("AllowHttpStatus", "Creates a new FlurlRequest and adds a pattern representing an HTTP status code or range of codes which (in addition to 2xx) will NOT result in a FlurlHttpException being thrown.")
				.AddArg("pattern", "string", "Examples: \"3xx\", \"100,300,600\", \"100-299,6xx\"");
			yield return Create("AllowHttpStatus", "Creates a new FlurlRequest and adds an HttpStatusCode which (in addtion to 2xx) will NOT result in a FlurlHttpException being thrown.")
				.AddArg("statusCodes", "params HttpStatusCode[]", "The HttpStatusCode(s) to allow.");
			yield return Create("AllowAnyHttpStatus", "Creates a new FlurlRequest and configures it to allow any returned HTTP status without throwing a FlurlHttpException.");
			yield return Create("WithAutoRedirect", "Creates a new FlurlRequest and configures whether redirects are automatically followed.")
				.AddArg("enabled", "bool", "true if Flurl should automatically send a new request to the redirect URL, false if it should not.");

			yield return Create("WithClient", "Creates a new FlurlRequest and configures it to use the given IFlurlClient.")
				.AddArg("client", "IFlurlClient", "The IFlurlClient to use to send the request.");

			// a couple oddballs that don't really belong here but here we are

			yield return new ExtensionMethod("DownloadFileAsync", "Creates a new FlurlRequest and asynchronously downloads a file.")
				.Returns("Task<string>", "A Task whose result is the local path of the downloaded file.")
				.Extends(extendedArg)
				.AddArg("localFolderPath", "string", "Path of local folder where file is to be downloaded.")
				.AddArg("localFileName", "string", "Name of local file. If not specified, the source filename (last segment of the URL) is used.", "null")
				.AddArg("bufferSize", "int", "Buffer size in bytes. Default is 4096.", "4096")
				.AddArg("cancellationToken", "CancellationToken", "The token to monitor for cancellation requests.", "default(CancellationToken)");

			yield return new ExtensionMethod("PostMultipartAsync", "Creates a FlurlRequest and sends an asynchronous multipart/form-data POST request.")
				.Returns("Task<IFlurlResponse>", "A Task whose result is the received IFlurlResponse.")
				.Extends(extendedArg)
				.AddArg("buildContent", "Action<CapturedMultipartContent>", "A delegate for building the content parts.")
				.AddArg("cancellationToken", "CancellationToken", "The token to monitor for cancellation requests.", "default(CancellationToken)");
		}

		/// <summary>
		/// HTTP-calling methods for all valid verb/content type combinations (PutStringAsync, PatchJsonAsync, etc),
		/// with generic and non-generic overloads.
		/// </summary>
		public static IEnumerable<HttpExtensionMethod> GetHttpCallingExtensions(MethodArg extendedArg) {
			return
				from verb in new[] { null, "Get", "Post", "Head", "Put", "Delete", "Patch", "Options" }
				from reqType in new[] { null, "Json", "String", "UrlEncoded" }
				from respType in new[] { null, "Json", "JsonList", "String", "Stream", "Bytes" }
				where IsSupportedCombo(verb, reqType, respType, extendedArg.Type)
				from isGeneric in new[] { true, false }
				where !isGeneric || AllowDeserializeToGeneric(respType)
				select new HttpExtensionMethod(verb, isGeneric, reqType, respType) { ExtendedTypeArg = extendedArg };
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

		private static bool AllowDeserializeToGeneric(string deserializeType) {
			switch (deserializeType) {
				case "Json":
					return true;
				default:
					return false;
			}
		}
	}
}
