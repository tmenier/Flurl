using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Flurl.Http.Content
{
	/// <summary>
	/// Provides HTTP content based on a string, with the string itself captured to a property
	/// so it can be read without affecting the read-once content stream.
	/// </summary>
	public class CapturedStringContent : StringContent
	{
		/// <summary>
		/// The content body captured as a string. Can be read multiple times (unlike the content stream).
		/// </summary>
		public string Content { get; }

		/// <summary>
		/// Initializes a new instance of <see cref="CapturedStringContent"/> with a Content-Type header of text/plain; charset=UTF-8
		/// </summary>
		/// <param name="content">The content.</param>
		public CapturedStringContent(string content) : base(content) {
			Content = content;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="CapturedStringContent"/> class.
		/// </summary>
		/// <param name="content">The content.</param>
		/// <param name="contentType">Value of the Content-Type header. To exclude the header, set to null explicitly.</param>
		public CapturedStringContent(string content, string contentType) : base(content) {
			Content = content;
			Headers.Remove("Content-Type");
			if (contentType != null)
				Headers.TryAddWithoutValidation("Content-Type", contentType);
		}
	}
}