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

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.SelfHealingLib;
using Amdocs.Ginger.Common.Telemetry;
using Amdocs.Ginger.Common.WorkSpaceLib;
using Amdocs.Ginger.CoreNET.Run.SolutionCategory;
using Amdocs.Ginger.Repository;
using Ginger.Configurations;
using Ginger.Reports;
using GingerCore.Platforms;
using GingerCore.Variables;
using GingerCoreNET.ALMLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerCoreNET.SourceControl;

namespace Ginger.SolutionGeneral
    {
    public class Solution : RepositoryItemBase, ISolution
        {
        public readonly string ExternalIntegrationsTabName = "External Integrations";
        public ISolutionOperations SolutionOperations;

        public SourceControlBase SourceControl { get; set; }

        [IsSerializedForLocalRepository]

        public bool ShowIndicationkForLockedItems { get; set; }   // TODO: fix typo - note serialized attr

        public Solution()
            {
            Tags = [];
            }

        public enum eSolutionItemToSave { GeneralDetails, TargetApplications, GlobalVariabels, Tags, ALMSettings, SourceControlSettings, LoggerConfiguration, ReportConfiguration }

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
                if (mName != value)
                    {
                    mName = value;
                    OnPropertyChanged(nameof(Name));
                    }
                }
            }

        public string Folder { get; set; }

        [IsSerializedForLocalRepository]
        public ObservableList<RepositoryItemTag> Tags;


        private string mAccount;

        /// <summary>
        /// For encrypting password variables
        /// </summary>
        public string EncryptionKey { get; set; }

        [IsSerializedForLocalRepository]
        public string EncryptedValidationString { get; set; }

        public bool NeedVariablesReEncryption { get; set; } = false;

        [IsSerializedForLocalRepository]
        public string Account
            {
            get
                {
                return mAccount;
                }
            set
                {
                mAccount = value;
                }
            }

        public ePlatformType MainPlatform
            {
            get
                {
                if (ApplicationPlatforms != null && ApplicationPlatforms.Any())
                    {
                    return ApplicationPlatforms[0].Platform;
                    }
                else
                    {
                    return ePlatformType.NA;
                    }
                }
            }

        [IsSerializedForLocalRepository]
        public ObservableList<ALMConfig> ALMConfigs
            {
            get;
            set;
            } = [];

        [IsSerializedForLocalRepository]
        public ObservableList<ApplicationPlatform> ApplicationPlatforms { get; set; }

        public string MainApplication
            {
            //TODO: check usage
            get
                {
                if (ApplicationPlatforms == null)
                    ApplicationPlatforms = [];

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

        private SolutionSelfHealingConfig _selfHealingConfig;

        [IsSerializedForLocalRepository]
        public SolutionSelfHealingConfig SelfHealingConfig
            {
            get
                {
                if (_selfHealingConfig == null)
                    {
                    _selfHealingConfig = new();
                    }
                return _selfHealingConfig;
                }
            set
                {
                _selfHealingConfig = value;
                OnPropertyChanged(nameof(SelfHealingConfig));
                }
            }

        public ObservableList<TargetBase> GetSolutionTargetApplications()
            {
            ObservableList<TargetBase> solTargetApplications = [];
            foreach (ApplicationPlatform app in ApplicationPlatforms)
                {
                solTargetApplications.Add(new TargetApplication() { AppName = app.AppName, Guid = app.Guid });
                }
            return solTargetApplications;
            }

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
        public override string GetNameForFileName() { return Name; }

        public string BusinessFlowsMainFolder
            {
            get
                {
                string folderPath = Path.Combine(Folder, @"BusinessFlows\");
                if (!Directory.Exists(folderPath))
                    {
                    Directory.CreateDirectory(folderPath);
                    }
                return folderPath;
                }
            }

        public void SetUniqueApplicationName(ApplicationPlatform app)
            {
            if (ApplicationPlatforms.FirstOrDefault(obj => obj.AppName == app.AppName) == null) return; //no name like it in the group

            List<ApplicationPlatform> sameNameObjList =
                this.ApplicationPlatforms.Where(obj => obj.AppName == app.AppName).ToList<ApplicationPlatform>();
            if (sameNameObjList.Count == 1 && sameNameObjList[0] == app) return; //Same internal object

            //Set unique name
            int counter = 2;
            while ((ApplicationPlatforms.FirstOrDefault(obj => obj.AppName == app.AppName + counter)) != null)
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
            ConcurrentBag<string> fileEntries =
            [
                //add Solution.xml
                Path.Combine(solutionFolder, "Ginger.Solution.xml"),
            ];

            string[] SolutionMainFolders = new string[] { "Agents", "ALMDefectProfiles", "Applications Models", "BusinessFlows", "Configurations", "DataSources", "Environments", "HTMLReportConfigurations", "PluginPackages", "Plugins", "RunSetConfigs", "SharedRepository" };
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
            //need to look for all .xmls and not only *Ginger.*.xml" for covering old xml's as well
            IEnumerable<string> files = Directory.EnumerateFiles(folder, "*.xml", SearchOption.AllDirectories).Where(x => !x.Contains("RQMServerConfigurationsPackage")).AsParallel().AsOrdered();
            Parallel.ForEach(files, file =>
            {
                CB.Add(file);
            });
            }

        public object CreateNewReportTemplate()
            {
            //ReportTemplate NewReportTemplate = new ReportTemplate() { Name = "New Report Template", Status = ReportTemplate.eReportStatus.Development };

            //ReportTemplateSelector RTS = new ReportTemplateSelector();
            //RTS.ShowAsWindow();

            //if (RTS.SelectedReportTemplate != null)
            //{

            //    NewReportTemplate.Xaml = RTS.SelectedReportTemplate.Xaml;

            //    //Make it Generic or Const string for names used for File
            //    string NewReportName = string.Empty;
            //    if (GingerCore.General.GetInputWithValidation("Add Report Template", "Report Template Name:", ref NewReportName, System.IO.Path.GetInvalidFileNameChars()))
            //    {
            //        NewReportTemplate.Name = NewReportName;                    
            //        WorkSpace.Instance.SolutionRepository.AddRepositoryItem(NewReportTemplate);
            //    }
            //    return NewReportTemplate;
            //}
            //return null;
            object report = TargetFrameworkHelper.Helper.CreateNewReportTemplate();
            return report;
            }

        [IsSerializedForLocalRepository]
        public ObservableList<VariableBase> Variables { get; set; } = [];


        [IsSerializedForLocalRepository]
        public ObservableList<ExecutionLoggerConfiguration> ExecutionLoggerConfigurationSetList { get; set; } = [];
        public ExecutionLoggerConfiguration LoggerConfigurations
            {
            get
                {
                if (ExecutionLoggerConfigurationSetList.Count == 0)
                    {
                    ExecutionLoggerConfigurationSetList.Add(new ExecutionLoggerConfiguration());
                    }
                return ExecutionLoggerConfigurationSetList[0];
                }
            set
                {
                ExecutionLoggerConfigurationSetList[0] = value;
                }
            }


        [IsSerializedForLocalRepository]
        public ObservableList<HTMLReportsConfiguration> HTMLReportsConfigurationSetList { get; set; } = [];

        [IsSerializedForLocalRepository]
        public VRTConfiguration VRTConfiguration { get; set; } = new VRTConfiguration();

        [IsSerializedForLocalRepository]
        public ApplitoolsConfiguration ApplitoolsConfiguration { get; set; } = new ApplitoolsConfiguration();

        [IsSerializedForLocalRepository]
        public SealightsConfiguration SealightsConfiguration { get; set; } = new SealightsConfiguration();

        public void AddVariable(VariableBase v, int insertIndex = -1)
            {
            if (v != null)
                {
                Reporter.AddFeatureUsage(FeatureId.GlobalParameter, new TelemetryMetadata()
                {
                    { "Operation", "Create" },
                });

                if (string.IsNullOrEmpty(v.Name)) v.Name = "NewVar";
                SetUniqueVariableName(v);
                if (insertIndex < 0 || insertIndex > Variables.Count - 1)
                    {
                    Variables.Add(v);
                    }
                else
                    {
                    Variables.Insert(insertIndex, v);
                    }
                }
            }

        public void SetUniqueVariableName(VariableBase var)
            {
            if (string.IsNullOrEmpty(var.Name)) var.Name = "Variable";
            if (Variables.FirstOrDefault(x => x.Name == var.Name) == null) return; //no name like it

            List<VariableBase> sameNameObjList =
                this.Variables.Where(x => x.Name == var.Name).ToList<VariableBase>();
            if (sameNameObjList.Count == 1 && sameNameObjList[0] == var) return; //Same internal object

            //Set unique name
            int counter = 2;
            while ((Variables.FirstOrDefault(x => x.Name == var.Name + "_" + counter.ToString())) != null)
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

        ObservableList<ExecutionLoggerConfiguration> ISolution.ExecutionLoggerConfigurationSetList { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        [IsSerializedForLocalRepository]
        public ObservableList<ExternalItemFieldBase> ExternalItemsFields = [];

        public ePlatformType GetTargetApplicationPlatform(RepositoryItemKey TargetApplicationKey)
        {
            if (TargetApplicationKey != null)
            {
                string targetAppName = TargetApplicationKey.ItemName;
                Guid targetAppGuid = TargetApplicationKey.Guid;
                ApplicationPlatform appPlatform = ApplicationPlatforms.FirstOrDefault(ap => string.Equals(ap.AppName, targetAppName));
                if (appPlatform == null)
                {
                    appPlatform = ApplicationPlatforms.FirstOrDefault(ap => ap.Guid == targetAppGuid);
                }
                ePlatformType platform = appPlatform != null ? appPlatform.Platform : default;
                return platform;
            }
            return ePlatformType.Web;
        }

        /// <summary>
        /// This method will return platform for the target application name
        /// </summary>
        /// <param name="targetapp"></param>
        /// <returns></returns>
        public ePlatformType GetApplicationPlatformForTargetApp(string targetapp)
        {
            if (!string.IsNullOrEmpty(targetapp))
            {
                ApplicationPlatform appPlatform = ApplicationPlatforms.FirstOrDefault(ap => string.Equals(ap.AppName, targetapp));
                return appPlatform != null ? appPlatform.Platform : default;
            }
            return ePlatformType.NA;
        }


        // overriding this SerializationError here because previously we were supporting only one ALMConfig 
        // Now we changed this to support MultiALM Connection, so serializing those values to ALMConfigs List
        public override bool SerializationError(SerializationErrorType errorType, string name, string value)
            {
            if (errorType == SerializationErrorType.PropertyNotFound)
                {
                if (name.ToLower().Contains("alm") || name.ToLower().Contains("userest") || name.ToLower().Contains("configpackagefolderpath"))
                    {
                    ALMConfig AlmConfig = ALMConfigs.FirstOrDefault();
                    if (AlmConfig == null)
                        {
                        AlmConfig = new ALMConfig
                            {
                            DefaultAlm = true
                            };
                        ALMConfigs.Add(AlmConfig);
                        }
                    if (name == "ALMServerURL")
                        {
                        AlmConfig.ALMServerURL = value;
                        return true;
                        }
                    if (name == "AlmType")
                        {
                        AlmConfig.AlmType = (ALMIntegrationEnums.eALMType)Enum.Parse(typeof(ALMIntegrationEnums.eALMType), value);
                        ALMUserConfig AlmUserConfig = GingerCoreCommonWorkSpace.Instance.UserProfile.ALMUserConfigs.FirstOrDefault();
                        if (AlmUserConfig == null)
                            {
                            AlmUserConfig = new ALMUserConfig();
                            GingerCoreCommonWorkSpace.Instance.UserProfile.ALMUserConfigs.Add(AlmUserConfig);
                            }
                        AlmUserConfig.AlmType = AlmConfig.AlmType;
                        return true;
                        }
                    if (name == "ALMDomain")
                        {
                        AlmConfig.ALMDomain = value;
                        return true;
                        }
                    if (name == "ALMProject")
                        {
                        AlmConfig.ALMProjectName = value;
                        return true;
                        }
                    if (name == "ALMProjectKey")
                        {
                        AlmConfig.ALMProjectKey = value;
                        return true;
                        }
                    if (name == "UseRest")
                        {
                        bool.TryParse(value, out bool res);
                        AlmConfig.UseRest = res;
                        return true;
                        }
                    if (name == "ConfigPackageFolderPath")
                        {
                        AlmConfig.ALMConfigPackageFolderPath = value;
                        return true;
                        }
                    }
                }
            return false;
            }

        [IsSerializedForLocalRepository]
        public ObservableList<SolutionCategory> SolutionCategories = [];

        public override void PostDeserialization()
            {
            foreach (eSolutionCategories category in Enum.GetValues(typeof(eSolutionCategories)))
                {
                if (!SolutionCategories.Any(x => x.Category == category))
                    {
                    AddCategory(category);
                    }
                }

            }

        private void AddCategory(eSolutionCategories category)
            {
            switch (category)
                {
                case eSolutionCategories.BusinessProcessTag:
                    SolutionCategories.Add(GetBusinessProcessTagCategory());
                    break;
                case eSolutionCategories.SubBusinessProcessTag:
                    SolutionCategories.Add(GetSubBusinessProcessTagCategory());
                    break;
                case eSolutionCategories.TestType:
                    SolutionCategories.Add(GetTestTypeCategory());
                    break;
                case eSolutionCategories.Product:
                    SolutionCategories.Add(GetProductCategory());
                    break;
                case eSolutionCategories.Release:
                    SolutionCategories.Add(GetReleaseCategory());
                    break;
                case eSolutionCategories.Iteration:
                    SolutionCategories.Add(GetIterationCategory());
                    break;
                case eSolutionCategories.UserCategory1:
                    SolutionCategories.Add(new SolutionCategory(eSolutionCategories.UserCategory1));
                    break;
                case eSolutionCategories.UserCategory2:
                    SolutionCategories.Add(new SolutionCategory(eSolutionCategories.UserCategory2));
                    break;
                case eSolutionCategories.UserCategory3:
                    SolutionCategories.Add(new SolutionCategory(eSolutionCategories.UserCategory3));
                    break;
                default:
                    break;
                }
            }

        private SolutionCategory GetIterationCategory()
            {
            SolutionCategory iteration = new SolutionCategory(eSolutionCategories.Iteration);
            iteration.CategoryOptionalValues.Add(new SolutionCategoryValue("Iteration 1"));
            iteration.CategoryOptionalValues.Add(new SolutionCategoryValue("Iteration 2"));
            return iteration;
            }

        private SolutionCategory GetReleaseCategory()
            {
            SolutionCategory release = new SolutionCategory(eSolutionCategories.Release);
            release.CategoryOptionalValues.Add(new SolutionCategoryValue("Release 1"));
            release.CategoryOptionalValues.Add(new SolutionCategoryValue("Release 2"));
            return release;
            }

        private SolutionCategory GetProductCategory()
            {
            SolutionCategory product = new SolutionCategory(eSolutionCategories.Product);
            product.CategoryOptionalValues.Add(new SolutionCategoryValue("Product 1"));
            product.CategoryOptionalValues.Add(new SolutionCategoryValue("Product 2"));
            return product;
            }

        private static SolutionCategory GetTestTypeCategory()
            {
            SolutionCategory testType = new SolutionCategory(eSolutionCategories.TestType);
            testType.CategoryOptionalValues.Add(new SolutionCategoryValue("Regression"));
            testType.CategoryOptionalValues.Add(new SolutionCategoryValue("Progression"));
            testType.CategoryOptionalValues.Add(new SolutionCategoryValue("Sanity"));
            testType.CategoryOptionalValues.Add(new SolutionCategoryValue("Acceptance"));
            testType.CategoryOptionalValues.Add(new SolutionCategoryValue("End to End"));
            testType.CategoryOptionalValues.Add(new SolutionCategoryValue("Security"));
            testType.CategoryOptionalValues.Add(new SolutionCategoryValue("Performance"));
            testType.CategoryOptionalValues.Add(new SolutionCategoryValue("Data Creation"));
            return testType;
            }

        private SolutionCategory GetSubBusinessProcessTagCategory()
            {
            SolutionCategory subBusinessProcessTags = new(eSolutionCategories.SubBusinessProcessTag);
            subBusinessProcessTags.CategoryOptionalValues.Add(new SolutionCategoryValue("Customer Payment (CP)"));
            subBusinessProcessTags.CategoryOptionalValues.Add(new SolutionCategoryValue("Customer Question (CHQ)"));
            subBusinessProcessTags.CategoryOptionalValues.Add(new SolutionCategoryValue("Order Creation (COP)"));

            return subBusinessProcessTags;
            }

        private SolutionCategory GetBusinessProcessTagCategory()
            {
            SolutionCategory businessProcessTags = new(eSolutionCategories.BusinessProcessTag);
            businessProcessTags.CategoryOptionalValues.Add(new SolutionCategoryValue("Prepaid"));
            businessProcessTags.CategoryOptionalValues.Add(new SolutionCategoryValue("Postpaid"));
            businessProcessTags.CategoryOptionalValues.Add(new SolutionCategoryValue("Customer Creation"));
            businessProcessTags.CategoryOptionalValues.Add(new SolutionCategoryValue("Subscriber Creation"));
            businessProcessTags.CategoryOptionalValues.Add(new SolutionCategoryValue("Contact Creation"));
            return businessProcessTags;
            }

        public List<ApplicationPlatform> GetListOfPomSupportedPlatform()
            {
            if (ApplicationPlatforms != null)
                {
                if (ApplicationPlatforms.Count != 0)
                    {
                    return ApplicationPlatforms.Where(x => ApplicationPOMModel.PomSupportedPlatforms.Contains(x.Platform)).ToList();
                    }
                }
            return [];
            }

        public void SetSealightsOldConifurationsToNewObject()
            {
            if (!string.IsNullOrEmpty(this.LoggerConfigurations.SealightsURL) && string.IsNullOrEmpty(this.SealightsConfiguration.SealightsURL))
                {
                this.SealightsConfiguration.SealightsURL = this.LoggerConfigurations.SealightsURL;
                this.SealightsConfiguration.SealightsAgentToken = this.LoggerConfigurations.SealightsAgentToken;
                this.SealightsConfiguration.SealightsBuildSessionID = this.LoggerConfigurations.SealightsBuildSessionID;
                this.SealightsConfiguration.SealightsLabId = this.LoggerConfigurations.SealightsLabId;
                this.SealightsConfiguration.SealightsLog = this.LoggerConfigurations.SealightsLog;
                this.SealightsConfiguration.SealightsReportedEntityLevel = this.LoggerConfigurations.SealightsReportedEntityLevel;
                this.SealightsConfiguration.SealightsSessionTimeout = this.LoggerConfigurations.SealightsSessionTimeout;
                this.SealightsConfiguration.SealightsTestStage = this.LoggerConfigurations.SealightsTestStage;
                }
            }


        [IsSerializedForLocalRepository]
        public AskLisaConfiguration AskLisaConfiguration { get; set; } = new AskLisaConfiguration();
        }
    }
