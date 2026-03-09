#region License
/*
Copyright © 2014-2026 European Support Limited

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

using AlmDataContractsStd.Contracts;
using AlmDataContractsStd.Enums;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.ALMLib.Azure;
using Amdocs.Ginger.CoreNET.ALMLib.DataContract;
using Amdocs.Ginger.Repository;
using Applitools.Utils;
using AzureRepositoryStd;
using AzureRepositoryStd.BLL;
using GingerCore.Activities;
using GingerCore.Environments;
using GingerCoreNET.ALMLib;
using GingerCoreNET.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using Newtonsoft.Json.Linq;
using OctaneStdSDK.Entities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using TestPlan = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestPlan;
using TestPlanWebApi = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using TestSuite = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestSuite;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem;
using WorkItem2 = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.WorkItem;


namespace GingerCore.ALM
{
    public class AzureDevOpsCore : ALMCore
    {
        protected AzureDevOpsRepository opsRepository;

        private static readonly Dictionary<string, string> ExploredApplicationModule = [];
        public override ALMIntegrationEnums.eALMType ALMType => ALMIntegrationEnums.eALMType.Azure;

        public AzureDevOpsCore()
        {
            opsRepository = new AzureDevOpsRepository(new LoginDTO()
            {
                Password = ALMCore.DefaultAlmConfig.ALMPassword,
                Server = ALMCore.DefaultAlmConfig.ALMServerURL
            });

        }


        public ObservableList<ActivitiesGroup> _gingerActivitiesGroupsRepo { get; set; }

        public override ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo
        {
            get
            { return _gingerActivitiesGroupsRepo; }
            set
            { _gingerActivitiesGroupsRepo = value; }

        }


        public ObservableList<Activity> _gingerActivitiesRepo { get; set; }

        public override ObservableList<Activity> GingerActivitiesRepo
        {
            get { return _gingerActivitiesRepo; }
            set { _gingerActivitiesRepo = value; }
        }

        public ObservableList<ApplicationPlatform> _applicationPlatforms { get; set; }
        public override ObservableList<ApplicationPlatform> ApplicationPlatforms { get { return _applicationPlatforms; } set { _applicationPlatforms = value; } }




        public override bool ConnectALMProject()
        {
            return this.ConnectALMServer();
        }

        public override bool ConnectALMServer()
        {
            LoginDTO loginDTO = GetLoginDTO();

            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Connecting to Azure server");
                return AzureDevOpsRepository.IsLoginValid(loginDTO);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Connecting to Azure server", ex);
                return false;
            }
        }

        public Dictionary<string, string> AzureProjectList()
        {
            dynamic list = AzureDevOpsManager.GetProjectsList(ALMCore.DefaultAlmConfig.ALMServerURL, ALMCore.DefaultAlmConfig.ALMPassword);
            Dictionary<string, string> listOfItems = [];
            if (list.DataResult is null)
            {
                return listOfItems;
            }

            foreach (var item in list.DataResult)
            {
                foreach (var i in item.Projects)
                {
                    listOfItems.Add(i.Guid, i.ProjectName);
                }

            }
            return listOfItems;
        }

        public override Dictionary<Guid, string> CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, List<ExternalItemFieldBase> defectsFields, bool useREST = false)
        {
            try
            {

                Dictionary<Guid, string> defectsOpeningResults = [];
                foreach (KeyValuePair<Guid, Dictionary<string, string>> defectForOpening in defectsForOpening)
                {
                    defectsOpeningResults.Add(defectForOpening.Key, CreateOrUpdateDefectData(defectForOpening).Id.ToString());
                }
                return defectsOpeningResults;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Unable to Create New Defect", ex);

                return null;
            }
        }

        private static WorkItem CreateOrUpdateDefectData(KeyValuePair<Guid, Dictionary<string, string>> defectForOpening)
        {
            try
            {
                string tempDefectId = string.Empty;

                string summaryValue = defectForOpening.Value.ContainsKey("Summary") ? defectForOpening.Value["Summary"] : string.Empty;
                if (!string.IsNullOrEmpty(summaryValue))
                {
                    string defectId = CheckIfDefectExist(summaryValue);
                    if (!string.IsNullOrEmpty(defectId))
                    {
                        tempDefectId = defectId;
                    }
                }

                LoginDTO login = GetLoginDTO();

                VssConnection connection = AzureDevOpsRepository.LoginAzure(login);

                WorkItemTrackingHttpClient workItemTrackingClient = connection.GetClient<WorkItemTrackingHttpClient>();

                Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem newWorkItem = null;

                JsonPatchDocument patchDocument =
                [
                    new JsonPatchOperation()
                    {
                        Operation = Operation.Add,
                        Path = "/fields/System.Title",
                        Value = defectForOpening.Value.TryGetValue("Summary", out string value) ? value : string.Empty,
                    },
                    new JsonPatchOperation()
                    {
                        Operation = Operation.Add,
                        Path = "/fields/Microsoft.VSTS.TCM.ReproSteps",
                        Value = defectForOpening.Value.TryGetValue("description", out string systeminfo) ? systeminfo : string.Empty,
                    },
                ];

                patchDocument = AddAttachmentsToDefect(patchDocument, defectForOpening, workItemTrackingClient);

                if (!string.IsNullOrEmpty(tempDefectId))
                {
                    newWorkItem = workItemTrackingClient.UpdateWorkItemAsync(patchDocument, tempDefectId.ToInt32()).Result;
                }
                else
                {
                    newWorkItem = workItemTrackingClient.CreateWorkItemAsync(patchDocument, login.Project, AzureDevOpsManager.WorkItemTypeEnum.Bug.ToString()).Result;
                }

                return newWorkItem;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error creating defect ", ex);
                return null;
            }
        }

        private static JsonPatchDocument AddAttachmentsToDefect(JsonPatchDocument patchDocument, KeyValuePair<Guid, Dictionary<string, string>> defectForOpening, WorkItemTrackingHttpClient wit)
        {
            var attachmentPaths = defectForOpening.Value.TryGetValue("screenshots", out string picspath) ? picspath : string.Empty;

            if (string.IsNullOrEmpty(attachmentPaths))
            {
                return patchDocument;
            }

            var attachmentPathsArray = attachmentPaths.Split(',');
            dynamic attachment = null;

            foreach (var attachmentPath in attachmentPathsArray)
            {
                bool IsSuccess = false;
                int retryCount = 5;
                try
                {
                    Task.Run(async () =>
                    {
                        for (int attempt = 1; attempt <= retryCount; attempt++)
                        {
                            try
                            {
                                attachment = await wit.CreateAttachmentAsync(attachmentPath.Trim());
                                IsSuccess = true;
                                break;
                            }
                            catch (IOException ioEx) when (ioEx.Message.Contains("being used by another process"))
                            {
                                Reporter.ToLog(eLogLevel.DEBUG, $"Attempt {attempt} failed: File '{attachmentPath.Trim()}' is being used by another process. Retrying...");
                                Thread.Sleep(1000);
                            }
                            catch (Exception ex)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, $"Unexpected error adding attachment '{attachmentPath.Trim()}': {ex.Message}", ex);
                                break;
                            }
                        }

                        if (!IsSuccess)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, $"Failed to add attachment '{attachmentPath.Trim()}' after {retryCount} attempts.");
                        }
                    }).Wait();

                    if (IsSuccess)
                    {
                        patchDocument.Add(
                            new JsonPatchOperation()
                            {
                                Operation = Operation.Add,
                                Path = "/relations/-",
                                Value = new
                                {
                                    rel = "AttachedFile",
                                    url = attachment.Url,
                                    attributes = new
                                    {
                                        comment = "Attached Screenshot"
                                    }
                                }
                            }
                        );
                    }

                }

                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Unexpected error adding attachment '{attachmentPath.Trim()}': {ex.Message}", ex);
                    break;
                }

            }
            return patchDocument;

        }


        private static string CheckIfDefectExist(string summaryValue)
        {
            try
            {
                Wiql wiql = new Wiql()
                {
                    Query = $"Select [System.Title] From WorkItems WHERE [System.WorkItemType] = 'Bug' AND [System.Title] = '{summaryValue}'"
                };
                LoginDTO login = GetLoginDTO();

                VssConnection connection = AzureDevOpsRepository.LoginAzure(login);

                WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();

                WorkItemQueryResult queryResult = witClient.QueryByWiqlAsync(wiql).Result;

                if (queryResult.WorkItems.Any())
                {
                    List<int> workItemIds = queryResult.WorkItems.Select(wi => wi.Id).ToList();

                    // Fetch details of each work item
                    List<Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem> workItems = witClient.GetWorkItemsAsync(workItemIds, new[] { "System.Title" }).Result;

                    foreach (var workItem in workItems)
                    {
                        if (workItem.Fields["System.Title"].ToString().Equals(summaryValue))
                        {
                            return workItem.Id.ToString();
                        }
                    }
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Unable to get data from the Azure", ex);
            }
            return "";
        }

        public override bool ExportExecutionDetailsToALM(BusinessFlow bizFlow, ref string result, bool exectutedFromAutomateTab = false, PublishToALMConfig publishToALMConfig = null, ProjEnvironment projEnvironment = null)
        {
            if (bizFlow is null)
            {
                return false;
            }
            LoginDTO login = GetLoginDTO();

            try
            {
                // Establishing connection
                var connection = AzureDevOpsRepository.LoginAzure(login);
                var testClient = connection.GetClient<TestManagementHttpClient>();

                // Parsing external IDs
                if (!int.TryParse(bizFlow.ExternalID, out int testPlanId) || !int.TryParse(bizFlow.ExternalID2, out int suiteId))
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Unable to convert ExternalId: {bizFlow.ExternalID} of the BusinessFlow: {bizFlow.Name} to TestPlanId/SuiteId");
                    return false;
                }

                string projectName = login.Project;

                // Fetching test points
                var testPoints = testClient.GetPointsAsync(projectName, testPlanId, suiteId).Result;
                if (testPoints == null)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"No TestPoint found for given ProjectName: {projectName}, TestPlanId: {testPlanId}, SuiteId: {suiteId} or BusinessFlow: {bizFlow.Name}");
                    return false;
                }
                int TestCaseNotFounCnt = 0;
                foreach (ActivitiesGroup activitiesGroup in bizFlow.ActivitiesGroups)
                {
                    var matchingTC = testPoints.FirstOrDefault(p => activitiesGroup.ExternalID == p.TestCase.Id);

                    if (matchingTC != null)
                    {

                        string commentTestRes = string.Empty;
                        if (publishToALMConfig.ToExportReportLink)
                        {
                            string reportLink = General.CreateReportLinkPerFlow(HtmlReportUrl: publishToALMConfig.HtmlReportUrl, ExecutionId: publishToALMConfig.ExecutionId, BusinessFlowInstanceGuid: bizFlow.InstanceGuid.ToString());
                            commentTestRes = $"[Ginger Report Link]({reportLink})";

                        }
                        // Creating test run
                        var runModel = new RunCreateModel(name: matchingTC.TestCase.Name, plan: new Microsoft.TeamFoundation.TestManagement.WebApi.ShallowReference(bizFlow.ExternalID), pointIds: new[] { matchingTC.Id });
                        var testrun = testClient.CreateTestRunAsync(runModel, projectName).Result;

                        // Updating test results
                        var caseResult = new TestCaseResult { State = "Completed", Outcome = activitiesGroup.RunStatus.ToString(), Id = 100000, Comment = commentTestRes };
                        testClient.UpdateTestResultsAsync(new[] { caseResult }, projectName, testrun.Id);

                        // Updating test run
                        var runUpdateModel = new RunUpdateModel(state: "Completed");
                        testClient.UpdateTestRunAsync(runUpdateModel, projectName, testrun.Id, runUpdateModel);
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"No Matching TestCase(ActivityGroup) found for TestPointId: {matchingTC.Id}");
                        TestCaseNotFounCnt++;
                    }
                }

                if (TestCaseNotFounCnt == testPoints.Count)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "ExternalId not found for all Activity group are either Null or Incorrect");
                    return false;
                }

                result = "Export has been finished Successfully";
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error in ExportExecutionDetailsToALM", ex);
                Reporter.ToUser(eUserMsgKey.ALMIncorrectExternalID, ex.Message);
                return false;
            }
        }

        public override Dictionary<string, string> GetALMDomainProjects(string ALMDomainName)
        {
            return AzureProjectList();
        }

        public override List<string> GetALMDomains()
        {
            List<string> azureDomains = ["Azure Domain"];
            return azureDomains;
        }

        public override ObservableList<ExternalItemFieldBase> GetALMItemFields(BackgroundWorker bw, bool online, ResourceType resourceType = ResourceType.ALL)
        {
            ObservableList<ExternalItemFieldBase> fields = [];
            LoginDTO _loginDto = GetLoginDTO();

            string[] witType = ["Test Case", "Test Suite", "Test Plan"];
            foreach (var i in witType)
            {
                List<WorkItemTypeFieldWithReferences> listnodes = AzureDevOpsManager.GetListNodes(_loginDto, i);
                ExtractFields(fields, i, listnodes);
            }

            return UpdatedAlmFields(fields);
        }

        private void ExtractFields(ObservableList<ExternalItemFieldBase> fields, string resource2, List<WorkItemTypeFieldWithReferences> listnodes)
        {
            fields.Append(AddFieldsValues(resource2, listnodes));
        }

        private ObservableList<ExternalItemFieldBase> AddFieldsValues(string entityType, List<WorkItemTypeFieldWithReferences> listnodes)
        {
            ObservableList<ExternalItemFieldBase> fields = [];

            if (listnodes != null)
            {
                foreach (var field in listnodes)
                {
                    ExternalItemFieldBase itemfield = new ExternalItemFieldBase
                    {
                        ID = field.Name,
                        Name = field.Name,
                        ItemType = entityType,
                        Mandatory = field.AlwaysRequired
                    };

                    if (field.AllowedValues != null && field.AllowedValues.Length > 0)
                    {
                        itemfield.SelectedValue = (field.AllowedValues[0] != null) ? field.AllowedValues[0].ToString() : "Unassigned";
                        foreach (var item in field.AllowedValues)
                        {
                            itemfield.PossibleValues.Add(item?.ToString() ?? "Unassigned");
                        }
                    }
                    else
                    {
                        itemfield.SelectedValue = "Unassigned";
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

        private static LoginDTO GetLoginDTO()
        {
            LoginDTO loginDTO = new()
            {
                Password = ALMCore.DefaultAlmConfig.ALMPassword,
                Server = ALMCore.DefaultAlmConfig.ALMServerURL,
                Project = ALMCore.DefaultAlmConfig.ALMProjectName
            };

            return loginDTO;
        }

        public override void DisconnectALMServer()
        {
            return;
        }

        public override bool DisconnectALMProjectStayLoggedIn()
        {
            return true;
        }

        public Dictionary<string, List<string>> GetTestPlan()
        {
            List<TestPlanWebApi.TestPlan> listPlans = AzureDevOpsManager.GetTestPlans(GetLoginDTO());
            Dictionary<string, List<string>> tempPlan = [];
            if (listPlans != null)
            {
                foreach (var item in listPlans)
                {
                    if (!tempPlan.ContainsKey(item.Name))
                    {
                        tempPlan.Add(item.Name, []);
                        tempPlan[item.Name].Add(item.Iteration);
                        tempPlan[item.Name].Add(item.Id.ToString());
                        tempPlan[item.Name].Add(item.State);
                    }
                }
            }
            return tempPlan;
        }

        public bool ExportActivitiesGroupToALM(ActivitiesGroup activitiesGroup, ALMTestCase mappedTest, string fatherId, ObservableList<ExternalItemFieldBase> testCaseFields, ObservableList<ExternalItemFieldBase> designStepsFields, ObservableList<ExternalItemFieldBase> designStepsParamsFields, ref string result)
        {
            try
            {
                List<string> step = [];
                foreach (var item in activitiesGroup.ActivitiesIdentifiers)
                {
                    step.Add(item.ActivityName);
                }

                if (mappedTest == null) //#Create new test case
                {
                    CreateNewTestCase(activitiesGroup, fatherId, testCaseFields, step);
                }
                else //##update existing test case
                {
                    //TODO: Maheshk: Update existing testcase
                    if (!string.IsNullOrEmpty(activitiesGroup.ExternalID))
                    {
                        UpdateTestCase(activitiesGroup, fatherId, testCaseFields, step);
                    }
                    else
                    {
                        CreateNewTestCase(activitiesGroup, fatherId, testCaseFields, step);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                result = "Unexpected error occurred- " + ex.Message;
                Reporter.ToLog(eLogLevel.ERROR, "Failed to export the " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " to Azure/ALM", ex);
                return false;
            }
        }
        public bool ExportBusinessFlow(BusinessFlow businessFlow, ALMTestSetData mappedTestSet, string fatherId, ObservableList<ExternalItemFieldBase> testSetFields, ObservableList<ExternalItemFieldBase> testInstanceFields, ref string result)
        {
            int? testSetId = 0;
            try
            {
                if (mappedTestSet == null) //##create new Test Set in QC
                {
                    CreateTestPlan(businessFlow);
                }
                else //##update existing test set
                {
                    testSetId = UpdateExistingTestSuite(businessFlow, mappedTestSet, fatherId, testSetFields);
                }

                return true;
            }
            catch (Exception ex)
            {
                result = "Unexpected error occurred- " + ex.Message;
                Reporter.ToLog(eLogLevel.ERROR, "Failed to export the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " to Azure DevOps", ex);
                return false;
            }
        }

        public void TestCaseEntryInSuite(BusinessFlow bf)
        {
            try
            {
                LoginDTO login = GetLoginDTO();
                string projectName = login.Project;
                VssConnection connection = AzureDevOpsRepository.LoginAzure(login);

                if (Int32.TryParse(bf.ExternalID, out int planId) && Int32.TryParse(bf.ExternalID2, out int suiteId))
                {
                    // Get a testplan client instance
                    TestPlanHttpClient testPlanClient = connection.GetClient<TestPlanHttpClient>();

                    foreach (ActivitiesGroup ag in bf.ActivitiesGroups)
                    {
                        WorkItem2 testcasetoAdd = new()
                        {
                            Id = Int32.Parse(ag.ExternalID)
                        };

                        SuiteTestCaseCreateUpdateParameters parameters = new()
                        {
                            workItem = testcasetoAdd
                        };

                        IEnumerable<SuiteTestCaseCreateUpdateParameters> parametersCollection = [parameters];

                        testPlanClient.AddTestCasesToSuiteAsync(parametersCollection, projectName, planId, suiteId);
                    }
                }
                else
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to parse ExternalID/ExternalID2 as an integer.");
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error related to Suite case entry", ex);
            }
        }
        public static void CreateTestPlan(BusinessFlow bf)
        {
            LoginDTO login = GetLoginDTO();
            string projectName = login.Project;
            VssConnection connection = AzureDevOpsRepository.LoginAzure(login);

            // Get a testplan client instance
            TestPlanHttpClient testPlanClient = connection.GetClient<TestPlanHttpClient>();

            TestPlanWebApi.TestPlanCreateParams testPlanCreateParams = new TestPlanWebApi.TestPlanCreateParams()
            {
                Name = bf.Name,
            };

            // create a test plan
            TestPlanWebApi.TestPlan plan = testPlanClient.CreateTestPlanAsync(testPlanCreateParams, projectName).Result;

            // this will be the id of test plan
            bf.ExternalID = plan.Id.ToString();

            // this will be root suite id of the test plan
            bf.ExternalID2 = plan.RootSuite.Id.ToString();
        }

        public int? UpdateExistingTestSuite(BusinessFlow bf, ALMTestSetData mappedTestSet, string fatherId, ObservableList<ExternalItemFieldBase> testsetfield)
        {
            LoginDTO logincred = GetLoginDTO();
            // Get a testplan client instance
            VssConnection connection = AzureDevOpsRepository.LoginAzure(logincred);
            WorkItemTrackingHttpClient workItemTrackingClient = connection.GetClient<WorkItemTrackingHttpClient>();
            WorkItem workItem;
            JsonPatchDocument patchDocument =
            [
                new JsonPatchOperation()
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Title",
                    Value = bf.Name
                },
            ];

            workItem = workItemTrackingClient.UpdateWorkItemAsync(patchDocument, Int32.Parse(bf.ExternalID2)).Result;

            return workItem.Id;
        }

        public void CreateNewTestCase(ActivitiesGroup ag, string fatherId, ObservableList<ExternalItemFieldBase> testcasefields, List<string> step)
        {
            TestBaseHelper helper = new TestBaseHelper();
            ITestBase testBase = helper.Create();
            testBase = CreateTestStep(step, testBase);

            LoginDTO logincred = GetLoginDTO();
            // Get a testplan client instance
            VssConnection connection = AzureDevOpsRepository.LoginAzure(logincred);

            // Get a client
            WorkItemTrackingHttpClient witclient = connection.GetClient<WorkItemTrackingHttpClient>();

            JsonPatchDocument json = [];

            // create a title field
            JsonPatchOperation patchDocument1 = new JsonPatchOperation
            {
                Operation = Operation.Add,
                Path = "/fields/System.Title",
                Value = ag.Name
            };
            json.Add(patchDocument1);

            // add test steps in json
            // it will update json document based on test steps and attachments
            json = testBase.SaveActions(json);

            // create a test case
            var testCaseObject = witclient.CreateWorkItemAsync(json, logincred.Project, "Test Case").Result;
            ag.ExternalID = testCaseObject.Id.ToString();
        }

        public void UpdateTestCase(ActivitiesGroup ag, string fatherId, ObservableList<ExternalItemFieldBase> testcasefields, List<string> step)
        {
            try
            {
                LoginDTO logincred = GetLoginDTO();
                // Get a testplan client instance
                VssConnection connection = AzureDevOpsRepository.LoginAzure(logincred);

                // Get a client
                WorkItemTrackingHttpClient _witClient = connection.GetClient<WorkItemTrackingHttpClient>();

                int testcaseId = Int32.Parse(ag.ExternalID);
                var testCaseObject = _witClient.GetWorkItemAsync(logincred.Project, testcaseId, null, null, WorkItemExpand.Relations).Result;

                // initiate testbase object again
                ITestBaseHelper helper = new TestBaseHelper();
                ITestBase testBase = helper.Create();

                testBase = CreateTestStep(step, testBase);

                JsonPatchDocument patchDocument = [];

                // create a title field
                JsonPatchOperation patchDocument1 = new JsonPatchOperation
                {
                    Operation = Operation.Add,
                    Path = "/fields/System.Title",
                    Value = ag.Name
                };
                patchDocument.Add(patchDocument1);

                // add test steps in json
                // it will update json document based on test steps and attachments
                patchDocument = testBase.SaveActions(patchDocument);

                var ress = _witClient.UpdateWorkItemAsync(patchDocument, Int32.Parse(ag.ExternalID)).Result;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Unable to update the test case", ex);
            }
        }

        public static ITestBase CreateTestStep(List<string> step, ITestBase testBase)
        {
            foreach (var title in step)
            {
                ITestStep testStep = testBase.CreateTestStep();
                testStep.Title = title;
                testBase.Actions.Add(testStep);
            }

            return testBase;
        }


        public string GetLastTestPlanIdFromPath(string path)
        {
            string[] separatePath;
            if (!string.IsNullOrEmpty(path))
            {
                if (!path.Contains("Application Modules"))
                {
                    path = @"Application Modules\" + path;
                }
                separatePath = path.Split('\\');
                separatePath[0] = ExploredApplicationModule.ContainsKey("Application Modules") ? ExploredApplicationModule["Application Modules"] : GetRootFolderId();

                if (!ExploredApplicationModule.ContainsKey("Application Modules"))
                {
                    ExploredApplicationModule.Add("Application Modules", separatePath[0]);
                }
                for (int i = 1; i < separatePath.Length; i++)
                {
                    separatePath[i] = GetTestLabFolderId(separatePath[i], separatePath[i - 1]);
                }

                return separatePath.Last();
            }
            else
            {
                return ExploredApplicationModule.ContainsKey("Application Modules") ? ExploredApplicationModule["Application Modules"] : GetRootFolderId();
            }
        }

        private string GetTestLabFolderId(string separateAti, string separateAtIMinusOne)
        {
            LoginDTO login = GetLoginDTO();
            if (!ExploredApplicationModule.ContainsKey(separateAti))
            {
                return "dummy";
            }
            else
            {
                return ExploredApplicationModule[separateAti];
            }
        }

        private static string GetRootFolderId()
        {
            LoginDTO loginDTO = GetLoginDTO();

            return Task.Run(() =>
            {
                Wiql wiql = new Wiql()
                {
                    Query = "Select [System.Id]" +
                            "From WorkItems"
                };

                VssConnection connection = AzureDevOpsRepository.LoginAzure(loginDTO);

                WorkItemTrackingHttpClient witClient = connection.GetClient<WorkItemTrackingHttpClient>();
                WorkItemQueryResult workItemQueryResult = witClient.QueryByWiqlAsync(wiql).Result;
                return workItemQueryResult;
            }).Result.QueryResultType.ToString();
        }

        public string CreateApplicationModule(string appModuleNameTobeCreated, string desc, string paraentId)
        {
            ApplicationModule applicationModule = new ApplicationModule
            {
                Name = appModuleNameTobeCreated
            };
            applicationModule.SetValue("description", desc);

            applicationModule.SetValue("parent", new BaseEntity("application_module")
            {
                Id = paraentId,
                TypeName = "application_module"
            });

            ApplicationModule module = Task.Run(() =>
            {
                ApplicationModule ap = new()
                {
                    Name = appModuleNameTobeCreated
                };
                return ap;
            }).Result;

            return module.Id.ToString();
        }

        public List<string> GetTestLabExplorer(string path)
        {

            List<string> testlabPathList = [];
            try
            {
                Dictionary<string, List<string>> listoftestPlans = GetTestPlan();
                foreach (var testset in listoftestPlans)
                {
                    testlabPathList.Add(testset.Key);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get Test Lab with REST API", ex);
            }
            return testlabPathList;
        }

        public List<AzureTestPlan> GetTestSetExplorer(string PathNode)
        {
            List<AzureTestPlan> testlabPathList = [];
            Dictionary<string, List<string>> listoftestPlans = GetTestPlan();
            foreach (var testset in listoftestPlans)
            {
                AzureTestPlan azureTestPlanItems = new AzureTestPlan
                {
                    Name = testset.Key,
                    AzureID = testset.Value[1],
                    State = testset.Value[2],
                    Project = testset.Value[0]
                };
                testlabPathList.Add(azureTestPlanItems);
            }

            return testlabPathList;
        }
        public dynamic GetTSRunStatus(dynamic TSItem)
        {
            List<TestSuite> testInstances = GetTestSuiteRun(TSItem.TestSetID);

            foreach (TestSuite testInstance in testInstances)
            {
                bool existing = false;
                foreach (string[] status in TSItem.TestSetStatuses)
                {
                    if (status[0] == testInstance.Name)
                    {
                        existing = true;
                        status[1] = (Int32.Parse(status[1]) + 1).ToString();
                    }
                }
                if (!existing) { TSItem.TestSetStatuses.Add(new string[] { testInstance.Name, "1" }); }
            }
            return TSItem;
        }

        public List<TestPlanWebApi.TestPlan> GetTestSuiteRun(string testSuiteId)
        {
            string projectName = ALMCore.DefaultAlmConfig.ALMProjectName;

            LoginDTO loginCred = GetLoginDTO();
            // Get a testplan client instance
            VssConnection connection = AzureDevOpsRepository.LoginAzure(loginCred);
            TestPlanHttpClient testPlanClient = connection.GetClient<TestPlanHttpClient>();

            // Get test plans
            List<TestPlanWebApi.TestPlan> plans = testPlanClient.GetTestPlansAsync(projectName).Result;
            return plans;
        }
        public ALMTestSetData GetTestSuiteById(string tsId)
        {
            LoginDTO logincred = GetLoginDTO();

            // Get a testplan client instance
            VssConnection connection = AzureDevOpsRepository.LoginAzure(logincred);
            WorkItemTrackingHttpClient witc = connection.GetClient<WorkItemTrackingHttpClient>();

            TestPlanHttpClient testPlanClient = connection.GetClient<TestPlanHttpClient>();
            List<TestPlan> plans = testPlanClient.GetTestPlansAsync(logincred.Project).Result;

            if (Int32.TryParse(tsId, out int testplanId))
            {
                try
                {
                    int suiteId = testplanId + 1;
                    TestSuite testsuite = testPlanClient.GetTestSuiteByIdAsync(logincred.Project, testplanId, suiteId).Result;

                    ALMTestSetData aLMTestSetData = new()
                    {
                        Id = testsuite.Id.ToString(),
                        Name = testsuite.Name,
                        ParentId = testplanId.ToString()

                    };

                    return aLMTestSetData;
                }
                catch (Exception ex)
                {
                    Reporter.ToUser(eUserMsgKey.ALMIncorrectExternalID, $"{ex.InnerException.Message}", ex);
                    return null;
                }
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, "Unable to parse ExternalId to test suite id");
                return null;
            }
        }

        public AzureTestPlan GetAzureTestSetData(AzureTestPlan selectedTS)
        {
            try
            {
                int testplanId = Int32.Parse(selectedTS.AzureID);
                LoginDTO logincred = GetLoginDTO();
                VssConnection? connection = AzureDevOpsRepository.LoginAzure(logincred);

                TestPlanHttpClient testPlanClient = connection.GetClient<TestPlanHttpClient>();
                var testCasesIds = testPlanClient.GetTestCaseListAsync(logincred.Project, testplanId, testplanId + 1).Result;

                // In case of  Import from Test Set By d
                if (string.IsNullOrEmpty(selectedTS.Name) && string.IsNullOrEmpty(selectedTS.Project))
                {
                    var itm = testPlanClient.GetTestSuiteByIdAsync(logincred.Project, testplanId, testplanId + 1).Result;
                    selectedTS.Name = itm.Name;
                    selectedTS.Project = logincred.Project;
                }

                List<AzureTestCasesSteps> testCasesSteps = [];

                foreach (var testCaseId in testCasesIds)
                {
                    testCasesSteps = SetTestCaseSteps(testCaseId);

                    selectedTS.TestCases.Add(new AzureTestCases
                    {
                        TestName = testCaseId.workItem.Name,
                        TestID = testCaseId.workItem.Id.ToString(),
                        Steps = testCasesSteps
                    });
                }
                return selectedTS;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Unable to Get the Azure Test Plan Data", ex);
                return null;
            }
        }

        /// <summary>
        /// Need to implement this method, related to test steps during import, explore ITestStep interface for parametrized tag
        /// </summary>
        /// <param name="testCasesIds"></param>
        /// <returns></returns>
        public List<AzureTestCasesSteps> SetTestCaseSteps(dynamic testCasesId)
        {

            JObject testStep = (JObject)testCasesId.workItem.WorkItemFields[0];
            string testStepValue = testStep.GetValue("Microsoft.VSTS.TCM.Steps").ToString();

            List<AzureTestCasesSteps> testCasesSteps = [];
            if (!string.IsNullOrEmpty(testStepValue))
            {
                XmlDocument xmlDoc = new();
                xmlDoc.LoadXml(testStepValue);
                if (xmlDoc != null)
                {
                    var test = xmlDoc.FirstChild.ChildNodes;
                    foreach (var item in test)
                    {
                        var stepText = RemoveHtmlTags(((XmlNode)item)?.FirstChild?.InnerText);
                        testCasesSteps.Add(new AzureTestCasesSteps(stepText, Guid.NewGuid().ToString()));
                    }
                }
                else
                {
                    Reporter.ToLog(eLogLevel.INFO, "Unable to convert Test Step/s to XML");
                }
            }
            else
            {
                Reporter.ToLog(eLogLevel.INFO, "Test Case may not contains test steps");
            }
            return testCasesSteps;
        }

        static string RemoveHtmlTags(string htmlString)
        {
            return Regex.Replace(htmlString, "<.*?>", string.Empty);
        }

        public BusinessFlow ConvertAzureTestSetToBF(AzureTestPlan azureTestPlan)
        {
            try
            {
                if (azureTestPlan == null)
                {
                    return null;
                }

                //Create Business Flow
                BusinessFlow busFlow = new BusinessFlow
                {
                    Name = azureTestPlan.Name,
                    ExternalID = azureTestPlan.AzureID,
                    Status = BusinessFlow.eBusinessFlowStatus.Development,
                    Activities = [],
                    Variables = [],

                    //Test suite ID
                    ExternalID2 = (Int32.Parse(azureTestPlan.AzureID) + 1).ToString()
                };

                //Create Activities Group + Activities for each TC
                foreach (AzureTestCases tc in azureTestPlan.TestCases)
                {
                    ActivitiesGroup tcActivsGroup = ConvertAzureTestToAG(busFlow, tc);

                    //Add the TC steps as Activities if not already on the Activities group
                    foreach (AzureTestCasesSteps step in tc.Steps)
                    {
                        Activity stepActivity;
                        bool toAddStepActivity;
                        ConvertAzureTestStepToActivity(busFlow, tc, tcActivsGroup, step, out stepActivity, out toAddStepActivity);

                        if (toAddStepActivity)
                        {
                            //not in group- need to add it
                            busFlow.AddActivity(stepActivity, tcActivsGroup);
                        }
                    }

                    //order the Activities Group activities according to the order of the matching steps in the TC
                    try
                    {
                        int startGroupActsIndxInBf = 0;
                        if (tcActivsGroup.ActivitiesIdentifiers.Count > 0)
                        {
                            startGroupActsIndxInBf = busFlow.Activities.IndexOf(tcActivsGroup.ActivitiesIdentifiers[0].IdentifiedActivity);
                        }
                        foreach (AzureTestCasesSteps step in tc.Steps)
                        {
                            int stepIndx = tc.Steps.IndexOf(step) + 1;
                            ActivityIdentifiers actIdent = tcActivsGroup.ActivitiesIdentifiers.FirstOrDefault(x => x.ActivityExternalID == step.StepID);
                            if (actIdent == null || actIdent.IdentifiedActivity == null)
                            {
                                break;//something wrong- shouldnt be null
                            }

                            Activity act = actIdent.IdentifiedActivity;
                            int groupActIndx = tcActivsGroup.ActivitiesIdentifiers.IndexOf(actIdent);
                            int bfActIndx = busFlow.Activities.IndexOf(act);

                            //set it in the correct place in the group
                            int numOfSeenSteps = 0;
                            int groupIndx = -1;
                            foreach (ActivityIdentifiers ident in tcActivsGroup.ActivitiesIdentifiers)
                            {
                                groupIndx++;
                                if (string.IsNullOrEmpty(ident.ActivityExternalID) ||
                                        tc.Steps.FirstOrDefault(x => x.StepID == ident.ActivityExternalID) == null)
                                {
                                    continue;//activity which not originaly came from the TC
                                }

                                numOfSeenSteps++;

                                if (numOfSeenSteps >= stepIndx)
                                {
                                    break;
                                }
                            }
                            ActivityIdentifiers identOnPlace = tcActivsGroup.ActivitiesIdentifiers[groupIndx];
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
                        Reporter.ToLog(eLogLevel.ERROR, $"Method , Error - {ex.Message}", ex);
                        //failed to re order the activities to match the tc steps order, not worth breaking the import because of this
                    }
                }
                return busFlow;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to import Azure test set and convert it into " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), ex);
                return null;
            }
        }


        private ActivitiesGroup ConvertAzureTestToAG(BusinessFlow busFlow, AzureTestCases tc)
        {
            //check if the TC is already exist in repository
            ActivitiesGroup tcActivsGroup;
            ActivitiesGroup repoActivsGroup = null;

            if (repoActivsGroup != null)
            {
                List<Activity> repoNotExistsStepActivity = GingerActivitiesRepo.Where(z => repoActivsGroup.ActivitiesIdentifiers.Select(y => y.ActivityExternalID).ToList().Contains(z.ExternalID))
                                                                               .Where(x => !tc.Steps.Select(y => y.StepID).ToList().Contains(x.ExternalID)).ToList();

                tcActivsGroup = (ActivitiesGroup)repoActivsGroup.CreateInstance(true);

                var ActivitySIdentifiersToRemove = tcActivsGroup.ActivitiesIdentifiers.Where(x => repoNotExistsStepActivity.Select(z => z.ExternalID).ToList().Contains(x.ActivityExternalID));
                for (int indx = tcActivsGroup.ActivitiesIdentifiers.Count - 1; indx >= 0; indx--)
                {
                    if ((indx < tcActivsGroup.ActivitiesIdentifiers.Count) && (ActivitySIdentifiersToRemove.Contains(tcActivsGroup.ActivitiesIdentifiers[indx])))
                    {
                        tcActivsGroup.ActivitiesIdentifiers.Remove(tcActivsGroup.ActivitiesIdentifiers[indx]);
                    }
                }

                tcActivsGroup.ExternalID = tc.TestID;
                busFlow.AddActivitiesGroup(tcActivsGroup);
                busFlow.ImportActivitiesGroupActivitiesFromRepository(tcActivsGroup, GingerActivitiesRepo, ApplicationPlatforms, true);

                busFlow.AttachActivitiesGroupsAndActivities();
            }
            else //TC not exist in Ginger repository so create new one
            {
                tcActivsGroup = new ActivitiesGroup
                {
                    Name = tc.TestName,
                    ExternalID = tc.TestID,
                    Description = tc.Description
                };

                busFlow.AddActivitiesGroup(tcActivsGroup);
            }

            return tcActivsGroup;
        }

        private void ConvertAzureTestStepToActivity(BusinessFlow busFlow, AzureTestCases tc, ActivitiesGroup tcActivsGroup, AzureTestCasesSteps step, out Activity stepActivity, out bool toAddStepActivity)
        {
            toAddStepActivity = false;

            //check if mapped activity exist in repository
            Activity repoStepActivity = null;
            if (repoStepActivity != null)
            {
                //check if it is part of the Activities Group
                ActivityIdentifiers groupStepActivityIdent = tcActivsGroup.ActivitiesIdentifiers.FirstOrDefault(x => x.ActivityExternalID == step.StepID);
                if (groupStepActivityIdent != null)
                {
                    //already in Activities Group so get link to it
                    stepActivity = busFlow.Activities.FirstOrDefault(x => x.Guid == groupStepActivityIdent.ActivityGuid);
                    // in any case update description/expected/name - even if "step" was taken from repository

                    stepActivity.ActivityName = step.StepName;
                }
                else//not in ActivitiesGroup so get instance from repo
                {
                    stepActivity = (Activity)repoStepActivity.CreateInstance();
                    stepActivity.ExternalID = step.StepID;
                    toAddStepActivity = true;
                }
            }
            else//Step not exist in Ginger repository so create new one
            {
                stepActivity = new Activity
                {
                    ActivityName = step.StepName,
                    ExternalID = step.StepID
                };

                toAddStepActivity = true;
            }
        }
    }
}
