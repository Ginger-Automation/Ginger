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
using GingerCore.Activities;
using GingerCore.ALM.QC;
using GingerCore.ALM.Qtest;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common.InterfacesLib;
using System.Linq;
using System.IO.Compression;
using Amdocs.Ginger.IO;
using System.Text.RegularExpressions;
using System.Web;
using amdocs.ginger.GingerCoreNET;

namespace GingerCore.ALM
{
    public class QtestCore : ALMCore
    {
        QTestApiClient.ApiClient apiClient = new QTestApiClient.ApiClient();
        QTestApiClient.Configuration configuration = new QTestApiClient.Configuration();


        QTestApi.LoginApi connObj = new QTestApi.LoginApi();
        QTestApi.TestrunApi testrunApi = new QTestApi.TestrunApi();
        QTestApi.TestcaseApi testcaseApi = new QTestApi.TestcaseApi();
        QTestApi.FieldApi fieldApi = new QTestApi.FieldApi();


        public override bool ConnectALMServer()
        {
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Connecting to qTest server");
                connObj = new QTestApi.LoginApi(ALMCore.DefaultAlmConfig.ALMServerURL);
                string granttype = "password";
                string authorization = "Basic bWFoZXNoLmthbGUzQHQtbW9iaWxlLmNvbTo=";
                QTestApiModel.OAuthResponse response = connObj.PostAccessToken(granttype, ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, authorization);
                connObj.Configuration.MyAPIConfig = new QTestApiClient.QTestClientConfig();
                connObj.Configuration.AccessToken = response.AccessToken;
                connObj.Configuration.ApiKey.Add("Authorization", response.AccessToken);
                connObj.Configuration.ApiKeyPrefix.Add("Authorization", response.TokenType);
                System.Diagnostics.Trace.WriteLine("Authentication Successful");
                return true;
            }           
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Connecting to qTest server",ex);
                connObj = null;
                return false;
            }
        }

        public override bool ConnectALMProject()
        {
            // ALMCore.DefaultAlmConfig.ALMProjectName = ALMCore.DefaultAlmConfig.ALMProjectKey;
            if (!string.IsNullOrEmpty(ALMCore.DefaultAlmConfig.ALMServerURL) &&
                !string.IsNullOrEmpty(ALMCore.DefaultAlmConfig.ALMUserName) &&
                !string.IsNullOrEmpty(ALMCore.DefaultAlmConfig.ALMPassword) &&
                !string.IsNullOrEmpty(ALMCore.DefaultAlmConfig.ALMProjectKey) &&
                !string.IsNullOrEmpty(ALMCore.DefaultAlmConfig.ALMProjectName))
            {
                if (ConnectALMServer())
                {
                    return true;
                }
            }
            return false;
        }

        public override Boolean IsServerConnected()
        {
            return true;
        }

        public override void DisconnectALMServer()
        {
            // not needed at Qtest
        }

        public override List<string> GetALMDomains()
        {
            return new List<string>();
        }

        public override Dictionary<string,string> GetALMDomainProjects(string ALMDomain)
        {
            QTestApi.ProjectApi projectsApi = new QTestApi.ProjectApi(connObj.Configuration);
            List<QTestApiModel.ProjectResource> projectList =  projectsApi.GetProjects("descendents", true);
            return projectList.ToDictionary(f => f.Id.ToString(), f => f.Name);
        }

        public override bool DisconnectALMProjectStayLoggedIn()
        {
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

            if (resourceType == ALM_Common.DataContracts.ResourceType.ALL)
            {
                fields = GetALMItemFields();
            }
            else
            {
                string fieldInRestSyntax = QtestConnect.Instance.ConvertResourceType(resourceType);
                List<QTestApiModel.FieldResource> fieldsCollection = fieldApi.GetFields((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), fieldInRestSyntax);

                fields.Append(AddFieldsValues(fieldsCollection, resourceType.ToString()));
            }

            return UpdatedAlmFields(fields);
        }

        private ObservableList<ExternalItemFieldBase> GetALMItemFields()
        {
            ObservableList<ExternalItemFieldBase> fields = new ObservableList<ExternalItemFieldBase>();
            //QC   ->|testSet,     |testCase,   |designStep, |testInstance, |designStepParams, |run
            //QTest->|test-suites, |test-cases, |test-steps, |test-cycles,  |parameters,       |test-runs

            string testSetfieldInRestSyntax = QtestConnect.Instance.ConvertResourceType(ALM_Common.DataContracts.ResourceType.TEST_SET);
            List<QTestApiModel.FieldResource> testSetfieldsCollection = fieldApi.GetFields((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testSetfieldInRestSyntax);

            string testCasefieldInRestSyntax = QtestConnect.Instance.ConvertResourceType(ALM_Common.DataContracts.ResourceType.TEST_CASE);
            List<QTestApiModel.FieldResource> testCasefieldsCollection = fieldApi.GetFields((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testCasefieldInRestSyntax);

            string designStepfieldInRestSyntax = QtestConnect.Instance.ConvertResourceType(ALM_Common.DataContracts.ResourceType.DESIGN_STEP);
            List<QTestApiModel.FieldResource> designStepfieldsCollection = fieldApi.GetFields((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), designStepfieldInRestSyntax);

            string testInstancefieldInRestSyntax = QtestConnect.Instance.ConvertResourceType(ALM_Common.DataContracts.ResourceType.TEST_CYCLE);
            List<QTestApiModel.FieldResource> testInstancefieldsCollection = fieldApi.GetFields((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testInstancefieldInRestSyntax);

            string runfieldInRestSyntax = QtestConnect.Instance.ConvertResourceType(ALM_Common.DataContracts.ResourceType.TEST_RUN);
            List<QTestApiModel.FieldResource> runfieldsCollection = fieldApi.GetFields((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), runfieldInRestSyntax);

            fields.Append(AddFieldsValues(testSetfieldsCollection, testSetfieldInRestSyntax));
            fields.Append(AddFieldsValues(testCasefieldsCollection, testCasefieldInRestSyntax));
            fields.Append(AddFieldsValues(designStepfieldsCollection, designStepfieldInRestSyntax));
            fields.Append(AddFieldsValues(testInstancefieldsCollection, testInstancefieldInRestSyntax));
            fields.Append(AddFieldsValues(runfieldsCollection, runfieldInRestSyntax));

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
                    if (string.IsNullOrEmpty(field.Label))
                    {
                        continue;
                    }
                    ExternalItemFieldBase itemfield = new ExternalItemFieldBase();
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
                    {
                        itemfield.ToUpdate = true;
                    }
                    itemfield.ItemType = testSetfieldInRestSyntax;
                    itemfield.Type = field.DataType;

                    if (field.AllowedValues != null)
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
                         itemfield.SelectedValue = "Unassigned";
                    }

                    fields.Add(itemfield);
                }
            }

            return fields;
        }

        public override bool ExportExecutionDetailsToALM(BusinessFlow bizFlow, ref string result, bool exectutedFromAutomateTab = false, PublishToALMConfig publishToALMConfig = null)
        {
            result = string.Empty;
            if (bizFlow.ExternalID == "0" || String.IsNullOrEmpty(bizFlow.ExternalID))
            {
                result = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ": " + bizFlow.Name + " is missing ExternalID, cannot locate QC TestSet without External ID";
                return false;
            }

            try
            {
                //get the BF matching test set
                ConnectALMServer();
                testcaseApi = new QTestApi.TestcaseApi(connObj.Configuration);
                testrunApi = new QTestApi.TestrunApi(connObj.Configuration);
                QTestApi.TestlogApi testlogApi = new QTestApi.TestlogApi(connObj.Configuration);
                QTestApi.AttachmentApi attachmentApi = new QTestApi.AttachmentApi(connObj.Configuration);

                QtestTestSuite testSuite = GetQtestTestSuite(bizFlow.ExternalID);
                if (testSuite != null)
                {
                    //get all BF Activities groups
                    ObservableList<ActivitiesGroup> activGroups = bizFlow.ActivitiesGroups;                    
                    if (activGroups.Count > 0)
                    {
                        foreach (ActivitiesGroup activGroup in activGroups)
                        {
                            if ((publishToALMConfig.FilterStatus == FilterByStatus.OnlyPassed && activGroup.RunStatus == eActivitiesGroupRunStatus.Passed)
                            || (publishToALMConfig.FilterStatus == FilterByStatus.OnlyFailed && activGroup.RunStatus == eActivitiesGroupRunStatus.Failed)
                            || publishToALMConfig.FilterStatus == FilterByStatus.All)
                            {
                                QtestTest tsTest = null;
                                //go by TC ID = TC Instance ID
                                tsTest = testSuite.Tests.Where(x => x.TestID == activGroup.ExternalID).FirstOrDefault();                              
                                if (tsTest != null)
                                {
                                    //get activities in group
                                    List<Activity> activities = (bizFlow.Activities.Where(x => x.ActivitiesGroupID == activGroup.Name)).Select(a => a).ToList();
                                    string TestCaseName = PathHelper.CleanInValidPathChars(tsTest.TestName);
                                    if ((publishToALMConfig.VariableForTCRunName == null) || (publishToALMConfig.VariableForTCRunName == string.Empty))
                                    {
                                        String timeStamp = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss");
                                        publishToALMConfig.VariableForTCRunName = "GingerRun_" + timeStamp;
                                    }

                                    
                                    if (tsTest.Runs[0] != null)
                                    {
                                        List<QTestApiModel.StatusResource> statuses = testrunApi.GetStatusValuable((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey));
                                        List<QTestApiModel.StatusResource> stepsStatuses = new List<QTestApiModel.StatusResource>();
                                        QTestApiModel.StatusResource testCaseStatus;

                                        QTestApiModel.TestRunWithCustomFieldResource testRun = testrunApi.Get((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), (long)Convert.ToInt32(tsTest.Runs[0].RunID), "descendents");
                                        List<QTestApiModel.TestStepLogResource> testStepLogs = new List<QTestApiModel.TestStepLogResource>();
                                        
                                        QTestApiModel.TestCaseWithCustomFieldResource testCase = testcaseApi.GetTestCase((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testRun.TestCase.Id);
                                        int testStepsCount = 0;
                                        foreach (QTestApiModel.TestStepResource step in testCase.TestSteps)
                                        {
                                            if (step.CalledTestCaseId != null)
                                            {
                                                QTestApiModel.TestCaseWithCustomFieldResource calledTestCase = testcaseApi.GetTestCase((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), step.CalledTestCaseId);
                                                foreach (QTestApiModel.TestStepResource nestedStep in calledTestCase.TestSteps)
                                                {
                                                    Activity matchingActivity = activities.Where(x => x.ExternalID == nestedStep.Id.ToString()).FirstOrDefault();
                                                    if (matchingActivity != null)
                                                    {
                                                        QTestApiModel.TestStepLogResource testStepLog = new QTestApiModel.TestStepLogResource(null, nestedStep.Id);
                                                        testStepLog.CalledTestCaseId = step.CalledTestCaseId;
                                                        testStepLog.ParentTestStepId = step.Id;
                                                        testStepLog.ActualResult = string.Empty;
                                                        SetTestStepLogStatus(matchingActivity, ref testStepLog, statuses);
                                                        stepsStatuses.Add(testStepLog.Status);
                                                        testStepLogs.Add(testStepLog);
                                                        testStepsCount++;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                Activity matchingActivity = activities.Where(x => x.ExternalID == step.Id.ToString()).FirstOrDefault();
                                                if (matchingActivity != null)
                                                {
                                                    QTestApiModel.TestStepLogResource testStepLog = new QTestApiModel.TestStepLogResource(null, step.Id);
                                                    testStepLog.ActualResult = string.Empty;
                                                    SetTestStepLogStatus(matchingActivity, ref testStepLog, statuses);
                                                    stepsStatuses.Add(testStepLog.Status);
                                                    testStepLogs.Add(testStepLog);
                                                    testStepsCount++;
                                                }
                                            }
                                        }

                                        //update the TC general status based on the activities status collection.                                
                                        if (stepsStatuses.Where(x => x.Name == "Failed").Count() > 0)
                                        {
                                            testCaseStatus = statuses.Where(z => z.Name == "Failed").FirstOrDefault();
                                        }
                                        else if (stepsStatuses.Where(x => x.Name == "No Run").Count() == testStepsCount || stepsStatuses.Where(x => x.Name == "Not Applicable").Count() == testStepsCount)
                                        {
                                            testCaseStatus = statuses.Where(z => z.Name == "Unexecuted").FirstOrDefault();
                                        }
                                        else if (stepsStatuses.Where(x => x.Name == "Passed").Count() == testStepsCount || (stepsStatuses.Where(x => x.Name == "Passed").Count() + stepsStatuses.Where(x => x.Name == "Not Applicable").Count()) == testStepsCount)
                                        {
                                            testCaseStatus = statuses.Where(z => z.Name == "Passed").FirstOrDefault();
                                        }
                                        else
                                        {
                                            testCaseStatus = statuses.Where(z => z.Name == "Unexecuted").FirstOrDefault();
                                        }

                                        QTestApiModel.ManualTestLogResource automationTestLog = new QTestApiModel.ManualTestLogResource(null, null, DateTime.Parse(bizFlow.StartTimeStamp.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:sszz:00")), DateTime.Parse(bizFlow.EndTimeStamp.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:sszz:00")),
                                                                                                                                                null, null, tsTest.TestName + " - execution", null, null,
                                                                                                                                                null, null, null, testCaseStatus, null, testStepLogs);

                                        QTestApiModel.TestLogResource testLog = testlogApi.SubmitTestLog((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), automationTestLog, (long)Convert.ToInt32(tsTest.Runs[0].RunID));

                                        // Attach ActivityGroup Report if needed                                       
                                        if (publishToALMConfig.ToAttachActivitiesGroupReport)
                                        {
                                            if ((activGroup.TempReportFolder != null) && (activGroup.TempReportFolder != string.Empty) &&
                                                (System.IO.Directory.Exists(activGroup.TempReportFolder)))
                                            {
                                                //Creating the Zip file - start
                                                string targetZipPath = System.IO.Directory.GetParent(activGroup.TempReportFolder).ToString();
                                                string zipFileName = targetZipPath + "\\" + TestCaseName + "_GingerHTMLReport.zip";

                                                if (!System.IO.File.Exists(zipFileName))
                                                {
                                                    ZipFile.CreateFromDirectory(activGroup.TempReportFolder, zipFileName);
                                                }
                                                else
                                                {
                                                    System.IO.File.Delete(zipFileName);
                                                    ZipFile.CreateFromDirectory(activGroup.TempReportFolder, zipFileName);
                                                }
                                                System.IO.Directory.Delete(activGroup.TempReportFolder, true);
                                                // to discuss an issue
                                                attachmentApi.Upload((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), "test-logs", testLog.Id, "GingerExecutionHTMLReport.zip", "application/x-zip-compressed", File.ReadAllBytes(zipFileName));
                                                System.IO.File.Delete(zipFileName);
                                            }
                                        }
                                    }                                   
                                }
                                else
                                {
                                    //No matching TC was found for the ActivitiesGroup in QC
                                    result = "Matching TC's were not found for all " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " in qTest.";
                                }
                            }
                        }
                    }
                    else
                    {
                        //No matching Test Set was found for the BF in QC
                        result = "No matching Test Set was found in qTest.";
                    }

                }
                if (result == string.Empty)
                {
                    result = "Export performed successfully.";
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                result = "Unexpected error occurred- " + ex.Message;
                Reporter.ToLog(eLogLevel.ERROR, "Failed to export execution details to Qtest", ex);               
                return false;
            }
        }

        public void SetTestStepLogStatus(Activity matchingActivity, ref QTestApiModel.TestStepLogResource testStepLog, List<QTestApiModel.StatusResource> statuses)
        {
            if (matchingActivity != null)
            {
                switch (matchingActivity.Status)
                {
                    case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed:
                        testStepLog.Status = statuses.Where(z => z.Name == "Failed").FirstOrDefault();
                        break;
                    case Amdocs.Ginger.CoreNET.Execution.eRunStatus.NA:
                        testStepLog.Status = statuses.Where(z => z.Name == "Not Applicable").FirstOrDefault();
                        break;
                    case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed:
                        testStepLog.Status = statuses.Where(z => z.Name == "Passed").FirstOrDefault();
                        break;
                    case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped:
                        testStepLog.Status = statuses.Where(z => z.Name == "Unexecuted").FirstOrDefault();
                        break;
                    case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending:
                        testStepLog.Status = statuses.Where(z => z.Name == "Deffered").FirstOrDefault();
                        break;
                    default:
                        //Not used
                        break;
                }
            }
            else
            {
                testStepLog.Status = statuses.Where(z => z.Name == "Unexecuted").FirstOrDefault();
            }
        }

        public bool ExportActivitiesGroupToALM(ActivitiesGroup activitiesGroup, QtestTest mappedTest, string parentObjectId, ObservableList<ExternalItemFieldBase> testCaseFields, ref string result)
        {
            ConnectALMServer();
            testcaseApi = new QTestApi.TestcaseApi(connObj.Configuration);

            try
            {
                if (mappedTest == null) // Create new test case
                {
                    QTestApiModel.TestCaseWithCustomFieldResource testCase = new QTestApiModel.TestCaseWithCustomFieldResource(null, null, null, new List<QTestApiModel.PropertyResource>());
                    testCase.Description = activitiesGroup.Description;
                    testCase.Name = activitiesGroup.Name;
                    testCase = testcaseApi.CreateTestCase((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testCase);
                    foreach (ActivityIdentifiers actIdent in activitiesGroup.ActivitiesIdentifiers)
                    {
                        QTestApiModel.TestStepResource stepResource = new QTestApiModel.TestStepResource(   null, null, 
                                                                                                            ((Activity)actIdent.IdentifiedActivity).Description == null ? string.Empty : ((Activity)actIdent.IdentifiedActivity).Description,
                                                                                                            ((Activity)actIdent.IdentifiedActivity).Expected == null ? string.Empty : ((Activity)actIdent.IdentifiedActivity).Expected);
                        stepResource.PlainValueText = ((Activity)actIdent.IdentifiedActivity).ActivityName;
                        stepResource = testcaseApi.AddTestStep((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testCase.Id, stepResource);
                        ((Activity)actIdent.IdentifiedActivity).ExternalID = stepResource.Id.ToString();
                        ((Activity)actIdent.IdentifiedActivity).ExternalID2 = stepResource.Id.ToString();
                    }
                    activitiesGroup.ExternalID = testCase.Id.ToString();
                    activitiesGroup.ExternalID2 = testCase.Id.ToString();
                }
                else 
                {
                    // update existing test case
                }

                return true;
            }
            catch (Exception ex)
            {
                result = "Unexpected error occurred- " + ex.Message;
                Reporter.ToLog(eLogLevel.ERROR, "Failed to export the " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " to qTest", ex);
                return false;
            }
        }

        public bool ExportBusinessFlowToALM(BusinessFlow businessFlow, QtestTestSuite mappedTestSuite, string parentObjectId, ObservableList<ExternalItemFieldBase> testSetFields, ref string result)
        {
            ConnectALMServer();
            testrunApi = new QTestApi.TestrunApi(connObj.Configuration);
            testcaseApi = new QTestApi.TestcaseApi(connObj.Configuration);
            QTestApi.TestsuiteApi testsuiteApi = new QTestApi.TestsuiteApi(connObj.Configuration);

            ObservableList<ActivitiesGroup> existingActivitiesGroups = new ObservableList<ActivitiesGroup>();
            try
            {
                QTestApiModel.TestSuiteWithCustomFieldResource testSuite = null;
                if (mappedTestSuite == null)
                {
                    testSuite = new QTestApiModel.TestSuiteWithCustomFieldResource(null, null, null, null,
                                                                                    businessFlow.Name,
                                                                                    new List<QTestApiModel.PropertyResource>());
                    testSuite = testsuiteApi.CreateTestSuite((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testSuite, (long)Convert.ToInt32(parentObjectId), "test-cycle");

                    foreach (ActivitiesGroup ag in businessFlow.ActivitiesGroups)
                    {
                        QTestApiModel.TestRunWithCustomFieldResource testRun = new QTestApiModel.TestRunWithCustomFieldResource(null, null, null, null,
                                                                                                                                ag.Name,
                                                                                                                                new List<QTestApiModel.PropertyResource>(),
                                                                                                                                testcaseApi.GetTestCase((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), (long)Convert.ToInt32(ag.ExternalID)));
                        testrunApi.Create((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testRun, testSuite.Id, "test-suite");
                    }
                }
                else
                {
                    //##update existing TestSuite -> TestRun
                    foreach (QtestTest test in mappedTestSuite.Tests)
                    {
                        QTestApiModel.TestRunWithCustomFieldResource testRun = testrunApi.Get((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), (long)Convert.ToInt32(test.Runs[0].RunID), "descendents");
                        QTestApiModel.TestCaseWithCustomFieldResource testCase = testcaseApi.GetTestCase((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testRun.TestCase.Id);

                        ActivitiesGroup ag = businessFlow.ActivitiesGroups.Where(x => (x.ExternalID == test.TestID.ToString())).FirstOrDefault();
                        if (ag != null)
                        {
                            foreach (ActivityIdentifiers actIdent in ag.ActivitiesIdentifiers)
                            {
                                if ((actIdent.IdentifiedActivity.ExternalID == null) && (actIdent.IdentifiedActivity.ExternalID2 == null))
                                {
                                    QTestApiModel.TestCaseWithCustomFieldResource newTestCase = new QTestApiModel.TestCaseWithCustomFieldResource(null, null, null, new List<QTestApiModel.PropertyResource>());
                                    newTestCase.Description = actIdent.ActivityDescription;
                                    newTestCase.Name = actIdent.ActivityName;
                                    testcaseApi.CreateTestCase((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), newTestCase);
                                    QTestApiModel.TestStepResource stepResource = new QTestApiModel.TestStepResource(null, null,
                                                                                                                            ((Activity)actIdent.IdentifiedActivity).Description == null ? string.Empty : ((Activity)actIdent.IdentifiedActivity).Description,
                                                                                                                            ((Activity)actIdent.IdentifiedActivity).Expected == null ? string.Empty : ((Activity)actIdent.IdentifiedActivity).Expected);
                                    stepResource.PlainValueText = ((Activity)actIdent.IdentifiedActivity).ActivityName;
                                    testcaseApi.AddTestStep((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), newTestCase.Id, stepResource);

                                    stepResource = new QTestApiModel.TestStepResource(  null, null, string.Empty, string.Empty,
                                                                                        null, null, null, null, null, newTestCase.Id);
                                    stepResource = testcaseApi.AddTestStep((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testCase.Id, stepResource);

                                    actIdent.IdentifiedActivity.ExternalID = newTestCase.Id.ToString();
                                    actIdent.IdentifiedActivity.ExternalID2 = stepResource.Id.ToString();
                                }
                            }
                        }
                    }
                }

                businessFlow.ExternalID = testSuite != null ? testSuite.Id.ToString() : string.Empty;
                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(businessFlow);
                return true;
            }
            catch (Exception ex)
            {
                result = "Unexpected error occurred- " + ex.Message;
                Reporter.ToLog(eLogLevel.ERROR, "Failed to export the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " to qTest", ex);
                return false;
            }
        }

        public QtestTest GetQtestTest(long? testID, QTestApiModel.ParameterPostQueryResponse existedParameters = null, QTestApiModel.TestCaseWithCustomFieldResource testCase = null)
        {
            testcaseApi = new QTestApi.TestcaseApi(connObj.Configuration);
            if (testCase == null)
            {
                testCase = testcaseApi.GetTestCase((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testID);
            }

            QtestTest test = new QtestTest();
            test.Description = testCase.Description;
            test.TestName = testCase.Name;
            test.TestID = testCase.Id.ToString();
            foreach (QTestApiModel.TestStepResource testStep in testCase.TestSteps)
            {
                if (testStep.CalledTestCaseId != null)
                {
                    QTestApiModel.TestCaseWithCustomFieldResource calledTestCase = testcaseApi.GetTestCase((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testStep.CalledTestCaseId);
                    calledTestCase.TestSteps.ForEach(z => test.Steps.Add(new QtestTestStep(z.Id.ToString(), StripHTML(z.Description), StripHTML(z.Expected), testStep.CalledTestCaseId.ToString())));
                }
                else
                {
                    QtestTestStep newStep = new QtestTestStep(testStep.Id.ToString(), testStep.Description, testStep.Expected);
                    if ((testStep.ParameterValues != null) && (testStep.ParameterValues.Count > 0) && (testStep.ParameterValues[0] != null))
                    {
                        if (existedParameters != null)
                        {
                            if (testStep.RootCalledTestCaseId != null)
                            {
                                QTestApiModel.ParameterModel currentParameter = existedParameters.Items.Where(z => z.TcIds.Contains(testStep.RootCalledTestCaseId) && z.Values.Contains(testStep.ParameterValues[0])).FirstOrDefault();
                                newStep.Params = new QtestTestParameter();
                                newStep.Params.Value = testStep.ParameterValues[0];
                                newStep.Params.Values = currentParameter.Values.Split(',').ToList();
                                newStep.Params.Name = currentParameter.Description;
                            }
                        }
                    }
                    test.Steps.Add(newStep);
                }
            }
              
            return test;
        }

        public QtestTestSuite ImportTestSetData(QtestTestSuite TS)
        {
            ConnectALMServer();
            testrunApi = new QTestApi.TestrunApi(connObj.Configuration);
            testcaseApi = new QTestApi.TestcaseApi(connObj.Configuration);

            QTestApi.ParametersApi parametersApi = new QTestApi.ParametersApi(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, connObj.Configuration);
            QTestApiModel.ParameterPostQueryResponse existedParameters = parametersApi.GetAllParameters((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey));

            List<QTestApiModel.TestRunWithCustomFieldResource> testRunList = testrunApi.GetOf((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), (long)Convert.ToInt32(TS.ID), "test-suite");
            foreach (QTestApiModel.TestRunWithCustomFieldResource testRun in testRunList)
            {
                QTestApiModel.TestRunWithCustomFieldResource testRunCurrent = testrunApi.Get((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testRun.Id, "testcase.teststep");
                QtestTest test = GetQtestTest(testRun.TestCase.Id, existedParameters, testRunCurrent.TestCase);
                test.Runs = new List<QtestTestRun>();
                test.Runs.Add(new QtestTestRun(testRun.Id.ToString(), testRun.Name, testRun.Properties[0].ToString(), testRun.CreatorId.ToString()));
                TS.Tests.Add(test);
            }
          
            return TS;
        }

        public ObservableList<QTestApiModel.TestCycleResource> GetQTestCyclesByProject(string qTestServerUrl, string qTestUserName, string qTestPassword, string qTestProject)
        {
            ConnectALMServer();
            ObservableList<QTestApiModel.TestCycleResource> cyclestList;
            QTestApi.TestcycleApi TestcycleApi = new QTestApi.TestcycleApi(connObj.Configuration);
            cyclestList = new ObservableList<QTestApiModel.TestCycleResource>(TestcycleApi.GetTestCycles((long)Convert.ToInt32(qTestProject), null, null, "descendants"));

            return cyclestList;
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
            return new Dictionary<Guid, string>();     
        }

        public static string StripHTML(string HTMLText, bool toDecodeHTML = true)
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
                stripped = stripped.TrimStart(new [] { '\r', '\n' });
                stripped = stripped.TrimEnd(new [] { '\r', '\n' });

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
