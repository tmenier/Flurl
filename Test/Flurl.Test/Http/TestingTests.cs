using System;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Testing;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture, Parallelizable]
	public class TestingTests : HttpTestFixtureBase
	{
		[Test]
		public async Task fake_get_works() {
			HttpTest.RespondWith("great job");

			await "http://www.api.com".GetAsync();

			var lastCall = HttpTest.CallLog.Last();
			Assert.AreEqual(200, lastCall.Response.StatusCode);
			Assert.AreEqual("great job", await lastCall.Response.GetStringAsync());
		}

		[Test]
		public async Task fake_post_works() {
			HttpTest.RespondWith("great job");

			await "http://www.api.com".PostJsonAsync(new { x = 5 });

			var lastCall = HttpTest.CallLog.Last();
			Assert.AreEqual("{\"x\":5}", lastCall.RequestBody);
			Assert.AreEqual(200, lastCall.Response.StatusCode);
			Assert.AreEqual("great job", await lastCall.Response.GetStringAsync());
		}

		[Test]
		public async Task no_response_setup_returns_empty_reponse() {
			await "http://www.api.com".GetAsync();

			var lastCall = HttpTest.CallLog.Last();
			Assert.AreEqual(200, lastCall.Response.StatusCode);
			Assert.AreEqual("", await lastCall.Response.GetStringAsync());
		}

		[Test]
		public async Task can_setup_multiple_responses() {
			HttpTest
				.RespondWith("one")
				.RespondWith("two")
				.RespondWith("three");

			HttpTest.ShouldNotHaveMadeACall();

			await "http://www.api.com/1".GetAsync();
			await "http://www.api.com/2".GetAsync();
			await "http://www.api.com/3".GetAsync();

			var calls = HttpTest.CallLog;
			Assert.AreEqual(3, calls.Count);
			Assert.AreEqual("one", await calls[0].Response.GetStringAsync());
			Assert.AreEqual("two", await calls[1].Response.GetStringAsync());
			Assert.AreEqual("three", await calls[2].Response.GetStringAsync());

			HttpTest.ShouldHaveMadeACall();
			HttpTest.ShouldHaveCalled("http://www.api.com/*").WithVerb(HttpMethod.Get).Times(3);
			HttpTest.ShouldNotHaveCalled("http://www.otherapi.com/*");

			// #323 make sure it's a full string match and not a "contains"
			Assert.Throws<HttpTestException>(() => HttpTest.ShouldHaveCalled("http://www.api.com/"));
			HttpTest.ShouldNotHaveCalled("http://www.api.com/");
		}

		[Test] // #482
		public async Task last_response_is_sticky() {
			HttpTest.RespondWith("1").RespondWith("2").RespondWith("3");

			Assert.AreEqual("1", await "http://api.com".GetStringAsync());
			Assert.AreEqual("2", await "http://api.com".GetStringAsync());
			Assert.AreEqual("3", await "http://api.com".GetStringAsync());
			Assert.AreEqual("3", await "http://api.com".GetStringAsync());
			Assert.AreEqual("3", await "http://api.com".GetStringAsync());
		}

		[Test]
		public async Task can_respond_based_on_url() {
			HttpTest.RespondWith("never");
			HttpTest.ForCallsTo("*/1").RespondWith("one");
			HttpTest.ForCallsTo("*/2").RespondWith("two");
			HttpTest.ForCallsTo("*/3").RespondWith("three");
			HttpTest.ForCallsTo("http://www.api.com/*").RespondWith("foo!");

			Assert.AreEqual("foo!", await "http://www.api.com/4".GetStringAsync());
			Assert.AreEqual("three", await "http://www.api.com/3".GetStringAsync());
			Assert.AreEqual("two", await "http://www.api.com/2".GetStringAsync());
			Assert.AreEqual("one", await "http://www.api.com/1".GetStringAsync());

			Assert.AreEqual(4, HttpTest.CallLog.Count);
		}

		[Test]
		public async Task can_respond_based_on_verb() {
			HttpTest.RespondWith("catch-all");

			HttpTest
				.ForCallsTo("http://www.api.com*")
				.WithVerb(HttpMethod.Post)
				.RespondWith("I posted.");

			HttpTest
				.ForCallsTo("http://www.api.com*")
				.WithVerb("put", "PATCH")
				.RespondWith("I put or patched.");

			Assert.AreEqual("I put or patched.", await "http://www.api.com/1".PatchAsync(null).ReceiveString());
			Assert.AreEqual("I posted.", await "http://www.api.com/2".PostAsync(null).ReceiveString());
			Assert.AreEqual("I put or patched.", await "http://www.api.com/3".SendAsync(HttpMethod.Put, null).ReceiveString());
			Assert.AreEqual("catch-all", await "http://www.api.com/4".DeleteAsync().ReceiveString());

			Assert.AreEqual(4, HttpTest.CallLog.Count);
		}

		[Test]
		public async Task can_respond_based_on_query_params() {
			HttpTest
				.ForCallsTo("*")
				.WithQueryParam("x", 1)
				.WithQueryParams(new { y = 2, z = 3 })
				.WithAnyQueryParam("a", "b", "c")
				.WithoutQueryParam("d")
				.WithoutQueryParams(new { c = "n*" })
				.RespondWith("query param conditions met!");

			Assert.AreEqual("", await "http://api.com?x=1&y=2&a=yes".GetStringAsync());
			Assert.AreEqual("", await "http://api.com?y=2&z=3&b=yes".GetStringAsync());
			Assert.AreEqual("", await "http://api.com?x=1&y=2&z=3&c=yes&d=yes".GetStringAsync());
			Assert.AreEqual("", await "http://api.com?x=1&y=2&z=3&c=no".GetStringAsync());
			Assert.AreEqual("query param conditions met!", await "http://api.com?x=1&y=2&z=3&c=yes".GetStringAsync());
		}

		[Test] // #596
		public async Task url_patterns_ignore_query_when_not_specified() {
			HttpTest.ForCallsTo("http://api.com/1").RespondWith("one");
			HttpTest.ForCallsTo("http://api.com/2").WithAnyQueryParam().RespondWith("two");
			HttpTest.ForCallsTo("http://api.com/3").WithoutQueryParams().RespondWith("three");

			Assert.AreEqual("one", await "http://api.com/1".GetStringAsync());
			Assert.AreEqual("one", await "http://api.com/1?x=foo&y=bar".GetStringAsync());

			Assert.AreEqual("", await "http://api.com/2".GetStringAsync());
			Assert.AreEqual("two", await "http://api.com/2?x=foo&y=bar".GetStringAsync());

			Assert.AreEqual("three", await "http://api.com/3".GetStringAsync());
			Assert.AreEqual("", await "http://api.com/3?x=foo&y=bar".GetStringAsync());

			HttpTest.ShouldHaveCalled("http://api.com/1").Times(2);
			HttpTest.ShouldHaveCalled("http://api.com/1").WithAnyQueryParam().Times(1);
			HttpTest.ShouldHaveCalled("http://api.com/1").WithoutQueryParams().Times(1);
			HttpTest.ShouldHaveCalled("http://api.com/1?x=foo").Times(1);
			HttpTest.ShouldHaveCalled("http://api.com/1?x=foo").WithQueryParam("y").Times(1);
			HttpTest.ShouldHaveCalled("http://api.com/1?x=foo").WithQueryParam("y", "bar").Times(1);
		}

		[Test]
		public async Task can_respond_based_on_headers() {
			HttpTest
				.ForCallsTo("*")
				.WithHeader("x")
				.WithHeader("y", "f*o")
				.WithoutHeader("y", "flo")
				.WithoutHeader("z")
				.RespondWith("header conditions met!");

			Assert.AreEqual("", await "http://api.com".WithHeaders(new { y = "foo" }).GetStringAsync());
			Assert.AreEqual("", await "http://api.com".WithHeaders(new { x = 1, y = "flo" }).GetStringAsync());
			Assert.AreEqual("", await "http://api.com".WithHeaders(new { x = 1, y = "foo", z = 2 }).GetStringAsync());
			Assert.AreEqual("header conditions met!", await "http://api.com".WithHeaders(new { x = 1, y = "foo" }).GetStringAsync());
		}

		[Test]
		public async Task can_respond_based_on_body() {
			HttpTest
				.ForCallsTo("*")
				.WithRequestBody("*something*")
				.WithRequestJson(new { a = "*", b = new { c = "*", d = "yes" } })
				.RespondWith("body conditions met!");

			Assert.AreEqual("", await "http://api.com".PostStringAsync("something").ReceiveString());
			Assert.AreEqual("", await "http://api.com".PostJsonAsync(
				new { a = "hi", b = new { c = "bye", d = "yes" } }).ReceiveString());

			Assert.AreEqual("body conditions met!", await "http://api.com".PostJsonAsync(
				new { a = "hi", b = new { c = "this is something!", d = "yes" } }).ReceiveString());
		}

		[Test]
		public async Task can_respond_based_on_any_call_condition() {
			HttpTest
				.ForCallsTo("*")
				.With(call => call.Request.Url.Fragment.StartsWith("abc"))
				.Without(call => call.Request.Url.Fragment.EndsWith("xyz"))
				.RespondWith("arbitrary conditions met!");

			Assert.AreEqual("", await "http://api.com#abcxyz".GetStringAsync());
			Assert.AreEqual("", await "http://api.com#xyz".GetStringAsync());
			Assert.AreEqual("arbitrary conditions met!", await "http://api.com#abcxy".GetStringAsync());
		}

		[Test]
		public async Task can_assert_verb() {
			await "http://www.api.com/1".PostStringAsync("");
			await "http://www.api.com/2".PutStringAsync("");
			await "http://www.api.com/3".PatchStringAsync("");
			await "http://www.api.com/4".DeleteAsync();

			HttpTest.ShouldHaveMadeACall().WithVerb(HttpMethod.Post).Times(1);
			HttpTest.ShouldHaveMadeACall().WithVerb("put", "PATCH").Times(2);
			HttpTest.ShouldHaveMadeACall().WithVerb("get", "delete").Times(1);
			Assert.Throws<HttpTestException>(() => HttpTest.ShouldHaveMadeACall().WithVerb(HttpMethod.Get));
		}

		[Test]
		public async Task can_assert_json_request() {
			var body = new { a = 1, b = 2 };
			await "http://some-api.com".PostJsonAsync(body);

			HttpTest.ShouldHaveMadeACall().WithRequestJson(body);
		}

		[Test]
		public async Task can_assert_url_encoded_request() {
			var body = new { a = 1, b = 2, c = "hi there", d = new[] { 1, 2, 3 } };
			await "http://some-api.com".PostUrlEncodedAsync(body);

			HttpTest.ShouldHaveMadeACall().WithRequestUrlEncoded(body);
		}

		[Test]
		public async Task can_assert_query_params() {
			await "http://www.api.com?x=111&y=222&z=333#abcd".GetAsync();

			HttpTest.ShouldHaveCalled("http://www.api.com*").WithQueryParams();
			HttpTest.ShouldHaveMadeACall().WithQueryParam("x");
			HttpTest.ShouldHaveCalled("http://www.api.com*").WithQueryParams("z", "y");
			HttpTest.ShouldHaveMadeACall().WithQueryParam("y", 222);
			HttpTest.ShouldHaveCalled("http://www.api.com*").WithQueryParam("z", "*3");
			HttpTest.ShouldHaveMadeACall().WithQueryParams(new { z = 333, y = 222 });
			HttpTest.ShouldHaveMadeACall().WithQueryParams(new { z = "*", y = 222, x = "*" });
			HttpTest.ShouldHaveMadeACall().WithAnyQueryParam("a", "z", "b");

			// without
			HttpTest.ShouldHaveCalled("http://www.api.com*").WithoutQueryParam("w");
			HttpTest.ShouldHaveMadeACall().WithoutQueryParams("t", "u", "v");
			HttpTest.ShouldHaveCalled("http://www.api.com*").WithoutQueryParam("x", 112);
			HttpTest.ShouldHaveMadeACall().WithoutQueryParams(new { x = 112, y = 223, z = 666 });
			HttpTest.ShouldHaveMadeACall().WithoutQueryParams(new { a = "*", b = "*" });

			// failures
			Assert.Throws<HttpTestException>(() =>
				HttpTest.ShouldHaveMadeACall().WithQueryParam("w"));
			Assert.Throws<HttpTestException>(() =>
				HttpTest.ShouldHaveMadeACall().WithQueryParam("y", 223));
			Assert.Throws<HttpTestException>(() =>
				HttpTest.ShouldHaveMadeACall().WithQueryParam("z", "*4"));
			Assert.Throws<HttpTestException>(() =>
				HttpTest.ShouldHaveMadeACall().WithQueryParams(new { x = 111, y = 666 }));

			Assert.Throws<HttpTestException>(() =>
				HttpTest.ShouldHaveMadeACall().WithoutQueryParams());
			Assert.Throws<HttpTestException>(() =>
				HttpTest.ShouldHaveMadeACall().WithoutQueryParam("x"));
			Assert.Throws<HttpTestException>(() =>
				HttpTest.ShouldHaveMadeACall().WithoutQueryParams("z", "y"));
			Assert.Throws<HttpTestException>(() =>
				HttpTest.ShouldHaveMadeACall().WithoutQueryParam("y", 222));
			Assert.Throws<HttpTestException>(() =>
				HttpTest.ShouldHaveMadeACall().WithoutQueryParam("z", "*3"));
			Assert.Throws<HttpTestException>(() =>
				HttpTest.ShouldHaveMadeACall().WithoutQueryParams(new { z = 333, y = 222 }));
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task can_assert_multiple_occurances_of_query_param(bool buildFluently) {
			// #276 showed that this failed when the URL was built fluently (caused by #301)
			var url = buildFluently ?
				"http://www.api.com".SetQueryParam("x", new[] { 1, 2, 3 }).SetQueryParam("y", 4).SetFragment("abcd") :
				new Url("http://www.api.com?x=1&x=2&x=3&y=4#abcd");

			await url.GetAsync();

			HttpTest.ShouldHaveMadeACall().WithQueryParam("x");
			HttpTest.ShouldHaveMadeACall().WithQueryParam("x", new[] { 2, 1 }); // order shouldn't matter
			HttpTest.ShouldHaveMadeACall().WithQueryParams(new { x = new[] { 3, 2, 1 } }); // order shouldn't matter

			Assert.Throws<HttpTestException>(() =>
				HttpTest.ShouldHaveMadeACall().WithQueryParam("x", new[] { 1, 2, 4 }));
			Assert.Throws<HttpTestException>(() =>
				HttpTest.ShouldHaveMadeACall().WithQueryParams(new { x = new[] { 1, 2, 4 } }));
		}

		[Test]
		public async Task can_assert_headers() {
			await "http://api.com"
				.WithHeaders(new { h1 = "val1", h2 = "val2" })
				.WithHeader("User-Agent", "two words") // #307
				.WithHeader("x", "dos       words")    // crazier than #307
				.WithHeader("y", "hi;  there")         // crazier still
				.GetAsync();

			HttpTest.ShouldHaveMadeACall().WithHeader("h1");
			HttpTest.ShouldHaveMadeACall().WithHeader("h2", "val2");
			HttpTest.ShouldHaveMadeACall().WithHeader("h1", "val*");
			HttpTest.ShouldHaveMadeACall().WithHeader("User-Agent", "two words");
			HttpTest.ShouldHaveMadeACall().WithHeader("x", "dos       words");
			HttpTest.ShouldHaveMadeACall().WithHeader("y", "hi;  there");

			HttpTest.ShouldHaveMadeACall().WithoutHeader("h3");
			HttpTest.ShouldHaveMadeACall().WithoutHeader("h2", "val1");
			HttpTest.ShouldHaveMadeACall().WithoutHeader("h1", "foo*");

			Assert.Throws<HttpTestException>(() =>
				HttpTest.ShouldHaveMadeACall().WithHeader("h3"));
			Assert.Throws<HttpTestException>(() =>
				HttpTest.ShouldHaveMadeACall().WithoutHeader("h1"));
		}

		[Test]
		public async Task can_assert_oauth_token() {
			await "https://auth.com".WithOAuthBearerToken("foo").GetAsync();
			HttpTest.ShouldHaveMadeACall().WithOAuthBearerToken();
			HttpTest.ShouldHaveMadeACall().WithOAuthBearerToken("foo");
			HttpTest.ShouldHaveMadeACall().WithOAuthBearerToken("*oo");
			Assert.Throws<HttpTestException>(() => HttpTest.ShouldHaveMadeACall().WithOAuthBearerToken("bar"));
			Assert.Throws<HttpTestException>(() => HttpTest.ShouldHaveMadeACall().WithBasicAuth());
		}

		[Test]
		public async Task can_assert_basic_auth() {
			await "https://auth.com".WithBasicAuth("me", "letmein").GetAsync();
			HttpTest.ShouldHaveMadeACall().WithBasicAuth();
			HttpTest.ShouldHaveMadeACall().WithBasicAuth("me", "letmein");
			HttpTest.ShouldHaveMadeACall().WithBasicAuth("me");
			HttpTest.ShouldHaveMadeACall().WithBasicAuth("m*", "*in");
			Assert.Throws<HttpTestException>(() => HttpTest.ShouldHaveMadeACall().WithBasicAuth("me", "wrong"));
			Assert.Throws<HttpTestException>(() => HttpTest.ShouldHaveMadeACall().WithBasicAuth("you"));
			Assert.Throws<HttpTestException>(() => HttpTest.ShouldHaveMadeACall().WithBasicAuth("m*", "*out"));
			Assert.Throws<HttpTestException>(() => HttpTest.ShouldHaveMadeACall().WithOAuthBearerToken());
		}

		[Test]
		public async Task can_simulate_timeout() {
			HttpTest.SimulateTimeout();
			try {
				await "http://www.api.com".GetAsync();
				Assert.Fail("Exception was not thrown!");
			}
			catch (FlurlHttpTimeoutException ex) {
				Assert.IsInstanceOf<TaskCanceledException>(ex.InnerException);
				StringAssert.Contains("timed out", ex.Message);
			}
		}

		[Test]
		public async Task can_simulate_exception() {
			var expectedException = new SocketException();
			HttpTest.SimulateException(expectedException);
			try {
				await "http://www.api.com".GetAsync();
				Assert.Fail("Exception was not thrown!");
			}
			catch (FlurlHttpException ex) {
				Assert.AreEqual(expectedException, ex.InnerException);
			}
		}

		[Test]
	    public async Task can_simulate_timeout_with_exception_handled() {
	        HttpTest.SimulateTimeout();
	        var result = await "http://www.api.com"
	            .ConfigureRequest(c => c.OnError = call => call.ExceptionHandled = true)
	            .GetAsync();
	        Assert.IsNull(result);
	    }

	    [Test]
		public async Task can_fake_headers() {
			HttpTest.RespondWith(headers: new { h1 = "foo" });

			var resp = await "http://www.api.com".GetAsync();
			Assert.AreEqual(("h1", "foo"), resp.Headers.Single());
		}

		[Test]
		public async Task can_fake_cookies() {
			HttpTest.RespondWith(cookies: new { c1 = "foo" });

			var resp = await "http://www.api.com".GetAsync();
			Assert.AreEqual(1, resp.Cookies.Count);
			Assert.AreEqual("foo", resp.Cookies.FirstOrDefault(c => c.Name == "c1")?.Value);
		}

		// https://github.com/tmenier/Flurl/issues/175
		[Test]
		public async Task can_deserialize_default_response_more_than_once() {
			var resp = await "http://www.api.com".GetJsonAsync();
			Assert.IsNull(resp);
			// bug: couldn't deserialize here due to reading stream twice
			resp = await "http://www.api.com".GetJsonAsync();
			Assert.IsNull(resp);
			resp = await "http://www.api.com".GetJsonAsync();
			Assert.IsNull(resp);
		}

		[Test]
		public void can_configure_settings_for_test() {
			Assert.IsTrue(new FlurlRequest().Settings.Redirects.Enabled);
			using (new HttpTest().Configure(settings => settings.Redirects.Enabled = false)) {
				Assert.IsFalse(new FlurlRequest().Settings.Redirects.Enabled);
			}
			// test disposed, should revert back to global settings
			Assert.IsTrue(new FlurlRequest().Settings.Redirects.Enabled);
		}

		[Test]
		public async Task can_test_in_parallel() {
			async Task CallAndAssertCountAsync(int calls) {
				using (var test = new HttpTest()) {
					for (int i = 0; i < calls; i++) {
						await "http://www.api.com".GetAsync();
						await Task.Delay(100);
					}
					test.ShouldHaveCalled("http://www.api.com").Times(calls);
					//Console.WriteLine($"{calls} calls expected, {test.CallLog.Count} calls made");
				}
			}

			await Task.WhenAll(
				CallAndAssertCountAsync(7),
				CallAndAssertCountAsync(5),
				CallAndAssertCountAsync(3),
				CallAndAssertCountAsync(4),
				CallAndAssertCountAsync(6));
		}

		[Test]
		public async Task does_not_throw_nullref_for_empty_content() {
			await "http://some-api.com".AppendPathSegment("foo").SendAsync(HttpMethod.Post, null);
			await "http://some-api.com".AppendPathSegment("foo").PostJsonAsync(new { foo = "bar" });

			HttpTest.ShouldHaveCalled("http://some-api.com/foo")
				.WithVerb(HttpMethod.Post)
				.WithContentType("application/json");
		}

		[Test] // #331
		public async Task can_fake_content_headers() {
			HttpTest.RespondWith("<xml></xml>", 200, new { Content_Type = "text/xml" });
			await "http://api.com".GetAsync();
			HttpTest.ShouldHaveMadeACall().With(call => call.Response.Headers.Contains(("Content-Type", "text/xml")));
			HttpTest.ShouldHaveMadeACall().With(call => call.HttpResponseMessage.Content.Headers.ContentType.MediaType == "text/xml");
		}

		[Test] // #335
		public async Task doesnt_record_calls_made_with_HttpClient() {
			using (var httpTest = new HttpTest()) {
				httpTest.RespondWith("Hello");
				var flurClient = new FlurlClient();
				// use the underlying HttpClient directly
				await flurClient.HttpClient.GetStringAsync("https://www.google.com/");
				CollectionAssert.IsEmpty(httpTest.CallLog);
			}
		}

		[Test] // #366 & #398
		public async Task can_use_response_queue_in_parallel() {
			// this was hard to test. numbers used (200 ms delay, 10 calls, repeat 5 times) were not
			// arrived at by any exact science. they just seemed to be the point where failure is
			// virtually guaranteed without thread-safe collections backing ResponseQueue and CallLog,
			// but without making the test unbearably slow.
			var cli = new FlurlClient("http://api.com");
			cli.Settings.BeforeCallAsync = call => Task.Delay(200);

			for (var i = 0; i < 5; i++) {
				using (var test = new HttpTest()) {
					test
						.RespondWith("0")
						.RespondWith("1")
						.RespondWith("2")
						.RespondWith("3")
						.RespondWith("4")
						.RespondWith("5")
						.RespondWith("6")
						.RespondWith("7")
						.RespondWith("8")
						.RespondWith("9");

					var results = await Task.WhenAll(
						cli.Request().GetStringAsync(),
						cli.Request().GetStringAsync(),
						cli.Request().GetStringAsync(),
						cli.Request().GetStringAsync(),
						cli.Request().GetStringAsync(),
						cli.Request().GetStringAsync(),
						cli.Request().GetStringAsync(),
						cli.Request().GetStringAsync(),
						cli.Request().GetStringAsync(),
						cli.Request().GetStringAsync());

					CollectionAssert.AllItemsAreUnique(results);
					test.ShouldHaveMadeACall().Times(10);
				}
			}
		}
	}
}
