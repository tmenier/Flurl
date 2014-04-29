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
		public static Task<HttpResponseMessage> DeleteAsync(this FlurlClient client, object data) {
			return client.HttpClient.DeleteAsync(client.Url);
		}
	}
}
