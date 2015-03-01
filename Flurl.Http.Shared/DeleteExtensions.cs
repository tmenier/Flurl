using System.Net.Http;
using System.Threading.Tasks;

namespace Flurl.Http
{
	public static class DeleteExtensions
	{
		/// <summary>
		/// Sends an asynchronous DELETE request.
		/// </summary>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> DeleteAsync(this FlurlClient client) {
			return client.SendAsync(HttpMethod.Delete);
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous DELETE request.
		/// </summary>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> DeleteAsync(this string url) {
			return new FlurlClient(url, true).DeleteAsync();
		}

		/// <summary>
		/// Creates a FlurlClient from the URL and sends an asynchronous DELETE request.
		/// </summary>
		/// <returns>A Task whose result is the received HttpResponseMessage.</returns>
		public static Task<HttpResponseMessage> DeleteAsync(this Url url) {
			return new FlurlClient(url, true).DeleteAsync();
		}
	}
}
