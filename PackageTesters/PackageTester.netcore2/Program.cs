using System;

namespace PackageTester
{
	public class Program
	{
		public static void Main(string[] args) {
			new Tester().DoTestsAsync().Wait();
			Console.ReadLine();
		}
	}
}