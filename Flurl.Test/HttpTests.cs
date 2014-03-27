using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using NUnit.Framework;
using Flurl.Http;

namespace Flurl.Test
{
	[TestFixture]
	public class HttpTests
	{
		[Test]
		// check that for every FlurlClient extension method, we have an equivalent Url and string extension
		public void extension_methods_consistently_supported() {
			var allExtMethods = ReflectionHelper.GetAllExtensionMethods(typeof(FlurlClient).Assembly);
			var fcExtensions = allExtMethods.Where(m => m.GetParameters()[0].ParameterType == typeof(FlurlClient)).ToArray();

			foreach (var method in fcExtensions) {
				foreach (var type in new[] { typeof(string), typeof(Url) }) {
					if (!allExtMethods.Except(fcExtensions).Any(m => ReflectionHelper.IsEquivalentExtensionMethod(method, m, type))) {
						Assert.Fail("No equivalent {0} extension method found for FlurlClient.{1}", type.Name, method.Name);
					}
				}
			}
		}

		[Test]
		public void can_set_timeout() {
			var client = "http://www.google.com".WithTimeout(15);
			Assert.AreEqual(client.HttpClient.Timeout, TimeSpan.FromSeconds(15));
		}

		[Test]
		public async Task can_download_file() {
			var path = await "http://www.google.com".DownloadAsync(@"c:\a\b", "google.txt");
			Assert.That(File.Exists(path));
			File.Delete(path);
			Directory.Delete(@"c:\a", true);
		}

		[Test]
		public async Task can_post_json() {
			FlurlHttp.TestMode = true;
			await "http://some-api.com".PostJsonAsync<object>(new { a = 1, b = 2 });

			Assert.AreEqual(HttpMethod.Post, FlurlHttp.Testing.LastRequest.Method);
			Assert.AreEqual("{\"a\":1,\"b\":2}", FlurlHttp.Testing.LastRequestBody);
		}

		[Test]
		public async Task can_get_json_dynamic() {
			FlurlHttp.TestMode = false;
			var result = await "http://echo.jsontest.com/key/value/one/two".GetJsonAsync();
			Assert.AreEqual("value", result.key);
			Assert.AreEqual("two", result.one);
		}

		[Test]
		public async Task can_get_json_strongly_typed() {
			FlurlHttp.TestMode = false;
			var result = await "http://echo.jsontest.com/key/value/one/two".GetJsonAsync<JsonTestData>();
			Assert.AreEqual("value", result.key);
			Assert.AreEqual("two", result.one);
		}

		class JsonTestData
		{
			public string key { get; set; }
			public string one { get; set; }
		}
	}
}
