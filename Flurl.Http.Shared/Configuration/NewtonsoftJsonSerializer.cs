using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Flurl.Http.Configuration
{
	/// <summary>
	/// ISerializer implementation that uses Newtonsoft Json.NET. Used as Flurl.Http's default JSON serializer.
	/// </summary>
    public class NewtonsoftJsonSerializer : ISerializer
    {
	    private readonly JsonSerializerSettings _settings;

	    public NewtonsoftJsonSerializer(JsonSerializerSettings settings) {
		    _settings = settings;
	    }

	    public string Serialize(object obj) {
		    return JsonConvert.SerializeObject(obj, _settings);
	    }

	    public T Deserialize<T>(string s) {
		    return JsonConvert.DeserializeObject<T>(s, _settings);
	    }

	    public T Deserialize<T>(Stream stream) {
			// http://james.newtonking.com/json/help/index.html?topic=html/Performance.htm
			using (var sr = new StreamReader(stream))
			using (var jr = new JsonTextReader(sr)) {
				return JsonSerializer.CreateDefault(_settings).Deserialize<T>(jr);
			}
	    }
    }
}
