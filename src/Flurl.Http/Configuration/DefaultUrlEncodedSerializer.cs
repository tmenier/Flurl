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
		/// <summary>
		/// Serializes the specified object to a URL-encoded string.
		/// </summary>
		public string Serialize(object obj) {
			if (obj == null)
				return null;

			var qp = new QueryParamCollection();
			foreach (var kv in obj.ToKeyValuePairs())
				qp.AddOrReplace(kv.Key, kv.Value, false, NullValueHandling.Ignore);
			return qp.ToString(true);
		}

		/// <summary>
		/// Deserializing a URL-encoded string is not supported.
		/// </summary>
		public T Deserialize<T>(string s) => throw new NotImplementedException("Deserializing a URL-encoded string is not supported.");

		/// <summary>
		/// Deserializing a URL-encoded string is not supported.
		/// </summary>
		public T Deserialize<T>(Stream stream) => throw new NotImplementedException("Deserializing a URL-encoded string is not supported.");
	}
}