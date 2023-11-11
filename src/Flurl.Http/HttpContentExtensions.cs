using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Flurl.Http
{
	/// <summary>
	/// Extension methods of HttpContent
	/// </summary>
	public static class HttpContentExtensions
	{
		/// <summary>Get a copy of the request content.</summary>
		/// <param name="content">The content to copy.</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <remarks>Note that cloning content isn't possible after it's dispatched, because the stream is automatically disposed after the request.</remarks>
		internal static async Task<HttpContent> CloneAsync(this HttpContent content, CancellationToken cancellationToken = default) {
			if (content == null)
				return null;

			Stream stream = new MemoryStream();
			await content
				.CopyToAsync(stream
#if NET5_0_OR_GREATER
					, cancellationToken
#endif
				)
				.ConfigureAwait(false);
			stream.Position = 0;

			StreamContent clone = new StreamContent(stream);
			foreach (var header in content.Headers)
				clone.Headers.Add(header.Key, header.Value);

			return clone;
		}
	}
}
