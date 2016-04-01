using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
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
	}
}
