using System.IO;

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
	    string Serialize(object obj);
		/// <summary>
		/// Deserializes an object from a string representation.
		/// </summary>
		T Deserialize<T>(string s);
		/// <summary>
		/// Deserializes an object from a stream representation.
		/// </summary>
		T Deserialize<T>(Stream stream);
    }
}
