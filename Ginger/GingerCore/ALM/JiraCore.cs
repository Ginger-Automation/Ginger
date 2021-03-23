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
using GingerCore.ALM.JIRA;
using Ginger;
using System;
using System.Collections.Generic;
using GingerCore.Activities;
using ALM_Common.DataContracts;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.IO.Compression;
using Newtonsoft.Json;
using GingerCore.External;
using Amdocs.Ginger.Repository;
using GingerCore.ALM.JIRA.Bll;
using JiraRepository.BLL;
using JiraRepository.Data_Contracts;
using amdocs.ginger.GingerCoreNET;
using System.Linq;

namespace GingerCore.ALM
{
    public class JiraCore : ALMCore
    {
        private JIRA.Bll.JiraExportManager exportMananger;
        private JiraConnectManager jiraConnectObj;
        private JiraImportManager jiraImportObj;
        private JiraRepository.JiraRepository jiraRepObj;
        public static string ALMProjectGroupName { get; set; }
        public static string ALMProjectGuid { get; set; }
        public override ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo
        {
            get { return jiraImportObj.GingerActivitiesGroupsRepo; }
            set { jiraImportObj.GingerActivitiesGroupsRepo = value; }
        }
        public override ObservableList<Activity> GingerActivitiesRepo
        {
            get { return jiraImportObj.GingerActivitiesRepo; }
            set { jiraImportObj.GingerActivitiesRepo = value; }
        }
        public JiraCore()
        {
            string settingsPath = DefaultAlmConfig.ALMConfigPackageFolderPath;
            jiraRepObj = new JiraRepository.JiraRepository(settingsPath, (TestingALMType)Enum.Parse(typeof(TestingALMType), ALMCore.DefaultAlmConfig.JiraTestingALM.ToString()));
            exportMananger = new JIRA.Bll.JiraExportManager(jiraRepObj);
            jiraConnectObj = new JiraConnectManager(jiraRepObj);
            jiraImportObj = new JiraImportManager(jiraRepObj);
        }
        public override bool ConnectALMProject()
        {
            return jiraConnectObj.SetJiraProjectFullDetails();
        }

        public override bool ConnectALMServer()
        {
            return jiraConnectObj.ConnectJiraServer();
        }

        public override Dictionary<Guid, string> CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, List<ExternalItemFieldBase> defectsFields, bool useREST = false)
        {
            switch (ALMCore.DefaultAlmConfig.JiraTestingALM)
            {
                case GingerCoreNET.ALMLib.ALMIntegration.eTestingALMType.Xray:
                    return exportMananger.CreateNewALMDefects(defectsForOpening, defectsFields);
                case GingerCoreNET.ALMLib.ALMIntegration.eTestingALMType.Zephyr:
                    Dictionary<Guid, string> defectsOpeningResults = exportMananger.CreateNewALMDefects(defectsForOpening, defectsFields);
                    exportMananger.AssignDefectsToZephyrExecutions(defectsForOpening, defectsOpeningResults);
                    return defectsOpeningResults;
                default:
                    return exportMananger.CreateNewALMDefects(defectsForOpening, defectsFields);
            }
        }

        public override bool DisconnectALMProjectStayLoggedIn()
        {
            return jiraConnectObj.DisconnectALMProjectStayLoggedIn();
        }

        public override void DisconnectALMServer()
        {
            jiraConnectObj.DisconnectJiraServer();
        }

        public override bool ExportExecutionDetailsToALM(BusinessFlow bizFlow, ref string result, bool exectutedFromAutomateTab = false, PublishToALMConfig publishToALMConfig = null)
        {
            switch (ALMCore.DefaultAlmConfig.JiraTestingALM)
            {
                case GingerCoreNET.ALMLib.ALMIntegration.eTestingALMType.Xray:
                    return exportMananger.ExecuteDataToJira(bizFlow, publishToALMConfig, ref result);
                case GingerCoreNET.ALMLib.ALMIntegration.eTestingALMType.Zephyr:
                    return exportMananger.ExportExecutionDetailsToJiraZephyr(bizFlow, publishToALMConfig, ref result);
                default:
                    return exportMananger.ExecuteDataToJira(bizFlow, publishToALMConfig, ref result);
            }
        }

