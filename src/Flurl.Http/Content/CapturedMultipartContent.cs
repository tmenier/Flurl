using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Flurl.Http.Configuration;
using Flurl.Util;

namespace Flurl.Http.Content
{
	/// <summary>
	/// Provides HTTP content for a multipart/form-data request.
	/// </summary>
	public class CapturedMultipartContent : MultipartContent
	{
		private readonly FlurlHttpSettings _settings;

		/// <summary>
		/// Gets an array of HttpContent objects that make up the parts of the multipart request.
		/// </summary>
		public HttpContent[] Parts => this.ToArray();

		/// <summary>
		/// Initializes a new instance of the <see cref="CapturedMultipartContent"/> class.
		/// </summary>
		/// <param name="settings">The FlurlHttpSettings used to serialize each content part. (Defaults to FlurlHttp.GlobalSettings.)</param>
		public CapturedMultipartContent(FlurlHttpSettings settings = null) : base("form-data") {
			_settings = settings ?? FlurlHttp.GlobalSettings;
		}

		/// <summary>
		/// Add a content part to the multipart request.
		/// </summary>
		/// <param name="name">The control name of the part.</param>
		/// <param name="content">The HttpContent of the part.</param>
		/// <returns>This CapturedMultipartContent instance (supports method chaining).</returns>
		public CapturedMultipartContent Add(string name, HttpContent content) {
			return AddInternal(name, content, null);
		}

		/// <summary>
		/// Add a simple string part to the multipart request.
		/// </summary>
		/// <param name="name">The control name of the part.</param>
		/// <param name="content">The string content of the part.</param>
		/// <param name="encoding">The encoding of the part.</param>
		/// <param name="mediaType">The media type of the part.</param>
		/// <returns>This CapturedMultipartContent instance (supports method chaining).</returns>
		public CapturedMultipartContent AddString(string name, string content, Encoding encoding = null, string mediaType = null) {
			return Add(name, new CapturedStringContent(content, encoding, mediaType));
		}

		/// <summary>
		/// Add multiple string parts to the multipart request by parsing an object's properties into control name/content pairs.
		/// </summary>
		/// <param name="data">The object (typically anonymous) whose properties are parsed into control name/content pairs.</param>
		/// <param name="encoding">The encoding of the parts.</param>
		/// <param name="mediaType">The media type of the parts.</param>
		/// <returns>This CapturedMultipartContent instance (supports method chaining).</returns>
		public CapturedMultipartContent AddStringParts(object data, Encoding encoding = null, string mediaType = null) {
			foreach (var kv in data.ToKeyValuePairs()) {
				if (kv.Value == null)
					continue;
				AddString(kv.Key, kv.Value.ToInvariantString(), encoding, mediaType);
			}
			return this;
		}

		/// <summary>
		/// Add a JSON-serialized part to the multipart request.
		/// </summary>
		/// <param name="name">The control name of the part.</param>
		/// <param name="data">The content of the part, which will be serialized to JSON.</param>
		/// <returns>This CapturedMultipartContent instance (supports method chaining).</returns>
		public CapturedMultipartContent AddJson(string name, object data) {
			return Add(name, new CapturedJsonContent(_settings.JsonSerializer.Serialize(data)));
		}

		/// <summary>
		/// Add a URL-encoded part to the multipart request.
		/// </summary>
		/// <param name="name">The control name of the part.</param>
		/// <param name="data">The content of the part, whose properties will be parsed and serialized to URL-encoded format.</param>
		/// <returns>This CapturedMultipartContent instance (supports method chaining).</returns>
		public CapturedMultipartContent AddUrlEncoded(string name, object data) {
			return Add(name, new CapturedUrlEncodedContent(_settings.UrlEncodedSerializer.Serialize(data)));
		}

		/// <summary>
		/// Adds a file to the multipart request from a stream.
		/// </summary>
		/// <param name="name">The control name of the part.</param>
		/// <param name="stream">The file stream to send.</param>
		/// <param name="fileName">The filename, added to the Content-Disposition header of the part.</param>
		/// <param name="mediaType">The media type of the file.</param>
		/// <param name="bufferSize">The buffer size of the stream upload in bytes. Defaults to 4096.</param>
		/// <returns>This CapturedMultipartContent instance (supports method chaining).</returns>
		public CapturedMultipartContent AddFile(string name, Stream stream, string fileName, string mediaType = null, int bufferSize = 4096) {
			var content = new StreamContent(stream, bufferSize);
			if (mediaType != null)
				content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
			return AddInternal(name, content, fileName);
		}

		/// <summary>
		/// Adds a file to the multipart request from a local path.
		/// </summary>
		/// <param name="name">The control name of the part.</param>
		/// <param name="path">The local path to the file.</param>
		/// <param name="mediaType">The media type of the file.</param>
		/// <param name="bufferSize">The buffer size of the stream upload in bytes. Defaults to 4096.</param>
		/// <returns>This CapturedMultipartContent instance (supports method chaining).</returns>
		public CapturedMultipartContent AddFile(string name, string path, string mediaType = null, int bufferSize = 4096) {
			var fileName = FileUtil.GetFileName(path);
			var content = new FileContent(path, bufferSize);
			if (mediaType != null)
				content.Headers.ContentType = new MediaTypeHeaderValue(mediaType);
			return AddInternal(name, content, fileName);
		}

		private CapturedMultipartContent AddInternal(string name, HttpContent content, string fileName) {
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentException("name must not be empty", nameof(name));

			content.Headers.ContentDisposition = new ContentDispositionHeaderValue("form-data") {
				Name = name,
				FileName = fileName,
				FileNameStar = fileName
			};
			base.Add(content);
			return this;
		}
	}
}