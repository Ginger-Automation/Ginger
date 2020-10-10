using ALM_Common.DataContracts;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Activities;
using GingerCore.Variables;
using OctaneSDK.Connector;
using OctaneSDK.Connector.Authentication;
using OctaneSDK.Connector.Credentials;
using OctaneSDK.Services;
using OctaneSDK.Services.RequestContext;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Octane_Repository;
using ALM_Common.Data_Contracts;
using Octane_Repository.BLL;
using OctaneSDK.Entities.Base;
using OctaneSDK.Entities.WorkItems;
using OctaneSDK.Entities.Tests;
using OctaneSDK.Entities.Requirements;
using GingerCore.ALM.QC;
using QCRestClient;
using OctaneSDK.Services.Queries;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Web;

namespace GingerCore.ALM
{
    public class OctaneCore : ALMCore
    {
        public override ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo { get; set; }
        public override ObservableList<Activity> GingerActivitiesRepo { get; set; }
        public ProjectArea ProjectArea { get; private set; }

        public RestConnector mOctaneRestConnector;
        public EntityService entityService;
        protected LwssoAuthenticationStrategy lwssoAuthenticationStrategy;
        protected WorkspaceContext workspaceContext;
        protected SharedSpaceContext sharedSpaceContext;
        protected OctaneRepository octaneRepository;

        private static Dictionary<string, string> ExploredTestLabFolder = new Dictionary<string, string>();
        public OctaneCore()
        {
            octaneRepository = new OctaneRepository();
        }
        public override bool ConnectALMProject()
        {
            return this.ConnectALMServer();
        }

        public List<string> GetTestLabExplorer(string path)
        {

            List<string> testlabPathList = new List<string>();
            try
            {
                string[] separatePath = path.Split('\\');
                separatePath[0] = ExploredTestLabFolder.ContainsKey("Application Modules") ? ExploredTestLabFolder["Application Modules"] : GetRootFolderId();

                if (!ExploredTestLabFolder.ContainsKey("Application Modules"))
                {
                    ExploredTestLabFolder.Add("Application Modules", separatePath[0]);
                }

                for (int i = 1; i < separatePath.Length; i++)
                {
                    separatePath[i] = GetTestLabFolderId(separatePath[i], separatePath[i - 1]);
                }

                LogicalQueryPhrase parent = new LogicalQueryPhrase("id", separatePath[separatePath.Length - 1], ComparisonOperator.Equal);
                CrossQueryPhrase qdParent = new CrossQueryPhrase("parent", parent);
                List<IQueryPhrase> filter = new List<IQueryPhrase>() { qdParent };

                return Task.Run(() =>
                {
                    return octaneRepository.GetEntities<ApplicationModule>(GetLoginDTO(), filter);
                }).Result.Select(d => d.Name).ToList();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get Test Lab with REST API", ex);
            }
            return testlabPathList;
        }

        private string GetRootFolderId()
        {
            return Task.Run(() =>
            {
                LogicalQueryPhrase f1 = new LogicalQueryPhrase("name", "Application Modules", ComparisonOperator.Equal);
                LogicalQueryPhrase f2 = new LogicalQueryPhrase("path", "000000", ComparisonOperator.Equal);
                return octaneRepository.GetEntities<ApplicationModule>(GetLoginDTO(), new List<IQueryPhrase>() { f1, f2 });
            }).Result.FirstOrDefault().Id.ToString();
        }

        private string GetTestLabFolderId(string separateAti, string separateAtIMinusOne)
        {
            if (!ExploredTestLabFolder.ContainsKey(separateAti))
            {
                LogicalQueryPhrase parent = new LogicalQueryPhrase("id", separateAtIMinusOne, ComparisonOperator.Equal);
                CrossQueryPhrase qdParent = new CrossQueryPhrase("parent", parent);
                LogicalQueryPhrase lp = new LogicalQueryPhrase("name", separateAti, ComparisonOperator.Equal);
                List<IQueryPhrase> filter = new List<IQueryPhrase>() { qdParent, lp };
                List<ApplicationModule> listnodes = Task.Run(() =>
                {
                    return octaneRepository.GetEntities<ApplicationModule>(GetLoginDTO(), filter);
                }).Result;

                ExploredTestLabFolder.Add(listnodes.FirstOrDefault().Name, listnodes.FirstOrDefault().Id);
                return listnodes.FirstOrDefault().Id;
            }
            else
            {
                return ExploredTestLabFolder[separateAti];
            }
        }

