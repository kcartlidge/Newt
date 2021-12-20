using System;
using System.IO;
using Newt.Postgres;

namespace Newt
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("NEWT");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  Newt <db-env-var> <folder> <namespace> [-force]");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine("  Newt DB_CONNSTR \"\\Source\\Core\\SampleAPI\" SampleAPI -force");
            Console.WriteLine();
            Console.WriteLine("db-env-var = Environment variable containing the connection string");
            Console.WriteLine("folder     = Location of the new solution (and nested projects)");
            Console.WriteLine("namespace  = The top level namespace for the generated C# code");
            Console.WriteLine("-force     = Allows overwriting (if provided it must be last)");
            Console.WriteLine();
            
            try
            {
                if (args.Length == 3 || (args.Length == 4 && args[3].TrimStart('-').ToLower() == "force"))
                {
                    // Has the expected number of arguments.
                    var envVar = args[0];
                    var folder = args[1];
                    var @namespace = args[2];
                    var useForce = (args.Length == 4);
                    var dataFolder = Path.Combine(folder, $"{@namespace}");

                    // Show the details.
                    Console.WriteLine();
                    Console.WriteLine("COMMAND ARGUMENTS");
                    Console.WriteLine($"Environment = {envVar}");
                    Console.WriteLine($"Folder      = {folder}");
                    Console.WriteLine($"Namespace   = {@namespace}");
                    Console.WriteLine($"Force?      = {useForce}");
                    Console.WriteLine($"Destination = {dataFolder}");
                    
                    // Scan the database.
                    Console.WriteLine();
                    Console.WriteLine("SCANNING DATABASE");
                    var connstr = Environment.GetEnvironmentVariable(envVar) ?? string.Empty;
                    var db = new PostgresScanner(connstr, "public").Scan();
                    Console.WriteLine($"Database    = {db.DatabaseName}");
                    Console.WriteLine($"Tables      = {db.Tables.Count}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("ERROR:");
                Console.WriteLine(ex.Message);
                Console.WriteLine();
            }
        }
    }
}
