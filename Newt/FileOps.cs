using System;
using System.Diagnostics;
using System.IO;

namespace Newt
{
    internal static class FileOps
    {
        /// <summary>Removes all files/subfolders from the given folder.</summary>
        public static void ClearFolder(string folder)
        {
            Console.WriteLine(folder);
            if (Directory.Exists(folder) == false) Directory.CreateDirectory(folder);
            if (Directory.Exists(folder) == false) return;

            foreach (var file in Directory.GetFiles(folder))
                File.Delete(file);

            var subfolders = Directory.GetDirectories(folder);
            foreach (var subfolder in subfolders)
            {
                Console.WriteLine(subfolder);
                Directory.Delete(subfolder, true);
            }
        }

        /// <summary>If the file exists in the folder, deletes it.</summary>
        public static void RemoveFile(string topFolder, string filename)
        {
            var fullName = Path.Combine(topFolder, filename);
            if (File.Exists(fullName)) File.Delete(fullName);
        }

        /// <summary>Runs a command, and returns the stderr output.</summary>
        public static void RunCommand(string command, string args, string folder = "")
        {
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
                    // Ignore stdio; only interested in stderr.
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
        }
    }
}

