#region License
/*
Copyright © 2014-2020 European Support Limited

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
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCore.Activities;
using GingerCore.ALM.Rally;
using GingerCore.ALM.RQM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace GingerCore.ALM
{

    public abstract class ALMCore
    {
        public static ObservableList<GingerCoreNET.ALMLib.ALMConfig> AlmConfigs { get; set; } = new ObservableList<GingerCoreNET.ALMLib.ALMConfig>();

        public static GingerCoreNET.ALMLib.ALMConfig DefaultAlmConfig { get; set; }

        public GingerCoreNET.ALMLib.ALMConfig GetCurrentAlmConfig()
        {
            GingerCoreNET.ALMLib.ALMConfig AlmConfig = null;
            if (this.GetType() == typeof(GingerCore.ALM.QCCore))
            {
                AlmConfig = WorkSpace.Instance.Solution.ALMConfigs.Where(x => x.AlmType == GingerCoreNET.ALMLib.ALMIntegration.eALMType.QC).FirstOrDefault();
            }
            if (this.GetType() == typeof(GingerCore.ALM.QCRestAPICore))
            {
                AlmConfig = WorkSpace.Instance.Solution.ALMConfigs.Where(x => x.AlmType == GingerCoreNET.ALMLib.ALMIntegration.eALMType.QC).FirstOrDefault();
            }
            if (this.GetType() == typeof(GingerCore.ALM.RQMCore))
            {
                AlmConfig = WorkSpace.Instance.Solution.ALMConfigs.Where(x => x.AlmType == GingerCoreNET.ALMLib.ALMIntegration.eALMType.RQM).FirstOrDefault();
            }
            if (this.GetType() == typeof(GingerCore.ALM.JiraCore))
            {
                AlmConfig = WorkSpace.Instance.Solution.ALMConfigs.Where(x => x.AlmType == GingerCoreNET.ALMLib.ALMIntegration.eALMType.Jira).FirstOrDefault();
            }
            if (this.GetType() == typeof(GingerCore.ALM.QtestCore))
            {
                AlmConfig = WorkSpace.Instance.Solution.ALMConfigs.Where(x => x.AlmType == GingerCoreNET.ALMLib.ALMIntegration.eALMType.Qtest).FirstOrDefault();
            }
            if (this.GetType() == typeof(GingerCore.ALM.OctaneCore))
            {
                AlmConfig = WorkSpace.Instance.Solution.ALMConfigs.Where(x => x.AlmType == GingerCoreNET.ALMLib.ALMIntegration.eALMType.Octane).FirstOrDefault();
            }

            if (this.GetType() == typeof(GingerCore.ALM.RallyCore))
            {
                AlmConfig = WorkSpace.Instance.Solution.ALMConfigs.Where(x => x.AlmType == GingerCoreNET.ALMLib.ALMIntegration.eALMType.RALLY).FirstOrDefault();
            }

            if (this.GetType() == typeof(GingerCore.ALM.ZephyrEntCore))
            {
                AlmConfig = WorkSpace.Instance.Solution.ALMConfigs.Where(x => x.AlmType == GingerCoreNET.ALMLib.ALMIntegration.eALMType.ZephyrEnterprise).FirstOrDefault();
            }

            if (AlmConfig != null)
            {
                GingerCoreNET.ALMLib.ALMUserConfig AlmUserConfig = WorkSpace.Instance.UserProfile.ALMUserConfigs.FirstOrDefault(x => x.AlmType == AlmConfig.AlmType);
                if (AlmUserConfig == null)
                {
                    AlmUserConfig = new GingerCoreNET.ALMLib.ALMUserConfig();
                    AlmUserConfig.AlmType = AlmConfig.AlmType;
                    WorkSpace.Instance.UserProfile.ALMUserConfigs.Add(AlmUserConfig);
                }
                AlmConfig.ALMUserName = AlmUserConfig.ALMUserName;
                AlmConfig.ALMPassword = AlmUserConfig.ALMPassword;
                AlmConfig.ALMToken = AlmUserConfig.ALMToken;
            }
            else
            {
                AlmConfig = AlmConfigs.FirstOrDefault();
            }
            DefaultAlmConfig = AlmConfig;
            return AlmConfig;

        }
        
        public static string SolutionFolder { get; set; }
        public ObservableList<ExternalItemFieldBase> AlmItemFields { get; set; }
        public abstract bool ConnectALMServer(); 
        public abstract bool ConnectALMProject();
        public abstract Boolean IsServerConnected();
        public abstract void DisconnectALMServer();
        public abstract bool DisconnectALMProjectStayLoggedIn();
        public abstract List<string> GetALMDomains();
        public abstract Dictionary<string, string> GetALMDomainProjects(string ALMDomainName);
        public abstract bool ExportExecutionDetailsToALM(BusinessFlow bizFlow, ref string result, bool exectutedFromAutomateTab = false, PublishToALMConfig publishToALMConfig = null);
        public abstract ObservableList<ExternalItemFieldBase> GetALMItemFields(BackgroundWorker bw, bool online, ALM_Common.DataContracts.ResourceType resourceType = ALM_Common.DataContracts.ResourceType.ALL);
        public abstract Dictionary<Guid, string> CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, List<ExternalItemFieldBase> defectsFields, bool useREST = false);

        public virtual void SetALMConfigurations(   string ALMServerUrl, bool UseRest, string ALMUserName, string ALMPassword,
                                                    string ALMDomain, string ALMProject, string ALMProjectKey, GingerCoreNET.ALMLib.ALMIntegration.eALMType almType,
                                                    string ALMConfigPackageFolderPath, bool ZephyrEntToken, string ALMToken, GingerCoreNET.ALMLib.ALMIntegration.eTestingALMType jiraTestingALM = GingerCoreNET.ALMLib.ALMIntegration.eTestingALMType.None)
        {
            GingerCoreNET.ALMLib.ALMConfig AlmConfig = ALMCore.AlmConfigs.FirstOrDefault(x => x.AlmType == almType);
            if (AlmConfig == null)
            {
                AlmConfig = new GingerCoreNET.ALMLib.ALMConfig();
                AlmConfigs.Add(AlmConfig);
            }

            GingerCoreNET.ALMLib.ALMUserConfig CurrentAlmUserConfigurations = WorkSpace.Instance.UserProfile.ALMUserConfigs.FirstOrDefault(x => x.AlmType == almType);
            if (CurrentAlmUserConfigurations == null)
            {
                CurrentAlmUserConfigurations = new GingerCoreNET.ALMLib.ALMUserConfig();
                CurrentAlmUserConfigurations.AlmType = almType;
                WorkSpace.Instance.UserProfile.ALMUserConfigs.Add(CurrentAlmUserConfigurations);
            }

            if (AlmConfig == null)
            {
                AlmConfig = new GingerCoreNET.ALMLib.ALMConfig();
                AlmConfigs.Add(AlmConfig);
            }
            AlmConfig.ALMServerURL = ALMServerUrl;
            AlmConfig.UseRest = UseRest;
            AlmConfig.ALMUserName = CurrentAlmUserConfigurations.ALMUserName;
            AlmConfig.ALMPassword = CurrentAlmUserConfigurations.ALMPassword;
            AlmConfig.ALMToken = CurrentAlmUserConfigurations.ALMToken;
            AlmConfig.ZepherEntToken = ZephyrEntToken;
            AlmConfig.ALMDomain = ALMDomain;
            AlmConfig.ALMProjectName = ALMProject;
            AlmConfig.ALMProjectKey = ALMProjectKey;
            AlmConfig.AlmType = almType;
            AlmConfig.JiraTestingALM = jiraTestingALM;

            if (!String.IsNullOrEmpty(ALMConfigPackageFolderPath))
            {
                AlmConfig.ALMConfigPackageFolderPath = ALMConfigPackageFolderPath;
            }
        }

        public BusinessFlow ConvertRQMTestPlanToBF(RQMTestPlan testPlan)
        {
            return ImportFromRQM.ConvertRQMTestPlanToBF(testPlan);
        }

        public BusinessFlow ConvertRallyTestPlanToBF(RallyTestPlan testPlan)
        {
            return ImportFromRally.ConvertRallyTestPlanToBF(testPlan);
        }

        public abstract ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo
        {
            get; set;
        }

        public abstract ObservableList<Activity> GingerActivitiesRepo
        {
            get; set;
        }

        internal ObservableList<ExternalItemFieldBase> UpdatedAlmFields(ObservableList<ExternalItemFieldBase> tempFieldsList)
        {
            AlmItemFields = new ObservableList<ExternalItemFieldBase>();
            foreach (ExternalItemFieldBase item in tempFieldsList)
            {
                AlmItemFields.Add((ExternalItemFieldBase)item.CreateCopy());
            }
            return tempFieldsList;
        }
    }
}
