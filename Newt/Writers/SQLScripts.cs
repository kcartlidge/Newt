using System;
using System.IO;
using System.Linq;
using System.Text;
using Newt.Models;

namespace Newt.Writers
{
    /// <summary>Creates the emergency-use SQL scripts for the schema.</summary>
    internal static class SqlScripts
    {
        /// <summary>Write the emergency-use SQL scripts for the schema.</summary>
        public static void Write(Config config)
        {
            Console.WriteLine();
            Console.WriteLine("SQL SCRIPT");
            Support.EnsureFullPathExists(config.DataProjectFolder, "SQL");

            var filename = Path.Join(config.DataProjectFolder, "SQL", "Postgres.sql");
            var src = new StringBuilder();
            src.AppendLine($"/*");
            src.AppendLine($"  WARNING - DESTRUCTIVE SCRIPT");
            src.AppendLine($"  This is a fallback script only, NOT a structural database backup.");
            src.AppendLine($"  The SQL below only represents what was needed to generate C# code");
            src.AppendLine($"  and is intended solely for emergency use when other backups fail.");
            src.AppendLine($"  Schemas created from it may not be complete (eg sequences).");
            src.AppendLine($"");
            src.AppendLine($"  The script should NOT be simply executed in one go!");
            src.AppendLine($"  Several deliberate restrictions force you to take care:");
            src.AppendLine($"  - DROP statements assume things already exist");
            src.AppendLine($"  - Tables are ALPHABETICAL, not in order of dependencies");
            src.AppendLine($"*/");
            foreach (var table in config.Schema.Tables)
            {
                src.AppendLine($"");
                GetPostgresScriptForTable(table, src, config.DataNamespace, true);
            }

            Support.WriteFileWithChecks(filename, config.OverwriteData, src.ToString());
        }

        private static void GetPostgresScriptForTable(DBTable table, StringBuilder src, string dbNamespace, bool includeDrop)
        {
            src.AppendLine($"");
            src.AppendLine($"");
            src.AppendLine($"-------- {dbNamespace}.Models.{table.ClassName} --------");
            src.AppendLine($"");
            if (includeDrop) src.AppendLine($"DROP TABLE {table.Schema}.{table.Name} CASCADE;");

            src.AppendLine($"CREATE TABLE IF NOT EXISTS {table.Schema}.{table.Name} (");
            foreach (var c in table.Columns)
            {
                var nullable = c.IsNullable ? "" : " NOT NULL";
                var defVal = c.HasDefault ? $" DEFAULT {c.DefaultValue}" : "";
                var name = c.Name.PadRight(table.ColumnNameWidth);
                var capacity = c.Capacity.HasValue ? $"({c.Capacity})" : "";
                var sqlType = c.Datatype + capacity;
                var isPrimaryKey = table.Constraints.Any(x => x.Column == c.Name && x.IsPrimaryKey);
                if (isPrimaryKey && c.IsCardinal) sqlType = "BIGSERIAL";
                src.AppendLine($"  {name}  {sqlType}{nullable}{defVal},");
            }

            src.AppendLine();
            var remaining = table.Constraints.Count;
            foreach (var c in table.Constraints)
                src.AppendLine($"{c.GetDefinition(--remaining > 0)}");

            src.AppendLine($");");
            src.AppendLine($"");
            src.AppendLine($"ALTER TABLE {table.Schema}.{table.Name} OWNER to {table.Owner};");
            foreach (var c in table.Indexes) src.AppendLine($"{c.Definition};");

            if (table.Comment.HasValue())
            {
                var comment = table.Comment.Trim().Replace("'", "''");
                src.AppendLine($"COMMENT ON TABLE {table.Schema}.{table.Name} IS '{comment}';");
            }

            foreach (var c in table.Columns)
            {
                if (c.Comment.HasValue())
                {
                    var comment = c.Comment.Trim().Replace("'", "''");
                    src.AppendLine($"COMMENT ON COLUMN {c.FullName} IS '{comment}';");
                }
            }
        }
    }
}
