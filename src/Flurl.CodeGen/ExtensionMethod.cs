using System;
using System.Collections.Generic;
using System.Linq;

namespace Flurl.CodeGen
{
	public class MethodArg
	{
		public string Name { get; set; }
		public string Type { get; set; }
		public string Description { get; set; }
		public string Default { get; set; }
		public bool IsOut { get; set; }

		public override string ToString() {
			var result = IsOut ? "out " : "";
			result += $"{Type} {Name}";
			if (Default != null) result += $" = {Default}";
			return result;
		}
	}

	public class ExtensionMethod
	{
		public virtual string Name { get; }
		public virtual string Description { get; }
		public bool IsGeneric { get; set; }
		public MethodArg ExtendedTypeArg { get; set; }
		public IList<MethodArg> Args { get; } = new List<MethodArg>();
		public string ReturnType { get; set; }
		public string ReturnDescrip { get; set; }

		protected ExtensionMethod() { }

		public ExtensionMethod(string name, string description) {
			Name = name;
			Description = description;
		}

		public ExtensionMethod AddArg(string name, string type, string description, string defaultVal = null, bool isOut = false) {
			Args.Add(new MethodArg { Name = name, Type = type, Description = description, Default = defaultVal, IsOut = isOut });
			return this;
		}

		public ExtensionMethod Extends(MethodArg arg) {
			ExtendedTypeArg = arg;
			return this;
		}

		public ExtensionMethod Returns(string type, string descrip) {
			ReturnType = type;
			ReturnDescrip = descrip;
			return this;
		}

		public void Write(CodeWriter writer, Action writeBody) {
			Console.WriteLine($"writing {Name} for {ExtendedTypeArg.Type}...");

			writer.WriteLine("/// <summary>");
			writer.WriteLine($"/// {Description}");
			writer.WriteLine("/// </summary>");
			writer.WriteLine($"/// <param name=\"{ExtendedTypeArg.Name}\">{ExtendedTypeArg.Description}</param>");
			foreach (var arg in Args)
				writer.WriteLine($"/// <param name=\"{arg.Name}\">{arg.Description}</param>");
			writer.WriteLine($"/// <returns>{ReturnDescrip}</returns>");

			var argList = new List<string> { $"this {ExtendedTypeArg.Type} {ExtendedTypeArg.Name}" };
			argList.AddRange(Args.Select(p => p.ToString()));
			var genericArg = IsGeneric ? "<T>" : "";
			writer.WriteLine($"public static {ReturnType} {Name}{genericArg}({string.Join(", ", argList)}) {{");
			writeBody();
			writer.WriteLine("}").WriteLine("");
		}

		public void Write(CodeWriter writer, string forwardCallToObject) {
			Write(writer, () => {
				var genericArg = IsGeneric ? "<T>" : "";
				var argList = string.Join(", ", Args.Select(p => p.IsOut ? $"out {p.Name}" : p.Name));
				writer.WriteLine($"return {forwardCallToObject}.{Name}{genericArg}({argList});");
			});
		}
	}
}
