using System;
using System.Collections.Generic;
using System.Text;

namespace Flurl.Http.CodeGen
{
	public class ExtensionMethod
	{
		public static IEnumerable<ExtensionMethod> GetAllForUrlBuilder() {
			yield return new ExtensionMethod("AppendPathSegment", "Creates a new Url object from the string and appends a segment to the URL path, ensuring there is one and only one '/' character as a separator.")
				.AddParam("segment", "object", "The segment to append")
				.AddParam("fullyEncode", "bool", "If true, URL-encodes reserved characters such as '/', '+', and '%'. Otherwise, only encodes strictly illegal characters (including '%' but only when not followed by 2 hex characters).", "false");

			yield return new ExtensionMethod("AppendPathSegments", "Appends multiple segments to the URL path, ensuring there is one and only one '/' character as a seperator.")
				.AddParam("segments", "params object[]", "The segments to append");

			yield return new ExtensionMethod("AppendPathSegments", "Appends multiple segments to the URL path, ensuring there is one and only one '/' character as a seperator.")
				.AddParam("segments", "IEnumerable<object>", "The segments to append");

			yield return new ExtensionMethod("RemovePathSegment", "Removes the last path segment from the URL.");

			yield return new ExtensionMethod("RemovePath", "Removes the entire path component of the URL.");

			yield return new ExtensionMethod("SetQueryParam", "Creates a new Url object from the string and adds a parameter to the query, overwriting the value if name exists.")
				.AddParam("name", "string", "Name of query parameter")
				.AddParam("value", "object", "Value of query parameter")
				.AddParam("nullValueHandling", "NullValueHandling", "Indicates how to handle null values. Defaults to Remove (any existing)", "NullValueHandling.Remove");

			yield return new ExtensionMethod("SetQueryParam", "Creates a new Url object from the string and adds a parameter to the query, overwriting the value if name exists.")
				.AddParam("name", "string", "Name of query parameter")
				.AddParam("value", "string", "Value of query parameter")
				.AddParam("isEncoded", "bool", "Set to true to indicate the value is already URL-encoded. Defaults to false.", "false")
				.AddParam("nullValueHandling", "NullValueHandling", "Indicates how to handle null values. Defaults to Remove (any existing).", "NullValueHandling.Remove");

			yield return new ExtensionMethod("SetQueryParam", "Creates a new Url object from the string and adds a parameter without a value to the query, removing any existing value.")
				.AddParam("name", "string", "Name of query parameter");

			yield return new ExtensionMethod("SetQueryParams", "Creates a new Url object from the string, parses values object into name/value pairs, and adds them to the query, overwriting any that already exist.")
				.AddParam("values", "object", "Typically an anonymous object, ie: new { x = 1, y = 2 }")
				.AddParam("nullValueHandling", "NullValueHandling", "Indicates how to handle null values. Defaults to Remove (any existing)", "NullValueHandling.Remove");

			yield return new ExtensionMethod("SetQueryParams", "Creates a new Url object from the string and adds multiple parameters without values to the query.")
				.AddParam("names", "IEnumerable<string>", "Names of query parameters.");

			yield return new ExtensionMethod("SetQueryParams", "Creates a new Url object from the string and adds multiple parameters without values to the query.")
				.AddParam("names", "params string[]", "Names of query parameters");

			yield return new ExtensionMethod("RemoveQueryParam", "Creates a new Url object from the string and removes a name/value pair from the query by name.")
				.AddParam("name", "string", "Query string parameter name to remove");

			yield return new ExtensionMethod("RemoveQueryParams", "Creates a new Url object from the string and removes multiple name/value pairs from the query by name.")
				.AddParam("names", "params string[]", "Query string parameter names to remove");

			yield return new ExtensionMethod("RemoveQueryParams", "Creates a new Url object from the string and removes multiple name/value pairs from the query by name.")
				.AddParam("names", "IEnumerable<string>", "Query string parameter names to remove");

			yield return new ExtensionMethod("RemoveQuery", "Removes the entire query component of the URL.");

			yield return new ExtensionMethod("SetFragment", "Set the URL fragment fluently.")
				.AddParam("fragment", "string", "The part of the URL after #");

			yield return new ExtensionMethod("RemoveFragment", "Removes the URL fragment including the #.");

			yield return new ExtensionMethod("ResetToRoot", "Trims the URL to its root, including the scheme, any user info, host, and port (if specified).");
		}

