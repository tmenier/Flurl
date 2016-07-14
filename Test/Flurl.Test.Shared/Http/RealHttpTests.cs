using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	/// <summary>
	/// Most HTTP tests in this project are with Flurl in fake mode. These are some real ones, mostly using the handy site 
	/// http://httpbin.org. One important aspect these verify is that AutoDispose behavior is not preventing us from getting
	/// stuff out of the response (i.e. that we're not disposing too early).
	/// </summary>
	[TestFixture]
	public class RealHttpTests
	{
		[Test]
		public async Task can_download_file() {
			var folder = "c:\\flurl-test-" + Guid.NewGuid(); // random so parallel tests don't trip over each other
			var path = await "http://www.google.com".DownloadFileAsync(folder, "google.txt");
			Assert.AreEqual($@"{folder}\google.txt", path);
			Assert.That(File.Exists(path));
			File.Delete(path);
			Directory.Delete(folder, true);
		}

		[Test]
		public async Task can_set_request_cookies() {
			var resp = await "http://httpbin.org/cookies".WithCookies(new { x = 1, y = 2 }).GetJsonAsync();

			// httpbin returns json representation of cookies that were set on the server.
			Assert.AreEqual("1", resp.cookies.x);
			Assert.AreEqual("2", resp.cookies.y);
		}

		[Test]
		public async Task can_set_cookies_before_setting_url() {
			var fc = new FlurlClient().WithCookie("z", "999");
			var resp = await fc.WithUrl("http://httpbin.org/cookies").GetJsonAsync();
			Assert.AreEqual("999", resp.cookies.z);
		}

		[Test]
		public async Task can_get_response_cookies() {
			var fc = new FlurlClient().EnableCookies();
			await fc.WithUrl("https://httpbin.org/cookies/set?z=999").HeadAsync();
			Assert.AreEqual("999", fc.Cookies["z"].Value);
		}

		[Test]
		public async Task cant_persist_cookies_without_resuing_client() {
			var fc = "http://httpbin.org/cookies".WithCookie("z", 999);
			// cookie should be set
			Assert.AreEqual("999", fc.Cookies["z"].Value);

			await fc.HeadAsync();
			// FlurlClient was auto-disposed, so cookie should be gone
			CollectionAssert.IsEmpty(fc.Cookies);

			// httpbin returns json representation of cookies that were set on the server.
			var resp = await "http://httpbin.org/cookies".GetJsonAsync();
			Assert.IsFalse((resp.cookies as IDictionary<string, object>).ContainsKey("z"));
		}

		[Test]
		public async Task can_persist_cookies() {
			using (var fc = new FlurlClient()) {
				var fc2 = "http://httpbin.org/cookies".WithClient(fc).WithCookie("z", 999);
				// cookie should be set
				Assert.AreEqual("999", fc.Cookies["z"].Value);
				Assert.AreEqual("999", fc2.Cookies["z"].Value);

				await fc2.HeadAsync();
				// FlurlClient should be re-used, so cookie should stick
				Assert.AreEqual("999", fc.Cookies["z"].Value);
				Assert.AreEqual("999", fc2.Cookies["z"].Value);

				// httpbin returns json representation of cookies that were set on the server.
				var resp = await "http://httpbin.org/cookies".WithClient(fc).GetJsonAsync();
				Assert.AreEqual("999", resp.cookies.z);
			}
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

		[Test]
		public async Task can_get_string() {
			var s = await "http://www.google.com".GetStringAsync();
			Assert.Greater(s.Length, 0);
		}

		[Test]
		public async Task can_get_byte_array() {
			var bytes = await "http://www.google.com".GetBytesAsync();
			Assert.Greater(bytes.Length, 0);
		}

		[Test]
		public void fails_on_non_success_status() {
			Assert.ThrowsAsync<FlurlHttpException>(async () => await "http://httpbin.org/status/418".GetAsync());
		}

		[Test]
		public async Task can_allow_non_success_status() {
			await "http://httpbin.org/status/418".AllowHttpStatus("4xx").GetAsync();
		}

		[Test]
		public void can_cancel_request() {
			var ex = Assert.ThrowsAsync<FlurlHttpException>(async () =>
			{
				var cts = new CancellationTokenSource();
				var task = "http://www.google.com".GetStringAsync(cts.Token);
				cts.Cancel();
				await task;
			});

			Assert.IsNotNull(ex.InnerException as TaskCanceledException);
		}

		[Test]
		public async Task can_post_multipart() {
			var folder = "c:\\flurl-test-" + Guid.NewGuid(); // random so parallel tests don't trip over each other
			Directory.CreateDirectory(folder);

			var path1 = Path.Combine(folder, "upload1.txt");
			var path2 = Path.Combine(folder, "upload2.txt");

			File.WriteAllText(path1, "file contents 1");
			File.WriteAllText(path2, "file contents 2");

			try {
				using (var stream = File.OpenRead(path2)) {
					var resp = await "http://httpbin.org/post"
						.PostMultipartAsync(content => content
							.AddStringParts(new {a = 1, b = 2})
							.AddString("DataField", "hello!")
							.AddFile("File1", path1)
							.AddFile("File2", stream, "foo.txt"))
						//.ReceiveString();
						.ReceiveJson();
					Assert.AreEqual("1", resp.form.a);
					Assert.AreEqual("2", resp.form.b);
					Assert.AreEqual("hello!", resp.form.DataField);
					Assert.AreEqual("file contents 1", resp.files.File1);
					Assert.AreEqual("file contents 2", resp.files.File2);
				}
			}
			finally {
				Directory.Delete(folder, true);
			}
		}

		// https://github.com/tmenier/Flurl/pull/76
		// quotes around charset value is technically legal but there's a bug in .NET we want to avoid: https://github.com/dotnet/corefx/issues/5014
		[Test]
		public async Task supports_quoted_charset() {
			// Respond with header Content-Type: text/javascript; charset="UTF-8"
			var url = "https://httpbin.org/response-headers?Content-Type=text/javascript;%20charset=%22UTF-8%22";

			// confirm thart repsonse has quoted charset value
			var resp = await url.GetAsync();
			Assert.AreEqual("\"UTF-8\"", resp.Content.Headers.ContentType.CharSet);

			// GetStringAsync is where we need to work around the .NET bug
			var s = await url.GetStringAsync();
			// not throwing should be enough, but do a little more for good measure..
			s = s.Trim();
			StringAssert.StartsWith("{", s);
			StringAssert.EndsWith("}", s);
		}
	}
}