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

using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.ALM;
using Ginger.GeneralLib;
using Ginger.Reports;
using Ginger.Repository;
using GingerCore;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerCoreNET.SourceControl;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Ginger.Environments
{
    public class Solution : RepositoryItem
    {
        public new static class Fields
        {
            public static string Name = "Name";
            public static string Folder = "Folder";
            public static string MainPlatform = "MainPlatform";   //TODO: delete old usage , after fully moving to apps/platform, meanwhile keeping so code will compile
            public static string Account = "Account";
            public static string Variables = "Variables";
            public static string VariablesNames = "VariablesNames";
            public static string AlmType = "AlmType";

            public static string ALMServerURL = "ALMServerURL";
            public static string ALMDomain = "ALMDomain";
            public static string ALMProject = "ALMProject";

            public static string ShowIndicationkForLockedItems = "ShowIndicationkForLockedItems";
        }

        public SourceControlBase SourceControl { get; set; }

        [IsSerializedForLocalRepository]

        public bool ShowIndicationkForLockedItems { get; set; }

        public Solution()
        {
            Tags = new ObservableList<RepositoryItemTag>();
        }

        [IsSerializedForLocalRepository]
        public string Name { get; set; }
                
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
                    mRecentUsedBusinessFlows.Init(Folder + "RecentlyUsed.dat");
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
                string folderPath = Folder + @"BusinessFlows\";
                if (Directory.Exists(folderPath) == false)
                    Directory.CreateDirectory(folderPath);
                return folderPath;
            }
        }

        public string ApplicationsMainFolder
        {
            get
            {
                string folderPath = Folder + @"Applications\";
                if (Directory.Exists(folderPath) == false)
                    Directory.CreateDirectory(folderPath);
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
            // super fast way to get files list in parllel
            if (GingerCore.Repository.RepositorySerializer.FastLoad)
            {
                //List only need directories which have repo items
                //Do not add documents, ExecutionResults, HTMLReports
                ConcurrentBag<string> fileEntries = new ConcurrentBag<string>();

                string[] SolutionMainFolders = new string[] { "Agents", "Applications", "BusinessFlows", "DataSources", "Environments", "ALMDefectProfiles", "HTMLReportConfigurations", "RunSetConfigs", "SharedRepository" };
                
                Parallel.ForEach(SolutionMainFolders, folder =>
                {
                    // Get each main folder sub folder all levels
                    string MainFolderFullPath = Path.Combine(solutionFolder, folder);

                    if (Directory.Exists(MainFolderFullPath))
                    {
                        // Add main folder files
                        AddFolderFiles(fileEntries, MainFolderFullPath);

                        //Now drill down to ALL sub folders
                        string[] SubFolders = Directory.GetDirectories(MainFolderFullPath, "*", SearchOption.AllDirectories);

                        Parallel.ForEach(SubFolders, sf =>
                        {
                            // Add all files of sub folder
                            if (sf != "PrevVersions")  //TODO: use const
                            {
                                AddFolderFiles(fileEntries, sf);
                            }
                        });
                    }
                });

                //To comapre with the old way I wrote the file sorted in both option and compared them - it was matching 1:1 on big project with 1512 files.
                
                return fileEntries.ToList();

            }
            else
            {

                ConcurrentBag<string> fileEntries = new ConcurrentBag<string>();
                //take root folder files
                IEnumerable<string> files = Directory.EnumerateFiles(solutionFolder, "*Ginger.*.xml", SearchOption.TopDirectoryOnly).AsParallel().AsOrdered().ToList();
                foreach (string file in files)
                {
                    fileEntries.Add(file);
                }

                //take sub folders files
                string[] excludedSolutionFolders = new string[] { "HTMLReports", "ExecutionResults", "Backups", "Backup", "Documents","AutoSave","Recover" };
                IEnumerable<string> AllFolderList = Directory.EnumerateDirectories(solutionFolder, "*", SearchOption.TopDirectoryOnly).Where(file => !excludedSolutionFolders.Any(x => file.Contains(x))).ToList();
                
                foreach (string folder in AllFolderList)
                {
                    // Get each main folder sub folder all levels
                 if (Directory.Exists(folder))
                    {    // Add main folder files
                        AddFolderFiles(fileEntries, folder);
                    }
                }
                return fileEntries.ToList();
            }
        }

        static void AddFolderFiles(ConcurrentBag<string> CB, string folder)
        {
            IEnumerable<string> files = Directory.EnumerateFiles(folder, "*Ginger.*.xml", SearchOption.AllDirectories).AsParallel().AsOrdered();
            Parallel.ForEach(files, file =>
            {
                CB.Add(file);
            });
        }

        internal ReportTemplate CreateNewReportTemplate(string path = "")
        {
            ReportTemplate NewReportTemplate = new ReportTemplate() { Name = "New Report Template", Status = ReportTemplate.eReportStatus.Development };

            System.Reflection.Assembly ExecutingAssembly;
            ExecutingAssembly = System.Reflection.Assembly.GetExecutingAssembly();

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
                    if (string.IsNullOrEmpty(path))
                        NewReportTemplate.FileName = LocalRepository.GetRepoItemFileName(NewReportTemplate);
                    else
                        NewReportTemplate.FileName = LocalRepository.GetRepoItemFileName(NewReportTemplate, path);
                    if (App.LocalRepository != null)
                    {
                        App.LocalRepository.SaveNewItem(NewReportTemplate, path);
                        App.LocalRepository.AddItemToCache(NewReportTemplate);
                    }
                }
                return NewReportTemplate;
            }
            return null;
        }

        internal HTMLReportTemplate CreateNewHTMLReportTemplate(string path = "")
        {
            HTMLReportTemplate NewReportTemplate = new HTMLReportTemplate() { Name = "New Report Template", Status = HTMLReportTemplate.eReportStatus.Development };

            System.Reflection.Assembly ExecutingAssembly;
            ExecutingAssembly = System.Reflection.Assembly.GetExecutingAssembly();

            HTMLReportTemplateSelector RTS = new HTMLReportTemplateSelector();
            RTS.ShowAsWindow();

            if (RTS.SelectedReportTemplate != null)
            {
                NewReportTemplate.HTML = RTS.SelectedReportTemplate.HTML;

                //Make it Generic or Const string for names used for File
                string NewReportName = string.Empty;
                if (GingerCore.General.GetInputWithValidation("Add New Report", "Report Name:", ref NewReportName, System.IO.Path.GetInvalidFileNameChars()))
                {
                    NewReportTemplate.Name = NewReportName;
                    if (string.IsNullOrEmpty(path))
                        NewReportTemplate.FileName = LocalRepository.GetRepoItemFileName(NewReportTemplate);
                    else
                        NewReportTemplate.FileName = LocalRepository.GetRepoItemFileName(NewReportTemplate, path);
                    if (App.LocalRepository != null)
                    {
                        App.LocalRepository.SaveNewItem(NewReportTemplate, path);
                        App.LocalRepository.AddItemToCache(NewReportTemplate);
                    }
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

        public string VariablesNames
        {
            get
            {
                string varsNames = string.Empty;
                foreach (VariableBase var in Variables)
                    varsNames += var.Name + ", ";
                return (varsNames.TrimEnd(new char[] { ',', ' ' }));
            }
        }

        public void RefreshVariablesNames() { OnPropertyChanged(Fields.VariablesNames); }

        public VariableBase GetVariable(string varName)
        {
            VariableBase v = (from v1 in Variables where v1.Name == varName select v1).FirstOrDefault();
            return v;
        }

        public void ResetVaribles()
        {
            foreach (VariableBase va in Variables)
                va.ResetValue();
        }

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
