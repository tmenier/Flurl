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
		/// Serializes the specified object.
		/// </summary>
		/// <param name="obj">The object.</param>
		public string Serialize(object obj) {
			if (obj == null)
				return null;

			var qp = new QueryParamCollection();
			qp.Merge(obj, NullValueHandling.Ignore);
			return qp.ToString(true);
		}

		/// <summary>
		/// Deserializes the specified s.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="s">The s.</param>
		/// <exception cref="NotImplementedException">Deserializing to UrlEncoded not supported.</exception>
		public T Deserialize<T>(string s) {
			throw new NotImplementedException("Deserializing to UrlEncoded is not supported.");
		}

		/// <summary>
		/// Deserializes the specified stream.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="stream">The stream.</param>
		/// <exception cref="NotImplementedException">Deserializing to UrlEncoded not supported.</exception>
		public T Deserialize<T>(Stream stream) {
			throw new NotImplementedException("Deserializing to UrlEncoded is not supported.");
		}
	}
}