using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Flurl.Http.Configuration
{
	/// <summary>
	/// Contract for serializing and deserializing objects.
	/// </summary>
    public interface ISerializer
    {
		/// <summary>
		/// Serializes an object to a string representation.
		/// </summary>
		/// <param name="obj">The object to serialize.</param>
		/// <returns>A JSON string representation of the object.</returns>
		string Serialize(object obj);

		/// <summary>
		/// Deserializes an object from a string representation.
		/// </summary>
		/// <typeparam name="T">The type of the object to deserialize to.</typeparam>
		/// <param name="s">The JSON to deserialize.</param>
		/// <returns>The deserialized object from the JSON string.</returns>
		T Deserialize<T>(string s);

		/// <summary>
		/// Deserializes an object from a stream representation.
		/// </summary>
		/// <typeparam name="T">The type of the object to deserialize to.</typeparam>
		/// <param name="stream">The JSON stream to deserialize.</param>
		/// <returns>The deserialized object from the JSON stram.</returns>
		T Deserialize<T>(Stream stream);
	}
}
