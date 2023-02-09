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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository;
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
            solutionFilesWithVersion = new ConcurrentBag<Tuple<eGingerVersionComparisonResult, string>>();
        }

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

                long currentVersion = Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.ConvertApplicationVersionToLong(Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.ApplicationMajorVersion);
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


        static ConcurrentBag<Tuple<eGingerVersionComparisonResult, string>> solutionFilesWithVersion = new ConcurrentBag<Tuple<eGingerVersionComparisonResult, string>>();

        /// <summary>
        /// Return list of Solution files path with their version compare result
        /// </summary>
        /// <param name="solutionFiles"></param>
        /// <param name="addInfoExtention"></param>
        /// <returns></returns>
        public static ConcurrentBag<Tuple<eGingerVersionComparisonResult, string>> GetSolutionFilesWithVersion(IEnumerable<string> solutionFiles, bool addInfoExtention = true)
        {            
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
        internal static bool IsGingerUpgradeNeeded(string solutionFolder, IEnumerable<string> solutionFiles)
        {                        
            ConcurrentBag<Tuple<eGingerVersionComparisonResult, string>> solutionFilesWithVersion = null;

            //check if Ginger Upgrade is needed for loading this Solution
            try
            {
                //Reporter.ToLog(eLogLevel.INFO, "Checking if Ginger upgrade is needed for loading the Solution");
                if (solutionFilesWithVersion == null)
                {
                    solutionFilesWithVersion = SolutionUpgrade.GetSolutionFilesWithVersion(solutionFiles);
                }
                ConcurrentBag<string> higherVersionFiles = SolutionUpgrade.GetSolutionFilesCreatedWithRequiredGingerVersion(solutionFilesWithVersion, eGingerVersionComparisonResult.HigherVersion);
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
                if (solutionFilesWithVersion == null)
                {
                    solutionFilesWithVersion = SolutionUpgrade.GetSolutionFilesWithVersion(solutionFiles);
                }
                ConcurrentBag<string> lowerVersionFiles = SolutionUpgrade.GetSolutionFilesCreatedWithRequiredGingerVersion(solutionFilesWithVersion, eGingerVersionComparisonResult.LowerVersion);
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

            long fileVersionAsLong = Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.ConvertApplicationVersionToLong(fileGingerVersion);
            long currentVersionAsLong = Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.ConvertApplicationVersionToLong(Amdocs.Ginger.Common.GeneralLib.ApplicationInfo.ApplicationMajorVersion);

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
                    Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to get the Ginger Version of the file: '{0}'", xmlFilePath));
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
            if (xml.Contains("<!--Ginger Repository Item ")) return true;
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

                string BuildInfo = xml.Substring(i1, i2 - i1);
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
                            counter++;
                        if (counter == 2)
                            return ver + ".0.0";
                        else
                            ver += match.Value[indx];
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
                if (WorkSpace.Instance.UserProfile.DoNotAskToUpgradeSolutions == false && WorkSpace.Instance.RunningInExecutionMode == false && WorkSpace.Instance.RunningFromUnitTest == false)
                {
                    Reporter.ToLog(eLogLevel.INFO, "Checking is Solution Items Upgrade is needed");
                    if (solutionFilesWithVersion == null)
                    {
                        solutionFilesWithVersion = SolutionUpgrade.GetSolutionFilesWithVersion(solutionFiles);
                    }
                    ConcurrentBag<string> lowerVersionFiles = SolutionUpgrade.GetSolutionFilesCreatedWithRequiredGingerVersion(solutionFilesWithVersion, eGingerVersionComparisonResult.LowerVersion);
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
