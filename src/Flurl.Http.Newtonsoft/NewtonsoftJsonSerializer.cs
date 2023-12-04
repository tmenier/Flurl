using System.IO;
using Flurl.Http.Configuration;
using Newtonsoft.Json;

namespace Flurl.Http.Newtonsoft
{
	/// <summary>
	/// ISerializer implementation based on Newtonsoft.Json.
	/// Default serializer used in calls to GetJsonAsync, PostJsonAsync, etc.
	/// </summary>
	public class NewtonsoftJsonSerializer : ISerializer
	{
		private readonly JsonSerializerSettings _settings;

		/// <summary>
		/// Initializes a new instance of the <see cref="NewtonsoftJsonSerializer"/> class.
		/// </summary>
		/// <param name="settings">Settings to control (de)serialization behavior.</param>
		public NewtonsoftJsonSerializer(JsonSerializerSettings settings = null) {
			_settings = settings;
		}

		/// <summary>
		/// Serializes the specified object to a JSON string.
		/// </summary>
		/// <param name="obj">The object to serialize.</param>
		public string Serialize(object obj) => JsonConvert.SerializeObject(obj, _settings);

		/// <summary>
		/// Deserializes the specified JSON string to an object of type T.
		/// </summary>
		/// <param name="s">The JSON string to deserialize.</param>
		public T Deserialize<T>(string s) => JsonConvert.DeserializeObject<T>(s, _settings);

		/// <summary>
		/// Deserializes the specified stream to an object of type T.
		/// </summary>
		/// <param name="stream">The stream to deserialize.</param>
		public T Deserialize<T>(Stream stream) {
			// https://www.newtonsoft.com/json/help/html/Performance.htm#MemoryUsage
			using var sr = new StreamReader(stream);
			using var jr = new JsonTextReader(sr);
			return JsonSerializer.CreateDefault(_settings).Deserialize<T>(jr);
		}
	}
}