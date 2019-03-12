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

using ALM_Common.DataContracts;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.ALM.JIRA.Data_Contracts;
using GingerCore.Variables;
using JiraRepository.Data_Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amdocs.Ginger.Common.InterfacesLib;


namespace GingerCore.ALM.JIRA.Bll
{
    public class JiraExportManager
    {
        private JiraRepository.JiraRepository jiraRepObj;
        public JiraExportManager(JiraRepository.JiraRepository jiraRep)
        {
            this.jiraRepObj = jiraRep;
        }

        public Dictionary<Guid, string> CreateNewALMDefects(Dictionary<Guid, Dictionary<string, string>> defectsForOpening, List<ExternalItemFieldBase> defectsFields)
        {
            Dictionary<Guid, string> defectsOpeningResults = new Dictionary<Guid, string>();
            List<JiraIssueExport> defectsToExport = new List<JiraIssueExport>();
            foreach (KeyValuePair<Guid, Dictionary<string, string>> defectForOpening in defectsForOpening)
            {
                defectsToExport.Add(this.CreateDefectData(defectForOpening, defectsFields));
            }
            var exportedDefects = jiraRepObj.ExportJiraIssues(ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword, ALMCore.AlmConfig.ALMServerURL, defectsToExport);
            for (var a = 0; a < exportedDefects.Count; a++)
            {
                if (defectsForOpening.Count > a)
                    defectsOpeningResults.Add(defectsForOpening.ElementAt(a).Key, exportedDefects[a].DataResult.key);
            }
            return defectsOpeningResults;
        }

        private JiraIssueExport CreateDefectData(KeyValuePair<Guid, Dictionary<string, string>> defectForOpening, List<ExternalItemFieldBase> defectsFields)
        {
            JiraIssueExport jiraIssue = new JiraIssueExport();
            jiraIssue.issueType = "Defect";
            jiraIssue.self = defectForOpening.Key.ToString();
            jiraIssue.resourceType = ALM_Common.DataContracts.ResourceType.DEFECT;
            jiraIssue.ExportFields.Add("project", new List<IJiraExportData>() { new JiraExportData() { value = ALMCore.AlmConfig.ALMProjectKey } });
            jiraIssue.ExportFields.Add("summary", new List<IJiraExportData>() { new JiraExportData() { value = defectForOpening.Value.ContainsKey("Summary") ? defectForOpening.Value["Summary"] : string.Empty } });
            jiraIssue.ExportFields.Add("description", new List<IJiraExportData>() { new JiraExportData() { value = defectForOpening.Value.ContainsKey("description") ? defectForOpening.Value["description"] : string.Empty } });
            jiraIssue.ExportFields.Add("issuetype", new List<IJiraExportData>() { new JiraExportData() { value = "Defect" } });
            jiraIssue.ExportFields.Add("reporter", new List<IJiraExportData>() { new JiraExportData() { value = ALMCore.AlmConfig.ALMUserName } });
            this.CreateDefectFields(defectsFields, jiraIssue);
            return jiraIssue;
        }

        private void CreateDefectFields(List<ExternalItemFieldBase> defectsFields, JiraIssueExport exportData)
        {
            foreach (var item in defectsFields.Where(a => a.Mandatory || a.ToUpdate))
            {
                var issueTemplate = jiraRepObj.GetFieldFromTemplateByName(ALM_Common.DataContracts.ResourceType.DEFECT, ALMCore.AlmConfig.ALMProjectKey, item.Name);
                if (issueTemplate == null || exportData.ExportFields.ContainsKey(issueTemplate.key))
                    continue;
                if (issueTemplate != null)
                {
                    exportData.ExportFields.Add(issueTemplate.key, new List<IJiraExportData>() { new JiraExportData() { value = item.SelectedValue } });
                }
            }
        }

