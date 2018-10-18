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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.ALM;
using Ginger.GeneralLib;
using Ginger.Reports;
using GingerCore;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerCoreNET.SourceControl;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace Ginger.SolutionGeneral
{
    public class Solution : RepositoryItemBase
    {
        public SourceControlBase SourceControl { get; set; }

        [IsSerializedForLocalRepository]

        public bool ShowIndicationkForLockedItems { get; set; }

        public Solution()
        {
            Tags = new ObservableList<RepositoryItemTag>();
        }

        public static Solution LoadSolution(string solutionFileName, bool startDirtyTracking= true)
        {
            string txt = File.ReadAllText(solutionFileName);
            Solution solution = (Solution)NewRepositorySerializer.DeserializeFromText(txt);
            solution.FilePath = solutionFileName;
            solution.Folder = Path.GetDirectoryName(solutionFileName);
            if (startDirtyTracking)
            {
                solution.StartDirtyTracking();
            }
            return solution;
        }

        public enum eSolutionItemToSave { GeneralDetails, TargetApplications, GlobalVariabels, Tags, ALMSettings, SourceControlSettings, ReportsSettings}
        public void SaveSolution(bool showWarning = true, eSolutionItemToSave solutionItemToSave = eSolutionItemToSave.GeneralDetails)
        {
            bool doSave = false;

            if (!showWarning)
            {
                doSave = true;
            }
            else
            {
                Solution lastSavedSolution = LoadSolution(FilePath, false);
                string extraChangedItems = "";
                StringBuilder bldExtraChangedItems = new StringBuilder();
                
                if (solutionItemToSave != eSolutionItemToSave.GeneralDetails)
                {
                    if (this.Name != lastSavedSolution.Name || this.Account != lastSavedSolution.Account)
                    {
                        bldExtraChangedItems.Append("Solution General Details, ");
                    }                        
                }
                if (solutionItemToSave != eSolutionItemToSave.ALMSettings)
                {
                    if (this.ALMDomain != lastSavedSolution.ALMDomain || this.ALMProject != lastSavedSolution.ALMProject || this.ALMServerURL != lastSavedSolution.ALMServerURL || this.AlmType != lastSavedSolution.AlmType)
                    {
                        bldExtraChangedItems.Append("ALM Details, ");
                    }                        
                }
                if (solutionItemToSave != eSolutionItemToSave.SourceControlSettings)
                {
                    if (this.SourceControl != lastSavedSolution.SourceControl)
                    {
                        bldExtraChangedItems.Append("Source Control Details, ");
                    }                        
                }
                if (solutionItemToSave != eSolutionItemToSave.ReportsSettings)
                {
                    if (ExecutionLoggerConfigurationSetList != null && (ExecutionLoggerConfigurationSetList.Count != lastSavedSolution.ExecutionLoggerConfigurationSetList.Count || HTMLReportsConfigurationSetList.Count != lastSavedSolution.HTMLReportsConfigurationSetList.Count))
                    {
                        bldExtraChangedItems.Append("Reports Settings, ");
                    }
                    else
                    {
                        if (ExecutionLoggerConfigurationSetList != null)
                        {
                            foreach (ExecutionLoggerConfiguration config in ExecutionLoggerConfigurationSetList)
                            {
                                if (config.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified || config.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.NoTracked)
                                {
                                    bldExtraChangedItems.Append("Reports Settings, ");
                                    break;
                                }
                            }
                        }
                        if (!bldExtraChangedItems.ToString().Contains("Reports Settings"))
                        {
                            foreach (HTMLReportsConfiguration config in HTMLReportsConfigurationSetList)
                            {
                                if (config.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified || config.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.NoTracked)
                                {

                                    bldExtraChangedItems.Append("Reports Settings, ");
                                    break;
                                }
                            }
                        }
                    }
                }                
                if (solutionItemToSave != eSolutionItemToSave.GlobalVariabels)
                {
                    if (Variables.Count != lastSavedSolution.Variables.Count)
                    {
                        bldExtraChangedItems.Append(GingerDicser.GetTermResValue(eTermResKey.Variables, "Global ", ", "));
                    }
                    else
                    {
                        foreach (VariableBase var in Variables)
                        {
                            if (var.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified || var.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.NoTracked)
                            {
                                bldExtraChangedItems.Append(GingerDicser.GetTermResValue(eTermResKey.Variables, "Global ", ", "));
                                break;
                            }
                        }
                    }
                }
                if (solutionItemToSave != eSolutionItemToSave.TargetApplications)
                {
                    if (ApplicationPlatforms.Count != lastSavedSolution.ApplicationPlatforms.Count)
                    {
                        bldExtraChangedItems.Append("Target Applications, ");
                    }
                    else
                    {
                        foreach (ApplicationPlatform app in ApplicationPlatforms)
                        {
                            if (app.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.Modified || app.DirtyStatus == Amdocs.Ginger.Common.Enums.eDirtyStatus.NoTracked)
                            {
                                bldExtraChangedItems.Append("Target Applications, ");
                                break;
                            }
                        }
                    }
                }
                if (solutionItemToSave != eSolutionItemToSave.Tags)
                {
                    if (Tags.Count != lastSavedSolution.Tags.Count)
                    {
                        bldExtraChangedItems.Append("Tags, ");
                    }
                    else
                    {
                        foreach (RepositoryItemTag tag in Tags)
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
                    extraChangedItems= extraChangedItems.TrimEnd();
                    extraChangedItems= extraChangedItems.TrimEnd(new char[] { ',' });                    
                    if (Reporter.ToUser(eUserMsgKeys.SolutionSaveWarning, extraChangedItems) == System.Windows.MessageBoxResult.Yes)
                    {
                        doSave = true;
                    }
                }
            }

            if (doSave)
            {
                Reporter.ToGingerHelper(eGingerHelperMsgKey.SaveItem, null, "Solution Configurations", "item");
                RepositorySerializer.SaveToFile(this, FilePath);
                this.SetDirtyStatusToNoChange();
                Reporter.CloseGingerHelper();
            }
        }
                
        string mName;
        [IsSerializedForLocalRepository]
        public string Name
        {
            get
            {
                return mName;
            }
            set
            {
                mName = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string Folder { get; set; }        

        [IsSerializedForLocalRepository]
        public ObservableList<RepositoryItemTag> Tags;

        private string mAccount;

        [IsSerializedForLocalRepository]
        public string Account {
            get
            {
                return mAccount;
            }
            set
            {
                mAccount = value;
                AutoLogProxy.SetAccount(mAccount);
            } }

        public ePlatformType MainPlatform {
            get {
                if (ApplicationPlatforms != null && ApplicationPlatforms.Count() > 0)
                {
                    return ApplicationPlatforms[0].Platform;
                }
                else
                {
                    return ePlatformType.NA;
                }
            }
        }

        private ALMIntegration.eALMType mAlmType = ALMIntegration.eALMType.QC;
        [IsSerializedForLocalRepository]
        public ALMIntegration.eALMType AlmType
        {
            get
            {
                return mAlmType;
            }
            set
            {
                mAlmType = value;
            }
        }

        [IsSerializedForLocalRepository]
        public string ALMServerURL { get; set; }

        [IsSerializedForLocalRepository]
        public bool UseRest { get; set; }

        [IsSerializedForLocalRepository]
        public string ALMDomain { get; set; }

        [IsSerializedForLocalRepository]
        public string ALMProject { get; set; }

        public void SetReportsConfigurations()
        {
            if ((this.ExecutionLoggerConfigurationSetList == null) || (this.ExecutionLoggerConfigurationSetList.Count == 0))
            {
                this.ExecutionLoggerConfigurationSetList = new ObservableList<ExecutionLoggerConfiguration>();
                ExecutionLoggerConfiguration ExecutionLoggerConfiguration = new ExecutionLoggerConfiguration();
                ExecutionLoggerConfiguration.IsSelected = true;
                ExecutionLoggerConfiguration.ExecutionLoggerConfigurationIsEnabled = true;
                ExecutionLoggerConfiguration.ExecutionLoggerConfigurationMaximalFolderSize = 250;
                ExecutionLoggerConfiguration.ExecutionLoggerConfigurationExecResultsFolder = @"~\ExecutionResults\";
                ExecutionLoggerConfigurationSetList.Add(ExecutionLoggerConfiguration);
            }

            if ((this.HTMLReportsConfigurationSetList == null) || (this.HTMLReportsConfigurationSetList.Count == 0))
            {
                this.HTMLReportsConfigurationSetList = new ObservableList<HTMLReportsConfiguration>();
                HTMLReportsConfiguration HTMLReportsConfiguration = new HTMLReportsConfiguration();
                HTMLReportsConfiguration.IsSelected = true;
                HTMLReportsConfiguration.HTMLReportsFolder = @"~\HTMLReports\";
                HTMLReportsConfiguration.HTMLReportsAutomaticProdIsEnabled = false;
                HTMLReportsConfigurationSetList.Add(HTMLReportsConfiguration);
            }

            App.AutomateTabGingerRunner.ExecutionLogger.Configuration = this.ExecutionLoggerConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
        }

        [IsSerializedForLocalRepository]
        public ObservableList<ApplicationPlatform> ApplicationPlatforms;

        public string MainApplication
        {
            //TODO: check usage
            get
            {
                if (ApplicationPlatforms == null)
                    ApplicationPlatforms = new ObservableList<ApplicationPlatform>();

                if (ApplicationPlatforms.Count > 0)
                {
                    return ApplicationPlatforms[0].AppName;
                }
                else
                {
                    return null;
                }
            }
        }
        
        [IsSerializedForLocalRepository]
        public string LastBusinessFlowFileName { get; set; }

        MRUManager mRecentUsedBusinessFlows;

        public MRUManager RecentlyUsedBusinessFlows
        {
            get
            {
                if (mRecentUsedBusinessFlows == null)
                {
                    mRecentUsedBusinessFlows = new MRUManager();
                    mRecentUsedBusinessFlows.Init(Path.Combine(Folder, "RecentlyUsed.dat"));
                }
                return mRecentUsedBusinessFlows;
            }
        }

        // Need to be tree view
        public override string GetNameForFileName() { return Name; }

        public string BusinessFlowsMainFolder
        {
            get
            {
                string folderPath = Path.Combine(Folder , @"BusinessFlows\");
                if(!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                return folderPath;
            }
        }

        public void SetUniqueApplicationName(ApplicationPlatform app)
        {
            if (this.ApplicationPlatforms.Where(obj => obj.AppName == app.AppName).FirstOrDefault() == null) return; //no name like it in the group

            List<ApplicationPlatform> sameNameObjList =
                this.ApplicationPlatforms.Where(obj => obj.AppName == app.AppName).ToList<ApplicationPlatform>();
            if (sameNameObjList.Count == 1 && sameNameObjList[0] == app) return; //Same internal object

            //Set unique name
            int counter = 2;
            while ((this.ApplicationPlatforms.Where(obj => obj.AppName == app.AppName + counter).FirstOrDefault()) != null)
                counter++;
            app.AppName += counter.ToString();
        }

        /// <summary>
        ///  Return enumerator of all valid files in solution
        /// </summary>
        /// <param name="solutionFolder"></param>
        /// <returns></returns>
        public static IEnumerable<string> SolutionFiles(string solutionFolder)
        {
            //List only need directories which have repo items
            //Do not add documents, ExecutionResults, HTMLReports
            ConcurrentBag<string> fileEntries = new ConcurrentBag<string>();

            //add Solution.xml
            fileEntries.Add(Path.Combine(solutionFolder, "Ginger.Solution.xml"));

            string[] SolutionMainFolders = new string[] { "Agents", "ALMDefectProfiles", "Applications Models", "BusinessFlows", "Configurations", "DataSources", "Environments", "HTMLReportConfigurations", "PluginPackages", "RunSetConfigs", "SharedRepository" };
            Parallel.ForEach(SolutionMainFolders, folder =>
            {
                    // Get each main folder sub folder all levels
                    string MainFolderFullPath = Path.Combine(solutionFolder, folder);

                if (Directory.Exists(MainFolderFullPath))
                {
                    // Add folder and it sub folders files
                    AddFolderFiles(fileEntries, MainFolderFullPath);
                }
            });

            return fileEntries.ToList();
        }

        static void AddFolderFiles(ConcurrentBag<string> CB, string folder)
        {
            if (folder == "PrevVersions")//TODO: use const
            {
                return;
            }

            IEnumerable<string> files = Directory.EnumerateFiles(folder, "*Ginger.*.xml", SearchOption.AllDirectories).AsParallel().AsOrdered();
            Parallel.ForEach(files, file =>
            {
                CB.Add(file);
            });
        }

        internal ReportTemplate CreateNewReportTemplate(string path = "")
        {
            ReportTemplate NewReportTemplate = new ReportTemplate() { Name = "New Report Template", Status = ReportTemplate.eReportStatus.Development };
            
            ReportTemplateSelector RTS = new ReportTemplateSelector();
            RTS.ShowAsWindow();

            if (RTS.SelectedReportTemplate != null)
            {

                NewReportTemplate.Xaml = RTS.SelectedReportTemplate.Xaml;

                //Make it Generic or Const string for names used for File
                string NewReportName = string.Empty;
                if (GingerCore.General.GetInputWithValidation("Add Report Template", "Report Template Name:", ref NewReportName, System.IO.Path.GetInvalidFileNameChars()))
                {
                    NewReportTemplate.Name = NewReportName;                    
                    WorkSpace.Instance.SolutionRepository.AddRepositoryItem(NewReportTemplate);
                }
                return NewReportTemplate;
            }
            return null;
        }


        [IsSerializedForLocalRepository]
        public ObservableList<VariableBase> Variables = new ObservableList<VariableBase>();

        [IsSerializedForLocalRepository]
        public ObservableList<ExecutionLoggerConfiguration> ExecutionLoggerConfigurationSetList = new ObservableList<ExecutionLoggerConfiguration>();

        [IsSerializedForLocalRepository]
        public ObservableList<HTMLReportsConfiguration> HTMLReportsConfigurationSetList = new ObservableList<HTMLReportsConfiguration>();

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

        public void AddVariable(VariableBase v)
        {
            if (v != null)
            {
                if (string.IsNullOrEmpty(v.Name)) v.Name = "NewVar";
                SetUniqueVariableName(v);
                Variables.Add(v);
            }
        }

        public void SetUniqueVariableName(VariableBase var)
        {
            if (string.IsNullOrEmpty(var.Name)) var.Name = "Variable";
            if (this.Variables.Where(x => x.Name == var.Name).FirstOrDefault() == null) return; //no name like it

            List<VariableBase> sameNameObjList =
                this.Variables.Where(x => x.Name == var.Name).ToList<VariableBase>();
            if (sameNameObjList.Count == 1 && sameNameObjList[0] == var) return; //Same internal object

            //Set unique name
            int counter = 2;
            while ((this.Variables.Where(x => x.Name == var.Name + "_" + counter.ToString()).FirstOrDefault()) != null)
                counter++;
            var.Name = var.Name + "_" + counter.ToString();
        }

        public override string ItemName
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }
        
        [IsSerializedForLocalRepository]
        public ObservableList<ExternalItemFieldBase> ExternalItemsFields = new ObservableList<ExternalItemFieldBase>();

        public ePlatformType GetTargetApplicationPlatform(RepositoryItemKey TargetApplicationKey)
        {
            if (TargetApplicationKey != null)
            {
                string targetapp = TargetApplicationKey.ItemName;
                ePlatformType platform = (from x in ApplicationPlatforms where x.AppName == targetapp select x.Platform).FirstOrDefault();
                return platform;
            }
            return ePlatformType.Web;
        }

    }


}
