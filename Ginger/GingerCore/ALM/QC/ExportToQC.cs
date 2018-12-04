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
using System;
using System.Collections.Generic;
using System.Linq;
using TDAPIOLELib;
using GingerCore.Activities;
using GingerCore.Variables;
using GingerCore.Actions;
using System.IO.Compression;
using System.Reflection;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.IO;

namespace GingerCore.ALM.QC
{
    public static class ExportToQC
    {
        static TDConnection mTDConn = QCConnect.TDConn;

        public static bool ExportExecutionDetailsToQC(BusinessFlow bizFlow, ref string result, PublishToALMConfig publishToALMConfig=null)
        {       
            result = string.Empty;
            if (bizFlow.ExternalID == "0" || String.IsNullOrEmpty(bizFlow.ExternalID))
            {
                result = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)+": " + bizFlow.Name + " is missing ExternalID, cannot locate QC TestSet without External ID";
                return false;
            }

            try
            {
                //get the BF matching test set
                TestSet testSet = ImportFromQC.GetQCTestSet(bizFlow.ExternalID);//bf.externalID holds the TestSet TSTests collection id
                if (testSet != null)
                {
                    //get the Test set TC's
                    List<TSTest> qcTSTests = ImportFromQC.GetTSTestsList(testSet); //list of TSTest's on main TestSet in TestLab 

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
                                TSTest tsTest = null;
                                //go by TC ID = TC Instance ID
                                tsTest = qcTSTests.Where(x => x.TestId == activGroup.ExternalID && x.ID == activGroup.ExternalID2).FirstOrDefault();
                                if (tsTest == null)
                                {
                                    //go by Linked TC ID + TC Instance ID
                                    tsTest = qcTSTests.Where(x => ImportFromQC.GetTSTestLinkedID(x) == activGroup.ExternalID && x.ID == activGroup.ExternalID2).FirstOrDefault();
                                }
                                if (tsTest == null)
                                {
                                    //go by TC ID 
                                    tsTest = qcTSTests.Where(x => x.TestId == activGroup.ExternalID).FirstOrDefault();
                                }
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

                                    RunFactory runFactory = (RunFactory)tsTest.RunFactory;
                                    Run run = (Run)runFactory.AddItem(publishToALMConfig.VariableForTCRunNameCalculated);

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
                                            //Attaching Zip file - start
                                            AttachmentFactory attachmentFactory = (AttachmentFactory)run.Attachments;
                                            TDAPIOLELib.Attachment attachment = (TDAPIOLELib.Attachment)attachmentFactory.AddItem(System.DBNull.Value);
                                            attachment.Description = "TC Ginger Execution HTML Report";
                                            attachment.Type = 1;
                                            attachment.FileName = zipFileName;
                                            attachment.Post();

                                            //Attaching Zip file - finish
                                            System.IO.File.Delete(zipFileName);
                                        }
                                    }


                                    //create run with activities as steps
                                    run.CopyDesignSteps();
                                    run.Post();
                                    StepFactory stepF = run.StepFactory;
                                    List stepsList = stepF.NewList("");

