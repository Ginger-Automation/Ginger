//#region License
///*
//Copyright © 2014-2018 European Support Limited

//Licensed under the Apache License, Version 2.0 (the "License")
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at 

//http://www.apache.org/licenses/LICENSE-2.0 

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS, 
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and 
//limitations under the License. 
//*/
//#endregion

//using Amdocs.Ginger.Common;
//using Amdocs.Ginger.Common.Repository;
//using Amdocs.Ginger.CoreNET.SolutionRepositoryLib;
//using Amdocs.Ginger.Repository;
//using GingerCoreNET.ALMLib;
//using GingerCoreNET.GeneralLib;
//using GingerCoreNET.ReportLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ReportsLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.VariablesLib;
//using GingerCoreNET.SourceControl;
//using GingerCoreNET.UsageTrackingLib;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;

//namespace GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib
//{
//    public class Solution : RepositoryItem, ISolution
//    {
//        public SourceControlBase SourceControl { get; set; }

//        [IsSerializedForLocalRepository]

//        public bool ShowIndicationkForLockedItems { get; set; }

//        public Solution()
//        {
//            Tags = new ObservableList<RepositoryItemTag>();
//        }

//        [IsSerializedForLocalRepository]
//        public string Name { get; set; }
        
//        public string Folder { get; set; }

//        [IsSerializedForLocalRepository]
//        public ObservableList<RepositoryItemTag> Tags;

//        private string mAccount;

//        [IsSerializedForLocalRepository]
//        public string Account {
//            get
//            {
//                return mAccount;
//            }
//            set
//            {
//                mAccount = value;
//                AutoLogProxy.SetAccount(mAccount);
//            } }
        
//        private ALMIntegration.eALMType mAlmType = ALMIntegration.eALMType.QC;
//        [IsSerializedForLocalRepository]
//        public ALMIntegration.eALMType AlmType
//        {
//            get
//            {
//                return mAlmType;
//            }
//            set
//            {
//                mAlmType = value;
//            }
//        }

//        [IsSerializedForLocalRepository]
//        public string ALMServerURL { get; set; }

//        [IsSerializedForLocalRepository]
//        public string ALMDomain { get; set; }

//        [IsSerializedForLocalRepository]
//        public string ALMProject { get; set; }

//        public void SetReportsConfigurations()
//        {
//        }
        
//        // Keeping it for conversions - do not serialize after conversion
//        public ObservableList<ApplicationPlatform> ApplicationPlatforms;
        
//        [IsSerializedForLocalRepository]
//        public string LastBusinessFlowFileName { get; set; }


//        MRUManager mRecentUsedBusinessFlows;

//        public MRUManager RecentlyUsedBusinessFlows
//        {
//            get
//            {
//                if (mRecentUsedBusinessFlows == null)
//                {
//                    mRecentUsedBusinessFlows = new MRUManager();
//                    mRecentUsedBusinessFlows.Init(Folder + "RecentlyUsed.dat");
//                }
//                return mRecentUsedBusinessFlows;
//            }
//        }
        
//        public override string GetNameForFileName() { return Name; }
        
//        public string BusinessFlowsMainFolder
//        {
//            get
//            {
//                string folderPath = Folder + @"BusinessFlows\";
//                if (Directory.Exists(folderPath) == false)
//                    Directory.CreateDirectory(folderPath);
//                return folderPath;
//            }
//        }

//        public string ApplicationsMainFolder
//        {
//            get
//            {
//                string folderPath = Folder + @"Applications\";
//                if (Directory.Exists(folderPath) == false)
//                    Directory.CreateDirectory(folderPath);
//                return folderPath;
//            }
//        }
        
//        /// <summary>
//        ///  Return enumerator of all valid files in solution
//        /// </summary>
//        /// <param name="solutionFolder"></param>
//        /// <returns></returns>
//        static IEnumerable<string> SolutionFiles(string solutionFolder)
//        {
//                //List only need directories which have repo items
//                //Do not add documents, ExecutionResults, HTMLReports
//                ConcurrentBag<string> fileEntries = new ConcurrentBag<string>();

//                string[] SolutionMainFolders = new string[] { "Agents", "Applications", "BusinessFlows", "DataSources", "Environments", "ALMDefectProfiles", "HTMLReportConfigurations", "RunSetConfigs", "SharedRepository" };
                
//                Parallel.ForEach(SolutionMainFolders, folder =>
//                {
//                    // Get each main folder sub folder all levels
//                    string MainFolderFullPath = Path.Combine(solutionFolder, folder);

//                    if (Directory.Exists(MainFolderFullPath))
//                    {
//                        // Add main folder files
//                        AddFolderFiles(fileEntries, MainFolderFullPath);

//                        //Now drill down to ALL sub folders
//                        string[] SubFolders = Directory.GetDirectories(MainFolderFullPath, "*", SearchOption.AllDirectories);

