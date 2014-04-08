using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture]
	public class PostTests
	{
		[SetUp]
		public void Setup() {
			FlurlHttp.Testing.Reset();
		}

		[Test]
		public async Task can_post_json() {
			await "http://some-api.com".PostJsonAsync(new { a = 1, b = 2 });
			HttpAssert.LastRequest(HttpMethod.Post, "application/json", "{\"a\":1,\"b\":2}");
		}

		[Test]
		public async Task can_post_url_encoded() {
			await "http://some-api.com".PostUrlEncodedAsync(new { a = 1, b = 2, c = "hi there" });
			HttpAssert.LastRequest(HttpMethod.Post, "application/x-www-form-urlencoded", "a=1&b=2&c=hi+there");
		}

		[Test]
		public async Task can_receive_json() {
			FlurlHttp.Testing.RespondWithJson(new TestData { id = 1, name = "Frank" });
			var data = await "http://some-api.com".PostJsonAsync(new { a = 1, b = 2 }).ReceiveJsonAsync<TestData>();

			Assert.AreEqual(1, data.id);
			Assert.AreEqual("Frank", data.name);
		}

		[Test]
		public async Task can_receive_json_dynamic() {
			FlurlHttp.Testing.RespondWithJson(new { id = 1, name = "Frank" });
			var data = await "http://some-api.com".PostJsonAsync(new { a = 1, b = 2 }).ReceiveJsonAsync();

			Assert.AreEqual(1, data.id);
			Assert.AreEqual("Frank", data.name);
		}

		[Test]
		public async Task can_receive_json_dynamic_list() {
			FlurlHttp.Testing.RespondWithJson(new[] {
				new { id = 1, name = "Frank" },
				new { id = 2, name = "Claire" }
			});
			var data = await "http://some-api.com".PostJsonAsync(new { a = 1, b = 2 }).ReceiveJsonListAsync();

			Assert.AreEqual(1, data[0].id);
			Assert.AreEqual("Frank", data[0].name);
			Assert.AreEqual(2, data[1].id);
			Assert.AreEqual("Claire", data[1].name);
		}

		[Test]
		public async Task can_receive_string() {
			FlurlHttp.Testing.RespondWith("good job");
			var data = await "http://some-api.com".PostJsonAsync(new { a = 1, b = 2 }).ReceiveStringAsync();

			Assert.AreEqual("good job", data);
		}

		private class TestData
		{
			public int id { get; set; }
			public string name { get; set; }
		}
	}
}
