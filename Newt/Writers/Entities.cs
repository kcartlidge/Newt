using Newt.Models;

namespace Newt.Writers
{
    internal class Entities : BaseWriter
    {
        public Entities(DBSchema db, bool force, string folder, string @namespace)
            : base(db, force, folder, @namespace) { }

        public void Write()
        {
            EnsureFolder("ENTITIES", "Entities");

            foreach (var table in Schema.Tables)
            {
                var src = StartFile($"{table.ClassName}.cs");
                src.AppendLine($"using System;");
                src.AppendLine($"using System.ComponentModel;");
                src.AppendLine($"using System.ComponentModel.DataAnnotations;");
                src.AppendLine($"using System.ComponentModel.DataAnnotations.Schema;");
                src.AppendLine($"");
                src.AppendLine($"#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.");
                src.AppendLine($"");
                src.AppendLine($"namespace {Namespace}.Entities");
                src.AppendLine($"{{");
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
                src.AppendLine($"    }}");
                src.AppendLine($"}}");
                src.AppendLine($"");
                src.AppendLine($"#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.");
                FinishFile();
            }
        }
    }
}
