#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Microsoft.VisualStudio.Services.WebApi;
using Amdocs.Ginger.Common;
using GingerCore.Activities;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerCoreNET.ALMLib;
using System.Collections.Generic;
using Amdocs.Ginger.Repository;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.WebApi.Patch;
using Microsoft.VisualStudio.Services.WebApi.Patch.Json;
using GingerCore.Environments;
using System.ComponentModel;
using AlmDataContractsStd.Enums;
using AzureRepositoryStd.BLL;
using AzureRepositoryStd;
using AlmDataContractsStd.Contracts;
using TestPlanWebApi = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using OctaneStdSDK.Entities.Releases;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using WorkItem = Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models.WorkItem;
using Microsoft.TeamFoundation.TestManagement.WebApi;
using System.Linq;
using System.Threading.Tasks;
using OctaneStdSDK.Entities.Base;
using Amdocs.Ginger.CoreNET.ALMLib.DataContract;
using GingerCore.ALM.QC;
using TestSuite = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestSuite;

namespace GingerCore.ALM
{
    public class AzureDevOpsCore : ALMCore
    {
        protected AzureDevOpsRepository opsRepository;
        
        private static readonly Dictionary<string, string> ExploredApplicationModule = new Dictionary<string, string>();
        public override ALMIntegrationEnums.eALMType ALMType => ALMIntegrationEnums.eALMType.Azure;

        public AzureDevOpsCore()
        {
            opsRepository = new AzureDevOpsRepository(new LoginDTO()
            {
                Password = ALMCore.DefaultAlmConfig.ALMPassword,
                Server = ALMCore.DefaultAlmConfig.ALMServerURL
            });
            
        }