        private LoginDTO GetLoginDTO()
        {
            AlmResponseWithData<AlmDomainColl> domains = Task.Run(() =>
            {
                return octaneRepository.GetLoginProjects(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL);
            }).Result;
            AlmDomain domain = domains.DataResult.Where(f => f.DomainName.Equals(ALMCore.DefaultAlmConfig.ALMDomain)).FirstOrDefault();
            ProjectArea project = domain.Projects.Where(p => p.ProjectName.Equals(ALMCore.DefaultAlmConfig.ALMProjectName)).FirstOrDefault();
            LoginDTO loginDto = new LoginDTO()
            {
                User = ALMCore.DefaultAlmConfig.ALMUserName,
                Password = ALMCore.DefaultAlmConfig.ALMPassword,
                Server = ALMCore.DefaultAlmConfig.ALMServerURL,
                SharedSpaceId = domain.DomainId,
                WorkSpaceId = project.ProjectId
            };
            return loginDto;
        }

        public List<QCTestSetSummary> GetTestSetExplorer(string PathNode)
        {
            List<QCTestSetSummary> testlabPathList = new List<QCTestSetSummary>();
            string[] separatePath = PathNode.Split('\\');
            try
            {
                separatePath[0] = ExploredTestLabFolder.ContainsKey("Application Modules") ? ExploredTestLabFolder["Application Modules"] : GetRootFolderId();

                if (!ExploredTestLabFolder.ContainsKey("Application Modules"))
                {
                    ExploredTestLabFolder.Add("Application Modules", separatePath[0]);
                }

                for (int i = 1; i < separatePath.Length; i++)
                {
                    separatePath[i] = GetTestLabFolderId(separatePath[i], separatePath[i - 1]);
                }

                List<TestSuite> testSets = Task.Run(() =>
                {
                    LogicalQueryPhrase test_Suite = new LogicalQueryPhrase("id", separatePath[separatePath.Length - 1], ComparisonOperator.Equal);
                    CrossQueryPhrase qd = new CrossQueryPhrase("product_areas", test_Suite);
                    return octaneRepository.GetEntities<TestSuite>(GetLoginDTO(), new List<IQueryPhrase>() { qd });
                }).Result;

                foreach (TestSuite testset in testSets)
                {
                    QCTestSetSummary QCTestSetTreeItem = new QCTestSetSummary();
                    QCTestSetTreeItem.TestSetID = testset.Id;
                    QCTestSetTreeItem.TestSetName = testset.Name;
                    testlabPathList.Add(QCTestSetTreeItem);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get Test Set with REST API", ex);
            }
            return testlabPathList;
        }

        public override bool ConnectALMServer()
        {
            try
            {
                if (octaneRepository == null)
                {
                    octaneRepository = new OctaneRepository();
                }
                Reporter.ToLog(eLogLevel.DEBUG, "Connecting to Octane server");
                return Task.Run(() =>
                    {
                        return octaneRepository.IsLoginValid(
                            new LoginDTO()
                            {
                                User = ALMCore.DefaultAlmConfig.ALMUserName,
                                Password = ALMCore.DefaultAlmConfig.ALMPassword,
                                Server = ALMCore.DefaultAlmConfig.ALMServerURL
                            });
                    }).Result;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Connecting to Octane server", ex);
                mOctaneRestConnector = null;
                return false;
            }
        }

        public override Dictionary<Guid, string> CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, List<ExternalItemFieldBase> defectsFields, bool useREST = false)
        {
            throw new NotImplementedException();
        }

        public override bool DisconnectALMProjectStayLoggedIn()
        {
            DisconnectALMServer();
            ConnectALMServer();
            return true;
        }

        public override void DisconnectALMServer()
        {
            var result = Task.Run(() =>
            {
                this.octaneRepository.DisconnectProject();
                return true;
            }).Result;
        }

        public override bool ExportExecutionDetailsToALM(BusinessFlow bizFlow, ref string result, bool exectutedFromAutomateTab = false, PublishToALMConfig publishToALMConfig = null)
        {
            throw new NotImplementedException();
        }

        public override Dictionary<string, string> GetALMDomainProjects(string ALMDomainName)
        {
            AlmResponseWithData<AlmDomainColl> domains = Task.Run(() =>
            {
                return octaneRepository.GetLoginProjects(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL);
            }).Result;
            return domains.DataResult.Where(f => f.DomainName.Equals(ALMDomainName)).FirstOrDefault().Projects.ToDictionary(project => project.ProjectName, project => project.ProjectName);
        }

