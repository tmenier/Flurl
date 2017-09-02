using System;
using System.Collections.Generic;
using System.Text;

namespace Flurl.Http.CodeGen
{
	/// <summary>
	/// Extension methods manually defined on IFlurlRequest and IFlurlClient. We'll auto-gen overloads for Url and string. 
	/// </summary>
	public class UrlExtensionMethod
	{
		public static IEnumerable<UrlExtensionMethod> GetAll() {
			// header extensions
			yield return new UrlExtensionMethod("WithHeader", "Creates a new FlurlRequest with the URL and sets a request header.")
				.AddParam("name", "string", "The header name.")
				.AddParam("value", "object", "The header value.");
			yield return new UrlExtensionMethod("WithHeaders", "Creates a new FlurlRequest with the URL and sets request headers based on property names/values of the provided object, or keys/values if object is a dictionary, to be sent")
				.AddParam("headers", "object", "Names/values of HTTP headers to set. Typically an anonymous object or IDictionary.");
			yield return new UrlExtensionMethod("WithBasicAuth", "Creates a new FlurlRequest with the URL and sets the Authorization header according to Basic Authentication protocol.")
				.AddParam("username", "string", "Username of authenticating user.")
				.AddParam("password", "string", "Password of authenticating user.");
			yield return new UrlExtensionMethod("WithOAuthBearerToken", "Creates a new FlurlRequest with the URL and sets the Authorization header with a bearer token according to OAuth 2.0 specification.")
				.AddParam("token", "string", "The acquired oAuth bearer token.");

			// cookie extensions
			yield return new UrlExtensionMethod("EnableCookies", "Creates a new FlurlRequest with the URL and allows cookies to be sent and received. Not necessary to call when setting cookies via WithCookie/WithCookies.");
			yield return new UrlExtensionMethod("WithCookie", "Creates a new FlurlRequest with the URL and sets an HTTP cookie to be sent")
				.AddParam("cookie", "Cookie", "");
			yield return new UrlExtensionMethod("WithCookie", "Creates a new FlurlRequest with the URL and sets an HTTP cookie to be sent.")
				.AddParam("name", "string", "The cookie name.")
				.AddParam("value", "object", "The cookie value.")
				.AddParam("expires", "DateTime?", "The cookie expiration (optional). If excluded, cookie only lives for duration of session.", "null");
			yield return new UrlExtensionMethod("WithCookies", "Creates a new FlurlRequest with the URL and sets HTTP cookies to be sent, based on property names / values of the provided object, or keys / values if object is a dictionary.")
				.AddParam("cookies", "object", "Names/values of HTTP cookies to set. Typically an anonymous object or IDictionary.")
				.AddParam("expires", "DateTime?", "Expiration for all cookies (optional). If excluded, cookies only live for duration of session.", "null");

			// settings extensions
			yield return new UrlExtensionMethod("ConfigureRequest", "Configure", "Creates a new FlurlRequest with the URL and allows changing its Settings inline.")
				.AddParam("action", "Action<FlurlHttpSettings>", "A delegate defining the Settings changes.");
			yield return new UrlExtensionMethod("WithTimeout", "Creates a new FlurlRequest with the URL and sets the request timeout.")
				.AddParam("timespan", "TimeSpan", "Time to wait before the request times out.");
			yield return new UrlExtensionMethod("WithTimeout", "Creates a new FlurlRequest with the URL and sets the request timeout.")
				.AddParam("seconds", "int", "Seconds to wait before the request times out.");
			yield return new UrlExtensionMethod("AllowHttpStatus", "Creates a new FlurlRequest with the URL and adds a pattern representing an HTTP status code or range of codes which (in addition to 2xx) will NOT result in a FlurlHttpException being thrown.")
				.AddParam("pattern", "string", "Examples: \"3xx\", \"100,300,600\", \"100-299,6xx\"");
			yield return new UrlExtensionMethod("AllowHttpStatus", "Creates a new FlurlRequest with the URL and adds an HttpStatusCode which (in addtion to 2xx) will NOT result in a FlurlHttpException being thrown.")
				.AddParam("statusCodes", "params HttpStatusCode[]", "The HttpStatusCode(s) to allow.");
			yield return new UrlExtensionMethod("AllowAnyHttpStatus", "Creates a new FlurlRequest with the URL and configures it to allow any returned HTTP status without throwing a FlurlHttpException.");
		}

		public string Name { get; }
		public string RequestMethodName { get; }
		public string Description { get; }
		public IList<Param> Params { get; } = new List<Param>();

		public UrlExtensionMethod(string name, string description) : this(name, name, description) { }

		public UrlExtensionMethod(string name, string requestMethodName, string description) {
			Name = name;
			RequestMethodName = requestMethodName;
			Description = description;
		}

		public UrlExtensionMethod AddParam(string name, string type, string description, string defaultVal = null) {
			Params.Add(new Param { Name = name, Type = type, Description = description, Default = defaultVal });
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