        public bool ExportBfToAlm(BusinessFlow businessFlow, IEnumerable<ExternalItemFieldBase> testCaseFields, IEnumerable<ExternalItemFieldBase> testSetFields, IEnumerable<ExternalItemFieldBase> testExecutionFields, ref string responseStr)
        {
            List<JiraIssueExport> exportData = new List<JiraIssueExport>();
            bool result = false;
            var bftestCases = businessFlow.ActivitiesGroups.ToList();
            foreach (var tc in bftestCases)
            {
                this.ExportActivitesGrToJira(tc, testCaseFields, ref responseStr);
            }
            List<IJiraExportData> tcArray = CreateExportArrayFromActivites(bftestCases);
            JiraIssueExport jiraIssue = CreateJiraTestSet(businessFlow, testSetFields, testCaseFields, tcArray);
            exportData.Add(jiraIssue);
            var exportResponse = jiraRepObj.ExportJiraIssues(ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword, ALMCore.AlmConfig.ALMServerURL, exportData);
            if (exportResponse.Count > 0 && exportResponse.First().AuthenticationResponseObj.ErrorCode == 0)
            {
                if (string.IsNullOrEmpty(businessFlow.ExternalID) && exportResponse.First().DataResult != null)
                {
                    businessFlow.ExternalID = exportResponse.First().DataResult.key;
                }
                result = CreateTestExecution(businessFlow, tcArray, testExecutionFields);
                this.UpdateTestCaseLabel(bftestCases);
            }
            else
                responseStr = exportResponse.FirstOrDefault().AuthenticationResponseObj.ErrorDesc;
            return result;
        }

        private void UpdateTestCaseLabel(List<ActivitiesGroup> bftestCases)
        {
            foreach (var tc in bftestCases)
            {
                Dictionary<string, List<IJiraExportData>> fields = new Dictionary<string, List<IJiraExportData>>();
                fields.Add("labels", new List<IJiraExportData>() { new JiraExportData() { value = tc.ExternalID2 } });
                List<JiraIssueExport> tcList = new List<JiraIssueExport>();
                tcList.Add(this.UpdateTestCaseWithSpecificFields(tc, fields));
                var exportResponse = jiraRepObj.ExportJiraIssues(ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword, ALMCore.AlmConfig.ALMServerURL, tcList);
            }
        }

        public bool ExecuteDataToJira(BusinessFlow bizFlow, PublishToALMConfig publishToALMConfig, ref string result)
        {
            bool resultFlag = false;
            if (bizFlow.ExternalID != "0" && (!String.IsNullOrEmpty(bizFlow.ExternalID)))
            {
                if (string.IsNullOrEmpty(publishToALMConfig.VariableForTCRunName))
                {
                    String timeStamp = DateTime.Now.ToString("dd-MMM-yyyy HH:mm:ss");
                    publishToALMConfig.VariableForTCRunName = "GingerRun_" + timeStamp;
                }
                foreach (var actGroup in bizFlow.ActivitiesGroups)
                {
                    var testExecutionData = CreateTestRunData(actGroup);
                    var relevantTcRun = testExecutionData.FirstOrDefault(a => a.TestExecutionId == bizFlow.AlmData);
                    if (!string.IsNullOrEmpty(relevantTcRun.TestCaseRunId))
                    {
                        List<Activity> activities = (bizFlow.Activities.Where(x => x.ActivitiesGroupID == actGroup.Name)).Select(a => a).ToList();
                        JiraRunStatus runs = new JiraRunStatus();
                        runs.comment = publishToALMConfig.VariableForTCRunName;
                        JiraRunStepStautusColl stepColl = new JiraRunStepStautusColl();
                        runs.steps = stepColl;
                        foreach (var act in activities)
                        {
                            RunStatus jiraStatus = ConvertGingerStatusToJira(act.Status.HasValue ? act.Status.Value : eRunStatus.NA);
                            string comment = CreateCommentForRun(act.Acts.ToList());
                            stepColl.Add(new JiraRunStepStautus() { comment = comment, status = jiraStatus });
                        }
                        resultFlag = jiraRepObj.ExecuteRunStatusBySteps(ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword, ALMCore.AlmConfig.ALMServerURL, runs, relevantTcRun.TestCaseRunId);
                    }
                }
            }
            if (resultFlag)
                result = "Export has been finished successfully";
            else
                result = "Error Has been Happend while export to ALM";
            return resultFlag;
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
            switch (runStatus)
            {
                case eRunStatus.Failed:
                    responseStatus = RunStatus.FAIL;
                    break;
                case eRunStatus.Passed:
                    responseStatus = RunStatus.PASS;
                    break;
                case eRunStatus.Skipped:
                case eRunStatus.Blocked:
                case eRunStatus.Pending:
                    responseStatus = RunStatus.TODO;
                    break;
                default:
                    responseStatus = RunStatus.TODO;
                    break;
            }
            return responseStatus;
        }

