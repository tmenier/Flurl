using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Flurl.Util;

namespace Flurl.Http.Content
{
	/// <summary>
	/// Provides HTTP content for a multipart/form-data request.
	/// Useful in simulating an HTML form POST that includes one or more file uploads. Provided content
	/// is captured to Data and Files properties so they be read without affecting the read-once content stream.
	/// </summary>
	public class CapturedMultipartContent : MultipartFormDataContent
	{
		/// <summary>
		/// Represents all text-based values added to the multipart content.
		/// </summary>
		public IDictionary<string, string> Data { get; }

		/// <summary>
		/// Represents all files added to the multipart content.
		/// </summary>
		public IDictionary<string, HttpFile> Files { get; }

		private readonly List<Stream> _streams = new List<Stream>();

		/// <summary>
		/// Initializes a new instance of the <see cref="CapturedMultipartContent"/> class.
		/// </summary>
		/// <param name="data">Content represented as a (typically anonymous) object, which will be parsed into name/value pairs.
		/// Use properties of type HttpFile to represent file uploads.</param>
		public CapturedMultipartContent(object data) {
			foreach (var kv in data.ToKeyValuePairs()) {
				if (kv.Value == null)
					continue;

				var file = kv.Value as HttpFile;
				if (file != null)
					AddFile(kv.Key, file);
				else
					AddTextField(kv.Key, kv.Value);
			}
		}

		protected override void Dispose(bool disposing) {
			if (disposing) {
				foreach (var stream in _streams)
					stream.Dispose();
				_streams.Clear();
			}
			base.Dispose(disposing);
		}

		/// <summary>
		/// Fluently add a file to the multipart content.
		/// </summary>
		/// <returns>this CapturedMultipartContent instance.</returns>
		public CapturedMultipartContent AddFile(string fieldName, string filePath, string contentType = null) {
			AddFile(fieldName, new HttpFile(filePath, contentType));
			return this;
		}

		private void AddFile(string fieldName, HttpFile file) {
			var stream = File.OpenRead(file.Path);
			_streams.Add(stream);
			var content = new StreamContent(stream);
			if (file.ContentTye != null)
				content.Headers.ContentType.MediaType = file.ContentTye;
			Add(content, fieldName);
			Files.Add(fieldName, file);
		}

		/// <summary>
		/// Fluently add a simple text-based value to the multipart content.
		/// </summary>
		/// <returns>this CapturedMultipartContent instance.</returns>
		public CapturedMultipartContent AddTextField(string fieldName, object value) {
			var s = value?.ToInvariantString();
			Add(new StringContent(s), fieldName);
			Data.Add(fieldName, s);
			return this;
		}
	}

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