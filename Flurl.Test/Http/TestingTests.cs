using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Flurl.Http;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture]
	public class TestingTests
	{
		[Test]
		public void reset_clears_response_queue_and_call_log() {
			FlurlHttp.Testing.Reset();
			FlurlHttp.Testing
				.RespondWith("x")
				.RespondWith("y")
				.RespondWith("z");

			FlurlHttp.Testing.CallLog.Add(null);
			FlurlHttp.Testing.CallLog.Add(null);
			FlurlHttp.Testing.CallLog.Add(null);

			Assert.AreEqual(3, FlurlHttp.Testing.ResponseQueue.Count);
			Assert.AreEqual(3, FlurlHttp.Testing.CallLog.Count);

			FlurlHttp.Testing.Reset();

			Assert.AreEqual(0, FlurlHttp.Testing.ResponseQueue.Count);
			Assert.AreEqual(0, FlurlHttp.Testing.CallLog.Count);
		}

		[Test]
		public void reset_switches_on_test_mode() {
			FlurlHttp.TestMode = false;
			FlurlHttp.Testing.Reset();
			Assert.IsTrue(FlurlHttp.TestMode);
		}

		[Test]
		public async Task fake_get_works() {
			FlurlHttp.Testing.Reset();
			FlurlHttp.Testing.RespondWith("great job");

			await "http://www.api.com".GetAsync();

			var lastCall = FlurlHttp.Testing.CallLog.Last();
			Assert.AreEqual(HttpStatusCode.OK, lastCall.Response.StatusCode);
			Assert.AreEqual("great job", lastCall.ResponseBody);
		}

		[Test]
		public async Task fake_post_works() {
			FlurlHttp.Testing.Reset();
			FlurlHttp.Testing.RespondWith("great job");

			await "http://www.api.com".PostJsonAsync(new { x = 5 });

			var lastCall = FlurlHttp.Testing.CallLog.Last();
			Assert.AreEqual("{\"x\":5}", lastCall.RequestBody);
			Assert.AreEqual(HttpStatusCode.OK, lastCall.Response.StatusCode);
			Assert.AreEqual("great job", lastCall.ResponseBody);
		}

		[Test]
		public async Task no_response_setup_returns_empty_reponse() {
			FlurlHttp.Testing.Reset();

			await "http://www.api.com".GetAsync();

			var lastCall = FlurlHttp.Testing.CallLog.Last();
			Assert.AreEqual(HttpStatusCode.OK, lastCall.Response.StatusCode);
			Assert.AreEqual("", lastCall.ResponseBody);
		}

		[Test]
		public async Task can_setup_multiple_responses() {
			FlurlHttp.Testing
				.Reset()
				.RespondWith("one")
				.RespondWith("two")
				.RespondWith("three");

			await "http://www.api.com/1".GetAsync();
			await "http://www.api.com/2".GetAsync();
			await "http://www.api.com/3".GetAsync();

			var calls = FlurlHttp.Testing.CallLog;
			Assert.AreEqual(3, calls.Count);
			Assert.AreEqual("one", calls[0].ResponseBody);
			Assert.AreEqual("two", calls[1].ResponseBody);
			Assert.AreEqual("three", calls[2].ResponseBody);
		}

	}
}
