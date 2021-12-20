using System;
using Newt.Models;

namespace Newt.Writers
{
    internal class Contexts : BaseWriter
    {
        private readonly string _envVar;

        public Contexts(DBSchema db, bool force, string folder, string @namespace, string envVar)
            : base(db, force, folder, @namespace)
        {
            _envVar = envVar;
        }

        public void Write()
        {
            EnsureFolder("EF CORE CONTEXTS");

            foreach (var contextType in new[] {"Data", "InMemoryData"})
            {
                var src = StartFile($"{contextType}Context.cs");
                src.AppendLine($"using System;");
                src.AppendLine($"using Microsoft.EntityFrameworkCore;");
                src.AppendLine($"using {Namespace}.Entities;");
                src.AppendLine($"");
                src.AppendLine($"#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.");
                if (contextType == "Data")
                    src.AppendLine($"#pragma warning disable CS8604 // Possible null reference argument.");
                src.AppendLine($"");
                src.AppendLine($"namespace {Namespace}");
                src.AppendLine($"{{");
                src.AppendLine($"    public class {contextType}Context : DbContext");
                src.AppendLine($"    {{");
                foreach (var table in Schema.Tables)
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
                    src.AppendLine($"            var connectionString = Environment.GetEnvironmentVariable(\"{_envVar}\");");
                    src.AppendLine($"            optionsBuilder.UseNpgsql(connectionString);");
                }
                else
                {
                    src.AppendLine($"            optionsBuilder.UseInMemoryDatabase(databaseName: \"InMemoryDataContext\");");
                }
                src.AppendLine($"        }}");
                src.AppendLine($"    }}");
                src.AppendLine($"}}");
                src.AppendLine($"");
                src.AppendLine($"#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor.");
                if (contextType == "Data")
                    src.AppendLine($"#pragma warning restore CS8604 // Possible null reference argument.");
                FinishFile();
            }
        }
    }
}
