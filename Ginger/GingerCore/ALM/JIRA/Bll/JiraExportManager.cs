using ALM_Common.DataContracts;
using Amdocs.Ginger.Common;
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

namespace GingerCore.ALM.JIRA.Bll
{
    public class JiraExportManager
    {
        private JiraRepository.JiraRepository jiraRepObj;
        public JiraExportManager(JiraRepository.JiraRepository jiraRep)
        {
            this.jiraRepObj = jiraRep;
        }

        public bool ExportBfToAlm(BusinessFlow businessFlow, IEnumerable<ExternalItemFieldBase> testCaseFields, IEnumerable<ExternalItemFieldBase> testSetFields, IEnumerable<ExternalItemFieldBase> testExecutionFields, ref string responseStr)
        {
            List<JiraIssueExport> exportData = new List<JiraIssueExport>();
            bool result = false;
            var bftestCases = businessFlow.ActivitiesGroups.ToList();
            foreach (var tc in bftestCases)
            {
                this.ExportActivitesToJira(tc, testCaseFields, ref responseStr);
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
                CreateTestExecution(businessFlow, tcArray, testExecutionFields);
                result = true;
            }
            else
                responseStr = exportResponse.FirstOrDefault().AuthenticationResponseObj.ErrorDesc;
            return result;
        }

        public bool ExecuteDataToJira(BusinessFlow bizFlow, PublishToALMConfig publishToALMConfig)
        {
            bool result = false;
            if (bizFlow.ExternalID != "0" && (!String.IsNullOrEmpty(bizFlow.ExternalID)))
            {
                foreach(var actGroup in bizFlow.ActivitiesGroups)
                {
                    RunStatus jiraStatus = ConvertGingerStatusToJira(actGroup.RunStatus);
                    var testExecutionData = CreateTestRunData(actGroup);
                    if (!string.IsNullOrEmpty(testExecutionData.TestCaseRunId))
                    {
                       var executionResponse= jiraRepObj.SetRunStatus(ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword, ALMCore.AlmConfig.ALMServerURL, testExecutionData.TestCaseRunId, jiraStatus);
                        if (executionResponse.AuthenticationResponseObj.ErrorCode == 0)
                            result = true;
                        else
                            result = false;
                    }
                }
            }
            return result;
        }

