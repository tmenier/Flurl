using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Flurl.Http;
using NUnit.Framework;

namespace Flurl.Test.Http
{
	[TestFixture, Parallelizable]
	public class GetTests : HttpMethodTests
	{
		public GetTests() : base(HttpMethod.Get) { }

		protected override Task<HttpResponseMessage> CallOnStringAsync(string url) => url.GetAsync();
		protected override Task<HttpResponseMessage> CallOnUrlAsync(Url url) => url.GetAsync();
		protected override Task<HttpResponseMessage> CallOnFlurlRequestAsync(IFlurlRequest req) => req.GetAsync();
		protected override HttpResponseMessage CallOnString(string url) => url.Get();
		protected override HttpResponseMessage CallOnUrl(Url url) => url.Get();
		protected override HttpResponseMessage CallOnFlurlRequest(IFlurlRequest req) => req.Get();

		[Test]
		public async Task can_get_json() {
			HttpTest.RespondWithJson(new TestData { id = 1, name = "Frank" });

			var data = await "http://some-api.com".GetJsonAsync<TestData>();

			Assert.AreEqual(1, data.id);
			Assert.AreEqual("Frank", data.name);
		}

		[Test]
		public async Task can_get_json_dynamic() {
			HttpTest.RespondWithJson(new { id = 1, name = "Frank" });

			var data = await "http://some-api.com".GetJsonAsync();

			Assert.AreEqual(1, data.id);
			Assert.AreEqual("Frank", data.name);
		}

		[Test]
		public async Task can_get_json_dynamic_list() {
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
		public async Task can_get_string() {
			HttpTest.RespondWith("good job");

			var data = await "http://some-api.com".GetStringAsync();

			Assert.AreEqual("good job", data);
		}

		[Test]
		public async Task can_get_stream() {
			HttpTest.RespondWith("good job");

			var data = await "http://some-api.com".GetStreamAsync();

			Assert.AreEqual(new MemoryStream(Encoding.UTF8.GetBytes("good job")), data);
		}

		[Test]
		public async Task can_get_bytes() {
			HttpTest.RespondWith("good job");

			var data = await "http://some-api.com".GetBytesAsync();

			Assert.AreEqual(Encoding.UTF8.GetBytes("good job"), data);
		}

		[Test]
		public async Task failure_throws_detailed_exception() {
			HttpTest.RespondWith("bad job", status: 500);

			try {
				await "http://api.com".GetStringAsync();
				Assert.Fail("FlurlHttpException was not thrown!");
			}
			catch (FlurlHttpException ex) {
				Assert.AreEqual("http://api.com/", ex.Call.Request.RequestUri.AbsoluteUri);
				Assert.AreEqual(HttpMethod.Get, ex.Call.Request.Method);
				Assert.AreEqual(HttpStatusCode.InternalServerError, ex.Call.Response.StatusCode);
				Assert.AreEqual("bad job", await ex.GetResponseStringAsync());
			}
		}

		[Test]
		public async Task can_get_error_json_typed() {
			HttpTest.RespondWithJson(new { code = 999, message = "our server crashed" }, 500);

			try {
				await "http://api.com".GetStringAsync();
			}
			catch (FlurlHttpException ex) {
				var error = await ex.GetResponseJsonAsync<TestError>();
				Assert.IsNotNull(error);
				Assert.AreEqual(999, error.code);
				Assert.AreEqual("our server crashed", error.message);
			}
		}

		[Test]
		public async Task can_get_error_json_untyped() {
			HttpTest.RespondWithJson(new { code = 999, message = "our server crashed" }, 500);

			try {
				await "http://api.com".GetStringAsync();
			}
			catch (FlurlHttpException ex) {
				var error = await ex.GetResponseJsonAsync(); // error is a dynamic this time
				Assert.IsNotNull(error);
				Assert.AreEqual(999, error.code);
				Assert.AreEqual("our server crashed", error.message);
			}
		}

        [Test]
        public async Task can_get_null_json_when_timeout_and_exception_handled() {
            HttpTest.SimulateTimeout();
            var data = await "http://api.com"
                .ConfigureRequest(c => c.OnError = call => call.ExceptionHandled = true)
                .GetJsonAsync<TestData>();
            Assert.IsNull(data);
        }

		// https://github.com/tmenier/Flurl/pull/76
		// quotes around charset value is technically legal but there's a bug in .NET we want to avoid: https://github.com/dotnet/corefx/issues/5014
		[Test]
		public async Task can_get_string_with_quoted_charset_header() {
			var content = new StringContent("foo");
			content.Headers.Clear();
			content.Headers.Add("Content-Type", "text/javascript; charset=\"UTF-8\"");
			HttpTest.RespondWith(content);

			var resp = await "http://api.com".GetStringAsync(); // without StripCharsetQuotes, this fails
			Assert.AreEqual("foo", resp);
		}

		[Test] // #313
		public async Task can_setting_content_header_with_no_content() {
			await "http://api.com"
				.WithHeader("Content-Type", "application/json")
				.GetAsync();

			HttpTest.ShouldHaveMadeACall().WithContentType("application/json");
		}

		private class TestData
		{
			public int id { get; set; }
			public string name { get; set; }
		}

		private class TestError
		{
			public int code { get; set; }
			public string message { get; set; }
		}
	}
}
