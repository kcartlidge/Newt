#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
#pragma warning disable CS8604 // Possible null reference argument.

using System;
using System.IO;
using System.Linq;
using System.Text;
using Newt.Models;

namespace Newt.Writers
{
    /// <summary>Base class for all the output writers.</summary>
    internal class BaseWriter
    {
        internal readonly DBSchema Schema;
        internal readonly string TopFolder;
        internal readonly string Namespace;

        private readonly bool _force;
        private string _folder;
        private string _filename;
        private StringBuilder _writer;

        /// <summary>Creates a new base writer instance.</summary>
        /// <param name="schema">Database schema model.</param>
        /// <param name="force">Overwrite existing?</param>
        /// <param name="topFolder">Where to write to.</param>
        /// <param name="namespace">Namespace to use for paths and .Net stuff.</param>
        protected BaseWriter(DBSchema schema, bool force, string topFolder, string @namespace)
        {
            Schema = schema;
            TopFolder = topFolder;
            Namespace = @namespace;

            _force = force;
            _folder = topFolder;
            _filename = string.Empty;
        }

        /// <summary>
        /// Combines the subfolders to make a single path and creates it
        /// if it does not already exist.
        /// </summary>
        /// <param name="title">Display name for the path.</param>
        /// <param name="subFolders">Sequential segments of the path.</param>
        protected void EnsureFullPathExists(string title, params string[] subFolders)
        {
            _folder = TopFolder;
            if (subFolders.Any())
                foreach (var subFolder in subFolders) _folder = Path.Combine(_folder, subFolder);

            Console.WriteLine();
            Console.WriteLine(title);
            if (subFolders.Any()) Console.WriteLine(_folder);
            Directory.CreateDirectory(_folder);
        }

        /// <summary>
        /// Starts a new StringBuilder for capturing contents in anticipation
        /// of writing them out. If the file exists, and the option to 'force'
        /// was not provided at construction, an exception is raised.
        /// </summary>
        protected StringBuilder StartFile(string name)
        {
            _filename = Path.Combine(_folder, name);
            Console.WriteLine(_filename);
            if (_force == false && File.Exists(_filename)) throw new Exception($"File exists: {_filename}");
            _writer = new StringBuilder();
            return _writer;
        }

        /// <summary>Writes the gathered content to the file.</summary>
        protected void FinishFile()
        {
            File.WriteAllText(_filename, _writer.ToString(), Encoding.ASCII);
        }
    }
}
