using System;
using System.Collections.Generic;
using System.ComponentModel;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Activities;
using GingerCore.ALM.QCRestAPI;
using QCRestClient;

namespace GingerCore.ALM
{
    public class QCRestAPICore : ALMCore
    {
        public QCRestAPICore() { }

        public override bool ConnectALMServer()
        {
            return QCRestAPIConnect.ConnectQCServer(ALMCore.AlmConfig.ALMServerURL, ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword);
        }

        public override bool ConnectALMProject()
        {
            return QCRestAPIConnect.ConnectQCProject(ALMCore.AlmConfig.ALMServerURL, ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword, ALMCore.AlmConfig.ALMDomain, ALMCore.AlmConfig.ALMProjectName);
        }

        public override void DisconnectALMServer()
        {
            QCRestAPIConnect.DisconnectQCServer();
        }

        public override List<string> GetALMDomains()
        {
            return QCRestAPIConnect.GetQCDomains();
        }

        public override List<string> GetALMDomainProjects(string ALMDomainName)
        {
            ALMCore.AlmConfig.ALMDomain = ALMDomainName;
            return QCRestAPIConnect.GetQCDomainProjects(ALMCore.AlmConfig.ALMDomain);
        }

        public QCTestSet ImportTestSetData(string tSId)
        {
            return ImportFromQCRest.ImportTestSetData(tSId);
        }

        public QCTestInstanceColl ImportTestSetInstanceData(QCTestSet TS)
        {
            return ImportFromQCRest.ImportTestSetInstanceData(TS);
        }

        public QCTestCaseColl ImportTestSetTestCasesData(QCTestInstanceColl TStestInstances)
        {
            return ImportFromQCRest.ImportTestSetTestCasesData(TStestInstances);
        }

        public QCTestCaseStepsColl ImportTestCasesSteps(QCTestCaseColl testCases)
        {
            return ImportFromQCRest.ImportTestCasesSteps(testCases);
        }

        public QCTestCaseParamsColl ImportTestCasesParams(QCTestCaseColl testCases)
        {
            return ImportFromQCRest.ImportTestCasesParams(testCases);
        }

        public BusinessFlow ConvertQCTestSetToBF(QCTestSet testSet)
        {
            throw new NotImplementedException();
        }

        public BusinessFlow ConvertQCTestSetToBF(QCTestSet tS, QCTestInstanceColl testInstances, QCTestCaseColl tSTestCases, QCTestCaseStepsColl tSTestCaseSteps, QCTestCaseParamsColl tSTestCasesParams)
        {
            return ImportFromQCRest.ConvertQCTestSetToBF(tS, testInstances, tSTestCases, tSTestCaseSteps, tSTestCasesParams);
        }

        public override Dictionary<Guid, string> CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, bool useREST = false)
        {
            throw new NotImplementedException();
        }

        public override bool DisconnectALMProjectStayLoggedIn()
        {
            return QCRestAPIConnect.DisconnectQCProjectStayLoggedIn();
        }

        public bool ExportActivitiesGroupToALM(ActivitiesGroup activtiesGroup, QCTestCase matchingTC, string uploadPath, ObservableList<ExternalItemFieldBase> testCaseFields, ObservableList<ExternalItemFieldBase> designStepsFields, ObservableList<ExternalItemFieldBase> designStepsParamsFields, ref string res)
        {
            return ExportToQCRestAPI.ExportActivitiesGroupToQC(activtiesGroup, matchingTC, uploadPath, testCaseFields, designStepsFields, designStepsParamsFields, ref res);
        }

        public bool ExportBusinessFlowToALM(BusinessFlow businessFlow, QCTestSet mappedTestSet, string uploadPath, ObservableList<ExternalItemFieldBase> testSetFields, ref string result)
        {
            return ExportToQCRestAPI.ExportBusinessFlowToQC(businessFlow, mappedTestSet, uploadPath, testSetFields, ref result);
        }

