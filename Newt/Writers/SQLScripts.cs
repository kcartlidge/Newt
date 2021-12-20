using System.Linq;
using System.Text;
using Newt.Models;

namespace Newt.Writers
{
    internal class SqlScripts : BaseWriter
    {
        public SqlScripts(DBSchema db, bool useForce, string folder, string @namespace)
            : base(db, useForce, folder, @namespace) { }

        public void Write()
        {
            EnsureFolder("SQL SCRIPT", "SQL");

            var src = StartFile($"Postgres.sql");
            src.AppendLine($"/*");
            src.AppendLine($"   WARNING - DESTRUCTIVE SCRIPT");
            src.AppendLine($"   This is a fallback script only, NOT a structural database backup.");
            src.AppendLine($"   The SQL below only represents what was needed to generate C# code");
            src.AppendLine($"   and is intended solely for emergency use when other backups fail.");
            src.AppendLine($"   Schemas created from it may not be complete (eg sequences).");
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
                var name = c.Name.PadRight(table.ColumnNameWidth);
                var capacity = c.Capacity.HasValue ? $"({c.Capacity})" : "";
                var sqlType = c.Datatype + capacity;
                var isPrimaryKey = table.Constraints.Any(x => x.Column == c.Name && x.IsPrimaryKey);
                if (isPrimaryKey && c.IsCardinal) sqlType = "BIGSERIAL";
                src.AppendLine($"  {name}  {sqlType}{nullable},");
            }

            src.AppendLine();
            var remaining = table.Constraints.Count;
            foreach (var c in table.Constraints)
                src.AppendLine($"{c.GetDefinition(--remaining > 0)}");

            src.AppendLine($");");
            src.AppendLine($"ALTER TABLE {table.Schema}.{table.Name} OWNER to {table.Owner};");
            foreach (var c in table.Indexes) src.AppendLine($"{c.Definition};");
        }
    }
}
