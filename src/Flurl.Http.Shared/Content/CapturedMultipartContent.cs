using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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
		public IDictionary<string, string> Data { get; } = new Dictionary<string, string>();

		/// <summary>
		/// Represents all files added to the multipart content.
		/// </summary>
		public IDictionary<string, HttpFile> Files { get; } = new Dictionary<string, HttpFile>();

		private readonly List<Stream> _openStreams = new List<Stream>();

		/// <summary>
		/// Initializes a new instance of the <see cref="CapturedMultipartContent"/> class.
		/// </summary>
		/// <param name="data">Content represented as a (typically anonymous) object, which will be parsed into name/value pairs.
		/// Use properties of type HttpFile to represent file uploads.</param>
		public CapturedMultipartContent(object data) {
			foreach (var kv in data.ToKeyValuePairs()) {
				if (kv.Value == null)
					continue;

				if (kv.Value is HttpFile)
					Files.Add(kv.Key, (HttpFile)kv.Value);
				else
					AddTextField(kv.Key, kv.Value);
			}
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing) {
			if (disposing) {
				foreach (var stream in _openStreams)
					stream.Dispose();
				_openStreams.Clear();
			}
			base.Dispose(disposing);
		}

		/// <summary>
		/// Fluently add a file to the multipart content.
		/// </summary>
		/// <returns>this CapturedMultipartContent instance.</returns>
		public CapturedMultipartContent AddFile(string fieldName, string filePath, string contentType = null) {
			Files.Add(fieldName, new HttpFile(filePath, contentType));
			return this;
		}

		/// <summary>
		/// Fluently add a simple text-based value to the multipart content.
		/// </summary>
		/// <returns>this CapturedMultipartContent instance.</returns>
		public CapturedMultipartContent AddTextField(string fieldName, object value) {
			var s = value?.ToInvariantString();
			Data.Add(fieldName, s);
			return this;
		}

		/// <summary>
		/// Serializes to stream asynchronous.
		/// </summary>
		/// <param name="stream">The stream.</param>
		/// <param name="context">The context.</param>
		protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context) {
			foreach (var kv in Data) {
				Add(new StringContent(kv.Value), kv.Key);
			}

			foreach (var kv in Files) {
				var file = kv.Value;
				var fs = file.Stream;
				if (fs == null) {
					fs = await FileUtil.OpenReadAsync(file.Path, 4096).ConfigureAwait(false);
					_openStreams.Add(fs);
				}
				var content = new StreamContent(fs);
				if (file.ContentTye != null)
					content.Headers.ContentType.MediaType = file.ContentTye;
				Add(content, kv.Key, file.Name);
			}
			await base.SerializeToStreamAsync(stream, context).ConfigureAwait(false);
		}

		// https://blogs.msdn.microsoft.com/henrikn/2012/02/16/push-and-pull-streams-using-httpclient/
		/// <summary>
		/// Tries the length of the compute.
		/// </summary>
		/// <param name="length">The length.</param>
		protected override bool TryComputeLength(out long length) {
			length = -1;
			return false;
		}
	}
}