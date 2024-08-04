using System;

namespace Newt.Writers
{
    /// <summary>Creates the .Net data project.</summary>
    internal static class DataProject
    {
        /// <summary>
        /// Create the .Net project using the installed 'dotnet' command.
        /// </summary>
        public static void Write(Config config)
        {
            Console.WriteLine();
            Console.WriteLine("DATA PROJECT");

            Console.WriteLine("Creating project");
            Support.RunCommand("dotnet", $"new classlib -n {config.DataNamespace}", config.SolutionFolder);
            
            Console.WriteLine($"Removing default class file");
            Support.RemoveFile(config.DataProjectFolder, "Class1.cs");
        }
    }
}