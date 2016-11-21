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
	[TestFixture]
	public class TestingTests : HttpTestFixtureBase
	{
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

			await "http://www.api.com/1".GetAsync();
			await "http://www.api.com/2".GetAsync();
			await "http://www.api.com/3".GetAsync();

			var calls = HttpTest.CallLog;
			Assert.AreEqual(3, calls.Count);
			Assert.AreEqual("one", await calls[0].Response.Content.ReadAsStringAsync());
			Assert.AreEqual("two", await calls[1].Response.Content.ReadAsStringAsync());
			Assert.AreEqual("three", await calls[2].Response.Content.ReadAsStringAsync());

			HttpTest.ShouldHaveCalled("http://www.api.com/*").WithVerb(HttpMethod.Get).Times(3);
			HttpTest.ShouldNotHaveCalled("http://www.otherapi.com/*");
		}

		[Test]
		public async Task can_assert_query_params() {
			await "http://www.api.com?x=111&y=222&z=333".GetAsync();

			HttpTest.ShouldHaveCalled("http://www.api.com*").WithQueryParam("x");
			HttpTest.ShouldHaveCalled("http://www.api.com*").WithQueryParam("y", 222);
			HttpTest.ShouldHaveCalled("*").WithQueryParam("z", "*3");
			HttpTest.ShouldHaveCalled("*").WithQueryParams(new { z = 333, y = 222 });

			Assert.Throws<HttpCallAssertException>(() =>
				HttpTest.ShouldHaveCalled("http://www.api.com*").WithQueryParam("w"));
			Assert.Throws<HttpCallAssertException>(() =>
				HttpTest.ShouldHaveCalled("http://www.api.com*").WithQueryParam("y", 223));
			Assert.Throws<HttpCallAssertException>(() =>
				HttpTest.ShouldHaveCalled("*").WithQueryParam("z", "*4"));
			Assert.Throws<HttpCallAssertException>(() =>
				HttpTest.ShouldHaveCalled("*").WithQueryParams(new { x = 111, y = 666 }));
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

			var fc = "http://www.api.com".EnableCookies();
			await fc.GetAsync();
			Assert.AreEqual(1, fc.Cookies.Count());
			Assert.AreEqual("foo", fc.Cookies["c1"].Value);
		}

// parallel testing not supported in PCL
#if !PORTABLE
		[Test]
		public async Task can_test_in_parallel() {
			await Task.WhenAll(
				CallAndAssertCountAsync(7),
				CallAndAssertCountAsync(5),
				CallAndAssertCountAsync(3),
				CallAndAssertCountAsync(4),
				CallAndAssertCountAsync(6));
		}
#endif

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
