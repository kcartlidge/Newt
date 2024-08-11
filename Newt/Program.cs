using System;
using System.IO;
using System.Linq;
using Newt.Postgres;
using Newt.Writers;

namespace Newt
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Welcome text.
            Console.WriteLine();
            Console.WriteLine("NEWT (build 2024-08-11)");
            Console.WriteLine();
            Console.WriteLine("Generates a DotNet (C#/EF Core) data access repository project from a Postgres database.");
            Console.WriteLine("Optionally also creates a matching MVC site for data management.");
            Console.WriteLine();
            Console.WriteLine("If the parent folder has no solution it will be created and the project(s) added.");
            Console.WriteLine("Existing solutions will NOT be updated to include new project(s).");
            Console.WriteLine();
            Console.WriteLine("The solution name is used if a new solution needs creating.");
            Console.WriteLine("Namespaces will also be used as project names/subfolders.");
            Console.WriteLine();

            // Command argument details.
            Console.WriteLine();
            Console.WriteLine("USAGE:");
            Console.WriteLine();
            var parser = new ArgsParser.Parser(args)
                .RequiresOption<string>("env", "Environment variable containing the connection string")
                .RequiresOption<string>("s", "The database schema to generate code for")
                .RequiresOption<string>("f", "The parent folder for the solution")
                .RequiresOption<string>("sln", "The solution name/namespace")
                .RequiresOption<string>("dn", "The C# data project name/namespace")
                .SupportsOption<string>("wn", "The C# MVC web project name/namespace")
                .SupportsFlag("od", "Overwrite existing data project")
                .SupportsFlag("ow", "Overwrite existing web project")
                .SupportsFlag("os", "Overwrite existing solution")
                .Help(2);
            Console.WriteLine("EXAMPLE:");
            Console.WriteLine();
            Console.WriteLine("  Newt -env DB_CONNSTR -s public -f ./sample -sln SampleAPI -dn Data -wn Admin");
            Console.WriteLine();

            try
            {
                // Validate the arguments.
                parser.Parse();
                if (parser.HasErrors)
                {
                    Console.WriteLine("ISSUES:");
                    Console.WriteLine();
                    parser.ShowErrors(2);
                    throw new Exception("The provided arguments are invalid.");
                }

                // Set up the config.
                Config config = new Config
                {
                    EnvironmentVariableName = parser.GetOption<string>("env"),
                    SchemaName = parser.GetOption<string>("s"),
                    SolutionFolder = Path.GetFullPath(parser.GetOption<string>("f")),
                    SolutionName = parser.GetOption<string>("sln"),
                    DataNamespace = parser.GetOption<string>("dn"),
                    WebNamespace = parser.GetOption<string>("wn"),
                    OverwriteData = parser.IsFlagProvided("od"),
                    OverwriteWeb = parser.IsFlagProvided("ow"),
                    OverwriteSolution = parser.IsFlagProvided("os")
                };
                config.SolutionExists = Directory.Exists(config.SolutionFolder);
                config.DataExists = Directory.Exists(config.DataProjectFolder);
                config.WebExists = Directory.Exists(config.WebProjectFolder);

                // Show the details.
                Console.WriteLine();
                Console.WriteLine("CONFIG:");
                Console.WriteLine();
                Console.WriteLine($"Environment     = {config.EnvironmentVariableName}");
                Console.WriteLine($"DB Schema       = {config.SchemaName}");
                Console.WriteLine();
                Console.WriteLine($"Folder          = {config.SolutionFolder}");
                Console.WriteLine($"Solution        = {config.SolutionName}");
                Console.WriteLine($"Data Project    = {config.DataNamespace}");
                Console.WriteLine($"Web Project     = {config.WebNamespace.Or("-")}");
                Console.WriteLine();
                if (config.SolutionExists && config.OverwriteSolution == false)
                    Console.WriteLine($"The SOLUTION folder exists and WILL NOT be overwritten!");
                if (config.DataExists && config.OverwriteData == false)
                    Console.WriteLine($"The DATA project folder exists and WILL NOT be overwritten!");
                if (config.WebExists && config.OverwriteWeb == false)
                    Console.WriteLine($"The WEB project folder exists and WILL NOT be overwritten!");
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("============== PROCESSING ==============");

                // Scan the database.
                Console.WriteLine();
                Console.WriteLine("SCANNING DATABASE");
                config.Schema = new PostgresScanner(config.ConnectionString, config.SchemaName).Scan();
                Console.WriteLine($"Database    = {config.Schema.DatabaseName}");
                Console.WriteLine($"Tables      = {config.Schema.Tables.Count}");

                // Sort out the destination folders.
                Console.WriteLine();
                if (config.CreateSolution)
                {
                    if (config.SolutionExists) Support.ClearFolder(config.SolutionFolder);
                    Support.EnsureFullPathExists(config.SolutionFolder);
                    config.DataExists = false;
                    config.WebExists = false;
                }
                if (config.CreateData)
                {
                    if (config.DataExists) Support.ClearFolder(config.DataProjectFolder);
                    Support.EnsureFullPathExists(config.DataProjectFolder);
                }
                if (config.CreateWeb)
                {
                    if (config.WebExists) Support.ClearFolder(config.WebProjectFolder);
                    Support.EnsureFullPathExists(config.WebProjectFolder);
                }

                // Check the mimimum .NET SDK version.
                // Only relevant for Web; the Data project doesn't care.
                // NET 6.0.300 is needed for the dotnet new mvc parameter
                // to switch off top level statements (--use-program-main).
                var reason = "Check support for switching off top level statements";
                if (config.IncludeWebProject)
                    Support.RequireVersion(config.SolutionFolder, 6, 0, 300, reason);

                // Create the solution.
                DotNetSolution.Write(config);
                SolutionExtras.Write(config);

                // Create the data project.
                JSON.Write(config);
                Graphviz.Write(config);
                DataProject.Write(config);
                SqlScripts.Write(config);
                Entities.Write(config);
                Contexts.Write(config);

                // Create the web project.
                WebProject.Write(config);

                // Tidy up the solution and packages.
                AddProjectsToSolution.Write(config);
                NugetPackages.Write(config);

                if (config.IncludeWebProject)
                {
                    Console.WriteLine();
                    Console.WriteLine();
                    Console.WriteLine("IMPORTANT!");
                    Console.WriteLine("The web project is password protected.");
                    Console.WriteLine("By default the password is a random guid stored");
                    Console.WriteLine("in the AppSettings.json file; you should change");
                    Console.WriteLine("this to something not committed to source control.");
                }

                // Solution warning.
                if (config.CreateSolution)
                {
                    Console.WriteLine();
                    Console.WriteLine("You MUST update the LICENCE.txt file!");
                }

                Console.WriteLine();
                Console.WriteLine("DONE");
                Console.WriteLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("ERROR:");
                Console.WriteLine(ex.Message);
                Console.WriteLine();
            }
        }
    }
}