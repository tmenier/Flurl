using System.IO;
#if NETSTANDARD1_1
using System.Linq;
#endif
using System.Threading.Tasks;

namespace Flurl.Http
{
	internal static class FileUtil
	{
#if NETSTANDARD1_1
		internal static string GetFileName(string path) {
			return path?.Split(PCLStorage.PortablePath.DirectorySeparatorChar).Last();
		}

		internal static string CombinePath(params string[] paths) {
			return PCLStorage.PortablePath.Combine(paths);
		}

		internal static async Task<Stream> OpenReadAsync(string path, int bufferSize) {
			var file = await PCLStorage.FileSystem.Current.GetFileFromPathAsync(path).ConfigureAwait(false);
			return await file.OpenAsync(PCLStorage.FileAccess.Read).ConfigureAwait(false);
		}

		internal static async Task<Stream> OpenWriteAsync(string folderPath, string fileName, int bufferSize) {
			var folder = await PCLStorage.FileSystem.Current.LocalStorage.CreateFolderAsync(folderPath, PCLStorage.CreationCollisionOption.OpenIfExists).ConfigureAwait(false);
			var file = await folder.CreateFileAsync(fileName, PCLStorage.CreationCollisionOption.ReplaceExisting).ConfigureAwait(false);
			return await file.OpenAsync(PCLStorage.FileAccess.ReadAndWrite).ConfigureAwait(false);
		}
#else
		internal static string GetFileName(string path) {
			return Path.GetFileName(path);
		}

		internal static string CombinePath(params string[] paths) {
			return Path.Combine(paths);
		}

        internal static Task<Stream> OpenReadAsync(string path, int bufferSize) {
			return Task.FromResult<Stream>(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize, useAsync: true));
		}

		internal static Task<Stream> OpenWriteAsync(string folderPath, string fileName, int bufferSize) {
			Directory.CreateDirectory(folderPath); // checks existence
			var filePath = Path.Combine(folderPath, fileName);
			return Task.FromResult<Stream>(new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, useAsync: true));
		}
#endif
    }
}