using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Utility
{
    public class IoHandler
    {
        private static readonly IoHandler _instance = new IoHandler();

        public static IoHandler Instance
        {
            get
            {
                return _instance;
            }
        }
        public void CopyFolderRec(string sourceFolder, string destinationFolder, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceFolder);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceFolder);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destinationFolder))
            {
                Directory.CreateDirectory(destinationFolder);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destinationFolder, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destinationFolder, subdir.Name);
                    CopyFolderRec(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}
