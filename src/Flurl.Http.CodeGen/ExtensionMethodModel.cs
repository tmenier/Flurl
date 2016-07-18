using System.Collections.Generic;
using System.Linq;

namespace Flurl.Http.CodeGen
{
	public class ExtensionMethodModel
	{
		public static IEnumerable<ExtensionMethodModel> GetAll() {
			return
				from httpVerb in new[] { null, "Get", "Post", "Head", "Put", "Delete", "Patch" }
				from bodyType in new[] { null, "Json", /*"Xml",*/ "String", "UrlEncoded" }
				from extensionType in new[] { "FlurlClient", "Url", "string" }
				where SupportedCombo(httpVerb, bodyType, extensionType)
				from deserializeType in new[] { null, "Json", "JsonList", /*"Xml",*/ "String", "Stream", "Bytes" }
				where httpVerb == "Get" || deserializeType == null
				from isGeneric in new[] { true, false }
				where AllowDeserializeToGeneric(deserializeType) || !isGeneric
				select new ExtensionMethodModel {
					HttpVerb = httpVerb,
					BodyType = bodyType,
					ExtentionOfType = extensionType,
					DeserializeToType = deserializeType,
					IsGeneric = isGeneric
				};
		}

		private static bool SupportedCombo(string verb, string bodyType, string extensionType) {
			switch (verb) {
				case null: // Send
					return bodyType != null || extensionType != "FlurlClient";
				case "Post":
					return true;
				case "Put":
				case "Patch":
					return bodyType != "UrlEncoded";
				default: // Get, Head, Delete
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
		public string BodyType { get; set; }
		public string ExtentionOfType { get; set; }
		public string DeserializeToType { get; set; }
		public bool IsGeneric { get; set; }

		public string Name => $"{HttpVerb ?? "Send"}{BodyType ?? DeserializeToType}Async";

		public string TaskArg {
			get {
				switch (DeserializeToType) {
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
				switch (DeserializeToType) {
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