        private bool CreateTestExecution(BusinessFlow businessFlow, List<IJiraExportData> tcArray, IEnumerable<ExternalItemFieldBase> testExecutionFields)
        {
            bool result = true;
            List<JiraIssueExport> exportData = new List<JiraIssueExport>();
            JiraIssueExport jiraIssue = new JiraIssueExport();
            jiraIssue.issueType = "Test Execution";
            jiraIssue.resourceType = ALM_Common.DataContracts.ResourceType.TEST_CASE_EXECUTION_RECORDS;
            jiraIssue.ExportFields.Add("project", new List<IJiraExportData>() { new JiraExportData() { value = ALMCore.AlmConfig.ALMProjectKey } });
            jiraIssue.ExportFields.Add("summary", new List<IJiraExportData>() { new JiraExportData() { value = businessFlow.Name + "Test Execution" } });
            jiraIssue.ExportFields.Add("description", new List<IJiraExportData>() { new JiraExportData() { value = businessFlow.Description + " Test Execution" } });
            jiraIssue.ExportFields.Add("issuetype", new List<IJiraExportData>() { new JiraExportData() { value = "Test Execution" } });
            jiraIssue.ExportFields.Add("reporter", new List<IJiraExportData>() { new JiraExportData() { value = ALMCore.AlmConfig.ALMUserName } });
            foreach (var item in testExecutionFields)
            {
                var issueTemplate = jiraRepObj.GetFieldFromTemplateByName(ALM_Common.DataContracts.ResourceType.TEST_CASE_EXECUTION_RECORDS, ALMCore.AlmConfig.ALMProjectKey, item.Name);
                if (issueTemplate == null || jiraIssue.ExportFields.ContainsKey(issueTemplate.key))
                    continue;
                if (issueTemplate != null)
                {
                    jiraIssue.ExportFields.Add(issueTemplate.key, new List<IJiraExportData>() { new JiraExportData() { value = item.SelectedValue } });
                }
            }
            var testCaseTemplate = jiraRepObj.GetFieldFromTemplateByName(ALM_Common.DataContracts.ResourceType.TEST_CASE_EXECUTION_RECORDS, ALMCore.AlmConfig.ALMProjectKey, "Test Cases");
            if (testCaseTemplate != null && tcArray.Count > 0)
                jiraIssue.ExportFields.Add(testCaseTemplate.key, tcArray);
            jiraIssue.key = businessFlow.AlmData;
            exportData.Add(jiraIssue);
            var exportExecutionResponse = jiraRepObj.ExportJiraIssues(ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword, ALMCore.AlmConfig.ALMServerURL, exportData);
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
            List<TestRunData> response = new List<TestRunData>();
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

            var thisTestExecution = jiraRepObj.GetJiraIssueById(ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword, ALMCore.AlmConfig.ALMServerURL, ALMCore.AlmConfig.ALMProjectKey, testExecutionKey, ALM_Common.DataContracts.ResourceType.TEST_CASE_EXECUTION_RECORDS);
            if (thisTestExecution != null && thisTestExecution.fields != null)
            {
                dynamic dc = null;
                thisTestExecution.fields.TryGetValue(testCaseTemplate.key, out dc);
                if (dc != null)
                {
                    foreach (var tc in dc)
                    {
                        businessFlow.ActivitiesGroups.Where(a => a.ExternalID == tc.b.Value).ToList().ForEach(b =>
                        {
                            string patern = testExecutionKey + "***" + tc.c.Value;
                            List<string> tcRuns = new List<string>();
                            if (!string.IsNullOrEmpty(b.ExternalID2))
                                tcRuns = b.ExternalID2.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            if (!tcRuns.Contains(patern))
                                tcRuns.Add(patern);
                            b.ExternalID2 = string.Join("||", tcRuns);
                        });
                    }
                }
            }

        }

