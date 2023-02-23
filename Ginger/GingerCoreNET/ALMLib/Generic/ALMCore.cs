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
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCore.Activities;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AlmDataContractsStd.Enums;
//using ALM_Common.DataContracts;
using static GingerCoreNET.ALMLib.ALMIntegrationEnums;

namespace GingerCore.ALM
{

    public abstract class ALMCore : IALMCore
    {
        public static ObservableList<GingerCoreNET.ALMLib.ALMConfig> AlmConfigs { get; set; } = new ObservableList<GingerCoreNET.ALMLib.ALMConfig>();

        public static GingerCoreNET.ALMLib.ALMConfig DefaultAlmConfig { get; set; }
        public bool IsConnectValidationDone { get; set; }
        public GingerCoreNET.ALMLib.ALMConfig GetCurrentAlmConfig(bool isOperationAlmType = false)
        {
            GingerCoreNET.ALMLib.ALMConfig AlmConfig = null;
            if (isOperationAlmType)
            {
                AlmConfig = WorkSpace.Instance.Solution.ALMConfigs.Where(x => x.AlmType == this.ALMType).FirstOrDefault();
            }
            else
            {
                AlmConfig = WorkSpace.Instance.Solution.ALMConfigs.Where(x => x.DefaultAlm).FirstOrDefault();
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
                if (AlmUserConfig.ALMServerURL != null)
                {
                    AlmConfig.ALMServerURL = AlmUserConfig.ALMServerURL;
                }
                if (AlmUserConfig.ALMConfigPackageFolderPath != null)
                {
                    AlmConfig.ALMConfigPackageFolderPath = AlmUserConfig.ALMConfigPackageFolderPath;
                }
                if (!string.IsNullOrEmpty(AlmConfig.ALMConfigPackageFolderPath))
                {
                    AlmConfig.ALMConfigPackageFolderPath = WorkSpace.Instance.SolutionRepository.ConvertFullPathToBeRelative(AlmConfig.ALMConfigPackageFolderPath);
                }
                AlmConfig.ALMUserName = AlmUserConfig.ALMUserName;
                AlmConfig.ALMPassword = AlmUserConfig.ALMPassword;
            }
            else
            {
                AlmConfig = AlmConfigs.FirstOrDefault();
            }
            DefaultAlmConfig = AlmConfig;
            return AlmConfig;
        }
        public static string SolutionFolder { get; set; }
        public static ObservableList<ExternalItemFieldBase> AlmItemFields { get; set; }
        public abstract bool ConnectALMServer();
        public abstract bool ConnectALMProject();
        public abstract Boolean IsServerConnected();
        public abstract void DisconnectALMServer();
        public abstract bool DisconnectALMProjectStayLoggedIn();
        public abstract List<string> GetALMDomains();
        public abstract Dictionary<string, string> GetALMDomainProjects(string ALMDomainName);
        public abstract bool ExportExecutionDetailsToALM(BusinessFlow bizFlow, ref string result, bool exectutedFromAutomateTab = false, PublishToALMConfig publishToALMConfig = null);
        public abstract ObservableList<ExternalItemFieldBase> GetALMItemFields(BackgroundWorker bw, bool online, AlmDataContractsStd.Enums.ResourceType resourceType = AlmDataContractsStd.Enums.ResourceType.ALL);
        public abstract Dictionary<Guid, string> CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, List<ExternalItemFieldBase> defectsFields, bool useREST = false);


        public virtual Dictionary<string, string> GetSSOTokens()
        {
            return null;
        }

        public virtual Dictionary<string, string> GetTokenInfo()
        {
            return null;
        }

        public virtual Dictionary<string, string> GetConnectionInfo()
        {
            return null;
        }


        public virtual void SetALMConfigurations(string ALMServerUrl, bool UseRest, string ALMUserName, string ALMPassword,
                                                    string ALMDomain, string ALMProject, string ALMProjectKey, GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType almType,
                                                    string ALMConfigPackageFolderPath, bool UseToken, GingerCoreNET.ALMLib.ALMIntegrationEnums.eTestingALMType jiraTestingALM = GingerCoreNET.ALMLib.ALMIntegrationEnums.eTestingALMType.None)
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
            if (CurrentAlmUserConfigurations.ALMServerURL != null)
            {
                AlmConfig.ALMServerURL = CurrentAlmUserConfigurations.ALMServerURL;
            }
            else
            {
                AlmConfig.ALMServerURL = ALMServerUrl;
            }
            AlmConfig.UseRest = UseRest;
            AlmConfig.ALMUserName = CurrentAlmUserConfigurations.ALMUserName;
            AlmConfig.ALMPassword = CurrentAlmUserConfigurations.ALMPassword;
            AlmConfig.UseToken = UseToken;
            AlmConfig.ALMDomain = ALMDomain;
            AlmConfig.ALMProjectName = ALMProject;
            AlmConfig.ALMProjectKey = ALMProjectKey;
            AlmConfig.AlmType = almType;
            AlmConfig.JiraTestingALM = jiraTestingALM;

