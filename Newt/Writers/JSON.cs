using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newt.Models;

namespace Newt.Writers
{
    /// <summary>Creates a JSON dump for the schema.</summary>
    internal class JSON : BaseWriter
    {
        public JSON(DBSchema db, bool useForce, string folder, string @namespace)
            : base(db, useForce, folder, @namespace) { }

        /// <summary>Write a JSON dump for the schema.</summary>
        public void Write()
        {
            EnsureFullPathExists("JSON DUMP");

            var jsonFilename = Path.Combine(TopFolder, Namespace, "schema-dump.json");
            Console.WriteLine(jsonFilename);

            var opts = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() },
                ReferenceHandler = ReferenceHandler.Preserve,
            };
            var jsonString = JsonSerializer.Serialize(Schema, opts);
            File.WriteAllText(jsonFilename, jsonString);
        }
    }
}
