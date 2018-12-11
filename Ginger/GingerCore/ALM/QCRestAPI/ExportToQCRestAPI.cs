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

using ALM_Common.DataContracts;
using ALMRestClient;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.IO;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Variables;
using QCRestClient;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Reflection;

namespace GingerCore.ALM.QCRestAPI
{
    public static class ExportToQCRestAPI
    {

        #region public Methods
        /// <summary>
        /// Export Activities Group details to QC, can be used for creating new matching QC Test Case or updating an existing one
        /// </summary>
        /// <param name="activitiesGroup">Activities Group to Export</param>
        /// <param name="mappedTest">The QC Test Case which mapped to the Activities Group (in case exist) and needs to be updated</param>
        /// <param name="uploadPath">Upload path in QC Test Plan</param>
        /// <param name="result">Export error result</param>
        /// <returns></returns>
        public static bool ExportActivitiesGroupToQC(ActivitiesGroup activitiesGroup, QCTestCase mappedTest, string uploadPath, ObservableList<ExternalItemFieldBase> testCaseFields, ObservableList<ExternalItemFieldBase> designStepsFields, ObservableList<ExternalItemFieldBase> designStepsParamsFields, ref string result)
        {
            try
            {
                QCTestCase test = null;

                if (mappedTest == null) //#Create new test case
                {
                    test = CreateNewTestCase(activitiesGroup, uploadPath, testCaseFields);

                    int order = 1;
                    foreach (ActivityIdentifiers actIdent in activitiesGroup.ActivitiesIdentifiers)
                        CreateTestStep(test, actIdent.IdentifiedActivity, designStepsFields, designStepsParamsFields, order++);
                }
                else //##update existing test case
                {
                    test = UpdateExistingTestCase(mappedTest, activitiesGroup, testCaseFields);
                    UpdateTestSteps(test, activitiesGroup, designStepsFields, designStepsParamsFields);
                }

                return true;
            }
            catch (Exception ex)
            {
                result = "Unexpected error occurred- " + ex.Message;
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to export the Activities Group to QC/ALM", ex);
                return false;
            }
        }

        public static bool ExportBusinessFlowToQC(BusinessFlow businessFlow, QCTestSet mappedTestSet, string uploadPath, ObservableList<ExternalItemFieldBase> testSetFields, ObservableList<ExternalItemFieldBase> testInstanceFields, ref string result)
        {
            QCTestSet testSet = null;
            ObservableList<ActivitiesGroup> existingActivitiesGroups = new ObservableList<ActivitiesGroup>();

            try
            {
                if (mappedTestSet == null) //##create new Test Set in QC
                {
                    testSet = CreateNewTestSet(businessFlow, uploadPath, testSetFields);
                    CreateNewTestInstances(businessFlow, existingActivitiesGroups, testSet, testInstanceFields);
                }
                else //##update existing test set
                {
                    testSet = UpdateExistingTestSet(businessFlow, mappedTestSet, uploadPath, testSetFields);
                    UpdateTestInstances(businessFlow, existingActivitiesGroups, testSet, testInstanceFields);
                }

                businessFlow.ExternalID = testSet.Id.ToString();
                return true;
            }
            catch (Exception ex)
            {
                result = "Unexpected error occurred- " + ex.Message;
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to export the Business Flow to QC/ALM", ex);
                return false;
            }
        }

