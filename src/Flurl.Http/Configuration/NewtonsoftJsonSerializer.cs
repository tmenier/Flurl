using System.IO;
using Newtonsoft.Json;

namespace Flurl.Http.Configuration
{
	/// <summary>
	/// ISerializer implementation that uses Newtonsoft Json.NET.
	/// Default serializer used in calls to GetJsonAsync, PostJsonAsync, etc.
	/// </summary>
	public class NewtonsoftJsonSerializer : ISerializer
	{
		private readonly JsonSerializerSettings _settings;

		/// <summary>
		/// Initializes a new instance of the <see cref="NewtonsoftJsonSerializer"/> class.
		/// </summary>
		/// <param name="settings">The settings.</param>
		public NewtonsoftJsonSerializer(JsonSerializerSettings settings) {
			_settings = settings;
		}

		/// <summary>
		/// Serializes the specified object.
		/// </summary>
		/// <param name="obj">The object.</param>
		/// <returns></returns>
		public string Serialize(object obj) {
			return JsonConvert.SerializeObject(obj, _settings);
		}

		/// <summary>
		/// Deserializes the specified s.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="s">The s.</param>
		/// <returns></returns>
		public T Deserialize<T>(string s) {
			return JsonConvert.DeserializeObject<T>(s, _settings);
		}

		/// <summary>
		/// Deserializes the specified stream.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="stream">The stream.</param>
		/// <returns></returns>
		public T Deserialize<T>(Stream stream) {
			// https://www.newtonsoft.com/json/help/html/Performance.htm#MemoryUsage
			using (var sr = new StreamReader(stream))
			using (var jr = new JsonTextReader(sr)) {
				return JsonSerializer.CreateDefault(_settings).Deserialize<T>(jr);
			}
		}
	}
}