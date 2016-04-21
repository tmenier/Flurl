using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PackageTester.NET461
{
	class Program
	{
		static void Main(string[] args) {
			TestAllPlatformsAsync().Wait();
			Console.ReadLine();
		}

		private static async Task TestAllPlatformsAsync() {
			Cleanup();
			await new Net461Tester().DoTestsAsync(Console.WriteLine);
			Cleanup();
		}

		private static void Cleanup() {
			var file = "c:\\flurl\\google.txt";
			if (File.Exists(file)) File.Delete(file);
		}
	}

}
