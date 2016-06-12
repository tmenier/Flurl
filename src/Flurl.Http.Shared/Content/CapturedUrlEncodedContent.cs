namespace Flurl.Http.Content
{
	/// <summary>
	/// Provides HTTP content based on an object serialized to URL-encoded name-value pairs.
	/// Useful in simulating an HTML form POST. Serialized content is captured to Content property
	/// so it can be read without affecting the read-once content stream.
	/// </summary>
	public class CapturedUrlEncodedContent : CapturedStringContent
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CapturedUrlEncodedContent"/> class.
		/// </summary>
		/// <param name="data">Content represented as a (typically anonymous) object, which will be parsed into name/value pairs.</param>
		public CapturedUrlEncodedContent(string data) : base(data, null, "application/x-www-form-urlencoded") { }
	}
}