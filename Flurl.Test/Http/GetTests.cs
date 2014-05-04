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
	public class GetTests
	{
		[Test]
		public async Task can_get() {
			var test = new HttpTest();

			await "http://www.api.com".GetAsync();

			Assert.AreEqual(1, test.CallLog.Count);
			Assert.AreEqual(HttpMethod.Get, test.CallLog.Single().Request.Method);
		}

		[Test]
		public async Task can_get_json() {
			new HttpTest().RespondWithJson(new TestData { id = 1, name = "Frank" });

			var data = await "http://some-api.com".GetJsonAsync<TestData>();

			Assert.AreEqual(1, data.id);
			Assert.AreEqual("Frank", data.name);
		}

		[Test]
		public async Task can_get_json_dynamic() {
			new HttpTest().RespondWithJson(new { id = 1, name = "Frank" });

			var data = await "http://some-api.com".GetJsonAsync();

			Assert.AreEqual(1, data.id);
			Assert.AreEqual("Frank", data.name);
		}

		[Test]
		public async Task can_get_json_dynamic_list() {
			new HttpTest().RespondWithJson(new[] {
				new { id = 1, name = "Frank" },
				new { id = 2, name = "Claire" }
			});

			var data = await "http://some-api.com".GetJsonListAsync();

			Assert.AreEqual(1, data[0].id);
			Assert.AreEqual("Frank", data[0].name);
			Assert.AreEqual(2, data[1].id);
			Assert.AreEqual("Claire", data[1].name);
		}

		[Test]
		public async Task can_get_string() {
			new HttpTest().RespondWith("good job");

			var data = await "http://some-api.com".GetStringAsync();

			Assert.AreEqual("good job", data);
		}

		[Test]
		public async Task failure_throws_detailed_exception() {
			new HttpTest().RespondWith(500, "bad job");

			try {
				await "http://api.com".GetStringAsync();
				Assert.Fail("FlurlHttpException was not thrown!");
			}
			catch (FlurlHttpException ex) {
				Assert.AreEqual("http://api.com/", ex.Call.Request.RequestUri.AbsoluteUri);
				Assert.AreEqual(HttpMethod.Get, ex.Call.Request.Method);
				Assert.AreEqual(HttpStatusCode.InternalServerError, ex.Call.Response.StatusCode);
			}
		}

		private class TestData
		{
			public int id { get; set; }
			public string name { get; set; }
		}
	}
}