        private List<IJiraExportData> CreateExportArrayFromActivites(List<ActivitiesGroup> bftestCases)
        {
            List<IJiraExportData> response = new List<IJiraExportData>();
            for (var a = 0; a < bftestCases.Count; a++)
            {
                response.Add(new JiraExportData() { id = a.ToString(), value = bftestCases[a].ExternalID });
            }
            return response;
        }

        private JiraIssueExport CreateJiraTestSet(BusinessFlow businessFlow, IEnumerable<ExternalItemFieldBase> testSetFields, IEnumerable<ExternalItemFieldBase> testCaseFields, List<IJiraExportData> tcIds)
        {
            JiraIssueExport jiraIssue = new JiraIssueExport();
            jiraIssue.name = businessFlow.Name;
            jiraIssue.key = businessFlow.ExternalID;
            CreateTestSetFields(businessFlow, testSetFields, jiraIssue, tcIds);
            return jiraIssue;
        }

        private void CreateTestSetFields(BusinessFlow businessFlow, IEnumerable<ExternalItemFieldBase> testSetFields, JiraIssueExport jiraIssue, List<IJiraExportData> tcIds)
        {
            jiraIssue.resourceType = ALM_Common.DataContracts.ResourceType.TEST_SET;
            jiraIssue.ExportFields.Add("project", new List<IJiraExportData>() { new JiraExportData() { value = ALMCore.AlmConfig.ALMProjectKey } });
            jiraIssue.ExportFields.Add("summary", new List<IJiraExportData>() { new JiraExportData() { value = businessFlow.Name } });
            jiraIssue.ExportFields.Add("description", new List<IJiraExportData>() { new JiraExportData() { value = businessFlow.Description } });
            jiraIssue.ExportFields.Add("issuetype", new List<IJiraExportData>() { new JiraExportData() { value = "Test Set" } });
            jiraIssue.ExportFields.Add("reporter", new List<IJiraExportData>() { new JiraExportData() { value = ALMCore.AlmConfig.ALMUserName } });

            foreach (var item in testSetFields)
            {
                var issueTemplate = jiraRepObj.GetFieldFromTemplateByName(ALM_Common.DataContracts.ResourceType.TEST_SET, ALMCore.AlmConfig.ALMProjectKey, item.Name);
                if (issueTemplate == null || jiraIssue.ExportFields.ContainsKey(issueTemplate.key))
                    continue;
                if (issueTemplate != null)
                {
                    jiraIssue.ExportFields.Add(issueTemplate.key, new List<IJiraExportData>() { new JiraExportData() { value = item.SelectedValue } });
                }
            }
            var testCaseTemplate = jiraRepObj.GetFieldFromTemplateByName(ALM_Common.DataContracts.ResourceType.TEST_SET, ALMCore.AlmConfig.ALMProjectKey, "Test Cases");
            if (testCaseTemplate != null && tcIds.Count > 0)
                jiraIssue.ExportFields.Add(testCaseTemplate.key, tcIds);
        }

        public bool ExportActivitesGrToJira(ActivitiesGroup activtiesGroup, IEnumerable<ExternalItemFieldBase> testCaseFields, ref string errorResult)
        {
            List<JiraIssueExport> exportData = new List<JiraIssueExport>();
            bool result = false;
            var testCaseFilterdFields = testCaseFields.Where(tc => tc.ToUpdate || tc.Mandatory);
            JiraIssueExport jiraIssue = CreateJiraTestCase(activtiesGroup, testCaseFilterdFields);
            exportData.Add(jiraIssue);
            var exportResponse = jiraRepObj.ExportJiraIssues(ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword, ALMCore.AlmConfig.ALMServerURL, exportData);
            if (exportResponse.Count > 0 && exportResponse.First().AuthenticationResponseObj.ErrorCode == 0)
            {
                if (string.IsNullOrEmpty(activtiesGroup.ExternalID))
                {
                    activtiesGroup.ExternalID = exportResponse.First().DataResult.key;
                }
                result = true;
            }
            else
                errorResult = exportResponse.FirstOrDefault().AuthenticationResponseObj.ErrorDesc;
            return result;
        }

