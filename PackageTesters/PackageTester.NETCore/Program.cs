using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PackageTester
{
    public class Program
    {
		static void Main(string[] args) {
			Cleanup();
			new NetCoreTester().DoTestsAsync(Console.WriteLine).Wait();
			Cleanup();
			Console.ReadLine();
		}

		private static void Cleanup() {
			var file = "c:\\flurl\\google.txt";
			if (File.Exists(file)) File.Delete(file);
		}
	}
}