        public override Dictionary<string, string> GetALMDomainProjects(string ALMDomainName)
        {
            ALMCore.DefaultAlmConfig.ALMDomain = ALMDomainName;
            return jiraConnectObj.GetJiraDomainProjects();
        }

        public override List<string> GetALMDomains()
        {
            return jiraConnectObj.GetJiraDomains();
        }

        public List<string> GetJiraTestingALMs()
        {
            return jiraConnectObj.GetJiraTestingALMs();
        }

        public override ObservableList<ExternalItemFieldBase> GetALMItemFields(BackgroundWorker bw, bool online, ResourceType resourceType = ResourceType.ALL)
        {
            ObservableList<ExternalItemFieldBase> tempFieldsList = jiraImportObj.GetALMItemFields(resourceType, bw, online);
            AlmItemFields = new ObservableList<ExternalItemFieldBase>();
            foreach (ExternalItemFieldBase item in tempFieldsList)
            {
                AlmItemFields.Add((ExternalItemFieldBase)item.CreateCopy());
            }
            return tempFieldsList;
        }

        public override bool IsServerConnected()
        {
            throw new NotImplementedException();
        }

        public bool ExportActivitiesGroupToALM(ActivitiesGroup activtiesGroup, IEnumerable<ExternalItemFieldBase> testCaseFields, ref string errorResult)
        {
            return exportMananger.ExportActivitesGrToJira(activtiesGroup, testCaseFields, ref errorResult);
        }

        public bool ExportBfToAlm(BusinessFlow businessFlow, IEnumerable<ExternalItemFieldBase> testCaseFields, IEnumerable<ExternalItemFieldBase> testSetFields, IEnumerable<ExternalItemFieldBase> testExecutionFields, ref string responseStr)
        {
            return exportMananger.ExportBfToAlm(businessFlow, testCaseFields, testSetFields, testExecutionFields, ref responseStr);
        }

        public bool ExportBfToZephyr(   BusinessFlow businessFlow, IEnumerable<ExternalItemFieldBase> testCaseFields,
                                        IEnumerable<ExternalItemFieldBase> testSetFields, IEnumerable<ExternalItemFieldBase> testExecutionFields,
                                        ref string responseStr, string versionId, string cycleId)
        {
            return exportMananger.ExportBfToZephyr(businessFlow, testCaseFields, testSetFields, testExecutionFields, ref responseStr, versionId, cycleId);
        }

        public ObservableList<JiraTestSet> GetJiraTestSets()
        {
            return jiraImportObj.GetJiraTestSets();
        }

        public JiraZephyrCyclesCollection GetZephyrCyclesWithFolders(bool getFolders = false)
        {
            JiraZephyrCyclesCollection jiraZephyrCyclesCollection = ((JiraManagerZephyr)jiraImportObj.JiraRepObj().TestAlmManager()).GetZephyrCyclesList(DefaultAlmConfig.ALMUserName, DefaultAlmConfig.ALMPassword, DefaultAlmConfig.ALMServerURL,
                                                                                                                                                            DefaultAlmConfig.ALMProjectKey, string.Empty, string.Empty, string.Empty, string.Empty).DataResult;
            if (getFolders)
            {
                jiraZephyrCyclesCollection.projectsReleasesList.ForEach(z => z.releasesCycles.ForEach(y => y.FoldersList = ((JiraManagerZephyr)jiraImportObj.JiraRepObj().TestAlmManager()).GetCycleFoldersList(DefaultAlmConfig.ALMUserName, DefaultAlmConfig.ALMPassword, DefaultAlmConfig.ALMServerURL,
                                                                                                                              DefaultAlmConfig.ALMProjectKey, y.versionId.ToString(), y.id.ToString(), string.Empty, string.Empty).DataResult));
            }
            return jiraZephyrCyclesCollection;
        }

