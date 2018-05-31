using System;
using System.Linq;
using System.Net;
using System.Net.Http;
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
	    public async Task can_assert_url() {
	        await "http://api.com"
	            .AppendPathSegment("test")
	            .GetAsync();

	        HttpTest.ShouldHaveMadeACall().WithUrlPattern("http://api.com/test");

	        Assert.Throws<HttpCallAssertException>(() =>
	                HttpTest.ShouldHaveMadeACall().WithUrlPattern("http://api.com"));
	    }

		[Test]
		public async Task fake_get_works() {
			HttpTest.RespondWith("great job");

			await "http://www.api.com".GetAsync();

			var lastCall = HttpTest.CallLog.Last();
			Assert.AreEqual(HttpStatusCode.OK, lastCall.Response.StatusCode);
			Assert.AreEqual("great job", await lastCall.Response.Content.ReadAsStringAsync());
		}

		[Test]
		public async Task fake_post_works() {
			HttpTest.RespondWith("great job");

			await "http://www.api.com".PostJsonAsync(new { x = 5 });

			var lastCall = HttpTest.CallLog.Last();
			Assert.AreEqual("{\"x\":5}", lastCall.RequestBody);
			Assert.AreEqual(HttpStatusCode.OK, lastCall.Response.StatusCode);
			Assert.AreEqual("great job", await lastCall.Response.Content.ReadAsStringAsync());
		}

		[Test]
		public async Task no_response_setup_returns_empty_reponse() {
			await "http://www.api.com".GetAsync();

			var lastCall = HttpTest.CallLog.Last();
			Assert.AreEqual(HttpStatusCode.OK, lastCall.Response.StatusCode);
			Assert.AreEqual("", await lastCall.Response.Content.ReadAsStringAsync());
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
			Assert.AreEqual("one", await calls[0].Response.Content.ReadAsStringAsync());
			Assert.AreEqual("two", await calls[1].Response.Content.ReadAsStringAsync());
			Assert.AreEqual("three", await calls[2].Response.Content.ReadAsStringAsync());

			HttpTest.ShouldHaveMadeACall();
			HttpTest.ShouldHaveCalled("http://www.api.com/*").WithVerb(HttpMethod.Get).Times(3);
			HttpTest.ShouldNotHaveCalled("http://www.otherapi.com/*");
		}

		[Test]
		public async Task can_assert_query_params() {
			await "http://www.api.com?x=111&y=222&z=333#abcd".GetAsync();

			HttpTest.ShouldHaveCalled("http://www.api.com*").WithQueryParams();
			HttpTest.ShouldHaveMadeACall().WithQueryParam("x");
			HttpTest.ShouldHaveCalled("http://www.api.com*").WithQueryParams("z", "y");
			HttpTest.ShouldHaveMadeACall().WithQueryParamValue("y", 222);
			HttpTest.ShouldHaveCalled("http://www.api.com*").WithQueryParamValue("z", "*3");
			HttpTest.ShouldHaveMadeACall().WithQueryParamValues(new { z = 333, y = 222 });

			// without
			HttpTest.ShouldHaveCalled("http://www.api.com*").WithoutQueryParam("w");
			HttpTest.ShouldHaveMadeACall().WithoutQueryParams("t", "u", "v");
			HttpTest.ShouldHaveCalled("http://www.api.com*").WithoutQueryParamValue("x", 112);
			HttpTest.ShouldHaveMadeACall().WithoutQueryParamValues(new { x = 112, y = 223, z = 666 });

			// failures
			Assert.Throws<HttpCallAssertException>(() =>
				HttpTest.ShouldHaveMadeACall().WithQueryParam("w"));
			Assert.Throws<HttpCallAssertException>(() =>
				HttpTest.ShouldHaveMadeACall().WithQueryParamValue("y", 223));
			Assert.Throws<HttpCallAssertException>(() =>
				HttpTest.ShouldHaveMadeACall().WithQueryParamValue("z", "*4"));
			Assert.Throws<HttpCallAssertException>(() =>
				HttpTest.ShouldHaveMadeACall().WithQueryParamValues(new { x = 111, y = 666 }));

			Assert.Throws<HttpCallAssertException>(() =>
				HttpTest.ShouldHaveMadeACall().WithoutQueryParams());
			Assert.Throws<HttpCallAssertException>(() =>
				HttpTest.ShouldHaveMadeACall().WithoutQueryParam("x"));
			Assert.Throws<HttpCallAssertException>(() =>
				HttpTest.ShouldHaveMadeACall().WithoutQueryParams("z", "y"));
			Assert.Throws<HttpCallAssertException>(() =>
				HttpTest.ShouldHaveMadeACall().WithoutQueryParamValue("y", 222));
			Assert.Throws<HttpCallAssertException>(() =>
				HttpTest.ShouldHaveMadeACall().WithoutQueryParamValue("z", "*3"));
			Assert.Throws<HttpCallAssertException>(() =>
				HttpTest.ShouldHaveMadeACall().WithoutQueryParamValues(new { z = 333, y = 222 }));
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
			HttpTest.ShouldHaveMadeACall().WithQueryParamValue("x", new[] { 2, 1 }); // order shouldn't matter
			HttpTest.ShouldHaveMadeACall().WithQueryParamValues(new { x = new[] { 3, 2, 1 } }); // order shouldn't matter

			Assert.Throws<HttpCallAssertException>(() =>
				HttpTest.ShouldHaveMadeACall().WithQueryParamValue("x", new[] { 1, 2, 4 }));
			Assert.Throws<HttpCallAssertException>(() =>
				HttpTest.ShouldHaveMadeACall().WithQueryParamValues(new { x = new[] { 1, 2, 4 } }));
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

			Assert.Throws<HttpCallAssertException>(() =>
				HttpTest.ShouldHaveMadeACall().WithHeader("h3"));
			Assert.Throws<HttpCallAssertException>(() =>
				HttpTest.ShouldHaveMadeACall().WithoutHeader("h1"));
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
			Assert.AreEqual(1, resp.Headers.Count());
			Assert.AreEqual("h1", resp.Headers.First().Key);
			Assert.AreEqual("foo", resp.Headers.First().Value.First());
		}

		[Test]
		public async Task can_fake_cookies() {
			HttpTest.RespondWith(cookies: new { c1 = "foo" });

			var rec = "http://www.api.com".EnableCookies();
			await rec.GetAsync();
			Assert.AreEqual(1, rec.Cookies.Count);
			Assert.AreEqual("foo", rec.Cookies["c1"].Value);
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
			Assert.IsFalse(new FlurlRequest().Settings.CookiesEnabled);
			using (new HttpTest().Configure(settings => settings.CookiesEnabled = true)) {
				Assert.IsTrue(new FlurlRequest().Settings.CookiesEnabled);
			}
			// test disposed, should revert back to global settings
			Assert.IsFalse(new FlurlRequest().Settings.CookiesEnabled);
		}

		[Test]
		public async Task can_test_in_parallel() {
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

		private async Task CallAndAssertCountAsync(int calls) {
			using (var test = new HttpTest()) {
				for (int i = 0; i < calls; i++) {
					await "http://www.api.com".GetAsync();
					await Task.Delay(100);
				}
				test.ShouldHaveCalled("http://www.api.com").Times(calls);
				//Console.WriteLine($"{calls} calls expected, {test.CallLog.Count} calls made");
			}
		}
	}
}