        private JiraIssueExport UpdateTestCaseWithSpecificFields(ActivitiesGroup activtiesGroup, Dictionary<string, List<IJiraExportData>> fields)
        {
            JiraIssueExport jiraIssue = new JiraIssueExport();
            jiraIssue.ProjectKey = ALMCore.AlmConfig.ALMProjectKey;
            jiraIssue.key = activtiesGroup.ExternalID;
            fields.ToList().ForEach(x => jiraIssue.ExportFields.Add(x.Key, x.Value));
            return jiraIssue;
        }

        private JiraIssueExport CreateJiraTestCase(ActivitiesGroup activtiesGroup, IEnumerable<ExternalItemFieldBase> issueFields)
        {
            JiraIssueExport jiraIssue = new JiraIssueExport();
            jiraIssue.ProjectKey = ALMCore.AlmConfig.ALMProjectKey;
            jiraIssue.key = activtiesGroup.ExternalID;
            CreateTestCaseFields(activtiesGroup, issueFields, jiraIssue);
            CreateTestCaseSteps(activtiesGroup.ActivitiesIdentifiers, jiraIssue);
            return jiraIssue;
        }

        private void CreateTestCaseFields(ActivitiesGroup activtiesGroup, IEnumerable<ExternalItemFieldBase> issueFields, JiraIssueExport jiraIssue)
        {
            jiraIssue.name = activtiesGroup.Name;
            jiraIssue.resourceType = ALM_Common.DataContracts.ResourceType.TEST_CASE;
            jiraIssue.ExportFields.Add("project", new List<IJiraExportData>() { new JiraExportData() { value = ALMCore.AlmConfig.ALMProjectKey } });
            jiraIssue.ExportFields.Add("summary", new List<IJiraExportData>() { new JiraExportData() { value = activtiesGroup.Name } });
            jiraIssue.ExportFields.Add("description", new List<IJiraExportData>() { new JiraExportData() { value = activtiesGroup.Description } });
            jiraIssue.ExportFields.Add("issuetype", new List<IJiraExportData>() { new JiraExportData() { value = "Test" } });
            jiraIssue.ExportFields.Add("reporter", new List<IJiraExportData>() { new JiraExportData() { value = ALMCore.AlmConfig.ALMUserName } });
            if (!string.IsNullOrEmpty(activtiesGroup.ExternalID2))
                jiraIssue.ExportFields.Add("labels", new List<IJiraExportData>() { new JiraExportData() { value = activtiesGroup.ExternalID2 } });

            foreach (var item in issueFields)
            {
                var issueTemplate = jiraRepObj.GetFieldFromTemplateByName(ALM_Common.DataContracts.ResourceType.TEST_CASE, ALMCore.AlmConfig.ALMProjectKey, item.Name);
                if (issueTemplate == null || jiraIssue.ExportFields.ContainsKey(issueTemplate.key))
                    continue;
                if (issueTemplate != null)
                {
                    jiraIssue.ExportFields.Add(issueTemplate.key, new List<IJiraExportData>() { new JiraExportData() { value = item.SelectedValue } });
                }
            }
        }

        private void CreateTestCaseSteps(ObservableList<ActivityIdentifiers> activitiesIdentifiers, JiraIssueExport jiraIssue)
        {
            var steps = new List<IJiraExportData>();
            var issueTemplate = jiraRepObj.GetFieldFromTemplateByName(ALM_Common.DataContracts.ResourceType.TEST_CASE, ALMCore.AlmConfig.ALMProjectKey, "Test Steps");
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
                        step_index = a.ToString(),
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
                    variablesSb.Append("\\n=>Actions: ");
                variablesSb.Append("\\n");
                variablesSb.Append((a + 1).ToString());
                variablesSb.Append(")");
                variablesSb.Append(acts[a].Description);
                if (a < acts.Count - 1)
                    variablesSb.Append(";");
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
                variablesSb.Remove(variablesSb.Length - 1, 1);
            return variablesSb.ToString();
        }
    }
}
