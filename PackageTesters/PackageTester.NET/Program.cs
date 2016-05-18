using System;
using System.IO;

namespace PackageTester
{
	public class Program
	{
		static void Main(string[] args) {
			Cleanup();
			new NetTester().DoTestsAsync(Console.WriteLine).Wait();
			Cleanup();
			Console.ReadLine();
		}

		private static void Cleanup() {
			var file = "c:\\flurl\\google.txt";
			if (File.Exists(file)) File.Delete(file);
		}
	}
}
