using System;
using System.Threading.Tasks;
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

			var path = await "http://www.google.com".DownloadFileAsync("c:\\", "google.txt");
			log("dowloaded google source to " + path);
			log("done");
		}
	}
}