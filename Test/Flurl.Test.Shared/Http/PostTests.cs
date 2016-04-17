using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture]
	public class PostTests : HttpTestFixtureBase
	{
		[Test]
		public async Task can_post_json() {
			await "http://some-api.com".PostJsonAsync(new { a = 1, b = 2 });
			HttpTest.ShouldHaveCalled("http://some-api.com")
				.WithVerb(HttpMethod.Post)
				.WithContentType("application/json")
				.WithRequestBody("{\"a\":1,\"b\":2}")
				.Times(1);
		}

	    [Test]
	    public async Task can_post_object_as_json(){
	        var expectedEndpoint = "http://some-api.com";
	        var expectedBody = new {a = 1, b = 2};
	        await expectedEndpoint.PostJsonAsync(expectedBody);
            HttpTest.ShouldHaveCalled(expectedEndpoint)
                .WithVerb(HttpMethod.Post)
                .WithContentType("application/json")
                .WithRequestBodyJson(expectedBody)
                .Times(1);
	    }

		[Test]
		public async Task can_post_url_encoded() {
			await "http://some-api.com".PostUrlEncodedAsync(new { a = 1, b = 2, c = "hi there" });
			HttpTest.ShouldHaveCalled("http://some-api.com")
				.WithVerb(HttpMethod.Post)
				.WithContentType("application/x-www-form-urlencoded")
				.WithRequestBody("a=1&b=2&c=hi+there")
				.Times(1);
		}

		[Test]
		public async Task can_receive_json() {
			HttpTest.RespondWithJson(new TestData { id = 1, name = "Frank" });

			var data = await "http://some-api.com".PostJsonAsync(new { a = 1, b = 2 }).ReceiveJson<TestData>();

			Assert.AreEqual(1, data.id);
			Assert.AreEqual("Frank", data.name);
		}

		[Test]
		public async Task can_receive_json_dynamic() {
			HttpTest.RespondWithJson(new { id = 1, name = "Frank" });

			var data = await "http://some-api.com".PostJsonAsync(new { a = 1, b = 2 }).ReceiveJson();

			Assert.AreEqual(1, data.id);
			Assert.AreEqual("Frank", data.name);				
		}

		[Test]
		public async Task can_receive_json_dynamic_list() {
			HttpTest.RespondWithJson(new[] {
				new { id = 1, name = "Frank" },
				new { id = 2, name = "Claire" }
			});

			var data = await "http://some-api.com".PostJsonAsync(new { a = 1, b = 2 }).ReceiveJsonList();

			Assert.AreEqual(1, data[0].id);
			Assert.AreEqual("Frank", data[0].name);
			Assert.AreEqual(2, data[1].id);
			Assert.AreEqual("Claire", data[1].name);
		}

		[Test]
		public async Task can_receive_string() {
			HttpTest.RespondWith("good job");

			var data = await "http://some-api.com".PostJsonAsync(new { a = 1, b = 2 }).ReceiveString();

			Assert.AreEqual("good job", data);
		}

		private class TestData
		{
			public int id { get; set; }
			public string name { get; set; }
		}
	}
}
