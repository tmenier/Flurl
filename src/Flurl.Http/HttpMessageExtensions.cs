using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Flurl.Http.Content;
using Flurl.Util;

namespace Flurl.Http
{
	/// <summary>
	/// Extension methods off HttpRequestMessage and HttpResponseMessage.
	/// </summary>
	public static class HttpMessageExtensions
	{
		/// <summary>
		/// Set a header on this HttpRequestMessage (default), or its Content property if it's a known content-level header.
		/// No validation. Overwrites any existing value(s) for the header. 
		/// </summary>
		/// <param name="request">The HttpRequestMessage.</param>
		/// <param name="name">The header name.</param>
		/// <param name="value">The header value.</param>
		/// <param name="createContentIfNecessary">If it's a content-level header and there is no content, this determines whether to create an empty HttpContent or just ignore the header.</param>
		public static void SetHeader(this HttpRequestMessage request, string name, object value, bool createContentIfNecessary = true) {
			new HttpMessage(request).SetHeader(name, value, createContentIfNecessary);
		}

		/// <summary>
		/// Set a header on this HttpResponseMessage (default), or its Content property if it's a known content-level header.
		/// No validation. Overwrites any existing value(s) for the header. 
		/// </summary>
		/// <param name="response">The HttpResponseMessage.</param>
		/// <param name="name">The header name.</param>
		/// <param name="value">The header value.</param>
		/// <param name="createContentIfNecessary">If it's a content-level header and there is no content, this determines whether to create an empty HttpContent or just ignore the header.</param>
		public static void SetHeader(this HttpResponseMessage response, string name, object value, bool createContentIfNecessary = true) {
			new HttpMessage(response).SetHeader(name, value, createContentIfNecessary);
		}

		private static void SetHeader(this HttpMessage msg, string name, object value, bool createContentIfNecessary) {
			switch (name.ToLower()) {
				// https://docs.microsoft.com/en-us/dotnet/api/system.net.http.headers.httpcontentheaders
				case "allow":
				case "content-disposition":
				case "content-encoding":
				case "content-language":
				case "content-length":
				case "content-location":
				case "content-md5":
				case "content-range":
				case "content-type":
				case "expires":
				case "last-modified":
					// it's a content-level header
					if (msg.Content != null) {
						msg.Content.Headers.Remove(name);
					}
					else if (createContentIfNecessary && value != null) {
						msg.Content = new CapturedStringContent("");
						msg.Content.Headers.Clear();
					}
					else {
						break;
					}

					if (value != null)
						msg.Content.Headers.TryAddWithoutValidation(name, new[] { value.ToInvariantString() });
					break;
				default:
					// it's a request/response-level header
					if (!name.OrdinalEquals("Set-Cookie", true)) // multiple set-cookie headers are allowed
						msg.Headers.Remove(name);
					if (value != null)
						msg.Headers.TryAddWithoutValidation(name, new[] { value.ToInvariantString() });
					break;
			}
		}

		/// <summary>
		/// Wrapper class for treating HttpRequestMessage and HttpResponseMessage uniformly. (Unfortunately they don't have a common interface.)
		/// </summary>
		private class HttpMessage
		{
			private readonly HttpRequestMessage _request;
			private readonly HttpResponseMessage _response;

			public HttpHeaders Headers => _request?.Headers as HttpHeaders ?? _response?.Headers;

			public HttpContent Content {
				get => _request?.Content ?? _response?.Content;
				set {
					if (_request != null) _request.Content = value;
					else _response.Content = value;
				}
			}

			public HttpMessage(HttpRequestMessage request) {
				_request = request;
			}

			public HttpMessage(HttpResponseMessage response) {
				_response = response;
			}
		}
	}
}
