using System;
using System.IO;
using System.Text;

namespace Newt.Writers
{
    /// <summary>Writes the EF Core context definitions.</summary>
    internal static class Contexts
    {
        /// <summary>Write the contexts for both InMemory and Postgres.</summary>
        public static void Write(Config config)
        {
            Support.ShowHeading("EF CORE CONTEXTS");
            Support.EnsureFullPathExists(config.DataProjectFolder);

            var filename = Path.Combine(config.DataProjectFolder, "UtcDateAnnotation.cs");
            var utcFixSource = ScaffoldedFiles.Scaffold.GetSource(config, "UtcDateAnnotation", null);
            Support.WriteFileWithChecks(config.SolutionFolder, filename, config.OverwriteData, utcFixSource);

            foreach (var contextType in new[] { "Data", "InMemoryData" })
            {
                filename = Path.Combine(config.DataProjectFolder, $"{contextType}Context.cs");
                var src = new StringBuilder();
                src.AppendLine($"#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.");
                if (contextType == "Data")
                    src.AppendLine($"#pragma warning disable CS8604 // Possible null reference argument.");
                src.AppendLine($"");
                src.AppendLine($"using System;");
                src.AppendLine($"using Microsoft.EntityFrameworkCore;");
                src.AppendLine($"using {config.DataNamespace}.Entities;");
                src.AppendLine($"");
                src.AppendLine($"namespace {config.DataNamespace}");
                src.AppendLine($"{{");
                src.AppendLine($"    public class {contextType}Context : DbContext");
                src.AppendLine($"    {{");
                foreach (var table in config.Schema.Tables)
                {
                    if (table.ClassName == table.ClassNamePlural)
                        throw new Exception($"Table name must be singular, not plural: {table.Name}");
                    src.AppendLine($"        public DbSet<{table.ClassName}> {table.ClassNamePlural} {{ get; set; }}");
                }

                src.AppendLine($"");
                src.AppendLine($"        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)");
                src.AppendLine($"        {{");
                if (contextType == "Data")
                {
                    src.AppendLine($"            var connectionString = Environment.GetEnvironmentVariable(\"{config.EnvironmentVariableName}\");");
                    src.AppendLine($"            optionsBuilder.UseNpgsql(connectionString);");
                }
                else
                {
                    src.AppendLine($"            optionsBuilder.UseInMemoryDatabase(databaseName: \"InMemoryDataContext\");");
                }
                src.AppendLine($"        }}");
                src.AppendLine($"");
                src.AppendLine($"        protected override void OnModelCreating(ModelBuilder builder)");
                src.AppendLine($"        {{");
                src.AppendLine($"             builder.ApplyUtcDateTimeConverter();");
                src.AppendLine($"        }}");
                src.AppendLine($"    }}");
                src.AppendLine($"}}");
                src.AppendLine($"");
                src.AppendLine($"#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor.");
                if (contextType == "Data")
                    src.AppendLine($"#pragma warning restore CS8604 // Possible null reference argument.");

                Support.WriteFileWithChecks(config.SolutionFolder, filename, config.OverwriteData, src.ToString());
            }
        }
    }
}
