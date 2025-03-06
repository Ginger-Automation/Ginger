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

using ACL_Data_Contract;
using ACL_Data_Contract.Abstraction;
using ALM_CommonStd.DataContracts;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.IO;
using GingerCore.Activities;
using GingerCore.Environments;
using GingerCoreNET.GeneralLib;
using Newtonsoft.Json;
using RQMExportStd.ExportBLL;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using ExternalItemFieldBase = Amdocs.Ginger.Repository.ExternalItemFieldBase;
//using AlmDataContractsStd.Contracts;

namespace GingerCore.ALM.RQM
{
    public class ExportToRQM
    {
        ObservableList<ExternalItemFieldBase> mExternalItemsFields = [];

        private ExportToRQM()
        {
        }

        #region singlton
        private static readonly ExportToRQM _instance = new ExportToRQM();
        public static ExportToRQM Instance
        {
            get
            {
                return _instance;
            }
        }
        #endregion

        RQMProjectListConfiguration RQMProjectListConfig;
        XmlReader reader;
        private static readonly object importFileLock = new object();
        static List<string> valuelist = [];
        private void GetRQMProjectListConfiguration()
        {
            try
            {
                lock (importFileLock)
                {
                    Thread.Sleep(500);
                    if (RQMProjectListConfig != null)
                    {
                        return;
                    }
                    string importConfigTemplate = System.IO.Path.Combine(RQMCore.ConfigPackageFolderPath, "RQM_Import", "RQM_ImportConfigs_Template.xml");
                    if (File.Exists(importConfigTemplate))
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(RQMProjectListConfiguration));
                        FileStream fs = new FileStream(importConfigTemplate, FileMode.Open, FileAccess.Read, FileShare.Read);
                        try
                        {
                            reader = XmlReader.Create(fs);
                            RQMProjectListConfig = (RQMProjectListConfiguration)serializer.Deserialize(reader);
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.DEBUG, "Failed To Load RQM_ImportConfigs_Template.xml", ex);
                        }
                        finally
                        {
                            fs.Close();
                            fs.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Failed To Load RQM_ImportConfigs_Template.xml", ex);
            }
        }

