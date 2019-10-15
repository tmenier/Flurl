using System;
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
		/// <param name="settings">The <see cref="JsonSerializerSettings"/> used to deserialize the object.</param>
		public NewtonsoftJsonSerializer(JsonSerializerSettings settings) {
			_settings = settings;
		}

		/// <inheritdoc cref="ISerializer.Serialize"/>
		public string Serialize(object obj) {
			return JsonConvert.SerializeObject(obj, _settings);
		}

		/// <inheritdoc cref="ISerializer.Deserialize{T}(string)" />
		public T Deserialize<T>(string s) {
			return (T)Deserialize(s, typeof(T));
		}

		/// <inheritdoc cref="ISerializer.Deserialize{T}(Stream)" />
		public T Deserialize<T>(Stream stream) {
			return (T)Deserialize(stream, typeof(T));
		}

		/// <inheritdoc cref="ISerializer.Deserialize(string, Type)" />
		public object Deserialize(string s, Type type) {
			return JsonConvert.DeserializeObject(s, type, _settings);
		}

		/// <inheritdoc cref="ISerializer.Deserialize(Stream, Type)" />
		public object Deserialize(Stream stream, Type type) {
			// http://james.newtonking.com/json/help/index.html?topic=html/Performance.htm
			using (var sr = new StreamReader(stream))
			using (var jr = new JsonTextReader(sr)) {
				return JsonSerializer.CreateDefault(_settings).Deserialize(jr, type);
			}
		}
	}
}