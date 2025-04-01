using System;
using System.IO;
using System.Text;

namespace Flurl.CodeGen
{
    /// <summary>
    /// Wraps a StreamWriter. Mainly just keeps track of indentation.
    /// </summary>
    public class CodeWriter : IDisposable
    {
        private readonly StreamWriter _sw;
        private int _indent;
        private bool _wrapping;
        private readonly StringBuilder _sb = new StringBuilder();

        public CodeWriter(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            _sw = new StreamWriter(File.OpenWrite(filePath));
        }

        /// <summary>
        /// Writes a line with optional parameters. Use @0, @1, @2, etc for tokens.
        /// </summary>
        public CodeWriter WriteLine(string line, params object[] args)
        {
            if (line == null)
                throw new ArgumentNullException(nameof(line));

            line = line.Trim();

            if (args?.Length > 0)
            {
                _sb.Clear();
                _sb.Append(line);
                
                for (int i = 0; i < args.Length; i++)
                {
                    var val = args[i]?.ToString() ?? string.Empty;
                    _sb.Replace($"@{i}", val);
                }
                
                line = _sb.ToString();
            }

            if (line == "}" || line == "{")
            {
                _indent--;
            }

            _sw.Write(new string('\t', _indent));
            _sw.WriteLine(line);

            UpdateIndentation(line);

            return this;
        }

        private void UpdateIndentation(string line)
        {
            if (string.IsNullOrEmpty(line) || line.StartsWith("//") || line.EndsWith("]"))
            {
                _wrapping = false;
                return;
            }

            if (line.EndsWith(";") || line.EndsWith("}"))
            {
                if (_wrapping)
                    _indent--;
                _wrapping = false;
                return;
            }

            if (line.EndsWith("{"))
            {
                _indent++;
                _wrapping = false;
                return;
            }

            if (!_wrapping)
                _indent++;
            _wrapping = true;
        }

        public CodeWriter WriteLine()
        {
            _sw.WriteLine();
            return this;
        }

        public void Dispose()
        {
            _sw?.Dispose();
            _sb?.Clear();
        }
    }
}