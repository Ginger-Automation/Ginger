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

//using ALM_Common.DataContracts;
using AlmDataContractsStd.Contracts;
using AlmDataContractsStd.Enums;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Activities;
using GingerCore.ALM.QC;
using GingerCore.ALM.ZephyrEnt.Bll;
using GingerCoreNET.ALMLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using ZephyrEntStdSDK.Models;
using ZephyrEntStdSDK.Models.Base;

namespace GingerCore.ALM
{
    public class ZephyrEntCore : ALMCore
    {
        protected Zepyhr_Ent_Repository_Std.ZephyrEntRepositoryStd zephyrEntRepository;
        private ZephyrEntExportManager zephyrEntExportManager;
        private ZephyrEntImportManager zephyrEntImportManager;
        public override ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo { get; set; }
        public override ObservableList<Activity> GingerActivitiesRepo { get; set; }
        public override ObservableList<ApplicationPlatform> ApplicationPlatforms
        {
            get { return ZephyrEntImportManager.ApplicationPlatforms; }
            set { ZephyrEntImportManager.ApplicationPlatforms = value; }
        }

        public override ALMIntegrationEnums.eALMType ALMType => ALMIntegrationEnums.eALMType.ZephyrEnterprise;

        public ZephyrEntCore()
        {
            zephyrEntRepository = new Zepyhr_Ent_Repository_Std.ZephyrEntRepositoryStd(new LoginDTO()
            {
                User = ALMCore.DefaultAlmConfig.ALMUserName,
                Password = ALMCore.DefaultAlmConfig.ALMPassword,
                AuthToken = ALMCore.DefaultAlmConfig.ALMPassword,
                Server = ALMCore.DefaultAlmConfig.ALMServerURL
            });
            zephyrEntExportManager = new ZephyrEntExportManager(zephyrEntRepository);
            zephyrEntImportManager = new ZephyrEntImportManager(zephyrEntRepository);
        }

        public dynamic CreateNewTestPlanningFolder(long cycleId, long parenttreeid, string folderName, string folderDesc)
        {
            return zephyrEntRepository.CreateNewTestPlanningFolder(cycleId, parenttreeid, folderName, folderDesc);
        }
        public dynamic UpdateTestPlanningFolder(long cycleId, long parenttreeid, BusinessFlow businessFlow)
        {
            return zephyrEntExportManager.UpdateTestPlanningFolder(cycleId, parenttreeid, businessFlow);
        }

        public override bool ConnectALMProject()
        {
            return this.ConnectALMServer();
        }

        public List<string[]> GetTCsDataSummary(int tsId)
        {
            return zephyrEntImportManager.GetTCsDataSummary(tsId);
        }
        public override bool ConnectALMServer()
        {
            LoginDTO loginDTO = null;
            if (ALMCore.DefaultAlmConfig.UseToken)
            {
                loginDTO = new LoginDTO()
                {
                    User = ALMCore.DefaultAlmConfig.ALMUserName,
                    Password = ALMCore.DefaultAlmConfig.ALMPassword,
                    AuthToken = ALMCore.DefaultAlmConfig.ALMPassword,
                    Server = ALMCore.DefaultAlmConfig.ALMServerURL
                };
            }
            else
            {
                loginDTO = new LoginDTO()
                {
                    User = ALMCore.DefaultAlmConfig.ALMUserName,
                    Password = ALMCore.DefaultAlmConfig.ALMPassword,
                    Server = ALMCore.DefaultAlmConfig.ALMServerURL
                };
            }
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Connecting to Zephyr server");
                return Task.Run(() =>
                {
                    return zephyrEntRepository.IsLoginValid(loginDTO);
                }).Result;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Connecting to Zephyr server", ex);
                return false;
            }
        }

        public TestCaseResource UpdateTestCase(long treeNodeId, ActivitiesGroup activtiesGroup, List<BaseResponseItem> matchingTC)
        {
            return zephyrEntExportManager.UpdateTestCase(treeNodeId, activtiesGroup, matchingTC);
        }

        public TestCaseResource CreateTestCase(long treeNodeId, ActivitiesGroup activtiesGroup, Dictionary<string, string> testInstanceFields)
        {
            return zephyrEntExportManager.CreateTestCase(treeNodeId, activtiesGroup, testInstanceFields);
        }

