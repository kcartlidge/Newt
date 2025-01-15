using System;

namespace Newt.Writers
{
    /// <summary>Creates the .Net solution.</summary>
    internal static class DotNetSolution
    {
        /// <summary>
        /// Create the .Net solution using the installed 'dotnet' command.
        /// </summary>
        public static void Write(Config config)
        {
            Support.ShowHeading("DOTNET SOLUTION");

            // Skip if there's already any solution file(s).
            if (config.CreateSolution == false)
            {
                Console.WriteLine("Skipped - a solution file is already present");
                return;
            }

            // Define the necessary stuff.
            if (config.SolutionName.HasValue() == false)
                throw new Exception("A solution name is needed as there is no existing solution.");

            // Create the solution.
            Console.WriteLine($"Creating solution");
            Support.RunCommand("dotnet", $"new solution -n {config.SolutionName}", config.SolutionFolder);
        }
    }
}