        public static bool ExportExceutionDetailsToALM(BusinessFlow bizFlow, ref string result, ObservableList<ExternalItemFieldBase> runFields, bool exectutedFromAutomateTab, PublishToALMConfig publishToALMConfig = null)
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
                QCTestSet testSet = ImportFromQCRest.GetQCTestSet(bizFlow.ExternalID);//bf.externalID holds the TestSet TSTests collection id
                if (testSet != null)
                {
                    //get the Test set TC's
                    QCTestInstanceColl qcTSTests = QCRestAPIConnect.GetTestInstancesOfTestSet(testSet.Id); //list of TSTest's on main TestSet in TestLab 

                    //get all BF Activities groups
                    ObservableList<ActivitiesGroup> activGroups = bizFlow.ActivitiesGroups;
                    if (activGroups.Count > 0)
                    {
                        foreach (ActivitiesGroup activGroup in activGroups)
                        {
                            if ((publishToALMConfig.FilterStatus == FilterByStatus.OnlyPassed && activGroup.RunStatus == ActivitiesGroup.eActivitiesGroupRunStatus.Passed)
                            || (publishToALMConfig.FilterStatus == FilterByStatus.OnlyFailed && activGroup.RunStatus == ActivitiesGroup.eActivitiesGroupRunStatus.Failed)
                            || publishToALMConfig.FilterStatus == FilterByStatus.All)
                            {
                                QCTestInstance tsTest = null;
                                //go by TC ID = TC Instances ID
                                tsTest = qcTSTests.Find(x => x.TestId == activGroup.ExternalID && x.Id == activGroup.ExternalID2);
                                if (tsTest == null)
                                {
                                    //go by Linked TC ID + TC Instances ID
                                    tsTest = qcTSTests.Find(x => ImportFromQCRest.GetTSTestLinkedID(x) == activGroup.ExternalID && x.Id == activGroup.ExternalID2);
                                }
                                if (tsTest == null)
                                {
                                    //go by TC ID 
                                    tsTest = qcTSTests.Find(x => x.TestId == activGroup.ExternalID);
                                }
                                if (tsTest != null)
                                {
                                    //get activities in group
                                    List<Activity> activities = (bizFlow.Activities.Where(x => x.ActivitiesGroupID == activGroup.Name)).Select(a => a).ToList();
                                    string TestCaseName = PathHelper.CleanInValidPathChars(tsTest.Name);
                                    if ((publishToALMConfig.VariableForTCRunName == null) || (publishToALMConfig.VariableForTCRunName == string.Empty))
                                    {
                                        String timeStamp = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss");
                                        publishToALMConfig.VariableForTCRunName = "GingerRun_" + timeStamp;
                                    }

                                    QCRun runToExport = new QCRun();

                                    foreach (ExternalItemFieldBase field in runFields)
                                    {
                                        if (field.ToUpdate || field.Mandatory)
                                        {
                                            if (string.IsNullOrEmpty(field.SelectedValue) == false && field.SelectedValue != "NA")
                                                runToExport.ElementsField.Add(field.ExternalID, field.SelectedValue);
                                            else
                                                try { runToExport.ElementsField.Add(field.ExternalID, "NA"); }
                                                catch { }
                                        }
                                    }

                                    runToExport.ElementsField["name"] = publishToALMConfig.VariableForTCRunNameCalculated;
                                    runToExport.ElementsField["test-id"] = tsTest.TestId;
                                    runToExport.ElementsField["testcycl-id"] = tsTest.Id;
                                    runToExport.ElementsField["cycle-id"] = tsTest.CycleId;
                                    runToExport.ElementsField["duration"] = "0";
                                    runToExport.ElementsField["subtype-id"] = "hp.qc.run.MANUAL";
                                    runToExport.ElementsField["owner"] = ALMCore.AlmConfig.ALMUserName;

                                    QCItem itemToExport = ConvertObjectValuesToQCItem(runToExport, ResourceType.TEST_RUN);
                                    ALMResponseData responseData = QCRestAPIConnect.CreateNewEntity(ResourceType.TEST_RUN, itemToExport);
                                    if (!responseData.IsSucceed)
                                    {
                                        result = "Failed to create run using rest API";
                                        return false;
                                    }
                                    QCRun currentRun = QCRestAPIConnect.GetRunDetail(responseData.IdCreated);

                                    // Attach ActivityGroup Report if needed
                                    if (publishToALMConfig.ToAttachActivitiesGroupReport)
                                    {
                                        if ((activGroup.TempReportFolder != null) && (activGroup.TempReportFolder != string.Empty) &&
                                            (System.IO.Directory.Exists(activGroup.TempReportFolder)))
                                        {
                                            //Creating the Zip file - start
                                            string targetZipPath = System.IO.Directory.GetParent(activGroup.TempReportFolder).ToString();
                                            string zipFileName = targetZipPath + "\\" + TestCaseName.ToString() + "_GingerHTMLReport.zip";

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
                                            
                                            ALMResponseData attachmentResponse = QCRestAPIConnect.CreateAttachment(ResourceType.TEST_RUN, currentRun.Id, zipFileName);

                                            if (!attachmentResponse.IsSucceed)
                                            {
                                                result = "Failed to create attachment";
                                                return false;
                                            }

                                            System.IO.File.Delete(zipFileName);
                                        }
                                    }


                                    //create run with activities as steps
                                    QCRunStepColl runSteps = ImportFromQCRest.GetRunSteps(currentRun.Id);

                                    int index = 1;
                                    foreach (QCRunStep runStep in runSteps)
                                    {
                                        //search for matching activity based on ID and not order, un matching steps need to be left as No Run
                                        string stepName = runStep.Name;
                                        Activity matchingActivity = activities.Where(x => x.ExternalID == runStep.ElementsField["desstep-id"].ToString()).FirstOrDefault();
                                        if (matchingActivity != null)
                                        {
                                            switch (matchingActivity.Status)
                                            {
                                                case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed:
                                                    runStep.Status = "Failed";
                                                    List<Act> failedActs = matchingActivity.Acts.Where(x => x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed).ToList();
                                                    string errors = string.Empty;
                                                    foreach (Act act in failedActs) errors += act.Error + Environment.NewLine;
                                                    runStep.Actual = errors;
                                                    break;
                                                case Amdocs.Ginger.CoreNET.Execution.eRunStatus.NA:
                                                    runStep.ElementsField["status"] = "N/A";
                                                    runStep.ElementsField["actual"] = "NA";
                                                    break;
                                                case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed:
                                                    runStep.ElementsField["status"] = "Passed";
                                                    runStep.ElementsField["actual"] = "Passed as expected";
                                                    break;
                                                case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped:
                                                    runStep.ElementsField["status"] = "N/A";
                                                    runStep.ElementsField["actual"] = "Skipped";
                                                    break;
                                                case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending:
                                                    runStep.ElementsField["status"] = "No Run";
                                                    runStep.ElementsField["actual"] = "Was not executed";
                                                    break;
                                                case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running:
                                                    runStep.ElementsField["status"] = "Not Completed";
                                                    runStep.ElementsField["actual"] = "Not Completed";
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            //Step not exist in Ginger so left as "No Run" unless it is step data
                                            if (runStep.Name.ToUpper() == "STEP DATA")
                                                runStep.ElementsField["status"] = "Passed";
                                            else
                                                runStep.ElementsField["status"] = "No Run";
                                        }

                                        QCItem stepToUpdate = ConvertObjectValuesToQCItem(runStep, ResourceType.RUN_STEP);
                                        ALMResponseData stepDataForUpdate = QCRestAPIConnect.UpdateEntity(ResourceType.RUN_STEP, runStep.Id, stepToUpdate);

                                        index++;
                                    }

                                    //get all execution status for all steps
                                    ObservableList<string> stepsStatuses = new ObservableList<string>();
                                    foreach (QCRunStep runStep in runSteps)
                                        stepsStatuses.Add(runStep.Status);

                                    //update the TC general status based on the activities status collection.                                
                                    if (stepsStatuses.Where(x => x == "Failed").Count() > 0)
                                        currentRun.Status = "Failed";
                                    else if (stepsStatuses.Where(x => x == "No Run").Count() == runSteps.Count || stepsStatuses.Where(x => x == "N/A").Count() == runSteps.Count)
                                        currentRun.Status = "No Run";
                                    else if (stepsStatuses.Where(x => x == "Passed").Count() == runSteps.Count || (stepsStatuses.Where(x => x == "Passed").Count() + stepsStatuses.Where(x => x == "N/A").Count()) == runSteps.Count)
                                        currentRun.ElementsField["status"] = "Passed";
                                    else
                                        currentRun.ElementsField["status"] = "Not Completed";

                                    QCItem runToUpdate = ConvertObjectValuesToQCItem(currentRun, ResourceType.TEST_RUN);
                                    ALMResponseData runDataForUpdate = QCRestAPIConnect.UpdateEntity(ResourceType.TEST_RUN, currentRun.Id, runToUpdate);
                                }
                                else
                                {
                                    //No matching TC was found for the ActivitiesGroup in QC
                                    result = "Matching TC's were not found for all " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " in QC/ALM.";
                                }
                            }
                            if (result != string.Empty)
                                return false;
                        }
                    }
                    else
                    {
                        //No matching Test Set was found for the BF in QC
                        result = "No matching Test Set was found in QC/ALM.";
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
            }
            catch (Exception ex)
            {
                result = "Unexpected error occurred- " + ex.Message;
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to export execution details to QC/ALM", ex);
                return false;
            }

            return false; // Remove it at the end

        }

        #endregion Public Methods

        #region private methods
        private static QCTestCase CreateNewTestCase(ActivitiesGroup activitiesGroup, string uploadPath, ObservableList<ExternalItemFieldBase> testCaseFields)
        {
            QCTestCase test = new QCTestCase();

            test.ElementsField["subtype-id"] = "MANUAL";
            test.ElementsField["parent-id"] = QCRestAPIConnect.GetLastTestPlanIdFromPath(uploadPath).ToString();

            //set item fields
            foreach (ExternalItemFieldBase field in testCaseFields)
            {
                if (field.ToUpdate || field.Mandatory)
                {
                    if (string.IsNullOrEmpty(field.SelectedValue) == false && field.SelectedValue != "NA")
                        test.ElementsField.Add(field.ExternalID, field.SelectedValue);
                    else
                        try { test.ElementsField.Add(field.ExternalID, "NA"); }
                        catch { }
                }
            }

            //post the test
            test.ElementsField["name"] = activitiesGroup.Name;
            test.ElementsField["description"] = activitiesGroup.Description;
            QCItem item = ConvertObjectValuesToQCItem(test, ResourceType.TEST_CASE);
            ALMResponseData response = QCRestAPIConnect.CreateNewEntity(ResourceType.TEST_CASE, item);

            test.Id = response.IdCreated;
            activitiesGroup.ExternalID = test.Id;
            activitiesGroup.ExternalID2 = test.Id;

            return QCRestAPIConnect.GetTestCases(new List<string> { test.Id })[0];
        }

        private static QCTestSet CreateNewTestSet(BusinessFlow businessFlow, string uploadPath, ObservableList<ExternalItemFieldBase> testSetFields)
        {
            QCTestSet testSet = new QCTestSet();

            //set the upload path
            testSet.ElementsField["parent-id"] = QCRestAPIConnect.GetLastTestSetIdFromPath(uploadPath).ToString();

            //set item fields for test set
            foreach (ExternalItemFieldBase field in testSetFields)
            {
                if (field.ToUpdate || field.Mandatory)
                {
                    if (string.IsNullOrEmpty(field.ExternalID) == false && field.SelectedValue != "NA")
                        testSet.ElementsField[field.ExternalID] = field.SelectedValue;
                    else
                        try { testSet.ElementsField[field.ID] = "NA"; }
                        catch { }
                }
            }

            testSet.ElementsField["name"] = businessFlow.Name;
            testSet.ElementsField["subtype-id"] = "hp.qc.test-set.default";

            try
            {
                QCItem item = ConvertObjectValuesToQCItem(testSet, ResourceType.TEST_SET);
                ALMResponseData response = QCRestAPIConnect.CreateNewEntity(ResourceType.TEST_SET, item);
                return QCRestAPIConnect.GetTestSetDetails(response.IdCreated);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("The Test Set already exists"))
                {
                    string result = "Cannot export Business Flow - The Test Set already exists in the selected folder. ";
                    Reporter.ToLog(eAppReporterLogLevel.ERROR, result, ex);
                    return null;
                }

                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return null;
            }
        }

