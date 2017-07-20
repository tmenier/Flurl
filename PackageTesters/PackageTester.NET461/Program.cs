using System;

namespace PackageTester.NET461
{
	public class Program
	{
		public static void Main(string[] args) {
			new Tester().DoTestsAsync().Wait();
			Console.ReadLine();
		}
	}
}