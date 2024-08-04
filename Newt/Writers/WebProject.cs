using System;
using System.IO;
using System.Linq;

namespace Newt.Writers
{
    /// <summary>Creates the .Net MVC project from an Empty web template.</summary>
    internal static class WebProject
    {
        /// <summary>
        /// Create the .Net project using the installed 'dotnet' command.
        /// </summary>
        public static void Write(Config config)
        {
            if (config.IncludeWebProject == false) return;

            Console.WriteLine();
            Console.WriteLine("WEB PROJECT");

            Console.WriteLine("Creating project");
            Support.RunCommand("dotnet", $"new web -n {config.WebNamespace} --kestrelHttpPort 3000 --kestrelHttpsPort 3001 --use-program-main", config.SolutionFolder);

            // Set up for data access.
            Console.WriteLine($"Referencing data project");
            Support.RunCommand("dotnet", $"add reference {config.DataProjectFile}", config.WebProjectFolder);

            // Add DI registration, controller discovery, etc.
            Console.WriteLine($"Updating pre-generated default file");
            WriteTemplatedFile("Program", config, null, "Program.cs");
            WriteTemplatedFile("AppSettings", config, null, "AppSettings.json");
            WriteTemplatedFile("LaunchSettings", config, null, "Properties", "LaunchSettings.json");

            // Add the services.
            Console.WriteLine($"Adding services");
            WriteTemplatedFile("ConfigService", config, null, "ConfigService.cs");
            WriteTemplatedFile("SessionService", config, null, "SessionService.cs");

            // Add the controllers, one per entity in an Admin folder.
            Console.WriteLine($"Adding controllers");
            foreach (var table in config.Schema.Tables)
            {
                var name = $"{table.ClassNamePlural}Controller";
                WriteTemplatedFile("Controller", config, table, "Controllers", $"{name}.cs");
            }
            WriteTemplatedFile("HomeController", config, null, "Controllers", $"HomeController.cs");
            WriteTemplatedFile("LoginRequestViewModel", config, null, "Models", $"LoginRequestViewModel.cs");
            WriteTemplatedFile("PagerData", config, null, "Models", $"PagerData.cs");

            // Add the layouts etc.
            Console.WriteLine($"Adding layouts etc");
            WriteTemplatedFile("ViewImports", config, null, "Views", "_ViewImports.cshtml");
            WriteTemplatedFile("ViewStart", config, null, "Views", "_ViewStart.cshtml");
            WriteTemplatedFile("Layout", config, null, "Views", "Shared", "_Layout.cshtml");
            WriteTemplatedFile("Stylesheet", config, null, "wwwroot", "site.css");

            // Add the views, one folder per entity.
            Console.WriteLine($"Adding views");
            foreach (var table in config.Schema.Tables)
            {
                var name = table.ClassName;
                var plural = table.ClassNamePlural;
                WriteTemplatedFile("List", config, table, "Views", plural, "Index.cshtml");
                WriteTemplatedFile("Create", config, table, "Views", plural, "Create.cshtml");
                WriteTemplatedFile("Edit", config, table, "Views", plural, "Edit.cshtml");
                WriteTemplatedFile("Delete", config, table, "Views", plural, "Delete.cshtml");
            }
            WriteTemplatedFile("HomeView", config, null, "Views", "Home", "Index.cshtml");
            WriteTemplatedFile("LoginView", config, null, "Views", "Home", "Login.cshtml");
            WriteTemplatedFile("LogoutView", config, null, "Views", "Home", "Logout.cshtml");
            WriteTemplatedFile("PagerView", config, null, "Views", "Shared", "_Pager.cshtml");
        }

        private static void WriteTemplatedFile(
            string template,
            Config config,
            Models.DBTable? table,
            params string[] pathSegments)
        {
            var src = ScaffoldedFiles.Scaffold.GetSource(config, template,
                    config.DataNamespace, config.WebNamespace, table);

            var route = pathSegments.ToList();
            route.Insert(0, config.WebProjectFolder);
            var filename = Path.Join(route.ToArray());
            var folder = Path.GetDirectoryName(filename);

            Directory.CreateDirectory(folder!);
            Support.WriteFileWithChecks(filename, true, src);
        }
    }
}