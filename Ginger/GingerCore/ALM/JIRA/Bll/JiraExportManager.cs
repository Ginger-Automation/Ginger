using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.Activities;
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

        public bool ExportActivitesToJira(ActivitiesGroup activtiesGroup, string uploadPath, IEnumerable<ExternalItemFieldBase> testCaseFields)
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
                    stepDataSb.Append("=>");
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
