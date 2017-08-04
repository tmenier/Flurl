using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Testing;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	/// <summary>
	/// Most HTTP tests in this project are with Flurl in fake mode. These are some real ones, mostly using http://httpbin.org.
	/// </summary>
	[TestFixture, Parallelizable]
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
		public async Task can_persist_cookies() {
			using (var fc = new FlurlClient()) {
				var req = "http://httpbin.org/cookies".WithClient(fc).WithCookie("z", 999);
				// cookie should be set
				Assert.AreEqual("999", fc.Cookies["z"].Value);
				Assert.AreEqual("999", req.Cookies["z"].Value);

				await req.HeadAsync();
				// FlurlClient should be re-used, so cookie should stick
				Assert.AreEqual("999", fc.Cookies["z"].Value);
				Assert.AreEqual("999", req.Cookies["z"].Value);

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
						.PostMultipartAsync(content => {
							content
								.AddStringParts(new { a = 1, b = 2 })
								.AddString("DataField", "hello!")
								.AddFile("File1", path1)
								.AddFile("File2", stream, "foo.txt");

							// hack to deal with #179, remove when this is fixed: https://github.com/kennethreitz/httpbin/issues/340
							content.Headers.ContentLength = 735;
						})
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

		[Test]
		public async Task can_handle_error() {
			var handlerCalled = false;

			try {
				await "https://httpbin.org/status/500".WithSettings(c => {
					c.OnError = call => {
						call.ExceptionHandled = true;
						handlerCalled = true;
					};
				}).GetAsync();
				Assert.IsTrue(handlerCalled, "error handler shoule have been called.");
			}
			catch (FlurlHttpException) {
				Assert.Fail("exception should have been supressed.");
			}
		}

		[Test]
		public async Task can_comingle_real_and_fake_tests() {
			// do a fake call while a real call is running
			var realTask = "https://www.google.com/".GetStringAsync();
			using (var test = new HttpTest()) {
				test.RespondWith("fake!");
				var fake = await "https://www.google.com/".GetStringAsync();
				Assert.AreEqual("fake!", fake);
			}
			Assert.AreNotEqual("fake!", await realTask);
		}
	}
}