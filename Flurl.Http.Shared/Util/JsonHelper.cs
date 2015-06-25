using System.IO;
using Newtonsoft.Json;

namespace Flurl.Http
{
	public static class JsonHelper
	{
		public static T ReadJsonFromStream<T>(Stream stream) {
			// http://james.newtonking.com/json/help/index.html?topic=html/Performance.htm
			using (var sr = new StreamReader(stream))
			using (var jr = new JsonTextReader(sr)) {
				return JsonSerializer.CreateDefault().Deserialize<T>(jr);
			}
		}
	}
}