        private RunStatus ConvertGingerStatusToJira(ActivitiesGroup.eActivitiesGroupRunStatus runStatus)
        {
            RunStatus responseStatus = RunStatus.EXECUTING;
            switch(runStatus)
            {
                case ActivitiesGroup.eActivitiesGroupRunStatus.Blocked:
                    responseStatus = RunStatus.BLOCKED;
                    break;
                case ActivitiesGroup.eActivitiesGroupRunStatus.Failed:
                    responseStatus = RunStatus.FAIL;
                    break;
                case ActivitiesGroup.eActivitiesGroupRunStatus.Passed:
                    responseStatus = RunStatus.PASS;
                    break;
                case ActivitiesGroup.eActivitiesGroupRunStatus.Skipped:
                    responseStatus = RunStatus.ABORTED;
                    break;
                case ActivitiesGroup.eActivitiesGroupRunStatus.Pending:
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
            jiraIssue.ExportFields.Add("project", new List<IJiraExportData>() { new JiraExportData() { value = ALMCore.AlmConfig.ALMDomain } });
            jiraIssue.ExportFields.Add("summary", new List<IJiraExportData>() { new JiraExportData() { value = businessFlow.Name + "Test Execution" } });
            jiraIssue.ExportFields.Add("description", new List<IJiraExportData>() { new JiraExportData() { value = businessFlow.Description + " Test Execution" } });
            jiraIssue.ExportFields.Add("issuetype", new List<IJiraExportData>() { new JiraExportData() { value = "Test Execution" } });
            jiraIssue.ExportFields.Add("reporter", new List<IJiraExportData>() { new JiraExportData() { value = ALMCore.AlmConfig.ALMUserName } });
            foreach (var item in testExecutionFields)
            {
                var issueTemplate = jiraRepObj.GetFieldFromTemplateByName(ALM_Common.DataContracts.ResourceType.TEST_CASE_EXECUTION_RECORDS, ALMCore.AlmConfig.ALMDomain, item.Name);
                if (issueTemplate == null || jiraIssue.ExportFields.ContainsKey(issueTemplate.key))
                    continue;
                if (issueTemplate != null)
                {
                    jiraIssue.ExportFields.Add(issueTemplate.key, new List<IJiraExportData>() { new JiraExportData() { value = item.SelectedValue } });
                }
            }
            var testCaseTemplate = jiraRepObj.GetFieldFromTemplateByName(ALM_Common.DataContracts.ResourceType.TEST_CASE_EXECUTION_RECORDS, ALMCore.AlmConfig.ALMDomain, "Test Cases");
            if (testCaseTemplate != null && tcArray.Count > 0)
                jiraIssue.ExportFields.Add(testCaseTemplate.key, tcArray);
            var testExecutionData = CreateTestRunData(businessFlow.ActivitiesGroups.FirstOrDefault());
            jiraIssue.key = testExecutionData.TestExecutionId;
            exportData.Add(jiraIssue);
            var exportExecutionResponse = jiraRepObj.ExportJiraIssues(ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword, ALMCore.AlmConfig.ALMServerURL, exportData);
            SetRunIdFromTestExecution(exportExecutionResponse, testCaseTemplate, businessFlow, testExecutionData.TestExecutionId);
            return result;
        }

        private TestRunData CreateTestRunData(ActivitiesGroup activitiesGroup)
        {
            TestRunData response = new TestRunData();
            if (!string.IsNullOrEmpty(activitiesGroup.ExternalID2))
            {
                var resultArr = activitiesGroup.ExternalID2.Split(new string[] { "***" }, StringSplitOptions.RemoveEmptyEntries);
                if (resultArr.Length == 2)
                {
                    response.TestExecutionId = resultArr[0];
                    response.TestCaseRunId = resultArr[1];

                }
            }
            return response;
        }

        private void SetRunIdFromTestExecution(List<AlmResponseWithData<JiraIssue>> exportExecutionResponse, FieldSchema testCaseTemplate, BusinessFlow businessFlow, string executionExisitingKey)
        {
            if (exportExecutionResponse.Count > 0 && exportExecutionResponse.First().AuthenticationResponseObj.ErrorCode == 0)
            {
                var testExecution = exportExecutionResponse.First();
                string tExeckey = string.Empty;
                if (testExecution.DataResult != null)
                    tExeckey = testExecution.DataResult.key;
                else
                    tExeckey = executionExisitingKey;
                var thisTestExecution = jiraRepObj.GetJiraIssueById(ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword, ALMCore.AlmConfig.ALMServerURL, ALMCore.AlmConfig.ALMDomain, tExeckey, ALM_Common.DataContracts.ResourceType.TEST_CASE_EXECUTION_RECORDS);
                if (thisTestExecution != null && thisTestExecution.fields != null)
                {
                    dynamic dc = null;
                    thisTestExecution.fields.TryGetValue(testCaseTemplate.key, out dc);
                    if (dc != null)
                    {
                        foreach (var tc in dc)
                        {
                            businessFlow.ActivitiesGroups.Where(a => a.ExternalID == tc.b.Value).ToList().ForEach(b => { b.ExternalID2 = (tExeckey + "***" + tc.c.Value); });
                        }
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
            jiraIssue.ExportFields.Add("project", new List<IJiraExportData>() { new JiraExportData() { value = ALMCore.AlmConfig.ALMDomain } });
            jiraIssue.ExportFields.Add("summary", new List<IJiraExportData>() { new JiraExportData() { value = businessFlow.Name } });
            jiraIssue.ExportFields.Add("description", new List<IJiraExportData>() { new JiraExportData() { value = businessFlow.Description } });
            jiraIssue.ExportFields.Add("issuetype", new List<IJiraExportData>() { new JiraExportData() { value = "Test Set" } });
            jiraIssue.ExportFields.Add("reporter", new List<IJiraExportData>() { new JiraExportData() { value = ALMCore.AlmConfig.ALMUserName } });
            foreach (var item in testSetFields)
            {
                var issueTemplate = jiraRepObj.GetFieldFromTemplateByName(ALM_Common.DataContracts.ResourceType.TEST_SET, ALMCore.AlmConfig.ALMDomain, item.Name);
                if (issueTemplate == null || jiraIssue.ExportFields.ContainsKey(issueTemplate.key))
                    continue;
                if (issueTemplate != null)
                {
                    jiraIssue.ExportFields.Add(issueTemplate.key, new List<IJiraExportData>() { new JiraExportData() { value = item.SelectedValue } });
                }
            }
            var testCaseTemplate = jiraRepObj.GetFieldFromTemplateByName(ALM_Common.DataContracts.ResourceType.TEST_SET, ALMCore.AlmConfig.ALMDomain, "Test Cases");
            if (testCaseTemplate != null && tcIds.Count > 0)
                jiraIssue.ExportFields.Add(testCaseTemplate.key, tcIds);
        }

        public bool ExportActivitesToJira(ActivitiesGroup activtiesGroup, IEnumerable<ExternalItemFieldBase> testCaseFields, ref string errorResult)
        {
            List<JiraIssueExport> exportData = new List<JiraIssueExport>();
            bool result = false;
            var testCaseFilterdFields = testCaseFields.Where(tc => tc.ToUpdate || tc.Mandatory);
            JiraIssueExport jiraIssue = CreateJiraTestCsae(activtiesGroup, testCaseFilterdFields);
            exportData.Add(jiraIssue);
            var exportResponse = jiraRepObj.ExportJiraIssues(ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword, ALMCore.AlmConfig.ALMServerURL, exportData);
            if (exportResponse.Count > 0 && exportResponse.First().AuthenticationResponseObj.ErrorCode == 0)
            {
                //Insert
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

        private JiraIssueExport CreateJiraTestCsae(ActivitiesGroup activtiesGroup, IEnumerable<ExternalItemFieldBase> issueFields)
        {
            JiraIssueExport jiraIssue = new JiraIssueExport();
            jiraIssue.ProjectKey = ALMCore.AlmConfig.ALMDomain;
            jiraIssue.key = activtiesGroup.ExternalID;
            CreateTestCaseFields(activtiesGroup, issueFields, jiraIssue);
            CreateTestCaseSteps(activtiesGroup.ActivitiesIdentifiers, jiraIssue);
            return jiraIssue;
        }

        private void CreateTestCaseFields(ActivitiesGroup activtiesGroup, IEnumerable<ExternalItemFieldBase> issueFields, JiraIssueExport jiraIssue)
        {
            jiraIssue.name = activtiesGroup.Name;
            jiraIssue.resourceType = ALM_Common.DataContracts.ResourceType.TEST_CASE;
            jiraIssue.ExportFields.Add("project", new List<IJiraExportData>() { new JiraExportData() { value = ALMCore.AlmConfig.ALMDomain } });
            jiraIssue.ExportFields.Add("summary", new List<IJiraExportData>() { new JiraExportData() { value = activtiesGroup.Name } });
            jiraIssue.ExportFields.Add("description", new List<IJiraExportData>() { new JiraExportData() { value = activtiesGroup.Description } });
            jiraIssue.ExportFields.Add("issuetype", new List<IJiraExportData>() { new JiraExportData() { value = "Test" } });
            jiraIssue.ExportFields.Add("reporter", new List<IJiraExportData>() { new JiraExportData() { value = ALMCore.AlmConfig.ALMUserName } });
            foreach (var item in issueFields)
            {
                var issueTemplate = jiraRepObj.GetFieldFromTemplateByName(ALM_Common.DataContracts.ResourceType.TEST_CASE, ALMCore.AlmConfig.ALMDomain, item.Name);
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
            var issueTemplate = jiraRepObj.GetFieldFromTemplateByName(ALM_Common.DataContracts.ResourceType.TEST_CASE, ALMCore.AlmConfig.ALMDomain, "Test Steps");
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

        private string CreateStringFromActions(ObservableList<Act> acts)
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
