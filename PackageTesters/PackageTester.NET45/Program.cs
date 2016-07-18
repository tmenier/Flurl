using System;
using System.IO;
using System.Threading.Tasks;
using PackageTester.PCL;

namespace PackageTester.NET45
{
	public class Program
	{
		public static void Main(string[] args) {
			TestAllPlatformsAsync().Wait();
			Console.ReadLine();
		}

		private static async Task TestAllPlatformsAsync() {
			Cleanup();
			await new Net45Tester().DoTestsAsync(Console.WriteLine);
			Cleanup();
			await new PclTester().DoTestsAsync(Console.WriteLine);
			Cleanup();
		}

		private static void Cleanup() {
			var file = "c:\\google.txt";
			if (File.Exists(file)) File.Delete(file);
		}
	}
}