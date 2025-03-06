#region License
/*
Copyright © 2014-2025 European Support Limited

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
using AlmDataContractsStd.Enums;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.IO;
using Amdocs.Ginger.Repository;
using GingerCore.Activities;
using GingerCore.ALM.Qtest;
using GingerCore.Environments;
using GingerCoreNET.ALMLib;
using GingerCoreNET.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace GingerCore.ALM
{
    public class QtestCore : ALMCore
    {
        QTestAPIStdClient.ApiClient apiClient = new QTestAPIStdClient.ApiClient();
        QTestAPIStdClient.Configuration configuration = new QTestAPIStdClient.Configuration();


        QTestAPIStd.LoginApi connObj = new QTestAPIStd.LoginApi();
        QTestAPIStd.TestrunApi testrunApi = new QTestAPIStd.TestrunApi();
        QTestAPIStd.TestcaseApi testcaseApi = new QTestAPIStd.TestcaseApi();
        QTestAPIStd.FieldApi fieldApi = new QTestAPIStd.FieldApi();


        public override bool ConnectALMServer()
        {
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Connecting to qTest server");
                connObj = new QTestAPIStd.LoginApi(ALMCore.DefaultAlmConfig.ALMServerURL);
                string granttype = "password";
                string authorization = "Basic bWFoZXNoLmthbGUzQHQtbW9iaWxlLmNvbTo=";
                string accessToken = ALMCore.DefaultAlmConfig.ALMPassword;
                string tokenType = "bearer";

                if (ALMCore.DefaultAlmConfig.UseToken)
                {
                    QTestAPIStdModel.OAuthTokenStatusVM oAuthTokenStatusVM = connObj.TokenStatus(tokenType + " " + accessToken);
                    if (oAuthTokenStatusVM.ToString().ToLower().Contains("error"))
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to connect qTest Server" + System.Environment.NewLine + oAuthTokenStatusVM.ToString());
                        return false;
                    }
                }
                else
                {
                    QTestAPIStdModel.OAuthResponse response = connObj.PostAccessToken(granttype, ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, authorization);
                    accessToken = response.AccessToken;
                    tokenType = response.TokenType;
                }
                connObj.Configuration.AccessToken = accessToken;
                connObj.Configuration.ApiKey.Add("Authorization", accessToken);
                connObj.Configuration.ApiKeyPrefix.Add("Authorization", tokenType);

                string almConfigPackageFolderPathCalculated = WorkSpace.Instance.Solution.SolutionOperations.ConvertSolutionRelativePath(ALMCore.DefaultAlmConfig.ALMConfigPackageFolderPath);
                if (General.IsConfigPackageExists(almConfigPackageFolderPathCalculated, GingerCoreNET.ALMLib.ALMIntegrationEnums.eALMType.Qtest))
                {
                    connObj.Configuration.MyAPIConfig.LoadSettingsFromConfig(Path.Combine(almConfigPackageFolderPathCalculated, "QTestSettings", "QTestSettings.json"));
                }
                else
                {
                    connObj.Configuration.MyAPIConfig = new QTestAPIStdClient.QTestClientConfig();
                }
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Connecting to qTest server", ex);
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
            return [];
        }

        public override Dictionary<string, string> GetALMDomainProjects(string ALMDomain)
        {
            QTestAPIStd.ProjectApi projectsApi = new QTestAPIStd.ProjectApi(connObj.Configuration);
            List<QTestAPIStdModel.ProjectResource> projectList = projectsApi.GetProjects("descendents", true);
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

        public override ObservableList<ApplicationPlatform> ApplicationPlatforms
        {
            get { return ImportFromQtest.ApplicationPlatforms; }
            set { ImportFromQtest.ApplicationPlatforms = value; }
        }

        public override ALMIntegrationEnums.eALMType ALMType => ALMIntegrationEnums.eALMType.Qtest;

        public override ObservableList<ExternalItemFieldBase> GetALMItemFields(BackgroundWorker bw, bool online, AlmDataContractsStd.Enums.ResourceType resourceType)
        {
            ResourceType resource = resourceType;
            ConnectALMServer();
            fieldApi = new QTestAPIStd.FieldApi(connObj.Configuration);
            ObservableList<ExternalItemFieldBase> fields = [];

            if (resource == ResourceType.ALL)
            {
                fields = GetALMItemFields();
            }
            else
            {
                string fieldInRestSyntax = QtestConnect.Instance.ConvertResourceType(resource);
                List<QTestAPIStdModel.FieldResource> fieldsCollection = fieldApi.GetFields(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), fieldInRestSyntax);

                fields.Append(AddFieldsValues(fieldsCollection, resourceType.ToString()));
            }

            return UpdatedAlmFields(fields);
        }

        private ObservableList<ExternalItemFieldBase> GetALMItemFields()
        {
            ObservableList<ExternalItemFieldBase> fields = [];
            //QC   ->|testSet,     |testCase,   |designStep, |testInstance, |designStepParams, |run
            //QTest->|test-suites, |test-cases, |test-steps, |test-cycles,  |parameters,       |test-runs

            string testSetfieldInRestSyntax = QtestConnect.Instance.ConvertResourceType(ResourceType.TEST_SET);
            List<QTestAPIStdModel.FieldResource> testSetfieldsCollection = fieldApi.GetFields(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testSetfieldInRestSyntax);

            string testCasefieldInRestSyntax = QtestConnect.Instance.ConvertResourceType(ResourceType.TEST_CASE);
            List<QTestAPIStdModel.FieldResource> testCasefieldsCollection = fieldApi.GetFields(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testCasefieldInRestSyntax);

            string designStepfieldInRestSyntax = QtestConnect.Instance.ConvertResourceType(ResourceType.DESIGN_STEP);
            List<QTestAPIStdModel.FieldResource> designStepfieldsCollection = fieldApi.GetFields(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), designStepfieldInRestSyntax);

            string testInstancefieldInRestSyntax = QtestConnect.Instance.ConvertResourceType(ResourceType.TEST_CYCLE);
            List<QTestAPIStdModel.FieldResource> testInstancefieldsCollection = fieldApi.GetFields(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testInstancefieldInRestSyntax);

            string runfieldInRestSyntax = QtestConnect.Instance.ConvertResourceType(ResourceType.TEST_RUN);
            List<QTestAPIStdModel.FieldResource> runfieldsCollection = fieldApi.GetFields(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), runfieldInRestSyntax);

            fields.Append(AddFieldsValues(testSetfieldsCollection, testSetfieldInRestSyntax));
            fields.Append(AddFieldsValues(testCasefieldsCollection, testCasefieldInRestSyntax));
            fields.Append(AddFieldsValues(designStepfieldsCollection, designStepfieldInRestSyntax));
            fields.Append(AddFieldsValues(testInstancefieldsCollection, testInstancefieldInRestSyntax));
            fields.Append(AddFieldsValues(runfieldsCollection, runfieldInRestSyntax));

            return fields;
        }
        private ObservableList<ExternalItemFieldBase> AddFieldsValues(List<QTestAPIStdModel.FieldResource> testSetfieldsCollection, string testSetfieldInRestSyntax)
        {
            //TODO: need to handle duplicate fields
            ObservableList<ExternalItemFieldBase> fields = [];

            if ((testSetfieldsCollection != null) && (testSetfieldsCollection.Count > 0))
            {
                foreach (QTestAPIStdModel.FieldResource field in testSetfieldsCollection)
                {
                    if (string.IsNullOrEmpty(field.Label))
                    {
                        continue;
                    }
                    ExternalItemFieldBase itemfield = new ExternalItemFieldBase
                    {
                        ID = field.Id.ToString(),
                        ExternalID = field.OriginalName,  // Temp ??? Check if ExternalID has other use in this case
                        Name = field.Label
                    };
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
                        foreach (QTestAPIStdModel.AllowedValueResource value in field.AllowedValues)
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

        public override bool ExportExecutionDetailsToALM(BusinessFlow bizFlow, ref string result, bool exectutedFromAutomateTab = false, PublishToALMConfig publishToALMConfig = null, ProjEnvironment projEnvironment = null)
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
                testcaseApi = new QTestAPIStd.TestcaseApi(connObj.Configuration);
                testrunApi = new QTestAPIStd.TestrunApi(connObj.Configuration);
                QTestAPIStd.TestlogApi testlogApi = new QTestAPIStd.TestlogApi(connObj.Configuration);
                QTestAPIStd.AttachmentApi attachmentApi = new QTestAPIStd.AttachmentApi(connObj.Configuration);

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
                                tsTest = testSuite.Tests.FirstOrDefault(x => x.TestID == activGroup.ExternalID);
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
                                        List<QTestAPIStdModel.StatusResource> statuses = testrunApi.GetStatusValuable(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey));
                                        List<QTestAPIStdModel.StatusResource> stepsStatuses = [];
                                        QTestAPIStdModel.StatusResource testCaseStatus;

                                        QTestAPIStdModel.TestRunWithCustomFieldResource testRun = testrunApi.Get(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), Convert.ToInt32(tsTest.Runs[0].RunID), "descendents");
                                        List<QTestAPIStdModel.TestStepLogResource> testStepLogs = [];

                                        QTestAPIStdModel.TestCaseWithCustomFieldResource testCase = testcaseApi.GetTestCase(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testRun.TestCase.Id);
                                        int testStepsCount = 0;
                                        foreach (QTestAPIStdModel.TestStepResource step in testCase.TestSteps)
                                        {
                                            if (step.CalledTestCaseId != null)
                                            {
                                                QTestAPIStdModel.TestCaseWithCustomFieldResource calledTestCase = testcaseApi.GetTestCase(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), step.CalledTestCaseId);
                                                foreach (QTestAPIStdModel.TestStepResource nestedStep in calledTestCase.TestSteps)
                                                {
                                                    Activity matchingActivity = activities.FirstOrDefault(x => x.ExternalID == nestedStep.Id.ToString());
                                                    if (matchingActivity != null)
                                                    {
                                                        QTestAPIStdModel.TestStepLogResource testStepLog = new QTestAPIStdModel.TestStepLogResource(null, nestedStep.Id)
                                                        {
                                                            CalledTestCaseId = step.CalledTestCaseId,
                                                            ParentTestStepId = step.Id,
                                                            ActualResult = string.Empty
                                                        };
                                                        SetTestStepLogStatus(matchingActivity, ref testStepLog, statuses);
                                                        stepsStatuses.Add(testStepLog.Status);
                                                        testStepLogs.Add(testStepLog);
                                                        testStepsCount++;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                Activity matchingActivity = activities.FirstOrDefault(x => x.ExternalID == step.Id.ToString());
                                                if (matchingActivity != null)
                                                {
                                                    QTestAPIStdModel.TestStepLogResource testStepLog = new QTestAPIStdModel.TestStepLogResource(null, step.Id)
                                                    {
                                                        ActualResult = string.Empty
                                                    };
                                                    SetTestStepLogStatus(matchingActivity, ref testStepLog, statuses);
                                                    stepsStatuses.Add(testStepLog.Status);
                                                    testStepLogs.Add(testStepLog);
                                                    testStepsCount++;
                                                }
                                            }
                                        }

                                        //update the TC general status based on the activities status collection.                                
                                        if (stepsStatuses.Any(x => x.Name == "Failed"))
                                        {
                                            testCaseStatus = statuses.FirstOrDefault(z => z.Name == "Failed");
                                        }
                                        else if (stepsStatuses.Count(x => x.Name == "No Run") == testStepsCount || (stepsStatuses.Count(x => x.Name == "Not Applicable") + stepsStatuses.Count(x => x.Name == "Unexecuted")) == testStepsCount)
                                        {
                                            testCaseStatus = statuses.FirstOrDefault(z => z.Name == "Unexecuted");
                                        }
                                        else if (stepsStatuses.Count(x => x.Name == "Passed") == testStepsCount || (stepsStatuses.Count(x => x.Name == "Passed")) == testStepsCount)
                                        {
                                            testCaseStatus = statuses.FirstOrDefault(z => z.Name == "Passed");
                                        }
                                        else
                                        {
                                            testCaseStatus = statuses.FirstOrDefault(z => z.Name == "Unexecuted");
                                        }

                                        QTestAPIStdModel.ManualTestLogResource automationTestLog = new QTestAPIStdModel.ManualTestLogResource(null, null, DateTime.Parse(bizFlow.StartTimeStamp.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:sszz:00")), DateTime.Parse(bizFlow.EndTimeStamp.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:sszz:00")),
                                                                                                                                                null, null, tsTest.TestName + " - execution", null, null,
                                                                                                                                                null, null, null, testCaseStatus, null, testStepLogs);

                                        QTestAPIStdModel.TestLogResource testLog = testlogApi.SubmitTestLog(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), automationTestLog, Convert.ToInt32(tsTest.Runs[0].RunID));

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
                                                attachmentApi.Upload(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), "test-logs", testLog.Id, "GingerExecutionHTMLReport.zip", "application/x-zip-compressed", File.ReadAllBytes(zipFileName));
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

        public void SetTestStepLogStatus(Activity matchingActivity, ref QTestAPIStdModel.TestStepLogResource testStepLog, List<QTestAPIStdModel.StatusResource> statuses)
        {
            if (matchingActivity != null)
            {
                switch (matchingActivity.Status)
                {
                    case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed:
                        testStepLog.Status = statuses.FirstOrDefault(z => z.Name == "Failed");
                        break;
                    case Amdocs.Ginger.CoreNET.Execution.eRunStatus.NA:
                        testStepLog.Status = statuses.FirstOrDefault(z => z.Name == "Not Applicable");
                        break;
                    case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed:
                        testStepLog.Status = statuses.FirstOrDefault(z => z.Name == "Passed");
                        break;
                    case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped:
                        testStepLog.Status = statuses.FirstOrDefault(z => z.Name == "Unexecuted");
                        break;
                    case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending:
                        testStepLog.Status = statuses.FirstOrDefault(z => z.Name == "Deffered");
                        break;
                    default:
                        //Not used
                        break;
                }
            }
            else
            {
                testStepLog.Status = statuses.FirstOrDefault(z => z.Name == "Unexecuted");
            }
        }

        public bool ExportActivitiesGroupToALM(ActivitiesGroup activitiesGroup, QtestTest mappedTest, string parentObjectId, ObservableList<ExternalItemFieldBase> testCaseFields, ref string result)
        {
            ConnectALMServer();
            testcaseApi = new QTestAPIStd.TestcaseApi(connObj.Configuration);

            try
            {
                if (mappedTest == null) // Create new test case
                {
                    QTestAPIStdModel.TestCaseWithCustomFieldResource testCase = new QTestAPIStdModel.TestCaseWithCustomFieldResource(null, null, null, [])
                    {
                        Description = activitiesGroup.Description,
                        Name = activitiesGroup.Name,
                        ParentId = Convert.ToInt32(parentObjectId)
                    };
                    //create testCase
                    testCase = testcaseApi.CreateTestCase(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testCase);
                    //add testSteps
                    foreach (ActivityIdentifiers actIdent in activitiesGroup.ActivitiesIdentifiers)
                    {
                        string stepNameWithDesc = actIdent.IdentifiedActivity.ActivityName + "=>" + actIdent.IdentifiedActivity.Description;
                        QTestAPIStdModel.TestStepResource stepResource = new QTestAPIStdModel.TestStepResource(null, null,
                                                                                                            stepNameWithDesc,
                                                                                                            actIdent.IdentifiedActivity.Expected == null ? string.Empty : actIdent.IdentifiedActivity.Expected)
                        {
                            PlainValueText = actIdent.IdentifiedActivity.ActivityName
                        };
                        stepResource = testcaseApi.AddTestStep(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testCase.Id, stepResource);
                        actIdent.IdentifiedActivity.ExternalID = stepResource.Id.ToString();
                        actIdent.IdentifiedActivity.ExternalID2 = stepResource.Id.ToString();
                    }

                    //approve testCase - it needs to be called each time whenever testCase is updated
                    testcaseApi.ApproveTestCase(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testCase.Id);

                    activitiesGroup.ExternalID = testCase.Id.ToString();
                    activitiesGroup.ExternalID2 = testCase.Id.ToString();
                }
                else
                {
                    // update existing test case
                    QTestAPIStdModel.TestCaseWithCustomFieldResource testCase = testcaseApi.GetTestCase(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), Convert.ToInt32(mappedTest.TestID)); ;

                    //update testCase
                    testCase.Description = activitiesGroup.Description;
                    testCase.Name = activitiesGroup.Name;
                    testCase = testcaseApi.UpdateTestCase(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testCase.Id, testCase);

                    //update steps
                    int counter = 0;
                    foreach (ActivityIdentifiers actIdent in activitiesGroup.ActivitiesIdentifiers)
                    {
                        //get testCase each time because update step changes the ids of the steps
                        testCase = testcaseApi.GetTestCase(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), Convert.ToInt32(mappedTest.TestID));

                        string stepNameWithDesc = actIdent.IdentifiedActivity.ActivityName + "=>" + actIdent.IdentifiedActivity.Description;
                        QTestAPIStdModel.TestStepResource stepResource = new QTestAPIStdModel.TestStepResource(null, null,
                                                                                                            stepNameWithDesc,
                                                                                                            actIdent.IdentifiedActivity.Expected == null ? string.Empty : actIdent.IdentifiedActivity.Expected)
                        {
                            PlainValueText = actIdent.IdentifiedActivity.ActivityName
                        };

                        stepResource = testcaseApi.UpdateTestStep(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testCase.Id, testCase.TestSteps[counter].Id, stepResource);
                        actIdent.IdentifiedActivity.ExternalID = stepResource.Id.ToString();
                        actIdent.IdentifiedActivity.ExternalID2 = stepResource.Id.ToString();

                        counter++;
                    }

                    //approve testCase - it needs to be called each time whenever testCase is updated
                    testcaseApi.ApproveTestCase(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testCase.Id);
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
            testrunApi = new QTestAPIStd.TestrunApi(connObj.Configuration);
            testcaseApi = new QTestAPIStd.TestcaseApi(connObj.Configuration);
            QTestAPIStd.TestsuiteApi testsuiteApi = new QTestAPIStd.TestsuiteApi(connObj.Configuration);

            ObservableList<ActivitiesGroup> existingActivitiesGroups = [];
            try
            {
                QTestAPIStdModel.TestSuiteWithCustomFieldResource testSuite = null;
                if (mappedTestSuite == null)
                {
                    testSuite = new QTestAPIStdModel.TestSuiteWithCustomFieldResource(null, null, null, null,
                                                                                    businessFlow.Name,
                                                                                    []);
                    testSuite = testsuiteApi.CreateTestSuite(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testSuite, Convert.ToInt32(parentObjectId), "test-cycle");

                    foreach (ActivitiesGroup ag in businessFlow.ActivitiesGroups)
                    {
                        QTestAPIStdModel.TestRunWithCustomFieldResource testRun = new QTestAPIStdModel.TestRunWithCustomFieldResource(null, null, null, null,
                                                                                                                                ag.Name,
                                                                                                                                [],
                                                                                                                                testcaseApi.GetTestCase(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), Convert.ToInt32(ag.ExternalID)));
                        testrunApi.Create(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testRun, testSuite.Id, "test-suite");
                    }
                }
                else
                {
                    //update test-suite
                    testSuite = new QTestAPIStdModel.TestSuiteWithCustomFieldResource(null, null, null, null,
                                                                                    businessFlow.Name,
                                                                                    []);
                    testSuite = testsuiteApi.UpdateTestSuite(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), Convert.ToInt32(mappedTestSuite.ID), testSuite);


                    //##update existing TestSuite -> TestRun
                    foreach (QtestTest test in mappedTestSuite.Tests)
                    {
                        QTestAPIStdModel.TestRunWithCustomFieldResource testRun = testrunApi.Get(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), Convert.ToInt32(test.Runs[0].RunID), "descendents");
                        QTestAPIStdModel.TestCaseWithCustomFieldResource testCase = testcaseApi.GetTestCase(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testRun.TestCase.Id);

                        ActivitiesGroup ag = businessFlow.ActivitiesGroups.FirstOrDefault(x => (x.ExternalID == test.TestID.ToString()));
                        if (ag != null)
                        {
                            foreach (ActivityIdentifiers actIdent in ag.ActivitiesIdentifiers)
                            {
                                if ((actIdent.IdentifiedActivity.ExternalID == null) && (actIdent.IdentifiedActivity.ExternalID2 == null))
                                {
                                    QTestAPIStdModel.TestCaseWithCustomFieldResource newTestCase = new QTestAPIStdModel.TestCaseWithCustomFieldResource(null, null, null, [])
                                    {
                                        Description = actIdent.ActivityDescription,
                                        Name = actIdent.ActivityName
                                    };
                                    testcaseApi.CreateTestCase(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), newTestCase);
                                    string stepNameWithDesc = actIdent.IdentifiedActivity.ActivityName + "=>" + actIdent.IdentifiedActivity.Description;
                                    QTestAPIStdModel.TestStepResource stepResource = new QTestAPIStdModel.TestStepResource(null, null,
                                                                                                                            stepNameWithDesc,
                                                                                                                            actIdent.IdentifiedActivity.Expected == null ? string.Empty : actIdent.IdentifiedActivity.Expected)
                                    {
                                        PlainValueText = actIdent.IdentifiedActivity.ActivityName
                                    };
                                    testcaseApi.AddTestStep(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), newTestCase.Id, stepResource);

                                    stepResource = new QTestAPIStdModel.TestStepResource(null, null, string.Empty, string.Empty,
                                                                                        null, null, null, null, null, newTestCase.Id);
                                    stepResource = testcaseApi.AddTestStep(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testCase.Id, stepResource);

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

        public QtestTest GetQtestTest(long? testID, QTestAPIStdModel.ParameterPostQueryResponse existedParameters = null, QTestAPIStdModel.TestCaseWithCustomFieldResource testCase = null)
        {
            testcaseApi = new QTestAPIStd.TestcaseApi(connObj.Configuration);
            if (testCase == null)
            {
                testCase = testcaseApi.GetTestCase(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testID);
            }

            QtestTest test = new QtestTest
            {
                Description = testCase.Description,
                TestName = testCase.Name,
                TestID = testCase.Id.ToString()
            };
            foreach (QTestAPIStdModel.TestStepResource testStep in testCase.TestSteps)
            {
                if (testStep.CalledTestCaseId != null)
                {
                    QTestAPIStdModel.TestCaseWithCustomFieldResource calledTestCase = testcaseApi.GetTestCase(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testStep.CalledTestCaseId);
                    calledTestCase.TestSteps.ForEach(z => test.Steps.Add(new QtestTestStep(z.Id.ToString(), StripHTML(z.Description), StripHTML(z.Expected), testStep.CalledTestCaseId.ToString())));
                }
                else
                {
                    string stepName = testStep.Description;
                    string stepDesc = testStep.Description;
                    if (testStep.Description.Contains("=>"))
                    {
                        string[] activityData = testStep.Description.Split(new[] { "=>" }, StringSplitOptions.None);
                        stepName = activityData[0];
                        stepDesc = activityData[1];
                    }
                    QtestTestStep newStep = new QtestTestStep(testStep.Id.ToString(), stepDesc, testStep.Expected, null, stepName);

                    if ((testStep.ParameterValues != null) && (testStep.ParameterValues.Count > 0) && (testStep.ParameterValues[0] != null))
                    {
                        if (existedParameters != null)
                        {
                            if (testStep.RootCalledTestCaseId != null)
                            {
                                QTestAPIStdModel.ParameterModel currentParameter = existedParameters.Items.FirstOrDefault(z => z.TcIds.Contains(testStep.RootCalledTestCaseId) && z.Values.Contains(testStep.ParameterValues[0]));
                                newStep.Params = new QtestTestParameter
                                {
                                    Value = testStep.ParameterValues[0],
                                    Values = currentParameter.Values.Split(',').ToList(),
                                    Name = currentParameter.Description
                                };
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
            testrunApi = new QTestAPIStd.TestrunApi(connObj.Configuration);
            testcaseApi = new QTestAPIStd.TestcaseApi(connObj.Configuration);

            QTestAPIStdModel.ParameterPostQueryResponse existedParameters = null;
            try
            {
                QTestAPIStd.ParametersApi parametersApi = new QTestAPIStd.ParametersApi(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, connObj.Configuration);
                existedParameters = parametersApi.GetAllParameters(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey));
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
            List<QTestAPIStdModel.TestRunWithCustomFieldResource> testRunList = testrunApi.GetOf(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), Convert.ToInt32(TS.ID), "test-suite");
            foreach (QTestAPIStdModel.TestRunWithCustomFieldResource testRun in testRunList)
            {
                QTestAPIStdModel.TestRunWithCustomFieldResource testRunCurrent = testrunApi.Get(Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), testRun.Id, "testcase.teststep");
                QtestTest test = GetQtestTest(testRun.TestCase.Id, existedParameters, testRunCurrent.TestCase);
                test.Runs =
                [
                    new QtestTestRun(testRun.Id.ToString(), testRun.Name, testRun.Properties[0].ToString(), testRun.CreatorId.ToString()),
                ];
                TS.Tests.Add(test);
            }

            return TS;
        }

        public ObservableList<QTestAPIStdModel.TestCycleResource> GetQTestCyclesByProject(string qTestServerUrl, string qTestUserName, string qTestPassword, string qTestProject)
        {
            ConnectALMServer();
            ObservableList<QTestAPIStdModel.TestCycleResource> cyclestList;
            QTestAPIStd.TestcycleApi TestcycleApi = new QTestAPIStd.TestcycleApi(connObj.Configuration);
            cyclestList = new ObservableList<QTestAPIStdModel.TestCycleResource>(TestcycleApi.GetTestCycles(Convert.ToInt32(qTestProject), null, null, "descendants"));

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
            QtestTestSuite testSuite = new QtestTestSuite
            {
                ID = testSuiteID
            };
            return ImportTestSetData(testSuite);
        }

        public override Dictionary<Guid, string> CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, List<ExternalItemFieldBase> defectsFields, bool useREST)
        {
            return [];
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
                stripped = stripped.TrimStart(new[] { '\r', '\n' });
                stripped = stripped.TrimEnd(new[] { '\r', '\n' });

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
