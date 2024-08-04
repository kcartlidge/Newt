using HandlebarsDotNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Newt.Writers.ScaffoldedFiles
{
    internal static class Scaffold
    {
        public static string GetSource(
            Config config,
            string templateName,
            string dataNamespace,
            string webNamespace,
            Models.DBTable? table
            )
        {
            var info = Assembly.GetExecutingAssembly().GetName();
            var name = info.Name;
            using (var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream($"{name}.Writers.ScaffoldedFiles.Templates.{templateName}.hbs")!)
            {
                using (var streamReader = new StreamReader(stream, Encoding.UTF8))
                {
                    var source = streamReader.ReadToEnd();
                    var data = new
                    {
                        DataNS = dataNamespace,
                        WebNS = webNamespace,
                        Plural = table == null ? "" : table.ClassNamePlural,
                        Name = table == null ? "" : table.ClassName,
                        Columns = table == null ? new List<Models.DBColumn>() : table.Columns,
                        ReadonlyColumns = table == null ? new List<Models.DBColumn>() : table.ReadonlyColumns,
                        EditableColumns = table == null ? new List<Models.DBColumn>() : table.EditableColumns,
                        Comment = table == null ? "" : table.Comment,
                        Tables = config.Schema.Tables,
                        RandomGuid = Guid.NewGuid(),
                    };

                    // The Compile can be cached for performance.
                    // For our purpose it's not worth the bother.
                    var template = Handlebars.Compile(source);

                    var result = template(data);
                    return result;
                }
            }
        }
    }
}
