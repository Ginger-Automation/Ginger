using ALM_Common.Data_Contracts;
using ALM_Common.DataContracts;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Activities;
using GingerCore.ALM.QC;
using GingerCore.ALM.ZephyrEnt.Bll;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using ZephyrEntSDK.Models;
using ZephyrEntSDK.Models.Base;

namespace GingerCore.ALM
{
    public class ZephyrEntCore : ALMCore
    {
        protected Zepyhr_Ent_Repository.ZephyrEntRepository zephyrEntRepository;
        private ZephyrEntExportManager zephyrEntExportManager;
        private ZephyrEntImportManager zephyrEntImportManager;
        public override ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo { get; set; }
        public override ObservableList<Activity> GingerActivitiesRepo { get; set; }
        public ZephyrEntCore()
        {
            zephyrEntRepository = new Zepyhr_Ent_Repository.ZephyrEntRepository(new LoginDTO()
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
        public dynamic UpdateTestPlanningFolder(long cycleId, long parenttreeid, string folderName, string folderDesc)
        {
            return zephyrEntRepository.UpdateTestPlanningFolder(cycleId, parenttreeid, folderName, folderDesc);
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
            if(ALMCore.DefaultAlmConfig.ZepherEntToken)
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

        public TestCaseResource CreateTestCase(long treeNodeId, ActivitiesGroup activtiesGroup)
        {
            return zephyrEntExportManager.CreateTestCase(treeNodeId, activtiesGroup);
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
            throw new NotImplementedException();
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

        public override ObservableList<ExternalItemFieldBase> GetALMItemFields(BackgroundWorker bw, bool online, ResourceType resourceType = ResourceType.ALL)
        {
            ObservableList<ExternalItemFieldBase> almFields = new ObservableList<ExternalItemFieldBase>();
            zephyrEntRepository.GetCustomFields().ForEach(ent => {
                almFields.Add(new ExternalItemFieldBase()
                {
                    ID = ent.id.ToString(),
                    Name = String.IsNullOrEmpty(ent.displayName) ? ent.fieldName : ent.displayName,
                    ExternalID = ent.description,
                    Mandatory = ent.mandatory,
                    ItemType = ent.entityName
                });
            });
            return almFields;
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
        public override bool IsServerConnected()
        {
            throw new NotImplementedException();
        }
        public List<BaseResponseItem> GetTreeByCretiria(string type, int releaseId, int revisionId, int parentId)
        {
            return zephyrEntRepository.GetTreeByCretiria(type, releaseId, revisionId, parentId);
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

        public QC.QCTestSet ImportTestSetData(QC.QCTestSet testSet)
        {
            List <BaseResponseItem> selectedTcs = GetZephyrEntTcsByTreeId(Convert.ToInt32(testSet.TestSetID));
            var token = (JToken)selectedTcs[0].TryGetItem("results");
            foreach (var testInstance in token)
            {
                testSet.Tests.Add(ImportTSTest(((JToken)testInstance).SelectToken("testcase")));
            }
            return testSet;
        }
        public QC.QCTSTest ImportTSTest(JToken testInstance)
        {
            QC.QCTSTest newTSTest = new QC.QCTSTest();
            if (newTSTest.Runs == null)
            {
                newTSTest.Runs = new List<QCTSTestRun>();
            }

            int versionId = Convert.ToInt32(testInstance["tcrVersionNumber"]);
            if (testInstance != null)
            {
                //Regular TC
                newTSTest.TestID = testInstance["id"].ToString();
                newTSTest.TestName = testInstance["name"].ToString();
                newTSTest.Description = testInstance["description"] == null ? "" : testInstance["description"].ToString();
            }

            var testSteps = zephyrEntRepository.GetTeststepByTestcaseId(Convert.ToInt32(newTSTest.TestID), versionId);
            //Get the TC design steps
            //QCTestCaseStepsColl TSTestSteps = GetListTSTestSteps(testCase);
            if (testSteps.Count > 0)
            {
                var testStepsToken = (JToken)testSteps[0].Properties["steps"];
                foreach (var testcaseStep in testStepsToken)
                {

                    QC.QCTSTestStep newtsStep = new QC.QCTSTestStep();
                    newtsStep.StepID = testcaseStep["id"].ToString();
                    newtsStep.StepName = testcaseStep["step"].ToString();
                    newtsStep.Description = testcaseStep["data"].ToString();
                    newtsStep.Expected = testcaseStep["result"].ToString();
                    newTSTest.Steps.Add(newtsStep);
                    ////Get the TC parameters
                    //CheckForParameter(newTSTest, newtsStep.Description);
                }

                ////Get the TC execution history
                //try
                //{
                //    List<RunSuite> TSTestRuns = GetTestSuiteRun(testCase.TestSetId);

                //    foreach (RunSuite run in TSTestRuns)
                //    {
                //        QC.QCTSTestRun newtsRun = new QC.QCTSTestRun();
                //        newtsRun.RunID = run.Id;
                //        newtsRun.RunName = run.Name;
                //        newtsRun.Status = run.NativeStatus.Name;
                //        newtsRun.ExecutionDate = (run.GetValue("started").ToString());
                //        newtsRun.ExecutionTime = (run.GetValue("last_modified").ToString());
                //        newtsRun.Tester = (run.DefaultRunBy.FullName).ToString();
                //        newTSTest.Runs.Add(newtsRun);
                //    }
                //}
                //catch (Exception ex)
                //{
                //    Reporter.ToLog(eLogLevel.ERROR, "Failed to pull QC test case RUN info", ex);
                //    newTSTest.Runs = new List<QC.QCTSTestRun>();
                //}
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

        public BusinessFlow ConvertQCTestSetToBF(QC.QCTestSet tS)
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
