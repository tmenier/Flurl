using System;
using System.Collections.Generic;
using NUnit.Framework;
using Flurl.Http;
using Flurl.Http.Newtonsoft;
using Flurl.Http.Testing;
using System.Threading.Tasks;
using Flurl.Http.Configuration;
using Newtonsoft.Json;

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

		[TestCaseSource(nameof(GetJson))]
		public async Task can_get_dynamic(Task<dynamic> getJson) {
			HttpTest.RespondWithJson(new { id = 1, name = "Frank" });

			var data = await getJson;

			Assert.AreEqual(1, data.id);
			Assert.AreEqual("Frank", data.name);
		}

		[TestCaseSource(nameof(GetJsonList))]
		public async Task can_get_dynamic_list(Task<IList<dynamic>> getList) {
			HttpTest.RespondWithJson(new[] {
				new { id = 1, name = "Frank" },
				new { id = 2, name = "Claire" }
			});

			var data = await getList;

			Assert.AreEqual(1, data[0].id);
			Assert.AreEqual("Frank", data[0].name);
			Assert.AreEqual(2, data[1].id);
			Assert.AreEqual("Claire", data[1].name);
		}

		private static IEnumerable<Task<dynamic>> GetJson() {
			yield return "http://some-api.com".GetJsonAsync();
			yield return new Url("http://some-api.com").GetJsonAsync();
			yield return new Uri("http://some-api.com").GetJsonAsync();
		}

		private static IEnumerable<Task<IList<dynamic>>> GetJsonList() {
			yield return "http://some-api.com".GetJsonListAsync();
			yield return new Url("http://some-api.com").GetJsonListAsync();
			yield return new Uri("http://some-api.com").GetJsonListAsync();
		}
	}

	[TestFixture, Parallelizable]
	public class NewtonsoftPostTests : PostTests
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

	[TestFixture, Parallelizable]
	public class NewtonsoftConfigTests
	{
		[Test]
		public void can_register_with_builder() {
			var cache = new FlurlClientCache();
			var cli = cache.GetOrAdd("foo", null, builder => builder.UseNewtonsoft(new JsonSerializerSettings { DateFormatString = "1234" }));

			Assert.IsInstanceOf<NewtonsoftJsonSerializer>(cli.Settings.JsonSerializer);

			var obj = new { Date = DateTime.Now };
			var json = cli.Settings.JsonSerializer.Serialize(obj);
			Assert.AreEqual("{\"Date\":\"1234\"}", json);
		}

		[Test]
		public void can_register_with_cache() {
			var cache = new FlurlClientCache().UseNewtonsoft(new JsonSerializerSettings { DateFormatString = "1234" });
			var cli = cache.GetOrAdd("foo");

			Assert.IsInstanceOf<NewtonsoftJsonSerializer>(cli.Settings.JsonSerializer);

			var obj = new { Date = DateTime.Now };
			var json = cli.Settings.JsonSerializer.Serialize(obj);
			Assert.AreEqual("{\"Date\":\"1234\"}", json);
		}

	}
}