        public override bool ExportExecutionDetailsToALM(BusinessFlow bizFlow, ref string result, bool exectutedFromAutomateTab = false, PublishToALMConfig publishToALMConfig = null)
        {
            result = string.Empty;
            //if (bizFlow.ExternalID == "0" || String.IsNullOrEmpty(bizFlow.ExternalID))
            //{
            //    result = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ": " + bizFlow.Name + " is missing ExternalID, cannot locate QC TestSet without External ID";
            //    return false;
            //}

            //try
            //{
            //    //get the BF matching test set
            //    QCTestSet testSet = ImportFromQCRest.GetQCTestSet(bizFlow.ExternalID);//bf.externalID holds the TestSet TSTests collection id
            //    if (testSet != null)
            //    {
            //        //get the Test set TC's
            //        QCTestInstanceColl qcTSTests = ImportFromQCRest.GetQCTestInstances(testSet); //list of TSTest's on main TestSet in TestLab 

            //        //get all BF Activities groups
            //        ObservableList<ActivitiesGroup> activGroups = bizFlow.ActivitiesGroups;
            //        if (activGroups.Count > 0)
            //        {
            //            foreach (ActivitiesGroup activGroup in activGroups)
            //            {
            //                if ((publishToALMConfig.FilterStatus == FilterByStatus.OnlyPassed && activGroup.RunStatus == ActivitiesGroup.eActivitiesGroupRunStatus.Passed)
            //                || (publishToALMConfig.FilterStatus == FilterByStatus.OnlyFailed && activGroup.RunStatus == ActivitiesGroup.eActivitiesGroupRunStatus.Failed)
            //                || publishToALMConfig.FilterStatus == FilterByStatus.All)
            //                {
            //                    QCTestInstance tsTest = null;
            //                    //go by TC ID = TC Instancs ID
            //                    tsTest = qcTSTests.Where(x => x.TestId == activGroup.ExternalID && x.Id == activGroup.ExternalID2).FirstOrDefault();
            //                    if (tsTest == null)
            //                    {
            //                        //go by Linked TC ID + TC Instancs ID
            //                        tsTest = qcTSTests.Where(x => ImportFromQCRest.GetTSTestLinkedID(x) == activGroup.ExternalID && x.Id == activGroup.ExternalID2).FirstOrDefault();
            //                    }
            //                    if (tsTest == null)
            //                    {
            //                        //go by TC ID 
            //                        tsTest = qcTSTests.Where(x => x.TestId == activGroup.ExternalID).FirstOrDefault();
            //                    }
            //                    if (tsTest != null)
            //                    {
            //                        //get activities in group
            //                        List<Activity> activities = (bizFlow.Activities.Where(x => x.ActivitiesGroupID == activGroup.Name)).Select(a => a).ToList();
            //                        string TestCaseName = PathHelper.CleanInValidPathChars(tsTest.Name);
            //                        if ((publishToALMConfig.VariableForTCRunName == null) || (publishToALMConfig.VariableForTCRunName == string.Empty))
            //                        {
            //                            String timeStamp = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss");
            //                            publishToALMConfig.VariableForTCRunName = "GingerRun_" + timeStamp;
            //                        }

            //                        RunFactory runFactory = (RunFactory)tsTest.RunFactory;
            //                        Run run = (Run)runFactory.AddItem(publishToALMConfig.VariableForTCRunNameCalculated);

            //                        // Attach ActivityGroup Report if needed
            //                        if (publishToALMConfig.ToAttachActivitiesGroupReport)
            //                        {
            //                            if ((activGroup.TempReportFolder != null) && (activGroup.TempReportFolder != string.Empty) &&
            //                                (System.IO.Directory.Exists(activGroup.TempReportFolder)))
            //                            {
            //                                //Creating the Zip file - start
            //                                string targetZipPath = System.IO.Directory.GetParent(activGroup.TempReportFolder).ToString();
            //                                string zipFileName = targetZipPath + "\\" + TestCaseName.ToString() + "_GingerHTMLReport.zip";

            //                                if (!System.IO.File.Exists(zipFileName))
            //                                {
            //                                    ZipFile.CreateFromDirectory(activGroup.TempReportFolder, zipFileName);
            //                                }
            //                                else
            //                                {
            //                                    System.IO.File.Delete(zipFileName);
            //                                    ZipFile.CreateFromDirectory(activGroup.TempReportFolder, zipFileName);
            //                                }
            //                                System.IO.Directory.Delete(activGroup.TempReportFolder, true);
            //                                //Creating the Zip file - finish
            //                                //Attaching Zip file - start
            //                                AttachmentFactory attachmentFactory = (AttachmentFactory)run.Attachments;
            //                                TDAPIOLELib.Attachment attachment = (TDAPIOLELib.Attachment)attachmentFactory.AddItem(System.DBNull.Value);
            //                                attachment.Description = "TC Ginger Execution HTML Report";
            //                                attachment.Type = 1;
            //                                attachment.FileName = zipFileName;
            //                                attachment.Post();

            //                                //Attaching Zip file - finish
            //                                System.IO.File.Delete(zipFileName);
            //                            }
            //                        }


            //                        //create run with activities as steps
            //                        run.CopyDesignSteps();
            //                        run.Post();
            //                        StepFactory stepF = run.StepFactory;
            //                        List stepsList = stepF.NewList("");

            //                        //int i = 0;
            //                        int index = 1;
            //                        foreach (Step step in stepsList)
            //                        {
            //                            //search for matching activity based on ID and not order, un matching steps need to be left as No Run
            //                            int stepDesignID = (stepsList[index]).Field("ST_DESSTEP_ID");
            //                            Activity matchingActivity = activities.Where(x => x.ExternalID == stepDesignID.ToString()).FirstOrDefault();
            //                            if (matchingActivity != null)
            //                            {
            //                                switch (matchingActivity.Status)
            //                                {
            //                                    case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed:
            //                                        step.Status = "Failed";
            //                                        List<Act> failedActs = matchingActivity.Acts.Where(x => x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed).ToList();
            //                                        string errors = string.Empty;
            //                                        foreach (Act act in failedActs) errors += act.Error + Environment.NewLine;
            //                                        step["ST_ACTUAL"] = errors;
            //                                        break;
            //                                    case Amdocs.Ginger.CoreNET.Execution.eRunStatus.NA:
            //                                        step.Status = "N/A";
            //                                        step["ST_ACTUAL"] = "NA";
            //                                        break;
            //                                    case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed:
            //                                        step.Status = "Passed";
            //                                        step["ST_ACTUAL"] = "Passed as expected";
            //                                        break;
            //                                    case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped:
            //                                        //step.Status = "No Run";
            //                                        step.Status = "N/A";
            //                                        step["ST_ACTUAL"] = "Skipped";
            //                                        break;
            //                                    case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending:
            //                                        step.Status = "No Run";
            //                                        step["ST_ACTUAL"] = "Was not executed";
            //                                        break;
            //                                    case Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running:
            //                                        step.Status = "Not Completed";
            //                                        step["ST_ACTUAL"] = "Not Completed";
            //                                        break;
            //                                }
            //                            }
            //                            else
            //                            {
            //                                //Step not exist in Ginger so left as "No Run" unless it is step data
            //                                if (step.Name.ToUpper() == "STEP DATA")
            //                                    step.Status = "Passed";
            //                                else
            //                                    step.Status = "No Run";
            //                            }
            //                            step.Post();
            //                            index++;
            //                        }

            //                        //get all execution status for all steps
            //                        ObservableList<string> stepsStatuses = new ObservableList<string>();
            //                        foreach (Step step in stepsList)
            //                            stepsStatuses.Add(step.Status);

            //                        //update the TC general status based on the activities status collection.                                
            //                        if (stepsStatuses.Where(x => x == "Failed").Count() > 0)
            //                            run.Status = "Failed";
            //                        else if (stepsStatuses.Where(x => x == "No Run").Count() == stepsList.Count || stepsStatuses.Where(x => x == "N/A").Count() == stepsList.Count)
            //                            run.Status = "No Run";
            //                        else if (stepsStatuses.Where(x => x == "Passed").Count() == stepsList.Count || (stepsStatuses.Where(x => x == "Passed").Count() + stepsStatuses.Where(x => x == "N/A").Count()) == stepsList.Count)
            //                            run.Status = "Passed";
            //                        else
            //                            run.Status = "Not Completed";
            //                        run.Post();
            //                    }
            //                    else
            //                    {
            //                        //No matching TC was found for the ActivitiesGroup in QC
            //                        result = "Matching TC's were not found for all " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " in QC/ALM.";
            //                    }
            //                }
            //            }
            //        }
            //        else
            //        {
            //            //No matching Test Set was found for the BF in QC
            //            result = "No matching Test Set was found in QC/ALM.";
            //        }

            //    }
            if (result == string.Empty)
            {
                result = "Export performed successfully.";
                return true;
            }
            else
            {
                return false;
            }
            //}
            //catch (Exception ex)
            //{
            //    result = "Unexpected error occurred- " + ex.Message;
            //    Reporter.ToLog(eLogLevel.ERROR, "Failed to export execution details to QC/ALM", ex);
            //    //if (!silentMode)
            //    //    Reporter.ToUser(eUserMsgKeys.ErrorWhileExportingExecDetails, ex.Message);
            //    return false;
            ////}
        }

