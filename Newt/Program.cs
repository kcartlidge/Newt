using System;
using System.IO;
using Newt.ArgsParsing;
using Newt.Postgres;
using Newt.Writers;

namespace Newt
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("NEWT (build 2023-08-18)");
            Console.WriteLine("Generate a DotNet (C#/EF Core) data access repository project from a Postgres database.");
            var parser = new Parser(args)
                .RequiresParameter<string>("env", "Environment variable containing the connection string", "")
                .RequiresParameter<string>("schema", "The database schema to generate code for", "public")
                .RequiresParameter<string>("folder", "Location of the (existing) solution", "")
                .RequiresParameter<string>("namespace", "The top level namespace for the generated C# code")
                .SupportsOption("force", "Overwrite any destination content")
                .Help();
            Console.WriteLine("Example:");
            Console.WriteLine("  Newt -env DB_CONNSTR -folder \"/Source/Core/SampleAPI\" -namespace SampleAPI.Data -schema public -force");
            Console.WriteLine();
            Console.WriteLine("The namespace will also be used as the project name/subfolder.");
            Console.WriteLine();

            try
            {
                parser.Parse();
                if (parser.HasErrors)
                {
                    foreach (var err in parser.ExpectationErrors) Console.WriteLine(err.Value);
                    foreach (var err in parser.ArgumentErrors) Console.WriteLine(err.Value);
                }
                else
                {
                    // Has the expected arguments.
                    var envVar = parser.GetParameter<string>("env");
                    var schema = parser.GetParameter<string>("schema");
                    var folder = Path.GetFullPath(parser.GetParameter<string>("folder"));
                    var @namespace = parser.GetParameter<string>("namespace");
                    var useForce = parser.IsOptionProvided("force");
                    var dataFolder = Path.Combine(folder, $"{@namespace}");

                    // Show the details.
                    Console.WriteLine();
                    Console.WriteLine("COMMAND ARGUMENTS");
                    Console.WriteLine($"Environment = {envVar}");
                    Console.WriteLine($"DB Schema   = {schema}");
                    Console.WriteLine($"Folder      = {folder}");
                    Console.WriteLine($"Namespace   = {@namespace}");
                    Console.WriteLine($"Force?      = {useForce}");

                    // Scan the database.
                    Console.WriteLine();
                    Console.WriteLine("SCANNING DATABASE");
                    var connstr = Environment.GetEnvironmentVariable(envVar) ?? string.Empty;
                    var db = new PostgresScanner(connstr, schema).Scan();
                    Console.WriteLine($"Database    = {db.DatabaseName}");
                    Console.WriteLine($"Tables      = {db.Tables.Count}");

                    // Clear out existing stuff at the destination.
                    if (useForce)
                    {
                        Console.WriteLine();
                        Console.WriteLine("CLEARING EXISTING");
                        FileOps.ClearFolder(dataFolder);
                    }

                    // Create the data solution.
                    new JSON(db, useForce, folder, @namespace).Write();
                    new Graphviz(db, useForce, folder, @namespace).Write();
                    new DataProject(db, useForce, folder, @namespace).Write();
                    new SqlScripts(db, useForce, dataFolder, @namespace).Write();
                    new Entities(db, useForce, dataFolder, @namespace).Write();
                    new Contexts(db, useForce, dataFolder, @namespace, envVar).Write();
                    new NugetPackages(db, useForce, folder, @namespace).Write();
                }

                Console.WriteLine();
                Console.WriteLine("DONE");
                Console.WriteLine();
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