        private static bool CreateTestStep(QCTestCase test, Activity activity, ObservableList<ExternalItemFieldBase> designStepsFields, ObservableList<ExternalItemFieldBase> designStepsParamsFields, int stepOrder)
        {

            //create new step
            QCTestCaseStep step = new QCTestCaseStep();

            //set item fields
            foreach (ExternalItemFieldBase field in designStepsFields)
            {
                if (field.ToUpdate || field.Mandatory)
                {
                    if (string.IsNullOrEmpty(field.SelectedValue) == false && field.SelectedValue != "NA")
                        step.ElementsField.Add(field.ExternalID, field.SelectedValue);
                    else
                        try { step.ElementsField.Add(field.ExternalID, "NA"); }
                        catch { }
                }
            }

            step.ElementsField["name"] = activity.ActivityName;
            step.ElementsField["parent-id"] = test.Id;
            step.ElementsField["step-order"] = stepOrder.ToString();

            string descriptionTemplate =
                    "<html><body><div align=\"left\"><font face=\"Arial\"><span style=\"font-size:8pt\"><<&Description&&>><br /><<&Parameters&>><br /><<&Actions&>></span></font></div></body></html>";
            string description = descriptionTemplate.Replace("<<&Description&&>>", activity.Description);
            QCTestCaseParamsColl testParams = QCRestAPIConnect.GetTestCaseParams(test.Id);
            string paramsSigns = string.Empty;
            if (activity.Variables.Count > 0)
            {
                paramsSigns = "<br />Parameters:<br />";
                foreach (VariableBase var in activity.Variables)
                {
                    paramsSigns += "&lt;&lt;&lt;" + var.Name.ToLower() + "&gt;&gt;&gt;<br />";
                    //try to add the parameter to the test case parameters list
                    try
                    {
                        QCTestCaseParam newParam = new QCTestCaseParam();

                        //set item fields
                        foreach (ExternalItemFieldBase field in designStepsParamsFields)
                        {
                            if (field.ToUpdate || field.Mandatory)
                            {
                                if (string.IsNullOrEmpty(field.SelectedValue) == false && field.SelectedValue != "NA")
                                    newParam.ElementsField.Add(field.ExternalID, field.SelectedValue);
                                else
                                    try { newParam.ElementsField.Add(field.ExternalID, "NA"); }
                                    catch { }
                            }
                        }

                        newParam.Name = var.Name.ToLower();
                        newParam.TestId = test.Id;

                        QCItem itemTestCaseParam = ConvertObjectValuesToQCItem(newParam, ResourceType.TEST_CASE_PARAMETERS);
                        QCRestAPIConnect.CreateNewEntity(ResourceType.TEST_CASE_PARAMETERS, itemTestCaseParam);
                    }
                    catch (Exception ex) { Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
                }
            }
            description = description.Replace("<<&Parameters&>>", paramsSigns);

            string actsDesc = string.Empty;
            if (activity.Acts.Count > 0)
            {
                actsDesc = "Actions:<br />";
                foreach (Act act in activity.Acts)
                    actsDesc += act.Description + "<br />";
            }
            description = description.Replace("<<&Actions&>>", actsDesc);
            step.Description = description;
            step.ElementsField["expected"] = activity.Expected;

            QCItem itemDesignStep = ConvertObjectValuesToQCItem(step, ResourceType.DESIGN_STEP);
            ALMResponseData response = QCRestAPIConnect.CreateNewEntity(ResourceType.DESIGN_STEP, itemDesignStep);

            activity.ExternalID = response.IdCreated;

            if (activity.ExternalID != null)
                return true;
            else
                return false;
        }

        private static void CreateNewTestInstances(BusinessFlow businessFlow, ObservableList<ActivitiesGroup> existingActivitiesGroups, QCTestSet testSet, ObservableList<ExternalItemFieldBase> testInstancesFields)
        {
            int counter = 1;
            foreach (ActivitiesGroup ag in businessFlow.ActivitiesGroups)
            {
                if (existingActivitiesGroups.Contains(ag) == false && string.IsNullOrEmpty(ag.ExternalID) == false && ImportFromQCRest.GetQCTest(ag.ExternalID) != null)
                {
                    QCTestInstance testInstance = new QCTestInstance
                    {
                        TestId = ag.ExternalID,
                        CycleId = testSet.Id,
                        TestOrder = counter++.ToString(),
                    };

                    //set item fields for test instances
                    foreach (ExternalItemFieldBase field in testInstancesFields)
                    {
                        if ((field.ToUpdate || field.Mandatory) && (!(field.ExternalID == "test-id") && !(field.ExternalID == "cycle-id")))
                        {
                            if (string.IsNullOrEmpty(field.ExternalID) == false && field.SelectedValue != "NA")
                                testInstance.ElementsField[field.ExternalID] = field.SelectedValue;
                            else
                                try { testInstance.ElementsField[field.ID] = "NA"; }
                                catch { }
                        }
                    }

                    testInstance.ElementsField["subtype-id"] = "hp.qc.test-instance.MANUAL";
                    QCItem item = ConvertObjectValuesToQCItem(testInstance, ResourceType.TEST_CYCLE);
                    ALMResponseData response = QCRestAPIConnect.CreateNewEntity(ResourceType.TEST_CYCLE, item);

                    if (response.IsSucceed)
                    {
                        ag.ExternalID2 = response.IdCreated; //the test case instance ID in the test set- used for exporting the execution details
                    }
                }
            }
        }

        private static void UpdateTestSteps(QCTestCase test, ActivitiesGroup activitiesGroup, ObservableList<ExternalItemFieldBase> designStepsFields, ObservableList<ExternalItemFieldBase> designStepsParamsFields)
        {
            QCTestCaseStepsColl testCaseDesignStep = QCRestAPIConnect.GetTestCasesSteps(new List<string> { test.Id });

            // Add new steps
            for (int i = 0; i < activitiesGroup.ActivitiesIdentifiers.Count; i++)
            {
                if(activitiesGroup.ActivitiesIdentifiers[i].ActivityExternalID == null)
                {
                    CreateTestStep(test, activitiesGroup.ActivitiesIdentifiers[i].IdentifiedActivity, designStepsFields, designStepsParamsFields, i + 1);
                }
            }

            //delete the un-needed steps
            foreach (QCTestCaseStep step in testCaseDesignStep.Reverse<QCTestCaseStep>())
            {
                if (!activitiesGroup.ActivitiesIdentifiers.Any(x => x.ActivityExternalID == step.Id))
                {
                    QCRestAPIConnect.DeleteEntity(ResourceType.DESIGN_STEP, step.Id);
                    testCaseDesignStep.Remove(step);
                } 
            }

            //delete the existing parameters
            QCTestCaseParamsColl testCaseParams = QCRestAPIConnect.GetTestCaseParams(test.Id);

            if (testCaseParams.Count > 0)
            {
                for (int indx = 0; indx < testCaseParams.Count; indx++)
                {
                    QCRestAPIConnect.DeleteEntity(ALM_Common.DataContracts.ResourceType.DESIGN_STEP_PARAMETERS, testCaseParams[indx].Id);
                }
            }

            foreach (QCTestCaseStep step in testCaseDesignStep)
            {
                Activity identifiedActivity = activitiesGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.Id).FirstOrDefault().IdentifiedActivity;
                //set item fields
                foreach (ExternalItemFieldBase field in designStepsFields)
                {
                    if (field.ToUpdate || field.Mandatory)
                    {
                        if (string.IsNullOrEmpty(field.SelectedValue) == false && field.SelectedValue != "NA")
                        {
                            if (step.ElementsField.ContainsKey(field.ExternalID))
                                step.ElementsField[field.ExternalID] = field.SelectedValue;
                        }
                    }
                }

                step.ElementsField["name"] = identifiedActivity.ActivityName;
                string descriptionTemplate =
                    "<html><body><div align=\"left\"><font face=\"Arial\"><span style=\"font-size:8pt\"><<&Description&&>><br /><<&Parameters&>><br /><<&Actions&>></span></font></div></body></html>";
                string description = descriptionTemplate.Replace("<<&Description&&>>", identifiedActivity.Description);

                QCTestCaseParamsColl testParams = QCRestAPIConnect.GetTestCaseParams(test.Id);
                string paramsSigns = string.Empty;
                if (identifiedActivity.Variables.Count > 0)
                {
                    paramsSigns = "<br />Parameters:<br />";
                    foreach (VariableBase var in identifiedActivity.Variables)
                    {
                        paramsSigns += "&lt;&lt;&lt;" + var.Name.ToLower() + "&gt;&gt;&gt;<br />";
                        //try to add the parameter to the test case parameters list
                        try
                        {
                            QCTestCaseParam newParam = new QCTestCaseParam();

                            //set item fields
                            foreach (ExternalItemFieldBase field in designStepsParamsFields)
                            {
                                if (field.ToUpdate || field.Mandatory)
                                {
                                    if (string.IsNullOrEmpty(field.SelectedValue) == false && field.SelectedValue != "NA")
                                        newParam.ElementsField.Add(field.ExternalID, field.SelectedValue);
                                    else
                                        try { newParam.ElementsField.Add(field.ExternalID, "NA"); }
                                        catch { }
                                }
                            }

                            newParam.Name = var.Name.ToLower();
                            newParam.TestId = test.Id;

                            QCItem itemTestCaseParam = ConvertObjectValuesToQCItem(newParam, ResourceType.TEST_CASE_PARAMETERS);
                            QCRestAPIConnect.CreateNewEntity(ResourceType.TEST_CASE_PARAMETERS, itemTestCaseParam);
                        }
                        catch (Exception ex) { Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
                    }
                }

                description = description.Replace("<<&Parameters&>>", paramsSigns);

                string actsDesc = string.Empty;
                if (identifiedActivity.Acts.Count > 0)
                {
                    actsDesc = "Actions:<br />";
                    foreach (Act act in identifiedActivity.Acts)
                        actsDesc += act.Description + "<br />";
                }
                description = description.Replace("<<&Actions&>>", actsDesc);
                step.Description = description;
                step.ElementsField["expected"] = identifiedActivity.Expected;

                QCItem itemDesignStep = ConvertObjectValuesToQCItem(step, ResourceType.DESIGN_STEP, true);
                ALMResponseData response = QCRestAPIConnect.UpdateEntity(ResourceType.DESIGN_STEP, step.Id, itemDesignStep);

                identifiedActivity.ExternalID = step.Id;
            }
        }

        private static QCTestCase UpdateExistingTestCase(QCTestCase mappedTest, ActivitiesGroup activitiesGroup, ObservableList<ExternalItemFieldBase> testCaseFields)
        {
            QCTestCase test = mappedTest;

            //set item fields
            foreach (ExternalItemFieldBase field in testCaseFields)
            {
                if (field.ToUpdate || field.Mandatory)
                {
                    if (string.IsNullOrEmpty(field.SelectedValue) == false && field.SelectedValue != "NA")
                    {
                        if (test.ElementsField.ContainsKey(field.ExternalID))
                            test.ElementsField[field.ExternalID] = field.SelectedValue;
                    }

                    else
                        try { test.ElementsField.Add(field.ExternalID, "NA"); }
                        catch { }
                }
            }

            //update the test
            test.ElementsField["name"] = activitiesGroup.Name;
            test.ElementsField["description"] = activitiesGroup.Description;
            QCItem item = ConvertObjectValuesToQCItem(test, ResourceType.TEST_CASE, true);
            ALMResponseData response = QCRestAPIConnect.UpdateEntity(ResourceType.TEST_CASE, test.Id, item);

            activitiesGroup.ExternalID = test.Id;
            //activitiesGroup.ExternalID2 = test.Id; TODO: Check if it's good

            return test;
        }

        private static QCTestSet UpdateExistingTestSet(BusinessFlow businessFlow, QCTestSet mappedTestSet, string uploadPath, ObservableList<ExternalItemFieldBase> testSetFields)
        {
            QCTestSet testSet = ImportFromQCRest.GetQCTestSet(mappedTestSet.Id.ToString());

            //set item fields for test set
            foreach (ExternalItemFieldBase field in testSetFields)
            {
                if (field.ToUpdate || field.Mandatory)
                {
                    if (string.IsNullOrEmpty(field.ExternalID) == false && field.SelectedValue != "NA")
                        if (testSet.ElementsField.ContainsKey(field.ID))
                        {
                            testSet.ElementsField[field.ExternalID] = field.SelectedValue;
                        }
                }
            }

            testSet.ElementsField["name"] = businessFlow.Name;

            try
            {
                QCItem item = ConvertObjectValuesToQCItem(testSet, ResourceType.TEST_SET, true);
                ALMResponseData response = QCRestAPIConnect.UpdateEntity(ResourceType.TEST_SET, testSet.Id, item);
                return QCRestAPIConnect.GetTestSetDetails(testSet.Id);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return null;
            }
        }

        private static void UpdateTestInstances(BusinessFlow businessFlow, ObservableList<ActivitiesGroup> existingActivitiesGroups, QCTestSet testSet, ObservableList<ExternalItemFieldBase> testInstancesFields)
        {
            QCTestInstanceColl testInstances = ImportFromQCRest.ImportTestSetInstanceData(testSet);

            foreach (QCTestInstance testInstance in testInstances)
            {
                ActivitiesGroup ag = businessFlow.ActivitiesGroups.Where(x => (x.ExternalID == testInstance.TestId.ToString() && x.ExternalID2 == testInstance.Id.ToString())).FirstOrDefault();
                if (ag == null)
                    QCRestAPIConnect.DeleteEntity(ResourceType.TEST_CYCLE, testInstance.Id);
                else
                {
                    existingActivitiesGroups.Add(ag);
                    //set item fields for test instances
                    foreach (ExternalItemFieldBase field in testInstancesFields)
                    {
                        if ((field.ToUpdate || field.Mandatory) && (!(field.ExternalID == "test-id") && !(field.ExternalID == "cycle-id")))
                        {
                            if (string.IsNullOrEmpty(field.ExternalID) == false && field.SelectedValue != "NA")
                            {
                                if (testInstance.ElementsField.ContainsKey(field.ID))
                                {
                                    testInstance.ElementsField[field.ExternalID] = field.SelectedValue;
                                }
                            }
                        }
                    }

                    try
                    {
                        QCItem item = ConvertObjectValuesToQCItem(testInstance, ResourceType.TEST_CYCLE, true);
                        ALMResponseData response = QCRestAPIConnect.UpdateEntity(ResourceType.TEST_CYCLE, testInstance.Id, item);
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                    }
                }
            }
        }

        private static QCItem ConvertObjectValuesToQCItem(object item, ResourceType type, bool isUpdate = false)
        {
            QCItem itemWithValues = new QCItem();

            dynamic itemVals = item;

            foreach (var keyValue in itemVals.ElementsField)
            {
                if (keyValue.Key != "order-id")
                    itemWithValues.Fields.Add(keyValue.Key, keyValue.Value);
            }

            if (itemVals.GetType().GetProperty("Id") != null)
            {
                // Do nothing
            }
            if (itemVals.GetType().GetProperty("ParentId") != null && itemVals.GetType().GetProperty("ParentId").GetValue(itemVals, null) != null)
            {
                itemWithValues.Fields.Add("parent-id", itemVals.GetType().GetProperty("ParentId").GetValue(itemVals, null));
            }
            if (itemVals.GetType().GetProperty("Description") != null && itemVals.GetType().GetProperty("Description").GetValue(itemVals, null) != null && !isUpdate)
            {
                itemWithValues.Fields.Add("description", EscapeChars(itemVals.GetType().GetProperty("Description").GetValue(itemVals, null)));
            }
            if (itemVals.GetType().GetProperty("Name") != null && itemVals.GetType().GetProperty("Name").GetValue(itemVals, null) != null && !isUpdate)
            {
                itemWithValues.Fields.Add("name", EscapeChars(itemVals.GetType().GetProperty("Name").GetValue(itemVals, null)));
            }
            if (itemVals.GetType().GetProperty("Path") != null && itemVals.GetType().GetProperty("Path").GetValue(itemVals, null) != null && !isUpdate)
            {
                if (!(type == ResourceType.TEST_SET))
                    itemWithValues.Fields.Add("hierarchical-path", itemVals.GetType().GetProperty("Path").GetValue(itemVals, null));
            }
            if (itemVals.GetType().GetProperty("DefualtValue") != null && itemVals.GetType().GetProperty("DefualtValue").GetValue(itemVals, null) != null)
            {
                itemWithValues.Fields.Add("default-value", itemVals.GetType().GetProperty("DefualtValue").GetValue(itemVals, null));
            }
            if (itemVals.GetType().GetProperty("ActualValue") != null && itemVals.GetType().GetProperty("ActualValue").GetValue(itemVals, null) != null)
            {
                itemWithValues.Fields.Add("actual-value", itemVals.GetType().GetProperty("ActualValue").GetValue(itemVals, null));
            }
            if (itemVals.GetType().GetProperty("TestOrder") != null && itemVals.GetType().GetProperty("TestOrder").GetValue(itemVals, null) != null && !isUpdate)
            {
                itemWithValues.Fields.Add("order-id", itemVals.GetType().GetProperty("TestOrder").GetValue(itemVals, null));
            }
            if (itemVals.GetType().GetProperty("TestId") != null && itemVals.GetType().GetProperty("TestId").GetValue(itemVals, null) != null && !isUpdate)
            {
                itemWithValues.Fields.Add("test-id", itemVals.GetType().GetProperty("TestId").GetValue(itemVals, null));
            }
            if (itemVals.GetType().GetProperty("CycleId") != null && itemVals.GetType().GetProperty("CycleId").GetValue(itemVals, null) != null && !isUpdate)
            {
                itemWithValues.Fields.Add("cycle-id", itemVals.GetType().GetProperty("CycleId").GetValue(itemVals, null));
            }

            return itemWithValues;
        }

        private static string EscapeChars(string origStr)
        {
            if (origStr != null)
            {
                origStr = origStr.Replace("&", "&amp;");
                origStr = origStr.Replace("<", "&lt;");
                origStr = origStr.Replace(">", "&gt;");
                origStr = origStr.Replace("\"", "&quot;");
                origStr = origStr.Replace("\'", "&apos;");
            }

            return origStr;
        }
        #endregion private Methods
    }
}
