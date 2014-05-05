using System;
using Flurl.Http;
using Flurl.Http.Testing;

namespace PackageTester
{
	class Program
	{
		static void Main(string[] args) {
			Console.WriteLine("http://www.google.com".GetStringAsync().Result);
			Console.WriteLine("^-- real response");
			using (var test = new HttpTest()) {
				test.RespondWith("totally fake google source");
				Console.WriteLine("http://www.google.com".GetStringAsync().Result);
				Console.WriteLine("^-- fake response");
			}
			var path = "http://www.google.com".DownloadFileAsync("c:\\flurl", "google.txt").Result;
			Console.WriteLine("dowloaded google source to " + path);
			Console.ReadLine();
		}
	}
}
