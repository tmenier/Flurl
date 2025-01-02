using System;
using System.Net.Http;

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

			if (HasRequestBody) {
				if (reqBodyType == "Json")
					AddArg("body", "object", "An object representing the request body, which will be serialized to JSON.");
				else if (reqBodyType == "UrlEncoded")
					AddArg("body", "object", "An object representing the request body, which will be serialized to a URL-encoded string.");
				else if (reqBodyType == "String")
					AddArg("body", "string", "The request body.");
				else if (reqBodyType == null)
					AddArg("content", "HttpContent", "The request body content.", "null");
				else
					throw new Exception("how did we get here?");
			}

			AddArg("completionOption", "HttpCompletionOption", "The HttpCompletionOption used in the request. Optional.", $"HttpCompletionOption.{DefaultHttpCompletionOption}");
			AddArg("cancellationToken", "CancellationToken", "The token to monitor for cancellation requests.", "default");

			Returns($"Task<{TaskArg}>", $"A Task whose result is {ReturnTypeDescription}.");
		}

		public override string Name => $"{HttpVerb ?? "Send"}{RequestBodyType ?? ResponseBodyType}Async";

		public override string Description => string.Concat(
			(ExtendedTypeArg.Type == "IFlurlRequest") ? "Sends" : "Creates a FlurlRequest and sends",
			" an asynchronous ",
			(HttpVerb == null) ? "request." : $"{HttpVerb.ToUpperInvariant()} request.");

		public bool HasRequestBody => HttpVerb is "Post" or "Put" or "Patch" or null;

		public string HttpVerb { get; }
		public string RequestBodyType { get; }
		public string ResponseBodyType { get; }

		public string TaskArg => ResponseBodyType switch {
			"Json" => IsGeneric ? "T" : "dynamic",
			"JsonList" => "IList<dynamic>",
			"String" => "string",
			"Stream" => "Stream",
			"Bytes" => "byte[]",
			_ => "IFlurlResponse"
		};

		public string ReturnTypeDescription => ResponseBodyType switch {
			"Json" => "the JSON response body deserialized to " + (IsGeneric ? "an object of type T" : "a dynamic"),
			"JsonList" => "the JSON response body deserialized to a list of dynamics",
			"String" => "the response body as a string",
			"Stream" => "the response body as a Stream",
			"Bytes" => "the response body as a byte array",
			_ => "the received IFlurlResponse"
		};

		public HttpCompletionOption DefaultHttpCompletionOption => ResponseBodyType switch {
			"Stream" => HttpCompletionOption.ResponseHeadersRead, // #630
			_ => HttpCompletionOption.ResponseContentRead
		};
	}
}
