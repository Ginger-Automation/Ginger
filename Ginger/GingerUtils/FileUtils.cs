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

using System.IO;

namespace GingerUtils
{
    public class FileUtils
    {
        public static string RemoveInvalidChars(string fileName)
        {
            string invalidChars = new string(Path.GetInvalidFileNameChars());

            foreach (char c in invalidChars)
            {
                fileName = fileName.Replace(c.ToString(), "_");
            }

            return fileName;
        }
        /// <summary>
        /// Generic funtion to rename the file with counter 
        /// </summary>
        /// <param name="sourceFilePath">File to rename</param>
        /// <param name="targetDirectory">target directory where renamed file need to be saaved</param>
        /// <param name="extension">Optional extension for renamed file, default is old</param>
        /// <returns></returns>
        public static bool RenameFile(string sourceFilePath, string targetDirectory, string extension= "Old")
        {
            try
            {
                // Get the target file name from the source file path
                string sourceFileName = Path.GetFileName(sourceFilePath);

                // Check if the target file name already exists in the directory
                string targetFilePath = Path.Combine(targetDirectory, sourceFileName);
                int counter = 1;
                while (File.Exists(targetFilePath))
                {
                    // Generate a new unique file name by appending a counter
                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceFileName);
                    string fileExtension = Path.GetExtension(sourceFileName);
                    sourceFileName = $"{fileNameWithoutExtension}_{extension}_{counter}{fileExtension}";
                    targetFilePath = Path.Combine(targetDirectory, sourceFileName);
                    counter++;
                }

                // Rename the file to the new unique name
                File.Move(sourceFilePath, targetFilePath);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