        RQMTestPlan testPlan;
        public bool ExportExecutionDetailsToRQM(BusinessFlow businessFlow, ref string result, bool exectutedFromAutomateTab = false, PublishToALMConfig publishToALMConfig = null, ProjEnvironment projEnvironment = null)
        {
            var originalExternalFields = General.GetExternalFields();

            if (!originalExternalFields.Any(x => x.ItemType == "TestCase"))
            {
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Current solution have no predefined values for RQM's mandatory fields. Please configure before doing export. ('ALM'-'ALM Items Fields Configuration')");
                return false;
            }
            List<ACL_Data_Contract.ExternalItemFieldBase> ExternalFields = ConvertExternalFieldsToACLDataContractfields(originalExternalFields);
            result = string.Empty;
            string bfExportedID = GetExportedIDString(businessFlow.ExternalIdCalCulated, "RQMID");
            if (string.IsNullOrEmpty(bfExportedID) || bfExportedID.Equals("0"))
            {
                if (businessFlow.ALMTestSetLevel == "RunSet")
                {
                    result = $"{GingerDicser.GetTermResValue(eTermResKey.RunSet)}: {businessFlow.Name} is missing ExternalID, cannot export RQM TestPlan execution results without External ID";
                }
                else
                {
                    result = $"{GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)}: {businessFlow.Name} is missing ExternalID, cannot export RQM TestPlan execution results without External ID";
                }

                return false;
            }
            if (businessFlow.ActivitiesGroups.Count == 0)
            {
                if (businessFlow.ALMTestSetLevel == "RunSet")
                {
                    result = $"{GingerDicser.GetTermResValue(eTermResKey.RunSet)}: {businessFlow.Name} Must have at least one {GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)}";
                }
                else
                {
                    result = $"{GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)}: {businessFlow.Name} Must have at least one {GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)}";
                }

                return false;
            }
            LoginDTO loginData = new LoginDTO() { User = ALMCore.DefaultAlmConfig.ALMUserName, Password = ALMCore.DefaultAlmConfig.ALMPassword, Server = ALMCore.DefaultAlmConfig.ALMServerURL };

            // get data about execution records per current test plan - start
            GetRQMProjectListConfiguration();
            if (RQMProjectListConfig != null)
            {
                RQMProject currentRQMProjectMapping;
                if (RQMProjectListConfig.RQMProjects.Count > 0)
                {
                    currentRQMProjectMapping = RQMProjectListConfig.RQMProjects.FirstOrDefault(x => x.Name == ALMCore.DefaultAlmConfig.ALMProjectName || x.Name == "DefaultProjectName");
                    if (currentRQMProjectMapping != null)
                    {
                        if (testPlan == null || testPlan.RQMID != bfExportedID)
                        {
                            testPlan = RQMConnect.Instance.GetRQMTestPlanByIdByProject(ALMCore.DefaultAlmConfig.ALMServerURL, ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMProjectName, bfExportedID);
                            if (testPlan == null)
                            {
                                result = $"Testing Plan does not found with id {bfExportedID} in connected RQM project.";
                                return false;
                            }
                        }
                        // Commented code to get  execution records by test plan as it becomes slow if there are more records on test plan.
                        // Changed it to get records by test case in GetExeResultforAg() function 
                        //if (!string.IsNullOrEmpty(testPlan.URLPathVersioned))
                        //{
                        //    testPlan.RQMExecutionRecords = RQMConnect.Instance.GetExecutionRecordsByTestPlan(loginData, reader, currentRQMProjectMapping, RQMCore.ALMProjectGroupName, RQMCore.ALMProjectGuid, testPlan.URLPathVersioned);
                        //}
                        //else
                        //{
                        //    Reporter.ToLog(eLogLevel.ERROR, $"Execution Test Plan not found.");
                        //}

                        List<ExecutionResult> exeResultList = [];
                        bool isFlowskipped = false;
                        foreach (ActivitiesGroup activGroup in businessFlow.ActivitiesGroups)
                        {

                            if (ALMCore.DefaultAlmConfig.PublishSkipped.Equals("False", StringComparison.CurrentCultureIgnoreCase))
                            {
                                if (activGroup.RunStatus == eActivitiesGroupRunStatus.Skipped)
                                {
                                    isFlowskipped = true;
                                    continue;
                                }
                            }
                            if (activGroup.ActivitiesIdentifiers.Count > 0)
                            {
                                if (projEnvironment != null)
                                {
                                    IValueExpression mAGVE = new GingerCore.ValueExpression(projEnvironment, businessFlow, [], false, "", false);
                                    activGroup.CalculateExternalId(mAGVE);
                                }
                                if ((publishToALMConfig.FilterStatus == FilterByStatus.OnlyPassed && activGroup.RunStatus == eActivitiesGroupRunStatus.Passed)
                                    || (publishToALMConfig.FilterStatus == FilterByStatus.OnlyFailed && activGroup.RunStatus == eActivitiesGroupRunStatus.Failed)
                                    || publishToALMConfig.FilterStatus == FilterByStatus.All)
                                {
                                    testPlan.Name = !string.IsNullOrEmpty(publishToALMConfig.VariableForTCRunNameCalculated) ? publishToALMConfig.VariableForTCRunNameCalculated : testPlan.Name;
                                    ExecutionResult exeResult = GetExeResultforActivityGroup(businessFlow, bfExportedID, activGroup, ref result, testPlan, currentRQMProjectMapping, loginData, publishToALMConfig, ExternalFields);
                                    if (exeResult != null)
                                    {
                                        exeResultList.Add(exeResult);
                                    }
                                    else
                                    {
                                        result += $" {Environment.NewLine} Execution Result not created for {businessFlow.Name} and testplan {bfExportedID}";
                                        Reporter.ToLog(eLogLevel.DEBUG, result);
                                        //return false;///Need to improve for Multiple Activity Group
                                    }
                                }
                            }
                            else
                            {
                                Reporter.ToLog(eLogLevel.DEBUG, $"Skipping ALM Results Publish of '{activGroup.Name}' Group in {businessFlow.Name}' Flow as it dows not have any activities in it");
                            }
                        }

                        if (!exeResultList.Any())
                        {

                            if (isFlowskipped)
                            {
                                Reporter.ToLog(eLogLevel.DEBUG, $"Skipping ALM Results Publish of '{businessFlow.Name}' Flow and testplan '{bfExportedID}' as skippedUpdate configured as {ALMCore.DefaultAlmConfig.PublishSkipped}");
                                return true;
                            }
                            else
                            {
                                Reporter.ToLog(eLogLevel.DEBUG, $"Skipping ALM Results Publish of '{businessFlow.Name}' Flow and testplan '{bfExportedID}' as no valid Execution found for it");
                                result += $"  {Environment.NewLine} Skipping ALM Results Publish of '{businessFlow.Name}' Flow and 'testplan {bfExportedID}' as no valid Execution found for it ";
                                return false;
                            }
                        }

                        ResultInfo resultInfo = new ResultInfo();

                        ////
                        //// Updating of Execution Record Results (test plan level)
                        try
                        {

                            while (valuelist.Contains(exeResultList.FirstOrDefault().TestCaseExportID))
                            {
                                Thread.Sleep(1000);
                            }
                            valuelist.Add(exeResultList.FirstOrDefault().TestCaseExportID);

                            resultInfo = RQMConnect.Instance.RQMRep.ExportExecutionResult(loginData, exeResultList, RQMCore.ALMProjectGuid, ALMCore.DefaultAlmConfig.ALMProjectName, RQMCore.ALMProjectGroupName,null, ExternalFields);
                            if (!resultInfo.IsSuccess)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, $"Failed to Update Execution Record Results for  {businessFlow.Name} and testplan {bfExportedID}, execution record id {exeResultList.FirstOrDefault().ExecutionRecordExportID} Error: {resultInfo.ErrorDesc}");
                                return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, $"Failed to Update Execution Record Results for {businessFlow.Name} and testplan {bfExportedID}", ex);
                        }
                        finally
                        {
                            valuelist.Remove(exeResultList.FirstOrDefault().TestCaseExportID);
                        }

                        //
                        // Creating Test Suite Log (per test suite)
                        try
                        {
                            foreach (RQMTestSuite rQMTestSuite in testPlan.TestSuites)
                            {
                                if ((rQMTestSuite.ACL_TestSuite_Copy != null) && (rQMTestSuite.TestSuiteExecutionRecord.CurrentTestSuiteResult.ACL_TestSuiteLog_Copy != null))
                                {
                                    resultInfo = RQMConnect.Instance.RQMRep.CreateTestSuiteLog(loginData, RQMCore.ALMProjectGuid, ALMCore.DefaultAlmConfig.ALMProjectName, RQMCore.ALMProjectGroupName, rQMTestSuite.ACL_TestSuite_Copy, rQMTestSuite.TestSuiteExecutionRecord.CurrentTestSuiteResult.ACL_TestSuiteLog_Copy);
                                }
                            }
                        }
                        catch
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Failed to Update Execution Record Results");
                        }

                        //
                        // Attaching of Activity Groups Reports
                        try
                        {
                            // Attach ActivityGroup Report if needed
                            if ((publishToALMConfig.ToAttachActivitiesGroupReport) || (exectutedFromAutomateTab))
                            {
                                foreach (ActivitiesGroup activGroup in businessFlow.ActivitiesGroups)
                                {
                                    try
                                    {
                                        ACL_Data_Contract.Activity currentActivity = GetTestCaseFromActivityGroup(activGroup);
                                        ACL_Data_Contract.Attachment reportAttachment = new ACL_Data_Contract.Attachment();
                                        string activityGroupName = PathHelper.CleanInValidPathChars(activGroup.Name);
                                        if ((activGroup.TempReportFolder != null) && (activGroup.TempReportFolder != string.Empty) &&
                                        (System.IO.Directory.Exists(activGroup.TempReportFolder)))
                                        {
                                            //Creating the Zip file - start
                                            string targetZipPath = System.IO.Directory.GetParent(activGroup.TempReportFolder).ToString();
                                            string zipFileName = targetZipPath + "\\" + activityGroupName.ToString().Replace(" ", "_") + "_GingerHTMLReport.zip";
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
                                            //Creating the Zip file - finish

                                            //Attaching Zip file - start 
                                            reportAttachment.ClientFileName = activityGroupName.ToString().Replace(" ", "_") + "_GingerHTMLReport.zip";
                                            reportAttachment.ServerLocation = targetZipPath + @"\" + reportAttachment.ClientFileName;
                                            reportAttachment.CreatedBy = Environment.UserName;
                                            currentActivity.EntityId = Convert.ToInt32(GetExportedIDString(activGroup.ExternalID.ToString(), "RQMID"));
                                            currentActivity.ExportedID = (long)currentActivity.EntityId;
                                            currentActivity.ActivityData.AttachmentsColl = [reportAttachment];

                                            string exportJarFilePath = Assembly.GetExecutingAssembly().Location.Replace(@"GingerCore.dll", "") + @"ALM\\RQM\\JAVA";
                                            RQMConnect.Instance.RQMRep.UploadAttachmetToRQMAndGetIds(loginData, RQMCore.ALMProjectGuid, ALMCore.DefaultAlmConfig.ALMProjectName, RQMCore.ALMProjectGroupName, currentActivity, exportJarFilePath);
                                            RQMConnect.Instance.RQMRep.UpdateTestCaseWithNewAttachmentID(loginData, RQMCore.ALMProjectGuid, ALMCore.DefaultAlmConfig.ALMProjectName, RQMCore.ALMProjectGroupName, currentActivity);

                                            //Attaching Zip file - finish
                                            System.IO.File.Delete(zipFileName);
                                        }
                                    }
                                    catch
                                    {
                                        Reporter.ToLog(eLogLevel.ERROR, $"Failed to attach report Per ActivityGroup - {activGroup.Name}");
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            resultInfo.ErrorCode = 1;
                            result = e.ToString();
                        }

                        if (resultInfo.ErrorCode == 0)
                        {
                            if (businessFlow.ALMTestSetLevel == "RunSet")
                            {
                                result = $"{GingerDicser.GetTermResValue(eTermResKey.RunSet)}: {businessFlow.Name} Execution results published to RQM successfully.";
                            }
                            else
                            {
                                result = $"{GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)}: {businessFlow.Name} Execution results published to RQM successfully.";
                            }

                            return true;
                        }
                        else
                        {
                            if (businessFlow.ALMTestSetLevel == "RunSet")
                            {
                                result = $"{GingerDicser.GetTermResValue(eTermResKey.RunSet)}: {businessFlow.Name} Execution results failed to publist to RQM due to {resultInfo.ErrorDesc}";
                            }
                            else
                            {
                                result = $"{GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)}: {businessFlow.Name} Execution results failed to publist to RQM due to {resultInfo.ErrorDesc}";
                            }

                            Reporter.ToLog(eLogLevel.ERROR, $"Failed to export execution details to RQM/ALM due to {resultInfo.ErrorDesc}");
                            return false;
                        }
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Failed to Get Project list");
                    }

                }
            }
            // get data about execution records per current test plan - finish
            return false;
        }
        private ExecutionResult GetExeResultforActivityGroup(BusinessFlow businessFlow, string bfExportedID, ActivitiesGroup activGroup, ref string result, RQMTestPlan testPlan, RQMProject currentRQMProjectMapping, LoginDTO loginData, PublishToALMConfig publishToALMConfig, List<ACL_Data_Contract.ExternalItemFieldBase> ExternalFields = null)
        {
            try
            {
                if (string.IsNullOrEmpty(activGroup.ExternalIdCalculated))
                {
                    result = $"ExternalID not found for {GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)}: {businessFlow.Name}  {GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)}, cannot export RQM TestPlan execution results without it. Please check configured External Id.";
                    Reporter.ToLog(eLogLevel.ERROR, $"ExternalID not found for {GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)}: {businessFlow.Name}  {GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)}, cannot export RQM TestPlan execution results without it. Please check configured External Id.");
                    return null;
                }

                string testCaseId = GetExportedIDString(activGroup.ExternalIdCalculated, "RQMID");
                string testScriptId = GetExportedIDString(activGroup.ExternalIdCalculated, "RQMScriptID");
                string exeRecordId = string.Empty;//Ignore the Provided Record Id

                if (string.IsNullOrEmpty(testCaseId) || testCaseId.Equals("0"))
                {
                    result = $"Test Case Id not found for {GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)}: {businessFlow.Name} {GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)}, cannot export RQM TestPlan execution results without it. Please check configured External Id.";
                    Reporter.ToLog(eLogLevel.ERROR, $"Test Case Id not found for {GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)}: {businessFlow.Name} {GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)}, cannot export RQM TestPlan execution results without it. Please check configured External Id.");
                    return null;
                }

                ExecutionResult exeResult = new()
                {
                    TestPlanExportID = bfExportedID,
                    ExecutionStep = []
                };

                if ((activGroup.TestSuiteId != null) && (activGroup.TestSuiteId != string.Empty))
                {
                    // check if test suite execution record is exists per current Test Suite ID
                    // if not exists to create it and than procced to work on just created
                    RQMTestSuite testSuite = testPlan.TestSuites.FirstOrDefault(z => z.RQMID == activGroup.TestSuiteId);
                    if ((testSuite != null) && (testSuite.RQMID != null) && (testSuite.URLPathVersioned != null) &&
                          (testSuite.RQMID != string.Empty) && (testSuite.URLPathVersioned != string.Empty))
                    {
                        try
                        {
                            ResultInfo resultInfo;
                            // check if execution record of testSuite exist. If not - to create it
                            if ((testSuite.TestSuiteExecutionRecord == null) || (testSuite.TestSuiteExecutionRecord.RQMID == null) || (testSuite.TestSuiteExecutionRecord.URLPathVersioned == string.Empty))
                            {
                                testSuite.ACL_TestSuite_Copy = new TestSuite
                                {
                                    TestSuiteName = testSuite.Name,
                                    TestSuiteExportID = testSuite.RQMID
                                };
                                resultInfo = RQMConnect.Instance.RQMRep.CreateTestSuiteExecutionRecord(loginData, RQMCore.ALMProjectGuid, ALMCore.DefaultAlmConfig.ALMProjectName, RQMCore.ALMProjectGroupName, testSuite.ACL_TestSuite_Copy, bfExportedID, businessFlow.Name.ToString());
                                if (resultInfo.IsSuccess)
                                {
                                    if (testSuite.TestSuiteExecutionRecord == null)
                                    {
                                        testSuite.TestSuiteExecutionRecord = new RQMTestSuiteExecutionRecord();
                                    }

                                    testSuite.TestSuiteExecutionRecord.RQMID = testSuite.ACL_TestSuite_Copy.TestSuiteExecutionRecordExportID;
                                    testSuite.TestSuiteExecutionRecord.URLPathVersioned = testSuite.ACL_TestSuite_Copy.TestSuiteExecutionRecordExportUri;
                                }
                            }
                            else
                            {
                                testSuite.ACL_TestSuite_Copy = new TestSuite
                                {
                                    TestSuiteName = testSuite.Name,
                                    TestSuiteExportID = testSuite.RQMID,
                                    TestSuiteExecutionRecordExportID = testSuite.TestSuiteExecutionRecord.RQMID,
                                    TestSuiteExecutionRecordExportUri = testSuite.TestSuiteExecutionRecord.URLPathVersioned
                                };
                            }

                            // after creating of execution record at RQM and as object at Ginger (or checking that it's exists)
                            // need to create testsuiteLOG on it and add test caseexecution records on it Ginger (the objects at RQM will be created after loop)
                            ACL_Data_Contract.Activity currentActivity = GetTestCaseFromActivityGroup(activGroup);
                            resultInfo = RQMConnect.Instance.RQMRep.CreateExecutionRecordPerActivityWithInTestSuite(loginData, RQMCore.ALMProjectGuid, ALMCore.DefaultAlmConfig.ALMProjectName, RQMCore.ALMProjectGroupName, currentActivity, bfExportedID, businessFlow.Name, testSuite.Name.ToString());
                            if (resultInfo.IsSuccess)
                            {
                                if ((testSuite.TestSuiteExecutionRecord.TestSuiteResults == null) || (testSuite.TestSuiteExecutionRecord.TestSuiteResults.Count == 0) || (testSuite.TestSuiteExecutionRecord.CurrentTestSuiteResult == null))
                                {
                                    testSuite.TestSuiteExecutionRecord.TestSuiteResults = [];
                                    testSuite.TestSuiteExecutionRecord.CurrentTestSuiteResult = new RQMTestSuiteResults
                                    {
                                        RQMExecutionRecords = []
                                    };
                                    testSuite.TestSuiteExecutionRecord.TestSuiteResults.Add(testSuite.TestSuiteExecutionRecord.CurrentTestSuiteResult);
                                }

                                RQMExecutionRecord executionRecord = new RQMExecutionRecord(currentActivity.ExportedTcExecutionRecId.ToString(), currentActivity.ExportedTestScriptId.ToString(), currentActivity.ExportedID.ToString());
                                testSuite.TestSuiteExecutionRecord.CurrentTestSuiteResult.RQMExecutionRecords.Add(executionRecord);
                                testSuite.TestSuiteExecutionRecord.CurrentTestSuiteResult.ACL_TestSuiteLog_Copy.TestSuiteLogExecutionRecords.Add(exeResult);
                                exeResult.ExpectedResultName = currentActivity.EntityName;
                                exeRecordId = executionRecord.RQMID;
                                currentActivity.ExportedID = long.Parse(executionRecord.RQMID);

                                string atsID = GetExportedIDString(activGroup.ExternalID, "AtsID");
                                if (atsID == "0")
                                {
                                    atsID = string.Empty;
                                }
                                activGroup.ExternalID = $"RQMID={testCaseId}|RQMScriptID={testScriptId}|RQMRecordID={exeRecordId}|AtsID={atsID}";
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($" in if loop getExeResultforAg :{JsonConvert.SerializeObject(ex)}");
                        }
                    }
                    else
                    {

                    }
                }
                else
                {
                    string TestCaseVersionUrl = RQMConnect.Instance.GetTestCaseVersionURLByIdByProject(loginData, testPlan.PreFix, testCaseId);

                    if (string.IsNullOrEmpty(TestCaseVersionUrl))
                    {
                        if (businessFlow.ALMTestSetLevel == "RunSet")
                        {
                            result = $"At {GingerDicser.GetTermResValue(eTermResKey.RunSet)}: Test case with id {testCaseId} is not available under Test Plan {businessFlow.Name} Test Plan Id: {businessFlow.ExternalID}, Hence execution result is not published for this execution";
                            Reporter.ToLog(eLogLevel.ERROR, $"At {GingerDicser.GetTermResValue(eTermResKey.RunSet)}: Test case with id {testCaseId} is not available under Test Plan {businessFlow.Name} Test Plan Id: {businessFlow.ExternalID}, Hence execution result is not published for this execution");
                        }
                        else
                        {
                            result = $"At {GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)}: Test case with id {testCaseId} is not available under Test Plan {businessFlow.Name} Test Plan Id: {businessFlow.ExternalID}, Hence execution result is not published for this execution";
                            Reporter.ToLog(eLogLevel.ERROR, $"At {GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)}: Test case with id {testCaseId} is not available under Test Plan {businessFlow.Name} Test Plan Id: {businessFlow.ExternalID}, Hence execution result is not published for this execution");
                        }
                        return null;
                    }
                    RQMConnect.Instance.GetExecutionRecordsByTestCase(loginData, reader, currentRQMProjectMapping, RQMCore.ALMProjectGroupName, RQMCore.ALMProjectGuid, testPlan.URLPathVersioned, TestCaseVersionUrl, ref exeRecordId);
                    if (string.IsNullOrEmpty(exeRecordId) || exeRecordId.Equals("0"))
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, $"Record id not found for {businessFlow.Name}, creating new record");
                        result = CreateExecutionRecord(bfExportedID, activGroup, testPlan, loginData, testCaseId, testScriptId, ref exeRecordId,ExternalFields);
                    }
                }

                if (!string.IsNullOrEmpty(result))
                {
                    return null;
                }

                if (string.IsNullOrEmpty(exeRecordId) || exeRecordId.Equals("0"))
                {
                    if (businessFlow.ALMTestSetLevel == "RunSet")
                    {
                        result = $"Execution Record Id not found for {GingerDicser.GetTermResValue(eTermResKey.RunSet)}: {businessFlow.Name}, cannot export RQM TestPlan execution results without it. Please check configured External Id.";
                        Reporter.ToLog(eLogLevel.ERROR, $"Execution Record Id not found for {GingerDicser.GetTermResValue(eTermResKey.RunSet)}: {businessFlow.Name}, cannot export RQM TestPlan execution results without it. Please check configured External Id.");
                    }
                    else
                    {
                        result = $"Execution Record Id not found for {GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)}: {businessFlow.Name}, cannot export RQM TestPlan execution results without it. Please check configured External Id.";
                        Reporter.ToLog(eLogLevel.ERROR, $"Execution Record Id not found for {GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)}: {businessFlow.Name}, cannot export RQM TestPlan execution results without it. Please check configured External Id.");
                    }

                    return null;
                }

                exeResult.TestCaseExportID = testCaseId;
                exeResult.TestScriptExportID = testScriptId;
                exeResult.ExecutionRecordExportID = exeRecordId;
                exeResult.StartDate = businessFlow.StartTimeStamp.ToString("o");
                exeResult.EndDate = businessFlow.EndTimeStamp.ToString("o");
                if (publishToALMConfig.ToExportReportLink)
                {
                    if (!string.IsNullOrEmpty(publishToALMConfig.HtmlReportUrl))
                    {
                        if (publishToALMConfig.HtmlReportUrl.Last() != '/')
                        {
                            publishToALMConfig.HtmlReportUrl = $"{publishToALMConfig.HtmlReportUrl}/";
                        }
                        exeResult.HtmlReportUrl = publishToALMConfig.HtmlReportUrl;
                        exeResult.ExecutionId = publishToALMConfig.ExecutionId;
                        exeResult.ExecutionInstanceId = businessFlow.InstanceGuid.ToString();
                    }
                }

                int i = 1;
                StringBuilder errors;
                var relevantActivities = businessFlow.Activities.Where(x => x.ActivitiesGroupID == activGroup.FileName);

                foreach (Activity act in relevantActivities)
                {
                    ExecutionStep exeStep = new()
                    {
                        StepExpResults = act.Expected,
                        StepOrderId = i,
                        EntityDesc = act.ActivityName,
                    };

                    i++;

                    switch (act.Status)
                    {
                        case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed:
                            exeStep.StepStatus = ExecutoinStatus.Failed;
                            errors = new StringBuilder();
                            var failedActs = act.Acts.Where(x => x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed);
                            foreach (IAct action in failedActs)
                            {
                                errors.Append(action.Error).Append(Environment.NewLine);
                            }
                            exeStep.StepActualResult = errors.ToString();
                            break;
                        case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed:
                            exeStep.StepStatus = ExecutoinStatus.Passed;
                            exeStep.StepActualResult = "Passed as expected";
                            break;
                        case Amdocs.Ginger.CoreNET.Execution.eRunStatus.NA:
                            exeStep.StepStatus = ExecutoinStatus.NA;
                            exeStep.StepActualResult = "NA";
                            break;
                        case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending:
                            exeStep.StepStatus = ExecutoinStatus.In_Progress;
                            exeStep.StepActualResult = "Was not executed";
                            break;
                        case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running:
                            exeStep.StepStatus = ExecutoinStatus.In_Progress;
                            exeStep.StepActualResult = "Not Completed";
                            break;
                        case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped:
                            exeStep.StepStatus = ExecutoinStatus.Skipped;
                            exeStep.StepActualResult = "Skipped";
                            break;
                        case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked:
                            exeStep.StepStatus = ExecutoinStatus.Blocked;
                            exeStep.StepActualResult = "Blocked";
                            break;
                        case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped:
                            exeStep.StepStatus = ExecutoinStatus.Inconclusive;
                            exeStep.StepActualResult = "Stopped";
                            break;
                    }

                    //////Update Activity Group status
                    switch (activGroup.RunStatus)
                    {
                        case eActivitiesGroupRunStatus.Failed:
                            exeResult.ExecutionResultState = ExecutoinStatus.Failed;
                            break;
                        case eActivitiesGroupRunStatus.Passed:
                            exeResult.ExecutionResultState = ExecutoinStatus.Passed;
                            break;
                        case eActivitiesGroupRunStatus.NA:
                            exeResult.ExecutionResultState = ExecutoinStatus.NA;
                            break;
                        case eActivitiesGroupRunStatus.Pending:
                            exeResult.ExecutionResultState = ExecutoinStatus.In_Progress;
                            break;
                        case eActivitiesGroupRunStatus.Running:
                            exeResult.ExecutionResultState = ExecutoinStatus.In_Progress;
                            break;
                        case eActivitiesGroupRunStatus.Skipped:
                            exeResult.ExecutionResultState = ExecutoinStatus.Skipped;
                            break;
                        case eActivitiesGroupRunStatus.Blocked:
                            exeResult.ExecutionResultState = ExecutoinStatus.Blocked;
                            break;
                        case eActivitiesGroupRunStatus.Stopped:
                            exeResult.ExecutionResultState = ExecutoinStatus.Inconclusive;
                            break;
                    }

                    exeResult.ExecutionStep.Add(exeStep);
                }
                return exeResult;

            }
            catch (Exception ex)
            {
                result = $"Unexpected error occurred- {ex}";
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to export execution details to RQM/ALM {ex.InnerException}", ex);
                return null;
            }
        }

