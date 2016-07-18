using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Flurl.Http.Content
{
	/// <summary>
	/// Represents HTTP content based on a local file. Typically used with PostMultipartAsync for uploading files.
	/// </summary>
	public class FileContent : HttpContent
    {
		/// <summary>
		/// The local file path.
		/// </summary>
		public string Path { get; }

		private readonly int _bufferSize;

		/// <summary>
		/// Initializes a new instance of the <see cref="FileContent"/> class.
		/// </summary>
		/// <param name="path">The local file path.</param>
		/// <param name="bufferSize">The buffer size of the stream upload in bytes. Defaults to 4096.</param>
		public FileContent(string path, int bufferSize = 4096) {
			Path = path;
			_bufferSize = bufferSize;
		}

	    protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context) {
		    using (var source = await FileUtil.OpenReadAsync(Path, _bufferSize).ConfigureAwait(false)) {
			    await source.CopyToAsync(stream, _bufferSize).ConfigureAwait(false);
		    }
		}

	    protected override bool TryComputeLength(out long length) {
		    length = -1;
			return false;
	    }
    }
}
