#region License
/*
Copyright © 2014-2018 European Support Limited

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
using ACL_Data_Contract;
using ACL_Data_Contract.Abstraction;
using ALM_Common.DataContracts;
using GingerCore.Actions;
using GingerCore.Activities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO.Compression;
using System.Xml.Serialization;
using System.Reflection;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.IO;

namespace GingerCore.ALM.RQM
{
    public class ExportToRQM
    {
        ObservableList<ExternalItemFieldBase> mExternalItemsFields = new ObservableList<ExternalItemFieldBase>();
       
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

        public bool ExportExecutionDetailsToRQM(BusinessFlow businessFlow, ref string result, bool exectutedFromAutomateTab = false, PublishToALMConfig publishToALMConfig = null)
        {
            result = string.Empty;
            string bfExportedID = GetExportedIDString(businessFlow.ExternalID, "RQMID");

            if (string.IsNullOrEmpty(bfExportedID) || bfExportedID.Equals("0"))
            {
                result = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ": " + businessFlow.Name + " is missing ExternalID, cannot export RQM TestPlan execution results without Extrnal ID";
                return false;
            }

            if (businessFlow.ActivitiesGroups.Count == 0)
            {
                result = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ": " + businessFlow.Name + " Must have at least one " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup);
                return false;
            }

            LoginDTO loginData = new LoginDTO() { User = ALMCore.AlmConfig.ALMUserName, Password = ALMCore.AlmConfig.ALMPassword, Server = ALMCore.AlmConfig.ALMServerURL };

            // 
            // get data about execution records per current test plan - start
            RQMTestPlan testPlan = new RQMTestPlan();
            string importConfigTemplate = System.IO.Path.Combine(RQMCore.ConfigPackageFolderPath, "RQM_Import", "RQM_ImportConfigs_Template.xml");
            if (File.Exists(importConfigTemplate))
            {
                XmlSerializer serializer = new
                XmlSerializer(typeof(RQMProjectListConfiguration));
                FileStream fs = new FileStream(importConfigTemplate, FileMode.Open);
                XmlReader reader = XmlReader.Create(fs);
                RQMProjectListConfiguration RQMProjectList;
                RQMProjectList = (RQMProjectListConfiguration)serializer.Deserialize(reader);
                fs.Close();

                RQMProject currentRQMProjectMapping;
                if (RQMProjectList.RQMProjects.Count > 0)
                {
                    currentRQMProjectMapping = RQMProjectList.RQMProjects.Where(x => x.Name == ALMCore.AlmConfig.ALMProjectName || x.Name == "DefaultProjectName").FirstOrDefault();
                    if (currentRQMProjectMapping != null)
                    {
                        testPlan = RQMConnect.Instance.GetRQMTestPlanByIdByProject(ALMCore.AlmConfig.ALMServerURL, ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword, ALMCore.AlmConfig.ALMProjectName, GetExportedIDString(businessFlow.ExternalID, "RQMID"));

                        if (testPlan == null)
                        {
                            result = "Recent Testing Plan not exists in connected RQM project.";
                            return false;
                        }

                        testPlan.RQMExecutionRecords = RQMConnect.Instance.GetExecutionRecordsByTestPlan(loginData, reader, currentRQMProjectMapping, RQMCore.ALMProjectGroupName, RQMCore.ALMProjectGuid, testPlan.URLPathVersioned);
                    }
                }
            }
            // get data about execution records per current test plan - finish

            List<ExecutionResult> exeResultList = new List<ExecutionResult>();
            foreach (ActivitiesGroup activGroup in businessFlow.ActivitiesGroups)
            {                 
                if ((publishToALMConfig.FilterStatus == FilterByStatus.OnlyPassed && activGroup.RunStatus == ActivitiesGroup.eActivitiesGroupRunStatus.Passed) 
                    || (publishToALMConfig.FilterStatus == FilterByStatus.OnlyFailed && activGroup.RunStatus == ActivitiesGroup.eActivitiesGroupRunStatus.Failed)
                    || publishToALMConfig.FilterStatus == FilterByStatus.All)
                {
                    ExecutionResult exeResult = GetExeResultforAg(businessFlow, bfExportedID, activGroup, ref result, testPlan);
                    if (exeResult != null)
                        exeResultList.Add(exeResult);
                    else
                        return false;
                }                
            }

            ResultInfo resultInfo = new ResultInfo();

            //
            // Updating of Execution Record Results (test plan level)
            try
            {
                resultInfo = RQMConnect.Instance.RQMRep.ExportExecutionResult(loginData, exeResultList, RQMCore.ALMProjectGuid, ALMCore.AlmConfig.ALMProjectName, RQMCore.ALMProjectGroupName);
            }
            catch
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to Update Execution Record Results");
            }

            //
            // Creating Test Suite Log (per test suite)
            try
            {
                foreach (RQMTestSuite rQMTestSuite in testPlan.TestSuites)
                {
                    if ((rQMTestSuite.ACL_TestSuite_Copy != null) && (rQMTestSuite.TestSuiteExecutionRecord.CurrentTestSuiteResult.ACL_TestSuiteLog_Copy != null))
                    {
                        resultInfo = RQMConnect.Instance.RQMRep.CreateTestSuiteLog(loginData, RQMCore.ALMProjectGuid, ALMCore.AlmConfig.ALMProjectName, RQMCore.ALMProjectGroupName, rQMTestSuite.ACL_TestSuite_Copy, rQMTestSuite.TestSuiteExecutionRecord.CurrentTestSuiteResult.ACL_TestSuiteLog_Copy);
                    }
                }
            }
            catch
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to Update Execution Record Results");
            }

            //
            // Attaching of Activity Groups Reports
            try
            {
                // Attach ActivityGroup Report if needed
                if ((publishToALMConfig .ToAttachActivitiesGroupReport) || (exectutedFromAutomateTab))
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
                                currentActivity.ActivityData.AttachmentsColl = new Attachments();
                                currentActivity.ActivityData.AttachmentsColl.Add(reportAttachment);

                                string exportJarFilePath = Assembly.GetExecutingAssembly().Location.Replace(@"GingerCore.dll", "") + @"ALM\\RQM\\JAVA";
                                resultInfo = RQMConnect.Instance.RQMRep.UploadAttachmetToRQMAndGetIds(loginData, RQMCore.ALMProjectGuid, ALMCore.AlmConfig.ALMProjectName, RQMCore.ALMProjectGroupName, currentActivity, exportJarFilePath);

                                resultInfo = RQMConnect.Instance.RQMRep.UpdateTestCaseWithNewAttachmentID(loginData, RQMCore.ALMProjectGuid, ALMCore.AlmConfig.ALMProjectName, RQMCore.ALMProjectGroupName, currentActivity);

                                //Attaching Zip file - finish
                                System.IO.File.Delete(zipFileName);
                            }
                        }
                        catch
                        {
                            Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to attach report Per ActivityGroup - " + activGroup.Name);
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
                result = "Export execution details to RQM performed successfully.";
                return true;
            }
            else result = resultInfo.ErrorDesc;

            Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to export execution details to RQM/ALM");
            return false;
        }

        private ExecutionResult GetExeResultforAg(BusinessFlow businessFlow, string bfExportedID, ActivitiesGroup activGroup, ref string result, RQMTestPlan testPlan)
        {
            try
            {
                LoginDTO loginData = new LoginDTO() { User = ALMCore.AlmConfig.ALMUserName, Password = ALMCore.AlmConfig.ALMPassword, Server = ALMCore.AlmConfig.ALMServerURL };

                if (string.IsNullOrEmpty(activGroup.ExternalID))
                {
                    result = "At " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ": " + businessFlow.Name + " " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + ", is missing ExternalID, cannot export RQM TestPlan execution results without External ID";
                    return null;
                }

                ExecutionResult exeResult = new ExecutionResult {TestPlanExportID = bfExportedID};

                List<Activity> relevantActivities = new List<Activity>();
                relevantActivities = businessFlow.Activities.Where(x => x.ActivitiesGroupID == activGroup.FileName).ToList();
                exeResult.ExecutionStep = new List<ExecutionStep>();

                string txExportID = GetExportedIDString(activGroup.ExternalID, "RQMID");
                string tsExportID = GetExportedIDString(activGroup.ExternalID, "RQMScriptID");
                string erExportID = GetExportedIDString(activGroup.ExternalID, "RQMRecordID");
                if ((activGroup.TestSuiteId != null) && (activGroup.TestSuiteId != string.Empty))
                {
                    // check if test suite execution record is exists per current Test Suite ID
                    // if not exists to create it and than procced to work on just created
                    RQMTestSuite testSuite = testPlan.TestSuites.Where(z => z.RQMID == activGroup.TestSuiteId).FirstOrDefault();
                    if ((testSuite != null) && (testSuite.RQMID != null) && (testSuite.URLPathVersioned != null) &&
                          (testSuite.RQMID != string.Empty) && (testSuite.URLPathVersioned != string.Empty))
                    {
                        try
                        {
                            ResultInfo resultInfo;
                            // check if execution record of testSuite exist. If not - to create it
                            if ((testSuite.TestSuiteExecutionRecord == null) || (testSuite.TestSuiteExecutionRecord.RQMID == null) || (testSuite.TestSuiteExecutionRecord.URLPathVersioned == string.Empty))
                            {
                                testSuite.ACL_TestSuite_Copy = new TestSuite();
                                testSuite.ACL_TestSuite_Copy.TestSuiteName = testSuite.Name;
                                testSuite.ACL_TestSuite_Copy.TestSuiteExportID = testSuite.RQMID;
                                resultInfo = RQMConnect.Instance.RQMRep.CreateTestSuiteExecutionRecord(loginData, RQMCore.ALMProjectGuid, ALMCore.AlmConfig.ALMProjectName, RQMCore.ALMProjectGroupName, testSuite.ACL_TestSuite_Copy, bfExportedID, businessFlow.Name.ToString());

                                if (resultInfo.IsSuccess)
                                {
                                    if (testSuite.TestSuiteExecutionRecord == null)
                                        testSuite.TestSuiteExecutionRecord = new RQMTestSuiteExecutionRecord();
                                    testSuite.TestSuiteExecutionRecord.RQMID = testSuite.ACL_TestSuite_Copy.TestSuiteExecutionRecordExportID;
                                    testSuite.TestSuiteExecutionRecord.URLPathVersioned = testSuite.ACL_TestSuite_Copy.TestSuiteExecutionRecordExportUri;
                                }
                            }
                            else
                            {
                                testSuite.ACL_TestSuite_Copy = new TestSuite();
                                testSuite.ACL_TestSuite_Copy.TestSuiteName = testSuite.Name;
                                testSuite.ACL_TestSuite_Copy.TestSuiteExportID = testSuite.RQMID;
                                testSuite.ACL_TestSuite_Copy.TestSuiteExecutionRecordExportID = testSuite.TestSuiteExecutionRecord.RQMID;
                                testSuite.ACL_TestSuite_Copy.TestSuiteExecutionRecordExportUri = testSuite.TestSuiteExecutionRecord.URLPathVersioned;
                            }

                            // after creating of execution record at RQM and as object at Ginger (or checking that it's exists)
                            // need to create testsuiteLOG on it and add test caseexecution records on it Ginger (the objects at RQM will be created after loop)
                            ACL_Data_Contract.Activity currentActivity = GetTestCaseFromActivityGroup(activGroup);
                            resultInfo = RQMConnect.Instance.RQMRep.CreateExecutionRecordPerActivityWithInTestSuite(loginData, RQMCore.ALMProjectGuid, ALMCore.AlmConfig.ALMProjectName, RQMCore.ALMProjectGroupName, currentActivity, bfExportedID, businessFlow.Name, testSuite.Name.ToString());
                            if (resultInfo.IsSuccess)
                            {
                                if ((testSuite.TestSuiteExecutionRecord.TestSuiteResults == null) || (testSuite.TestSuiteExecutionRecord.TestSuiteResults.Count == 0) || (testSuite.TestSuiteExecutionRecord.CurrentTestSuiteResult == null))
                                {
                                    testSuite.TestSuiteExecutionRecord.TestSuiteResults = new ObservableList<RQMTestSuiteResults>();
                                    testSuite.TestSuiteExecutionRecord.CurrentTestSuiteResult = new RQMTestSuiteResults();
                                    testSuite.TestSuiteExecutionRecord.CurrentTestSuiteResult.RQMExecutionRecords = new ObservableList<RQMExecutionRecord>();
                                    testSuite.TestSuiteExecutionRecord.TestSuiteResults.Add(testSuite.TestSuiteExecutionRecord.CurrentTestSuiteResult);
                                }

                                RQMExecutionRecord executionRecord = new RQMExecutionRecord(currentActivity.ExportedTcExecutionRecId.ToString(), currentActivity.ExportedTestScriptId.ToString(), currentActivity.ExportedID.ToString());
                                testSuite.TestSuiteExecutionRecord.CurrentTestSuiteResult.RQMExecutionRecords.Add(executionRecord);
                                testSuite.TestSuiteExecutionRecord.CurrentTestSuiteResult.ACL_TestSuiteLog_Copy.TestSuiteLogExecutionRecords.Add(exeResult);
                                exeResult.ExpectedResultName = currentActivity.EntityName;
                                erExportID = executionRecord.RQMID;
                                currentActivity.ExportedID = long.Parse(executionRecord.RQMID);

                                string atsID = GetExportedIDString(activGroup.ExternalID, "AtsID");
                                if (atsID == "0")
                                {
                                    atsID = string.Empty;
                                }
                                activGroup.ExternalID = "RQMID=" + txExportID + "|RQMScriptID=" + tsExportID + "|RQMRecordID=" + erExportID + "|AtsID=" + atsID;
                            }
                        }
                        catch { }
                    }
                    else
                    {

                    }
                }
                else if (string.IsNullOrEmpty(erExportID) || erExportID.Equals("0") || !testPlan.RQMExecutionRecords.Select(z => z.RQMID).ToList().Contains(erExportID))
                {
                    ResultInfo resultInfo;
                    ACL_Data_Contract.Activity currentActivity = GetTestCaseFromActivityGroup(activGroup);
                    try
                    {
                        // check if executionRecordID exist in RQM but still was not updated in business flow XML
                        RQMExecutionRecord currentExecutionRecord = testPlan.RQMExecutionRecords.Where(y => y.RelatedTestCaseRqmID == txExportID && y.RelatedTestScriptRqmID == tsExportID).ToList().FirstOrDefault();
                        if (currentExecutionRecord != null)
                        {
                            erExportID = currentExecutionRecord.RQMID;
                        }
                        else
                        {
                            // if executionRecord not updated and not exists - so create one in RQM and update BussinesFlow object (this may be not saved due not existed "autosave" functionality)
                            resultInfo = RQMConnect.Instance.RQMRep.CreateExecutionRecordPerActivity(loginData, RQMCore.ALMProjectGuid, ALMCore.AlmConfig.ALMProjectName, RQMCore.ALMProjectGroupName, currentActivity, bfExportedID, businessFlow.Name);
                            if (!currentActivity.ExportedTcExecutionRecId.Equals("0"))
                            {
                                string atsID = GetExportedIDString(activGroup.ExternalID, "AtsID");
                                if (atsID == "0")
                                {
                                    atsID = string.Empty;
                                }
                                erExportID = currentActivity.ExportedTcExecutionRecId.ToString();
                                activGroup.ExternalID = "RQMID=" + txExportID + "|RQMScriptID=" + tsExportID + "|RQMRecordID=" + erExportID + "|AtsID=" + atsID;
                                ;
                            }
                        }
                    }
                    catch
                    {
                        Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to create Execution Record Per Activity - " + currentActivity.EntityName);
                    }
                }
                if (string.IsNullOrEmpty(txExportID) || string.IsNullOrEmpty(tsExportID) || string.IsNullOrEmpty(erExportID) || txExportID.Equals("0") || tsExportID.Equals("0") || erExportID.Equals("0"))
                {
                    result = "At " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ": " + businessFlow.Name + " " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + ", is missing ExternalID, cannot export RQM TestPlan execution results without Extrnal ID";
                    return null;
                }
                exeResult.TestCaseExportID = txExportID;
                exeResult.TestScriptExportID = tsExportID;
                exeResult.ExecutionRecordExportID = erExportID;                
                
                int i = 1;
                foreach (Activity act in relevantActivities)
                {
                    ExecutionStep exeStep = new ExecutionStep
                    {
                        StepExpResults = act.Expected,
                        StepOrderId = i,
                        EntityDesc = act.ActivityName,
                    };
                    i++;

                    switch (act.Status)
                    {
                        case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed:
                            exeStep.StepStatus = ACL_Data_Contract.ExecutoinStatus.Failed;
                            string errors = string.Empty;
                            List<Act> failedActs = act.Acts.Where(x => x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed).ToList();
                            foreach (Act action in failedActs) errors += action.Error + Environment.NewLine;
                            exeStep.StepActualResult = errors;
                            break;
                        case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed:
                            exeStep.StepStatus = ACL_Data_Contract.ExecutoinStatus.Passed;
                            exeStep.StepActualResult = "Passed as expected";
                            break;
                        case Amdocs.Ginger.CoreNET.Execution.eRunStatus.NA:
                            exeStep.StepStatus = ACL_Data_Contract.ExecutoinStatus.NA;
                            exeStep.StepActualResult = "NA";
                            break;
                        case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending:
                            exeStep.StepStatus = ACL_Data_Contract.ExecutoinStatus.In_Progress;
                            exeStep.StepActualResult = "Was not executed";
                            break;
                        case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running:
                            exeStep.StepStatus = ACL_Data_Contract.ExecutoinStatus.In_Progress;
                            exeStep.StepActualResult = "Not Completed";
                            break;
                        case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped:
                            exeStep.StepStatus = ACL_Data_Contract.ExecutoinStatus.Outscoped;
                            exeStep.StepActualResult = "Skipped";
                            break;
                        case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked:
                            exeStep.StepStatus = ACL_Data_Contract.ExecutoinStatus.Blocked;
                            exeStep.StepActualResult = "Blocked";
                            break;
                        case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped:
                            exeStep.StepStatus = ACL_Data_Contract.ExecutoinStatus.Inconclusive;
                            exeStep.StepActualResult = "Stopped";
                            break;
                    }
                    exeResult.ExecutionStep.Add(exeStep);                             
                }
                return exeResult;

            }
            catch (Exception ex)
            {
                result = "Unexpected error occurred- " + ex.Message;
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to export execution details to RQM/ALM", ex);
                return null;
            }
        }

        public bool ExportBfActivitiesGroupsToALM(BusinessFlow businessFlow, ObservableList<ActivitiesGroup> grdActivitiesGroups, ref string result)
        {
            LoginDTO loginData = new LoginDTO() { User = ALMCore.AlmConfig.ALMUserName, Password = ALMCore.AlmConfig.ALMPassword, Server = ALMCore.AlmConfig.ALMServerURL };
            //ActivityPlan is TestPlan in RQM and BusinessFlow in Ginger
            List<IActivityPlan> testPlanList = new List<IActivityPlan>(); //1
            ActivityPlan testPlan = new ActivityPlan();
            testPlan.IsPlanDisabled = true;
            testPlanList.Add(testPlan);

            try
            {
                //Create (RQM)TestCase for each Ginger ActivityGroup and add it to RQM TestCase List
                testPlan.Activities = new List<IActivityModel>();//3
                foreach (ActivitiesGroup ag in grdActivitiesGroups)
                {
                    testPlan.Activities.Add(GetTestCaseFromActivityGroup(ag));
                }

                RQMConnect.Instance.RQMRep.GetConection();

                ResultInfo resultInfo;
                resultInfo = RQMConnect.Instance.RQMRep.ExportTestPlan(loginData, testPlanList, ALMCore.AlmConfig.ALMServerURL, RQMCore.ALMProjectGuid, ALMCore.AlmConfig.ALMProjectName, RQMCore.ALMProjectGroupName, null);


                // Deal with response from RQM after export
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
                            string ActivityGroupID = "RQMID=" + act.ExportedID.ToString() + "|RQMScriptID=" + act.ExportedTestScriptId.ToString() + "|RQMRecordID=" + act.ExportedTcExecutionRecId.ToString() + "|AtsID=" + act.EntityId.ToString();
                            businessFlow.ActivitiesGroups[ActivityGroupCounter].ExternalID = ActivityGroupID;

                            foreach (ACL_Data_Contract.ActivityStep activityStep in act.ActivityData.ActivityStepsColl)
                            {
                                string activityStepID = "RQMID=" + activityStepOrderID.ToString() + "|AtsID=" + act.EntityId.ToString();
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
                result = "Failed to export the " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " to RQM/ALM " + ex.Message;
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to export " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " to RQM/ALM", ex);
                return false;
            }

            return true;
        }

        public bool ExportBusinessFlowToRQM(BusinessFlow businessFlow, ObservableList<ExternalItemFieldBase> ExternalItemsFields, ref string result)
        {
            mExternalItemsFields = ExternalItemsFields;
            LoginDTO loginData = new LoginDTO() { User = ALMCore.AlmConfig.ALMUserName, Password = ALMCore.AlmConfig.ALMPassword, Server = ALMCore.AlmConfig.ALMServerURL };

            //ActivityPlan is TestPlan in RQM and BusinessFlow in Ginger
            List<IActivityPlan> testPlanList = new List<IActivityPlan>(); //1
            ActivityPlan testPlan = GetTestPlanFromBusinessFlow(businessFlow);
            testPlanList.Add(testPlan);//2

            if (businessFlow.ActivitiesGroups.Count == 0)
            {
                throw new Exception(GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " must have at least one " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup));
            }

            ResultInfo resultInfo;
            try
            {
                //Create (RQM)TestCase for each Ginger ActivityGroup and add it to RQM TestCase List
                testPlan.Activities = new List<IActivityModel>();//3
                foreach (ActivitiesGroup ag in businessFlow.ActivitiesGroups)
                {
                    testPlan.Activities.Add(GetTestCaseFromActivityGroup(ag));
                }

                RQMConnect.Instance.RQMRep.GetConection();

                resultInfo = RQMConnect.Instance.RQMRep.ExportTestPlan(loginData, testPlanList, ALMCore.AlmConfig.ALMServerURL, RQMCore.ALMProjectGuid, ALMCore.AlmConfig.ALMProjectName, RQMCore.ALMProjectGroupName, null);
            }
            catch (Exception ex)
            {
                result = "Failed to export the Business Flow to RQM/ALM " + ex.Message;
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to export the Business Flow to RQM/ALM", ex);
                return false;
            }

            // Deal with response from RQM after export
            // 0 = sucsess , 1 = failed 
            if (resultInfo.ErrorCode == 0)
            {
                foreach (ActivityPlan plan in testPlanList)
                {
                    businessFlow.ExternalID = "RQMID=" + plan.ExportedID.ToString();
                    int ActivityGroupCounter = 0;
                    int activityStepCounter = 0;
                    int activityStepOrderID = 1;
                    foreach (ACL_Data_Contract.Activity act in plan.Activities)
                    {
                        string ActivityGroupID = "RQMID=" + act.ExportedID.ToString() + "|RQMScriptID=" + act.ExportedTestScriptId.ToString() + "|RQMRecordID=" + act.ExportedTcExecutionRecId.ToString() + "|AtsID=" + act.EntityId.ToString();
                        businessFlow.ActivitiesGroups[ActivityGroupCounter].ExternalID = ActivityGroupID;

                        foreach (ACL_Data_Contract.ActivityStep activityStep in act.ActivityData.ActivityStepsColl)
                        {
                            //string activityStepID = "RQMID=" + activityStepOrderID.ToString() + "|AtsID=" + act.EntityId.ToString();
                            string activityStepID = "RQMID=" + act.ExportedTestScriptId.ToString() + "_" + activityStepOrderID + "|AtsID=" + act.EntityId.ToString();
                            businessFlow.Activities[activityStepCounter].ExternalID = activityStepID;
                            activityStepCounter++;
                            activityStepOrderID++;
                        }
                        activityStepOrderID = 0;
                        ActivityGroupCounter++;
                    }
                }
                return true;
            }
            else
            {
                result = "Failed to export the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " to RQM/ALM, " + resultInfo.ErrorDesc;
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to export the Business Flow to RQM/ALM, " + resultInfo.ErrorDesc);
                return false;
            }
        }

        private ActivityPlan GetTestPlanFromBusinessFlow(BusinessFlow businessFlow)
        {
            ActivityPlan testPlan = new ActivityPlan();
            //Check if updating or creating new instance in RQM
            if (!string.IsNullOrEmpty(businessFlow.ExternalID))
            {
                try
                {
                    long rqmID = Convert.ToInt64(GetExportedIDString(businessFlow.ExternalID, "RQMID"));
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
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}");
                }
            }

            testPlan.EntityName = businessFlow.Name;
            testPlan.EntityDesc = businessFlow.Description == null ? "" : businessFlow.Description;

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
            if (!string.IsNullOrEmpty(activityGroup.ExternalID))
            {
                try
                {
                    long RQMID = Convert.ToInt64(GetExportedIDString(activityGroup.ExternalID, "RQMID"));
                    long RQMScriptID = Convert.ToInt64(GetExportedIDString(activityGroup.ExternalID, "RQMScriptID"));
                    long RQMRecordID = Convert.ToInt64(GetExportedIDString(activityGroup.ExternalID, "RQMRecordID"));

                    testCase.ExportedID = RQMID;
                    testCase.ExportedTestScriptId = RQMScriptID;
                    testCase.ExportedTcExecutionRecId = RQMRecordID;
                    testCase.ShouldUpdated = true;
                }
                catch (Exception e)
                {
                    testCase.ShouldUpdated = false;
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}");
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
                        Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}");
                    }
                }

                if (actIden == null || actIden.IdentifiedActivity == null)
                {
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error: When Exporting to RQM, ActivityIdentifiers object or actIden.IdentifiedActivity is null and cannot export to RQM");
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
                    ActivityParam param = new ActivityParam();
                    param.EntityName = variable.FileName;
                    param.DefaultValue = variable.Value;
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
            return "<<<" + variableName + "&?&" + variableValue + ">>>";
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
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                //No matches found
                return "0";
            }
        }

        private Dictionary<string, string> GetCustomProperties(string ItemType)
        {
            Dictionary<string, string> propertiesList = new Dictionary<string, string>();
            if (mExternalItemsFields.Count > 0)
            {
                foreach (ExternalItemFieldBase itemField in mExternalItemsFields)
                {
                    if (itemField.ItemType == ItemType)
                        if (itemField.Mandatory == true || itemField.ToUpdate == true)
                            propertiesList.Add(itemField.Name, itemField.SelectedValue);
                }
            }
            else
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Could not export to RQM, External Items Fields values are missing");
            }
            return propertiesList;
        }
    }
}
