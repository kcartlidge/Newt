using Newt.Models;
using System;
using System.IO;

namespace Newt
{
    /// <summary>Holds options, settings, derived paths etc.</summary>
    internal class Config
    {
        public DBSchema Schema = new DBSchema();

        public string ParentFolder => Path.GetFullPath(Path.Combine(SolutionFolder, ".."));

        public string EnvironmentVariableName = "";
        public string SchemaName = "";
        public string SolutionFolder = "";
        public string SolutionName = "";
        public string DataNamespace = "";
        public string WebNamespace = "";

        public bool SolutionExists = false;
        public bool DataExists = false;
        public bool WebExists = false;

        public bool OverwriteData = false;
        public bool OverwriteWeb = false;
        public bool OverwriteSolution = false;

        public bool CreateSolution => (SolutionExists == false) || OverwriteSolution;
        public bool CreateData => (DataExists == false) || OverwriteData;
        public bool CreateWeb => (WebExists == false) || OverwriteWeb;

        public string ConnectionString => Environment.GetEnvironmentVariable(EnvironmentVariableName) ?? string.Empty;
        public bool IncludeWebProject => CreateWeb && WebNamespace.HasValue();
        public string DataProjectFolder => Path.Join(SolutionFolder, $"{DataNamespace}");
        public string WebProjectFolder => Path.Join(SolutionFolder, $"{WebNamespace}");

        public string SolutionFile => Path.Join(SolutionFolder, $"{SolutionName}.sln");
        public string DataProjectFile => Path.Join(DataProjectFolder, $"{DataNamespace}.csproj");
        public string WebProjectFile => Path.Join(WebProjectFolder, $"{WebNamespace}.csproj");
    }
}