        public override Dictionary<Guid, string> CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, List<ExternalItemFieldBase> defectsFields, bool useREST = false)
        {
            throw new NotImplementedException();
        }

        public override bool DisconnectALMProjectStayLoggedIn()
        {
            return true;
        }

        public List<string> GetTestLabExplorer(string path)
        {
            throw new NotImplementedException();
        }

        public override void DisconnectALMServer()
        {
            return;
        }

        public override bool ExportExecutionDetailsToALM(BusinessFlow bizFlow, ref string result, bool exectutedFromAutomateTab = false, PublishToALMConfig publishToALMConfig = null)
        {
            bool IsExecute = zephyrEntExportManager.ExportExceutionDetailsToALM(bizFlow, ref result, null, exectutedFromAutomateTab, publishToALMConfig);
            if(!IsExecute)
            {
                CreateActivitiesGroupsExecution(bizFlow);
            }
            return zephyrEntExportManager.ExportExceutionDetailsToALM(bizFlow, ref result, null, exectutedFromAutomateTab, publishToALMConfig);
        }
        public void CreateActivitiesGroupsExecution(BusinessFlow bizFlow)
        {
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Create mapped activities Groups Zephyr Ent. Execution Ids for publish run status.");
                long testerId = GetCurrentUser();
                List<TestCaseResource> tcsPlanningList = GetTestCasesByAssignmentTree(Convert.ToInt32(bizFlow.ExternalID));
                List<Execution> assignsList = AssigningTestCasesToTesterForExecution(tcsPlanningList.Select(z => z.tct.id).ToList(), Convert.ToInt64(bizFlow.ExternalID2), testerId, Convert.ToInt64(bizFlow.ExternalID));
                ObservableList<ActivitiesGroup> mappedAG = new ObservableList<ActivitiesGroup>();
                foreach (ActivitiesGroup ag in bizFlow.ActivitiesGroups)
                {
                    if (String.IsNullOrEmpty(ag.ExternalID))
                    {
                        mappedAG.Add(ag);
                    }
                }
                if (mappedAG.Count > 0)
                {
                    ExecuteTestCases(assignsList, testerId, bizFlow.ActivitiesGroups);
                }
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"Failed Create mapped activities Groups Zephyr Ent. Execution Ids. Error: {ex.Message}");
            }
        }
        public override Dictionary<string, string> GetALMDomainProjects(string ALMDomainName)
        {
            AlmResponseWithData<AlmDomainColl> domains = Task.Run(() =>
            {
                return zephyrEntRepository.GetLoginProjects(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL);
            }).Result;
            return domains.DataResult.Where(f => f.DomainName.Equals(ALMDomainName)).FirstOrDefault().Projects.ToDictionary(project => project.ProjectId.ToString(), project => project.ProjectName);
        }

        public long GetCurrentUser()
        {
            return zephyrEntRepository.GetCurrentUser()[0].id;
        }

        public List<TestCaseResource> GetTestCasesByAssignmentTree(int tcrCatalogTreeId)
        {
            return zephyrEntRepository.GetTestCasesByAssignmentTree(tcrCatalogTreeId);
        }

        public List<BaseResponseItem> GetRepositoryTreeByReleaseId(string releaseId)
        {
            return zephyrEntRepository.GetRepositoryTreeByReleaseId(Convert.ToInt32(releaseId));
        }
        public override List<string> GetALMDomains()
        {
            AlmResponseWithData<AlmDomainColl> domains = Task.Run(() =>
            {
                return zephyrEntRepository.GetLoginProjects(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL);
            }).Result;

            return domains.DataResult.Select(f => f.DomainName).ToList();
        }
        public override ObservableList<ExternalItemFieldBase> GetALMItemFields(BackgroundWorker bw, bool online, ResourceType resourceType)
        {
            return UpdatedAlmFields(zephyrEntImportManager.GetALMItemFields(bw, online, resourceType));
        }
        public object GetRepositoryTreeIdByTestcaseId(int testcaseId)
        {
            return zephyrEntExportManager.GetRepositoryTreeIdByTestcaseId(testcaseId);
        }

        public List<BaseResponseItem> GetZephyrEntTest(string externalID)
        {
            return zephyrEntRepository.GetTCDetailsByTCId(Convert.ToInt32(externalID));
        }

        public TreeNode CreateTreeNode()
        {
            return zephyrEntExportManager.CreateTreeNode();
        }
        public object GetTestRepositoryFolderType(int treeId)
        {
            return zephyrEntRepository.GetTestRepositoryFolderType(treeId);
        }
        public TreeNode GetTestRepositoryFolderById(int treeId)
        {
            return zephyrEntRepository.GetTestRepositoryFolderById(treeId);
        }
        public override bool IsServerConnected()
        {
            return IsConnectValidationDone;
        }
        public List<BaseResponseItem> GetTreeByCretiria(string type, int releaseId, int revisionId, int parentId)
        {
            TreeNode treeNode = zephyrEntRepository.GetTestRepositoryFolderById(parentId);
            return zephyrEntRepository.GetTreeByCretiria(type, releaseId, Convert.ToInt32(treeNode.revision), parentId);
        }

        public List<BaseResponseItem> GetZephyrEntTreeData(int releaseId, string entityType, bool v2)
        {
            return zephyrEntRepository.GetZephyrEntTreeData(releaseId, entityType, v2);
        }

        public List<BaseResponseItem> GetZephyrEntPhaseById(int treeId)
        {
            return zephyrEntRepository.GetPhaseById(treeId);
        }
        public Cycle GetZephyrEntCycleById(int cycleId)
        {
            List<BaseResponseItem> cycle = zephyrEntRepository.GetCycleById(cycleId);
            return new Cycle()
            {
                id = Convert.ToInt32(cycle[0].TryGetItem("id")),
                name = cycle[0].TryGetItem("name").ToString(),
                cycleStartDate = cycle[0].TryGetItem("cycleStartDate").ToString(),
                cycleEndDate = cycle[0].TryGetItem("cycleEndDate").ToString(),
                releaseId = Convert.ToInt32(cycle[0].TryGetItem("releaseId")),
                revision = Convert.ToInt32(cycle[0].TryGetItem("revision"))
            };
        }

        public List<BaseResponseItem> GetZephyrEntTcsByTreeId(int treeId)
        {
            return zephyrEntRepository.GetTCsByTreeId(treeId);
        }

        public QC.ALMTestSet ImportTestSetData(QC.ALMTestSet testSet)
        {
            List<BaseResponseItem> selectedTcs = GetZephyrEntTcsByTreeId(Convert.ToInt32(testSet.TestSetID));
            var token = (JToken)selectedTcs[0].TryGetItem("results");
            foreach (var testInstance in token)
            {
                testSet.Tests.Add(ImportTSTest(((JToken)testInstance).SelectToken("testcase")));
            }
            return testSet;
        }
        public QC.ALMTSTest ImportTSTest(JToken testInstance)
        {
            QC.ALMTSTest newTSTest = new QC.ALMTSTest();
            if (newTSTest.Runs == null)
            {
                newTSTest.Runs = new List<ALMTSTestRun>();
            }

            int versionId = Convert.ToInt32(testInstance["tcrVersionNumber"]);
            if (testInstance != null)
            {
                //Regular TC
                newTSTest.TestID = testInstance["testcaseId"].ToString();
                newTSTest.LinkedTestID = testInstance["id"].ToString();
                newTSTest.TestName = testInstance["name"].ToString();
                newTSTest.Description = testInstance["description"] == null ? "" : testInstance["description"].ToString();
            }

            var testSteps = zephyrEntRepository.GetTeststepByTestcaseId(Convert.ToInt32(newTSTest.LinkedTestID), versionId);
            //Get the TC design steps
            if (testSteps.Count > 0)
            {
                var testStepsToken = (JToken)testSteps[0].Properties["steps"];
                foreach (var testcaseStep in testStepsToken)
                {

                    QC.ALMTSTestStep newtsStep = new QC.ALMTSTestStep();
                    newtsStep.StepID = testcaseStep["id"].ToString();
                    newtsStep.StepName = testcaseStep["step"].ToString();
                    newtsStep.Description = testcaseStep["data"].ToString();
                    newtsStep.Expected = testcaseStep["result"].ToString();
                    newTSTest.Steps.Add(newtsStep);
                }
            }
            return newTSTest;
        }

        public CyclePhase CreateNewTestCyclePhase(Cycle cycle, string bfName)
        {
            return zephyrEntExportManager.CreateNewTestCyclePhase(cycle, bfName);
        }

        public Cycle CreateNewTestCycle()
        {
            return zephyrEntExportManager.CreateNewTestCycle();
        }

        public BusinessFlow ConvertQCTestSetToBF(QC.ALMTestSet tS)
        {
            return zephyrEntImportManager.ConvertQCTestSetToBF(tS);
        }

        public void AssigningTestCasesToCyclePhase(List<long> tctIds, long cyclePhaseId, long tcrCatalogTreeId)
        {
            zephyrEntExportManager.AssigningTestCasesToCyclePhase(tctIds, cyclePhaseId, tcrCatalogTreeId);
        }

        public List<Execution> AssigningTestCasesToTesterForExecution(List<long> tcVersionIds, long cyclePhaseId, long testerId, long tcrCatalogTreeId)
        {
            return zephyrEntExportManager.AssigningTestCasesToTesterForExecution(tcVersionIds, cyclePhaseId, testerId, tcrCatalogTreeId);
        }

        public void ExecuteTestCases(List<Execution> assignsList, long testerId, ObservableList<ActivitiesGroup> ActivitiesGroups)
        {
            zephyrEntExportManager.ExecuteTestCases(assignsList, testerId, ActivitiesGroups);
        }
    }
}
