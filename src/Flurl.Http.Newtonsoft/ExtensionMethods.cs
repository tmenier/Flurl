using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using Flurl.Http.Configuration;
using Newtonsoft.Json;

namespace Flurl.Http.Newtonsoft
{
	/// <summary>
	/// Extensions to Flurl objects that use Newtonsoft.Json.
	/// </summary>
	public static class ExtensionMethods
	{
		/// <summary>
		/// Deserializes JSON-formatted HTTP response body to a dynamic object.
		/// </summary>
		/// <returns>A Task whose result is a dynamic object containing data in the response body.</returns>
		public static async Task<dynamic> GetJsonAsync(this IFlurlResponse resp) {
			dynamic d = await resp.GetJsonAsync<ExpandoObject>().ConfigureAwait(false);
			return d;
		}

		/// <summary>
		/// Deserializes JSON-formatted HTTP response body to a list of dynamic objects.
		/// </summary>
		/// <returns>A Task whose result is a list of dynamic objects containing data in the response body.</returns>
		public static async Task<IList<dynamic>> GetJsonListAsync(this IFlurlResponse resp) {
			dynamic[] d = await resp.GetJsonAsync<ExpandoObject[]>().ConfigureAwait(false);
			return d;
		}

		/// <summary>
		/// Deserializes JSON-formatted HTTP response body to a dynamic object. Intended to chain off an async call.
		/// </summary>
		/// <returns>A Task whose result is a dynamic object containing data in the response body.</returns>
		public static async Task<dynamic> ReceiveJson(this Task<IFlurlResponse> response) {
			using var resp = await response.ConfigureAwait(false);
			if (resp == null) return null;
			return await resp.GetJsonAsync().ConfigureAwait(false);
		}

		/// <summary>
		/// Deserializes JSON-formatted HTTP response body to a list of dynamic objects. Intended to chain off an async call.
		/// </summary>
		/// <returns>A Task whose result is a list of dynamic objects containing data in the response body.</returns>
		public static async Task<IList<dynamic>> ReceiveJsonList(this Task<IFlurlResponse> response) {
			using var resp = await response.ConfigureAwait(false);
			if (resp == null) return null;
			return await resp.GetJsonListAsync().ConfigureAwait(false);
		}

		/// <summary>
		/// Shortcut to use NewtonsoftJsonSerializer with this IFlurlClientBuilder.
		/// </summary>
		/// <param name="builder">This IFlurlClientBuilder.</param>
		/// <param name="settings">Optional custom JsonSerializerSettings.</param>
		/// <returns></returns>
		public static IFlurlClientBuilder UseNewtonsoft(this IFlurlClientBuilder builder, JsonSerializerSettings settings = null) =>
			builder.WithSettings(fs => fs.JsonSerializer = new NewtonsoftJsonSerializer(settings));

		/// <summary>
		/// Shortcut to use NewtonsoftJsonSerializer with all FlurlClients registered in this cache.
		/// </summary>
		/// <param name="cache">This IFlurlClientCache.</param>
		/// <param name="settings">Optional custom JsonSerializerSettings.</param>
		/// <returns></returns>
		public static IFlurlClientCache UseNewtonsoft(this IFlurlClientCache cache, JsonSerializerSettings settings = null) =>
			cache.WithDefaults(builder => builder.UseNewtonsoft(settings));
	}
}
