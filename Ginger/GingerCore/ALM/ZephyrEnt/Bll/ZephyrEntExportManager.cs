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

using ALM_Common.DataContracts;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.IO;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Activities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZephyrEntSDK.Models;
using ZephyrEntSDK.Models.Base;
using Zepyhr_Ent_Repository;

namespace GingerCore.ALM.ZephyrEnt.Bll
{
    public class ZephyrEntExportManager
    {
        string currentExportName = String.Empty;
        private ZephyrEntRepository zephyrEntRepository;
        private int projectId;
        public ZephyrEntExportManager(ZephyrEntRepository zephyrEntRepository)
        {
            this.zephyrEntRepository = zephyrEntRepository;
            projectId = Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey);
        }

        public TreeNode CreateTreeNode(TreeNode node = null, int parentId = -1)
        {
            try
            {
                if (node == null)
                {
                    node = new TreeNode(currentExportName, currentExportName + " Description", "Phase", projectId);
                }
                else
                {
                    node.releaseId = projectId;
                }
                parentId = parentId == -1 ? projectId : parentId;
                TreeNode treeNode = zephyrEntRepository.CreateTreeNode(node, parentId);
                return treeNode;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to get Zephyr Ent. tree node data " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), ex);
                return null;
            }
        }
        public dynamic CreateNewTestPlanningFolder(long cycleId, long parenttreeid, string folderName, string folderDesc)
        {
            return zephyrEntRepository.CreateNewTestPlanningFolder(cycleId, parenttreeid, folderName, folderDesc);
        }
        public dynamic UpdateTestPlanningFolder(long cycleId, long parenttreeid, BusinessFlow businessFlow)
        {
            List<TestCaseResource> testCaseResources = zephyrEntRepository.GetTestCasesByAssignmentTree(Convert.ToInt32(parenttreeid));
            testCaseResources.ForEach(tcr => {
                if (!businessFlow.ActivitiesGroups.Any(ag => ag.ExternalID2.Equals(tcr.testcase.id.ToString())))
                {
                    bool isDeleted = zephyrEntRepository.DeleteTestFromPhaseByTestId(Convert.ToInt32(tcr.testcase.testcaseId), Convert.ToInt32(tcr.tct.tcrCatalogTreeId), Convert.ToInt32(cycleId));
                    if (!isDeleted)
                    {
                        Reporter.ToUser(eUserMsgKey.StaticWarnMessage, "User doesn't have permission to delete from Zephyr");
                    }
                }
            });
            return zephyrEntRepository.UpdateTestPlanningFolder(cycleId, parenttreeid, businessFlow.Name, String.IsNullOrEmpty(businessFlow.Description) ? businessFlow.Name + " description" : businessFlow.Description);
        }
        public TestCaseResource CreateTestCase(long treeNodeId, ActivitiesGroup activtiesGroup, Dictionary<string, string> testInstanceFields)
        {
            try
            {
                TestCaseResource testCaseResource = zephyrEntRepository.CreateTestCase(new TestCaseResource(treeNodeId,
                    new TestCase()
                    {
                        name = activtiesGroup.Name,
                        automated = true,
                        requirementIds = new List<string>(),
                        releaseId = Convert.ToInt64(projectId),
                        description = activtiesGroup.Description,
                        tcCreationDate = DateTime.Now.ToString("dd/MM/yyyy"),
                        creationDate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        tag = "Ginger"
                    }, true, 0, 1, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
                activtiesGroup.ExternalID = testCaseResource.testcase.testcaseId.ToString();
                activtiesGroup.ExternalID2 = testCaseResource.testcase.id.ToString();
                CreateTestSteps(activtiesGroup.ActivitiesIdentifiers, testCaseResource.testcase.id, testCaseResource.id);
                testCaseResource.testcase.customProperties = testInstanceFields;
                zephyrEntRepository.UpdateTestCase(testCaseResource);
                return testCaseResource;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to create new Zephyr Ent. test case " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), ex);
                return null;
            }
        }
        public TestCaseResource UpdateTestCase(long treeNodeId, ActivitiesGroup activtiesGroup, List<BaseResponseItem> matchingTC)
        {
            try
            {
                TestCaseResource testCaseResource = zephyrEntRepository.GetTestCaseByTcId(Convert.ToInt32(activtiesGroup.ExternalID), projectId.ToString());
                testCaseResource.testcase.name = activtiesGroup.Name;
                testCaseResource.testcase.description =
                    String.IsNullOrEmpty(activtiesGroup.Description) ? String.Empty : activtiesGroup.Description;
                testCaseResource = zephyrEntRepository.UpdateTestCase(testCaseResource);
                UpdateTestSteps(activtiesGroup.ActivitiesIdentifiers, testCaseResource.testcase.testcaseId, testCaseResource.id);
                return testCaseResource;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to update Zephyr Ent. test case " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), ex);
                return null;
            }
        }

        internal bool ExportExceutionDetailsToALM(BusinessFlow bizFlow, ref string result, ObservableList<ExternalItemFieldBase> runFields, bool exectutedFromAutomateTab, PublishToALMConfig publishToALMConfig)
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
                List<BaseResponseItem> exportedPhase = zephyrEntRepository.GetPhaseById(Convert.ToInt32(bizFlow.ExternalID2));
                BaseResponseItem item = exportedPhase.FirstOrDefault(md => md.id.ToString().Equals(bizFlow.ExternalID));
                if (item == null)
                {
                    BaseResponseItem firstItem = exportedPhase.FirstOrDefault();
                    foreach (var category in (JArray)firstItem.TryGetItem("categories"))
                    {
                        BaseResponseItem treeNode = category.ToObject<BaseResponseItem>();
                        if(treeNode.id.ToString().Equals(bizFlow.ExternalID))
                        {
                            item = treeNode;
                            break;
                        }
                    }
                }
                if (item != null)
                {
                    long scheduleId = 0;
                    //get the Test set TC's
                    List<TestCaseResource> testCaseResources = zephyrEntRepository.GetTestCasesByAssignmentTree(Convert.ToInt32(item.id));
                    //get phase execution list
                    List<Execution> executions = zephyrEntRepository.GetExecutionsByPhaseId(Convert.ToInt64(bizFlow.ExternalID2));
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
                                TestCaseResource testCase = null;
                                //go by TC ID = TC Instances ID
                                testCase = testCaseResources.Find(x => x.testcase.testcaseId.ToString() == activGroup.ExternalID);
                                if (testCase != null)
                                {
                                    //get activities in group
                                    List<Activity> activities = (bizFlow.Activities.Where(x => x.ActivitiesGroupID == activGroup.Name)).Select(a => a).ToList();
                                    string TestCaseName = PathHelper.CleanInValidPathChars(activGroup.Name);
                                    if ((publishToALMConfig.VariableForTCRunName == null) || (publishToALMConfig.VariableForTCRunName == string.Empty))
                                    {
                                        String timeStamp = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss");
                                        publishToALMConfig.VariableForTCRunName = "GingerRun_" + timeStamp;
                                    }
                                    scheduleId = executions.FindLast(tc => tc.tcrTreeTestcase.testcase.testcaseId.ToString().Equals(activGroup.ExternalID)).id;
                                    string notes = publishToALMConfig.VariableForTCRunNameCalculated;
                                    dynamic executionResult = zephyrEntRepository.ExecuteTestCase(scheduleId, GetTestStatus(activGroup), zephyrEntRepository.GetCurrentUser()[0].id, notes);

                                    if (executionResult == null)
                                    {
                                        result = "Failed to create run using rest API";
                                        return false;
                                    }

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
                                            //Creating the Zip file - finish

                                            Execution attachmentExecution = zephyrEntRepository.UpdateTestCaseAttachment(scheduleId.ToString() , zipFileName);

                                            if (attachmentExecution == null)
                                            {
                                                result = "Failed to create attachment";
                                                return false;
                                            }

                                            System.IO.File.Delete(zipFileName);
                                        }
                                    }

                                }
                                else
                                {
                                    //No matching TC was found for the ActivitiesGroup in QC
                                    result = "Matching TC's were not found for all " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " in QC/ALM.";
                                }
                            }
                            if (result != string.Empty)
                            {
                                return false;
                            }
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
                Reporter.ToLog(eLogLevel.ERROR, "Failed to export execution details to QC/ALM", ex);
                return false;
            }

            return false; // Remove it at the end
        }

        private List<TestStepResource> CreateTestSteps(ObservableList<ActivityIdentifiers> testStepsList, long tcVersionId, long tcId)
        {
            try
            {
                List<TestStepResource> testStepResourcesList = new List<TestStepResource>();
                long currentTestStepResourceId = 0;
                foreach (ActivityIdentifiers step in testStepsList)
                {
                    string stepDescription = String.IsNullOrEmpty(step.IdentifiedActivity.Description) ? String.Empty : step.IdentifiedActivity.Description;
                    string stepExpected = String.IsNullOrEmpty(step.IdentifiedActivity.Expected) ? String.Empty : step.IdentifiedActivity.Expected;
                    if (testStepsList.IndexOf(step) == 0)  // first step should be created via POST nexts - via PUT
                    {
                        TestStep newTestStep = new TestStep(step.ActivityName, stepDescription, testStepsList.IndexOf(step) + 1, stepExpected);
                        TestStepResource newTestStepResource = new TestStepResource(tcVersionId, testStepsList.IndexOf(step) + 1, newTestStep, tcId);
                        TestStepResource testStepResource = zephyrEntRepository.CreateTestStepResourceWithStep(newTestStepResource, tcVersionId, tcId);
                        currentTestStepResourceId = testStepResource.id;
                        testStepResourcesList.Add(testStepResource);
                        step.ExternalID = testStepResource.step.id.ToString();
                    }
                    else
                    {
                        TestStep newTestStep = new TestStep(step.ActivityName, stepDescription, testStepsList.IndexOf(step) + 1, stepExpected);
                        TestStepResource existedTestStepResource = new TestStepResource(currentTestStepResourceId, tcVersionId, testStepsList.IndexOf(step) + 1, tcId);
                        existedTestStepResource.step = newTestStep;
                        TestStepResource testStepResource = zephyrEntRepository.UpdateTestStep(existedTestStepResource, tcVersionId, tcId);
                        testStepResourcesList.Add(testStepResource);
                        step.ExternalID = testStepResource.step.id.ToString();
                    }
                }
                return testStepResourcesList;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to create Zephyr Ent. test steps " + GingerDicser.GetTermResValue(eTermResKey.Activities), ex);
                return null;
            }
        }
        private bool UpdateTestSteps(ObservableList<ActivityIdentifiers> testStepsList, long tcVersionId, long tcId)
        {
            try
            {
                List<long> toDeleteSteps = new List<long>();
                TestStepResource testSteps = zephyrEntRepository.GetTestCaseStepsByTcId((int)tcVersionId, projectId.ToString());
                testSteps.steps.ForEach(step => toDeleteSteps.Add(step.id));
                foreach (ActivityIdentifiers step in testStepsList)
                {
                    string stepDescription = String.IsNullOrEmpty(step.IdentifiedActivity.Description) ? String.Empty : step.IdentifiedActivity.Description;
                    string stepExpected = String.IsNullOrEmpty(step.IdentifiedActivity.Expected) ? String.Empty : step.IdentifiedActivity.Expected;
                    if (!String.IsNullOrEmpty(step.ExternalID))
                    {
                        TestStep currentStep = testSteps.steps.FirstOrDefault(st => step.ExternalID.Equals(st.id.ToString()));
                        TestStepResource existedTestStepResource = new TestStepResource(testSteps.id, tcVersionId, testSteps.maxId, testSteps.tcId);
                        TestStep existedTestStepToUpdate = new TestStep(step.ActivityName, stepDescription, testStepsList.IndexOf(step) + 1, stepExpected);
                        existedTestStepResource.step = existedTestStepToUpdate;
                        TestStepResource result = zephyrEntRepository.UpdateTestStep(existedTestStepResource, testSteps.tcId, tcId);
                        toDeleteSteps.Remove(existedTestStepResource.step.id);
                    }
                    else
                    {
                        TestStep newTestStep = new TestStep(step.ActivityName, stepDescription, testStepsList.IndexOf(step) + 1, stepExpected);
                        TestStepResource existedTestStepResource = new TestStepResource(testSteps.id, tcVersionId, testStepsList.IndexOf(step) + 1, tcId);
                        existedTestStepResource.step = newTestStep;
                        TestStepResource testStepResource = zephyrEntRepository.UpdateTestStep(existedTestStepResource, testSteps.tcId, tcId);
                        toDeleteSteps.Remove(testStepResource.step.id);
                        step.ExternalID = testStepResource.step.id.ToString();
                    }
                }
                // Check if steps deleted from zephyr
                if (toDeleteSteps.Count > 0)
                {
                    // todo - delete steps from zephyr permmision issue
                }
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to update Zephyr Ent. test steps " + GingerDicser.GetTermResValue(eTermResKey.Activities), ex);
                return false;
            }
        }
        public Cycle CreateNewTestCycle()
        {
            try
            {
                Cycle cycle = zephyrEntRepository.CreateCycle(new Cycle(currentExportName,
                                                                            DateTime.Now.ToString("MM/dd/yyyy"),        // "12/12/2020" format
                                                                            DateTime.Now.AddDays(30).ToString("MM/dd/yyyy"),
                                                                            0,
                                                                            projectId,
                                                                            new List<CyclePhase>(),
                                                                            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()));  // "1562668881250" format
                return cycle;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to create Zephyr Ent. new cycle " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), ex);
                return null;
            }
        }
        public CyclePhase CreateNewTestCyclePhase(Cycle cycle, string bfName)
        {
            try
            {
                CyclePhase CyclePhase = zephyrEntRepository.CreateCyclePhase(new CyclePhase()
                {
                    name = bfName,
                    phaseStartDate = cycle.cycleStartDate,
                    phaseEndDate = cycle.cycleEndDate,
                    freeForm = true,
                    releaseId = projectId
                }, cycle.id);
                return CyclePhase;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to create Zephyr Ent. new cycle phase " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), ex);
                return null;
            }
        }
        public void AssigningTestCasesToCyclePhase(List<long> tctIds, long cyclePhaseId, long parenttreeid)
        {
            zephyrEntRepository.AssignTestCaseToPhaseBySearch(tctIds, cyclePhaseId, parenttreeid);
        }
        public List<Execution> AssigningTestCasesToTesterForExecution(List<long> tcVersionIds, long cyclePhaseId, long testerId, long tcrCatalogTreeId)
        {
            try
            {
                List<Execution> assignsList = new List<Execution>();
                foreach (long tcVersionId in tcVersionIds)
                {
                    List<Execution> execution = zephyrEntRepository.AssignTestCaseToTesterForExecution(tcVersionId, cyclePhaseId, testerId, tcrCatalogTreeId);
                    if ((execution != null) && (execution.Count > 0))
                    {
                        assignsList.Add(execution[0]);
                    }
                }
                return assignsList;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to assign Zephyr Ent. test cases to user " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups), ex);
                return null;
            }
        }
        public void ExecuteTestCases(List<Execution> assignsList, long testerId, ObservableList<ActivitiesGroup> ActivitiesGroups)
        {
            List<TestCaseResource> testCaseResourcesList = new List<TestCaseResource>();
            foreach (ActivitiesGroup testCase in ActivitiesGroups)
            {
                long currentAssigmentId = assignsList.Where(z => z.tcrTreeTestcase.testcase.testcaseId == Convert.ToInt64(testCase.ExternalID)).Select(y => y.id).FirstOrDefault();
                if (currentAssigmentId != 0)
                {
                    zephyrEntRepository.ExecuteTestCase(currentAssigmentId, GetTestStatus(testCase), testerId);
                }
            }
        }
        public int GetRepositoryTreeIdByTestcaseId(int testcaseId)
        {
            List<BaseResponseItem> treeData = zephyrEntRepository.GetRepositoryTreeIdByTestcaseId(testcaseId, projectId);
            return Convert.ToInt32(((Newtonsoft.Json.Linq.JContainer)((Newtonsoft.Json.Linq.JContainer)((Newtonsoft.Json.Linq.JContainer)treeData[0].TryGetItem("hierarchy")).Last).First).First);
        }
        public int GetTestStatus(ActivitiesGroup testToExport)
        {
            if (testToExport.ActivitiesIdentifiers.Where(x => x.IdentifiedActivity.Status == eRunStatus.Failed).Any())
            {
                return 2;
            }
            else if (testToExport.ActivitiesIdentifiers.Where(x => x.IdentifiedActivity.Status == eRunStatus.Blocked).Any())
            {
                return 4;
            }
            else if (testToExport.ActivitiesIdentifiers.Where(x => x.IdentifiedActivity.Status == eRunStatus.Running).Any())
            {
                return 3;
            }
            else if ((testToExport.ActivitiesIdentifiers.Where(x => x.IdentifiedActivity.Status == eRunStatus.Pending).Any())
                        || (testToExport.ActivitiesIdentifiers.Where(x => x.IdentifiedActivity.Status == eRunStatus.Stopped).Any()))
            {
                return 3;
            }
            else if (testToExport.ActivitiesIdentifiers.Where(x => x.IdentifiedActivity.Status == eRunStatus.Passed || x.IdentifiedActivity.Status == eRunStatus.Skipped).Count() == testToExport.ActivitiesIdentifiers.Count())
            {
                return 1;
            }
            else if (testToExport.ActivitiesIdentifiers.Where(x => x.IdentifiedActivity.Status == eRunStatus.Skipped).Any())
            {
                return 3;
            }
            else
            {
                return 3;
            }
        }
        public string GetTestRepositoryFolderType(int treeId)
        {
            return zephyrEntRepository.GetTestRepositoryFolderType(treeId).ToString();
        }
    }
}
