using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Newt.Writers
{
    /// <summary>Creates a JSON dump for the schema.</summary>
    internal static class JSON
    {
        /// <summary>Write a JSON dump for the schema.</summary>
        public static void Write(Config config)
        {
            Console.WriteLine();
            Console.WriteLine("JSON DUMP");
            Support.EnsureFullPathExists(config.DataProjectFolder);

            var filename = Path.Combine(config.DataProjectFolder, "schema-dump.json");
            var opts = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() },
                ReferenceHandler = ReferenceHandler.Preserve,
            };
            var jsonString = JsonSerializer.Serialize(config.Schema, opts);

            Support.WriteFileWithChecks(filename, config.OverwriteData, jsonString);
        }
    }
}
