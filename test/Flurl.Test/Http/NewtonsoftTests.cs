using System;
using System.Collections.Generic;
using System.Net.Http;
using NUnit.Framework;
using Flurl.Http;
using Flurl.Http.Newtonsoft;
using Flurl.Http.Testing;
using System.Threading.Tasks;
using Flurl.Http.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Flurl.Test.Http
{
	[TestFixture]
	public class NewtonsoftTests
	{
		private readonly JsonSerializerSettings _jsonSettings = new JsonSerializerSettings {
			DateFormatString = "M/d/yy"
		};

		// JObject is from Newtsonsoft; STJ doesn't know how to serialize correctly
		private readonly object _requestObject = new JObject {
			["test"] = "test",
			["number"] = 1,
			["date"] = new DateTime(2024, 1, 2)
		};
		private readonly string _requestJson = "{\"test\":\"test\",\"number\":1,\"date\":\"1/2/24\"}";

		private class ResponseModel
		{
			// JsonProperty from Newtonsoft; STJ will ignore
			[JsonProperty("newtonsoft_prop")]
			public string SomeProp { get; set; }
		}
		private readonly ResponseModel _responseObject = new ResponseModel { SomeProp = "xyz" };
		private readonly string _responseJson = "{\"newtonsoft_prop\":\"xyz\"}";

		[Test]
		public void test_data_incompatible_with_default_serializer() {
			// confirms our test data works only with Newtonsoft

			var stj = new DefaultJsonSerializer();
			Assert.AreNotEqual(_requestJson, stj.Serialize(_requestObject));
			Assert.AreNotEqual(_responseObject.SomeProp, stj.Deserialize<ResponseModel>(_responseJson).SomeProp);

			var njs = new NewtonsoftJsonSerializer(_jsonSettings);
			Assert.AreEqual(_requestJson, njs.Serialize(_requestObject));
			Assert.AreEqual(_responseObject.SomeProp, njs.Deserialize<ResponseModel>(_responseJson).SomeProp);
		}

		private async Task AssertCallAsync(Func<Task<ResponseModel>> call, bool hasRequestBody) {
			using var test = new HttpTest();
			test.RespondWith(_responseJson);

			var resp = await call();

			Assert.AreEqual(_responseObject.SomeProp, resp.SomeProp);
			if (hasRequestBody)
				test.ShouldHaveMadeACall().WithRequestBody(_requestJson);
		}

		[Test, NonParallelizable]
		public async Task works_with_clientless() {
			try {
				FlurlHttp.Clients.Clear();
				FlurlHttp.Clients.UseNewtonsoft(_jsonSettings);

				Assert.IsInstanceOf<NewtonsoftJsonSerializer>(FlurlHttp.GetClientForRequest(new FlurlRequest("http://api.com")).Settings.JsonSerializer);

				await AssertCallAsync(() => "http://api.com".GetJsonAsync<ResponseModel>(), false);
				await AssertCallAsync(() => "http://api.com".PostJsonAsync(_requestObject).ReceiveJson<ResponseModel>(), true);
				await AssertCallAsync(() => "http://api.com".PutJsonAsync(_requestObject).ReceiveJson<ResponseModel>(), true);
				await AssertCallAsync(() => "http://api.com".PatchJsonAsync(_requestObject).ReceiveJson<ResponseModel>(), true);
			}
			finally {
				FlurlHttp.Clients.Clear();
				FlurlHttp.Clients.WithDefaults(b => b.Settings.ResetDefaults());
			}
		}

		[Test]
		public async Task works_with_cached_clients() {
			var cache = new FlurlClientCache().UseNewtonsoft(_jsonSettings);

			await AssertCallAsync(() => cache.GetOrAdd("c1").Request("http://api1.com").GetJsonAsync<ResponseModel>(), false);
			await AssertCallAsync(() => cache.GetOrAdd("c2").Request("http://api2.com").PostJsonAsync(_requestObject).ReceiveJson<ResponseModel>(), true);
			await AssertCallAsync(() => cache.GetOrAdd("c3").Request("http://api3.com").PutJsonAsync(_requestObject).ReceiveJson<ResponseModel>(), true);
			await AssertCallAsync(() => cache.GetOrAdd("c4").Request("http://api4.com").PatchJsonAsync(_requestObject).ReceiveJson<ResponseModel>(), true);
		}

		[Test]
		public async Task works_with_individual_cached_client() {
			var cache = new FlurlClientCache();
			var cli = cache.GetOrAdd("foo", null, builder => builder.UseNewtonsoft(_jsonSettings));

			await AssertCallAsync(() => cli.Request("http://api.com").GetJsonAsync<ResponseModel>(), false);
			await AssertCallAsync(() => cli.Request("http://api.com").PostJsonAsync(_requestObject).ReceiveJson<ResponseModel>(), true);
			await AssertCallAsync(() => cli.Request("http://api.com").PutJsonAsync(_requestObject).ReceiveJson<ResponseModel>(), true);
			await AssertCallAsync(() => cli.Request("http://api.com").PatchJsonAsync(_requestObject).ReceiveJson<ResponseModel>(), true);
		}

		[Test]
		public async Task works_with_individual_client() {
			var cli = new FlurlClient().WithSettings(s => s.JsonSerializer = new NewtonsoftJsonSerializer(_jsonSettings));

			await AssertCallAsync(() => cli.Request("http://api.com").GetJsonAsync<ResponseModel>(), false);
			await AssertCallAsync(() => cli.Request("http://api.com").PostJsonAsync(_requestObject).ReceiveJson<ResponseModel>(), true);
			await AssertCallAsync(() => cli.Request("http://api.com").PutJsonAsync(_requestObject).ReceiveJson<ResponseModel>(), true);
			await AssertCallAsync(() => cli.Request("http://api.com").PatchJsonAsync(_requestObject).ReceiveJson<ResponseModel>(), true);
		}

		[Test]
		public async Task can_get_dynamic() {
			using var test = new HttpTest();
			test.RespondWithJson(new { id = 1, name = "Frank" });

			var cli = new FlurlClient().WithSettings(s => s.JsonSerializer = new NewtonsoftJsonSerializer());

			AssertResponse(await cli.Request("https://api.com").GetJsonAsync());
			AssertResponse(await cli.Request("https://api.com").PostJsonAsync(new { a = 1, b = 2 }).ReceiveJson());

			void AssertResponse(dynamic resp) {
				Assert.AreEqual(1, resp.id);
				Assert.AreEqual("Frank", resp.name);
			}
		}

		[Test]
		public async Task can_get_dynamic_list() {
			using var test = new HttpTest();
			test.RespondWithJson(new[] {
				new { id = 1, name = "Frank" },
				new { id = 2, name = "Claire" }
			});

			var cli = new FlurlClient().WithSettings(s => s.JsonSerializer = new NewtonsoftJsonSerializer());

			AssertResponse(await cli.Request("https://api.com").GetJsonListAsync());
			AssertResponse(await cli.Request("https://api.com").PatchJsonAsync(new { a = 1, b = 2 }).ReceiveJsonList());

			void AssertResponse(IList<dynamic> resp) {
				Assert.AreEqual(1, resp[0].id);
				Assert.AreEqual("Frank", resp[0].name);
				Assert.AreEqual(2, resp[1].id);
				Assert.AreEqual("Claire", resp[1].name);
			}
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

			var ex = new FlurlHttpException(new FlurlCall {
				Request = new FlurlRequest(),
				HttpRequestMessage = new HttpRequestMessage(),
				Response = null,
			});
			var err = await ex.GetResponseJsonAsync();
			Assert.IsNull(err);
		}

		[TestCase(false)]
		[TestCase(true)]
		public async Task can_get_error_json_untyped(bool useShortcut) {
			using var test = new HttpTest();
			test.RespondWithJson(new { code = 999, message = "our server crashed" }, 500);

			var cli = new FlurlClient().WithSettings(s => s.JsonSerializer = new NewtonsoftJsonSerializer());

			try {
				await cli.Request("http://api.com").GetStringAsync();
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
}
