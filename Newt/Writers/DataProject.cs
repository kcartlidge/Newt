using System;
using System.IO;
using Newt.Models;

namespace Newt.Writers
{
    /// <summary>Creates the .Net project.</summary>
    internal class DataProject : BaseWriter
    {
        public DataProject(DBSchema db, bool force, string folder, string @namespace)
            : base(db, force, folder, @namespace)
        {
        }

        /// <summary>
        /// Create the .Net project using the installed 'dotnet' command.
        /// </summary>
        public void Write()
        {
            Console.WriteLine();
            Console.WriteLine("DATA PROJECT");

            Console.WriteLine("Creating project");
            FileOps.RunCommand("dotnet", $"new classlib -n {Namespace}", TopFolder);
            
            Console.WriteLine($"Removing default class file");
            FileOps.RemoveFile(Path.Combine(TopFolder, Namespace), "Class1.cs");
        }
    }
}