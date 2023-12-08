using System;
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

		[Test]
		public async Task null_response_returns_null_dynamic() {
			// a null IFlurlResponse is likely not even possible in real-world scenarios, but we have
			// null checks that need to be tested in order to meet our coverage threshold for this lib.
			Task<IFlurlResponse> resp = Task.FromResult<IFlurlResponse>(null);

			var json = await resp.ReceiveJson();
			Assert.IsNull(json);

			var list = await resp.ReceiveJsonList();
			Assert.IsNull(list);
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task can_get_error_json_untyped(bool useShortcut) {
			HttpTest.RespondWithJson(new { code = 999, message = "our server crashed" }, 500);

			try {
				await "http://api.com".GetStringAsync();
			}
			catch (FlurlHttpException ex) {
				var error = useShortcut ? // error is a dynamic this time
					await ex.GetResponseJsonAsync() :
					await ex.Call.Response.GetJsonAsync();
				Assert.IsNotNull(error);
				Assert.AreEqual(999, error.code);
				Assert.AreEqual("our server crashed", error.message);
			}
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