        public override List<string> GetALMDomains()
        {
            AlmResponseWithData<AlmDomainColl> domains = Task.Run(() =>
            {
                return octaneRepository.GetLoginProjects(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL);
            }).Result;

            return domains.DataResult.Select(f => f.DomainName).ToList();
        }

        public override ObservableList<ExternalItemFieldBase> GetALMItemFields(BackgroundWorker bw, bool online, ResourceType resourceType = ResourceType.ALL)
        {
            ObservableList<ExternalItemFieldBase> fields = new ObservableList<ExternalItemFieldBase>();
            string resourse = string.Empty;
            AlmResponseWithData<AlmDomainColl> domains = Task.Run(() =>
            {
                return octaneRepository.GetLoginProjects(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL);
            }).Result;
            AlmDomain domain = domains.DataResult.Where(f => f.DomainName.Equals(ALMCore.DefaultAlmConfig.ALMDomain)).FirstOrDefault();
            ProjectArea project = domain.Projects.Where(p => p.ProjectName.Equals(ALMCore.DefaultAlmConfig.ALMProjectName)).FirstOrDefault();
            LoginDTO loginDto = new LoginDTO()
            {
                User = ALMCore.DefaultAlmConfig.ALMUserName,
                Password = ALMCore.DefaultAlmConfig.ALMPassword,
                Server = ALMCore.DefaultAlmConfig.ALMServerURL,
                SharedSpaceId = domain.DomainId,
                WorkSpaceId = project.ProjectId
            };
            Dictionary<string, List<string>> listnodes = Task.Run(() =>
            {
                return octaneRepository.GetListNodes(loginDto);
            }).Result;

            if (resourceType == ALM_Common.DataContracts.ResourceType.ALL)
            {
                resourse = Octane_Repository.BLL.Extensions.ConvertResourceType(ResourceType.TEST_CASE);
                fields.Append(
                    AddFieldsValues(octaneRepository.GetEntityFields(resourse, loginDto), "Test Case",
                    listnodes, octaneRepository.GetPhases(loginDto, resourse)));

                resourse = Octane_Repository.BLL.Extensions.ConvertResourceType(ResourceType.TEST_SET);
                fields.Append(
                    AddFieldsValues(octaneRepository.GetEntityFields(resourse, loginDto), "Test Suit",
                    listnodes, octaneRepository.GetPhases(loginDto, resourse)));

                resourse = Octane_Repository.BLL.Extensions.ConvertResourceType(ResourceType.REQUIREMENT);
                fields.Append(
                    AddFieldsValues(
                        octaneRepository.GetEntityFields(resourse, loginDto), "Requirement",
                        listnodes, octaneRepository.GetPhases(loginDto, resourse)));

                resourse = Octane_Repository.BLL.Extensions.ConvertResourceType(ResourceType.TEST_RUN);
                fields.Append(
                    AddFieldsValues(
                        octaneRepository.GetEntityFields(resourse, loginDto), "Run",
                        listnodes, octaneRepository.GetPhases(loginDto, resourse)));
            }
            else
            {

                resourse = Octane_Repository.BLL.Extensions.ConvertResourceType(resourceType);

                fields.Append(AddFieldsValues(Task.Run(() =>
                {
                    return octaneRepository.GetEntityFields(resourse, loginDto);
                }).Result, resourse, listnodes, Task.Run(() =>
                {
                    return octaneRepository.GetPhases(loginDto, resourse);
                }).Result));
            }
            return fields;
        }

