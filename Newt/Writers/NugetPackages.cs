using System;

namespace Newt.Writers
{
    /// <summary>Registers the required Nuget packages.</summary>
    internal static class NugetPackages
    {
        /// <summary>
        /// Register the required Nuget packages using the installed 'dotnet' command.
        /// </summary>
        public static void Write(Config config)
        {
            Support.ShowHeading("NUGET PACKAGES");

            Console.WriteLine($"Adding nuget references to data project");
            Support.RunCommand("dotnet", $"add package Npgsql", config.DataProjectFolder);
            Support.RunCommand("dotnet", $"add package Microsoft.EntityFrameworkCore", config.DataProjectFolder);
            Support.RunCommand("dotnet", $"add package Microsoft.EntityFrameworkCore.InMemory", config.DataProjectFolder);
            Support.RunCommand("dotnet", $"add package Npgsql.EntityFrameworkCore.PostgreSQL", config.DataProjectFolder);

            if (config.IncludeWebProject)
            {
                Console.WriteLine($"Adding nuget references to web project");
                Support.RunCommand("dotnet", $"add package Microsoft.EntityFrameworkCore", config.WebProjectFolder);
                Support.RunCommand("dotnet", $"add package Microsoft.EntityFrameworkCore.Design", config.WebProjectFolder);
            }
        }
    }
}
