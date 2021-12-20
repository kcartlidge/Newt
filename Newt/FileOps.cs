using System;
using System.IO;

namespace Newt
{
    internal static class FileOps
    {
        /// <summary>Removes all files/subfolders from the given folder.</summary>
        public static void ClearFolder(string folder)
        {
            Console.WriteLine(folder);
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
    }
}

