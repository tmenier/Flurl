using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Flurl.Http
{
	public static class PostExtensions
	{
		public static async Task<T> PostJsonAsync<T>(this FlurlClient client, object data) {
			var content = (HttpContent)new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
			var resp = await client.HttpClient.PostAsync(client.Url, content);
			using (var stream = await resp.Content.ReadAsStreamAsync())
				return JsonHelper.ReadJsonFromStream<T>(stream);
		}

		public static Task<T> PostJsonAsync<T>(this string url, object data) {
			return new FlurlClient(url).PostJsonAsync<T>(data);
		}

		public static Task<T> PostJsonAsync<T>(this Url url, object data) {
			return new FlurlClient(url).PostJsonAsync<T>(data);
		}
	}
}
