#region License
/*
Copyright © 2014-2025 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public enum SolutionUpgradePageViewMode
{
    UpgradeSolution,
    UpgradeGinger,
    FailedUpgradeSolution
}

public enum eGingerVersionComparisonResult
{
    LowerVersion,
    SameVersion,
    HigherVersion,
    ComparisonFailed
}


// TODO: change all static to regular method so can be reused better !!!!!!

namespace GingerCoreNET.SolutionRepositoryLib.UpgradeLib
{
    public class SolutionUpgrade
    {
        public static void ClearPreviousScans()
        {
            solutionFilesWithVersion = null;
        }

        static ConcurrentBag<Tuple<eGingerVersionComparisonResult, string>> solutionFilesWithVersion = [];

        /// <summary>
        /// Return list of Solution files path with their version compare result
        /// </summary>
        /// <param name="solutionFiles"></param>
        /// <param name="addInfoExtention"></param>
        /// <returns></returns>
        public static ConcurrentBag<Tuple<eGingerVersionComparisonResult, string>> GetSolutionFilesWithVersion(IEnumerable<string> solutionFiles, bool addInfoExtention = true)
        {
            if (solutionFilesWithVersion == null)
            {
                solutionFilesWithVersion = [];

                // read all XMLs and check for version
                Parallel.ForEach(solutionFiles, FileName =>
                    {
                        string fileVer = string.Empty;
                        eGingerVersionComparisonResult versionRes = CompareSolutionFileGingerVersionToCurrent(FileName, ref fileVer);

                        if (addInfoExtention)
                        {
                            solutionFilesWithVersion.Add(Tuple.Create(versionRes, FileName + "--> File Version: " + Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.ConvertBackendApplicationVersionToUIVersion(fileVer)));
                        }
                        else
                        {
                            solutionFilesWithVersion.Add(Tuple.Create(versionRes, FileName));
                        }
                    });
            }

            return solutionFilesWithVersion;
        }

        internal static bool IsGingerUpgradeNeeded(string solutionFolder, IEnumerable<string> solutionFiles)
        {
            //check if Ginger Upgrade is needed for loading this Solution
            try
            {
                //Reporter.ToLog(eLogLevel.INFO, "Checking if Ginger upgrade is needed for loading the Solution");
                ConcurrentBag<string> higherVersionFiles = SolutionUpgrade.GetSolutionFilesCreatedWithRequiredGingerVersion(SolutionUpgrade.GetSolutionFilesWithVersion(solutionFiles), eGingerVersionComparisonResult.HigherVersion);
                if (higherVersionFiles.Count > 0)
                {
                    if (WorkSpace.Instance.RunningInExecutionMode == false && WorkSpace.Instance.RunningFromUnitTest == false)
                    {
                        WorkSpace.Instance.EventHandler.ShowUpgradeGinger(solutionFolder, higherVersionFiles.ToList());
                    }
                    Reporter.ToLog(eLogLevel.WARN, "Ginger upgrade is needed for loading the Solution, aborting Solution load.");
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while checking if Solution requires Ginger Upgrade", ex);
                return false;
            }
        }
        internal static bool IsUserProceedWithLoadSolutionInNewerGingerVersion(string solutionFolder, IEnumerable<string> solutionFiles)
        {
            ConcurrentBag<Tuple<eGingerVersionComparisonResult, string>> solutionFilesWithVersion = null;
            try
            {
                ConcurrentBag<string> lowerVersionFiles = SolutionUpgrade.GetSolutionFilesCreatedWithRequiredGingerVersion(SolutionUpgrade.GetSolutionFilesWithVersion(solutionFiles), eGingerVersionComparisonResult.LowerVersion);
                if (lowerVersionFiles.Count > 0 && lowerVersionFiles.Any(x => x.Contains("Ginger.Solution.xml")))
                {
                    if (WorkSpace.Instance.RunningInExecutionMode == false && WorkSpace.Instance.RunningFromUnitTest == false)
                    {
                        if (Reporter.ToUser(eUserMsgKey.SolutionOpenedOnNewerVersion) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
                        {
                            return true;
                        }
                        Reporter.ToLog(eLogLevel.WARN, "User declined to load the Solution on newer Ginger version, aborting Solution load.");
                        return false;
                    }
                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while checking if the user wants to proceed with loading the Solution on Higher Ginger version", ex);
                return false;
            }
        }


        /// <summary>
        /// Return list of the Solution files paths which match to specific result type
        /// </summary>
        /// <param name="solutionFiles"></param>
        /// <returns></returns>
        public static ConcurrentBag<string> GetSolutionFilesCreatedWithRequiredGingerVersion(ConcurrentBag<Tuple<eGingerVersionComparisonResult, string>> solutionFilesWithVersionCompare, eGingerVersionComparisonResult requiredVersion)
        {
            // read all XMLs and check for version

            ConcurrentBag<string> requiredFiles = [];

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

            long fileVersionAsLong = Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.ConvertApplicationVersionToLong(fileGingerVersion);
            long currentVersionAsLong = Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.ConvertApplicationVersionToLong(Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.ApplicationBackendVersion);

            if (fileVersionAsLong == 0)
            {
                Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to read and compare the Ginger version for the file: '{0}'", filePath));
                return eGingerVersionComparisonResult.ComparisonFailed; ;//failed to identify and compare the version
            }
            else if (currentVersionAsLong == fileVersionAsLong)
            {
                return eGingerVersionComparisonResult.SameVersion; //same version
            }
            else if (currentVersionAsLong > fileVersionAsLong)
            {
                return eGingerVersionComparisonResult.LowerVersion; //File is from lower version
            }
            else if (currentVersionAsLong < fileVersionAsLong)
            {
                return eGingerVersionComparisonResult.HigherVersion; //File is from newer version
            }
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
                try
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
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to get the Ginger Version of the file: '{0}'", xmlFilePath), ex);
                    return null;
                }
            }

            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            //get the version based on XML type (new/old repository item type)
            if (string.IsNullOrEmpty(xml) == false)
            {
                if (IsLegacyXmlType(xml) == true)
                {
                    fileVersion = GetLegacyXMLGingerVersion(xml, xmlFilePath);
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
                {
                    Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to get the Ginger Version of the file: '{0}'", xmlFilePath));
                }

                return fileVersion;
            }
            else
            {
                Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to get the Ginger Version of the file: '{0}'", xmlFilePath));
                return null;
            }
        }

        public static bool IsLegacyXmlType(string xml)
        {
            if (xml.Contains("<!--Ginger Repository Item "))
            {
                return true;
            }

            return false;
        }
        public static string GetLegacyXMLGingerVersion(string xml, string xmlFilePath)
        {
            try
            {
                /* Expecting the 1st comment in file to contain build info and 
                * expecting  comment to look this: 
                * <!--Ginger Repository Item created with version: 0.1.2.3 -->*/
                int i1 = xml.IndexOf("<!--Ginger Repository Item created with version: ");
                int i2 = xml.IndexOf("-->");

                string BuildInfo = xml[i1..i2];
                Regex regex = new Regex(@"(\d+)\.(\d+)\.(\d+)\.(\d+)");
                Match match = regex.Match(BuildInfo);
                if (match.Success)
                {
                    //avoiding Beta + Alpha numbers because for now it is not supposed to be writen to XML's, only oficial release numbers
                    int counter = 0;
                    string ver = string.Empty;
                    for (int indx = 0; indx < match.Value.Length; indx++)
                    {
                        if (match.Value[indx] == '.')
                        {
                            counter++;
                        }

                        if (counter == 2)
                        {
                            return ver + ".0.0";
                        }
                        else
                        {
                            ver += match.Value[indx];
                        }
                    }
                    return ver;//something wronge
                }
                else
                {
                    Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to get the XML Ginger version of the XML at path = '{0}'", xmlFilePath));
                    return null;//failed to get the version
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to get the XML Ginger version of the XML at path = '{0}'", xmlFilePath));
                Console.WriteLine(ex.StackTrace);
                return null;//failed to get the version
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
            return Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.ConvertApplicationVersionToLong(fileGingerVersion);
        }


        // Offer to upgrade Solution items to current version
        internal static void CheckSolutionItemsUpgrade(string solutionFolder, string solutionName, List<string> solutionFiles)
        {

            try
            {
                if (WorkSpace.Instance.UserProfile.DoNotAskToUpgradeSolutions == false && WorkSpace.Instance.RunningInExecutionMode == false && WorkSpace.Instance.RunningFromUnitTest == false && Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.IsOfficialRelease)
                {
                    Reporter.ToLog(eLogLevel.INFO, "Checking is Solution Items Upgrade is needed");
                    ConcurrentBag<string> lowerVersionFiles = SolutionUpgrade.GetSolutionFilesCreatedWithRequiredGingerVersion(SolutionUpgrade.GetSolutionFilesWithVersion(solutionFiles), eGingerVersionComparisonResult.LowerVersion);
                    if (lowerVersionFiles != null && lowerVersionFiles.Count > 0)
                    {
                        WorkSpace.Instance.EventHandler.ShowUpgradeSolutionItems(SolutionUpgradePageViewMode.UpgradeSolution, solutionFolder, solutionName, lowerVersionFiles.ToList());

                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while checking if Solution files should be Upgraded", ex);
            }
        }

    }
}
