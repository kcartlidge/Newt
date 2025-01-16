using System;

namespace Newt.Writers
{
    /// <summary>Updates the .Net solution.</summary>
    internal static class AddProjectsToSolution
    {
        /// <summary>
        /// Update the .Net solution using the installed 'dotnet' command.
        /// </summary>
        public static void Write(Config config)
        {
            Support.ShowHeading("ADD PROJECT(S) TO SOLUTION");

            if (config.IncludeWebProject)
            {
                // Visual Studio (sometimes) treats the first project listed as
                // the default start-up one, so do any Web first.
                // Doesn't always work, but there's no harm trying it.
                Console.WriteLine($"Adding web project");
                Support.RunCommand("dotnet", $"sln add {config.DataProjectFile}", config.SolutionFolder);
            }

            Console.WriteLine($"Adding data project");
            Support.RunCommand("dotnet", $"sln add {config.WebProjectFile}", config.SolutionFolder);
        }
    }
}
