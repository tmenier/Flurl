using System.IO;
using Newtonsoft.Json;

namespace Flurl.Http
{
	public static class JsonExtensions
	{
		/// <summary>
		/// Deserializes the Json structure contained by the specified Stream into an instance of the specified type.
		/// </summary>
		public static T Deserialize<T>(this JsonSerializer serializer, Stream stream) {
			// http://james.newtonking.com/json/help/index.html?topic=html/Performance.htm
			using (var sr = new StreamReader(stream))
			using (var jr = new JsonTextReader(sr)) {
				return serializer.Deserialize<T>(jr);
			}
		}
	}
}
