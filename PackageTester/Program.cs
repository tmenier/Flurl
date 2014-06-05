using System;
using System.IO;
using PackageTester.PCL;

namespace PackageTester
{
	class Program
	{
		static void Main(string[] args) {
			var file = "c:\\flurl\\google.txt";
			if (File.Exists(file)) File.Delete(file);
			Tester.DoTestsAsync(Console.WriteLine).Wait();
			if (File.Exists(file)) File.Delete(file);
			Console.ReadLine();
		}
	}
}
