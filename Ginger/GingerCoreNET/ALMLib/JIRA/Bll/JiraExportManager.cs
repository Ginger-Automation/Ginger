#region License
/*
Copyright © 2014-2026 European Support Limited

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

using AlmDataContractsStd.Contracts;
using AlmDataContractsStd.Enums;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.Repository;
using GingerCore.Activities;
using GingerCore.ALM.JIRA.Data_Contracts;
using GingerCore.Variables;
using GingerCoreNET.GeneralLib;
using JiraRepositoryStd.BLL;
using JiraRepositoryStd.Data_Contracts;
using JiraRepositoryStd.Helpers;
using JiraRepositoryStd.Settings;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace GingerCore.ALM.JIRA.Bll
{
    public class JiraExportManager
    {
        private JiraRepositoryStd.JiraRepositoryStd jiraRepObj;
        private JiraManagerZephyr jmz;

        public JiraExportManager(JiraRepositoryStd.JiraRepositoryStd jiraRep)
        {
            this.jiraRepObj = jiraRep;
            this.jmz = new JiraManagerZephyr();
        }


        public Dictionary<Guid, string> CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, List<ExternalItemFieldBase> defectsFields)
        {
            Dictionary<Guid, string> defectsOpeningResults = [];
            List<JiraIssueExport> defectsToExport = [];
            List<string> screenshots = [];
            foreach (KeyValuePair<Guid, Dictionary<string, string>> defectForOpening in defectsForOpening)
            {
                string summaryValue = defectForOpening.Value.ContainsKey("Summary") ? defectForOpening.Value["Summary"] : string.Empty;
                if (!string.IsNullOrEmpty(summaryValue))
                {
                    string defectId = CheckIfDefectExist(summaryValue);
                    if (!string.IsNullOrEmpty(defectId))
                    {
                        defectsOpeningResults.Add(defectForOpening.Key, defectId);
                        continue;
                    }
                    else
                    {
                        string paths = defectForOpening.Value.ContainsKey("screenshots") ? defectForOpening.Value["screenshots"] : string.Empty;
                        screenshots.Add(paths);
                    }
                }
                //if no then add into list to open new defect
                defectsToExport.Add(this.CreateDefectData(defectForOpening, defectsFields));
            }

            if (defectsToExport.Count > 0)
            {
                var exportedDefects = jiraRepObj.ExportJiraIssues(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL, defectsToExport, ALMCore.DefaultAlmConfig.UseToken);
                if (exportedDefects.Count > 0)
                {
                    for (var a = 0; a < exportedDefects.Count; a++)
                    {
                        if (exportedDefects[a].DataResult != null)
                        {
                            AttachScreenshotsToDefects(screenshots[a], exportedDefects[a].DataResult.key);
                            defectsOpeningResults.Add(defectsForOpening.ElementAt(a).Key, exportedDefects[a].DataResult.key);
                        }
                        else
                        {
                            Reporter.ToLog(eLogLevel.ERROR, "Failed to create to defect -" + exportedDefects[a].AuthenticationResponseObj.ErrorDesc);
                        }
                    }
                }
            }
            return defectsOpeningResults;
        }

        /// <summary>
        /// Check for if defect is already exist with given summury value
        /// if exist return id of the first defect
        /// </summary>
        /// <param name="summary">string summary value</param>
        /// <returns></returns>
        string CheckIfDefectExist(string summary)
        {
            //Search for exist based on summary
            WhereDataList filterData = [new WhereData() { Name = "summary", Values = [summary], Operator = WhereOperator.And }];

            //and based on status, fetch statuses from JiraSettings.json 
            DefectStatusColl jiraDefectStatuses = null;
            JiraHelper jiraHelper = new JiraHelper();
            if (jiraHelper.TryGetJiraDefectStatuses(out jiraDefectStatuses))
            {
                foreach (DefectStatus defectStatus in jiraDefectStatuses)
                {
                    ComparisionOperator comparisionOperator = GetComparisionOperator(defectStatus.Operator.ToLower());
                    filterData.Add(new WhereData() { Name = "Status", Values = defectStatus.Status.ToList(), Operator = WhereOperator.And, ComparisionOperator = comparisionOperator });
                }
            }

            var issues = jiraRepObj.GetJiraIssues(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL, ALMCore.DefaultAlmConfig.ALMProjectKey, ResourceType.DEFECT, filterData, ALMCore.DefaultAlmConfig.UseToken);
            if (issues.DataResult.Count > 0)
            {
                return issues.DataResult.FirstOrDefault().key.ToString();
            }
            else
            {
                return "";
            }
        }

        public ComparisionOperator GetComparisionOperator(string compareOperator)
        {
            var comparisionOperator = compareOperator switch
            {
                "equal" => ComparisionOperator.EqualTo,
                "not equal" => ComparisionOperator.NotEqual,
                "in" => ComparisionOperator.In,
                "not in" => ComparisionOperator.NotIn,
                _ => ComparisionOperator.EqualTo,
            };
            return comparisionOperator;
        }

        /// <summary>
        /// Add attachments to defect id
        /// </summary>
        /// <param name="screenshots">string containing path of the screanshots separeted by commas</param>
        /// <param name="defectId">string defectId</param>
        void AttachScreenshotsToDefects(string screenshots, string defectId)
        {
            string[] screenshotsPaths = screenshots.Split(',');
            foreach (string screenshotsPath in screenshotsPaths)
            {
                if (screenshotsPath.Length > 0)
                {
                    var attachments = jiraRepObj.AddAttachment(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL, defectId, screenshotsPath);
                    if (attachments.DataResult != null)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Added Attachment to defect");
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to add attachment to defect -" + attachments.AuthenticationResponseObj.ErrorDesc);
                    }
                }
            }
        }

        private JiraIssueExport CreateDefectData(KeyValuePair<Guid, Dictionary<string, string>> defectForOpening, List<ExternalItemFieldBase> defectsFields)
        {
            string jiraResource = string.Empty;
            JiraHelper jiraHelper = new JiraHelper();
            jiraHelper.TryGetJiraResource(ResourceType.DEFECT, out jiraResource);

            JiraIssueExport jiraIssue = new JiraIssueExport
            {
                issueType = jiraResource,
                self = defectForOpening.Key.ToString(),
                resourceType = ResourceType.Defect
            };
            jiraIssue.ExportFields.Add("project", [new JiraExportData() { value = ALMCore.DefaultAlmConfig.ALMProjectKey }]);
            jiraIssue.ExportFields.Add("summary", [new JiraExportData() { value = defectForOpening.Value.ContainsKey("Summary") ? defectForOpening.Value["Summary"] : string.Empty }]);
            jiraIssue.ExportFields.Add("description", [new JiraExportData() { value = defectForOpening.Value.ContainsKey("description") ? defectForOpening.Value["description"] : string.Empty }]);
            jiraIssue.ExportFields.Add("issuetype", [new JiraExportData() { value = jiraResource }]);
            this.CreateDefectFields(defectsFields, jiraIssue);
            return jiraIssue;
        }

        private void CreateDefectFields(List<ExternalItemFieldBase> defectsFields, JiraIssueExport exportData)
        {
            foreach (var item in defectsFields.Where(a => a.Mandatory || a.ToUpdate))
            {
                var issueTemplate = jiraRepObj.GetFieldFromTemplateByName(ResourceType.DEFECT, ALMCore.DefaultAlmConfig.ALMProjectName, item.Name);
                if (issueTemplate == null || exportData.ExportFields.ContainsKey(issueTemplate.key))
                {
                    continue;
                }

                if (issueTemplate != null)
                {
                    exportData.ExportFields.Add(issueTemplate.key, [new JiraExportData() { value = item.SelectedValue }]);
                }
            }
        }

        public bool ExportBfToAlm(BusinessFlow businessFlow, IEnumerable<ExternalItemFieldBase> testCaseFields, IEnumerable<ExternalItemFieldBase> testSetFields, IEnumerable<ExternalItemFieldBase> testExecutionFields, ref string responseStr)
        {
            List<JiraIssueExport> exportData = [];
            bool result = false;
            var bftestCases = businessFlow.ActivitiesGroups.ToList();
            foreach (var tc in bftestCases)
            {
                this.ExportActivitesGrToJira(tc, testCaseFields, ref responseStr);
            }
            List<IJiraExportData> tcArray = CreateExportArrayFromActivites(bftestCases);
            JiraIssueExport jiraIssue = CreateJiraTestSet(businessFlow, testSetFields, testCaseFields, tcArray);
            exportData.Add(jiraIssue);
            var exportResponse = jiraRepObj.ExportJiraIssues(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL, exportData, ALMCore.DefaultAlmConfig.UseToken);
            if (exportResponse.Count > 0 && exportResponse.First().AuthenticationResponseObj.ErrorCode == 0)
            {
                if (string.IsNullOrEmpty(businessFlow.ExternalID) && exportResponse.First().DataResult != null)
                {
                    businessFlow.ExternalID = exportResponse.First().DataResult.key;
                }
                result = true;
                this.UpdateTestCaseLabel(bftestCases);
            }
            else
            {
                responseStr = exportResponse.FirstOrDefault().AuthenticationResponseObj.ErrorDesc;
            }

            return result;
        }

        public bool ExportBfToZephyr(BusinessFlow businessFlow, IEnumerable<ExternalItemFieldBase> testCaseFields,
                                        IEnumerable<ExternalItemFieldBase> testSetFields, IEnumerable<ExternalItemFieldBase> testExecutionFields,
                                        ref string responseStr, string versionId, string cycleId)
        {
            List<JiraIssueExport> exportData = [];
            bool result = false;
            var bftestCases = businessFlow.ActivitiesGroups.ToList();
            foreach (var tc in bftestCases)
            {
                tc.ExternalID = null;
                this.ExportActivitesGrToJira(tc, testCaseFields, ref responseStr);
                CreateZyphyrTestSteps(tc, tc.ExternalID, businessFlow);
            }
            if (cycleId == string.Empty)
            {
                JiraZephyrResponse response = CreateZephyrCycle(businessFlow, Convert.ToInt32(versionId));
                if ((response != null) && (response.id != 0))
                {
                    businessFlow.ExternalID = response.id.ToString();
                    result = AddTestsToZephyrCycle(bftestCases, versionId, businessFlow.ExternalID);
                }
            }
            else
            {
                long folderId = CreateZephyrFolder(businessFlow, Convert.ToInt32(versionId), Convert.ToInt32(cycleId));
                if (folderId != 0)
                {
                    businessFlow.ExternalID = cycleId;
                    businessFlow.ExternalID2 = folderId.ToString();
                    result = AddTestsToZephyrCycle(bftestCases, versionId, businessFlow.ExternalID, folderId);
                }
                else
                {
                    result = false;

                }
            }
            return result;
        }

        private void UpdateTestCaseLabel(List<ActivitiesGroup> bftestCases)
        {
            foreach (var tc in bftestCases)
            {
                Dictionary<string, List<IJiraExportData>> fields = new Dictionary<string, List<IJiraExportData>>
                {
                    { "labels", [new JiraExportData() { value = tc.ExternalID2 }] }
                };
                List<JiraIssueExport> tcList = [this.UpdateTestCaseWithSpecificFields(tc, fields)];
                var exportResponse = jiraRepObj.ExportJiraIssues(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL, tcList, ALMCore.DefaultAlmConfig.UseToken);
            }
        }

        public bool ExecuteDataToJira(BusinessFlow bizFlow, PublishToALMConfig publishToALMConfig, ref string result)
        {
            bool resultFlag = false;
            if (string.IsNullOrEmpty(bizFlow.ExternalID) || bizFlow.ExternalID.Equals("0"))
            {
                if (bizFlow.ALMTestSetLevel == "RunSet")
                {
                    result = $"{GingerDicser.GetTermResValue(eTermResKey.RunSet)}: {bizFlow.Name} is missing ExternalID, cannot export JIRA TestPlan execution results without External ID";
                    Reporter.ToLog(eLogLevel.ERROR, result);
                }
                else
                {
                    result = $"{GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)}: {bizFlow.Name} is missing ExternalID, cannot export JIRA TestPlan execution results without External ID";
                    Reporter.ToLog(eLogLevel.ERROR, result);
                }

                return resultFlag;
            }
            else if (bizFlow.ActivitiesGroups.Count == 0)
            {
                if (bizFlow.ALMTestSetLevel == "RunSet")
                {
                    result = $"{GingerDicser.GetTermResValue(eTermResKey.RunSet)}: {bizFlow.Name} Must have at least one {GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)}";
                }
                else
                {
                    result = $"{GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)}: {bizFlow.Name} Must have at least one {GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup)}";
                }

                return resultFlag;
            }
            else
            {
                if (string.IsNullOrEmpty(publishToALMConfig.VariableForTCRunName))
                {
                    String timeStamp = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss");
                    publishToALMConfig.VariableForTCRunName = "GingerRun_" + timeStamp;
                }

                //Create new TEST_EXECUTION for each execution result publish operation
                var bftestCases = bizFlow.ActivitiesGroups.ToList();
                List<IJiraExportData> tcArray = CreateExportArrayFromActivites(bftestCases);

                //Get updated fields
                ObservableList<ExternalItemFieldBase> exitingFields = new ObservableList<ExternalItemFieldBase>(WorkSpace.Instance.Solution.ExternalItemsFields);
                JiraImportManager jiraImportObj = new JiraImportManager(jiraRepObj);
                ObservableList<ExternalItemFieldBase> latestALMFields = jiraImportObj.GetALMItemFields(ResourceType.ALL, null, true);
                ALMCore AlmCore = new JiraCore();
                ObservableList<ExternalItemFieldBase> mergedFields = AlmCore.RefreshALMItemFields(exitingFields, latestALMFields);

                var testExecutionFields = mergedFields.Where(a => a.ItemType == "TEST_EXECUTION" && (a.ToUpdate || a.Mandatory));
                var isCreated = CreateTestExecution(bizFlow, tcArray, testExecutionFields);
                //get the Test set TC's                

                foreach (var actGroup in bizFlow.ActivitiesGroups)
                {
                    var testExecutionData = CreateTestRunData(actGroup);
                    var relevantTcRun = testExecutionData.FirstOrDefault(a => a.TestExecutionId == bizFlow.AlmData);
                    if (relevantTcRun != null && !string.IsNullOrEmpty(relevantTcRun.TestCaseRunId))
                    {
                        List<Activity> activities = (bizFlow.Activities.Where(x => x.ActivitiesGroupID == actGroup.Name)).Select(a => a).ToList();
                        JiraRunStatus runs = new JiraRunStatus
                        {
                            comment = publishToALMConfig.VariableForTCRunName
                        };
                        JiraRunStepStautusColl stepColl = [];
                        runs.steps = stepColl;
                        foreach (var act in activities)
                        {
                            RunStatus jiraStatus = ConvertGingerStatusToJira(act.Status.HasValue ? act.Status.Value : eRunStatus.NA);
                            string comment = CreateCommentForRun(act.Acts.ToList());
                            stepColl.Add(new JiraRunStepStautus() { comment = comment, status = jiraStatus });
                        }
                        resultFlag = jiraRepObj.ExecuteRunStatusBySteps(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL, runs, relevantTcRun.TestCaseRunId, ALMCore.DefaultAlmConfig.UseToken);

                        // Attach ActivityGroup execution Report if needed(if Checkbox is selected in GUI)
                        if (publishToALMConfig.ToAttachActivitiesGroupReport)
                        {
                            if ((actGroup.TempReportFolder != null) && (actGroup.TempReportFolder != string.Empty) &&
                            (System.IO.Directory.Exists(actGroup.TempReportFolder)))
                            {
                                //Creating the Zip file - start
                                string targetZipPath = System.IO.Directory.GetParent(actGroup.TempReportFolder).ToString();
                                string zipFileName = targetZipPath + "\\" + actGroup.Name.ToString() + "_GingerHTMLReport.zip";
                                if (System.IO.File.Exists(zipFileName))
                                {
                                    System.IO.File.Delete(zipFileName);
                                }
                                ZipFile.CreateFromDirectory(actGroup.TempReportFolder, zipFileName);
                                System.IO.Directory.Delete(actGroup.TempReportFolder, true);
                                //Creating the Zip file - finish                                
                                if (this.jiraRepObj.AddAttachment(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL, relevantTcRun.TestExecutionId, zipFileName) == null)
                                {
                                    Reporter.ToLog(eLogLevel.ERROR, "Failed to create attachment");
                                }
                                System.IO.File.Delete(zipFileName);
                            }
                        }


                        if (publishToALMConfig.ToExportReportLink)
                        {
                            if (!string.IsNullOrEmpty(publishToALMConfig.HtmlReportUrl))
                            {
                                string reportLink = General.CreateReportLinkPerFlow(HtmlReportUrl: publishToALMConfig.HtmlReportUrl, ExecutionId: publishToALMConfig.ExecutionId, BusinessFlowInstanceGuid: bizFlow.InstanceGuid.ToString());
                                reportLink = $"[Ginger Report Link|{reportLink}]";

                                if (this.jiraRepObj.AddComment(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL, relevantTcRun.TestExecutionId, reportLink) == null)
                                {
                                    Reporter.ToLog(eLogLevel.ERROR, "failed to add comment");
                                }

                            }
                        }
                    }
                    else
                    {
                        resultFlag = false;
                    }
                }
            }
            if (resultFlag)
            {
                result = "Export has been finished successfully";
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to Export to ALM, Error in Executing JIRA Run Status by Steps ");
            }

            return resultFlag;
        }

        public bool ExportExecutionDetailsToJiraZephyr(BusinessFlow bizFlow, PublishToALMConfig publishToALMConfig, ref string result)
        {
            bool resultFlag = true;
            if (bizFlow.ExternalID != "0" && (!String.IsNullOrEmpty(bizFlow.ExternalID)))
            {
                try
                {
                    List<JiraZephyrExecution> executionList = jmz.GetZephyrExecutionList(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword,
                                                                                                                                        ALMCore.DefaultAlmConfig.ALMServerURL, string.Empty, string.Empty,
                                                                                                                                        ALMCore.DefaultAlmConfig.ALMProjectKey, bizFlow.ExternalID, string.Empty, string.Empty,
                                                                                                                                        string.Empty, string.Empty, bizFlow.ExternalID2 == null ? string.Empty : bizFlow.ExternalID2).DataResult;
                    if ((executionList != null) && (executionList.Count > 0))
                    {
                        ZephyrExecutionStatus zephyrStepStatus;
                        ZephyrExecutionStatus zephyrIssueStatus;
                        if (string.IsNullOrEmpty(publishToALMConfig.VariableForTCRunName))
                        {
                            String timeStamp = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss");
                            publishToALMConfig.VariableForTCRunName = "GingerRun_" + timeStamp;
                        }
                        foreach (var actGroup in bizFlow.ActivitiesGroups)
                        {
                            List<Activity> activities = (bizFlow.Activities.Where(x => x.ActivitiesGroupID == actGroup.Name)).Select(a => a).ToList();
                            JiraZephyrExecution currentActivitiesGroupExecution = executionList.FirstOrDefault(z => z.IssueId.ToString() == actGroup.ExternalID);
                            if (currentActivitiesGroupExecution != null)
                            {
                                List<JiraZephyrStepResult> stepResults = jmz.GetZephyrStepResultsList(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword,
                                                                                                                                                ALMCore.DefaultAlmConfig.ALMServerURL, currentActivitiesGroupExecution.Id).DataResult;
                                foreach (var act in activities)
                                {
                                    zephyrStepStatus = ConvertGingerStatusToZephyr(act.Status.HasValue ? act.Status.Value : eRunStatus.NA);
                                    JiraZephyrStepResult currentStepResult = stepResults.FirstOrDefault(z => z.StepId.ToString() == act.ExternalID);
                                    if (currentStepResult != null)
                                    {
                                        currentStepResult = jmz.UpdateZephyrStepResult(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword,
                                                                                                                                     ALMCore.DefaultAlmConfig.ALMServerURL, currentStepResult.Id,
                                                                                                                                     ((int)zephyrStepStatus)).DataResult;
                                    }
                                    else
                                    {
                                        currentStepResult = jmz.CreateZephyrStepResult(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword,
                                                                                                                                     ALMCore.DefaultAlmConfig.ALMServerURL,
                                                                                                                                     new JiraZephyrStepResult(Convert.ToInt32(act.ExternalID),
                                                                                                                                                                currentActivitiesGroupExecution.IssueId.ToString(),
                                                                                                                                                                currentActivitiesGroupExecution.Id,
                                                                                                                                                                ((int)zephyrStepStatus).ToString())).DataResult;
                                    }
                                    if (currentStepResult == null)
                                    {
                                        resultFlag = false;
                                    }
                                }
                                if (resultFlag)
                                {
                                    zephyrIssueStatus = ConvertGingerStatusToZephyr(actGroup.RunStatus);
                                    currentActivitiesGroupExecution = jmz.UpdateExecution(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword,
                                                                                                                                         ALMCore.DefaultAlmConfig.ALMServerURL, currentActivitiesGroupExecution.Id,
                                                                                                                                         ((int)zephyrIssueStatus)).DataResult;
                                    if (currentActivitiesGroupExecution == null)
                                    {
                                        resultFlag = false;
                                    }
                                }
                            }
                            else
                            {
                                resultFlag = false;
                            }
                        }
                    }
                }
                catch
                {
                    resultFlag = false;
                }
            }
            if (resultFlag)
            {
                result = "Export has been finished successfully";
            }
            else
            {
                result = "Error Has been Happened while export to execution results to Jira-Zephyr";
            }

            return resultFlag;
        }

        public void AssignDefectsToZephyrExecutions(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, Dictionary<Guid, string> defectsOpeningResults)
        {
            try
            {
                if ((defectsForOpening != null) && (defectsForOpening.Count > 0))
                {
                    List<JiraZephyrExecution> executionList = jmz.GetZephyrExecutionList(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword,
                                                                                                                                            ALMCore.DefaultAlmConfig.ALMServerURL, string.Empty, string.Empty,
                                                                                                                                            ALMCore.DefaultAlmConfig.ALMProjectKey, defectsForOpening.First().Value["BFExternalID1"], string.Empty, string.Empty,
                                                                                                                                            string.Empty, string.Empty, defectsForOpening.First().Value["BFExternalID2"] == null ? string.Empty : defectsForOpening.First().Value["BFExternalID2"]).DataResult;

                    foreach (KeyValuePair<Guid, string> defectOpeningResult in defectsOpeningResults)
                    {
                        KeyValuePair<Guid, Dictionary<string, string>> currentDefect = defectsForOpening.FirstOrDefault(z => z.Key == defectOpeningResult.Key);
                        JiraZephyrExecution currentActivitiesGroupExecution = executionList.FirstOrDefault(z => z.IssueId.ToString() == currentDefect.Value["ActivityGroupExternalID"]);
                        if (currentDefect.Key != null)
                        {
                            JiraZephyrResponse jiraZephyrResponse = jmz.UpdateExecutionWithDefects(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL,
                                                                                                                                                [currentActivitiesGroupExecution.Id],
                                                                                                                                                [defectOpeningResult.Value]).DataResult;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error by assigning defects to Zephyr's executions", ex);
            }
        }

        private string CreateCommentForRun(List<IAct> list)
        {
            StringBuilder sb = new StringBuilder();
            if (list.Exists(a => !string.IsNullOrEmpty(a.Error)))
            {
                sb.Append("Errors:");
                list.ForEach(x =>
                    {
                        sb.AppendLine();
                        sb.Append(x.Error);
                    });
            }
            return sb.ToString();
        }

        private RunStatus ConvertGingerStatusToJira(eRunStatus runStatus)
        {
            RunStatus responseStatus = RunStatus.EXECUTING;
            responseStatus = runStatus switch
            {
                eRunStatus.Failed => RunStatus.FAIL,
                eRunStatus.Passed => RunStatus.PASS,
                eRunStatus.Skipped or eRunStatus.Blocked or eRunStatus.Pending => RunStatus.TODO,
                _ => RunStatus.TODO,
            };
            return responseStatus;
        }

        private ZephyrExecutionStatus ConvertGingerStatusToZephyr(eActivitiesGroupRunStatus runStatus)
        {
            ZephyrExecutionStatus responseStatus = ZephyrExecutionStatus.WIP;
            responseStatus = runStatus switch
            {
                eActivitiesGroupRunStatus.Failed => ZephyrExecutionStatus.FAIL,
                eActivitiesGroupRunStatus.Passed => ZephyrExecutionStatus.PASS,
                eActivitiesGroupRunStatus.Skipped => ZephyrExecutionStatus.UNEXECUTED,
                eActivitiesGroupRunStatus.Blocked => ZephyrExecutionStatus.BLOCKED,
                eActivitiesGroupRunStatus.Pending => ZephyrExecutionStatus.UNEXECUTED,
                _ => ZephyrExecutionStatus.UNEXECUTED,
            };
            return responseStatus;
        }

        private ZephyrExecutionStatus ConvertGingerStatusToZephyr(eRunStatus runStatus)
        {
            ZephyrExecutionStatus responseStatus = ZephyrExecutionStatus.WIP;
            responseStatus = runStatus switch
            {
                eRunStatus.Failed => ZephyrExecutionStatus.FAIL,
                eRunStatus.Passed => ZephyrExecutionStatus.PASS,
                eRunStatus.Skipped => ZephyrExecutionStatus.UNEXECUTED,
                eRunStatus.Blocked => ZephyrExecutionStatus.BLOCKED,
                eRunStatus.Pending => ZephyrExecutionStatus.UNEXECUTED,
                _ => ZephyrExecutionStatus.UNEXECUTED,
            };
            return responseStatus;
        }

        private bool CreateTestExecution(BusinessFlow businessFlow, List<IJiraExportData> tcArray, IEnumerable<ExternalItemFieldBase> testExecutionFields)
        {
            bool result = true;
            List<JiraIssueExport> exportData = [];
            JiraIssueExport jiraIssue = new JiraIssueExport
            {
                issueType = "Test Execution",
                resourceType = ResourceType.TEST_CASE_EXECUTION_RECORDS
            };
            jiraIssue.ExportFields.Add("project", [new JiraExportData() { value = ALMCore.DefaultAlmConfig.ALMProjectKey }]);
            jiraIssue.ExportFields.Add("summary", [new JiraExportData() { value = businessFlow.Name + " Test Execution" + DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss") }]);
            jiraIssue.ExportFields.Add("description", [new JiraExportData() { value = businessFlow.Description + " Test Execution" }]);
            jiraIssue.ExportFields.Add("issuetype", [new JiraExportData() { value = "Test Execution" }]);
            jiraIssue.ExportFields.Add("reporter", [new JiraExportData() { value = ALMCore.DefaultAlmConfig.ALMUserName }]);
            foreach (var item in testExecutionFields)
            {
                var issueTemplate = jiraRepObj.GetFieldFromTemplateByName(ResourceType.TEST_CASE_EXECUTION_RECORDS, ALMCore.DefaultAlmConfig.ALMProjectName, item.Name);
                if (issueTemplate == null || jiraIssue.ExportFields.ContainsKey(issueTemplate.key))
                {
                    continue;
                }

                if (issueTemplate != null)
                {
                    jiraIssue.ExportFields.Add(issueTemplate.key, [new JiraExportData() { value = item.SelectedValue }]);
                }
            }
            var testCaseTemplate = jiraRepObj.GetFieldFromTemplateByName(ResourceType.TEST_CASE_EXECUTION_RECORDS, ALMCore.DefaultAlmConfig.ALMProjectKey, "Test Cases");
            if (testCaseTemplate != null && tcArray.Count > 0)
            {
                jiraIssue.ExportFields.Add(testCaseTemplate.key, tcArray);
            }

            exportData.Add(jiraIssue);
            var exportExecutionResponse = jiraRepObj.ExportJiraIssues(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL, exportData, ALMCore.DefaultAlmConfig.UseToken);
            SetBusinessFlowAlmData(exportExecutionResponse, businessFlow);
            SetDataFromTestExecution(exportExecutionResponse, testCaseTemplate, businessFlow, businessFlow.AlmData);
            return result;
        }

        private void SetBusinessFlowAlmData(List<AlmResponseWithData<JiraIssue>> exportExecutionResponse, BusinessFlow businessFlow)
        {
            if (exportExecutionResponse.Count > 0 && exportExecutionResponse.First().AuthenticationResponseObj.ErrorCode == 0)
            {
                var testExecution = exportExecutionResponse.First();
                if (testExecution.DataResult != null)
                {
                    businessFlow.AlmData = testExecution.DataResult.key;
                }
            }
        }

        private List<TestRunData> CreateTestRunData(ActivitiesGroup activitiesGroup)
        {
            List<TestRunData> response = [];
            if (!string.IsNullOrEmpty(activitiesGroup.ExternalID2))
            {
                var tcRuns = activitiesGroup.ExternalID2.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                foreach (var run in tcRuns)
                {
                    var resultArr = run.Split(new string[] { "***" }, StringSplitOptions.RemoveEmptyEntries);
                    if (resultArr.Length == 2)
                    {
                        response.Add(new TestRunData() { TestExecutionId = resultArr[0], TestCaseRunId = resultArr[1] });
                    }
                }
            }
            return response;
        }

        private void SetDataFromTestExecution(List<AlmResponseWithData<JiraIssue>> exportExecutionResponse, FieldSchema testCaseTemplate, BusinessFlow businessFlow, string testExecutionKey)
        {

            var thisTestExecution = jiraRepObj.GetJiraIssueById(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL, ALMCore.DefaultAlmConfig.ALMProjectName, testExecutionKey, ResourceType.TEST_CASE_EXECUTION_RECORDS, ALMCore.DefaultAlmConfig.UseToken);
            int jiraVersion = jiraRepObj.GetJiraVersion(ALMCore.DefaultAlmConfig.ALMServerURL);
            if (thisTestExecution != null && thisTestExecution.fields != null)
            {
                dynamic dc = null;
                thisTestExecution.fields.TryGetValue(testCaseTemplate.key, out dc);
                if (dc != null)
                {
                    if (jiraVersion >= 9)
                    {
                        string testkey;
                        string testRunId;
                        foreach (var tc in dc)
                        {
                            // tc is coming as JObject, extracting the values of testkey and tesrunId
                            testkey = tc["testKey"].ToString();
                            testRunId = tc["testRunId"].ToString();

                            // converting testRunId value to tc.c.Value
                            tc["c"] = new JObject(new JProperty("Value", testRunId));

                            var pattern = $"{testExecutionKey}***{testRunId}";
                            foreach (var b in businessFlow.ActivitiesGroups.Where(a => a.ExternalID == testkey))
                            {
                                var tcRuns = (b.ExternalID2 ?? "").Split(new[] { "||" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                if (!tcRuns.Contains(pattern))
                                {

                                    tcRuns.Add(pattern);
                                }

                                b.ExternalID2 = string.Join("||", tcRuns);
                            }
                        }
                    }
                    else if (jiraVersion <= 8)
                    {
                        foreach (var tc in dc)
                        {
                            businessFlow.ActivitiesGroups.Where(a => a.ExternalID == tc.b.Value).ToList().ForEach(b =>
                            {
                                string patern = testExecutionKey + "***" + tc.c.Value;
                                List<string> tcRuns = [];
                                if (!string.IsNullOrEmpty(b.ExternalID2))
                                {
                                    tcRuns = b.ExternalID2.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                }

                                if (!tcRuns.Contains(patern))
                                {
                                    tcRuns.Add(patern);
                                }

                                b.ExternalID2 = string.Join("||", tcRuns);
                            });
                        }
                    }
                }
            }

        }

        private List<IJiraExportData> CreateExportArrayFromActivites(List<ActivitiesGroup> bftestCases)
        {
            List<IJiraExportData> response = [];
            for (var a = 0; a < bftestCases.Count; a++)
            {
                response.Add(new JiraExportData() { id = a.ToString(), value = bftestCases[a].ExternalID });
            }
            return response;
        }

        private JiraIssueExport CreateJiraTestSet(BusinessFlow businessFlow, IEnumerable<ExternalItemFieldBase> testSetFields, IEnumerable<ExternalItemFieldBase> testCaseFields, List<IJiraExportData> tcIds)
        {
            JiraIssueExport jiraIssue = new JiraIssueExport
            {
                name = businessFlow.Name,
                key = businessFlow.ExternalID
            };
            CreateTestSetFields(businessFlow, testSetFields, jiraIssue, tcIds);
            return jiraIssue;
        }

        private JiraZephyrResponse CreateZephyrCycle(BusinessFlow businessFlow, long versionId)
        {
            JiraZephyrCycle zephyrCycle = new JiraZephyrCycle(ALMCore.DefaultAlmConfig.ALMProjectKey, businessFlow.Name, businessFlow.Description,
                                                                DateTime.Now.ToString("d/MMM/y"), DateTime.Now.ToString("d/MMM/y"), versionId);
            JiraZephyrResponse response = jmz.CreateZephyrCycle(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword,
                                                                                                                ALMCore.DefaultAlmConfig.ALMServerURL, zephyrCycle).DataResult;
            return response;
        }

        private long CreateZephyrFolder(BusinessFlow businessFlow, long versionId, long cycleId)
        {
            long folderId = 0;
            JiraZephyrCycleFolder zephyrFolder = new JiraZephyrCycleFolder(cycleId, Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), versionId,
                                                                            1, businessFlow.Name, businessFlow.Description);
            JiraZephyrResponse response = jmz.CreateZephyrCycleFolder(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword,
                                                                                                                    ALMCore.DefaultAlmConfig.ALMServerURL, zephyrFolder).DataResult;

            if ((response != null) && (response.id != 0))
            {
                folderId = response.id;
            }
            else
            {
                System.Threading.Thread.Sleep(1000);
                List<JiraZephyrCycleFolder> foldersList = GetCycleFoldersList(businessFlow, Convert.ToInt32(versionId), Convert.ToInt32(cycleId));
                JiraZephyrCycleFolder currentFolder = foldersList.FirstOrDefault(z => z.FolderName == businessFlow.Name);
                if (currentFolder == null)
                {
                    folderId = 0;
                }
                else
                {
                    folderId = currentFolder.FolderId;
                }
            }
            return folderId;
        }

        private List<JiraZephyrCycleFolder> GetCycleFoldersList(BusinessFlow businessFlow, long versionId, long cycleId)
        {
            List<JiraZephyrCycleFolder> response = jmz.GetCycleFoldersList(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword,
                                                                                                                            ALMCore.DefaultAlmConfig.ALMServerURL, ALMCore.DefaultAlmConfig.ALMProjectKey,
                                                                                                                            versionId.ToString(), cycleId.ToString(), string.Empty, string.Empty).DataResult;
            return response;
        }

        private bool AddTestsToZephyrCycle(List<ActivitiesGroup> bftestCases, string versionId, string cycleId, long folderId = -1)
        {
            JiraZephyrResponse response = jmz.AddTestsToCycle(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword,
                                                                                                            ALMCore.DefaultAlmConfig.ALMServerURL, versionId, cycleId,
                                                                                                            folderId, ALMCore.DefaultAlmConfig.ALMProjectKey,
                                                                                                            bftestCases.Select(z => z.ExternalID2.ToString()).ToList()).DataResult;
            return response.jobProgressToken != null;
        }

        private void CreateTestSetFields(BusinessFlow businessFlow, IEnumerable<ExternalItemFieldBase> testSetFields, JiraIssueExport jiraIssue, List<IJiraExportData> tcIds)
        {
            jiraIssue.resourceType = ResourceType.TEST_SET;
            jiraIssue.ExportFields.Add("project", [new JiraExportData() { value = ALMCore.DefaultAlmConfig.ALMProjectKey }]);
            jiraIssue.ExportFields.Add("summary", [new JiraExportData() { value = businessFlow.Name }]);
            jiraIssue.ExportFields.Add("description", [new JiraExportData() { value = businessFlow.Description }]);
            jiraIssue.ExportFields.Add("issuetype", [new JiraExportData() { value = "Test Set" }]);
            jiraIssue.ExportFields.Add("reporter", [new JiraExportData() { value = ALMCore.DefaultAlmConfig.ALMUserName }]);

            foreach (var item in testSetFields)
            {
                var issueTemplate = jiraRepObj.GetFieldFromTemplateByName(ResourceType.TEST_SET, ALMCore.DefaultAlmConfig.ALMProjectName, item.Name);
                if (issueTemplate == null || jiraIssue.ExportFields.ContainsKey(issueTemplate.key))
                {
                    continue;
                }

                if (issueTemplate != null)
                {
                    jiraIssue.ExportFields.Add(issueTemplate.key, [new JiraExportData() { value = item.SelectedValue }]);
                }
            }
            var testCaseTemplate = jiraRepObj.GetFieldFromTemplateByName(ResourceType.TEST_SET, ALMCore.DefaultAlmConfig.ALMProjectName, "Test Cases");
            if (testCaseTemplate != null && tcIds.Count > 0)
            {
                jiraIssue.ExportFields.Add(testCaseTemplate.key, tcIds);
            }
        }

        public bool ExportActivitesGrToJira(ActivitiesGroup activtiesGroup, IEnumerable<ExternalItemFieldBase> testCaseFields, ref string errorResult)
        {
            List<JiraIssueExport> exportData = [];
            bool result = false;
            var testCaseFilterdFields = testCaseFields.Where(tc => tc.ToUpdate || tc.Mandatory);
            JiraIssueExport jiraIssue = CreateJiraTestCase(activtiesGroup, testCaseFilterdFields);
            exportData.Add(jiraIssue);
            var exportResponse = jiraRepObj.ExportJiraIssues(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL, exportData, ALMCore.DefaultAlmConfig.UseToken);

            if (exportResponse.Count > 0 && exportResponse.First().AuthenticationResponseObj.ErrorCode == 0)
            {
                if (string.IsNullOrEmpty(activtiesGroup.ExternalID))
                {
                    if (ALMCore.DefaultAlmConfig.JiraTestingALM == GingerCoreNET.ALMLib.ALMIntegrationEnums.eTestingALMType.Zephyr)
                    {
                        activtiesGroup.ExternalID = exportResponse.First().DataResult.id.ToString();
                        activtiesGroup.ExternalID2 = exportResponse.First().DataResult.key;
                    }
                    else
                    {
                        activtiesGroup.ExternalID = exportResponse.First().DataResult.key;
                    }
                }
                result = true;
            }
            else
            {
                errorResult = exportResponse.FirstOrDefault().AuthenticationResponseObj.ErrorDesc;
            }

            return result;
        }

        private JiraIssueExport UpdateTestCaseWithSpecificFields(ActivitiesGroup activtiesGroup, Dictionary<string, List<IJiraExportData>> fields)
        {
            JiraIssueExport jiraIssue = new JiraIssueExport
            {
                ProjectKey = ALMCore.DefaultAlmConfig.ALMProjectName,
                key = activtiesGroup.ExternalID
            };
            fields.ToList().ForEach(x => jiraIssue.ExportFields.Add(x.Key, x.Value));
            return jiraIssue;
        }

        private JiraIssueExport CreateJiraTestCase(ActivitiesGroup activtiesGroup, IEnumerable<ExternalItemFieldBase> issueFields)
        {
            JiraIssueExport jiraIssue = new JiraIssueExport
            {
                ProjectKey = ALMCore.DefaultAlmConfig.ALMProjectName,
                key = activtiesGroup.ExternalID
            };
            if (ALMCore.DefaultAlmConfig.JiraTestingALM == GingerCoreNET.ALMLib.ALMIntegrationEnums.eTestingALMType.Zephyr)
            {
                CreateTestCaseFields(activtiesGroup, issueFields, jiraIssue, ResourceType.TEST);
            }
            else
            {
                CreateTestCaseFields(activtiesGroup, issueFields, jiraIssue);
                CreateTestCaseSteps(activtiesGroup.ActivitiesIdentifiers, jiraIssue);
            }
            return jiraIssue;
        }

        private void CreateTestCaseFields(ActivitiesGroup activtiesGroup, IEnumerable<ExternalItemFieldBase> issueFields,
                                            JiraIssueExport jiraIssue, ResourceType resourceType = ResourceType.TEST_CASE)
        {
            jiraIssue.name = activtiesGroup.Name;
            jiraIssue.resourceType = resourceType;
            jiraIssue.ExportFields.Add("project", [new JiraExportData() { value = ALMCore.DefaultAlmConfig.ALMProjectKey }]);
            jiraIssue.ExportFields.Add("summary", [new JiraExportData() { value = activtiesGroup.Name }]);
            jiraIssue.ExportFields.Add("description", [new JiraExportData() { value = activtiesGroup.Description }]);
            jiraIssue.ExportFields.Add("issuetype", [new JiraExportData() { value = "Test" }]);
            jiraIssue.ExportFields.Add("reporter", [new JiraExportData() { value = ALMCore.DefaultAlmConfig.ALMUserName }]);
            if (!string.IsNullOrEmpty(activtiesGroup.ExternalID2))
            {
                jiraIssue.ExportFields.Add("labels", [new JiraExportData() { value = activtiesGroup.ExternalID2 }]);
            }

            foreach (var item in issueFields)
            {
                var issueTemplate = jiraRepObj.GetFieldFromTemplateByName(ResourceType.TEST_CASE, ALMCore.DefaultAlmConfig.ALMProjectName, item.Name);
                if (issueTemplate == null || jiraIssue.ExportFields.ContainsKey(issueTemplate.key))
                {
                    continue;
                }

                if (issueTemplate != null)
                {
                    jiraIssue.ExportFields.Add(issueTemplate.key, [new JiraExportData() { value = item.SelectedValue }]);
                }
            }
        }

        private void CreateZyphyrTestSteps(ActivitiesGroup tc, string isssueId, BusinessFlow bf)
        {
            for (var a = 0; a < tc.ActivitiesIdentifiers.Count; a++)
            {
                var activity = tc.ActivitiesIdentifiers[a];
                JiraZephyrTeststepCollection steps = jmz.CreateZephyrTestStep(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword,
                                                                                                                              ALMCore.DefaultAlmConfig.ALMServerURL, Convert.ToInt32(isssueId),
                                                                                                                              new JiraZephyrTeststep(activity.IdentifiedActivity.ActivityName,
                                                                                                                                                        activity.IdentifiedActivity.Description,
                                                                                                                                                        activity.IdentifiedActivity.Expected)).DataResult;
                if ((steps != null) && (steps.stepBeanCollection.Count > 0))
                {
                    bf.Activities.FirstOrDefault(z => z.Guid == activity.ActivityGuid).ExternalID = steps.stepBeanCollection[0].id.ToString();
                }
            }
        }

        private void CreateTestCaseSteps(ObservableList<ActivityIdentifiers> activitiesIdentifiers, JiraIssueExport jiraIssue)
        {
            var steps = new List<IJiraExportData>();
            var issueTemplate = jiraRepObj.GetFieldFromTemplateByName(ResourceType.TEST_CASE, ALMCore.DefaultAlmConfig.ALMProjectName, "Test Steps");
            if (issueTemplate != null)
            {
                for (var a = 0; a < activitiesIdentifiers.Count; a++)
                {
                    StringBuilder stepDataSb = new StringBuilder();
                    var activity = activitiesIdentifiers[a];
                    stepDataSb.Append("=>Description:");
                    stepDataSb.Append(activity.IdentifiedActivity.Description);
                    stepDataSb.AppendLine(CreateStringFromVars(activity.IdentifiedActivity.Variables));
                    stepDataSb.AppendLine(CreateStringFromActions(activity.IdentifiedActivity.Acts));

                    steps.Add(new JiraStepData()
                    {
                        step_index = (a + 1).ToString(),
                        step_name = activity.ActivityName,
                        step_result = activity.IdentifiedActivity.Expected,
                        step_data = stepDataSb.ToString()
                    });

                }
                jiraIssue.ExportFields.Add(issueTemplate.key, steps);
            }
        }

        private string CreateStringFromActions(ObservableList<IAct> acts)
        {
            StringBuilder variablesSb = new StringBuilder();
            for (var a = 0; a < acts.Count; a++)
            {
                if (a == 0)
                {
                    variablesSb.Append("\\n=>Actions: ");
                }

                variablesSb.Append("\\n");
                variablesSb.Append((a + 1).ToString());
                variablesSb.Append(")");
                variablesSb.Append(acts[a].Description);
                if (a < acts.Count - 1)
                {
                    variablesSb.Append(";");
                }
            }
            return variablesSb.ToString();
        }

        private string CreateStringFromVars(ObservableList<VariableBase> variables)
        {
            StringBuilder variablesSb = new StringBuilder();
            if (variables.Count > 0)
            {
                variablesSb.Append("\\n=>Variables: ");
                foreach (var param in variables)
                {
                    variablesSb.Append(param.Name);
                    variablesSb.Append("=");
                    variablesSb.Append(param.Value);
                    variablesSb.Append(";");
                }
            }
            if (variablesSb.Length > 0)
            {
                variablesSb.Remove(variablesSb.Length - 1, 1);
            }

            return variablesSb.ToString();
        }
    }
}
