using System;
using System.IO;
using Flurl.Util;

namespace Flurl.Http.Configuration
{
	/// <summary>
	/// ISerializer implementation that converts an object representing name/value pairs to a URL-encoded string.
	/// Default serializer used in calls to PostUrlEncodedAsync, etc. 
	/// </summary>
	public class DefaultUrlEncodedSerializer : ISerializer
	{
		/// <inheritdoc cref="ISerializer.Serialize"/>
		public string Serialize(object obj) {
			if (obj == null)
				return null;

			var qp = new QueryParamCollection();
            foreach (var kv in obj.ToKeyValuePairs())
	            qp.Merge(kv.Key, kv.Value, false, NullValueHandling.Ignore);

			return qp.ToString(true);
		}

		/// <inheritdoc cref="ISerializer.Deserialize{T}(string)" />
		/// <exception cref="NotImplementedException">Deserializing to UrlEncoded not supported.</exception>
		public T Deserialize<T>(string s) {
			throw new NotImplementedException("Deserializing to UrlEncoded is not supported.");
		}

		/// <inheritdoc cref="ISerializer.Deserialize{T}(Stream)" />
		/// <exception cref="NotImplementedException">Deserializing to UrlEncoded not supported.</exception>
		public T Deserialize<T>(Stream stream) {
			throw new NotImplementedException("Deserializing to UrlEncoded is not supported.");
		}
	}
}