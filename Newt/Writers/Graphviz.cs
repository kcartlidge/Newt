using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Newt.Writers
{
    /// <summary>Creates the Graphviz `.dot` file for the schema.</summary>
    internal static class Graphviz
    {
        /// <summary>Writes the Graphviz `.dot` file for the schema.</summary>
        public static void Write(Config config)
        {
            Support.ShowHeading("GRAPHVIZ");
            Support.EnsureFullPathExists(config.DataProjectFolder);

            var filename = Path.Combine(config.DataProjectFolder, "schema.dot");
            var src = new StringBuilder();
            src.AppendLine("// GRAPHVIZ DEFINITION");
            src.AppendLine("//");
            src.AppendLine("// Example usage:");
            src.AppendLine("//   dot -o schema.png -Tpng schema.dot");
            src.AppendLine("//   dot -o schema.svg -Tsvg schema.dot");
            src.AppendLine("");

            // Define the graph layout and styling.
            src.AppendLine(@"digraph {
  label = 'Class diagram for SampleAPI'
  labelloc = 'top'
  fontname = 'Verdana'
  fontsize = 16
  nodesep = 0.5
  ranksep = 0.5
  pad = 0.25

  node[
    fontname = 'Monospace'
    fontsize = 12
    shape = 'Mrecord'
    width = 1.5
    margin = '0.1,0.1'
  ]

  edge[
    arrowhead = 'dot'
  ]
").Replace("'", "\"").Replace("SampleAPI", config.DataNamespace);

            // Write the tables and their columns.
            src.AppendLine("  // Classes.");
            foreach (var table in config.Schema.Tables)
            {
                var def = new StringBuilder($"  {table.ClassName} [ label = \"{{{table.ClassName}");

                // Columns.
                if (table.Columns.Any())
                {
                    var maxTypeLen = table.Columns
                        .Max(x => $"{x.PropertyType} [{x.Capacity}]".Length);
                    def.Append(" | ");
                    foreach (var c in table.Columns)
                    {
                        var name = c.PropertyName + " ";
                        var tp = " " + c.PropertyType + (c.UseCapacity ? $"[{c.Capacity}]" : "");
                        name = name.PadRight(table.ColumnNameWidth + 2, '.');
                        tp = tp.PadLeft(maxTypeLen, '.');
                        def.Append($"{name}{tp}\\n");
                    }
                }

                // Collections.
                if (table.NavigationProperties.Any())
                {
                    var maxNameLen = table.NavigationProperties
                        .Max(x => x.Table?.ClassNamePlural.Length) ?? 0;
                    def.Append(" | ");
                    foreach (var navigation in table.NavigationProperties)
                    {
                        var clsNames = navigation.Constraint?.Table.ClassNamePlural ?? "\\<error\\>";
                        def.Append($"{clsNames}\\n");
                    }
                }
                def.Append("}}\" ]");
                src.AppendLine(def.ToString());
            }

            // Write the table relationships.
            var first = true;
            foreach (var table in config.Schema.Tables)
            {
                if (table.NavigationProperties.Any())
                {
                    if (first)
                    {
                        src.AppendLine();
                        src.AppendLine("  // Foreign keys.");
                    }
                    first = false;
                    foreach (var navigation in table.NavigationProperties)
                    {
                        var tableFrom = navigation.Constraint?.ForeignTable ?? "src";
                        var classFrom = config.Schema.Tables.First(x => x.Name == tableFrom).ClassName;
                        var classTo = navigation.Constraint?.Table.ClassName ?? "dest";
                        src.AppendLine($"  {classFrom} -> {classTo}");
                    }
                }
            }

            src.AppendLine("}");

            Support.WriteFileWithChecks(config.SolutionFolder, filename, config.OverwriteData, src.ToString());
        }
    }
}
