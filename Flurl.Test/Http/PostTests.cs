using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Testing;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture]
	public class PostTests
	{
		[Test]
		public async Task can_post_json() {
			using (var test = new HttpTest()) {
				await "http://some-api.com".PostJsonAsync(new { a = 1, b = 2 });
				test.ShouldHaveCalled("http://some-api.com", times: 1, verb: HttpMethod.Post, contentType: "application/json", bodyPattern: "{\"a\":1,\"b\":2}");
			}
		}

		[Test]
		public async Task can_post_url_encoded() {
			using (var test = new HttpTest()) {
				await "http://some-api.com".PostUrlEncodedAsync(new { a = 1, b = 2, c = "hi there" });
				test.ShouldHaveCalled("http://some-api.com", times: 1, verb: HttpMethod.Post, contentType: "application/x-www-form-urlencoded", bodyPattern: "a=1&b=2&c=hi+there");
			}
		}

		[Test]
		public async Task can_receive_json() {
			using (var test = new HttpTest()) {
				test.RespondWithJson(new TestData { id = 1, name = "Frank" });
				
				var data = await "http://some-api.com".PostJsonAsync(new { a = 1, b = 2 }).ReceiveJsonAsync<TestData>();

				Assert.AreEqual(1, data.id);
				Assert.AreEqual("Frank", data.name);				
			}
		}

		[Test]
		public async Task can_receive_json_dynamic() {
			using (var test = new HttpTest()) {
				new HttpTest().RespondWithJson(new { id = 1, name = "Frank" });

				var data = await "http://some-api.com".PostJsonAsync(new { a = 1, b = 2 }).ReceiveJsonAsync();

				Assert.AreEqual(1, data.id);
				Assert.AreEqual("Frank", data.name);				
			}
		}

		[Test]
		public async Task can_receive_json_dynamic_list() {
			using (var test = new HttpTest()) {
				test.RespondWithJson(new[] {
					new { id = 1, name = "Frank" },
					new { id = 2, name = "Claire" }
				});

				var data = await "http://some-api.com".PostJsonAsync(new { a = 1, b = 2 }).ReceiveJsonListAsync();

				Assert.AreEqual(1, data[0].id);
				Assert.AreEqual("Frank", data[0].name);
				Assert.AreEqual(2, data[1].id);
				Assert.AreEqual("Claire", data[1].name);
			}
		}

		[Test]
		public async Task can_receive_string() {
			using (var test = new HttpTest()) {
				test.RespondWith("good job");

				var data = await "http://some-api.com".PostJsonAsync(new { a = 1, b = 2 }).ReceiveStringAsync();

				Assert.AreEqual("good job", data);
			}
		}

		private class TestData
		{
			public int id { get; set; }
			public string name { get; set; }
		}
	}
}
