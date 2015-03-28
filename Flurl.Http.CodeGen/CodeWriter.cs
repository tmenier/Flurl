using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flurl.Http.CodeGen
{
	/// <summary>
	/// Wraps a StreamWriter. Mainly just keeps track of indentation.
	/// </summary>
	public class CodeWriter : IDisposable
	{
		private readonly StreamWriter _sw;
		private int _indent = 0;
		private bool _wrapping = false;

		public CodeWriter(string filePath) {
			_sw = new StreamWriter(filePath);
		}

		/// <summary>
		/// use @0, @1, @2, etc for tokens. ({0} would be a pain because you'd alway need to escape "{" and "}")
		/// </summary>
		public CodeWriter WriteLine(string line, params object[] args) {
			line = line.Trim();

			for (int i = 0; i < args.Length; i++) {
				var val = (args[i] == null) ? "" : args[i].ToString();
				line = line.Replace("@" + i, val);
			}

			if (line == "}" || line == "{") {
				_indent--;
			}

			_sw.Write(new String('\t', _indent));
			_sw.WriteLine(line);

			if (line == "" || line.StartsWith("//") || line.EndsWith("]")) {
				_wrapping = false;
			}
			else if (line.EndsWith(";") || line.EndsWith("}")) {
				if (_wrapping) _indent--;
				_wrapping = false;
			}
			else if (line.EndsWith("{")) {
				_indent++;
				_wrapping = false;
			}
			else {
				if (!_wrapping) _indent++;
				_wrapping = true;
			}

			return this; // fluent!
		}

		public CodeWriter WriteLine() {
			_sw.WriteLine();
			return this;
		}

		public void Dispose() {
			_sw.Dispose();
		}
	}
}
