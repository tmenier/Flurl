using System;
using System.IO;
using System.Threading.Tasks;
using PackageTester.PCL;

namespace PackageTester.NET461
{
    class Program
    {
        static void Main(string[] args)
        {
            TestAllPlatformsAsync().Wait();
            Console.ReadLine();
        }

        private static async Task TestAllPlatformsAsync()
        {
            Cleanup();
            await new Net461Tester().DoTestsAsync(Console.WriteLine);
            Cleanup();
            await new PclTester().DoTestsAsync(Console.WriteLine);
        }

        private static void Cleanup()
        {
            var file = "c:\\flurl\\google.txt";
            if (File.Exists(file))
                File.Delete(file);
        }
    }

}