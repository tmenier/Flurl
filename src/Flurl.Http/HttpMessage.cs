using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http.Content;
using Flurl.Util;

namespace Flurl.Http
{
	/// <summary>
	/// Wrapper class for treating HttpRequestMessage and HttpResponseMessage uniformly. (Unfortunately they don't have a common interface.)
	/// </summary>
	internal class HttpMessage
	{
		private readonly HttpRequestMessage _request;
		private readonly HttpResponseMessage _response;

		public HttpHeaders Headers => _request?.Headers as HttpHeaders ?? _response?.Headers;

		public HttpContent Content
		{
			get => _request?.Content ?? _response?.Content;
			set
			{
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

		public void SetHeader(string name, object value, bool createContentIfNecessary) {
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
					if (Content == null && (!createContentIfNecessary || value == null))
						break;

					if (Content == null) {
						Content = new CapturedStringContent("");
						Content.Headers.Clear();
					}
					else {
						Content.Headers.Remove(name);
					}

					if (value != null)
						Content.Headers.TryAddWithoutValidation(name, new[] { value.ToInvariantString() });
					break;
				default:
					// it's a request-level header
					Headers.Remove(name);
					if (value != null)
						Headers.TryAddWithoutValidation(name, new[] { value.ToInvariantString() });
					break;
			}
		}

		public string GetHeaderValue(string name) {
			return (Headers.TryGetValues(name, out var vals) || Content?.Headers.TryGetValues(name, out vals) == true) ?
				string.Join(",", vals) : null; // multiple values should be comma delimited https://stackoverflow.com/a/3097052/62600
		}
	}
}