        public override ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override ObservableList<Activity> GingerActivitiesRepo { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override ObservableList<ApplicationPlatform> ApplicationPlatforms { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


      

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

        public Dictionary<string,string> AzureProjectList()
        {
            dynamic list = AzureDevOpsRepository.GetLoginProjects(ALMCore.DefaultAlmConfig.ALMServerURL,ALMCore.DefaultAlmConfig.ALMPassword);
            Dictionary<string, string> listOfItems = new();
            if (list.DataResult is null)
            {
                return listOfItems;
            }

            foreach(var item in list.DataResult)
            {
                foreach (var i in item.Projects)
                {
                    listOfItems.Add(i.Guid,i.ProjectName);
                }
                
            }
            return listOfItems;
        }

        public override Dictionary<Guid, string> CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, List<ExternalItemFieldBase> defectsFields, bool useREST = false)
        {
            throw new NotImplementedException();
        }

      
        public override bool ExportExecutionDetailsToALM(BusinessFlow bizFlow, ref string result, bool exectutedFromAutomateTab = false, PublishToALMConfig publishToALMConfig = null, ProjEnvironment projEnvironment = null)
        {
            throw new NotImplementedException();
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
            ObservableList<ExternalItemFieldBase> fields = new ObservableList<ExternalItemFieldBase>();
            LoginDTO _loginDto = GetLoginDTO();

            string[] witType = ["Test Case", "Test Suite", "Test Plan"];
            foreach(var i in witType)
            {
                List<WorkItemTypeFieldWithReferences> listnodes =  AzureDevOpsRepository.GetListNodes(_loginDto, i);
               

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
           
            if ((listnodes != null))
            {
                foreach(var field in listnodes)
                {

                    ExternalItemFieldBase itemfield = new ExternalItemFieldBase();
                    itemfield.ID = field.Name;
                    itemfield.Name = field.Name;
                    itemfield.ItemType = entityType;
                    itemfield.Mandatory = field.AlwaysRequired;

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

        public Dictionary<string,List<string>> GetTestPlan()
        {
            List <TestPlanWebApi.TestPlan> listPlans =  AzureDevOpsManager.GetTestPlans(GetLoginDTO());
            Dictionary<string, List<string>> tempPlan = [];
            if (listPlans !=null )
            {
                foreach (var item in listPlans)
                {
                    if (!tempPlan.ContainsKey(item.Name))
                    {
                        tempPlan.Add(item.Name, []);
                        tempPlan[item.Name].Add(item.Iteration);
                        tempPlan[item.Name].Add(item.Id.ToString());
                    }
                }
            }
            return tempPlan;
        }

        public bool ExportActivitiesGroupToALM(ActivitiesGroup activitiesGroup, ALMTestCase mappedTest, string fatherId, ObservableList<ExternalItemFieldBase> testCaseFields, ObservableList<ExternalItemFieldBase> designStepsFields, ObservableList<ExternalItemFieldBase> designStepsParamsFields, ref string result)
        {
            try
            {
                string step = string.Empty;
                activitiesGroup.ActivitiesIdentifiers.ToList().ForEach(p =>
                {
                    if (!string.IsNullOrEmpty(step))
                    {
                        step += "- ";
                    }
                    step += p.ActivityName + " ";

                    if (p.IdentifiedActivity.Variables.Any())
                    {
                        p.IdentifiedActivity.Variables.ToList().ForEach(f =>
                        {
                            step += " <" + f.Name + "> ";
                        });
                    }
                    step += '\n';
                });

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
                    testSetId = CreateNewTestSuite(businessFlow, fatherId, testSetFields);
                }
                else //##update existing test set
                {
                    testSetId = UpdateExistingTestSuite(businessFlow, mappedTestSet, fatherId, testSetFields);
                }

                businessFlow.ExternalID = testSetId.ToString();
                return true;
            }
            catch (Exception ex)
            {
                result = "Unexpected error occurred- " + ex.Message;
                Reporter.ToLog(eLogLevel.ERROR, "Failed to export the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " to Octane ALM", ex);
                return false;
            }
        }
        public int? CreateNewTestSuite(BusinessFlow bf, string fatherId, ObservableList<ExternalItemFieldBase> testsetfield)
        {
            LoginDTO logincred = GetLoginDTO();
            // Get a testplan client instance
            VssConnection connection = AzureDevOpsRepository.LoginAzure(logincred);
            WorkItemTrackingHttpClient workItemTrackingClient = connection.GetClient<WorkItemTrackingHttpClient>();
            WorkItem workItem;
            JsonPatchDocument patchDocument = new JsonPatchDocument();


            patchDocument.Add(
               new JsonPatchOperation()
               {
                   Operation = Operation.Add,
                   Path = "/fields/System.Title",
                   Value = bf.Name
               });

            workItem = workItemTrackingClient.CreateWorkItemAsync(patchDocument, logincred.Project, "Test Suite").Result;
            bf.ExternalID = workItem.Id.ToString();
            
            return workItem.Id;

        }

        public int? UpdateExistingTestSuite(BusinessFlow bf, ALMTestSetData mappedTestSet, string fatherId, ObservableList<ExternalItemFieldBase> testsetfield)
        {
            LoginDTO logincred = GetLoginDTO();
            // Get a testplan client instance
            VssConnection connection = AzureDevOpsRepository.LoginAzure(logincred);
            WorkItemTrackingHttpClient workItemTrackingClient = connection.GetClient<WorkItemTrackingHttpClient>();
            WorkItem workItem;
            JsonPatchDocument patchDocument = new JsonPatchDocument();


            patchDocument.Add(
               new JsonPatchOperation()
               {
                   Operation = Operation.Add,
                   Path = "/fields/System.Title",
                   Value = bf.Name
               });

            workItem = workItemTrackingClient.CreateWorkItemAsync(patchDocument, fatherId, "Test Suite").Result;
            return workItem.Id;
        }

        public void CreateNewTestCase(ActivitiesGroup ag, string fatherId, ObservableList<ExternalItemFieldBase> testcasefields, string step)
        {
            TestBaseHelper helper = new TestBaseHelper();
            ITestBase testBase = helper.Create();
            testBase = CreateTestStep(step,testBase);

            LoginDTO logincred = GetLoginDTO();
            // Get a testplan client instance
            VssConnection connection = AzureDevOpsRepository.LoginAzure(logincred);

            // Get a client
            WorkItemTrackingHttpClient witclient = connection.GetClient<WorkItemTrackingHttpClient>();

            JsonPatchDocument json = new JsonPatchDocument();

            // create a title field
            JsonPatchOperation patchDocument1 = new JsonPatchOperation();
            patchDocument1.Operation = Operation.Add;
            patchDocument1.Path = "/fields/System.Title";
            patchDocument1.Value = ag.Name;
            json.Add(patchDocument1);

            // add test steps in json
            // it will update json document based on test steps and attachments
            json = testBase.SaveActions(json);
            
            // create a test case
            var testCaseObject = witclient.CreateWorkItemAsync(json, logincred.Project, "Test Case").Result;
            ag.ExternalID = testCaseObject.Id.ToString();
            ag.ExternalID2 = testCaseObject.Id.ToString();

        }

        public void UpdateTestCase(ActivitiesGroup ag, string fatherId, ObservableList<ExternalItemFieldBase> testcasefields, string step)
        {
            LoginDTO logincred = GetLoginDTO();
            // Get a testplan client instance
            VssConnection connection = AzureDevOpsRepository.LoginAzure(logincred);

            // Get a client
            WorkItemTrackingHttpClient _witClient = connection.GetClient<WorkItemTrackingHttpClient>();

            int testcaseId = Int32.Parse(ag.ExternalID);
            var testCaseObject = _witClient.GetWorkItemAsync( logincred.Project, testcaseId, null, null, WorkItemExpand.Relations).Result;

            // initiate testbase object again
            ITestBaseHelper helper = new TestBaseHelper();
           ITestBase testBase = helper.Create();
            var xml = testCaseObject.Fields["Microsoft.VSTS.TCM.Steps"].ToString();

            // create tcmattachemntlink object from workitem relation, teststep helper will use this
         
        }


        public ITestBase CreateTestStep(string step, ITestBase testBase)
        {
            ITestStep testStep1 = testBase.CreateTestStep();
            testStep1.Title = step;
            testStep1.ExpectedResult = "Pass";

            testBase.Actions.Add(testStep1);
            return testBase;
        }


        public  string GetLastTestPlanIdFromPath(string path)
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

        private  string GetTestLabFolderId(string separateAti, string separateAtIMinusOne)
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

        private string GetRootFolderId()
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
            ApplicationModule applicationModule = new ApplicationModule();
            applicationModule.Name = appModuleNameTobeCreated;
            applicationModule.SetValue("description", desc);

            applicationModule.SetValue("parent", new BaseEntity("application_module")
            {
                Id = paraentId,
                TypeName = "application_module"
            });

            ApplicationModule module = Task.Run(() =>
            {
                ApplicationModule ap = new();
                ap.Name = appModuleNameTobeCreated;
                return ap;
            }).Result;

            return module.Id.ToString();
        }

        public List<string> GetTestLabExplorer(string path)
        {

            List<string> testlabPathList = new List<string>();
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

        public List<ALMTestSetSummary> GetTestSetExplorer(string PathNode)
        {
            List<ALMTestSetSummary> testlabPathList = new List<ALMTestSetSummary>();
            Dictionary<string, List<string>> listoftestPlans = GetTestPlan();
            foreach (var testset in listoftestPlans)
                {
                    ALMTestSetSummary QCTestSetTreeItem = new ALMTestSetSummary();
                QCTestSetTreeItem.TestSetID = testset.Value[1];
                    QCTestSetTreeItem.TestSetName = testset.Key;
                    testlabPathList.Add(QCTestSetTreeItem);
                }
            
            return testlabPathList;
        }

        public ALMTestSetData GetTestSuiteById(string tsId)
        {
            

            LoginDTO logincred = GetLoginDTO();

            // Get a testplan client instance
            VssConnection connection = AzureDevOpsRepository.LoginAzure(logincred);
            WorkItemTrackingHttpClient witc = connection.GetClient<WorkItemTrackingHttpClient>();

            TestPlanHttpClient testPlanClient = connection.GetClient<TestPlanHttpClient>();

            int testplanId = 21;
            int id = Int32.Parse(tsId);

            TestSuite suite = testPlanClient.GetTestSuiteByIdAsync(logincred.Project,testplanId,id).Result;

            ALMTestSetData aLMTestSetData = new()
            {
                Id = suite.Id.ToString(),
                Name = suite.Name,
                
            };

            return aLMTestSetData;
        }
    }
}
