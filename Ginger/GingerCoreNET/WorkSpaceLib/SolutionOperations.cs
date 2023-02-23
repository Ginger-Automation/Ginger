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
using Amdocs.Ginger.Common.OS;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.CoreNET.Run.SolutionCategory;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using GingerCore;
using GingerCore.Platforms;
using GingerCore.Variables;
using GingerCoreNET.ALMLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerCoreNET.SourceControl;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.SolutionGeneral
{
    public class SolutionOperations : ISolutionOperations
    {
        public Solution Solution;
        public SolutionOperations(Solution Solution)
        {
            this.Solution = Solution;
            this.Solution.SolutionOperations = this;
        }

        public static Solution LoadSolution(string solutionFileName, bool startDirtyTracking = true, string encryptionKey = null)
        {
            string txt = File.ReadAllText(solutionFileName);
            Solution solution = (Solution)NewRepositorySerializer.DeserializeFromText(txt);
            solution.FilePath = solutionFileName;
            solution.Folder = Path.GetDirectoryName(solutionFileName);
            solution.EncryptionKey = encryptionKey ?? GetEncryptionKey(solution.Guid.ToString());
            if (startDirtyTracking)
            {
                solution.StartDirtyTracking();
            }
            return solution;
        }

        public void SaveSolution(bool showWarning = true, Solution.eSolutionItemToSave solutionItemToSave = Solution.eSolutionItemToSave.GeneralDetails)
        {
            bool doSave = false;

            if (!showWarning)
            {
                doSave = true;
            }
            else
            {
                Solution lastSavedSolution = LoadSolution(Solution.FilePath, false);
                string extraChangedItems = "";
                StringBuilder bldExtraChangedItems = new StringBuilder();

                if (solutionItemToSave != Solution.eSolutionItemToSave.GeneralDetails)
                {
                    if (Solution.Name != lastSavedSolution.Name || Solution.Account != lastSavedSolution.Account)
                    {
                        bldExtraChangedItems.Append("Solution General Details, ");
                    }
                }
                if (solutionItemToSave != Solution.eSolutionItemToSave.ALMSettings)
                {
                    if (!Solution.ALMConfigs.Equals(lastSavedSolution.ALMConfigs))
                    {
                        bldExtraChangedItems.Append("ALM Details, ");
                    }
                }
                if (solutionItemToSave != Solution.eSolutionItemToSave.SourceControlSettings)
                {
                    if (Solution.SourceControl != lastSavedSolution.SourceControl)
                    {
                        bldExtraChangedItems.Append("Source Control Details, ");
                    }
                }
                if (solutionItemToSave != Solution.eSolutionItemToSave.LoggerConfiguration)
                {
                    if (Solution.LoggerConfigurations != null)
                    {
                        if (Solution.LoggerConfigurations.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified || Solution.LoggerConfigurations.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.NoTracked)
                        {
                            bldExtraChangedItems.Append("Execution Logger configuration, ");
                        }
                    }
                }
                if (solutionItemToSave != Solution.eSolutionItemToSave.ReportConfiguration)
                {
                    if (Solution.HTMLReportsConfigurationSetList != null && lastSavedSolution.HTMLReportsConfigurationSetList.Count != 0)
                    {
                        foreach (HTMLReportsConfiguration config in Solution.HTMLReportsConfigurationSetList)
                        {
                            if (config.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified || config.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.NoTracked)
                            {

                                bldExtraChangedItems.Append("Report configuration");
                                break;
                            }
                        }
                    }
                }

                if (solutionItemToSave != Solution.eSolutionItemToSave.GlobalVariabels)
                {
                    if (Solution.Variables.Count != lastSavedSolution.Variables.Count)
                    {
                        bldExtraChangedItems.Append(GingerDicser.GetTermResValue(eTermResKey.Variables, "Global ", ", "));
                    }
                    else
                    {
                        foreach (VariableBase var in Solution.Variables)
                        {
                            if (var.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified || var.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.NoTracked)
                            {
                                bldExtraChangedItems.Append(GingerDicser.GetTermResValue(eTermResKey.Variables, "Global ", ", "));
                                break;
                            }
                        }
                    }
                }
                if (solutionItemToSave != Solution.eSolutionItemToSave.TargetApplications)
                {
                    if (Solution.ApplicationPlatforms.Count != lastSavedSolution.ApplicationPlatforms.Count)
                    {
                        bldExtraChangedItems.Append("Target Applications, ");
                    }
                    else
                    {
                        foreach (ApplicationPlatform app in Solution.ApplicationPlatforms)
                        {
                            if (app.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified || app.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.NoTracked)
                            {
                                bldExtraChangedItems.Append("Target Applications, ");
                                break;
                            }
                        }
                    }
                }
                if (solutionItemToSave != Solution.eSolutionItemToSave.Tags)
                {
                    if (Solution.Tags.Count != lastSavedSolution.Tags.Count)
                    {
                        bldExtraChangedItems.Append("Tags, ");
                    }
                    else
                    {
                        foreach (RepositoryItemTag tag in Solution.Tags)
                        {
                            if (tag.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified || tag.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.NoTracked)
                            {
                                bldExtraChangedItems.Append("Tags, ");
                                break;
                            }
                        }
                    }
                }
                extraChangedItems = bldExtraChangedItems.ToString();
                if (string.IsNullOrEmpty(extraChangedItems))
                {
                    doSave = true;
                }
                else
                {
                    extraChangedItems = extraChangedItems.TrimEnd();
                    extraChangedItems = extraChangedItems.TrimEnd(new char[] { ',' });
                    if (Reporter.ToUser(eUserMsgKey.SolutionSaveWarning, extraChangedItems) == eUserMsgSelection.Yes)
                    {
                        doSave = true;
                    }
                }
            }

            if (doSave)
            {
                Reporter.ToStatus(eStatusMsgKey.SaveItem, null, "Solution Configurations", "item");
                Solution.RepositorySerializer.SaveToFile(Solution, Solution.FilePath);
                Solution.SetDirtyStatusToNoChange();
                Reporter.HideStatusMessage();
                if (WorkSpace.Instance.SolutionRepository != null && WorkSpace.Instance.SolutionRepository.ModifiedFiles.Contains(Solution))
                {
                    WorkSpace.Instance.SolutionRepository.ModifiedFiles.Remove(Solution);
                }
            }
        }



        /// <summary>
        /// For encrypting password variables
        /// </summary>


        public bool ValidateKey(string encryptionKey = null)
        {
            try
            {
                bool isDecrypted = EncryptionHandler.DecryptwithKey(Solution.EncryptedValidationString, encryptionKey ?? Solution.EncryptionKey).Equals("valid");
                if (isDecrypted)
                {
                    Solution.EncryptionKey = encryptionKey ?? (Solution.EncryptionKey ?? EncryptionHandler.GetDefaultKey());
                }
                return isDecrypted;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, ex.Message);
            }
            return false;
        }

        public bool AddValidationString()
        {
            try
            {
                Solution.EncryptedValidationString = EncryptionHandler.EncryptwithKey("valid");
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, ex.Message);
            }
            return false;
        }

        public static string GetEncryptionKey(string guid)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return GingerCore.GeneralLib.WinCredentialUtil.GetCredential("Ginger_Sol_" + guid);
                }
                else
                {
                    Reporter.ToLog(eLogLevel.WARN, "Encryption Key was not found in OS passwords store");
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.WARN, "Failed to get Solution Encryption Key", ex);
            }
            return null;
        }

        public bool FetchEncryptionKey()
        {
            try
            {
                Solution.EncryptionKey = GingerCore.GeneralLib.WinCredentialUtil.GetCredential("Ginger_Sol_" + Solution.Guid);
                return string.IsNullOrEmpty(Solution.EncryptionKey);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, ex.Message);
            }
            return false;
        }

        public bool SaveEncryptionKey()
        {
            try
            {
                GingerCore.GeneralLib.WinCredentialUtil.SetCredentials("Ginger_Sol_" + Solution.Guid, Solution.Name, Solution.EncryptionKey);
                EncryptionHandler.SetCustomKey(Solution.EncryptionKey);
                AddValidationString();
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, ex.Message);
            }
            return false;
        }



        public void SetReportsConfigurations()
        {
            try
            {
                if (Solution.LoggerConfigurations == null || Solution.LoggerConfigurations.ExecutionLoggerConfigurationExecResultsFolder == null)
                {
                    Solution.LoggerConfigurations = new ExecutionLoggerConfiguration();
                    Solution.LoggerConfigurations.IsSelected = true;
                    Solution.LoggerConfigurations.ExecutionLoggerConfigurationIsEnabled = true;
                    Solution.LoggerConfigurations.ExecutionLoggerConfigurationMaximalFolderSize = 250;
                    Solution.LoggerConfigurations.ExecutionLoggerConfigurationExecResultsFolder = SolutionRepository.cSolutionRootFolderSign + "ExecutionResults";
                }

                if ((Solution.HTMLReportsConfigurationSetList == null) || (Solution.HTMLReportsConfigurationSetList.Count == 0))
                {
                    Solution.HTMLReportsConfigurationSetList = new ObservableList<HTMLReportsConfiguration>();
                    HTMLReportsConfiguration HTMLReportsConfiguration = new HTMLReportsConfiguration();
                    HTMLReportsConfiguration.IsSelected = true;
                    HTMLReportsConfiguration.HTMLReportTemplatesSeq = 1;
                    HTMLReportsConfiguration.HTMLReportsFolder = SolutionRepository.cSolutionRootFolderSign + "HTMLReports";
                    HTMLReportsConfiguration.HTMLReportsAutomaticProdIsEnabled = false;
                    Solution.HTMLReportsConfigurationSetList.Add(HTMLReportsConfiguration);
                }

                Solution.LoggerConfigurations.CalculatedLoggerFolder = Solution.LoggerConfigurations.ExecutionLoggerConfigurationExecResultsFolder;
                Ginger.Reports.GingerExecutionReport.ExtensionMethods.GetSolutionHTMLReportConfigurations();
                ExecutionLoggerConfiguration executionLoggerConfiguration = Solution.LoggerConfigurations;


                // !!!!!!!!!!!!! FIXME
                // ExecutionLogger executionLogger = App.AutomateTabGingerRunner.ExecutionLogger;
                // executionLogger.Configuration = executionLoggerConfiguration;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public string ConvertSolutionRelativePath(string relativePath)
        {
            if (String.IsNullOrWhiteSpace(relativePath))
            {
                return relativePath;
            }
            try
            {
                if (relativePath.TrimStart().StartsWith("~"))
                {
                    string fullPath = relativePath.TrimStart(new char[] { '~', '\\', '/' });
                    fullPath = Path.Combine(WorkSpace.Instance.SolutionRepository.SolutionFolder, fullPath);
                    return OperatingSystemBase.CurrentOperatingSystem.AdjustFilePath(fullPath);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Failed to replace relative path sign '~' with Solution path for the path: '" + relativePath + "'", ex);
            }

            return OperatingSystemBase.CurrentOperatingSystem.AdjustFilePath(relativePath);
        }

    }
}