        public JiraZephyrCycle GetZephyrCycleOrFolderWithIssuesAndStepsAsCycle(string versionId, string cycleId, string folderId = "-1")
        {
            JiraZephyrCycle cycle = ((JiraManagerZephyr)jiraImportObj.JiraRepObj().TestAlmManager()).GetZephyrCycle(DefaultAlmConfig.ALMUserName, DefaultAlmConfig.ALMPassword,
                                                                                                                        DefaultAlmConfig.ALMServerURL, Convert.ToInt32(cycleId)).DataResult;
            if (cycle != null)
            {
                cycle.IssuesList = new List<JiraZephyrIssue>();
                List<JiraZephyrExecution> issuesAsExecutionList = ((JiraManagerZephyr)jiraImportObj.JiraRepObj().TestAlmManager()).GetZephyrExecutionList(DefaultAlmConfig.ALMUserName, DefaultAlmConfig.ALMPassword, DefaultAlmConfig.ALMServerURL,
                                                                                                                                                            string.Empty, versionId, DefaultAlmConfig.ALMProjectKey, cycleId, string.Empty, string.Empty,
                                                                                                                                                            string.Empty, string.Empty, folderId).DataResult;
                if (issuesAsExecutionList != null)
                {
                    foreach (JiraZephyrExecution issuesAsExecution in issuesAsExecutionList.OrderBy(z => z.OrderId))
                    {
                        JiraZephyrIssue jiraZephyrIssue = new JiraZephyrIssue();
                        jiraZephyrIssue.name = issuesAsExecution.Summary;
                        jiraZephyrIssue.key = issuesAsExecution.IssueKey;
                        jiraZephyrIssue.id = issuesAsExecution.IssueId;
                        jiraZephyrIssue.Steps = ((JiraManagerZephyr)jiraImportObj.JiraRepObj().TestAlmManager()).GetZephyrTestStepsList(DefaultAlmConfig.ALMUserName, DefaultAlmConfig.ALMPassword, DefaultAlmConfig.ALMServerURL,
                                                                                                                                                                    issuesAsExecution.IssueId).DataResult.stepBeanCollection;
                        cycle.IssuesList.Add(jiraZephyrIssue);
                    }
                }
            }
            return cycle;
        }

        public JiraTestSet GetJiraTestSetData(JiraTestSet selectedTS)
        {
            return jiraImportObj.GetTestSetData(selectedTS);
        }

        public BusinessFlow ConvertJiraTestSetToBF(JiraTestSet testSet)
        {
            return jiraImportObj.ConvertJiraTestSetToBF(testSet);
        }

        public BusinessFlow ConvertJiraZypherCycleToBF(JiraZephyrCycle cycle)
        {
            return jiraImportObj.ConvertJiraZypherCycleToBF(cycle);
        }

        public Dictionary<string, JiraTest> GetJiraTestsUpdatedData(string testSetID, List<string> TCsIDs = null)
        {
            return jiraImportObj.GetJiraSelectedTestsData(testSetID, TCsIDs);
        }

        public void UpdateBFSelectedAG(ref BusinessFlow businessFlow, Dictionary<string, JiraTest> activitiesGroupToUpdatedData)
        {
            jiraImportObj.UpdateBFSelectedAG(ref businessFlow, activitiesGroupToUpdatedData);
        }
        public void UpdateBussinessFlow(ref BusinessFlow businessFlow)
        {
            jiraImportObj.UpdateBussinessFlow(ref businessFlow);
        }
        public bool ValidateConfigurationFile(string PackageFileName)
        {
            bool containJiraSettingsFile = false;

           
            using (FileStream configPackageZipFile = new FileStream(PackageFileName, FileMode.Open))
            {
                using (ZipArchive zipArchive = new ZipArchive(configPackageZipFile))
                {
                    foreach (ZipArchiveEntry entry in zipArchive.Entries)
                        if (entry.FullName == @"JiraSettings/JiraSettings.json")
                        {
                            containJiraSettingsFile = true;
                            break;
                        }
                }
            }
            return containJiraSettingsFile;
        }
        public bool IsConfigPackageExists()
        {
            string CurrJiraConfigPath = Path.Combine(ALMCore.SolutionFolder, "Configurations", "JiraConfigurationsPackage");
            if (Directory.Exists(Path.Combine(CurrJiraConfigPath, "JiraSettings")))
            {
                ALMCore.DefaultAlmConfig.ALMConfigPackageFolderPath = CurrJiraConfigPath;
                jiraConnectObj.CreateJiraRepository();
                return true;
            }
            else
            {
                Reporter.ToLog(eLogLevel.WARN, "Jira Configuration package not exist in solution, Jira Settings not exist at: " + Path.Combine(CurrJiraConfigPath, "JiraSettings"));
            }
            return false;
        }
    }
}
