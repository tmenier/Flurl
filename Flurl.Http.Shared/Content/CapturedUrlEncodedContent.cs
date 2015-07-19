using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;
using Flurl.Util;

namespace Flurl.Http.Content
{
	/// <summary>
	/// Provides HTTP content based on object serialized to URL-encoded name-value, with the with the captured to a property
	/// so it can be read without affecting the read-once content stream.
	/// </summary>
	public class CapturedUrlEncodedContent : CapturedStringContent
	{
		// This implementation was largely lifted from System.Net.Http.FormUrlEncodedContent, which unfortunately
		// doesn't have the hooks needed for getting the content body as a string.

		public CapturedUrlEncodedContent(object data) : base(GetContent(data)) {
			this.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
		}

		private static string GetContent(object data) {
			if (data == null)
				throw new ArgumentNullException("data");

			var sb = new StringBuilder();
			foreach (var kv in data.ToKeyValuePairs()) {
				if (kv.Value == null)
					continue;
				if (sb.Length > 0)
					sb.Append('&');
				sb.Append(Url.EncodeQueryParamValue(kv.Key, true));
				sb.Append('=');
				sb.Append(Url.EncodeQueryParamValue(kv.Value, true));
			}
			return sb.ToString();
		}
	}
}