                                    //int i = 0;
                                    int index = 1;
                                    foreach (Step step in stepsList)
                                    {
                                        //search for matching activity based on ID and not order, un matching steps need to be left as No Run
                                        int stepDesignID = (stepsList[index]).Field("ST_DESSTEP_ID");
                                        Activity matchingActivity = activities.Where(x => x.ExternalID == stepDesignID.ToString()).FirstOrDefault();
                                        if (matchingActivity != null)
                                        {
                                            switch(matchingActivity.Status)
                                            {
                                                case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed:
                                                    step.Status = "Failed";
                                                    List<Act> failedActs= matchingActivity.Acts.Where(x => x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed).ToList();
                                                    string errors = string.Empty;
                                                    foreach (Act act in failedActs) errors += act.Error + Environment.NewLine;
                                                    step["ST_ACTUAL"] = errors;
                                                    break;
                                                case Amdocs.Ginger.CoreNET.Execution.eRunStatus.NA:
                                                    step.Status = "N/A";
                                                    step["ST_ACTUAL"] = "NA";
                                                    break;
                                                case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed:
                                                    step.Status = "Passed";
                                                    step["ST_ACTUAL"] = "Passed as expected";
                                                    break;
                                                case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped:
                                                    //step.Status = "No Run";
                                                    step.Status = "N/A";
                                                    step["ST_ACTUAL"] = "Skipped";
                                                    break;
                                                case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending:
                                                    step.Status = "No Run";
                                                    step["ST_ACTUAL"] = "Was not executed";
                                                    break;
                                                case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running:
                                                    step.Status = "Not Completed";
                                                    step["ST_ACTUAL"] = "Not Completed";
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            //Step not exist in Ginger so left as "No Run" unless it is step data
                                            if (step.Name.ToUpper() == "STEP DATA")
                                                step.Status = "Passed";
                                            else
                                                step.Status = "No Run";
                                        }
                                        step.Post();
                                        index++;
                                    }

                                    //get all execution status for all steps
                                    ObservableList<string> stepsStatuses = new ObservableList<string>();
                                    foreach (Step step in stepsList)
                                        stepsStatuses.Add(step.Status);

                                    //update the TC general status based on the activities status collection.                                
                                    if (stepsStatuses.Where(x => x == "Failed").Count() > 0)
                                        run.Status = "Failed";
                                    else if (stepsStatuses.Where(x => x == "No Run").Count() == stepsList.Count || stepsStatuses.Where(x => x == "N/A").Count()== stepsList.Count)
                                        run.Status = "No Run";
                                    else if (stepsStatuses.Where(x => x == "Passed").Count() == stepsList.Count || (stepsStatuses.Where(x => x == "Passed").Count() + stepsStatuses.Where(x => x == "N/A").Count()) == stepsList.Count)
                                        run.Status = "Passed";
                                    else
                                        run.Status = "Not Completed";
                                    run.Post();
                                }
                                else
                                {
                                    //No matching TC was found for the ActivitiesGroup in QC
                                    result = "Matching TC's were not found for all " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " in QC/ALM.";
                                }
                            }
                        }
                    }
                    else
                    {
                        //No matching Test Set was found for the BF in QC
                        result = "No matching Test Set was found in QC/ALM.";
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
                result = "Unexpected error occurred- "+ex.Message;
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to export execution details to QC/ALM", ex);
                //if (!silentMode)
                //    Reporter.ToUser(eUserMsgKeys.ErrorWhileExportingExecDetails, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Export Activities Group details to QC, can be used for creating new matching QC Test Case or updating an existing one
        /// </summary>
        /// <param name="activitiesGroup">Activities Group to Export</param>
        /// <param name="mappedTest">The QC Test Case which mapped to the Activities Group (in case exist) and needs to be updated</param>
        /// <param name="uploadPath">Upload path in QC Test Plan</param>
        /// <param name="result">Export error result</param>
        /// <returns></returns>
        public static bool ExportActivitiesGroupToQC(ActivitiesGroup activitiesGroup, Test mappedTest, string uploadPath, ObservableList<ExternalItemFieldBase> testCaseFields, ref string result)
        {
            Test test;

            try
            {
                if (mappedTest == null)
                {
                    //##create new Test Case in QC
                    TestFactory TestF = (TestFactory)mTDConn.TestFactory;
                    test = (Test)TestF.AddItem(System.DBNull.Value);
                    test.Type = "MANUAL";

                    //set the upload path
                    TreeManager treeM = (TreeManager)mTDConn.TreeManager;
                    ISysTreeNode testParentFolder = (ISysTreeNode)treeM.get_NodeByPath(uploadPath);
                    test["TS_SUBJECT"] = testParentFolder.NodeID;
                    
                }
                else
                {
                    //##update existing test case
                    test = ImportFromQC.GetQCTest(activitiesGroup.ExternalID);

                    //delete the un-needed steps
                    DesignStepFactory stepF = test.DesignStepFactory;
                    List stepsList = stepF.NewList("");
                    foreach (DesignStep step in stepsList)
                    {
                        if (activitiesGroup.ActivitiesIdentifiers.Where(x => x.IdentifiedActivity.ExternalID == step.ID.ToString()).FirstOrDefault() == null)
                            stepF.RemoveItem(step.ID);
                    }

                    //delete the existing parameters
                    StepParams testParams = test.Params;
                    if (testParams.Count > 0)
                    {
                        for (int indx = 0; indx < testParams.Count; indx++)
                        {
                            testParams.DeleteParam(testParams.ParamName[indx]);
                            testParams.Save();
                        }
                    }

                }

                //set item fields
                foreach (ExternalItemFieldBase field in testCaseFields)
                {
                    if (field.ToUpdate)
                    {
                        if (string.IsNullOrEmpty(field.SelectedValue) == false && field.SelectedValue != "NA")
                            test[field.ID] = field.SelectedValue;
                        else
                            try { test[field.ID] = "NA"; }
                            catch { }
                    }
                }
                
                //post the test
                test.Name = activitiesGroup.Name;
                test.Post();
                activitiesGroup.ExternalID = test.ID.ToString();
                activitiesGroup.ExternalID2 = test.ID.ToString();

                //Add/update all test steps + Parameters
                foreach (ActivityIdentifiers actIdent in activitiesGroup.ActivitiesIdentifiers)
                    ExportActivityAsTestStep(test, actIdent.IdentifiedActivity);

                return true;
            }
            catch (Exception ex)
            {
                result = "Unexpected error occurred- " + ex.Message;
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to export the Activities Group to QC/ALM", ex);
                return false;
            }
        }

        private static bool ExportActivityAsTestStep(Test test, Activity activity)
        {
            DesignStepFactory stepF;
            DesignStep step = null;
            List stepsList;

            stepF = test.DesignStepFactory;

            if (string.IsNullOrEmpty(activity.ExternalID) == false)
            {
                //look for existing step                    
                stepsList = stepF.NewList("");
                if (stepsList != null && stepsList.Count > 0)
                    foreach (DesignStep s in stepsList)
                        if (s.ID.ToString() == activity.ExternalID)
                        {
                            step = s;//step already exist 
                            break;
                        }
            }

            if (step == null)
            {
                //create new step
                step = stepF.AddItem(System.DBNull.Value);
            }

            step.StepName = activity.ActivityName;
            string descriptionTemplate =
                "<html><body><div align=\"left\"><font face=\"Arial\"><span style=\"font-size:8pt\"><<&Description&&>><br /><<&Parameters&>><br /><<&Actions&>></span></font></div></body></html>";
            string description = descriptionTemplate.Replace("<<&Description&&>>",activity.Description);
            StepParams testParams = test.Params;
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
                        testParams.AddParam(var.Name.ToLower(), "String");
                        testParams.Save();
                        test.Post();
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
            step.StepDescription = description;

            string expectedTemplate =
               "<html><body><div align=\"left\"><font face=\"Arial\"><span style=\"font-size:8pt\"><<&Expected&&>></span></font></div></body></html>";
            step.StepExpectedResult = expectedTemplate.Replace("<<&Expected&&>>", activity.Expected);

            step.Post();

            activity.ExternalID = step.ID.ToString();
            return true;
        }

        public static bool ExportBusinessFlowToQC(BusinessFlow businessFlow, TestSet mappedTestSet, string uploadPath, ObservableList<ExternalItemFieldBase> testSetFields, ref string result)
        {
            TestSet testSet;
            ObservableList<ActivitiesGroup> existingActivitiesGroups = new ObservableList<ActivitiesGroup>();
            try
            {
                if (mappedTestSet == null)
                {
                    //##create new Test Set in QC
                    TestSetFactory TestSetF = (TestSetFactory)mTDConn.TestSetFactory;
                    testSet = (TestSet)TestSetF.AddItem(System.DBNull.Value);                    

                    //set the upload path
                    TestSetTreeManager treeM = (TestSetTreeManager)mTDConn.TestSetTreeManager;
                    ISysTreeNode testSetParentFolder = (ISysTreeNode)treeM.get_NodeByPath(uploadPath);
                    testSet.TestSetFolder = testSetParentFolder.NodeID;
                }
                else
                {
                    //##update existing test set
                    //testSet = mappedTestSet;
                    testSet = ImportFromQC.GetQCTestSet(mappedTestSet.ID.ToString());
                     
                    TSTestFactory testsF = (TSTestFactory)testSet.TSTestFactory;
                    List tsTestsList = testsF.NewList("");
                    foreach (TSTest tsTest in tsTestsList)
                    {
                        ActivitiesGroup ag = businessFlow.ActivitiesGroups.Where(x => (x.ExternalID == tsTest.TestId.ToString() && x.ExternalID2 == tsTest.ID.ToString())).FirstOrDefault();
                        if (ag == null)
                            testsF.RemoveItem(tsTest.ID);
                        else
                            existingActivitiesGroups.Add(ag);
                    }
                }
                
                //set item fields
                foreach (ExternalItemFieldBase field in testSetFields)
                {
                    if (field.ToUpdate)
                    {
                        if (string.IsNullOrEmpty(field.SelectedValue) == false && field.SelectedValue != "NA")
                            testSet[field.ID] = field.SelectedValue;
                        else
                            try { testSet[field.ID] = "NA"; }
                            catch { }
                    }
                }

                //post the test set
                testSet.Name = businessFlow.Name;

                try
                {
                    testSet.Post();
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("The Test Set already exists"))
                    {
                        result = "Cannot export Business Flow - The Test Set already exists in the selected folder. ";
                        Reporter.ToLog(eAppReporterLogLevel.ERROR, result, ex);
                        return false;
                    }

                    //Searching for the testset in case it was created in ALM although getting exception
                    TestSetFactory TSetFact = mTDConn.TestSetFactory;
                    TDFilter tsFilter = TSetFact.Filter;
                    TestSetTreeManager treeM = (TestSetTreeManager)mTDConn.TestSetTreeManager;
                    ISysTreeNode testSetParentFolder = (ISysTreeNode)treeM.get_NodeByPath(uploadPath);

                    try
                    {
                        tsFilter["CY_FOLDER_ID"] = "" + testSetParentFolder.NodeID + "";
                    }
                    catch(Exception e)
                    {
                        tsFilter["CY_FOLDER_ID"] = "\"" + testSetParentFolder.Path.ToString() + "\"";
                        Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
                    }

                    List TestsetList = TSetFact.NewList(tsFilter.Text);
                    foreach(TestSet set in TestsetList)
                    {
                        if(set.Name == businessFlow.Name)
                        {
                            testSet = set;
                            break;
                        }
                    }
                }

                businessFlow.ExternalID = testSet.ID.ToString();

                //Add missing test cases
                TSTestFactory testCasesF = testSet.TSTestFactory;
                foreach (ActivitiesGroup ag in businessFlow.ActivitiesGroups)
                {
                    if (existingActivitiesGroups.Contains(ag) == false && string.IsNullOrEmpty(ag.ExternalID) == false && ImportFromQC.GetQCTest(ag.ExternalID) != null)
                    {
                        TSTest tsTest = testCasesF.AddItem(ag.ExternalID);
                        if (tsTest != null)
                        {
                            ag.ExternalID2 = tsTest.ID;//the test case instance ID in the test set- used for exporting the execution details
                        }
                    }
                    else
                    {
                        foreach (ActivityIdentifiers actIdent in ag.ActivitiesIdentifiers)
                        {
                            ExportActivityAsTestStep(ImportFromQC.GetQCTest(ag.ExternalID), actIdent.IdentifiedActivity);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                result = "Unexpected error occurred- " + ex.Message;
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to export the Business Flow to QC/ALM", ex);
                return false;
            }
        }
        


        #region swapnas code
        
        // Create New Defect in Defects Tab QC 
        public static int CreateDefect(String Status, String Summary, String DetectedBy, String Version)
        {
            try
            {
                BugFactory bugFactory = mTDConn.BugFactory;

                //start of get bug mandatory fields
                Customization field = (Customization)mTDConn.Customization;
                CustomizationFields f = (CustomizationFields)field.Fields;

                List BugFieldsList = f.get_Fields("BUG");
                Dictionary<string, string> MandatoryFields = new Dictionary<string, string>();
                Dictionary<string, List<string>> MandatoryListSelections = new Dictionary<string, List<string>>();
                foreach (CustomizationField BugField in BugFieldsList)
                {
                    if (BugField.IsRequired)
                    {
                        List<string> FieldList = new List<string>();
                        if (BugField.List != null && BugField.List.RootNode.Children.Count > 0)
                        {
                            CustomizationListNode lnode = BugField.List.RootNode;
                            List cNodes = lnode.Children;
                            foreach (CustomizationListNode ccNode in cNodes)
                            {
                                //adds list of valid selections of Field
                                FieldList.Add(ccNode.Name);
                            }
                        }
                        if (!MandatoryFields.ContainsKey(BugField.UserLabel))
                        {
                            MandatoryFields.Add(BugField.UserLabel, BugField.ColumnName);
                            if (FieldList.Count > 0)
                                MandatoryListSelections.Add(BugField.UserLabel, FieldList);
                            else MandatoryListSelections.Add(BugField.UserLabel, null);
                        }
                    }
                }
                //end of get mandatory fileds
                /*
                            Bug bug = bugFactory.AddItem(System.DBNull.Value);
                            //start of populate mandatory field with first one from dropdown
                            foreach (dynamic item in MandatoryFields)
                            {
                                dynamic staticSelection = MandatoryListSelections.Where(M => M.Key == item.Key).Select(m => m.Value).FirstOrDefault();
                                bug[item.Value] = staticSelection[0]; //statically selecting first item in list
                            }
                            //end of populate mandatory fields
                */
                Bug bug = bugFactory.AddItem(System.DBNull.Value);
                bug.Status = Status;
                //bug.Project = "Internal";            
                bug.Summary = Summary;
                bug.DetectedBy = DetectedBy;
                bug["BG_USER_08"] = Version;
                //bug.AssignedTo = "Nobody";
                //bug.Priority = "Low";

                bug.Post();
                return bug.ID;
            }
            catch (Exception ex)
            {
                //String Text = "Defect Creation Failed.";
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                return 0;
            }
        }
        #endregion swapnas code
    }
}
