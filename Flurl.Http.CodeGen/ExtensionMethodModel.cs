using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flurl.Http.CodeGen
{
	public class ExtensionMethodModel
	{
		public static IEnumerable<ExtensionMethodModel> GetAll() {
			return
				from httpVerb in new[] { "Get", "Post", "Head", "Put", "Delete", "Patch" }
				from bodyType in new[] { null, "Json", /*"Xml",*/ "String", "UrlEncoded" }
				where AllowRequestBody(httpVerb) || bodyType == null
				from extensionType in new[] { "FlurlClient", "Url", "string" }
				from deserializeType in new[] { null, "Json", "JsonList", /*"Xml",*/ "String", "Stream", "Bytes" }
				where httpVerb == "Get" || deserializeType == null
				from isGeneric in new[] { true, false }
				where AllowDeserializeToGeneric(deserializeType) || !isGeneric
				from hasCancelationToken in new[] { true, false }
				select new ExtensionMethodModel {
					HttpVerb = httpVerb,
					BodyType = bodyType,
					ExtentionOfType = extensionType,
					DeserializeToType = deserializeType,
					HasCancelationToken = hasCancelationToken,
					IsGeneric = isGeneric
				};
		}

		private static bool AllowRequestBody(string verb) {
			switch (verb) {
				case "Get":
				case "Head":
				case "Delete":
					return false;
				default:
					return true;
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
		public bool HasCancelationToken { get; set; }
		public bool IsGeneric { get; set; }

		public string Name {
			get { return string.Format("{0}{1}Async", HttpVerb, BodyType ?? DeserializeToType); }
		}

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
