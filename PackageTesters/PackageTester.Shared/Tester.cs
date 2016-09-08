using System;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Flurl.Http.Testing;

namespace PackageTester
{
	public abstract class Tester
	{
		public async Task DoTestsAsync(Action<string> log) {
			var source = await "http://www.google.com".GetStringAsync();
			log(source.Substring(0, 40));
			log("^-- real response");
			using (var test = new HttpTest()) {
				test.RespondWith("totally fake google source");
				log(await "http://www.google.com".GetStringAsync());
				log("^-- fake response");
			}

			// Reproduce https://github.com/tmenier/Flurl/issues/128
			using (var test = new HttpTest()) {
				test.RespondWithJson(new TestResponse { TestString = "Test string" });

				var response = new Url("http://www.google.com")
				   .WithBasicAuth("test_username", "test_secret")
				   .PostUrlEncodedAsync(new { test = "" })
				   .ReceiveJson<TestResponse>()
				   .Result;

				log(response.TestString);
				log("^-- fake response https://github.com/tmenier/Flurl/issues/128");
			}

			var path = await "http://www.google.com".DownloadFileAsync("c:\\", "google.txt");
			log("dowloaded google source to " + path);
			log("done");
		}
	}

	internal class TestResponse
	{
		public string TestString { get; set; }
	}
}