using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Flurl.Http
{
	internal static class FileUtil
	{
#if PORTABLE
		internal static string GetFileName(string path) {
			return path?.Split(PCLStorage.PortablePath.DirectorySeparatorChar).Last();
		}

		internal static async Task<Stream> OpenStreamAsync(string path) {
			var file = await PCLStorage.FileSystem.Current.GetFileFromPathAsync(path).ConfigureAwait(false);
			return await file.OpenAsync(PCLStorage.FileAccess.Read).ConfigureAwait(false);
		}
#else
		internal static string GetFileName(string path) {
			return Path.GetFileName(path);
		}

		internal static Task<Stream> OpenStreamAsync(string path) {
			return Task.FromResult<Stream>(File.OpenRead(path));
		}
#endif
	}
}