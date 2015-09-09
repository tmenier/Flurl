using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Flurl.Util;

namespace Flurl.Http.Configuration
{
    public class DefaultUrlEncodedSerializer : ISerializer
    {
	    public string Serialize(object obj) {
			var sb = new StringBuilder();
			foreach (var kv in obj.ToKeyValuePairs()) {
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

	    public T Deserialize<T>(string s) {
		    throw new NotImplementedException("Deserializing to UrlEncoded not supported.");
	    }

	    public T Deserialize<T>(Stream stream) {
			throw new NotImplementedException("Deserializing to UrlEncoded not supported.");
	    }
    }
}
