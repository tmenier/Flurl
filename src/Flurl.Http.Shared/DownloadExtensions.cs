#if NETSTD
using System.IO;
#elif PORTABLE
using PCLStorage;
#endif
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Flurl.Http
{
#if NETSTD
	public static class DownloadExtensions
	{
		/// <summary>
		/// Asynchronously downloads a file at the specified URL.
		/// </summary>
		/// <param name="localFolderPath">Path of local folder where file is to be downloaded.</param>
		/// <param name="localFileName">Name of local file. If not specified, the source filename (last segment of the URL) is used.</param>
		/// <param name="bufferSize">Buffer size in bytes. Default is 4096.</param>
		/// <returns>A Task whose result is the local path of the downloaded file.</returns>
		public static async Task<string> DownloadFileAsync(this FlurlClient client, string localFolderPath, string localFileName = null, int bufferSize = 4096) {
			if (localFileName == null)
				localFileName = client.Url.Path.Split('/').Last();

			if (!Directory.Exists(localFolderPath))
				Directory.CreateDirectory(localFolderPath);

			var filePath = Path.Combine(localFolderPath, localFileName);

			// need to temporarily disable autodispose if set, otherwise reading from stream will fail
			var autoDispose = client.AutoDispose;
			client.AutoDispose = false;

			try {
				var response = await client.SendAsync(HttpMethod.Get, completionOption: HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

				// http://codereview.stackexchange.com/a/18679
				using (var httpStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
				using (var filestream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, useAsync: true)) {
					await httpStream.CopyToAsync(filestream, bufferSize).ConfigureAwait(false);
				}

				return filePath;
			}
			finally {
				client.AutoDispose = autoDispose;
				if (client.AutoDispose) client.Dispose();
			}
		}

		/// <summary>
		/// Asynchronously downloads a file at the specified URL.
		/// </summary>
		/// <param name="localFolderPath">Path of local folder where file is to be downloaded.</param>
		/// <param name="localFileName">Name of local file. If not specified, the source filename (last segment of the URL) is used.</param>
		/// <param name="bufferSize">Buffer size in bytes. Default is 4096.</param>
		/// <returns>A Task whose result is the local path of the downloaded file.</returns>
		public static Task<string> DownloadFileAsync(this string url, string localFolderPath, string localFileName = null, int bufferSize = 4096) {
			return new FlurlClient(url, true).DownloadFileAsync(localFolderPath, localFileName, bufferSize);
		}

		/// <summary>
		/// Asynchronously downloads a file at the specified URL.
		/// </summary>
		/// <param name="localFolderPath">Path of local folder where file is to be downloaded.</param>
		/// <param name="localFileName">Name of local file. If not specified, the source filename (last segment of the URL) is used.</param>
		/// <param name="bufferSize">Buffer size in bytes. Default is 4096.</param>
		/// <returns>A Task whose result is the local path of the downloaded file.</returns>
		public static Task<string> DownloadFileAsync(this Url url, string localFolderPath, string localFileName = null, int bufferSize = 4096) {
			return new FlurlClient(url, true).DownloadFileAsync(localFolderPath, localFileName, bufferSize);
		}
	}

#elif PORTABLE
	public static class DownloadExtensions
	{
		/// <summary>
		/// Asynchronously downloads a file at the specified URL.
		/// </summary>
		/// <param name="localFolderPath">Path of local folder where file is to be downloaded.</param>
		/// <param name="localFileName">Name of local file. If not specified, the source filename (last segment of the URL) is used.</param>
		/// <param name="bufferSize">Buffer size in bytes. Default is 4096.</param>
		/// <returns>A Task whose result is the local path of the downloaded file.</returns>
		public static async Task<string> DownloadFileAsync(this FlurlClient client, string localFolderPath, string localFileName = null, int bufferSize = 4096)
		{
			if (localFileName == null)
				localFileName = client.Url.Path.Split('/').Last();

			var folder = await EnsureFolderAsync(localFolderPath).ConfigureAwait(false);
			var file = await folder.CreateFileAsync(localFileName, CreationCollisionOption.ReplaceExisting).ConfigureAwait(false);

			// need to temporarily disable autodispose if set, otherwise reading from stream will fail
			var autoDispose = client.AutoDispose;
			client.AutoDispose = false;

			try
			{
				var response = await client.SendAsync(HttpMethod.Get, completionOption: HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

				// http://codereview.stackexchange.com/a/18679
				using (var httpStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
				using (var filestream = await file.OpenAsync(FileAccess.ReadAndWrite).ConfigureAwait(false))
				{
					await httpStream.CopyToAsync(filestream, bufferSize).ConfigureAwait(false);
				}

				return PortablePath.Combine(localFolderPath, localFileName);
			}
			finally
			{
				client.AutoDispose = autoDispose;
				if (client.AutoDispose)
					client.Dispose();
			}
		}

		/// <summary>
		/// Asynchronously downloads a file at the specified URL.
		/// </summary>
		/// <param name="localFolderPath">Path of local folder where file is to be downloaded.</param>
		/// <param name="localFileName">Name of local file. If not specified, the source filename (last segment of the URL) is used.</param>
		/// <param name="bufferSize">Buffer size in bytes. Default is 4096.</param>
		/// <returns>A Task whose result is the local path of the downloaded file.</returns>
		public static Task<string> DownloadFileAsync(this string url, string localFolderPath, string localFileName = null, int bufferSize = 4096)
		{
			return new FlurlClient(url, true).DownloadFileAsync(localFolderPath, localFileName, bufferSize);
		}

		/// <summary>
		/// Asynchronously downloads a file at the specified URL.
		/// </summary>
		/// <param name="localFolderPath">Path of local folder where file is to be downloaded.</param>
		/// <param name="localFileName">Name of local file. If not specified, the source filename (last segment of the URL) is used.</param>
		/// <param name="bufferSize">Buffer size in bytes. Default is 4096.</param>
		/// <returns>A Task whose result is the local path of the downloaded file.</returns>
		public static Task<string> DownloadFileAsync(this Url url, string localFolderPath, string localFileName = null, int bufferSize = 4096)
		{
			return new FlurlClient(url, true).DownloadFileAsync(localFolderPath, localFileName, bufferSize);
		}

		private static Task<IFolder> EnsureFolderAsync(string path)
		{
			return FileSystem.Current.LocalStorage.CreateFolderAsync(path, CreationCollisionOption.OpenIfExists);
		}
	}
#endif
}