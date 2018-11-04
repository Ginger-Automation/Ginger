#region License
/*
Copyright © 2014-2018 European Support Limited

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
using System.Text;

namespace Amdocs.Ginger.IO
{
    public static class PathHelper
    {
        private const int MAX_PATH = 260;

        public static string GetLongPath(string path)
        {
            // in case we got short name like from open dialog, convert it to full long path
            string longPath = Path.GetFullPath(path);
            if (longPath.Length < MAX_PATH)
            {
                return path;
            }
            else
            {
                Console.WriteLine("Found long path - " + path.Length + " "  + path);
                return LongPath(longPath);
            }

            
        }

        //public static string DirectoryName(string path)
        //{
        //    return path;
        //}

        private static string LongPath(string path)
        {

            if (path.StartsWith(@"\\?\")) return path;

            var newpath = path;
            if (newpath.StartsWith("\\"))
            {
                newpath = @"\\?\UNC\" + newpath.Substring(2);
            }
            else if (newpath.Contains(":"))
            {
                newpath = @"\\?\" + newpath;
            }
            else
            {
                var currdir = Environment.CurrentDirectory;
                newpath = Combine(currdir, newpath);
                while (newpath.Contains("\\.\\")) newpath = newpath.Replace("\\.\\", "\\");
                newpath = @"\\?\" + newpath;
            }
            return newpath.TrimEnd('.');
        }

        private static string Combine(string path1, string path2)
        {
            return path1.TrimEnd('\\') + "\\" + path2.TrimStart('\\').TrimEnd('.');
        }

        public static string CleanInValidPathChars(string folderName)
        {
            foreach (char invalidChar in System.IO.Path.GetInvalidFileNameChars())
            {
                folderName = folderName.Replace(invalidChar.ToString(), "");
            }
            folderName = folderName.Replace(@".", "");
            folderName = folderName.TrimEnd().TrimEnd('-').TrimEnd();
            if (folderName.Length > 30)
            {
                folderName = folderName.Substring(0, 30);
            }
            folderName = folderName.TrimEnd().TrimEnd('-').TrimEnd();
            return folderName;
        }

    }
}
