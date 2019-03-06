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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GingerCore.Repository.UpgradeLib
{
    public class SolutionUpgrade
    {
        public enum eGingerVersionComparisonResult
        {
            LowerVersion,
            SameVersion,
            HigherVersion,
            ComparisonFailed
        }

        /// <summary>
        /// Return list of Solution files path with their version compare result
        /// </summary>
        /// <param name="solutionFiles"></param>
        /// <param name="addInfoExtention"></param>
        /// <returns></returns>
        public static ConcurrentBag<Tuple<eGingerVersionComparisonResult, string>> GetSolutionFilesWithVersion(IEnumerable<string> solutionFiles, bool addInfoExtention = true)
        {
            ConcurrentBag<Tuple<eGingerVersionComparisonResult, string>> solutionFilesWithVersion = new ConcurrentBag<Tuple<eGingerVersionComparisonResult, string>>();
            // read all XMLs and check for version
            Parallel.ForEach(solutionFiles, FileName =>
            {
                string fileVer = string.Empty;
                eGingerVersionComparisonResult versionRes = CompareSolutionFileGingerVersionToCurrent(FileName, ref fileVer);
                
                if (addInfoExtention)
                    solutionFilesWithVersion.Add(Tuple.Create(versionRes, FileName + "--> File Version: " + fileVer));
                else
                    solutionFilesWithVersion.Add(Tuple.Create(versionRes, FileName));
            });

            return solutionFilesWithVersion;
        }

        /// <summary>
        /// Return list of the Solution files paths which match to specific result type
        /// </summary>
        /// <param name="solutionFiles"></param>
        /// <returns></returns>
        public static ConcurrentBag<string> GetSolutionFilesCreatedWithRequiredGingerVersion(ConcurrentBag<Tuple<eGingerVersionComparisonResult, string>> solutionFilesWithVersionCompare, eGingerVersionComparisonResult requiredVersion)
        {
            // read all XMLs and check for version
            
                ConcurrentBag<string> requiredFiles = new ConcurrentBag<string>();

                Parallel.ForEach(solutionFilesWithVersionCompare, solFile =>
                {
                    if (solFile.Item1 == requiredVersion)
                    {
                        requiredFiles.Add(solFile.Item2);
                    }
                });
            
            return requiredFiles;
        }

        /// <summary>
        /// Checks the Ginger version in the file and compare it to current Ginger version.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileGingerVersion"></param>
        /// <returns></returns>
        public static eGingerVersionComparisonResult CompareSolutionFileGingerVersionToCurrent(string filePath, ref string fileGingerVersion)
        {
            fileGingerVersion = GetSolutonFileGingerVersion(filePath);
            if (fileGingerVersion == null)
            {
                Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to read and compare the Ginger version for the file: '{0}'", filePath));
                return eGingerVersionComparisonResult.ComparisonFailed;//failed to identify and compare the version
            }

            if (fileGingerVersion == "3.0.0.0Beta")//Workaround needed due to move to new repository serializer in middle of release 
            {
                return eGingerVersionComparisonResult.LowerVersion;
            }

            long fileVersionAsLong = GingerCoreNET.GeneralLib.General.GetGingerVersionAsLong(fileGingerVersion);
            long currentVersionAsLong = RepositorySerializer.GetCurrentGingerVersionAsLong();

            if (fileVersionAsLong == 0)
            {
                Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to read and compare the Ginger version for the file: '{0}'", filePath));
                return eGingerVersionComparisonResult.ComparisonFailed; ;//failed to identify and compare the version
            }
            else if (currentVersionAsLong == fileVersionAsLong)
                return eGingerVersionComparisonResult.SameVersion; //same version
            else if (currentVersionAsLong > fileVersionAsLong)
                return eGingerVersionComparisonResult.LowerVersion; //File is from lower version
            else if (currentVersionAsLong < fileVersionAsLong)
                return eGingerVersionComparisonResult.HigherVersion; //File is from newer version
            else
            {
                Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to read and compare the Ginger version for the file: '{0}'", filePath));
                return eGingerVersionComparisonResult.ComparisonFailed;//failed to identify and compare the version
            }
        }

        /// <summary>
        /// Pull and return the Ginger Version (in String format) which the Solution file was created with 
        /// </summary>
        /// <param name="xmlFilePath"></param>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static string GetSolutonFileGingerVersion(string xmlFilePath, string xml = "")
        {
            string fileVersion;

            //get the XML if needed
            if (string.IsNullOrEmpty(xml))
            {                
                using (StreamReader reader = new StreamReader(xmlFilePath))
                {
                    //get XML 
                    reader.ReadLine();//no need first line
                    xml = reader.ReadLine();
                    if (xml != null)
                    {
                        if (xml.ToLower().Contains("version") == false)//to handle new line gap in some old xml's
                        {
                            xml = reader.ReadLine();
                        }
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to get the Ginger Version of the file: '{0}'", xmlFilePath));
                    }
                }
            }

            //get the version based on XML type (new/old repository item type)
            if (string.IsNullOrEmpty(xml) == false)
            {
                if (RepositorySerializer.IsLegacyXmlType(xml) == true)
                {
                    fileVersion = RepositorySerializer.GetXMLGingerVersion(xml, xmlFilePath);
                    if (fileVersion == "3.0.0.0")
                    {
                        fileVersion = fileVersion + "Beta";
                    }
                }
                else
                {
                    fileVersion = NewRepositorySerializer.GetXMLGingerVersion(xml, xmlFilePath);//New XML type
                }

                if (fileVersion == null)
                    Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to get the Ginger Version of the file: '{0}'", xmlFilePath));
                return fileVersion;
            }
            else
            {
                Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to get the Ginger Version of the file: '{0}'", xmlFilePath));
                return null;
            }
        }

        /// <summary>
        /// Pull and return the Ginger Version (in Long format) which the Solution file was created with 
        /// </summary>
        /// <param name="xmlFilePath"></param>
        /// <param name="xml"></param>
        /// <returns></returns>
        public static long GetSolutonFileGingerVersionAsLong(string xmlFilePath, string xml = "")
        {
            string fileGingerVersion = GetSolutonFileGingerVersion(xmlFilePath, xml);
            return GingerCoreNET.GeneralLib.General.GetGingerVersionAsLong(fileGingerVersion);           
        }
    }
}
