using System;
using System.IO;
using System.Linq;
using System.Text;
using Newt.Models;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
#pragma warning disable CS8604 // Possible null reference argument.

namespace Newt.Writers
{
    internal class BaseWriter
    {
        internal readonly DBSchema Schema;
        internal readonly string TopFolder;
        internal readonly string Namespace;

        private readonly bool _force;
        private string _folder;
        private string _filename;
        private StringBuilder _writer;

        protected BaseWriter(DBSchema schema, bool force, string topFolder, string @namespace)
        {
            Schema = schema;
            TopFolder = topFolder;
            Namespace = @namespace;

            _force = force;
            _folder = topFolder;
            _filename = string.Empty;
        }

        protected void EnsureFolder(string title, params string[] subFolders)
        {
            _folder = TopFolder;
            if (subFolders.Any())
                foreach (var subFolder in subFolders) _folder = Path.Combine(_folder, subFolder);

            Console.WriteLine();
            Console.WriteLine(title);
            if (subFolders.Any()) Console.WriteLine(_folder);
            Directory.CreateDirectory(_folder);
        }

        protected StringBuilder StartFile(string name)
        {
            _filename = Path.Combine(_folder, name);
            Console.WriteLine(_filename);
            if (_force == false && File.Exists(_filename)) throw new Exception($"File exists: {_filename}");
            _writer = new StringBuilder();
            return _writer;
        }

        protected void FinishFile()
        {
            File.WriteAllText(_filename, _writer.ToString());
        }
    }
}

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
#pragma warning restore CS8604 // Possible null reference argument.