        private static ObservableList<ExternalItemFieldBase> AddFieldsValues(ListResult<FieldMetadata> entityFields, string entityType, Dictionary<string, List<string>> listnodes, Dictionary<string, List<string>> phases)
        {
            ObservableList<ExternalItemFieldBase> fields = new ObservableList<ExternalItemFieldBase>();

            if ((entityFields != null) && (entityFields.total_count.Value > 0))
            {
                foreach (FieldMetadata field in entityFields.data)
                {
                    if (string.IsNullOrEmpty(field.Label) || !field.VisibleInUI)
                    {
                        continue;
                    }

                    ExternalItemFieldBase itemfield = new ExternalItemFieldBase();
                    itemfield.ID = field.Label;
                    itemfield.ExternalID = field.Label;
                    itemfield.Name = field.Name;
                    itemfield.Mandatory = field.IsRequired;
                    itemfield.SystemFieled = !field.IsUserField;

                    if (itemfield.Mandatory)
                    {
                        itemfield.ToUpdate = true;
                    }
                    itemfield.ItemType = entityType;
                    itemfield.Type = field.FieldType;

                    if (listnodes != null && listnodes.ContainsKey(field.Name) && listnodes[field.Name].Any())
                    {
                        itemfield.PossibleValues = new ObservableList<string>(listnodes[field.Name]);
                    }
                    else if (listnodes != null && listnodes.ContainsKey(entityType.ToLower() + "_" + field.Name) && listnodes[entityType.ToLower() + "_" + field.Name].Any())
                    {
                        itemfield.PossibleValues = new ObservableList<string>(listnodes[entityType.ToLower() + "_" + field.Name]);
                    }
                    else if (phases != null && phases.ContainsKey(field.Name) && phases[field.Name].Any())
                    {
                        itemfield.PossibleValues = new ObservableList<string>(phases[field.Name]);
                    }
                    fields.Add(itemfield);
                }
            }
            return fields;
        }

        public override bool IsServerConnected()
        {
            throw new NotImplementedException();
        }

        public QC.QCTestSet ImportTestSetData(QC.QCTestSet testSet)
        {
            QCTestInstanceColl testInstances = GetTestsFromTSId(testSet.TestSetID);

            foreach (QCTestInstance testInstance in testInstances)
            {
                testSet.Tests.Add(ImportTSTest(testInstance));
            }

            return testSet;
        }

        public QCTestInstanceColl GetTestsFromTSId(string testSetID)
        {
            try
            {
                List<TestSuite_Test_Link> TSLink;
                QCTestInstanceColl testCollection = new QCTestInstanceColl();
                CrossQueryPhrase qd = new CrossQueryPhrase("test_suite", new LogicalQueryPhrase("id", testSetID, ComparisonOperator.Equal));
                IList<IQueryPhrase> filter = new List<IQueryPhrase> { qd };
                TSLink = octaneRepository.GetEntities<TestSuite_Test_Link>(GetLoginDTO(), filter);
                foreach (TestSuite_Test_Link item in TSLink)
                {
                    testCollection.Add(new QCTestInstance() { Id = item.Test.Id, TestId = item.Test.Id, Name = item.Test.Name , CycleId = testSetID });
                }
                return testCollection;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get test instances of test set with REST API", ex);
                return null;
            }
        }

        public QCTestCase GetTestCases(string testcaseID)
        {
            LogicalQueryPhrase qd = new LogicalQueryPhrase("id", testcaseID, ComparisonOperator.Equal);
            IList<IQueryPhrase> filter = new List<IQueryPhrase> { qd };
            List<Test> test = octaneRepository.GetEntities<Test>(GetLoginDTO(), filter);
            if (test.Any())
            {
                Test testTemp = test.Where(f => true).FirstOrDefault();
                return new QCTestCase() { Id = testcaseID, TestId = testcaseID, Name = testTemp.Name };
            }
            return null;
        }

        public QCTestCaseStepsColl GetListTSTestSteps(QCTestCase testcase)
        {
            var lineBreaks = new [] { '\n' };
            QCTestCaseStepsColl stepsColl = new QCTestCaseStepsColl();
            string steps = Task.Run(() => { return octaneRepository.GetTestCaseStep(GetLoginDTO(), testcase.Id); }).Result;
            int i = 1;
            if (!string.IsNullOrEmpty(steps))
            {
                foreach (string step in steps.Split(lineBreaks))
                {
                    stepsColl.Add(new QCTestCaseStep() { Id = Convert.ToString(i), TestId = testcase.Id, Description = step, Name = step, StepOrder = Convert.ToString(i++) });
                }
            }
            return stepsColl;
        }

        private List<string> GetTCParameterList(string steps)
        {
            List<string> parameters = new List<string>();
            string hasParams = @"(<[a-zA-Z0-9,+/-_()~!@#$%^&*=]+>)";
            MatchCollection mc = Regex.Matches(steps, hasParams);
            if (mc != null && mc.Count > 0)
            {
                foreach (Match m in mc)
                {
                    parameters.Add(m.Value.Substring(1, m.Value.Length - 2));
                }
            }
            return parameters;
        }

