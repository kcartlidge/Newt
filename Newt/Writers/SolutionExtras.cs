using System;
using System.IO;
using System.Text;

namespace Newt.Writers
{
    /// <summary>Creates extra solution-level files.</summary>
    internal static class SolutionExtras
    {
        /// <summary>Create extra solution-level files.</summary>
        public static void Write(Config config)
        {
            Console.WriteLine();
            Console.WriteLine("SOLUTION EXTRAS");

            // Skip if there's already any solution file(s).
            if (config.CreateSolution == false)
            {
                Console.WriteLine("Skipped - a solution is already present");
                return;
            }

            // Define the necessary stuff.
            if (config.SolutionName.HasValue() == false)
                throw new Exception("A solution name is needed as there is no existing solution.");

            // Add .gitignore file.
            var filename = Path.Combine(config.SolutionFolder, ".gitignore");
            var src = new StringBuilder();
            src.AppendLine($".vs/");
            src.AppendLine($".idea/");
            src.AppendLine($".vscode/");
            src.AppendLine($".DS_Store");
            src.AppendLine($"");
            src.AppendLine($"bin/");
            src.AppendLine($"obj/");
            src.AppendLine($"packages/");
            Support.WriteFileWithChecks(filename, config.OverwriteData, src.ToString());

            // Add .gitattributes file.
            filename = Path.Combine(config.SolutionFolder, ".gitattributes");
            src = new StringBuilder();
            src.AppendLine($"* text=auto");
            src.AppendLine($"");
            src.AppendLine($"*.cs      text");
            src.AppendLine($"*.cshtml  text");
            Support.WriteFileWithChecks(filename, config.OverwriteData, src.ToString());

            // Add .editorconfig file.
            filename = Path.Combine(config.SolutionFolder, ".editorconfig");
            src = new StringBuilder();
            src.AppendLine($"root = true");
            src.AppendLine($"");
            src.AppendLine($"[*]");
            src.AppendLine($"charset = utf-8");
            src.AppendLine($"insert_final_newline = true");
            src.AppendLine($"trim_trailing_whitespace = true");
            src.AppendLine($"end_of_line = unset");
            src.AppendLine($"indent_size = 4");
            src.AppendLine($"indent_style = space");
            src.AppendLine($"tab_width = 4");
            src.AppendLine($"");
            src.AppendLine($"[*.md]");
            src.AppendLine($"indent_size = 2");
            Support.WriteFileWithChecks(filename, config.OverwriteData, src.ToString());

            // Add README file.
            filename = Path.Combine(config.SolutionFolder, "README.md");
            src = new StringBuilder();
            src.AppendLine($"# {config.SolutionName}");
            src.AppendLine($"");
            src.AppendLine($"- `{config.DataNamespace}` is the data project");
            src.AppendLine($"- `{config.WebNamespace}` is an MVC admin site");
            src.AppendLine($"- View the [CHANGELOG](./CHANGELOG.md)");
            src.AppendLine($"- Read the [LICENSE](./LICENSE.txt)");
            Support.WriteFileWithChecks(filename, config.OverwriteData, src.ToString());

            // Add a minimal CHANGELOG file.
            filename = Path.Combine(config.SolutionFolder, "CHANGELOG.md");
            src = new StringBuilder();
            src.AppendLine($"# CHANGELOG");
            src.AppendLine($"");
            src.AppendLine($"- {DateTime.Now.ToString("yyyy-MM-dd")}");
            src.AppendLine($"  - Initial solution and standard files");
            src.AppendLine($"  - Data project (generated by Newt)");
            if (config.IncludeWebProject)
                src.AppendLine($"  - Web project (generated by Newt)");
            Support.WriteFileWithChecks(filename, config.OverwriteData, src.ToString());

            // Add an empty LICENSE file.
            filename = Path.Combine(config.SolutionFolder, "LICENSE.txt");
            src = new StringBuilder();
            src.AppendLine($"FILL THIS IN");
            src.AppendLine($"");
            src.AppendLine($"You can find various license details at:");
            src.AppendLine($"https://www.choosealicense.com");
            Support.WriteFileWithChecks(filename, config.OverwriteData, src.ToString());
        }
    }
}