using System.Text;

namespace Flurl.Http.Content
{
	/// <summary>
	/// Provides HTTP content based on a serialized JSON object, with the JSON string captured to a property
	/// so it can be read without affecting the read-once content stream.
	/// </summary>
	public class CapturedJsonContent : CapturedStringContent
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CapturedJsonContent"/> class.
		/// </summary>
		/// <param name="json">The json.</param>
		public CapturedJsonContent(string json) : base(json, Encoding.UTF8, "application/json") { }
	}
}