using System;
using System.IO;
using System.Threading.Tasks;

namespace PackageTester.NET461
{
    public class Program
    {
        public static void Main(string[] args)
        {
            TestAllPlatformsAsync().Wait();
            Console.ReadLine();
        }

        private static async Task TestAllPlatformsAsync()
        {
            Cleanup();
            await new Net461Tester().DoTestsAsync(Console.WriteLine);
            Cleanup();
        }

        private static void Cleanup()
        {
            var file = "c:\\google.txt";
            if (File.Exists(file))
                File.Delete(file);
        }
    }
}