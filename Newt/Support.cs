using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Newt
{
    /// <summary>Miscellaneous support functionality.</summary>
    internal static class Support
    {
        /// <summary>
        /// Removes all files/subfolders from the given folder.
        /// It also ensures the folder exists (whilst being empty).
        /// </summary>
        public static void ClearFolder(string folder)
        {
            Console.WriteLine($"CLEARING {folder}");
            if (Directory.Exists(folder) == false) Directory.CreateDirectory(folder);
            if (Directory.Exists(folder) == false) throw new Exception($"Failed to create folder: {folder}");

            foreach (var file in Directory.GetFiles(folder)) File.Delete(file);

            var subfolders = Directory.GetDirectories(folder);
            foreach (var subfolder in subfolders) Directory.Delete(subfolder, true);
        }

        /// <summary>If the file exists in the folder, deletes it.</summary>
        public static void RemoveFile(string topFolder, string filename)
        {
            var fullName = Path.Combine(topFolder, filename);
            if (File.Exists(fullName)) File.Delete(fullName);
        }

        /// <summary>
        /// Combines the subfolders to make a single path and creates it
        /// if it does not already exist.
        /// </summary>
        /// <param name="baseFolder">The target folder path.</param>
        /// <param name="title">Display name for the path.</param>
        /// <param name="routeDown">Sequential segments of the path.</param>
        public static void EnsureFullPathExists(
            string baseFolder,
            params string[] routeDown)
        {
            if (routeDown.Any())
                foreach (var segment in routeDown)
                    baseFolder = Path.Join(baseFolder, segment);
            Directory.CreateDirectory(baseFolder);
        }

        /// <summary>
        /// Writes the content to the file. If the file exists,
        /// and the option to 'force' was not provided at construction,
        /// an exception is raised.
        /// </summary>
        public static void WriteFileWithChecks(
            string filename,
            bool force,
            string content)
        {
            Console.WriteLine(filename);
            if (force == false && File.Exists(filename))
                throw new Exception($"File exists: {filename}");
            File.WriteAllText(filename, content, Encoding.UTF8);
        }

        /// <summary>Runs a command.</summary>
        public static void RunCommand(
            string command,
            string args,
            string folder = "")
        {
            string discard = "";
            RunCommand(command, args, out discard, folder);
        }

        /// <summary>
        /// Runs a command. If provided, populates the stdout.
        /// </summary>
        public static void RunCommand(
            string command,
            string args,
            out string stdout,
            string folder = "")
        {
            var capture = "";
            var hadErrors = false;
            try
            {
                ProcessStartInfo si = new ProcessStartInfo(command, args)
                {
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                si.RedirectStandardInput = si.RedirectStandardOutput = si.RedirectStandardError = true;
                if (string.IsNullOrWhiteSpace(folder) == false) si.WorkingDirectory = folder;

                var p = new Process() { StartInfo = si };
                p.Start();

                p.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
                {
                    // Capture stdio.
                    if (string.IsNullOrWhiteSpace(e.Data) == false)
                        capture = e.Data.Trim();
                };
                p.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
                {
                    // Display stderr.
                    if (string.IsNullOrWhiteSpace(e.Data)) return;
                    hadErrors = true;
                    Console.WriteLine(e.Data);
                };
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.WaitForExit();
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception running shell command: {command} - {ex.Message}");
            }

            if (hadErrors)
            {
                throw new Exception($"Errors running command: {command}");
            }

            stdout = capture;
        }

        /// <summary>
        /// Shows the current dotnet version and the expected one.
        /// Throws an exception if the current version is too low.
        /// </summary>
        /// <param name="folder">
        /// Required to allow for global.json files.
        /// </param>
        public static void RequireVersion(
            string folder,
            int minMajor,
            int minMinor,
            int minPatch,
            string reason)
        {
            Console.WriteLine();
            Console.WriteLine("CHECKING DOTNET VERSION");
            Console.WriteLine(reason);

            var (major, minor, patch) = (0, 0, 0);
            try
            {
                // Gather the version details.
                var version = "";
                RunCommand("dotnet", "--version", out version, folder);
                var bits = version.Split(".");
                (major, minor, patch) = (int.Parse(bits[0]), int.Parse(bits[1]), int.Parse(bits[2]));
                Console.WriteLine($"Active dotnet version:    {major}.{minor}.{patch}");
                Console.WriteLine($"Minimum required version: {minMajor}.{minMinor}.{minPatch}");
            }
            catch (Exception)
            {
                // Assume the worst if the check fails.
                Console.WriteLine($"Failed to run `dotnet version`.");
                Console.WriteLine($"Minimum version {minMajor}.{minMinor}.{minPatch}");
                throw;
            }

            // Ensure we have a sufficient version.
            var minimum = $"{minMajor:0000}.{minMinor:0000}.{minPatch:0000}";
            var actual = $"{major:0000}.{minor:0000}.{patch:0000}";
            if (string.Compare(actual, minimum) == -1)
                throw new Exception($"Needs at least .NET {minMajor}.{minMinor}.{minPatch}");
        }
    }
}

