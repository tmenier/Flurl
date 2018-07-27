using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Util;

namespace Flurl.Http
{
	/// <summary>
	/// Fluent extension methods for downloading a file.
	/// </summary>
	public static class DownloadExtensions
	{
		/// <summary>
		/// Asynchronously downloads a file at the specified URL.
		/// </summary>
		/// <param name="request">The flurl request.</param>
		/// <param name="localFolderPath">Path of local folder where file is to be downloaded.</param>
		/// <param name="localFileName">Name of local file. If not specified, the source filename (from Content-Dispostion header, or last segment of the URL) is used.</param>
		/// <param name="bufferSize">Buffer size in bytes. Default is 4096.</param>
		/// <returns>A Task whose result is the local path of the downloaded file.</returns>
		public static async Task<string> DownloadFileAsync(this IFlurlRequest request, string localFolderPath, string localFileName = null, int bufferSize = 4096) {
			using (var resp = await request.SendAsync(HttpMethod.Get, completionOption: HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false)) {
				localFileName =
					localFileName ??
					resp.Content?.Headers.ContentDisposition?.FileName?.StripQuotes() ??
					request.Url.Path.Split('/').Last();

				// http://codereview.stackexchange.com/a/18679
				using (var httpStream = await resp.Content.ReadAsStreamAsync().ConfigureAwait(false))
				using (var filestream = await FileUtil.OpenWriteAsync(localFolderPath, localFileName, bufferSize).ConfigureAwait(false)) {
					await httpStream.CopyToAsync(filestream, bufferSize).ConfigureAwait(false);
				}
			}

			return FileUtil.CombinePath(localFolderPath, localFileName);
		}

		/// <summary>
		/// Asynchronously downloads a file at the specified URL.
		/// </summary>
		/// <param name="url">The Url.</param>
		/// <param name="localFolderPath">Path of local folder where file is to be downloaded.</param>
		/// <param name="localFileName">Name of local file. If not specified, the source filename (last segment of the URL) is used.</param>
		/// <param name="bufferSize">Buffer size in bytes. Default is 4096.</param>
		/// <returns>A Task whose result is the local path of the downloaded file.</returns>
		public static Task<string> DownloadFileAsync(this string url, string localFolderPath, string localFileName = null, int bufferSize = 4096) {
			return new FlurlRequest(url).DownloadFileAsync(localFolderPath, localFileName, bufferSize);
		}

		/// <summary>
		/// Asynchronously downloads a file at the specified URL.
		/// </summary>
		/// <param name="url">The Url.</param>
		/// <param name="localFolderPath">Path of local folder where file is to be downloaded.</param>
		/// <param name="localFileName">Name of local file. If not specified, the source filename (last segment of the URL) is used.</param>
		/// <param name="bufferSize">Buffer size in bytes. Default is 4096.</param>
		/// <returns>A Task whose result is the local path of the downloaded file.</returns>
		public static Task<string> DownloadFileAsync(this Url url, string localFolderPath, string localFileName = null, int bufferSize = 4096) {
			return new FlurlRequest(url).DownloadFileAsync(localFolderPath, localFileName, bufferSize);
		}
	}
}