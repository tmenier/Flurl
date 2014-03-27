using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using PCLStorage;

namespace Flurl.Http
{
	public static class DownloadExtensions
	{
		public static async Task<string> DownloadAsync(this FlurlClient client, string localFolderPath, string localFileName = null, int bufferSize = 4096) {
			if (localFileName == null)
				localFileName = client.Url.Path.Split('/').Last();


			var folder = await EnsureFolderAsync(localFolderPath);
			var file = await folder.CreateFileAsync(localFileName, CreationCollisionOption.ReplaceExisting);

			// http://developer.greenbutton.com/downloading-large-files-with-the-net-httpclient
			var response = await client.HttpClient.GetAsync(client.Url, HttpCompletionOption.ResponseHeadersRead);

			// http://codereview.stackexchange.com/a/18679
			using (var httpStream = await response.Content.ReadAsStreamAsync())
			using (var fileStream = await file.OpenAsync(FileAccess.ReadAndWrite)) {
				await httpStream.CopyToAsync(fileStream, bufferSize);
			}

			return PortablePath.Combine(localFolderPath, localFileName);
		}

		public static Task<string> DownloadAsync(this string url, string localFolderPath, string localFileName = null, int bufferSize = 4096) {
			return new FlurlClient(url).DownloadAsync(localFolderPath, localFileName, bufferSize);
		}

		public static Task<string> DownloadAsync(this Url url, string localFolderPath, string localFileName = null, int bufferSize = 4096) {
			return new FlurlClient(url).DownloadAsync(localFolderPath, localFileName, bufferSize);
		}

		private static Task<IFolder> EnsureFolderAsync(string path) {
			return FileSystem.Current.LocalStorage.CreateFolderAsync(path, CreationCollisionOption.OpenIfExists);
		}
	}
}
