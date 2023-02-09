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
using System.Collections.Generic;
using System.IO;

namespace GingerHelper
{
    public class GingerHelperHandler
    {
        public static bool CreateLibrary(string sourcePath, string targetPath, List<string> IgnorExtentions, bool? deleteExitingContentBeforeStart, ref String error)
        {
            try
            {
                //delete current
                if (deleteExitingContentBeforeStart == true && Directory.Exists(targetPath))
                {
                    Directory.Delete(targetPath, true);                    
                }
                Directory.CreateDirectory(targetPath);

                //copy root files
                CopyFiles(sourcePath, targetPath, IgnorExtentions);

                //create sub directories
                foreach (string dir in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dir.Replace(sourcePath, targetPath));

                    //copy sub directories files
                    CopyFiles(dir, dir.Replace(sourcePath, targetPath), IgnorExtentions);
                }
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static void CopyFiles(string sourcePath, string destinationPath, List<string> IgnorExtentions)
        {
            foreach(string file in Directory.GetFiles(sourcePath))
            {
                bool unWantedFile = false;

                if (file.Contains("~$")) continue;

                foreach (string extenationToIgnore in IgnorExtentions)
                    if (extenationToIgnore.ToUpper().Trim() == Path.GetExtension(file).ToUpper() || extenationToIgnore.ToUpper().Trim() == Path.GetExtension(file).ToUpper().TrimStart(new char[] { '.' }))
                    {
                        unWantedFile = true;
                        break;
                    }

                if (unWantedFile == false)
                    File.Copy(file, Path.Combine(destinationPath, Path.GetFileName(file)), true);
            }
        }
    }
}