//                        Parallel.ForEach(SubFolders, sf =>
//                        {
//                            // Add all files of sub folder
//                            if (sf != "PrevVersions")  //TODO: use const
//                            {
//                                AddFolderFiles(fileEntries, sf);
//                            }
//                        });
//                    }
//                });
//                return fileEntries.ToList();
//        }

//        static void AddFolderFiles(ConcurrentBag<string> CB, string folder)
//        {
//            IEnumerable<string> files = Directory.EnumerateFiles(folder, "*Ginger.*.xml", SearchOption.TopDirectoryOnly);                        
//            foreach (string file in files)
//            {
//                CB.Add(file);
//            }
//        }

//        public static bool CheckIfSolutionContainsItemFromHigherVersion(string solutionFolder)
//        {
//            return false; 
//        }
        
//        internal void CheckIfUpgradeRequired(bool silentMode = true)
//        {
//        }
        
//        internal ReportTemplate CreateNewReportTemplate(string path = "")
//        {
//            return null;
//        }
        
//        [IsSerializedForLocalRepository]
//        public ObservableList<VariableBase> Variables = new ObservableList<VariableBase>();

//        [IsSerializedForLocalRepository]
//        public ObservableList<ExecutionLoggerConfiguration> ExecutionLoggerConfigurationSetList = new ObservableList<ExecutionLoggerConfiguration>();

//        [IsSerializedForLocalRepository]        
//        public ObservableList<HTMLReportsConfiguration> HTMLReportsConfigurationSetList = new ObservableList<HTMLReportsConfiguration>();

//        public string VariablesNames
//        {
//            get
//            {
//                string varsNames = string.Empty;
//                foreach (VariableBase var in Variables)
//                    varsNames += var.Name + ", ";
//                return (varsNames.TrimEnd(new char[] { ',', ' ' }));
//            }
//        }

//        public VariableBase GetVariable(string varName)
//        {
//            VariableBase v = (from v1 in Variables where v1.Name == varName select v1).FirstOrDefault();
//            return v;
//        }

//        public void ResetVaribles()
//        {
//            foreach (VariableBase va in Variables)
//                va.ResetValue();
//        }

//        public void AddVariable(VariableBase v)
//        {
//            if (v != null)
//            {
//                if (string.IsNullOrEmpty(v.Name)) v.Name = "NewVar";
//                SetUniqueVariableName(v);
//                Variables.Add(v);
//            }
//        }

//        public void SetUniqueVariableName(VariableBase var)
//        {
//            if (string.IsNullOrEmpty(var.Name)) var.Name = "Variable";
//            if (this.Variables.Where(x => x.Name == var.Name).FirstOrDefault() == null) return; //no name like it

//            List<VariableBase> sameNameObjList =
//                this.Variables.Where(x => x.Name == var.Name).ToList<VariableBase>();
//            if (sameNameObjList.Count == 1 && sameNameObjList[0] == var) return; //Same internal object

//            //Set unique name
//            int counter = 2;
//            while ((this.Variables.Where(x => x.Name == var.Name + "_" + counter.ToString()).FirstOrDefault()) != null)
//                counter++;
//            var.Name = var.Name + "_" + counter.ToString();
//        }

//        public override string ItemName
//        {
//            get
//            {
//                return this.Name;
//            }
//            set
//            {
//                this.Name = value;
//            }
//        }

//        [IsSerializedForLocalRepository]
//        public ObservableList<ExternalItemFieldBase> ExternalItemsFields = new ObservableList<ExternalItemFieldBase>();
        
//        /// <summary>
//        /// Create new Solution in file system and get it Solution Repository
//        /// </summary>
//        /// <param name="solutionName"></param>
//        /// <param name="SolutionFolderPath"></param>
//        /// <returns></returns>
//        public static void CreateNewSolution(string solutionName, string SolutionFolderPath)
//        {
//            if (System.IO.Directory.Exists(SolutionFolderPath))
//            {
//                throw new Exception("Cannot create new solution, directory already exist - " + SolutionFolderPath);
//            }

//            System.IO.Directory.CreateDirectory(SolutionFolderPath);

//            ISolution solution = new Solution();
//            solution.Name = solutionName;
//            solution.Folder = SolutionFolderPath;
//            SaveSolution(solution);

//            //return SolutionRepository.GetSolutionRepository(SolutionFolderPath, RepositorySerializerInitilizer2.GetNewRepositoryFolders);
//        }

//        /// <summary>
//        /// Save Solution XML
//        /// </summary>
//        /// <param name="solution"></param>
//        public static void SaveSolution(ISolution solution)
//        {
//            //TODO: FIXME!!!

//            //if (!System.IO.Directory.Exists(solution.Folder))
//            //{
//            //    System.IO.Directory.CreateDirectory(solution.Folder);
//            //}

//            //string FileName = Path.Combine(solution.Folder, "Ginger.Solution.xml");
//            //string SolutionXML =  SolutionRepository.mRepositorySerializer.SerializeToString((RepositoryItemBase)solution);
//            //File.WriteAllText(FileName, SolutionXML);
//        }
//    }
//}