		/// <summary>
		/// Extension methods manually defined on IFlurlRequest and IFlurlClient. We'll auto-gen overloads for Url, Uri, and string. 
		/// </summary>
		public static IEnumerable<ExtensionMethod> GetAllForHttp() {
			// header extensions
			yield return new ExtensionMethod("WithHeader", "Creates a new FlurlRequest and sets a request header.")
				.AddParam("name", "string", "The header name.")
				.AddParam("value", "object", "The header value.");
			yield return new ExtensionMethod("WithHeaders", "Creates a new FlurlRequest and sets request headers based on property names/values of the provided object, or keys/values if object is a dictionary, to be sent")
				.AddParam("headers", "object", "Names/values of HTTP headers to set. Typically an anonymous object or IDictionary.")
				.AddParam("replaceUnderscoreWithHyphen", "bool", "If true, underscores in property names will be replaced by hyphens. Default is true.", "true");
			yield return new ExtensionMethod("WithBasicAuth", "Creates a new FlurlRequest and sets the Authorization header according to Basic Authentication protocol.")
				.AddParam("username", "string", "Username of authenticating user.")
				.AddParam("password", "string", "Password of authenticating user.");
			yield return new ExtensionMethod("WithOAuthBearerToken", "Creates a new FlurlRequest and sets the Authorization header with a bearer token according to OAuth 2.0 specification.")
				.AddParam("token", "string", "The acquired oAuth bearer token.");

			// cookie extensions
			yield return new ExtensionMethod("EnableCookies", "Creates a new FlurlRequest and allows cookies to be sent and received. Not necessary to call when setting cookies via WithCookie/WithCookies.");
			yield return new ExtensionMethod("WithCookie", "Creates a new FlurlRequest and sets an HTTP cookie to be sent")
				.AddParam("cookie", "Cookie", "");
			yield return new ExtensionMethod("WithCookie", "Creates a new FlurlRequest and sets an HTTP cookie to be sent.")
				.AddParam("name", "string", "The cookie name.")
				.AddParam("value", "object", "The cookie value.")
				.AddParam("expires", "DateTime?", "The cookie expiration (optional). If excluded, cookie only lives for duration of session.", "null");
			yield return new ExtensionMethod("WithCookies", "Creates a new FlurlRequest and sets HTTP cookies to be sent, based on property names / values of the provided object, or keys / values if object is a dictionary.")
				.AddParam("cookies", "object", "Names/values of HTTP cookies to set. Typically an anonymous object or IDictionary.")
				.AddParam("expires", "DateTime?", "Expiration for all cookies (optional). If excluded, cookies only live for duration of session.", "null");

			// settings extensions
			yield return new ExtensionMethod("ConfigureRequest", "Creates a new FlurlRequest and allows changing its Settings inline.")
				.AddParam("action", "Action<FlurlHttpSettings>", "A delegate defining the Settings changes.");
			yield return new ExtensionMethod("WithTimeout", "Creates a new FlurlRequest and sets the request timeout.")
				.AddParam("timespan", "TimeSpan", "Time to wait before the request times out.");
			yield return new ExtensionMethod("WithTimeout", "Creates a new FlurlRequest and sets the request timeout.")
				.AddParam("seconds", "int", "Seconds to wait before the request times out.");
			yield return new ExtensionMethod("AllowHttpStatus", "Creates a new FlurlRequest and adds a pattern representing an HTTP status code or range of codes which (in addition to 2xx) will NOT result in a FlurlHttpException being thrown.")
				.AddParam("pattern", "string", "Examples: \"3xx\", \"100,300,600\", \"100-299,6xx\"");
			yield return new ExtensionMethod("AllowHttpStatus", "Creates a new FlurlRequest and adds an HttpStatusCode which (in addtion to 2xx) will NOT result in a FlurlHttpException being thrown.")
				.AddParam("statusCodes", "params HttpStatusCode[]", "The HttpStatusCode(s) to allow.");
			yield return new ExtensionMethod("AllowAnyHttpStatus", "Creates a new FlurlRequest and configures it to allow any returned HTTP status without throwing a FlurlHttpException.");

			yield return new ExtensionMethod("WithClient", "Creates a new FlurlRequest and configures it to use the given IFlurlClient.")
				.AddParam("client", "IFlurlClient", "The IFlurlClient to use to send the request.");

			yield return new ExtensionMethod("DownloadFileAsync", "Creates a new FlurlRequest and asynchronously downloads a file.")
				.AddParam("localFolderPath", "string", "Path of local folder where file is to be downloaded.")
				.AddParam("localFileName", "string", "Name of local file. If not specified, the source filename (last segment of the URL) is used.", "null")
				.AddParam("bufferSize", "int", "Buffer size in bytes. Default is 4096.", "4096")
				.AddParam("cancellationToken", "CancellationToken", "The token to monitor for cancellation requests.", "default(CancellationToken)")
				.Returns("Task<string>", "A Task whose result is the local path of the downloaded file.");

			yield return new ExtensionMethod("PostMultipartAsync", "Creates a FlurlRequest and sends an asynchronous multipart/form-data POST request.")
				.AddParam("buildContent", "Action<CapturedMultipartContent>", "A delegate for building the content parts.")
				.AddParam("cancellationToken", "CancellationToken", "The token to monitor for cancellation requests.", "default(CancellationToken)")
				.Returns("Task<IFlurlResponse>", "A Task whose result is the received IFlurlResponse.");
		}

		public string Name { get; }
		public string Description { get; }
		public IList<Param> Params { get; } = new List<Param>();
		public string ReturnType { get; set; } = "IFlurlRequest";
		public string ReturnDescrip { get; set; } = "The IFlurlRequest.";

		public ExtensionMethod(string name, string description) {
			Name = name;
			Description = description;
		}

		public ExtensionMethod AddParam(string name, string type, string description, string defaultVal = null) {
			Params.Add(new Param { Name = name, Type = type, Description = description, Default = defaultVal });
			return this;
		}

		public ExtensionMethod Returns(string type, string descrip) {
			ReturnType = type;
			ReturnDescrip = descrip;
			return this;
		}

		public class Param
		{
			public string Name { get; set; }
			public string Type { get; set; }
			public string Description { get; set; }
			public string Default { get; set; }
		}
	}
}
