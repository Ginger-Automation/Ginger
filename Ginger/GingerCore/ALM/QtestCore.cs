#region License
/*
Copyright © 2014-2019 European Support Limited

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
using GingerCore.Activities;
using GingerCore.ALM.QC;
using GingerCore.ALM.Qtest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using TDAPIOLELib;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common.InterfacesLib;
using System.Linq;

namespace GingerCore.ALM
{
    public class QtestCore : ALMCore
    {
        public QtestCore() { }

        QTestApi.LoginApi connObj = new QTestApi.LoginApi();
        QTestApi.ProjectApi projectsApi = new QTestApi.ProjectApi();
        QTestApi.TestsuiteApi testsuiteApi = new QTestApi.TestsuiteApi();
        QTestApi.TestrunApi testrunApi = new QTestApi.TestrunApi();
        QTestApi.TestcaseApi testcaseApi = new QTestApi.TestcaseApi();

        QTestApiClient.ApiClient apiClient = new QTestApiClient.ApiClient();
        QTestApiClient.Configuration configuration = new QTestApiClient.Configuration();

        public override bool ConnectALMServer()
        {
            try
            {
                System.Diagnostics.Trace.WriteLine("Initiated Authentication");

                connObj = new QTestApi.LoginApi(ALMCore.AlmConfig.ALMServerURL);
                string granttype = "password";
                string authorization = "Basic bWFoZXNoLmthbGUzQHQtbW9iaWxlLmNvbTo=";
                QTestApiModel.OAuthResponse response = connObj.PostAccessToken(granttype, ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword, authorization);
                connObj.Configuration.AccessToken = response.AccessToken;
                connObj.Configuration.ApiKey.Add("Authorization", response.AccessToken);
                connObj.Configuration.ApiKeyPrefix.Add("Authorization", response.TokenType);
                System.Diagnostics.Trace.WriteLine("Authentication Successful");
                return true;
            }           
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("Exception in AuthenticateUser(): Authentication Failed: " + ex.Message);

                connObj = null;
                return false;
            }
        }

        public override bool ConnectALMProject()
        {
            ALMCore.AlmConfig.ALMProjectName = ALMCore.AlmConfig.ALMProjectKey;
            return true;
        }

        public override Boolean IsServerConnected()
        {
            // return QCConnect.IsServerConnected;
            return true;
        }

        public override void DisconnectALMServer()
        {
            // QCConnect.DisconnectQCServer();
        }

        public override List<string> GetALMDomains()
        {
            // return QCConnect.GetQCDomains();
            return new List<string>();
        }

        public override Dictionary<string,string> GetALMDomainProjects(string ALMDomain)
        {
            projectsApi = new QTestApi.ProjectApi(connObj.Configuration);
            List<QTestApiModel.ProjectResource> projectList =  projectsApi.GetProjects("descendents", true);
            return projectList.ToDictionary(f => f.Id.ToString(), f => f.Name);
        }

        public override bool DisconnectALMProjectStayLoggedIn()
        {
            // return QCConnect.DisconnectQCProjectStayLoggedIn();
            return true;
        }

        public override ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo
        {
            get { return ImportFromQtest.GingerActivitiesGroupsRepo; }
            set { ImportFromQtest.GingerActivitiesGroupsRepo = value; }
        }

        public override ObservableList<Activity> GingerActivitiesRepo
        {
            get { return ImportFromQtest.GingerActivitiesRepo; }
            set { ImportFromQtest.GingerActivitiesRepo = value; }
        }

        public override ObservableList<ExternalItemFieldBase> GetALMItemFields(BackgroundWorker bw, bool online, ALM_Common.DataContracts.ResourceType resourceType)
        {
            // return UpdatedAlmFields(ImportFromQtest.GetALMItemFields());
            return null;
        }
      
        public override bool ExportExecutionDetailsToALM(BusinessFlow bizFlow, ref string result, bool exectutedFromAutomateTab = false, PublishToALMConfig publishToALMConfig = null)
        {
            // return ExportToQC.ExportExecutionDetailsToQC(bizFlow, ref result, publishToALMConfig);
            return true;
        }

        public bool ExportActivitiesGroupToALM(ActivitiesGroup activitiesGroup, QtestTest mappedTest, string uploadPath, ObservableList<ExternalItemFieldBase> testCaseFields, ref string result)
        {
            // return ExportToQC.ExportActivitiesGroupToQC(activitiesGroup, mappedTest, uploadPath, testCaseFields, ref result);
            return true;
        }

        public bool ExportBusinessFlowToALM(BusinessFlow businessFlow, QtestTestSuite mappedTestSuite, string uploadPath, ObservableList<ExternalItemFieldBase> testSetFields, ref string result)
        {
            // return ExportToQC.ExportBusinessFlowToQC(businessFlow, mappedTestSuite, uploadPath, testSetFields, ref result);

            testrunApi = new QTestApi.TestrunApi(connObj.Configuration); 
            testcaseApi = new QTestApi.TestcaseApi(connObj.Configuration);
            testsuiteApi = new QTestApi.TestsuiteApi(connObj.Configuration);

            ObservableList<ActivitiesGroup> existingActivitiesGroups = new ObservableList<ActivitiesGroup>();
            try
            {
                QTestApiModel.TestSuiteWithCustomFieldResource testSuite = null;
                if (mappedTestSuite == null)
                {
                    testSuite = new QTestApiModel.TestSuiteWithCustomFieldResource();
                    testSuite.Name = businessFlow.Name;
                    testSuite = testsuiteApi.CreateTestSuite((long)Convert.ToInt32(ALMCore.AlmConfig.ALMProjectKey), testSuite);

                    foreach (QtestTest test in mappedTestSuite.Tests)
                    {
                        QTestApiModel.TestCaseWithCustomFieldResource testCase = new QTestApiModel.TestCaseWithCustomFieldResource();
                        testCase.Description = test.Description;
                        testCase.Name = test.TestName;
                        testCase = testcaseApi.CreateTestCase((long)Convert.ToInt32(ALMCore.AlmConfig.ALMProjectKey), testCase);
                        foreach (QtestTestStep step in test.Steps)
                        {
                            QTestApiModel.TestStepResource stepResource = new QTestApiModel.TestStepResource();
                            stepResource.Description = step.Description;
                            stepResource.Expected = step.Expected;
                            stepResource.PlainValueText = step.StepName; // (???)
                            // to extand
                            testcaseApi.AddTestStep((long)Convert.ToInt32(ALMCore.AlmConfig.ALMProjectKey), testCase.Id, stepResource);
                        }

                        QTestApiModel.TestRunWithCustomFieldResource testRun = new QTestApiModel.TestRunWithCustomFieldResource();
                        testRun.Name = businessFlow.Name;
                        testRun.TestCase = testCase;
                        testRun = testrunApi.Create((long)Convert.ToInt32(ALMCore.AlmConfig.ALMProjectKey), testRun, testSuite.Id);
                    }         
                }
                else
                {
                    //##update existing TestSuite
                    QTestApiModel.TestSuiteWithCustomFieldResource testSuiteToUpdate = testsuiteApi.GetTestSuite((long)Convert.ToInt32(ALMCore.AlmConfig.ALMProjectKey), (long)Convert.ToInt32(mappedTestSuite.ID));

                    foreach (QtestTest test in mappedTestSuite.Tests)
                    {
                        //    ActivitiesGroup ag = businessFlow.ActivitiesGroups.Where(x => (x.ExternalID == tsTest.TestId.ToString() && x.ExternalID2 == tsTest.ID.ToString())).FirstOrDefault();
                        //    if (ag == null)
                        //        testsF.RemoveItem(tsTest.ID);
                        //    else
                        //        existingActivitiesGroups.Add(ag);
                    }
                }

                //set item fields
                //foreach (ExternalItemFieldBase field in testSetFields)
                //{
                //    if (field.ToUpdate || field.Mandatory)
                //    {
                //        if (string.IsNullOrEmpty(field.SelectedValue) == false && field.SelectedValue != "NA")
                //            testSet[field.ID] = field.SelectedValue;
                //        else
                //            try { testSet[field.ID] = "NA"; }
                //            catch { }
                //    }
                //}                 

                businessFlow.ExternalID = testSuite.Id.ToString();

                //Add missing test cases
                // TSTestFactory testCasesF = testSet.TSTestFactory;
                //foreach (ActivitiesGroup ag in businessFlow.ActivitiesGroups)
                //{
                //    if (existingActivitiesGroups.Contains(ag) == false && string.IsNullOrEmpty(ag.ExternalID) == false && ImportFromQC.GetQCTest(ag.ExternalID) != null)
                //    {
                //        TSTest tsTest = testCasesF.AddItem(ag.ExternalID);
                //        if (tsTest != null)
                //        {
                //            ag.ExternalID2 = tsTest.ID;//the test case instance ID in the test set- used for exporting the execution details
                //        }
                //    }
                //    else
                //    {
                //        foreach (ActivityIdentifiers actIdent in ag.ActivitiesIdentifiers)
                //        {
                //            ExportActivityAsTestStep(ImportFromQC.GetQCTest(ag.ExternalID), (Activity)actIdent.IdentifiedActivity);
                //        }
                //    }
                //}
                return true;
            }
            catch (Exception ex)
            {
                result = "Unexpected error occurred- " + ex.Message;
                Reporter.ToLog(eLogLevel.ERROR, "Failed to export the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " to QC/ALM", ex);
                return false;
            }
        }

        public QtestTest GetQtestTest(long? testID)
        {
            testcaseApi = new QTestApi.TestcaseApi(connObj.Configuration);
            QTestApiModel.TestCaseWithCustomFieldResource testCase = testcaseApi.GetTestCase((long)Convert.ToInt32(ALMCore.AlmConfig.ALMProjectKey), testID);

            QtestTest test = new QtestTest();
            test.Description = testCase.Description;
            test.TestName = testCase.Name;
            test.TestID = testCase.Id.ToString();
            testCase.TestSteps.ForEach(z => test.Steps.Add(new QtestTestStep(z.Id.ToString(), z.Description, z.Description, z.Expected)));

            return test;
        }

        public QtestTestSuite ImportTestSetData(QtestTestSuite TS)
        {
            testrunApi = new QTestApi.TestrunApi(connObj.Configuration);
            testcaseApi = new QTestApi.TestcaseApi(connObj.Configuration);

            List<QTestApiModel.TestRunWithCustomFieldResource> testRunList = testrunApi.GetOf((long)Convert.ToInt32(ALMCore.AlmConfig.ALMProjectKey), (long)Convert.ToInt32(TS.ID), "test-suite", "descendents");
            foreach (QTestApiModel.TestRunWithCustomFieldResource testRun in testRunList)
            {
                QtestTest test = GetQtestTest(testRun.TestCase.Id);
                test.Runs = new List<QtestTestRun>();
                test.Runs.Add(new QtestTestRun(testRun.Id.ToString(), testRun.Name, testRun.Properties[0].ToString(), testRun.CreatorId.ToString()));
                TS.Tests.Add(test);
            }
          
            return TS;
        }

        public void UpdatedQCTestInBF(ref BusinessFlow businessFlow, List<QtestTest> tcsList)
        {
            ImportFromQtest.UpdatedQCTestInBF(ref businessFlow, tcsList);
        }

        public void UpdateBusinessFlow(ref BusinessFlow businessFlow, List<QtestTest> tcsList)
        {
            ImportFromQtest.UpdateBusinessFlow(ref businessFlow, tcsList);
        }

        public BusinessFlow ConvertQCTestSetToBF(QtestTestSuite TS)
        {
            return ImportFromQtest.ConvertQtestTestSuiteToBF(TS);
        }

        public QtestTestSuite GetQtestTestSuite(string testSuiteID)
        {
            QtestTestSuite testSuite = new QtestTestSuite();
            testSuite.ID = testSuiteID;
            return ImportTestSetData(testSuite);          
        }

        public override Dictionary<Guid, string> CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, List<ExternalItemFieldBase> defectsFields, bool useREST)
        {
            if (!useREST)
            {
                // do nothing
                return new Dictionary<Guid, string>();
            }
            else
            {
                return ImportFromQC.CreateNewDefectQCREST(defectsForOpening);
            }
        }
    }
}
