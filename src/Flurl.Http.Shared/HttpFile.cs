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
			Path = path;
			ContentTye = contentType;
		}

		/// <summary>
		/// The local file path.
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// The content-type header associated with the file.
		/// </summary>
		public string ContentTye { get; set; }
	}
}
