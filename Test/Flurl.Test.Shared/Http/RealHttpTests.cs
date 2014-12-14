using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	/// <summary>
	/// Most HTTP tests in this project are with Flurl in fake mode, these are some real ones, mostly using the handy site 
	/// http://httpbin.org. One important aspect these verify is that AutoDispose behavior is not preventing us from getting
	/// stuff out of the response (i.e. that we're not disposing too early).
	/// </summary>
	[TestFixture]
	public class RealHttpTests
	{
		[Test]
		public async Task can_download_file() {
			var path = await "http://www.google.com".DownloadFileAsync(@"c:\a\b", "google.txt");
			Assert.AreEqual(@"c:\a\b\google.txt", path);
			Assert.That(File.Exists(path));
			File.Delete(path);
			Directory.Delete(@"c:\a", true);
		}

		[Test]
		public async Task can_set_cookies() {
			var resp = await "http://httpbin.org/cookies".WithCookies(new { x = 1, y = 2 }).GetJsonAsync();
			// httpbin.org will return json representation of cookies that were set on the server.
			Assert.AreEqual("1", resp.cookies.x);
			Assert.AreEqual("2", resp.cookies.y);
		}

		[Test]
		public async Task can_post_and_receive_json() {
			var result = await "http://httpbin.org/post".PostJsonAsync(new { a = 1, b = 2 }).ReceiveJson();
			Assert.AreEqual(result.json.a, 1);
			Assert.AreEqual(result.json.b, 2);
		}

		[Test]
		public async Task can_get_stream() {
			using (var stream = await "http://www.google.com".GetStreamAsync()) 
			using (var ms = new MemoryStream()) {
				stream.CopyTo(ms);
				Assert.Greater(ms.Length, 0);
			}
		}
	}
}
