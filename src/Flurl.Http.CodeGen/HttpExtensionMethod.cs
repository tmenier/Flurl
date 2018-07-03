using System.Collections.Generic;
using System.Linq;

namespace Flurl.Http.CodeGen
{
	public class HttpExtensionMethod
	{
		public static IEnumerable<HttpExtensionMethod> GetAll() {
			return
				from httpVerb in new[] { null, "Get", "Post", "Head", "Put", "Delete", "Patch", "Options" }
				from reqType in new[] { null, "Json", /*"Xml",*/ "String", "UrlEncoded" }
				from extensionType in new[] { "IFlurlRequest", "Url", "string" }
				where SupportedCombo(httpVerb, reqType, extensionType)
				from respType in new[] { null, "Json", "JsonList", /*"Xml",*/ "String", "Stream", "Bytes" }
				where httpVerb == "Get" || respType == null
				from isGeneric in new[] { true, false }
				where AllowDeserializeToGeneric(respType) || !isGeneric
				select new HttpExtensionMethod {
					HttpVerb = httpVerb,
					RequestBodyType = reqType,
					ExtentionOfType = extensionType,
					ResponseBodyType = respType,
					IsGeneric = isGeneric
				};
		}

		private static bool SupportedCombo(string verb, string bodyType, string extensionType) {
			switch (verb) {
				case null: // Send
					return bodyType != null || extensionType != "IFlurlRequest";
				case "Post":
					return true;
				case "Put":
				case "Patch":
					return bodyType != "UrlEncoded";
				default: // Get, Head, Delete, Options
					return bodyType == null;
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

		public string HttpVerb { get; set; }
		public string RequestBodyType { get; set; }
		public string ExtentionOfType { get; set; }
		public string ResponseBodyType { get; set; }
		public bool IsGeneric { get; set; }

		public string Name => $"{HttpVerb ?? "Send"}{RequestBodyType ?? ResponseBodyType}Async";

		public string TaskArg {
			get {
				switch (ResponseBodyType) {
					case "Json": return IsGeneric ? "T" : "dynamic";
					case "JsonList": return "IList<dynamic>";
					//case "Xml": return ?;
					case "String": return "string";
					case "Stream": return "Stream";
					case "Bytes": return "byte[]";
					default: return "HttpResponseMessage";
				}
			}
		}

		public string ReturnTypeDescription {
			get {
				//var response = (xm.DeserializeToType == null) ? "" : "" + xm.TaskArg;
				switch (ResponseBodyType) {
					case "Json": return "the JSON response body deserialized to " + (IsGeneric ? "an object of type T" : "a dynamic");
					case "JsonList": return "the JSON response body deserialized to a list of dynamics";
					//case "Xml": return ?;
					case "String": return "the response body as a string";
					case "Stream": return "the response body as a Stream";
					case "Bytes": return "the response body as a byte array";
					default: return "the received HttpResponseMessage";
				}
			}
		}
	}
}
