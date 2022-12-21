using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
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
		public class HttpBinResponse
		{
			public Dictionary<string, JsonElement> json { get; set; }
			public Dictionary<string, string> args { get; set; }
			public Dictionary<string, string> form { get; set; }
			public Dictionary<string, string> cookies { get; set; }
			public Dictionary<string, string> files { get; set; }
		}

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
		// should use last path segment url-decoded (foo/bar), then replace illegal filename characters with _ ('/' and '\0' are only illegal chars in *nix)
		[TestCase("https://httpbin.org/anything/foo%2Fbar", null, null, "foo_bar")]
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
		public async Task can_post_and_receive_json() {
			var result = await "https://httpbin.org/post".PostJsonAsync(new { a = 1, b = 2 }).ReceiveJson<HttpBinResponse>();
			Assert.AreEqual(1, result.json["a"].GetInt32());
			Assert.AreEqual(2, result.json["b"].GetInt32());
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
			var resp = await "https://httpbin.org/status/418".AllowHttpStatus("4xx").GetAsync();
			Assert.AreEqual(418, resp.StatusCode);
		}

		[Test]
		public async Task can_post_multipart() {
			var folder = "c:\\flurl-test-" + Guid.NewGuid(); // random so parallel tests don't trip over each other
			var path1 = Path.Combine(folder, "upload1.txt");
			var path2 = Path.Combine(folder, "upload2.txt");

			Directory.CreateDirectory(folder);
			try {
				File.WriteAllText(path1, "file contents 1");
				File.WriteAllText(path2, "file contents 2");

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
						.ReceiveJson<HttpBinResponse>();
					Assert.AreEqual("1", resp.form["a"]);
					Assert.AreEqual("2", resp.form["b"]);
					Assert.AreEqual("hello!", resp.form["DataField"]);
					Assert.AreEqual("file contents 1", resp.files["File1"]);
					Assert.AreEqual("file contents 2", resp.files["File2"]);
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
				}).GetAsync();
				Assert.IsTrue(handlerCalled, "error handler should have been called.");
			}
			catch (FlurlHttpException) {
				Assert.Fail("exception should have been suppressed.");
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
				}).GetJsonAsync<object>();
				Assert.IsNotNull(ex, "error handler should have been called.");
				Assert.IsInstanceOf<FlurlParsingException>(ex);
			}
			catch (FlurlHttpException) {
				Assert.Fail("exception should have been suppressed.");
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
				var task = "https://httpbin.org/delay/5".GetAsync(cancellationToken: cts.Token);
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
					.GetAsync(cancellationToken: cts.Token);
				cts.Cancel();
				await task;
			});
			Assert.That(ex.InnerException is OperationCanceledException);
			Assert.IsTrue(cts.Token.IsCancellationRequested);

			// timeout with cancellation token set
			cts = new CancellationTokenSource();
			ex = Assert.ThrowsAsync<FlurlHttpTimeoutException>(async () => {
				await "https://httpbin.org/delay/5"
					.WithTimeout(TimeSpan.FromMilliseconds(50))
					.GetAsync(cancellationToken: cts.Token);
			});
			Assert.That(ex.InnerException is OperationCanceledException);
			Assert.IsFalse(cts.Token.IsCancellationRequested);
		}

		[Test]
		public async Task test_settings_override_client_settings() {
			var cli1 = new FlurlClient();
			cli1.Settings.HttpClientFactory = new DefaultHttpClientFactory();
			var h = cli1.HttpClient; // force (lazy) instantiation

			using (var test = new HttpTest()) {
				test.Settings.Redirects.Enabled = false;

				test.RespondWith("foo!");
				var s = await cli1.Request("http://www.google.com")
					.WithAutoRedirect(true) // test says redirects are off, and test should always win
					.GetStringAsync();
				Assert.AreEqual("foo!", s);
				Assert.IsFalse(cli1.Settings.Redirects.Enabled);

				var cli2 = new FlurlClient();
				cli2.Settings.HttpClientFactory = new DefaultHttpClientFactory();
				h = cli2.HttpClient;

				test.RespondWith("foo 2!");
				s = await cli2.Request("http://www.google.com")
					.WithAutoRedirect(true) // test says redirects are off, and test should always win
					.GetStringAsync();
				Assert.AreEqual("foo 2!", s);
				Assert.IsFalse(cli2.Settings.Redirects.Enabled);
			}
		}

		[Test]
		public async Task can_allow_real_http_in_test() {
			using var test = new HttpTest();
			test.RespondWith("foo");
			test.ForCallsTo("*httpbin*").AllowRealHttp();

			Assert.AreEqual("foo", await "https://www.google.com".GetStringAsync());
			Assert.AreNotEqual("foo", await "https://httpbin.org/get".GetStringAsync());
			Assert.AreEqual("bar", (await "https://httpbin.org/get?x=bar".GetJsonAsync<HttpBinResponse>()).args["x"]);
			Assert.AreEqual("foo", await "https://www.microsoft.com".GetStringAsync());

			// real calls still get logged
			Assert.AreEqual(4, test.CallLog.Count);
			test.ShouldHaveCalled("https://httpbin*").Times(2);
		}

		class RecordingHandler : DelegatingHandler
		{
			public int Hits { get; private set; }

			protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
				Hits++;
				return base.SendAsync(request, cancellationToken);
			}
		}

		[Test] // #683
		public async Task configured_client_used_when_real_http_allowed() {
			var rh = new RecordingHandler();
			var hc = new HttpClient();
			var fc = new FlurlClient(hc);

			using var test = new HttpTest();
			test.RespondWith("fake");
			test.ForCallsTo("*httpbin*").AllowRealHttp();

			Assert.AreNotEqual("fake", fc.Request("https://httpbin.org/get").GetStringAsync());

			// the call got logged
			test.ShouldHaveCalled("https://httpbin*");

			// but the inner handler got hit
			Assert.AreEqual(1, rh.Hits);
		}

		[Test]
		public async Task does_not_create_empty_content_for_forwarding_content_header() {
			// Flurl was auto-creating an empty HttpContent object in order to forward content-level headers,
			// and on .NET Framework a GET with a non-null HttpContent throws an exceptions (#583)
			var calls = new List<FlurlCall>();
			var resp = await "http://httpbingo.org/redirect-to?url=http%3A%2F%2Fexample.com%2F".ConfigureRequest(c => {
				c.Redirects.ForwardHeaders = true;
				c.BeforeCall = call => calls.Add(call);
			}).PostUrlEncodedAsync("test=test");

			Assert.AreEqual(2, calls.Count);
			Assert.AreEqual(HttpMethod.Post, calls[0].Request.Verb);
			Assert.IsNotNull(calls[0].HttpRequestMessage.Content);
			Assert.AreEqual(HttpMethod.Get, calls[1].Request.Verb);
			Assert.IsNull(calls[1].HttpRequestMessage.Content);
		}

		#region cookies
		[Test]
		public async Task can_send_cookies() {
			var req = "https://httpbin.org/cookies".WithCookies(new { x = 1, y = 2 });
			Assert.AreEqual(2, req.Cookies.Count());
			Assert.IsTrue(req.Cookies.Contains(("x", "1")));
			Assert.IsTrue(req.Cookies.Contains(("y", "2")));

			var s = await req.GetStringAsync();

			var resp = await req.WithAutoRedirect(false).GetJsonAsync<HttpBinResponse>();
			// httpbin returns json representation of cookies that were sent
			Assert.AreEqual("1", resp.cookies["x"]);
			Assert.AreEqual("2", resp.cookies["y"]);
		}

		[Test]
		public async Task can_receive_cookies() {
			// endpoint does a redirect, so we need to disable auto-redirect in order to see the cookie in the response
			var resp = await "https://httpbin.org/cookies/set?z=999".WithAutoRedirect(false).GetAsync();
			Assert.AreEqual("999", resp.Cookies.FirstOrDefault(c => c.Name == "z")?.Value);


			// but using WithCookies we can capture it even with redirects enabled
			await "https://httpbin.org/cookies/set?z=999".WithCookies(out var cookies).GetAsync();
			Assert.AreEqual("999", cookies.FirstOrDefault(c => c.Name == "z")?.Value);

			// this works with redirects too
			using (var session = new CookieSession("https://httpbin.org/cookies")) {
				await session.Request("set?z=999").GetAsync();
				Assert.AreEqual("999", session.Cookies.FirstOrDefault(c => c.Name == "z")?.Value);
			}
		}

		[Test]
		public async Task can_set_cookies_before_setting_url() {
			var req = new FlurlRequest().WithCookie("z", "999");
			req.Url = "https://httpbin.org/cookies";
			var resp = await req.GetJsonAsync<HttpBinResponse>();
			Assert.AreEqual("999", resp.cookies["z"]);
		}

		[Test]
		public async Task can_send_different_cookies_per_request() {
			var cli = new FlurlClient();

			var req1 = cli.Request("https://httpbin.org/cookies").WithCookie("x", "123");
			var req2 = cli.Request("https://httpbin.org/cookies").WithCookie("x", "abc");

			var resp2 = await req2.GetJsonAsync<HttpBinResponse>();
			var resp1 = await req1.GetJsonAsync<HttpBinResponse>();

			Assert.AreEqual("123", resp1.cookies["x"]);
			Assert.AreEqual("abc", resp2.cookies["x"]);
		}

		[Test]
		public async Task can_receive_cookie_from_redirect_response_and_add_it_to_jar() {
			// use httpbingo instead of httpbin because of redirect issue https://github.com/postmanlabs/httpbin/issues/617
			var resp = await "https://httpbingo.org/redirect-to"
				.SetQueryParam("url", "/cookies/set?x=foo")
				.WithCookies(out var jar)
				.GetJsonAsync<Dictionary<string, string>>();

			Assert.AreEqual("foo", resp["x"]);
			Assert.AreEqual(1, jar.Count);
		}
		#endregion
	}
}
