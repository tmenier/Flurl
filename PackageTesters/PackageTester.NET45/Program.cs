using System;

namespace PackageTester.NET45
{
	public class Program
	{
		public static void Main(string[] args) {
			new Tester().DoTestsAsync().Wait();
			Console.ReadLine();
		}
	}
}