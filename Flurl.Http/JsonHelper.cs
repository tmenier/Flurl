using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Flurl.Http
{
	public static class JsonHelper
	{
		public static T ReadJsonFromStream<T>(Stream stream) {
			// http://james.newtonking.com/json/help/index.html?topic=html/Performance.htm
			using (var sr = new StreamReader(stream))
			using (var jr = new JsonTextReader(sr)) {
				return new JsonSerializer().Deserialize<T>(jr);
			}
		}
	}
}
