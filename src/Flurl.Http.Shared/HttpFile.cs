using System;
using System.IO;

namespace Flurl.Http
{
	/// <summary>
	/// Represents a file to be uploaded via multipart POST.
	/// </summary>
	public class HttpFile
	{
		/// <param name="path">The local file path.</param>
		/// <param name="contentType">The content-type header associated with the file (optional).</param>
		public HttpFile(string path, string contentType = null) {
			if (path == null) throw new ArgumentNullException(nameof(path));
			Path = path;
			Name = FileUtil.GetFileName(path);
			ContentTye = contentType;
		}

		/// <param name="stream">The file stream.</param>
		/// <param name="filename">The filename.</param>
		/// <param name="contentType">The content-type header associated with the file (optional).</param>
		public HttpFile(Stream stream, string filename, string contentType = null) {
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			if (filename == null) throw new ArgumentNullException(nameof(filename));
			Stream = stream;
			Name = filename;
			ContentTye = contentType;
		}

		/// <summary>
		/// The file name.
		/// </summary>
		public string Name { get; private set; }

		/// <summary>
		/// The local file path.
		/// </summary>
		public string Path { get; private set; }

		/// <summary>
		/// The file stream.
		/// </summary>
		public Stream Stream { get; private set; }

		/// <summary>
		/// The content-type header associated with the file.
		/// </summary>
		public string ContentTye { get; set; }
	}
}
