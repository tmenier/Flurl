using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flurl.Http.CodeGen
{
	class Program
	{
		static void Main(string[] args) {
			if (args.Length == 0)
				throw new ArgumentException("Must provide a path to the .cs output file.");

			var codePath = args[0];
			using (var writer = new CodeWriter(codePath)) {
				writer
					.WriteLine("using System.Net.Http;")
					.WriteLine("using System.Threading.Tasks;")
					.WriteLine("using Flurl.Http.Content;")
					.WriteLine("")
					.WriteLine("namespace Flurl.Http")
					.WriteLine("{")
					.WriteLine("public static class HttpExtensions")
					.WriteLine("{");

				writer.WriteLine("// extension methods here");

				writer
					.WriteLine("}")
					.WriteLine("}");
			}
		}
	}
}
