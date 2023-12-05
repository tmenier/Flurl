using NUnit.Framework;
using Flurl.Http;
using Flurl.Http.Newtonsoft;
using Flurl.Http.Testing;
using System.Threading.Tasks;

namespace Flurl.Test.Http
{
	// These inherit from GetTests and PostTests and swap out the JSON serializer
	// in play, which gets us a lot of free tests but also a lot of redundant ones.
	// Maybe worth refactoring someday, but they're fast so it's tolerable for now.

	[TestFixture, Parallelizable]
	public class NewtonsoftGetTests : GetTests
	{
		protected override HttpTest CreateHttpTest() => base.CreateHttpTest()
			.WithSettings(settings => settings.JsonSerializer = new NewtonsoftJsonSerializer());

		[Test]
		public async Task can_get_dynamic() {
			HttpTest.RespondWithJson(new { id = 1, name = "Frank" });

			var data = await "http://some-api.com".GetJsonAsync();

			Assert.AreEqual(1, data.id);
			Assert.AreEqual("Frank", data.name);
		}

		[Test]
		public async Task can_get_dynamic_list() {
			HttpTest.RespondWithJson(new[] {
				new { id = 1, name = "Frank" },
				new { id = 2, name = "Claire" }
			});

			var data = await "http://some-api.com".GetJsonListAsync();

			Assert.AreEqual(1, data[0].id);
			Assert.AreEqual("Frank", data[0].name);
			Assert.AreEqual(2, data[1].id);
			Assert.AreEqual("Claire", data[1].name);
		}
	}

	[TestFixture, Parallelizable]
	public class NewtonsofPostTests : PostTests
	{
		protected override HttpTest CreateHttpTest() => base.CreateHttpTest()
			.WithSettings(settings => settings.JsonSerializer = new NewtonsoftJsonSerializer());

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
	}
}
