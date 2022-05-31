using System.Linq;
using System.Text;
using Newt.Models;

namespace Newt.Writers
{
    /// <summary>Creates the emergency-use SQL scripts for the schema.</summary>
    internal class SqlScripts : BaseWriter
    {
        public SqlScripts(DBSchema db, bool useForce, string folder, string @namespace)
            : base(db, useForce, folder, @namespace) { }

        /// <summary>Write the emergency-use SQL scripts for the schema.</summary>
        public void Write()
        {
            EnsureFullPathExists("SQL SCRIPT", "SQL");

            var src = StartFile($"Postgres.sql");
            src.AppendLine($"/*");
            src.AppendLine($"  WARNING - DESTRUCTIVE SCRIPT");
            src.AppendLine($"  This is a fallback script only, NOT a structural database backup.");
            src.AppendLine($"  The SQL below only represents what was needed to generate C# code");
            src.AppendLine($"  and is intended solely for emergency use when other backups fail.");
            src.AppendLine($"  Schemas created from it may not be complete (eg sequences).");
            src.AppendLine($"");
            src.AppendLine($"  The script should NOT be simply executed in one go!");
            src.AppendLine($"  Several steps are teken to ensure you need to take care:");
            src.AppendLine($"");
            src.AppendLine($"  * DROP statements assume things already exist");
            src.AppendLine($"    This is deliberate to stop simple execution");
            src.AppendLine($"");
            src.AppendLine($"  * The tables are listed alphabetically");
            src.AppendLine($"    Do them manually in order of dependencies");
            src.AppendLine($"*/");
            foreach (var table in Schema.Tables)
            {
                src.AppendLine($"");
                GetPostgresScriptForTable(table, src, Namespace, true);
            }
            FinishFile();
        }

        private void GetPostgresScriptForTable(DBTable table, StringBuilder src, string @namespace, bool includeDrop)
        {
            src.AppendLine($"-------- {@namespace}.Models.{table.ClassName} --------");
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
            src.AppendLine($"ALTER TABLE {table.Schema}.{table.Name} OWNER to {table.Owner};");
            foreach (var c in table.Indexes) src.AppendLine($"{c.Definition};");

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