            if (CurrentAlmUserConfigurations.ALMConfigPackageFolderPath != null)
            {
                AlmConfig.ALMConfigPackageFolderPath = CurrentAlmUserConfigurations.ALMConfigPackageFolderPath;
            }
            else
            {
                if (!String.IsNullOrEmpty(ALMConfigPackageFolderPath))
                {
                    AlmConfig.ALMConfigPackageFolderPath = ALMConfigPackageFolderPath;
                }
            }
            if (!string.IsNullOrEmpty(AlmConfig.ALMConfigPackageFolderPath))
            {
                AlmConfig.ALMConfigPackageFolderPath = WorkSpace.Instance.SolutionRepository.ConvertFullPathToBeRelative(AlmConfig.ALMConfigPackageFolderPath);
            }
        }
        public abstract ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo
        {
            get; set;
        }

        public abstract ObservableList<Activity> GingerActivitiesRepo
        {
            get; set;
        }

        public abstract ObservableList<ApplicationPlatform> ApplicationPlatforms
        {
            get; set;
        }

        public ObservableList<ExternalItemFieldBase> UpdatedAlmFields(ObservableList<ExternalItemFieldBase> tempFieldsList)
        {
            AlmItemFields = new ObservableList<ExternalItemFieldBase>();
            foreach (ExternalItemFieldBase item in tempFieldsList)
            {
                AlmItemFields.Add((ExternalItemFieldBase)item.CreateCopy());
            }
            return tempFieldsList;
        }

        public void InitCoreObjs()
        {
            this.GingerActivitiesGroupsRepo = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ActivitiesGroup>();
            this.GingerActivitiesRepo = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
            this.ApplicationPlatforms = WorkSpace.Instance.Solution.ApplicationPlatforms;
        }
        public abstract GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType ALMType
        {
            get;
        }

        public string name => throw new NotImplementedException();

        public bool ExportBusinessFlowsResultToALM(ObservableList<BusinessFlow> BusinessFlows, ref string result, PublishToALMConfig publishToALMConfig, eALMConnectType almConnectionType, bool exectutedFromAutomateTab = false)
        {
            SolutionFolder = WorkSpace.Instance.Solution.Folder.ToUpper();
            bool isExportSucc = false;
            try
            {
                foreach (BusinessFlow BizFlow in BusinessFlows) //Here going for each businessFlow
                {
                    try
                    {
                        if (BizFlow.ExternalID != "0" && !String.IsNullOrEmpty(BizFlow.ExternalID))
                        {
                            Reporter.ToLog(eLogLevel.DEBUG, "Executing RunSet Action Publish to ALM for " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " " + BizFlow.Name);
                            Reporter.ToStatus(eStatusMsgKey.ExportExecutionDetails, null, BizFlow.Name, "ALM");

                            if (publishToALMConfig.ToAttachActivitiesGroupReport)
                            {
                                Ginger.Reports.GingerExecutionReport.ExtensionMethods.CreateActivitiesGroupReportsOfBusinessFlow(null, BizFlow);//need to find a way to specify the releveant environment 
                            }
                            isExportSucc = ExportExecutionDetailsToALM(BizFlow, ref result, exectutedFromAutomateTab, publishToALMConfig);
                            if (isExportSucc)
                            {
                                BizFlow.PublishStatus = BusinessFlow.ePublishStatus.Published;
                            }
                            else
                            {
                                if ((result == null) || (result == string.Empty))
                                {
                                    result = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " - " + BizFlow.Name + " - Error when uploading to ALM." + Environment.NewLine;
                                }
                                BizFlow.PublishStatus = BusinessFlow.ePublishStatus.PublishFailed;
                            }
                            Reporter.HideStatusMessage();
                        }
                        else
                        {
                            BizFlow.PublishStatus = BusinessFlow.ePublishStatus.NotPublished;
                            result = $"{result}{GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)} - {BizFlow.Name} - doesn't have ExternalID, cannot execute publish to ALM RunSet Action{Environment.NewLine}";
                            Reporter.ToLog(eLogLevel.WARN, BizFlow.Name + " - doesn't have ExternalID, cannot execute publish to ALM RunSet Action");
                        }
                    }
                    catch (Exception ex)
                    {
                        result = ex.Message;
                        BizFlow.PublishStatus = BusinessFlow.ePublishStatus.NotPublished;
                        Reporter.ToLog(eLogLevel.ERROR, BizFlow.Name + " - Export results to ALM failed due to exception", ex);
                    }
                }

                return isExportSucc;
            }
            finally
            {
                DisconnectALMServer();
            }
            
        }
        public static GingerCoreNET.ALMLib.ALMConfig GetCurrentAlmConfig(GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType almType)
        {
            GingerCoreNET.ALMLib.ALMConfig AlmConfig = WorkSpace.Instance.Solution.ALMConfigs.FirstOrDefault(x => x.AlmType == almType);
            if (AlmConfig == null)
            {
                AlmConfig = new GingerCoreNET.ALMLib.ALMConfig();
                AlmConfig.AlmType = almType;
                WorkSpace.Instance.Solution.ALMConfigs.Add(AlmConfig);
            }
            return AlmConfig;
        }
        public static void SetALMCoreConfigurations(eALMType almType, ALMCore aLMCore)
        {
            GingerCoreNET.ALMLib.ALMConfig CurrentAlmConfigurations = GetCurrentAlmConfig(almType);

            ALMCore.SolutionFolder = WorkSpace.Instance.Solution.Folder.ToUpper();
            if (CurrentAlmConfigurations != null)
            {
                aLMCore.SetALMConfigurations(CurrentAlmConfigurations.ALMServerURL, CurrentAlmConfigurations.UseRest, CurrentAlmConfigurations.ALMUserName,
                                                CurrentAlmConfigurations.ALMPassword, CurrentAlmConfigurations.ALMDomain, CurrentAlmConfigurations.ALMProjectName,
                                                CurrentAlmConfigurations.ALMProjectKey, CurrentAlmConfigurations.AlmType, CurrentAlmConfigurations.ALMConfigPackageFolderPath,
                                                CurrentAlmConfigurations.UseToken, CurrentAlmConfigurations.JiraTestingALM);
            }
        }

        public static GingerCoreNET.ALMLib.ALMConfig GetDefaultAlmConfig()
        {
            GingerCoreNET.ALMLib.ALMConfig AlmConfig = WorkSpace.Instance.Solution.ALMConfigs.Where(x => x.DefaultAlm).FirstOrDefault();
            if (AlmConfig == null)
            {
                AlmConfig = new GingerCoreNET.ALMLib.ALMConfig();
                AlmConfig.DefaultAlm = true;
                WorkSpace.Instance.Solution.ALMConfigs.Add(AlmConfig);
            }
            return AlmConfig;
        }

        public ObservableList<ExternalItemFieldBase> RefreshALMItemFields(ObservableList<ExternalItemFieldBase> exitingFields, ObservableList<ExternalItemFieldBase> latestALMFields)
        {
            ObservableList<ExternalItemFieldBase> mergedFields = new ObservableList<ExternalItemFieldBase>();
            foreach (ExternalItemFieldBase latestField in latestALMFields)
            {
                ExternalItemFieldBase currentField = exitingFields.Where(x => x.ID == latestField.ID && x.ItemType == latestField.ItemType).FirstOrDefault();
                if (currentField != null)
                {
                    currentField.Name = latestField.Name;
                    currentField.ItemType = latestField.ItemType;
                    currentField.Mandatory = latestField.Mandatory;
                    currentField.ExternalID = latestField.ExternalID;
                    currentField.PossibleValues = latestField.PossibleValues;
                    currentField.ToUpdate = false;
                    if (string.IsNullOrEmpty(currentField.SelectedValue) == false)
                    {
                        if ((latestField.PossibleValues.Count == 0 && currentField.SelectedValue != latestField.SelectedValue) || (latestField.PossibleValues.Count > 0 && latestField.PossibleValues.Contains(currentField.SelectedValue) && currentField.SelectedValue != latestField.PossibleValues[0]))
                        {
                            currentField.ToUpdate = true;
                        }
                        else
                        {
                            currentField.SelectedValue = latestField.SelectedValue;
                            currentField.ToUpdate = false;
                        }
                    }
                    else
                    {
                        currentField.SelectedValue = latestField.SelectedValue;
                        currentField.ToUpdate = false;
                    }
                    mergedFields.Add(currentField);
                }
                else
                {
                    mergedFields.Add(latestField);
                }
            }
            exitingFields.ClearAll();
            exitingFields.Append(mergedFields);

            return mergedFields;
        }
    }
}
