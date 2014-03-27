using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;

namespace Flurl.Http
{
	public static class GetExtensions
	{
		public static async Task<T> GetJsonAsync<T>(this FlurlClient client) {
			using (var stream = await client.HttpClient.GetStreamAsync(client.Url))
				return JsonHelper.ReadJsonFromStream<T>(stream);
		}

		public static Task<T> GetJsonAsync<T>(this string url) {
			return new FlurlClient(url).GetJsonAsync<T>();
		}

		public static Task<T> GetJsonAsync<T>(this Url url) {
			return new FlurlClient(url).GetJsonAsync<T>();
		}

		public static async Task<dynamic> GetJsonAsync(this FlurlClient client) {
			dynamic d = await client.GetJsonAsync<ExpandoObject>();
			return d;
		}

		public static Task<dynamic> GetJsonAsync(this string url) {
			return new FlurlClient(url).GetJsonAsync();
		}

		public static Task<dynamic> GetJsonAsync(this Url url) {
			return new FlurlClient(url).GetJsonAsync();
		}
	}
}
