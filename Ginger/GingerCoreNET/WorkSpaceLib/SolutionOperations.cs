#region License
/*
Copyright © 2014-2021 European Support Limited

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
            //solution.SolutionOperations = this;
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


        //public ObservableList<TargetBase> GetSolutionTargetApplications()
        //{
        //    ObservableList<TargetBase> solTargetApplications = new ObservableList<TargetBase>();
        //    foreach (ApplicationPlatform app in ApplicationPlatforms)
        //    {
        //        solTargetApplications.Add(new TargetApplication() { AppName = app.AppName, Guid = app.Guid });
        //    }
        //    return solTargetApplications;
        //}

        // MRUManager mRecentUsedBusinessFlows;

        //public MRUManager RecentlyUsedBusinessFlows
        //{
        //    get
        //    {
        //        if (mRecentUsedBusinessFlows == null)
        //        {
        //            mRecentUsedBusinessFlows = new MRUManager();
        //            mRecentUsedBusinessFlows.Init(Path.Combine(Folder, "RecentlyUsed.dat"));
        //        }
        //        return mRecentUsedBusinessFlows;
        //    }
        //}

        // Need to be tree view


        //public void SetUniqueApplicationName(ApplicationPlatform app)
        //{
        //    if (this.ApplicationPlatforms.Where(obj => obj.AppName == app.AppName).FirstOrDefault() == null) return; //no name like it in the group

        //    List<ApplicationPlatform> sameNameObjList =
        //        this.ApplicationPlatforms.Where(obj => obj.AppName == app.AppName).ToList<ApplicationPlatform>();
        //    if (sameNameObjList.Count == 1 && sameNameObjList[0] == app) return; //Same internal object

        //    //Set unique name
        //    int counter = 2;
        //    while ((this.ApplicationPlatforms.Where(obj => obj.AppName == app.AppName + counter).FirstOrDefault()) != null)
        //        counter++;
        //    app.AppName += counter.ToString();
        //}

        /// <summary>
        ///  Return enumerator of all valid files in solution
        /// </summary>
        /// <param name="solutionFolder"></param>
        /// <returns></returns>
        //public static IEnumerable<string> SolutionFiles(string solutionFolder)
        //{
        //    //List only need directories which have repo items
        //    //Do not add documents, ExecutionResults, HTMLReports
        //    ConcurrentBag<string> fileEntries = new ConcurrentBag<string>();

        //    //add Solution.xml
        //    fileEntries.Add(Path.Combine(solutionFolder, "Ginger.Solution.xml"));

        //    string[] SolutionMainFolders = new string[] { "Agents", "ALMDefectProfiles", "Applications Models", "BusinessFlows", "Configurations", "DataSources", "Environments", "HTMLReportConfigurations", "PluginPackages", "Plugins", "RunSetConfigs", "SharedRepository" };
        //    Parallel.ForEach(SolutionMainFolders, folder =>
        //    {
        //        // Get each main folder sub folder all levels
        //        string MainFolderFullPath = Path.Combine(solutionFolder, folder);

        //        if (Directory.Exists(MainFolderFullPath))
        //        {
        //            // Add folder and it sub folders files
        //            AddFolderFiles(fileEntries, MainFolderFullPath);
        //        }
        //    });

        //    return fileEntries.ToList();
        //}

        //static void AddFolderFiles(ConcurrentBag<string> CB, string folder)
        //{
        //    //need to look for all .xmls and not only *Ginger.*.xml" for covering old xml's as well
        //    IEnumerable<string> files = Directory.EnumerateFiles(folder, "*.xml", SearchOption.AllDirectories).AsParallel().AsOrdered();
        //    Parallel.ForEach(files, file =>
        //    {
        //        CB.Add(file);
        //    });
        //}

        //public object CreateNewReportTemplate()
        //{
        //    //ReportTemplate NewReportTemplate = new ReportTemplate() { Name = "New Report Template", Status = ReportTemplate.eReportStatus.Development };

        //    //ReportTemplateSelector RTS = new ReportTemplateSelector();
        //    //RTS.ShowAsWindow();

        //    //if (RTS.SelectedReportTemplate != null)
        //    //{

        //    //    NewReportTemplate.Xaml = RTS.SelectedReportTemplate.Xaml;

        //    //    //Make it Generic or Const string for names used for File
        //    //    string NewReportName = string.Empty;
        //    //    if (GingerCore.General.GetInputWithValidation("Add Report Template", "Report Template Name:", ref NewReportName, System.IO.Path.GetInvalidFileNameChars()))
        //    //    {
        //    //        NewReportTemplate.Name = NewReportName;                    
        //    //        WorkSpace.Instance.SolutionRepository.AddRepositoryItem(NewReportTemplate);
        //    //    }
        //    //    return NewReportTemplate;
        //    //}
        //    //return null;
        //    object report = TargetFrameworkHelper.Helper.CreateNewReportTemplate();
        //    return report;
        //}



        //public string VariablesNames
        //{
        //    get
        //    {
        //        string varsNames = string.Empty;
        //        foreach (VariableBase var in Variables)
        //            varsNames += var.Name + ", ";
        //        return (varsNames.TrimEnd(new char[] { ',', ' ' }));
        //    }
        //}

        //public void RefreshVariablesNames() { OnPropertyChanged(Fields.VariablesNames); }

        //public VariableBase GetVariable(string varName)
        //{
        //    VariableBase v = (from v1 in Variables where v1.Name == varName select v1).FirstOrDefault();
        //    return v;
        //}

        //public void ResetVaribles()
        //{
        //    foreach (VariableBase va in Variables)
        //        va.ResetValue();
        //}

        //public void AddVariable(VariableBase v, int insertIndex = -1)
        //{
        //    if (v != null)
        //    {
        //        if (string.IsNullOrEmpty(v.Name)) v.Name = "NewVar";
        //        SetUniqueVariableName(v);
        //        if (insertIndex < 0 || insertIndex > Variables.Count - 1)
        //        {
        //            Variables.Add(v);
        //        }
        //        else
        //        {
        //            Variables.Insert(insertIndex, v);
        //        }
        //    }
        //}

        //public void SetUniqueVariableName(VariableBase var)
        //{
        //    if (string.IsNullOrEmpty(var.Name)) var.Name = "Variable";
        //    if (this.Variables.Where(x => x.Name == var.Name).FirstOrDefault() == null) return; //no name like it

        //    List<VariableBase> sameNameObjList =
        //        this.Variables.Where(x => x.Name == var.Name).ToList<VariableBase>();
        //    if (sameNameObjList.Count == 1 && sameNameObjList[0] == var) return; //Same internal object

        //    //Set unique name
        //    int counter = 2;
        //    while ((this.Variables.Where(x => x.Name == var.Name + "_" + counter.ToString()).FirstOrDefault()) != null)
        //        counter++;
        //    var.Name = var.Name + "_" + counter.ToString();
        //}

        //ObservableList<ExecutionLoggerConfiguration> ISolution.ExecutionLoggerConfigurationSetList { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


        //public ePlatformType GetTargetApplicationPlatform(RepositoryItemKey TargetApplicationKey)
        //{
        //    if (TargetApplicationKey != null)
        //    {
        //        string targetapp = TargetApplicationKey.ItemName;
        //        ePlatformType platform = (from x in ApplicationPlatforms where x.AppName == targetapp select x.Platform).FirstOrDefault();
        //        return platform;
        //    }
        //    return ePlatformType.Web;
        //}

        /// <summary>
        /// This method will return platform for the target application name
        /// </summary>
        /// <param name="targetapp"></param>
        /// <returns></returns>
        //public ePlatformType GetApplicationPlatformForTargetApp(string targetapp)
        //{
        //    if (!string.IsNullOrEmpty(targetapp))
        //    {
        //        ePlatformType platform = (from x in ApplicationPlatforms where x.AppName == targetapp select x.Platform).FirstOrDefault();
        //        return platform;
        //    }
        //    return ePlatformType.NA;
        //}


        // overriding this SerializationError here because previously we were supporting only one ALMConfig 
        // Now we changed this to support MultiALM Connection, so serializing those values to ALMConfigs List


        //public List<ApplicationPlatform> GetListOfPomSupportedPlatform()
        //{
        //    if (ApplicationPlatforms != null)
        //    {
        //        if (ApplicationPlatforms.Count != 0)
        //        {
        //            return ApplicationPlatforms.Where(x => ApplicationPOMModel.PomSupportedPlatforms.Contains(x.Platform)).ToList();
        //        }
        //    }
        //    return new List<ApplicationPlatform>();
        //}
    }
}
