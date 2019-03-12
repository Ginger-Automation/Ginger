#region License
/*
Copyright © 2014-2019 European Support Limited

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

using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Repository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GingerCoreNET.SolutionRepositoryLib.UpgradeLib
{
    public class SolutionUpgrade
    {
        public static IEnumerable<RepositoryFile> GetAllToUpgrade(IEnumerable<RepositoryFile> RepositoryFiles)
        {
            // need ConcurrentBag - since it is thread safe when running in parallel then no need to use lock
            ConcurrentBag<RepositoryFile> filesToUpgrade = new ConcurrentBag<RepositoryFile>();            
            
            Parallel.ForEach(RepositoryFiles, RF =>
            {
                string fileVer = string.Empty;

                if (CompareRepoFileVersionToCurrent(RF.FilePath, ref fileVer) == -1)  // TODO: use enum !!!!!!!!!!!!!!!!!!
                {                    
                    filesToUpgrade.Add(RF);                    
                }
            });

            return filesToUpgrade.ToArray();
        }

        /// <summary>
        /// Checks the Ginger version in the file and compare it to current Ginger version.
        /// returns 0 if file was created with the same version, -1 if created with lower version and 1 if created with newer version
        /// </summary>
        /// <param name="FileName"></param>
        /// <returns></returns>
        public static int CompareRepoFileVersionToCurrent(string FileName, ref string fileVersion)
        {
            //TODO: return code should be enum not int to make it easy read
            string line1, line2;
            using (StreamReader reader = new StreamReader(FileName))
            {
                line1 = reader.ReadLine();
                line2 = reader.ReadLine();

                long currentVersion = GingerVersion.GetCurrentVersionAsLong();
                fileVersion = GetXMLVersion(line2);
                long fileXmlVersion = GetXMLVersionAsLong(line2);

                if (currentVersion == fileXmlVersion)
                    return 0; //same version
                else if (currentVersion > fileXmlVersion)
                    return -1; //File is from lower version
                else if (currentVersion < fileXmlVersion)
                    return 1; //File is from newer version
                else
                    return -2;//failed to identify and compare the version
            }
        }

       

        public static long GetXMLVersionAsLong(string xml)
        {
            /* Expecting the 1st comment in file to contain build info and 
            * expecting  comment to look this: */
            int iMajor = 0, iMinor = 0, iBuild = 0, iRevision = 0;
            int i1 = xml.IndexOf("<!--Ginger Repository Item created with version: ");
            int i2 = xml.IndexOf("-->");

            try
            {
                string BuildInfo = xml.Substring(i1, i2 - i1);
                Regex regex = new Regex(@"(\d+)\.(\d+)\.(\d+)\.(\d+)");
                Match match = regex.Match(BuildInfo);
                if (match.Success)
                {
                    // TODO: add error messages for failures here
                    // extract build info
                    try { iMajor = Int32.Parse(match.Groups[1].Value); }
                    catch (Exception) { }
                    try { iMinor = Int32.Parse(match.Groups[2].Value); }
                    catch (Exception) { }
                    try { iBuild = Int32.Parse(match.Groups[3].Value); }
                    catch (Exception) { }
                    try { iRevision = Int32.Parse(match.Groups[4].Value); }
                    catch (Exception) { }
                }
                else
                {
                    //TODO: handle when regex match fails completely
                }
                long version = iMajor * 1000000 + iMinor * 10000 + iBuild * 100 + iRevision;
                return version;
            }
            catch (Exception)
            {
                return 10203;
            }
        }

        public static string GetXMLVersion(string xml)
        {
            /* Expecting the 1st comment in file to contain build info and 
            * expecting  comment to look this: */
            int i1 = xml.IndexOf("<!--Ginger Repository Item created with version: ");
            int i2 = xml.IndexOf("-->");
            try
            {
                string BuildInfo = xml.Substring(i1, i2 - i1);
                Regex regex = new Regex(@"(\d+)\.(\d+)\.(\d+)\.(\d+)");
                Match match = regex.Match(BuildInfo);
                if (match.Success)
                {
                    return match.Value;
                }
                else
                {
                    return "0.0.0.0";
                }
            }
            catch (Exception)
            {
                return "0.0.0.0";
            }
        }
    }
}