        public override ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo
        {
            get { return ImportFromQCRest.GingerActivitiesGroupsRepo; }
            set { ImportFromQCRest.GingerActivitiesGroupsRepo = value; }
        }

        public override ObservableList<Activity> GingerActivitiesRepo
        {
            get { return ImportFromQCRest.GingerActivitiesRepo; }
            set { ImportFromQCRest.GingerActivitiesRepo = value; }
        }

        public static ObservableList<ExternalItemFieldBase> GetALMItemFields()
        {
            return ImportFromQCRest.GetALMItemFields();
        }

        public override ObservableList<ExternalItemFieldBase> GetALMItemFields(BackgroundWorker bw, bool online, ALM_Common.DataContracts.ResourceType resourceType)
        {
            if(resourceType == ALM_Common.DataContracts.ResourceType.ALL)
                return ImportFromQCRest.GetALMItemFields();
            return ImportFromQCRest.GetALMItemFields(resourceType);
        }

        public override bool IsServerConnected()
        {
            throw new NotImplementedException();
        }

        public QCTestSet GetQCTestSet(string testSetID)
        {
            return ImportFromQCRest.GetQCTestSet(testSetID);
        }

        public QCTestCase GetQCTest(string testID)
        {
            return ImportFromQCRest.GetQCTest(testID);
        }
    }
}
