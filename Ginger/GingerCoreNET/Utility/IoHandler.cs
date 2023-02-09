#region License
/*
Copyright © 2014-2023 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using Amdocs.Ginger.Common;
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
            //ensuring the destination paths are as per od standards
            destinationFolder = destinationFolder.Replace('/', Path.DirectorySeparatorChar);
            destinationFolder = destinationFolder.Replace('\\', Path.DirectorySeparatorChar);

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

        public void DeleteFoldersData(string folderPath)
        {
            DirectoryInfo dir = new DirectoryInfo(folderPath);
            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.Delete();
            }
        }

        public void TryFolderDelete(string folderToDelete)
        {
            try
            {
                Directory.Delete(folderToDelete, true);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "TryFolderDelete error - " + ex.Message, ex);
            }
        }
    }
}