        private void CheckForParameter(QC.QCTSTest newTSTest, string steps)
        {
            string hasParams = @"(<[a-zA-Z0-9,+/-_()~!@#$%^&*=]+>)";
            MatchCollection mc = Regex.Matches(steps, hasParams);
            if (mc != null && mc.Count > 0)
            {
                foreach (Match m in mc)
                {
                    QC.QCTSTestParameter newtsVar = new QC.QCTSTestParameter();
                    newtsVar.Name = m.Value.Substring(1, m.Value.Length - 2);         
                    newTSTest.Parameters.Add(newtsVar);
                }
            }
        }

        public QC.QCTSTest ImportTSTest(QCTestInstance testInstance)
        {
            QC.QCTSTest newTSTest = new QC.QCTSTest();
            QCTestCase testCase = GetTestCases(testInstance.Id);
            testCase.TestSetId = testInstance.CycleId;
            if (testInstance != null)
            {
                //Regular TC
                newTSTest.TestID = testInstance.Id;
                newTSTest.TestName = testInstance.Name;
            }

            //Get the TC design steps
            QCTestCaseStepsColl TSTestSteps = GetListTSTestSteps(testCase);
            foreach (QCTestCaseStep testcaseStep in TSTestSteps)
            {
                QC.QCTSTestStep newtsStep = new QC.QCTSTestStep();
                newtsStep.StepID = testcaseStep.Id.ToString();
                newtsStep.StepName = testcaseStep.Name;
                newtsStep.Description = testcaseStep.Description;
                newtsStep.Expected = "";
                newTSTest.Steps.Add(newtsStep);
                //Get the TC parameters
                CheckForParameter(newTSTest, newtsStep.Description);
            }

            //Get the TC execution history
            try
            {
                List<RunSuite> TSTestRuns = GetTestSuiteRun(testCase.TestSetId);

                foreach (RunSuite run in TSTestRuns)
                {
                    QC.QCTSTestRun newtsRun = new QC.QCTSTestRun();
                    newtsRun.RunID = run.Id;
                    newtsRun.RunName = run.Name;
                    newtsRun.Status = run.NativeStatus.Name;
                    newtsRun.ExecutionDate = (run.GetValue("started").ToString());
                    newtsRun.ExecutionTime = (run.GetValue("last_modified").ToString());
                    newtsRun.Tester = (run.DefaultRunBy.FullName).ToString();
                    newTSTest.Runs.Add(newtsRun);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to pull QC test case RUN info", ex);
                newTSTest.Runs = new List<QC.QCTSTestRun>();
            }
            return newTSTest;
        }

        public List<RunSuite> GetTestSuiteRun(string testSuiteId)
        {
            QCRunColl runColl = new QCRunColl();
            EntityListResult<RunSuite> testFolders = new EntityListResult<RunSuite>();
            LogicalQueryPhrase test_Suite = new LogicalQueryPhrase("id", testSuiteId, ComparisonOperator.Equal);
            CrossQueryPhrase qd = new CrossQueryPhrase("test", test_Suite);

            return octaneRepository.GetEntities<RunSuite>(GetLoginDTO(), new List<IQueryPhrase> { qd });
        }

        public dynamic GetTSRunStatus(dynamic TSItem)
        {
            List<RunSuite> testInstances = GetTestSuiteRun(TSItem.TestSetID);

            foreach (RunSuite testInstance in testInstances)
            {
                bool existing = false;
                foreach (string[] status in TSItem.TestSetStatuses)
                {
                    if (status[0] == testInstance.NativeStatus.Name)
                    {
                        existing = true;
                        status[1] = (Int32.Parse(status[1]) + 1).ToString();
                    }
                }
                if (!existing) { TSItem.TestSetStatuses.Add(new string[] { testInstance.NativeStatus.Name, "1" }); }
            }
            return TSItem;
        }

        public BusinessFlow ConvertQCTestSetToBF(QC.QCTestSet testSet)
        {
            try
            {
                if (testSet == null) { return null; }

                TestSuite tsLatest = Task.Run(() =>
                {
                    LogicalQueryPhrase test_Suite = new LogicalQueryPhrase("id", testSet.TestSetID, ComparisonOperator.Equal);                    
                    return octaneRepository.GetEntities<TestSuite>(GetLoginDTO(), new List<IQueryPhrase>() { test_Suite });
                }).Result.FirstOrDefault();
                //Create Business Flow
                BusinessFlow busFlow = new BusinessFlow();
                busFlow.Name = testSet.TestSetName;
                busFlow.ExternalID = testSet.TestSetID;
                busFlow.Status = BusinessFlow.eBusinessFlowStatus.Development;
                busFlow.Description = StripHTML(tsLatest.GetStringValue("description"));                
                busFlow.Activities = new ObservableList<Activity>();
                busFlow.Variables = new ObservableList<VariableBase>();
                Dictionary<string, string> busVariables = new Dictionary<string, string>();//will store linked variables

                //Create Activities Group + Activities for each TC
                foreach (QC.QCTSTest tc in testSet.Tests)
                {
                    //check if the TC is already exist in repository
                    ActivitiesGroup tcActivsGroup;
                    ActivitiesGroup repoActivsGroup = null;
                    if (tc.LinkedTestID != null && tc.LinkedTestID != string.Empty)
                    {
                        repoActivsGroup = GingerActivitiesGroupsRepo.Where(x => x.ExternalID == tc.LinkedTestID).FirstOrDefault();
                    }
                    if (repoActivsGroup == null)
                    {
                        repoActivsGroup = GingerActivitiesGroupsRepo.Where(x => x.ExternalID == tc.TestID).FirstOrDefault();
                    }
                    if (repoActivsGroup != null)
                    {
                        List<Activity> repoNotExistsStepActivity = GingerActivitiesRepo.Where(z => repoActivsGroup.ActivitiesIdentifiers.Select(y => y.ActivityExternalID).ToList().Contains(z.ExternalID))
                                                                                       .Where(x => !tc.Steps.Select(y => y.StepID).ToList().Contains(x.ExternalID)).ToList();

                        tcActivsGroup = (ActivitiesGroup)repoActivsGroup.CreateInstance();

                        var ActivitySIdentifiersToRemove = tcActivsGroup.ActivitiesIdentifiers.Where(x => repoNotExistsStepActivity.Select(z => z.ExternalID).ToList().Contains(x.ActivityExternalID));
                        for (int indx = 0; indx < tcActivsGroup.ActivitiesIdentifiers.Count; indx++)
                        {
                            if ((indx < tcActivsGroup.ActivitiesIdentifiers.Count) && (ActivitySIdentifiersToRemove.Contains(tcActivsGroup.ActivitiesIdentifiers[indx])))
                            {
                                tcActivsGroup.ActivitiesIdentifiers.Remove(tcActivsGroup.ActivitiesIdentifiers[indx]);
                                indx--;
                            }
                        }

                        tcActivsGroup.ExternalID2 = tc.TestID;
                        busFlow.AddActivitiesGroup(tcActivsGroup);
                        busFlow.ImportActivitiesGroupActivitiesFromRepository(tcActivsGroup, GingerActivitiesRepo, true, true);
                        busFlow.AttachActivitiesGroupsAndActivities();
                    }
                    else //TC not exist in Ginger repository so create new one
                    {
                        tcActivsGroup = new ActivitiesGroup();
                        tcActivsGroup.Name = tc.TestName;
                        if (tc.LinkedTestID == null || tc.LinkedTestID == string.Empty)
                        {
                            tcActivsGroup.ExternalID = tc.TestID;
                            tcActivsGroup.ExternalID2 = tc.TestID;
                        }
                        else
                        {
                            tcActivsGroup.ExternalID = tc.LinkedTestID;
                            tcActivsGroup.ExternalID2 = tc.TestID; //original TC ID will be used for uploading the execution details back to QC
                            tcActivsGroup.Description = tc.Description;
                        }
                        busFlow.AddActivitiesGroup(tcActivsGroup);
                    }

                    //Add the TC steps as Activities if not already on the Activities group
                    foreach (QC.QCTSTestStep step in tc.Steps)
                    {
                        Activity stepActivity;
                        bool toAddStepActivity = false;

                        //check if mapped activity exist in repository
                        Activity repoStepActivity = (Activity)GingerActivitiesRepo.Where(x => x.ExternalID == step.StepID).FirstOrDefault();
                        if (repoStepActivity != null)
                        {
                            //check if it is part of the Activities Group
                            ActivityIdentifiers groupStepActivityIdent = (ActivityIdentifiers)tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.StepID).FirstOrDefault();
                            if (groupStepActivityIdent != null)
                            {
                                //already in Activities Group so get link to it
                                stepActivity = (Activity)busFlow.Activities.Where(x => x.Guid == groupStepActivityIdent.ActivityGuid).FirstOrDefault();
                                // in any case update description/expected/name - even if "step" was taken from repository
                                stepActivity.Description = step.Description;
                                stepActivity.Expected = step.Expected;
                                stepActivity.ActivityName = tc.TestName + ">" + step.StepName;
                            }
                            else//not in ActivitiesGroup so get instance from repo
                            {
                                stepActivity = (Activity)repoStepActivity.CreateInstance();
                                toAddStepActivity = true;
                            }
                        }
                        else//Step not exist in Ginger repository so create new one
                        {
                            stepActivity = new Activity();
                            stepActivity.ActivityName = tc.TestName + ">" + step.StepName;
                            stepActivity.ExternalID = step.StepID;
                            stepActivity.Description = step.Description;
                            stepActivity.Expected = step.Expected;

                            toAddStepActivity = true;
                        }

                        if (toAddStepActivity)
                        {
                            //not in group- need to add it
                            busFlow.AddActivity(stepActivity, tcActivsGroup);
                        }

                        //pull TC-Step parameters and add them to the Activity level
                        List<string> stepParamsList;
                        stepParamsList = GetTCParameterList(step.Description);
                        foreach (string param in stepParamsList)
                        {
                            //get the param value
                            string paramSelectedValue = string.Empty;
                            bool? isflowControlParam = null;
                            QC.QCTSTestParameter tcParameter = tc.Parameters.Where(x => x.Name.ToUpper() == param.ToUpper()).FirstOrDefault();

                            //get the param value
                            if (tcParameter != null && tcParameter.Value != null && tcParameter.Value != string.Empty)
                            {
                                paramSelectedValue = tcParameter.Value;
                            }
                            else
                            {
                                isflowControlParam = null;//empty value
                                paramSelectedValue = "<Empty>";
                            }

                            //check if parameter is part of a link
                            string linkedVariable = null;
                            if (paramSelectedValue.StartsWith("#$#"))
                            {
                                var valueParts = paramSelectedValue.Split(new [] { "#$#" }, StringSplitOptions.None);
                                if (valueParts.Count() == 3)
                                {
                                    linkedVariable = valueParts[1];
                                    paramSelectedValue = "$$_" + valueParts[2];//so it still will be considered as non-flow control

                                    if (busVariables.Keys.Contains(linkedVariable))
                                    {
                                        busVariables.Add(linkedVariable, valueParts[2]);
                                    }
                                }
                            }

                            //determine if the param is Flow Control Param or not based on it value and agreed sign "$$_"
                            if (paramSelectedValue.StartsWith("$$_"))
                            {
                                isflowControlParam = false;
                                if (paramSelectedValue.StartsWith("$$_"))
                                {
                                    paramSelectedValue = paramSelectedValue.Substring(3);//get value without "$$_"
                                }
                            }
                            else if (paramSelectedValue != "<Empty>")
                            {
                                isflowControlParam = true;
                            }

                            //check if already exist param with that name
                            VariableBase stepActivityVar = stepActivity.Variables.Where(x => x.Name.ToUpper() == param.ToUpper()).FirstOrDefault();
                            if (stepActivityVar == null)
                            {
                                //#Param not exist so add it
                                if (isflowControlParam.Value)
                                {
                                    //add it as selection list param                               
                                    stepActivityVar = new VariableSelectionList();
                                    stepActivityVar.Name = param;
                                    stepActivity.AddVariable(stepActivityVar);
                                    stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because new flow control param was added
                                }
                                else
                                {
                                    //add as String param
                                    stepActivityVar = new VariableString();
                                    stepActivityVar.Name = param;
                                    ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                                    stepActivity.AddVariable(stepActivityVar);
                                }
                            }
                            else
                            {
                                //#param exist
                                if (isflowControlParam.Value)
                                {
                                    if (!(stepActivityVar is VariableSelectionList))
                                    {
                                        //flow control param must be Selection List so transform it
                                        stepActivity.Variables.Remove(stepActivityVar);
                                        stepActivityVar = new VariableSelectionList();
                                        stepActivityVar.Name = param;
                                        stepActivity.AddVariable(stepActivityVar);
                                        stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because flow control param was added
                                    }
                                }
                                else if (!isflowControlParam.Value)
                                {
                                    if (stepActivityVar is VariableSelectionList)
                                    {
                                        //change it to be string variable
                                        stepActivity.Variables.Remove(stepActivityVar);
                                        stepActivityVar = new VariableString();
                                        stepActivityVar.Name = param;
                                        ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                                        stepActivity.AddVariable(stepActivityVar);
                                        stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because flow control param was removed
                                    }
                                }
                            }

                            //add the variable selected value                          
                            if (stepActivityVar is VariableSelectionList)
                            {
                                OptionalValue stepActivityVarOptionalVar = ((VariableSelectionList)stepActivityVar).OptionalValuesList.Where(x => x.Value == paramSelectedValue).FirstOrDefault();
                                if (stepActivityVarOptionalVar == null)
                                {
                                    //no such variable value option so add it
                                    stepActivityVarOptionalVar = new OptionalValue(paramSelectedValue);
                                    ((VariableSelectionList)stepActivityVar).OptionalValuesList.Add(stepActivityVarOptionalVar);
                                    if (isflowControlParam.Value)
                                    {
                                        stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because new param value was added
                                    }
                                }
                                //set the selected value
                                ((VariableSelectionList)stepActivityVar).SelectedValue = stepActivityVarOptionalVar.Value;
                            }
                            else
                            {
                                //try just to set the value
                                try
                                {
                                    stepActivityVar.Value = paramSelectedValue;
                                    if (stepActivityVar is VariableString)
                                    {
                                        ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                                    }
                                }
                                catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
                            }

                            //add linked variable if needed
                            if (string.IsNullOrEmpty(linkedVariable))
                            {
                                stepActivityVar.LinkedVariableName = linkedVariable;
                            }
                            else
                            {
                                stepActivityVar.LinkedVariableName = string.Empty;//clear old links
                            }
                        }
                    }

                    //order the Activities Group activities according to the order of the matching steps in the TC
                    try
                    {
                        int startGroupActsIndxInBf = busFlow.Activities.IndexOf(tcActivsGroup.ActivitiesIdentifiers[0].IdentifiedActivity);
                        foreach (QC.QCTSTestStep step in tc.Steps)
                        {
                            int stepIndx = tc.Steps.IndexOf(step) + 1;
                            ActivityIdentifiers actIdent = (ActivityIdentifiers)tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.StepID).FirstOrDefault();
                            if (actIdent == null || actIdent.IdentifiedActivity == null) { break; }//something wrong- shouldn't be null
                            Activity act = (Activity)actIdent.IdentifiedActivity;
                            int groupActIndx = tcActivsGroup.ActivitiesIdentifiers.IndexOf(actIdent);
                            int bfActIndx = busFlow.Activities.IndexOf(act);

                            //set it in the correct place in the group
                            int numOfSeenSteps = 0;
                            int groupIndx = -1;
                            foreach (ActivityIdentifiers ident in tcActivsGroup.ActivitiesIdentifiers)
                            {
                                groupIndx++;
                                if (string.IsNullOrEmpty(ident.ActivityExternalID) ||
                                        tc.Steps.Where(x => x.StepID == ident.ActivityExternalID).FirstOrDefault() == null)
                                {
                                    continue;//activity which not originally came from the TC
                                }
                                numOfSeenSteps++;

                                if (numOfSeenSteps >= stepIndx) { break; }
                            }
                            ActivityIdentifiers identOnPlace = (ActivityIdentifiers)tcActivsGroup.ActivitiesIdentifiers[groupIndx];
                            if (identOnPlace.ActivityGuid != act.Guid)
                            {
                                //replace places in group
                                tcActivsGroup.ActivitiesIdentifiers.Move(groupActIndx, groupIndx);
                                //replace places in business flow
                                busFlow.Activities.Move(bfActIndx, startGroupActsIndxInBf + groupIndx);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                        //failed to re order the activities to match the tc steps order, not worth breaking the import because of this
                    }
                }

                //Add the BF variables (linked variables)
                if (busVariables.Keys.Count > 0)
                {
                    foreach (KeyValuePair<string, string> var in busVariables)
                    {
                        //add as String param
                        VariableString busVar = new VariableString();
                        busVar.Name = var.Key;
                        busVar.InitialStringValue = var.Value;
                        busFlow.AddVariable(busVar);
                    }
                }

                return busFlow;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to import QC test set and convert it into " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), ex);
                return null;
            }
        }

        private static string StripHTML(string HTMLText, bool toDecodeHTML = true)
        {
            try
            {
                HTMLText = HTMLText.Replace("<br />", Environment.NewLine);
                Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
                var stripped = reg.Replace(HTMLText, "");
                if (toDecodeHTML)
                {
                    stripped = HttpUtility.HtmlDecode(stripped);
                }
                stripped = stripped.Trim();
                stripped = stripped.TrimStart('\n', '\r');
                stripped = stripped.TrimEnd('\n', '\r');

                return stripped;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while stripping the HTML from QC TC Step Description/Expected", ex);
                return HTMLText;
            }
        }
    }
}
