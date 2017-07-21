using System;
using PackageTester.PCL;

namespace PackageTester.NET45
{
	public class Program
	{
		public static void Main(string[] args) {
			new Tester().DoTestsAsync().Wait();
			Console.WriteLine();
			Console.WriteLine("Testing against PCL...");
			new PclTester().DoTestsAsync().Wait();
			Console.ReadLine();
		}
	}
}