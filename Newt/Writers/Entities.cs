using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Newt.Writers
{
    /// <summary>Creates the .Net EF Core entity models.</summary>
    internal static class Entities
    {
        /// <summary>Write all the .Net EF Core entity models.</summary>
        public static void Write(Config config)
        {
            Support.ShowHeading("ENTITIES");
            Support.EnsureFullPathExists(config.DataProjectFolder, "Entities");

            foreach (var table in config.Schema.Tables)
            {
                var filename = Path.Combine(config.DataProjectFolder, "Entities", $"{table.ClassName}.cs");
                var src = new StringBuilder();
                src.AppendLine($"#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.");
                src.AppendLine($"");
                src.AppendLine($"using System;");
                if (table.NavigationProperties.Any())
                    src.AppendLine($"using System.Collections.Generic;");
                src.AppendLine($"using System.ComponentModel;");
                src.AppendLine($"using System.ComponentModel.DataAnnotations;");
                src.AppendLine($"using System.ComponentModel.DataAnnotations.Schema;");
                src.AppendLine($"");
                src.AppendLine($"namespace {config.DataNamespace}.Entities");
                src.AppendLine($"{{");

                if (table.Comment.HasValue())
                {
                    src.AppendLine($"    /// <summary>");
                    src.AppendLine($"    /// {table.Comment}");
                    src.AppendLine($"    /// </summary>");
                }

                src.AppendLine($"    [Table(\"{table.Name}\")]");
                src.AppendLine($"    public class {table.ClassName}");
                src.AppendLine($"    {{");
                var first = true;
                foreach (var column in table.Columns)
                {
                    if (!first) src.AppendLine($"");
                    first = false;

                    src.Append(column.AsCode());
                }

                foreach (var navigation in table.NavigationProperties)
                {
                    var clsName = navigation.Constraint?.Table.ClassName;
                    var clsNames = navigation.Constraint?.Table.ClassNamePlural;
                    src.AppendLine();
                    src.AppendLine($"        /// <summary>Foreign key on {clsName}</summary>");
                    src.AppendLine($"        public List<{clsName}> {clsNames} {{ get; set; }} = new List<{clsName}>();");
                }

                src.AppendLine($"    }}");
                src.AppendLine($"}}");
                src.AppendLine($"");
                src.AppendLine($"#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.");

                Support.WriteFileWithChecks(config.SolutionFolder, filename, config.OverwriteData, src.ToString());
            }
        }
    }
}
