using System;
using Newt.Models;

namespace Newt.Writers
{
    /// <summary>Registers the required Nuget packages.</summary>
    internal class NugetPackages : BaseWriter
    {
        public NugetPackages(DBSchema db, bool force, string folder, string @namespace)
            : base(db, force, folder, @namespace)
        {
        }

        /// <summary>
        /// Register the required Nuget packages using the installed 'dotnet' command.
        /// </summary>
        public void Write()
        {
            Console.WriteLine();
            Console.WriteLine("NUGET PACKAGES");

            Console.WriteLine($"Adding nuget references");
            FileOps.RunCommand("dotnet", $"add {Namespace} package Npgsql", TopFolder);
            FileOps.RunCommand("dotnet", $"add {Namespace} package Microsoft.EntityFrameworkCore", TopFolder);
            FileOps.RunCommand("dotnet", $"add {Namespace} package Microsoft.EntityFrameworkCore.InMemory", TopFolder);
            FileOps.RunCommand("dotnet", $"add {Namespace} package Npgsql.EntityFrameworkCore.PostgreSQL", TopFolder);
        }
    }
}