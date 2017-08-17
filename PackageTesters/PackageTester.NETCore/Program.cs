using System;
using Flurl.Http;

namespace PackageTester
{
	public class Program
	{
		public static void Main(string[] args) {
			var client = new FlurlClient().EnableCookies();
			client.WithUrl("https://httpbin.org/cookies/set?z=999").HeadAsync().Wait();
			Console.WriteLine("999" == client.Cookies["z"].Value);
			Console.ReadLine();

			//new Tester().DoTestsAsync().Wait();
			//Console.ReadLine();
		}
	}
}