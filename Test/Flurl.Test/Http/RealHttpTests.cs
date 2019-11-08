using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Configuration;
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
		[TestCase("gzip", "gzipped")]
		[TestCase("deflate", "deflated"), Ignore("#474")]
		public async Task decompresses_automatically(string encoding, string jsonKey) {
			var result = await "https://httpbin.org"
				.AppendPathSegment(encoding)
				.WithHeader("Accept-encoding", encoding)
				.GetJsonAsync<Dictionary<string, object>>();

			Assert.AreEqual(true, result[jsonKey]);
		}

		[TestCase("https://httpbin.org/image/jpeg", null, "my-image.jpg", "my-image.jpg")]
		// should use last path segment url-decoded (foo?bar:ding), then replace illegal path characters with _
		[TestCase("https://httpbin.org/anything/foo%3Fbar%3Ading", null, null, "foo_bar_ding")]
		// should use filename from content-disposition excluding any leading/trailing quotes
		[TestCase("https://httpbin.org/response-headers", "attachment; filename=\"myfile.txt\"", null, "myfile.txt")]
		// should prefer filename* over filename, per https://tools.ietf.org/html/rfc6266#section-4.3
		[TestCase("https://httpbin.org/response-headers", "attachment; filename=filename.txt; filename*=utf-8''filenamestar.txt", null, "filenamestar.txt")]
		// has Content-Disposition header but no filename in it, should use last part of URL
		[TestCase("https://httpbin.org/response-headers", "attachment", null, "response-headers")]
		public async Task can_download_file(string url, string contentDisposition, string suppliedFilename, string expectedFilename) {
			var folder = Path.Combine(Path.GetTempPath(), $"flurl-test-{Guid.NewGuid()}"); // random so parallel tests don't trip over each other

			try {
				var path = await url.SetQueryParam("Content-Disposition", contentDisposition).DownloadFileAsync(folder, suppliedFilename);
				var expected = Path.Combine(folder, expectedFilename);
				Assert.AreEqual(expected, path);
				Assert.That(File.Exists(expected));
			}
			finally {
				Directory.Delete(folder, true);
			}
		}

		[Test]
		public async Task can_set_request_cookies() {
			var cli = new FlurlClient();
			var resp = await cli.Request("https://httpbin.org/cookies").WithCookies(new { x = 1, y = 2 }).GetJsonAsync();

			// httpbin returns json representation of cookies that were set on the server.
			Assert.AreEqual("1", resp.cookies.x);
			Assert.AreEqual("2", resp.cookies.y);
		}

		[Test]
		public async Task can_set_cookies_before_setting_url() {
			var cli = new FlurlClient().WithCookie("z", "999");
			var resp = await cli.Request("https://httpbin.org/cookies").GetJsonAsync();
			Assert.AreEqual("999", resp.cookies.z);
		}

		[Test]
		public async Task can_get_response_cookies() {
			var cli = new FlurlClient().EnableCookies();
			await cli.Request("https://httpbin.org/cookies/set?z=999").HeadAsync();
			Assert.AreEqual("999", cli.Cookies["z"].Value);
		}

		[Test]
		public async Task can_persist_cookies() {
			var cli = new FlurlClient("https://httpbin.org/cookies");
			var req = cli.Request().WithCookie("z", 999);
			// cookie should be set
			Assert.AreEqual("999", cli.Cookies["z"].Value);
			Assert.AreEqual("999", req.Cookies["z"].Value);

			await req.HeadAsync();
			// FlurlClient should be re-used, so cookie should stick
			Assert.AreEqual("999", cli.Cookies["z"].Value);
			Assert.AreEqual("999", req.Cookies["z"].Value);

			// httpbin returns json representation of cookies that were set on the server.
			var resp = await cli.Request().GetJsonAsync();
			Assert.AreEqual("999", resp.cookies.z);
		}

		[Test]
		public async Task can_post_and_receive_json() {
			var result = await "https://httpbin.org/post".PostJsonAsync(new { a = 1, b = 2 }).ReceiveJson();
			Assert.AreEqual(result.json.a, 1);
			Assert.AreEqual(result.json.b, 2);
		}

		[Test]
		public async Task can_get_stream() {
			using (var stream = await "https://www.google.com".GetStreamAsync())
			using (var ms = new MemoryStream()) {
				stream.CopyTo(ms);
				Assert.Greater(ms.Length, 0);
			}
		}

		[Test]
		public async Task can_get_string() {
			var s = await "https://www.google.com".GetStringAsync();
			Assert.Greater(s.Length, 0);
		}

		[Test]
		public async Task can_get_byte_array() {
			var bytes = await "https://www.google.com".GetBytesAsync();
			Assert.Greater(bytes.Length, 0);
		}

		[Test]
		public void fails_on_non_success_status() {
			Assert.ThrowsAsync<FlurlHttpException>(async () => await "https://httpbin.org/status/418".GetAsync());
		}

		[Test]
		public async Task can_allow_non_success_status() {
			await "https://httpbin.org/status/418".AllowHttpStatus("4xx").GetAsync();
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
					var resp = await "https://httpbin.org/post"
						.PostMultipartAsync(content => {
							content
								.AddStringParts(new { a = 1, b = 2 })
								.AddString("DataField", "hello!")
								.AddFile("File1", path1)
								.AddFile("File2", stream, "foo.txt");

							// hack to deal with #179. appears to be fixed on httpbin now.
							// content.Headers.ContentLength = 735;
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
		public async Task can_handle_http_error() {
			var handlerCalled = false;

			try {
				await "https://httpbin.org/status/500".ConfigureRequest(c => {
					c.OnError = call => {
						call.ExceptionHandled = true;
						handlerCalled = true;
					};
				}).GetJsonAsync();
				Assert.IsTrue(handlerCalled, "error handler should have been called.");
			}
			catch (FlurlHttpException) {
				Assert.Fail("exception should have been supressed.");
			}
		}

		[Test]
		public async Task can_handle_parsing_error() {
			Exception ex = null;

			try {
				await "http://httpbin.org/image/jpeg".ConfigureRequest(c => {
					c.OnError = call => {
						ex = call.Exception;
						call.ExceptionHandled = true;
					};
				}).GetJsonAsync();
				Assert.IsNotNull(ex, "error handler should have been called.");
				Assert.IsInstanceOf<FlurlParsingException>(ex);
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

		[Test]
		public void can_set_timeout() {
			var ex = Assert.ThrowsAsync<FlurlHttpTimeoutException>(async () => {
				await "https://httpbin.org/delay/5"
					.WithTimeout(TimeSpan.FromMilliseconds(50))
					.HeadAsync();
			});
			Assert.That(ex.InnerException is TaskCanceledException);
		}

		[Test]
		public void can_cancel_request() {
			var cts = new CancellationTokenSource();
			var ex = Assert.ThrowsAsync<FlurlHttpException>(async () => {
				var task = "https://httpbin.org/delay/5".GetAsync(cts.Token);
				cts.Cancel();
				await task;
			});
			Assert.That(ex.InnerException is TaskCanceledException);
		}

		[Test] // make sure the 2 tokens in play are playing nicely together
		public void can_set_timeout_and_cancellation_token() {
			// cancellation with timeout value set
			var cts = new CancellationTokenSource();
			var ex = Assert.ThrowsAsync<FlurlHttpException>(async () => {
				var task = "https://httpbin.org/delay/5"
					.WithTimeout(TimeSpan.FromMilliseconds(50))
					.GetAsync(cts.Token);
				cts.Cancel();
				await task;
			});
			Assert.That(ex.InnerException is TaskCanceledException);
			Assert.IsTrue(cts.Token.IsCancellationRequested);

			// timeout with cancellation token set
			cts = new CancellationTokenSource();
			ex = Assert.ThrowsAsync<FlurlHttpTimeoutException>(async () => {
				await "https://httpbin.org/delay/5"
					.WithTimeout(TimeSpan.FromMilliseconds(50))
					.GetAsync(cts.Token);
			});
			Assert.That(ex.InnerException is TaskCanceledException);
			Assert.IsFalse(cts.Token.IsCancellationRequested);
		}

		[Test]
		public async Task can_set_request_cookies_with_a_delegating_handler() {
			var resp = await new FlurlClient("http://httpbin.org")
				.Configure(settings => settings.HttpClientFactory = new DelegatingHandlerHttpClientFactory())
				.Request("cookies")
				.WithCookies(new { x = 1, y = 2 })
				.GetJsonAsync();

			// httpbin returns json representation of cookies that were set on the server.
			Assert.AreEqual("1", resp.cookies.x);
			Assert.AreEqual("2", resp.cookies.y);
		}

		[Test]
		public async Task can_get_response_cookies_with_a_delegating_handler() {
			var cli = new FlurlClient("https://httpbin.org")
				.Configure(settings => settings.HttpClientFactory = new DelegatingHandlerHttpClientFactory())
				.EnableCookies();

			await cli.Request("cookies/set?z=999").HeadAsync();
			Assert.AreEqual("999", cli.Cookies["z"].Value);
		}

		[Test]
		public async Task connection_lease_timeout_doesnt_disrupt_calls() {
			// testing this quickly is tricky. HttpClient will be replaced by a new instance after 1 timeout and disposed
			// after another, so the timeout period (typically minutes in real-world scenarios) needs to be long enough
			// that we don't dispose before the response from google is received. 1 second seems to work.
			var cli = new FlurlClient("http://www.google.com");
			cli.Settings.ConnectionLeaseTimeout = TimeSpan.FromMilliseconds(1000);

			// ping google for about 2.5 seconds
			var tasks = new List<Task>();
			for (var i = 0; i < 100; i++) {
				tasks.Add(cli.Request().HeadAsync());
				await Task.Delay(25);
			}
			await Task.WhenAll(tasks); // failed HTTP status, etc, would throw here and fail the test.
		}

		[Test]
		public async Task test_settings_override_client_settings() {
			var cli1 = new FlurlClient();
			cli1.Settings.HttpClientFactory = new DefaultHttpClientFactory();
			var h = cli1.HttpClient; // force (lazy) instantiation

			using (var test = new HttpTest()) {
				test.Settings.CookiesEnabled = false;

				test.RespondWith("foo!");
				var s = await cli1.Request("http://www.google.com")
					.EnableCookies() // test says cookies are off, and test should always win
					.GetStringAsync();
				Assert.AreEqual("foo!", s);
				Assert.IsFalse(cli1.Settings.CookiesEnabled);

				var cli2 = new FlurlClient();
				cli2.Settings.HttpClientFactory = new DefaultHttpClientFactory();
				h = cli2.HttpClient;

				test.RespondWith("foo 2!");
				s = await cli2.Request("http://www.google.com")
					.EnableCookies() // test says cookies are off, and test should always win
					.GetStringAsync();
				Assert.AreEqual("foo 2!", s);
				Assert.IsFalse(cli2.Settings.CookiesEnabled);
			}
		}

		public class DelegatingHandlerHttpClientFactory : DefaultHttpClientFactory
		{
			public override HttpMessageHandler CreateMessageHandler() {
				var handler = base.CreateMessageHandler();

				return new PassThroughDelegatingHandler(new PassThroughDelegatingHandler(handler));
			}

			public class PassThroughDelegatingHandler : DelegatingHandler
			{
				public PassThroughDelegatingHandler(HttpMessageHandler innerHandler) : base(innerHandler) { }
			}
		}
	}
}