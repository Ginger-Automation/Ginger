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

using System;
using System.IO;
using System.Linq;

namespace Amdocs.Ginger.Repository
{
    // Helper to get files and directories from disk - need to use only this one for all!
    public class FileSystem
    {
        public static string[] GetDirectoryFiles(string path, string pattern = null)
        {
            if (!System.IO.Directory.Exists(path))
                return new string[0];
            string[] files = System.IO.Directory.GetFiles(path, pattern);
            // We return the list sorted 
            return files.OrderBy(x => x).ToArray();
        }

        public static string[] GetDirectorySubFolders(string path)
        {
            string[] folders = System.IO.Directory.GetDirectories(path);
            // We return the list sorted 
            return folders.OrderBy(x => x).ToArray();
        }


        /// <summary>
        /// Append a timestamp to file name while keeping the extension 
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>The file name with timestamp in format yyyymmddhhmmss for example: log_20180906075812</returns>

        public static string AppendTimeStamp(string fileName)
        {
            return string.Concat(
                Path.GetFileNameWithoutExtension(fileName), "_",
                DateTime.Now.ToString("yyyymmddhhmmss"),
                Path.GetExtension(fileName)
                );
        }

    }
}