        private string CreateExecutionRecord(string bfExportedID, ActivitiesGroup activGroup, RQMTestPlan testPlan, LoginDTO loginData, string txExportID, string tsExportID, ref string erExportID, List<ACL_Data_Contract.ExternalItemFieldBase> ExternalFields = null)
        {
            string result = string.Empty;
            ACL_Data_Contract.Activity currentActivity = GetTestCaseFromActivityGroup(activGroup);
            try
            {
                
                // if executionRecord not updated and not exists - so create one in RQM and update BussinesFlow object (this may be not saved due not existed "autosave" functionality)
                var resultInfo = RQMConnect.Instance.RQMRep.CreateExecutionRecordPerActivity(loginData, RQMCore.ALMProjectGuid, ALMCore.DefaultAlmConfig.ALMProjectName, RQMCore.ALMProjectGroupName, currentActivity, bfExportedID, testPlan.Name, ExternalFields);
                if (resultInfo != null && !string.IsNullOrEmpty(resultInfo.ErrorDesc))
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - Test Case not found {resultInfo.ErrorCode}, {resultInfo.ErrorDesc}");
                    result = $"Method - {MethodBase.GetCurrentMethod().Name}, Error - Test Case not found {resultInfo.ErrorCode}, {resultInfo.ErrorDesc}";
                }
                if (!currentActivity.ExportedTcExecutionRecId.Equals("0"))
                {
                    string atsID = GetExportedIDString(activGroup.ExternalID, "AtsID");
                    if (atsID == "0")
                    {
                        atsID = string.Empty;
                    }
                    erExportID = currentActivity.ExportedTcExecutionRecId.ToString();
                    activGroup.ExternalIdCalculated = $"RQMID={txExportID}|RQMScriptID={tsExportID}|RQMRecordID={erExportID}|AtsID={atsID}";
                    Reporter.ToLog(eLogLevel.DEBUG
                        , $"created Record id with {activGroup.ExternalIdCalculated}");
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to create Execution Record Per Activity- {currentActivity.EntityName} in CreateExecutionRecord {ex.InnerException}", ex);
                result = $"Failed to create Execution Record Per Activity- {currentActivity.EntityName} in CreateExecutionRecord {ex.InnerException}";
            }
            return result;
        }
        public bool ExportBfActivitiesGroupsToALM(BusinessFlow businessFlow, ObservableList<ActivitiesGroup> grdActivitiesGroups, ref string result)
        {
            LoginDTO loginData = new LoginDTO() { User = ALMCore.DefaultAlmConfig.ALMUserName, Password = ALMCore.DefaultAlmConfig.ALMPassword, Server = ALMCore.DefaultAlmConfig.ALMServerURL };
            //ActivityPlan is TestPlan in RQM and BusinessFlow in Ginger
            List<IActivityPlan> testPlanList = []; //1
            ActivityPlan testPlan = new ActivityPlan
            {
                IsPlanDisabled = true
            };
            testPlanList.Add(testPlan);

            try
            {
                //Create (RQM)TestCase for each Ginger ActivityGroup and add it to RQM TestCase List
                testPlan.Activities = [];//3
                foreach (ActivitiesGroup ag in grdActivitiesGroups)
                {
                    testPlan.Activities.Add(GetTestCaseFromActivityGroup(ag));
                }

                RQMConnect.Instance.RQMRep.GetConection();

                ResultInfo resultInfo;
                resultInfo = RQMConnect.Instance.RQMRep.ExportTestPlan(loginData, testPlanList, ALMCore.DefaultAlmConfig.ALMServerURL, RQMCore.ALMProjectGuid, ALMCore.DefaultAlmConfig.ALMProjectName, RQMCore.ALMProjectGroupName, null);


                //Deal with response from RQM after export
                // 0 = sucsess , 1 = failed
                if (resultInfo.ErrorCode == 0)
                {
                    foreach (ActivityPlan plan in testPlanList)
                    {
                        int ActivityGroupCounter = 0;
                        int activityStepCounter = 0;
                        int activityStepOrderID = 0;
                        foreach (ACL_Data_Contract.Activity act in plan.Activities)
                        {
                            string ActivityGroupID = $"RQMID={act.ExportedID}|RQMScriptID={act.ExportedTestScriptId}|RQMRecordID={act.ExportedTcExecutionRecId}|AtsID={act.EntityId}";
                            businessFlow.ActivitiesGroups[ActivityGroupCounter].ExternalID = ActivityGroupID;

                            foreach (ACL_Data_Contract.ActivityStep activityStep in act.ActivityData.ActivityStepsColl)
                            {
                                string activityStepID = $"RQMID={activityStepOrderID}|AtsID={act.EntityId}";
                                businessFlow.Activities[activityStepCounter].ExternalID = activityStepID;
                                activityStepCounter++;
                                activityStepOrderID++;
                            }
                            activityStepOrderID = 0;
                            ActivityGroupCounter++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = $"Failed to export the {GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups)} to RQM/ALM {ex.Message}";
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to export {GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups)} to RQM/ALM", ex);
                return false;
            }

            return true;
        }

        public bool ExportBusinessFlowToRQM(BusinessFlow businessFlow, ObservableList<ExternalItemFieldBase> ExternalItemsFields, ref string result)
        {
            mExternalItemsFields = ExternalItemsFields;
            LoginDTO loginData = new LoginDTO() { User = ALMCore.DefaultAlmConfig.ALMUserName, Password = ALMCore.DefaultAlmConfig.ALMPassword, Server = ALMCore.DefaultAlmConfig.ALMServerURL };
            ProjEnvironment projEnvironment = new ProjEnvironment();
            if (projEnvironment != null)
            {
                IValueExpression mVE = new GingerCore.ValueExpression(projEnvironment, businessFlow, [], false, "", false);
                businessFlow.CalculateExternalId(mVE);
            }
            //ActivityPlan is TestPlan in RQM and BusinessFlow in Ginger
            List<IActivityPlan> testPlanList = []; //1
            ActivityPlan testPlan = GetTestPlanFromBusinessFlow(businessFlow);
            testPlanList.Add(testPlan);//2

            if (businessFlow.ActivitiesGroups.Count == 0)
            {
                if (businessFlow.ALMTestSetLevel == "RunSet")
                {
                    result = $"{GingerDicser.GetTermResValue(eTermResKey.RunSet)} must have at least one {GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)}";
                    Reporter.ToLog(eLogLevel.ERROR, $"{GingerDicser.GetTermResValue(eTermResKey.RunSet)} must have at least one {GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)}");
                    return false;
                }
                else
                {
                    result = $"{GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)} must have at least one {GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)}";
                    Reporter.ToLog(eLogLevel.ERROR, $"{GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)} must have at least one {GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)}");
                    return false;
                }

            }

            ALM_CommonStd.DataContracts.ResultInfo resultInfo;
            try
            {
                //Create (RQM)TestCase for each Ginger ActivityGroup and add it to RQM TestCase List
                testPlan.Activities = [];//3

                foreach (ActivitiesGroup ag in businessFlow.ActivitiesGroups)
                {
                    if (projEnvironment != null)
                    {
                        IValueExpression magVE = new GingerCore.ValueExpression(projEnvironment, businessFlow, [], false, "", false);
                        ag.CalculateExternalId(magVE);
                    }
                    testPlan.Activities.Add(GetTestCaseFromActivityGroup(ag));

                }
                List<ACL_Data_Contract.ExternalItemFieldBase> ExternalFields = ConvertExternalFieldsToACLDataContractfields(mExternalItemsFields);
                RQMConnect.Instance.RQMRep.GetConection();

                resultInfo = RQMConnect.Instance.RQMRep.ExportTestPlan(loginData, testPlanList, ALMCore.DefaultAlmConfig.ALMServerURL, RQMCore.ALMProjectGuid, ALMCore.DefaultAlmConfig.ALMProjectName, RQMCore.ALMProjectGroupName, null,null, ExternalFields);
            }
            catch (Exception ex)
            {
                if (businessFlow.ALMTestSetLevel == "RunSet")
                {
                    result = $"Failed to export the {GingerDicser.GetTermResValue(eTermResKey.RunSet)} to RQM/ALM {ex.Message}";
                    Reporter.ToLog(eLogLevel.ERROR, $"Failed to export the {GingerDicser.GetTermResValue(eTermResKey.RunSet)} to RQM/ALM", ex);
                }
                else
                {
                    result = $"Failed to export the {GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)} to RQM/ALM {ex.Message}";
                    Reporter.ToLog(eLogLevel.ERROR, $"Failed to export the {GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)} to RQM/ALM", ex);
                }

                return false;
            }

            // Deal with response from RQM after export
            // 0 = sucsess , 1 = failed 
            if (resultInfo.ErrorCode == 0)
            {
                foreach (ActivityPlan plan in testPlanList)
                {
                    if (string.IsNullOrEmpty(businessFlow.ExternalID))
                    {
                        businessFlow.ExternalID = $"RQMID={plan.ExportedID}";
                    }
                    else
                    {
                        businessFlow.ExternalIdCalCulated = $"RQMID={plan.ExportedID}";
                        if (plan.ALMTestSetLevel == "RunSet")
                        {
                            businessFlow.ExternalID = $"RQMID={plan.ExportedID}";
                        }
                    }

                    int ActivityGroupCounter = 0;
                    int activityStepCounter = 0;
                    int activityStepOrderID = 1;
                    if (ALMCore.DefaultAlmConfig.IsTestSuite == "True")
                    {
                        foreach (ACL_Data_Contract.TestSuite testSuite in plan.TestSuites)
                        {
                            ACL_Data_Contract.Activity act = (ACL_Data_Contract.Activity)testSuite.Activities.FirstOrDefault();
                            string ActivityGroupID = $"RQMID={act.ExportedID}|RQMScriptID={act.ExportedTestScriptId}|RQMRecordID={act.ExportedTcExecutionRecId}|AtsID={act.EntityId}";
                            businessFlow.ActivitiesGroups[ActivityGroupCounter].ExternalID = ActivityGroupID;
                            businessFlow.ActivitiesGroups[ActivityGroupCounter].TestSuiteId = testSuite.TestSuiteId;
                            businessFlow.ActivitiesGroups[ActivityGroupCounter].TestSuiteTitle = testSuite.TestSuiteName;
                            foreach (ACL_Data_Contract.ActivityStep activityStep in act.ActivityData.ActivityStepsColl)
                            {
                                string activityStepID = $"RQMID={act.ExportedTestScriptId}_{activityStepOrderID}|AtsID={act.EntityId}";
                                businessFlow.Activities[activityStepCounter].ExternalID = activityStepID;
                                activityStepCounter++;
                                activityStepOrderID++;
                            }
                            activityStepOrderID = 0;
                            ActivityGroupCounter++;
                        }
                    }
                    else
                    {
                        foreach (ACL_Data_Contract.Activity act in plan.Activities)
                        {
                            string ActivityGroupID = $"RQMID={act.ExportedID}|RQMScriptID={act.ExportedTestScriptId}|RQMRecordID={act.ExportedTcExecutionRecId}|AtsID={act.EntityId}";
                            if (string.IsNullOrEmpty(businessFlow.ActivitiesGroups[ActivityGroupCounter].ExternalID))
                            {
                                businessFlow.ActivitiesGroups[ActivityGroupCounter].ExternalID = ActivityGroupID;
                            }
                            else
                            {
                                businessFlow.ActivitiesGroups[ActivityGroupCounter].ExternalIdCalculated = ActivityGroupID;
                                if (plan.ALMTestSetLevel == "RunSet")
                                {
                                    businessFlow.ActivitiesGroups[ActivityGroupCounter].ExternalID = ActivityGroupID;
                                }
                            }
                            foreach (ACL_Data_Contract.ActivityStep activityStep in act.ActivityData.ActivityStepsColl)
                            {
                                string activityStepID = $"RQMID={act.ExportedTestScriptId}_{activityStepOrderID}|AtsID={act.EntityId}";
                                businessFlow.Activities[activityStepCounter].ExternalID = activityStepID;
                                activityStepCounter++;
                                activityStepOrderID++;
                            }
                            activityStepOrderID = 0;
                            ActivityGroupCounter++;
                        }
                    }
                }
                return true;
            }
            else
            {
                if (businessFlow.ALMTestSetLevel == "RunSet")
                {
                    result = $"Failed to export the {GingerDicser.GetTermResValue(eTermResKey.RunSet)} to RQM/ALM, {resultInfo.ErrorDesc}";
                    Reporter.ToLog(eLogLevel.ERROR, $"Failed to export the {GingerDicser.GetTermResValue(eTermResKey.RunSet)} to RQM/ALM, {resultInfo.ErrorDesc}");
                }
                else
                {
                    result = $"Failed to export the {GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)} to RQM/ALM, {resultInfo.ErrorDesc}";
                    Reporter.ToLog(eLogLevel.ERROR, $"Failed to export the {GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)} to RQM/ALM, {resultInfo.ErrorDesc}");
                }

                return false;
            }
            return false;
        }

        private List<ACL_Data_Contract.ExternalItemFieldBase> ConvertExternalFieldsToACLDataContractfields(ObservableList<ExternalItemFieldBase> fields)
        {
            List<ACL_Data_Contract.ExternalItemFieldBase> fieldsToReturn = [];

            //Going through the fields to leave only Test Set fields
            for (int indx = 0; indx < fields.Count; indx++)
            {
                    ACL_Data_Contract.ExternalItemFieldBase field = new ACL_Data_Contract.ExternalItemFieldBase();
                    field.ItemType = fields[indx].ItemType;
                    field.Name = fields[indx].Name;
                    field.SelectedValue = fields[indx].SelectedValue;
                    field.SelectedValueKey = fields[indx].SelectedValueKey;
                    field.ID = fields[indx].ID;
                    field.Type = fields[indx].Type;
                    field.TypeIdentifier = fields[indx].TypeIdentifier;
                    field.IsMultiple = fields[indx].IsMultiple;
                    
                if (!fieldsToReturn.Any(f => f.ID == field.ID))
                {
                    // Add it if it doesn't already exist
                    fieldsToReturn.Add(field);
                }
            }
            return fieldsToReturn;
        }

        private ActivityPlan GetTestPlanFromBusinessFlow(BusinessFlow businessFlow)
        {
            ActivityPlan testPlan = new ActivityPlan();
            //Check if updating or creating new instance in RQM
            if (!string.IsNullOrEmpty(businessFlow.ExternalIdCalCulated))
            {
                try
                {
                    long rqmID = Convert.ToInt64(GetExportedIDString(businessFlow.ExternalIdCalCulated, "RQMID"));
                    //getExportID(businessFlow.ExternalID);
                    if (rqmID != 0)
                    {
                        testPlan.ExportedID = rqmID;
                        testPlan.ShouldUpdated = true;
                    }
                }
                catch (Exception e)
                {
                    testPlan.ShouldUpdated = false;
                    Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
                }
            }

            testPlan.EntityName = businessFlow.Name;
            testPlan.EntityDesc = businessFlow.Description == null ? "" : businessFlow.Description;
            testPlan.IsEntitySearchByName = businessFlow.IsEntitySearchByName;
            testPlan.ALMTestSetLevel = businessFlow.ALMTestSetLevel;
            List<TestSuite> testSuites = [];
            if (ALMCore.DefaultAlmConfig.IsTestSuite == "True")
            {
                foreach (ActivitiesGroup activitiesGroup in businessFlow.ActivitiesGroups)
                {
                    ProjEnvironment projEnvironment = new ProjEnvironment();
                    if (projEnvironment != null)
                    {
                        IValueExpression mAgVE = new GingerCore.ValueExpression(projEnvironment, businessFlow, [], false, "", false);
                        activitiesGroup.CalculateExternalId(mAgVE);
                    }
                    TestSuite testSuite = new TestSuite
                    {
                        TestSuiteName = activitiesGroup.Name,
                        TestSuiteDescription = String.IsNullOrEmpty(activitiesGroup.Description) ? String.Empty : activitiesGroup.Description,
                        Activities = [GetTestCaseFromActivityGroup(activitiesGroup)]//3
                    };

                    testSuites.Add(testSuite);
                }
            }
            testPlan.TestSuites = testSuites;
            //Add custom properties
            Dictionary<string, string> ActivityLevelproperties = GetCustomProperties("TestPlan");
            testPlan.CustomProperties = ActivityLevelproperties;

            return testPlan;
        }

        private ACL_Data_Contract.Activity GetTestCaseFromActivityGroup(ActivitiesGroup activityGroup)
        {

            if (activityGroup.ActivitiesIdentifiers.Count == 0)
            {
                throw new Exception("Each Activity Group must have at least one activity");
            }

            ACL_Data_Contract.Activity testCase = new ACL_Data_Contract.Activity();

            //Check if updating or creating new instance in RQM
            if (!string.IsNullOrEmpty(activityGroup.ExternalIdCalculated))
            {
                try
                {
                    long RQMID = Convert.ToInt64(GetExportedIDString(activityGroup.ExternalIdCalculated, "RQMID"));
                    long RQMScriptID = Convert.ToInt64(GetExportedIDString(activityGroup.ExternalIdCalculated, "RQMScriptID"));
                    long RQMRecordID = Convert.ToInt64(GetExportedIDString(activityGroup.ExternalIdCalculated, "RQMRecordID"));

                    testCase.ExportedID = RQMID;
                    testCase.ExportedTestScriptId = RQMScriptID;
                    testCase.ExportedTcExecutionRecId = RQMRecordID;
                    testCase.ShouldUpdated = true;
                    testCase.StartDate = activityGroup.StartTimeStamp.ToString();
                    testCase.EndDate = activityGroup.EndTimeStamp.ToString();
                }
                catch (Exception e)
                {
                    testCase.ShouldUpdated = false;
                    Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
                }
            }

            testCase.EntityName = activityGroup.Name;
            testCase.EntityDesc = activityGroup.Description == null ? "" : activityGroup.Description;
            testCase.EntityId = 0;

            //Add custom properties
            Dictionary<string, string> properties = GetCustomProperties("TestCase");
            testCase.CustomProperties = properties;

            testCase.ActivityData = GetTestScriptStep(activityGroup.ActivitiesIdentifiers);//4

            return testCase;
        }

        private ACL_Data_Contract.ActivityData GetTestScriptStep(ObservableList<ActivityIdentifiers> stepList)
        {
            ActivityData ad = new ActivityData();
            int orderID = 0;

            foreach (ActivityIdentifiers actIden in stepList)
            {
                ActivityStep activityStep = new ActivityStep();
                if (!string.IsNullOrEmpty(actIden.ExternalID))
                {
                    try
                    {
                        long rqmID = Convert.ToInt64(GetExportedIDString(actIden.ExternalID, "RQMID"));
                        //getExportID(actIden.ExternalID);
                        if (rqmID != 0)
                        {
                            activityStep.ExportedID = rqmID;
                            activityStep.ShouldUpdated = true;
                        }
                    }
                    catch (Exception e)
                    {
                        activityStep.ShouldUpdated = false;
                        Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
                    }
                }

                if (actIden == null || actIden.IdentifiedActivity == null)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error: When Exporting to RQM, ActivityIdentifiers object or actIden.IdentifiedActivity is null and cannot export to RQM");
                    break;
                }

                activityStep.EntityName = actIden.ActivityName;
                string description = actIden.ActivityDescription == null ? string.Empty : actIden.ActivityDescription;
                activityStep.StepExpResults = actIden.IdentifiedActivity.Expected;
                activityStep.StepOrderId = orderID;
                orderID++;
                activityStep.EntityId = 0;

                foreach (GingerCore.Variables.VariableBase variable in actIden.IdentifiedActivity.Variables)
                {
                    ActivityParam param = new ActivityParam
                    {
                        EntityName = variable.FileName,
                        DefaultValue = variable.Value
                    };
                    //param. //Add automation status to param
                    ad.ActivityParamsColl.Add(param);
                    description = description + GetVariableWithSigns(variable.FileName, variable.Value);
                }
                activityStep.EntityDesc = description;
                ad.ActivityStepsColl.Add(activityStep);
            }
            return ad;
        }

        private string GetVariableWithSigns(string variableName, string variableValue)
        {
            return $"<<<{variableName}&?&{variableValue}>>>";
        }

        public static string GetExportedIDString(string externalID, string searchString)
        {
            try
            {
                string regexPattern = searchString + @"=(\d*)";
                MatchCollection matches = Regex.Matches(externalID, regexPattern);
                if ((matches[0].Groups[1].Value == null) || (matches[0].Groups[1].Value == string.Empty))
                {
                    return "0";
                }
                else
                {
                    return matches[0].Groups[1].Value;
                }
            }
            catch (Exception ex)
            {
                if (!searchString.Equals("AtsID"))
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                }
                //No matches found
                return "0";
            }
        }

