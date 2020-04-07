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
        QTestApi.FieldApi fieldApi = new QTestApi.FieldApi();

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
                connObj.Configuration.MyAPIConfig = new QTestApiClient.QTestClientConfig();
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
            ConnectALMServer();
            fieldApi = new QTestApi.FieldApi(connObj.Configuration);
            ObservableList<ExternalItemFieldBase> fields = new ObservableList<ExternalItemFieldBase>();

            //List<QTestApiModel.FieldResource> fieldsCollection = fieldApi.GetFields((long)Convert.ToInt32(ALMCore.AlmConfig.ALMProjectKey), "test-suites");

            if (resourceType == ALM_Common.DataContracts.ResourceType.ALL)
                return GetALMItemFields();
            else
            {
                //string fieldInRestSyntax = QCRestAPIConnect.ConvertResourceType(resourceType);
                List<QTestApiModel.FieldResource> fieldsCollection = fieldApi.GetFields((long)Convert.ToInt32(ALMCore.AlmConfig.ALMProjectKey), resourceType.ToString());

                fields.Append(AddFieldsValues(fieldsCollection, resourceType.ToString()));
            }

            return UpdatedAlmFields(fields);
            //return UpdatedAlmFields(ImportFromQtest.GetALMItemFields(resourceType));
            //return null;
        }

        private ObservableList<ExternalItemFieldBase> GetALMItemFields()
        {
            ObservableList<ExternalItemFieldBase> fields = new ObservableList<ExternalItemFieldBase>();
            fieldApi = new QTestApi.FieldApi(connObj.Configuration);

            //QC   ->|testSet,     |testCase,   |designStep, |testInstance, |designStepParams, |run
            //QTest->|test-suites, |test-cases, |test-steps, |test-cycles,  |parameters,       |test-runs

            //string testSetfieldInRestSyntax = QCRestAPIConnect.ConvertResourceType(ALM_Common.DataContracts.ResourceType.TEST_SET);
            List<QTestApiModel.FieldResource> testSetfieldsCollection = fieldApi.GetFields((long)Convert.ToInt32(ALMCore.AlmConfig.ALMProjectKey), "test-suites");

            //string testCasefieldInRestSyntax = QCRestAPIConnect.ConvertResourceType(ALM_Common.DataContracts.ResourceType.TEST_CASE);
            List<QTestApiModel.FieldResource> testCasefieldsCollection = fieldApi.GetFields((long)Convert.ToInt32(ALMCore.AlmConfig.ALMProjectKey), "test-cases");

            ////string designStepfieldInRestSyntax = QCRestAPIConnect.ConvertResourceType(ALM_Common.DataContracts.ResourceType.DESIGN_STEP);
            List<QTestApiModel.FieldResource> designStepfieldsCollection = fieldApi.GetFields((long)Convert.ToInt32(ALMCore.AlmConfig.ALMProjectKey), "test-steps");

            ////string testInstancefieldInRestSyntax = QCRestAPIConnect.ConvertResourceType(ALM_Common.DataContracts.ResourceType.TEST_CYCLE);
            List<QTestApiModel.FieldResource> testInstancefieldsCollection = fieldApi.GetFields((long)Convert.ToInt32(ALMCore.AlmConfig.ALMProjectKey), "test-cycles");

            //string designStepParamsfieldInRestSyntax = QCRestAPIConnect.ConvertResourceType(ALM_Common.DataContracts.ResourceType.DESIGN_STEP_PARAMETERS);
            //List<QTestApiModel.FieldResource> designStepParamsfieldsCollection = fieldApi.GetFields((long)Convert.ToInt32(ALMCore.AlmConfig.ALMProjectKey), "parameters");

            //string runfieldInRestSyntax = QCRestAPIConnect.ConvertResourceType(ALM_Common.DataContracts.ResourceType.TEST_RUN);
            List<QTestApiModel.FieldResource> runfieldsCollection = fieldApi.GetFields((long)Convert.ToInt32(ALMCore.AlmConfig.ALMProjectKey), "test-runs");

            fields.Append(AddFieldsValues(testSetfieldsCollection, "test-suites"));
            fields.Append(AddFieldsValues(testCasefieldsCollection, "test-cases"));
            fields.Append(AddFieldsValues(designStepfieldsCollection, "test-steps"));
            fields.Append(AddFieldsValues(testInstancefieldsCollection, "test-cycles"));
            //fields.Append(AddFieldsValues(designStepParamsfieldsCollection, "parameters"));
            fields.Append(AddFieldsValues(runfieldsCollection, "test-runs"));

            return fields;
        }
        private  ObservableList<ExternalItemFieldBase> AddFieldsValues(List<QTestApiModel.FieldResource> testSetfieldsCollection, string testSetfieldInRestSyntax)
        {
            //TODO: need to handle duplicate fields
            ObservableList<ExternalItemFieldBase> fields = new ObservableList<ExternalItemFieldBase>();

            if ((testSetfieldsCollection != null) && (testSetfieldsCollection.Count > 0))
            {
                foreach (QTestApiModel.FieldResource field in testSetfieldsCollection)
                {
                    if (string.IsNullOrEmpty(field.Label)) continue;

                    ExternalItemFieldBase itemfield = new ExternalItemFieldBase();
                    //itemfield.ID = field.OriginalName;
                    itemfield.ID = field.Id.ToString();
                    itemfield.ExternalID = field.OriginalName;  // Temp ??? Check if ExternalID has other use in this case
                    itemfield.Name = field.Label;
                    bool isMandatory;
                    bool.TryParse(field.Required.ToString(), out isMandatory);
                    itemfield.Mandatory = isMandatory;
                    bool isSystemField;
                    bool.TryParse(field.SystemField.ToString(), out isSystemField);
                    itemfield.SystemFieled = isSystemField;
                    if (itemfield.Mandatory)
                        itemfield.ToUpdate = true;
                    itemfield.ItemType = testSetfieldInRestSyntax.ToString();
                    itemfield.Type = field.DataType;

                    if ((field.AllowedValues != null))
                    {
                        foreach (QTestApiModel.AllowedValueResource value in field.AllowedValues)
                        {
                            itemfield.PossibleValues.Add(value.Label);
                        }
                    }

                    if (itemfield.PossibleValues.Count > 0)
                    {
                        if (field.DefaultValue != null)
                        {
                            itemfield.SelectedValue = field.DefaultValue;
                        }
                        else
                        {
                            itemfield.SelectedValue = itemfield.PossibleValues[0];
                        }
                    }
                    else
                    {
                        // itemfield.SelectedValue = "NA";
                    }

                    fields.Add(itemfield);
                }
            }

            return fields;
        }
        public override bool ExportExecutionDetailsToALM(BusinessFlow bizFlow, ref string result, bool exectutedFromAutomateTab = false, PublishToALMConfig publishToALMConfig = null)
        {
            // return ExportToQC.ExportExecutionDetailsToQC(bizFlow, ref result, publishToALMConfig);
            return true;
        }

        public bool ExportActivitiesGroupToALM(ActivitiesGroup activitiesGroup, Test mappedTest, string uploadPath, ObservableList<ExternalItemFieldBase> testCaseFields, ref string result)
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
                }
                else
                {
                    //##update existing TestSuite
                    QTestApiModel.TestSuiteWithCustomFieldResource testSuiteToUpdate = testsuiteApi.GetTestSuite((long)Convert.ToInt32(ALMCore.AlmConfig.ALMProjectKey), (long)Convert.ToInt32(mappedTestSuite.ID));

                    //foreach (TSTest tsTest in tsTestsList)
                    //{
                    //    ActivitiesGroup ag = businessFlow.ActivitiesGroups.Where(x => (x.ExternalID == tsTest.TestId.ToString() && x.ExternalID2 == tsTest.ID.ToString())).FirstOrDefault();
                    //    if (ag == null)
                    //        testsF.RemoveItem(tsTest.ID);
                    //    else
                    //        existingActivitiesGroups.Add(ag);
                    //}
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

        public QtestTestSuite ImportTestSetData(QtestTestSuite TS)
        {
            testrunApi = new QTestApi.TestrunApi(connObj.Configuration);
            testcaseApi = new QTestApi.TestcaseApi(connObj.Configuration);

            List<QTestApiModel.TestRunWithCustomFieldResource> testRunList = testrunApi.GetOf((long)Convert.ToInt32(ALMCore.AlmConfig.ALMProjectKey), (long)Convert.ToInt32(TS.ID), "test-suite", "descendents");
            foreach (QTestApiModel.TestRunWithCustomFieldResource testRun in testRunList)
            {
                QTestApiModel.TestCaseWithCustomFieldResource testCase = testcaseApi.GetTestCase((long)Convert.ToInt32(ALMCore.AlmConfig.ALMProjectKey), testRun.TestCase.Id);
                
                QtestTest test = new QtestTest();
                test.Description = testCase.Description;
                test.TestName = testCase.Name;
                test.TestID = testCase.Id.ToString();
                testCase.TestSteps.ForEach(z => test.Steps.Add(new QtestTestStep(z.Id.ToString(), z.Description, z.Description, z.Expected)));
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

        public Test GetQCTest(string testID)
        {
            // return ImportFromQtest.GetQtestTest(testID);
            return null;
        }

        public QtestTestSuite GetQCTestSet(string testSetID)
        {
            // return ImportFromQtest.GetQCTestSet(testSetID);
            return new QtestTestSuite();
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
