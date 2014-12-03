using System;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using Flurl.Http.Testing;

namespace PackageTester.PCL
{
    public static class Tester
    {
		public static async Task DoTestsAsync(Action<string> log) {
		    string nullValue = null;
            var source = await "http://www.google.com".SetQueryParams(new { RandomKeyName = nullValue, AnotherRandomKeyName = "" }).GetStringAsync();
			log(source.Substring(0, 40));
			log("^-- real response");
			using (var test = new HttpTest()) {
				test.RespondWith("totally fake google source");
				log(await "http://www.google.com".GetStringAsync());
				log("^-- fake response");
			}

			var path = await "http://www.google.com".DownloadFileAsync("c:\\flurl", "google.txt");
			log("dowloaded google source to " + path);
			log("done");
		}
    }
}