        private Dictionary<string, string> GetCustomProperties(string ItemType)
        {
            Dictionary<string, string> propertiesList = [];

            foreach (ExternalItemFieldBase itemField in mExternalItemsFields)
            {
                if (itemField.ItemType == ItemType)
                {
                    if (itemField.Mandatory == true || itemField.ToUpdate == true)
                    {
                        if (!propertiesList.ContainsKey(itemField.Name))
                        {
                            propertiesList.Add(itemField.Name, itemField.SelectedValue);
                        }
                    }
                }
            }

            Reporter.ToLog(eLogLevel.INFO, $"Custom Properties count {mExternalItemsFields.Count}");
            return propertiesList;
        }

        public Dictionary<Guid, string> CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, List<ExternalItemFieldBase> defectsFields, bool useREST)
        {
            LoginDTO loginData = new LoginDTO() { User = ALMCore.DefaultAlmConfig.ALMUserName, Password = ALMCore.DefaultAlmConfig.ALMPassword, Server = ALMCore.DefaultAlmConfig.ALMServerURL };

            Dictionary<Guid, string> defectsOpeningResults = [];
            Dictionary<string, List<string>> defectsBFs = [];
            DefectData newDefect = new DefectData();
            foreach (KeyValuePair<Guid, Dictionary<string, string>> defectForOpening in defectsForOpening)
            {

                if (defectForOpening.Value.ContainsKey("Summary"))
                {
                    newDefect.summary = defectForOpening.Value["Summary"];
                }
                if (defectForOpening.Value.ContainsKey("description"))
                {
                    newDefect.description = defectForOpening.Value["description"].TrimEnd(' ').Trim('\n');
                }
                AddEntityFieldValues(defectsFields, newDefect, "defect");
                ResultInfo resultInfo;
                RQMConnect.Instance.RQMRep.GetConection();

                resultInfo = RQMConnect.Instance.RQMRep.CreateDefectNew(loginData, newDefect, ALMCore.DefaultAlmConfig.ALMServerURL, RQMCore.ALMProjectGuid, ALMCore.DefaultAlmConfig.ALMProjectName, RQMCore.ALMProjectGroupName, null);
                if (resultInfo.IsSuccess)
                {
                    defectsOpeningResults.Add(defectForOpening.Key, newDefect.WorkItemId);
                }
            }
            return defectsOpeningResults;
        }

        private void AddEntityFieldValues(List<ExternalItemFieldBase> fields, DefectData newDefect, string entityType)
        {


            foreach (ExternalItemFieldBase field in fields)
            {
                try
                {
                    if (!string.IsNullOrEmpty(field.SelectedValue) && field.SelectedValue != "Unassigned" && field.ItemType.ToLower() == "string")
                    {
                        switch (field.ID)
                        {
                            case string str when str.Contains("filedAgainst")
                                :
                                newDefect.filedAgainst = field.SelectedValue;
                                break;
                            case string str when str.Contains("attachment")
                                :
                                break;
                            case string str when str.Contains("project")
                                :
                                newDefect.projectArea = field.SelectedValue;
                                break;
                            case string str when str.Contains("severity")
                                :
                                newDefect.severity = field.SelectedValue;
                                break;
                            case string str when str.Contains("priority")
                                :
                                newDefect.priority = field.SelectedValue;
                                break;
                            case string str when str.Contains("RootCauseCategory")
                                :
                                newDefect.rootCauseCategorySting = field.SelectedValue;
                                newDefect.rootCauseCategory = [];
                                if (newDefect.rootCauseCategorySting != null)
                                {
                                    if (newDefect.rootCauseCategorySting.Contains(","))
                                    {
                                        newDefect.rootCauseCategory = newDefect.rootCauseCategorySting.Split(',').ToList();
                                    }
                                    else
                                    {
                                        newDefect.rootCauseCategory.Add(newDefect.rootCauseCategorySting);
                                    }
                                }
                                break;
                            case string str when str.Contains("Environment")
                                :
                                newDefect.environement = field.SelectedValue;
                                break;
                            case string str when str.Contains("subscribers")
                                :
                                newDefect.subcriptionsString = field.SelectedValue;
                                newDefect.subcriptions = [];
                                if (newDefect.subcriptionsString != null)
                                {
                                    if (newDefect.subcriptionsString.Contains(","))
                                    {
                                        newDefect.subcriptions = newDefect.subcriptionsString.Split(',').ToList();
                                    }
                                    else
                                    {
                                        newDefect.subcriptions.Add(newDefect.subcriptionsString);
                                    }
                                }
                                break;
                            case string str when str.Contains("subdomain")
                                :
                                newDefect.subApplication = field.SelectedValue;
                                break;
                            case string str when str.Contains("AllowNoProjectLink")
                                :
                                newDefect.allowNoProjectLink = field.SelectedValue;
                                break;
                            case string str when str.Contains("type")
                                :
                                newDefect.workItemType = field.SelectedValue;
                                break;
                            case string str when str.Contains("DefectSubType")
                                :
                                newDefect.defectSubtype = field.SelectedValue;
                                break;
                            case string str when str.Contains("CcTo")
                                :
                                newDefect.ccToString = field.SelectedValue;
                                newDefect.ccTo = [];
                                if (newDefect.ccToString != null)
                                {
                                    if (newDefect.ccToString.Contains(","))
                                    {
                                        newDefect.ccTo = newDefect.ccToString.Split(',').ToList();
                                    }
                                    else
                                    {
                                        newDefect.ccTo.Add(newDefect.ccToString);
                                    }
                                }
                                break;
                            case string str when str.Contains("ProblemSourceSub-Category")
                                :
                                newDefect.problemSourceSubCategory = field.SelectedValue;
                                break;
                            case string str when str.Contains("ProblemSourceCategory")
                                :
                                newDefect.problemSourceCategory = field.SelectedValue;
                                break;
                            case string str when str.Contains("AssignmentGroup")
                                :
                                newDefect.assigenmentGroup = field.SelectedValue;
                                break;
                            case string str when str.Contains("RiskAssessment")
                                :
                                newDefect.riskAssessment = field.SelectedValue;
                                break;
                            case string str when str.Contains("foundIn")
                                :
                                newDefect.foundIn = field.SelectedValue;
                                break;
                            case string str when str.Contains("due")
                                :
                                try
                                {
                                    DateTime duedate = Convert.ToDateTime(field.SelectedValue);
                                    newDefect.dueDate = new DateTimeOffset(duedate).ToUnixTimeMilliseconds().ToString(); //duedate.Millisecond.ToString();
                                }
                                catch (Exception ex)
                                {
                                    Reporter.ToLog(eLogLevel.ERROR, "due date entered incorrect formate Please enter in 'yyyy-mm-dd'", ex.InnerException);
                                    Reporter.ToUser(eUserMsgKey.WrongDateValueInserted);
                                }

                                break;
                            case string str when str.Contains("Product")
                                :
                                newDefect.productGroup = field.SelectedValue;
                                break;
                            case string str when str.Contains("contributor")
                                :
                                newDefect.ownedBy = field.SelectedValue;
                                break;
                            case string str when str.Contains("RaisedByTeam")
                                :
                                newDefect.raisedByTeam = field.SelectedValue;
                                break;

                            default:
                                break;

                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "In AddEntityFieldValues function", ex);
                }
            }
        }
    }
}
