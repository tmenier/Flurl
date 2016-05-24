using System.Net.Http.Headers;

namespace Flurl.Http.Content
{
	/// <summary>
	/// Provides HTTP content based on object serialized to URL-encoded name-value, with the with the captured to a property
	/// so it can be read without affecting the read-once content stream.
	/// </summary>
	public class CapturedUrlEncodedContent : CapturedStringContent
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CapturedUrlEncodedContent"/> class.
		/// </summary>
		/// <param name="data">The data.</param>
		public CapturedUrlEncodedContent(string data) : base(data) {
			Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
		}
	}
}