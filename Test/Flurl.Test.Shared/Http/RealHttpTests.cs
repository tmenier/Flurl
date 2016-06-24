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
			var folder = "c:\\" + Guid.NewGuid(); // random so parallel tests don't trip over each other
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
		public async Task can_cancel_request() {
			try {
				var cts = new CancellationTokenSource();
				var task = "http://www.google.com".GetStringAsync(cts.Token);
				cts.Cancel();
				await task;
				Assert.Fail("Should have thrown exception on cancelation");
			}
			catch (FlurlHttpException ex) {
				Assert.IsNotNull(ex.InnerException as TaskCanceledException);
			}
		}

		[Test]
		public async Task can_post_multipart() {
			var path1 = @"c:\flurl-multipart-test1.txt";
			File.WriteAllText(path1, "file contents 1");
			var path2 = @"c:\flurl-multipart-test2.txt";
			File.WriteAllText(path2, "file contents 2");
			try {
				var resp = await new FlurlClient("http://httpbin.org/post")
					.PostMultipartAsync(new {
						DataField = "hello!",
						File1 = new HttpFile(path1),
						File2 = new HttpFile(path2)
					})//.ReceiveString();
					.ReceiveJson();

				Assert.AreEqual("hello!", resp.form.DataField);
				Assert.AreEqual("file contents 1", resp.files.File1);
				Assert.AreEqual("file contents 2", resp.files.File2);
			}
			finally {
				File.Delete(path1);
				File.Delete(path2);
			}
		}
	}
}