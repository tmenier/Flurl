using System;
using System.IO;

namespace PackageTester
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Cleanup();
            new NetCoreTester().DoTestsAsync(Console.WriteLine).Wait();
            Cleanup();
            Console.ReadLine();
        }

        private static void Cleanup()
        {
            var file = "c:\\google.txt";
            if (File.Exists(file))
                File.Delete(file);
        }
    }
}