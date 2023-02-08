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

using AlmDataContractsStd.Enums;
//using ALM_Common.DataContracts;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Activities;
using GingerCore.ALM.JIRA;
using GingerCoreNET.ALMLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using JiraRepositoryStd;
using JiraRepositoryStd.BLL;
using JiraRepositoryStd.Data_Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace GingerCore.ALM
{
    public class JiraCore : ALMCore
    {
        private JIRA.Bll.JiraExportManager exportMananger;
        private JiraConnectManager jiraConnectObj;
        private JiraImportManager jiraImportObj;
        private JiraManagerZephyr jmz;
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
        public override ObservableList<ApplicationPlatform> ApplicationPlatforms
        {
            get { return jiraImportObj.ApplicationPlatforms; }
            set { jiraImportObj.ApplicationPlatforms = value; }
        }

        public override ALMIntegrationEnums.eALMType ALMType => ALMIntegrationEnums.eALMType.Jira;

        public JiraCore()
        {
            string settingsPath = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(DefaultAlmConfig.ALMConfigPackageFolderPath);
            JiraRepositoryStd.JiraRepositoryStd jiraRepObj = new JiraRepositoryStd.JiraRepositoryStd(settingsPath, (TestingALMType)Enum.Parse(typeof(TestingALMType), ALMCore.DefaultAlmConfig.JiraTestingALM.ToString()));
            exportMananger = new JIRA.Bll.JiraExportManager(jiraRepObj);
            jiraConnectObj = new JiraConnectManager(jiraRepObj);
            jiraImportObj = new JiraImportManager(jiraRepObj);
            jmz = new JiraManagerZephyr();
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
                case GingerCoreNET.ALMLib.ALMIntegrationEnums.eTestingALMType.Xray:
                    return exportMananger.CreateNewALMDefects(defectsForOpening, defectsFields);
                case GingerCoreNET.ALMLib.ALMIntegrationEnums.eTestingALMType.Zephyr:
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
                case GingerCoreNET.ALMLib.ALMIntegrationEnums.eTestingALMType.Xray:
                    return exportMananger.ExecuteDataToJira(bizFlow, publishToALMConfig, ref result);
                case GingerCoreNET.ALMLib.ALMIntegrationEnums.eTestingALMType.Zephyr:
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
            JiraZephyrCyclesCollection jiraZephyrCyclesCollection = jmz.GetZephyrCyclesList(DefaultAlmConfig.ALMUserName, DefaultAlmConfig.ALMPassword, DefaultAlmConfig.ALMServerURL,
                                                                                                                                                            DefaultAlmConfig.ALMProjectKey, string.Empty, string.Empty, string.Empty, string.Empty).DataResult;
            if (getFolders)
            {
                jiraZephyrCyclesCollection.projectsReleasesList.ForEach(z => z.releasesCycles.ForEach(y => y.FoldersList = jmz.GetCycleFoldersList(DefaultAlmConfig.ALMUserName, DefaultAlmConfig.ALMPassword, DefaultAlmConfig.ALMServerURL,
                                                                                                                              DefaultAlmConfig.ALMProjectKey, y.versionId.ToString(), y.id.ToString(), string.Empty, string.Empty).DataResult));
            }
            return jiraZephyrCyclesCollection;
        }

        public JiraZephyrCycle GetZephyrCycleOrFolderWithIssuesAndStepsAsCycle(string versionId, string cycleId, string folderId = "-1")
        {
            JiraZephyrCycle cycle = jmz.GetZephyrCycle(DefaultAlmConfig.ALMUserName, DefaultAlmConfig.ALMPassword,
                                                                                                                        DefaultAlmConfig.ALMServerURL, Convert.ToInt32(cycleId)).DataResult;
            if (cycle != null)
            {
                cycle.IssuesList = new List<JiraZephyrIssue>();
                List<JiraZephyrExecution> issuesAsExecutionList = jmz.GetZephyrExecutionList(DefaultAlmConfig.ALMUserName, DefaultAlmConfig.ALMPassword, DefaultAlmConfig.ALMServerURL,
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
                        jiraZephyrIssue.Steps = jmz.GetZephyrTestStepsList(DefaultAlmConfig.ALMUserName, DefaultAlmConfig.ALMPassword, DefaultAlmConfig.ALMServerURL,
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
        
        public void CreateJiraRepository()
        {
            jiraConnectObj.CreateJiraRepository();
        }
    }
}
