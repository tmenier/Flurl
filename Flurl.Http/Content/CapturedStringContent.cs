using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Flurl.Http.Content
{
	/// <summary>
	/// Provides HTTP content based on a string, with the string itself captured to a property
	/// so it can be read without affecting the read-once content stream.
	/// </summary>
	public class CapturedStringContent : StringContent
	{
		private readonly string _content;

		/// <summary>
		/// The content body captured as a string. Can be read multiple times (unlike the content stream).
		/// </summary>
		public string Content {
			get { return _content; }
		}

		public CapturedStringContent(string content) : base(content) {
			_content = content;
		}

		public CapturedStringContent(string content, Encoding encoding) : base(content, encoding) {
			_content = content;
		}

		public CapturedStringContent(string content, Encoding encoding, string mediaType) : base(content, encoding, mediaType) {
			_content = content;
		}
	}
}
