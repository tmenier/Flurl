using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Flurl.Http;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture, Parallelizable]
	public class PostTests : HttpMethodTests
	{
		public PostTests() : base(HttpMethod.Post) { }

		protected override Task<IFlurlResponse> CallOnString(string url) => url.PostAsync(null);
		protected override Task<IFlurlResponse> CallOnUrl(Url url) => url.PostAsync(null);
		protected override Task<IFlurlResponse> CallOnFlurlRequest(IFlurlRequest req) => req.PostAsync(null);

		[Test]
		public async Task can_post_string() {
			var expectedEndpoint = "http://some-api.com";
			var expectedBody = "abc123";
			await expectedEndpoint.PostStringAsync(expectedBody);
			HttpTest.ShouldHaveCalled(expectedEndpoint)
				.WithVerb(HttpMethod.Post)
				.WithRequestBody(expectedBody)
				.Times(1);
		}

		[Test]
		public async Task can_post_object_as_json() {
			var body = new {a = 1, b = 2};
			await "http://some-api.com".PostJsonAsync(body);
			HttpTest.ShouldHaveCalled("http://some-api.com")
				.WithVerb(HttpMethod.Post)
				.WithContentType("application/json")
				.WithRequestBody("{\"a\":1,\"b\":2}")
				.Times(1);
		}

		[Test]
		public async Task can_post_url_encoded() {
			var body = new { a = 1, b = 2, c = "hi there", d = new[] { 1, 2, 3 } };
			await "http://some-api.com".PostUrlEncodedAsync(body);
			HttpTest.ShouldHaveCalled("http://some-api.com")
				.WithVerb(HttpMethod.Post)
				.WithContentType("application/x-www-form-urlencoded")
				.WithRequestBody("a=1&b=2&c=hi+there&d=1&d=2&d=3")
				.Times(1);
		}

		[Test]
		public async Task can_post_nothing() {
			await "http://some-api.com".PostAsync();
			HttpTest.ShouldHaveCalled("http://some-api.com")
				.WithVerb(HttpMethod.Post)
				.WithRequestBody("")
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
		public async Task can_receive_string() {
			HttpTest.RespondWith("good job");

			var data = await "http://some-api.com".PostJsonAsync(new { a = 1, b = 2 }).ReceiveString();

			Assert.AreEqual("good job", data);
		}

		[Test] // #740
		public async Task doesnt_add_space_in_content_type_header() {
			var req = new FlurlRequest("https://fake.com");
			await req.WithHeader("Content-Type", "application/octet-stream;some=b").PostStringAsync("hello");

			Assert.IsTrue(req.Headers.TryGetFirst("Content-Type", out var val));
			Assert.AreEqual("application/octet-stream;some=b", val);
		}

		private class TestData
		{
			public int id { get; set; }
			public string name { get; set; }
		}
	}
}
