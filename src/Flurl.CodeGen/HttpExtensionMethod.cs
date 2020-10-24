using System.Collections.Generic;
using System.Linq;

namespace Flurl.CodeGen
{
	public class HttpExtensionMethod : ExtensionMethod
	{
		public HttpExtensionMethod(string verb, bool isGeneric, string reqBodyType, string respBodyType) {
			HttpVerb = verb;
			IsGeneric = isGeneric;
			RequestBodyType = reqBodyType;
			ResponseBodyType = respBodyType;

			if (verb == null)
				AddArg("verb", "HttpMethod", "The HTTP verb used to make the request.");

			if (HasRequestBody)  {
				if (reqBodyType == "Json")
					AddArg("data", "object", "An object representing the request body, which will be serialized to JSON.");
				else if (reqBodyType == "String")
					AddArg("data", "string", "Contents of the request body.");
				else if (reqBodyType == null)
					AddArg("content", "HttpContent", "Contents of the request body.", "null");
				else
					AddArg("data", "object", "Contents of the request body.");
			}

			AddArg("cancellationToken", "CancellationToken", "The token to monitor for cancellation requests.", "default(CancellationToken)");
			AddArg("completionOption", "HttpCompletionOption", "The HttpCompletionOption used in the request. Optional.", "HttpCompletionOption.ResponseContentRead");

			Returns($"Task<{TaskArg}>", $"A Task whose result is {ReturnTypeDescription}.");
		}

		public override string Name => $"{HttpVerb ?? "Send"}{RequestBodyType ?? ResponseBodyType}Async";

		public override string Description => string.Concat(
			(ExtendedTypeArg.Type == "IFlurlRequest") ? "Sends" : "Creates a FlurlRequest and sends",
			" an asynchronous ",
			(HttpVerb == null) ? "request." : $"{HttpVerb.ToUpperInvariant()} request.");

		public bool HasRequestBody => (HttpVerb == "Post" || HttpVerb == "Put" || HttpVerb == "Patch" || HttpVerb == null);

		public string HttpVerb { get; }
		public string RequestBodyType { get; }
		public string ResponseBodyType { get; }

		public string TaskArg {
			get {
				switch (ResponseBodyType) {
					case "Json": return IsGeneric ? "T" : "dynamic";
					case "JsonList": return "IList<dynamic>";
					case "String": return "string";
					case "Stream": return "Stream";
					case "Bytes": return "byte[]";
					default: return "IFlurlResponse";
				}
			}
		}

		public string ReturnTypeDescription {
			get {
				//var response = (xm.DeserializeToType == null) ? "" : "" + xm.TaskArg;
				switch (ResponseBodyType) {
					case "Json": return "the JSON response body deserialized to " + (IsGeneric ? "an object of type T" : "a dynamic");
					case "JsonList": return "the JSON response body deserialized to a list of dynamics";
					case "String": return "the response body as a string";
					case "Stream": return "the response body as a Stream";
					case "Bytes": return "the response body as a byte array";
					default: return "the received IFlurlResponse";
				}
			}
		}
	}
}
