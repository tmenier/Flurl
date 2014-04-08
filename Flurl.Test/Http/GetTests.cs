using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture]
	public class GetTests
	{
		[SetUp]
		public void Setup() {
			FlurlHttp.Testing.Reset();
		}

		[Test]
		public async Task can_get() {
			await "http://www.api.com".GetAsync();
			HttpAssert.LastRequest(HttpMethod.Get, null, null);
		}

		[Test]
		public async Task can_get_json() {
			FlurlHttp.Testing.RespondWithJson(new TestData { id = 1, name = "Frank" });
			var data = await "http://some-api.com".GetJsonAsync<TestData>();

			Assert.AreEqual(1, data.id);
			Assert.AreEqual("Frank", data.name);
		}

		[Test]
		public async Task can_get_json_dynamic() {
			FlurlHttp.Testing.RespondWithJson(new { id = 1, name = "Frank" });
			var data = await "http://some-api.com".GetJsonAsync();

			Assert.AreEqual(1, data.id);
			Assert.AreEqual("Frank", data.name);
		}

		[Test]
		public async Task can_get_json_dynamic_list() {
			FlurlHttp.Testing.RespondWithJson(new[] {
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
			FlurlHttp.Testing.RespondWith("good job");
			var data = await "http://some-api.com".GetStringAsync();

			Assert.AreEqual("good job", data);
		}

		[Test]
		public async Task failure_throws_detailed_exception() {
			FlurlHttp.Testing.RespondWith(500, "bad job");

			try {
				await "http://api.com".GetStringAsync();
			}
			catch (FlurlHttpException ex) {
				Assert.AreEqual("http://api.com/", ex.Request.RequestUri.AbsoluteUri);
				Assert.AreEqual(HttpMethod.Get, ex.Request.Method);
				Assert.AreEqual(HttpStatusCode.InternalServerError, ex.Response.StatusCode);
				Assert.AreEqual(HttpStatusCode.Inte, ex.Response.StatusCode);
			}
		}

		private class TestData
		{
			public int id { get; set; }
			public string name { get; set; }
		}
	}
}
