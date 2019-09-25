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

using ALM_Common.Abstractions;
using ALM_Common.DataContracts;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Activities;
using GingerCore.External;
using GingerCore.Variables;
using Newtonsoft.Json;
using ALM_Common.Data_Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using JiraRepository;
using JiraRepository.Data_Contracts;

namespace GingerCore.ALM.JIRA
{
    public class JiraImportManager
    {
        private JiraRepository.JiraRepository jiraRepObj;

        public JiraImportManager(JiraRepository.JiraRepository jiraRepObj)
        {
            this.jiraRepObj = jiraRepObj;
        }

        public ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo { get; set; }
        public ObservableList<Activity> GingerActivitiesRepo { get; set; }
        internal ObservableList<ExternalItemFieldBase> GetALMItemFields(ResourceType resourceType, BackgroundWorker bw, bool online)
        {
            ObservableList<ExternalItemFieldBase> fields = new ObservableList<ExternalItemFieldBase>();
            try
            {
                JiraRepository.JiraRepository jiraRep = new JiraRepository.JiraRepository();
                LoginDTO loginData = new LoginDTO() { User = ALMCore.AlmConfig.ALMUserName, Password = ALMCore.AlmConfig.ALMPassword, Server = ALMCore.AlmConfig.ALMServerURL };
                if (resourceType == ResourceType.DEFECT)
                {
                    AlmResponseWithData<JiraRepository.Data_Contracts.JiraFieldColl> testDefectFieldsList;
                    testDefectFieldsList = jiraRep.GetIssueFields(loginData.User, loginData.Password, loginData.Server, ALMCore.AlmConfig.ALMProjectKey, ALM_Common.DataContracts.ResourceType.DEFECT);
                    fields.Append(SetALMItemsFields(testDefectFieldsList, ResourceType.DEFECT));
                }
                else
                {
                    AlmResponseWithData<JiraRepository.Data_Contracts.JiraFieldColl> testCaseFieldsList;
                    AlmResponseWithData<JiraRepository.Data_Contracts.JiraFieldColl> testSetFieldsList;
                    AlmResponseWithData<JiraRepository.Data_Contracts.JiraFieldColl> testExecutionFieldsList;

                    testSetFieldsList = jiraRep.GetIssueFields(loginData.User, loginData.Password, loginData.Server, ALMCore.AlmConfig.ALMProjectKey, ALM_Common.DataContracts.ResourceType.TEST_SET);
                    testCaseFieldsList = jiraRep.GetIssueFields(loginData.User, loginData.Password, loginData.Server, ALMCore.AlmConfig.ALMProjectKey, ALM_Common.DataContracts.ResourceType.TEST_CASE);
                    testExecutionFieldsList = jiraRep.GetIssueFields(loginData.User, loginData.Password, loginData.Server, ALMCore.AlmConfig.ALMProjectKey, ALM_Common.DataContracts.ResourceType.TEST_CASE_EXECUTION_RECORDS);

                    fields.Append(SetALMItemsFields(testSetFieldsList, ResourceType.TEST_SET));
                    fields.Append(SetALMItemsFields(testCaseFieldsList, ResourceType.TEST_CASE));
                    fields.Append(SetALMItemsFields(testExecutionFieldsList, ResourceType.TEST_CASE_EXECUTION_RECORDS));
                }
            }
            catch (Exception e) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e); }

            return fields;
        }

        private ObservableList<ExternalItemFieldBase> SetALMItemsFields(AlmResponseWithData<JiraRepository.Data_Contracts.JiraFieldColl> testCaseFieldsList, ResourceType fieldResourceType)
        {
            ObservableList<ExternalItemFieldBase> resourceFields = new ObservableList<ExternalItemFieldBase>();
            string fieldResourceTypeToString = fieldResourceType.ToString();
            if (fieldResourceType == ResourceType.TEST_CASE_EXECUTION_RECORDS)
            {
                fieldResourceTypeToString = "TEST_EXECUTION";
            }
            foreach (var field in testCaseFieldsList.DataResult)
            {
                if (string.IsNullOrEmpty(field.name)) continue;

                ExternalItemFieldBase itemfield = new ExternalItemFieldBase();
                itemfield.ID = field.name;
                itemfield.Name = field.name;
                itemfield.Mandatory = field.required;
                itemfield.ItemType = fieldResourceTypeToString;

                if (field.allowedValues.Count > 0)
                {
                    itemfield.SelectedValue = (field.allowedValues[0].name != null) ? field.allowedValues[0].name : field.allowedValues[0].value;
                    foreach (var item in field.allowedValues)
                    {
                        itemfield.PossibleValues.Add((item.name != null) ? item.name : item.value);
                    }
                }
                else
                {
                    itemfield.SelectedValue = "Unassigned";
                }
                resourceFields.Add(itemfield);
            }
            return resourceFields;
        }

        internal BusinessFlow ConvertJiraTestSetToBF(JiraTestSet testSet)
        {
            try
            {
                if (testSet == null) return null;

                //Create Business Flow
                BusinessFlow busFlow = new BusinessFlow();
                busFlow.Name = testSet.Name;
                busFlow.ExternalID = testSet.Key;
                busFlow.Status = BusinessFlow.eBusinessFlowStatus.Development;
                busFlow.Activities = new ObservableList<Activity>();
                busFlow.Variables = new ObservableList<VariableBase>();
                //Create Activities Group + Activities for each TC
                foreach (JiraTest tc in testSet.Tests)
                {
                    ActivitiesGroup tcActivsGroup = ConvertJiraTestToAG(busFlow, tc);

                    //Add the TC steps as Activities if not already on the Activities group
                    foreach (JiraTestStep step in tc.Steps)
                    {
                        Activity stepActivity;
                        bool toAddStepActivity;
                        ConvertJiraStepToActivity(busFlow, tc, tcActivsGroup, step, out stepActivity, out toAddStepActivity);

                        if (toAddStepActivity)
                        {
                            //not in group- need to add it
                            busFlow.AddActivity(stepActivity, tcActivsGroup);                            
                        }

                        //pull TC-Step parameters and add them to the Activity level
                        List<string> stepParamsList = new List<string>();
                        GetStepParameters(StripHTML(step.Variables), ref stepParamsList);
                        //GetStepParameters(StripHTML(step.Expected), ref stepParamsList);
                        foreach (string param in stepParamsList)
                        {
                            ConvertJiraParameters(tc, stepActivity, param);
                        }
                    }

                    //order the Activities Group activities according to the order of the matching steps in the TC
                    try
                    {
                        int startGroupActsIndxInBf = 0;// busFlow.Activities.IndexOf(tcActivsGroup.ActivitiesIdentifiers[0].IdentifiedActivity);
                        if (tcActivsGroup.ActivitiesIdentifiers.Count > 0)
                        {
                            startGroupActsIndxInBf = busFlow.Activities.IndexOf(tcActivsGroup.ActivitiesIdentifiers[0].IdentifiedActivity);
                        }
                        foreach (JiraTestStep step in tc.Steps)
                        {
                            int stepIndx = tc.Steps.IndexOf(step) + 1;
                            ActivityIdentifiers actIdent = tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.StepID).FirstOrDefault();
                            if (actIdent == null || actIdent.IdentifiedActivity == null) break;//something wrong- shouldnt be null
                            Activity act = actIdent.IdentifiedActivity;
                            int groupActIndx = tcActivsGroup.ActivitiesIdentifiers.IndexOf(actIdent);
                            int bfActIndx = busFlow.Activities.IndexOf(act);

                            //set it in the correct place in the group
                            int numOfSeenSteps = 0;
                            int groupIndx = -1;
                            foreach (ActivityIdentifiers ident in tcActivsGroup.ActivitiesIdentifiers)
                            {
                                groupIndx++;
                                if (string.IsNullOrEmpty(ident.ActivityExternalID) ||
                                        tc.Steps.Where(x => x.StepID == ident.ActivityExternalID).FirstOrDefault() == null)
                                    continue;//activity which not originaly came from the TC
                                numOfSeenSteps++;

                                if (numOfSeenSteps >= stepIndx) break;
                            }
                            ActivityIdentifiers identOnPlace = tcActivsGroup.ActivitiesIdentifiers[groupIndx];
                            if (identOnPlace.ActivityGuid != act.Guid)
                            {
                                //replace places in group
                                tcActivsGroup.ActivitiesIdentifiers.Move(groupActIndx, groupIndx);
                                //replace places in business flow
                                busFlow.Activities.Move(bfActIndx, startGroupActsIndxInBf + groupIndx);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                        //failed to re order the activities to match the tc steps order, not worth breaking the import because of this
                    }
                }
                return busFlow;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to import QC test set and convert it into " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), ex);
                return null;
            }
        }

        private static void ConvertJiraParameters(JiraTest tc, Activity stepActivity, string param)
        {
            //get the param value
            string paramSelectedValue = string.Empty;
            bool? isflowControlParam = null;
            JiraTestParameter tcParameter = tc.Parameters.Where(x => x.Name.ToUpper() == param.ToUpper()).FirstOrDefault();

            //get the param value
            if (tcParameter != null && tcParameter.Value != null && tcParameter.Value != string.Empty)
                paramSelectedValue = tcParameter.Value;
            else
            {
                isflowControlParam = null;//empty value
                paramSelectedValue = "<Empty>";
            }

            //check if parameter is part of a link
            string linkedVariable = null;
            if (paramSelectedValue.StartsWith("#$#"))
            {
                string[] valueParts = paramSelectedValue.Split(new string[] { "#$#" }, StringSplitOptions.None);
                if (valueParts.Count() == 3)
                {
                    linkedVariable = valueParts[1];
                    paramSelectedValue = "$$_" + valueParts[2];//so it still will be considered as non-flow control
                }
            }

            //detrmine if the param is Flow Control Param or not based on it value and agreed sign "$$_"
            if (paramSelectedValue.StartsWith("$$_"))
            {
                isflowControlParam = false;
                if (paramSelectedValue.StartsWith("$$_"))
                    paramSelectedValue = paramSelectedValue.Substring(3);//get value without "$$_"
            }
            else if (paramSelectedValue != "<Empty>")
                isflowControlParam = true;

            //check if already exist param with that name
            VariableBase stepActivityVar = stepActivity.Variables.Where(x => x.Name.ToUpper() == param.ToUpper()).FirstOrDefault();
            if (stepActivityVar == null)
            {
                //#Param not exist so add it
                if (isflowControlParam == true)
                {
                    //add it as selection list param                               
                    stepActivityVar = new VariableSelectionList();
                    stepActivityVar.Name = param;
                    stepActivity.AddVariable(stepActivityVar);
                    stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because new flow control param was added
                }
                else
                {
                    //add as String param
                    stepActivityVar = new VariableString();
                    stepActivityVar.Name = param;
                    ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                    stepActivity.AddVariable(stepActivityVar);
                }
            }
            else
            {
                //#param exist
                if (isflowControlParam == true)
                {
                    if (!(stepActivityVar is VariableSelectionList))
                    {
                        //flow control param must be Selection List so transform it
                        stepActivity.Variables.Remove(stepActivityVar);
                        stepActivityVar = new VariableSelectionList();
                        stepActivityVar.Name = param;
                        stepActivity.AddVariable(stepActivityVar);
                        stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because flow control param was added
                    }
                }
                else if (isflowControlParam == false)
                {
                    if (stepActivityVar is VariableSelectionList)
                    {
                        //change it to be string variable
                        stepActivity.Variables.Remove(stepActivityVar);
                        stepActivityVar = new VariableString();
                        stepActivityVar.Name = param;
                        ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                        stepActivity.AddVariable(stepActivityVar);
                        stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because flow control param was removed
                    }
                }
            }

            //add the variable selected value                          
            if (stepActivityVar is VariableSelectionList)
            {
                OptionalValue stepActivityVarOptionalVar = ((VariableSelectionList)stepActivityVar).OptionalValuesList.Where(x => x.Value == paramSelectedValue).FirstOrDefault();
                if (stepActivityVarOptionalVar == null)
                {
                    //no such variable value option so add it
                    stepActivityVarOptionalVar = new OptionalValue(paramSelectedValue);
                    ((VariableSelectionList)stepActivityVar).OptionalValuesList.Add(stepActivityVarOptionalVar);
                    if (isflowControlParam == true)
                        stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because new param value was added
                }
                //set the selected value
                ((VariableSelectionList)stepActivityVar).SelectedValue = stepActivityVarOptionalVar.Value;
            }
            else
            {
                //try just to set the value
                try
                {
                    stepActivityVar.Value = paramSelectedValue;
                    if (stepActivityVar is VariableString)
                        ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                }
                catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
            }

            //add linked variable if needed
            if (string.IsNullOrEmpty(linkedVariable) == false)
            {
                stepActivityVar.LinkedVariableName = linkedVariable;
            }
            else
                stepActivityVar.LinkedVariableName = string.Empty;//clear old links
        }

        private void ConvertJiraStepToActivity(BusinessFlow busFlow, JiraTest tc, ActivitiesGroup tcActivsGroup, JiraTestStep step, out Activity stepActivity, out bool toAddStepActivity)
        {
            toAddStepActivity = false;

            //check if mapped activity exist in repository
            Activity repoStepActivity = GingerActivitiesRepo.Where(x => x.ExternalID == step.StepID).FirstOrDefault();
            if (repoStepActivity != null)
            {
                //check if it is part of the Activities Group
                ActivityIdentifiers groupStepActivityIdent = tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.StepID).FirstOrDefault();
                if (groupStepActivityIdent != null)
                {
                    //already in Activities Group so get link to it
                    stepActivity = busFlow.Activities.Where(x => x.Guid == groupStepActivityIdent.ActivityGuid).FirstOrDefault();
                    // in any case update description/expected/name - even if "step" was taken from repository
                    stepActivity.Description = StripHTML(step.Description);
                    //stepActivity.Expected = StripHTML(step.Expected);
                    stepActivity.ActivityName = tc.TestName + ">" + step.StepName;
                }
                else//not in ActivitiesGroup so get instance from repo
                {
                    stepActivity = (Activity)repoStepActivity.CreateInstance();
                    toAddStepActivity = true;
                }
            }
            else//Step not exist in Ginger repository so create new one
            {
                stepActivity = new Activity();
                stepActivity.ActivityName = tc.TestName + ">" + step.StepName;
                stepActivity.ExternalID = step.StepID;
                stepActivity.Description = StripHTML(step.Description);
                //stepActivity.Expected = StripHTML(step.Expected);

                toAddStepActivity = true;
            }
        }

        private ActivitiesGroup ConvertJiraTestToAG(BusinessFlow busFlow, JiraTest tc)
        {
            //check if the TC is already exist in repository
            ActivitiesGroup tcActivsGroup;
            ActivitiesGroup repoActivsGroup = null;
            if (repoActivsGroup == null)
                repoActivsGroup = GingerActivitiesGroupsRepo.Where(x => x.ExternalID == tc.TestKey).FirstOrDefault();
            if (repoActivsGroup != null)
            {
                List<Activity> repoNotExistsStepActivity = GingerActivitiesRepo.Where(z => repoActivsGroup.ActivitiesIdentifiers.Select(y => y.ActivityExternalID).ToList().Contains(z.ExternalID))
                                                                               .Where(x => !tc.Steps.Select(y => y.StepID).ToList().Contains(x.ExternalID)).ToList();

                tcActivsGroup = (ActivitiesGroup)repoActivsGroup.CreateInstance(true);

                var ActivitySIdentifiersToRemove = tcActivsGroup.ActivitiesIdentifiers.Where(x => repoNotExistsStepActivity.Select(z => z.ExternalID).ToList().Contains(x.ActivityExternalID));
                for (int indx = tcActivsGroup.ActivitiesIdentifiers.Count - 1; indx >= 0; indx--)
                {
                    if ((indx < tcActivsGroup.ActivitiesIdentifiers.Count) && (ActivitySIdentifiersToRemove.Contains(tcActivsGroup.ActivitiesIdentifiers[indx])))
                    {
                        tcActivsGroup.ActivitiesIdentifiers.Remove(tcActivsGroup.ActivitiesIdentifiers[indx]);
                    }
                }

                tcActivsGroup.ExternalID2 = tc.Labels;
                busFlow.AddActivitiesGroup(tcActivsGroup);
                busFlow.ImportActivitiesGroupActivitiesFromRepository(tcActivsGroup, GingerActivitiesRepo, true, true);
                busFlow.AttachActivitiesGroupsAndActivities();
            }
            else //TC not exist in Ginger repository so create new one
            {
                tcActivsGroup = new ActivitiesGroup();
                tcActivsGroup.Name = tc.TestName;
                if (tc.LinkedTestID == null || tc.LinkedTestID == string.Empty)
                {
                    tcActivsGroup.ExternalID = tc.TestKey;
                    tcActivsGroup.ExternalID2 = tc.Labels;
                    tcActivsGroup.Description = tc.Description;
                }
                else
                {
                    tcActivsGroup.ExternalID = tc.LinkedTestID;
                    tcActivsGroup.ExternalID2 = tc.TestID; //original TC ID will be used for uploading the execution details back to QC
                    tcActivsGroup.Description = tc.Description;
                }
                busFlow.AddActivitiesGroup(tcActivsGroup);
            }

            return tcActivsGroup;
        }

        private string StripHTML(string HTMLText, bool toDecodeHTML = true)
        {
            try
            {
                HTMLText = HTMLText.Replace("<br />", Environment.NewLine);
                Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
                var stripped = reg.Replace(HTMLText, "");
                if (toDecodeHTML)
                    stripped = HttpUtility.HtmlDecode(stripped);

                stripped = stripped.TrimStart(new char[] { '\r', '\n' });
                stripped = stripped.TrimEnd(new char[] { '\r', '\n' });

                return stripped;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Error occurred while stripping the HTML from Jira TC Step Description/Expected", ex);
                return HTMLText;
            }
        }
        private static void GetStepParameters(string stepText, ref List<string> stepParamsList)
        {
            try
            {
                string[] stepParams = stepText.Split(new[] { ';' });

                foreach (string param in stepParams)
                {
                    string[] strParam = param.Split(new[] { '=' });
                    stepParamsList.Add(strParam[0].Trim());
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while pulling the parameters names from Jira TC Step Description/Expected", ex);
            }
        }
        public JiraTestSet GetTestSetData(JiraTestSet currentTS)
        {
            WhereDataList filterData = new WhereDataList();
            JiraTestSet issue = new JiraTestSet();
            filterData.Add(new WhereData() { Name = "id", Values = new List<string>() { currentTS.Key }, Operator = WhereOperator.And });
            AlmResponseWithData<List<JiraIssue>> getTestsSet = jiraRepObj.GetJiraIssues(ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword, ALMCore.AlmConfig.ALMServerURL, ALMCore.AlmConfig.ALMProjectKey, ResourceType.TEST_SET, filterData);
            List<FieldSchema> templates = JiraRepository.Settings.ExportSettings.Instance.GetSchemaByProject(ALMCore.AlmConfig.ALMProjectKey, ResourceType.TEST_SET);
            foreach (var item in getTestsSet.DataResult)
            {
                issue.ID = item.id.ToString();
                issue.URLPath = item.self;
                issue.Key = item.key;

                foreach (var tsKey in templates)
                {
                    if (item.fields.ContainsKey(tsKey.key))
                    {
                        List<string> fieldValue = getSelectedFieldValue(item.fields[tsKey.key], tsKey.name, ResourceType.TEST_SET);
                        if (fieldValue != null && fieldValue.Count > 0 && tsKey.name != null)
                        {
                            switch (tsKey.name)
                            {
                                case "created":
                                    issue.DateCreated = fieldValue.First();
                                    break;
                                case "Summary":
                                    issue.Name = fieldValue.First();
                                    break;
                                case "Reporter":
                                    issue.CreatedBy = fieldValue.First();
                                    break;
                                case "Project":
                                    issue.Project = fieldValue.First();
                                    break;
                                case "Description":
                                    issue.Description = fieldValue.First();
                                    break;
                                case "Test Cases":
                                    issue.Tests = new List<JiraTest>();
                                    foreach (var val in fieldValue)
                                    {
                                        issue.Tests.Add(new JiraTest() { TestID = val });
                                    }
                                    break;
                            }
                        }
                    }
                }
                if (issue.Tests != null && issue.Tests.Count > 0)
                {
                    GetTestData(issue.Tests);
                }
            }
            return issue;
        }
        private void GetTestData(List<JiraTest> tests)
        {
            WhereDataList filterData = new WhereDataList();
            foreach (JiraTest test in tests)
            {
                filterData.Clear();
                filterData.Add(new WhereData() { Name = "id", Values = new List<string>() { test.TestID }, Operator = WhereOperator.And });
                AlmResponseWithData<List<JiraIssue>> getTest = jiraRepObj.GetJiraIssues(ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword, ALMCore.AlmConfig.ALMServerURL, ALMCore.AlmConfig.ALMProjectKey, ResourceType.TEST_CASE, filterData);
                ObservableList<JiraTest> jiratests = new ObservableList<JiraTest>();
                List<FieldSchema> templates = JiraRepository.Settings.ExportSettings.Instance.GetSchemaByProject(ALMCore.AlmConfig.ALMProjectKey, ResourceType.TEST_CASE);
                foreach (var item in getTest.DataResult)
                {
                    test.TestID = item.id.ToString();
                    test.TestKey = item.key;
                    test.TestPath = item.self;

                    foreach (var tsKey in templates)
                    {
                        if (item.fields.ContainsKey(tsKey.key))
                        {
                            List<string> fieldValue = getSelectedFieldValue(item.fields[tsKey.key], tsKey.name, ResourceType.TEST_CASE);
                            if (fieldValue != null && fieldValue.Count > 0)
                            {
                                switch (tsKey.name)
                                {
                                    case "Summary":
                                        test.TestName = fieldValue.First();
                                        break;
                                    case "Reporter":
                                        test.CreatedBy = fieldValue.First();
                                        break;
                                    case "Project":
                                        test.Project = fieldValue.First();
                                        break;
                                    case "Description":
                                        test.Description = fieldValue.First();
                                        break;
                                    case "Labels":
                                        foreach (var val in fieldValue)
                                        {
                                            test.Labels = string.Join("||", fieldValue);
                                        }
                                        break;
                                    case "Test Steps":
                                        test.Steps = new List<JiraTestStep>();
                                        var stepAnonymousTypeDef = new { id = 0, index = 0, step = string.Empty, data = string.Empty };
                                        foreach (var val in fieldValue)
                                        {
                                            var stepAnonymous = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(val, stepAnonymousTypeDef);
                                            string[] stepDescription = new[] { "", "" };
                                            string[] stepVariables = new[] { "", "" };
                                            if (!string.IsNullOrEmpty(stepAnonymous.data) && stepAnonymous.data.Contains("=>"))
                                            {
                                                string[] getStepData = (stepAnonymous.data).Split(new[] { "=>" }, StringSplitOptions.None);
                                                if (getStepData.Count() > 1 && getStepData[1].Contains("Description:"))
                                                {
                                                    stepDescription = getStepData[1].Split(new[] { "Description:" }, StringSplitOptions.None);
                                                }
                                                if (getStepData.Count() > 2 && getStepData[2].Contains("Variables:"))
                                                {
                                                    stepVariables = getStepData[2].Split(new[] { "Variables:" }, StringSplitOptions.None);
                                                }
                                            }
                                            else
                                            {
                                                stepDescription[1] = stepAnonymous.data;
                                            }
                                            test.Steps.Add(new JiraTestStep() { StepID = stepAnonymous.id.ToString(), StepName = stepAnonymous.step, Description = StripHTML(stepDescription[1]) , Variables = stepVariables[1]});
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }
        }
        public ObservableList<JiraTestSet> GetJiraTestSets()
        {
            WhereDataList filterData = new WhereDataList();
            List<string> testSetKeys = new List<string> { "reporter", "created", "summary", "project" };
            filterData.Add(new WhereData() { Name = "fields", Values = testSetKeys, Operator = WhereOperator.Ampersand });
            AlmResponseWithData<List<JiraIssue>> getTestsSet = jiraRepObj.GetJiraIssues(ALMCore.AlmConfig.ALMUserName, ALMCore.AlmConfig.ALMPassword, ALMCore.AlmConfig.ALMServerURL, ALMCore.AlmConfig.ALMProjectKey, ResourceType.TEST_SET, filterData);

            ObservableList<JiraTestSet> jiratestset = new ObservableList<JiraTestSet>();
            List<FieldSchema> templates = JiraRepository.Settings.ExportSettings.Instance.GetSchemaByProject(ALMCore.AlmConfig.ALMProjectKey, ResourceType.TEST_SET);
            foreach (var item in getTestsSet.DataResult)
            {
                JiraTestSet issue = new JiraTestSet();
                issue.ID = item.id.ToString();
                issue.URLPath = item.self;
                issue.Key = item.key;

                foreach (string tsKey in testSetKeys)
                {
                    if (item.fields.ContainsKey(tsKey))
                    {
                        string templateFieldName = templates.Where(fld => fld.key.Equals(tsKey)).Select(n => n.name).FirstOrDefault();
                        List<string> fieldValue = getSelectedFieldValue(item.fields[tsKey], templateFieldName, ResourceType.TEST_SET);
                        if (fieldValue != null && fieldValue.Count > 0)
                        {
                            switch (tsKey)
                            {
                                case "created":
                                    issue.DateCreated = fieldValue.First();
                                    break;
                                case "summary":
                                    issue.Name = fieldValue.First();
                                    break;
                                case "reporter":
                                    issue.CreatedBy = fieldValue.First();
                                    break;
                                case "project":
                                    issue.Project = fieldValue.First();
                                    break;
                            }
                        }
                    }
                }
                jiratestset.Add(issue);
            }
            return jiratestset;
        }
        private List<string> getSelectedFieldValue(dynamic fields, string fieldName, ResourceType resourceType)
        {
            List<string> valuesList = new List<string>();
            try
            {
                FieldSchema temp = jiraRepObj.GetFieldFromTemplateByName(resourceType, "DE", fieldName);
                if (temp == null)
                {
                    return null;
                }
                switch (temp.type)
                {
                    case "string":
                        valuesList.Add(fields.Value.ToString());
                        break;
                    case "object":
                        var jsonTemplateObj = Newtonsoft.Json.Linq.JObject.Parse(temp.data);
                        valuesList.Add((fields[((Newtonsoft.Json.Linq.JProperty)jsonTemplateObj.First).Name]).ToString());
                        break;
                    case "strings_array":
                        foreach (var fieldIssue in fields)
                        {
                            valuesList.Add(fieldIssue.Value);
                        }
                        break;
                    case "array":
                        if (fields[0] != null)
                        {
                            valuesList.Add(fields[0].ToString());
                        }
                        break;
                    case "option":
                        break;
                    case "steps":
                        foreach (var step in fields["steps"])
                        {
                            valuesList.Add(step.ToString());
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("getSelectedFieldValue error: " + ex.Message);
            }
            return valuesList;
        }
        public void UpdateBFSelectedAG(ref BusinessFlow busFlow, Dictionary<string, JiraTest> activitiesGroupToUpdatedData)
        {
            if ((busFlow == null) || (activitiesGroupToUpdatedData.Count == 0)) return;
            Dictionary<string, string> busVariables = new Dictionary<string, string>();

            int startGroupActsIndxInBf = 0;
            Dictionary<string, int> activityGroupsToRemoveIndexes = new Dictionary<string, int>();
            foreach (JiraTest tc in activitiesGroupToUpdatedData.Values)
            {
                var activitiesToRemove = busFlow.Activities.Where(x => tc.Steps.Any(stepid => stepid.StepID.Equals(x.ExternalID))).ToList();
                foreach (Activity activityToRemove in activitiesToRemove)
                {
                    if (startGroupActsIndxInBf < busFlow.Activities.IndexOf(activityToRemove))
                        startGroupActsIndxInBf = busFlow.Activities.IndexOf(activityToRemove);
                    busFlow.Activities.Remove(activityToRemove);
                }
                var activityGroupsToRemove = busFlow.ActivitiesGroups.Where(x => x.ExternalID == tc.TestKey).ToList();
                foreach (ActivitiesGroup activityGroupToRemove in activityGroupsToRemove)
                {
                    activityGroupsToRemoveIndexes.Add(activityGroupToRemove.ExternalID, busFlow.ActivitiesGroups.IndexOf(activityGroupToRemove));
                }
                foreach (ActivitiesGroup activityGroupToRemove in activityGroupsToRemove)
                {
                    busFlow.ActivitiesGroups.Remove(activityGroupToRemove);
                }
            }

            int activityGroupToRemoveIndex;
            foreach (JiraTest tc in activitiesGroupToUpdatedData.Values)
            {
                activityGroupsToRemoveIndexes.TryGetValue(tc.TestKey, out activityGroupToRemoveIndex);

                //check if the TC is already exist in repository
                ActivitiesGroup tcActivsGroup = new ActivitiesGroup();

                tcActivsGroup = new ActivitiesGroup();
                tcActivsGroup.Name = tc.TestName;
                tcActivsGroup.ExternalID = tc.TestKey;
                tcActivsGroup.Description = tc.Description;
                busFlow.AddActivitiesGroup(tcActivsGroup, activityGroupToRemoveIndex);

                //Add the TC steps as Activities if not already on the Activities group
                foreach (JiraTestStep step in tc.Steps)
                {
                    Activity stepActivity;
                    bool toAddStepActivity = false;

                    //check if mapped activity exist in repository
                    Activity repoStepActivity = (Activity)GingerActivitiesRepo.Where(x => x.ExternalID == step.StepID).FirstOrDefault();
                    if (repoStepActivity != null)
                    {
                        //check if it is part of the Activities Group
                        ActivityIdentifiers groupStepActivityIdent = (ActivityIdentifiers)tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.StepID).FirstOrDefault();
                        if (groupStepActivityIdent != null)
                        {
                            //already in Activities Group so get link to it
                            stepActivity = (Activity)busFlow.Activities.Where(x => x.Guid == groupStepActivityIdent.ActivityGuid).FirstOrDefault();
                        }
                        else//not in ActivitiesGroup so get instance from repo
                        {
                            stepActivity = (Activity)repoStepActivity.CreateInstance();
                            toAddStepActivity = true;
                        }
                        stepActivity.IsSharedRepositoryInstance = true;
                    }
                    else//Step not exist in Ginger repository so create new one
                    {
                        stepActivity = new Activity();
                        stepActivity.ActivityName = tc.TestName + ">" + step.StepName;
                        stepActivity.ExternalID = step.StepID;
                        stepActivity.Description = StripHTML(step.Description);
                        //stepActivity.Expected = StripHTML(step.Expected);

                        toAddStepActivity = true;
                    }

                    if (toAddStepActivity)
                    {
                        //not in group- need to add it
                        busFlow.AddActivity(stepActivity, tcActivsGroup, startGroupActsIndxInBf++);                        
                    }

                    //pull TC-Step parameters and add them to the Activity level
                    List<string> stepParamsList = new List<string>();
                    GetStepParameters(StripHTML(step.Variables), ref stepParamsList);
                    //GetStepParameters(StripHTML(step.Expected), ref stepParamsList);
                    foreach (string param in stepParamsList)
                    {
                        //get the param value
                        string paramSelectedValue = string.Empty;
                        bool? isflowControlParam = null;
                        JiraTestParameter tcParameter = tc.Parameters.Where(x => x.Name.ToUpper() == param.ToUpper()).FirstOrDefault();

                        //get the param value
                        if (tcParameter != null && tcParameter.Value != null && tcParameter.Value != string.Empty)
                            paramSelectedValue = tcParameter.Value;
                        else
                        {
                            isflowControlParam = null;//empty value
                            paramSelectedValue = "<Empty>";
                        }

                        //check if parameter is part of a link
                        string linkedVariable = null;
                        if (paramSelectedValue.StartsWith("#$#"))
                        {
                            string[] valueParts = paramSelectedValue.Split(new string[] { "#$#" }, StringSplitOptions.None);
                            if (valueParts.Count() == 3)
                            {
                                linkedVariable = valueParts[1];
                                paramSelectedValue = "$$_" + valueParts[2];//so it still will be considered as non-flow control

                                if (busVariables.Keys.Contains(linkedVariable) == false)
                                {
                                    busVariables.Add(linkedVariable, valueParts[2]);
                                }
                            }
                        }

                        //detrmine if the param is Flow Control Param or not based on it value and agreed sign "$$_"
                        if (paramSelectedValue.StartsWith("$$_"))
                        {
                            isflowControlParam = false;
                            if (paramSelectedValue.StartsWith("$$_"))
                                paramSelectedValue = paramSelectedValue.Substring(3);//get value without "$$_"
                        }
                        else if (paramSelectedValue != "<Empty>")
                            isflowControlParam = true;

                        //check if already exist param with that name
                        VariableBase stepActivityVar = stepActivity.Variables.Where(x => x.Name.ToUpper() == param.ToUpper()).FirstOrDefault();
                        if (stepActivityVar == null)
                        {
                            //#Param not exist so add it
                            if (isflowControlParam == true)
                            {
                                //add it as selection list param                               
                                stepActivityVar = new VariableSelectionList();
                                stepActivityVar.Name = param;
                                stepActivity.AddVariable(stepActivityVar);
                                stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because new flow control param was added
                            }
                            else
                            {
                                //add as String param
                                stepActivityVar = new VariableString();
                                stepActivityVar.Name = param;
                                ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                                stepActivity.AddVariable(stepActivityVar);
                            }
                        }
                        else
                        {
                            //#param exist
                            if (isflowControlParam == true)
                            {
                                if (!(stepActivityVar is VariableSelectionList))
                                {
                                    //flow control param must be Selection List so transform it
                                    stepActivity.Variables.Remove(stepActivityVar);
                                    stepActivityVar = new VariableSelectionList();
                                    stepActivityVar.Name = param;
                                    stepActivity.AddVariable(stepActivityVar);
                                    stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because flow control param was added
                                }
                            }
                            else if (isflowControlParam == false)
                            {
                                if (stepActivityVar is VariableSelectionList)
                                {
                                    //change it to be string variable
                                    stepActivity.Variables.Remove(stepActivityVar);
                                    stepActivityVar = new VariableString();
                                    stepActivityVar.Name = param;
                                    ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                                    stepActivity.AddVariable(stepActivityVar);
                                    stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because flow control param was removed
                                }
                            }
                        }

                        //add the variable selected value                          
                        if (stepActivityVar is VariableSelectionList)
                        {
                            OptionalValue stepActivityVarOptionalVar = ((VariableSelectionList)stepActivityVar).OptionalValuesList.Where(x => x.Value == paramSelectedValue).FirstOrDefault();
                            if (stepActivityVarOptionalVar == null)
                            {
                                //no such variable value option so add it
                                stepActivityVarOptionalVar = new OptionalValue(paramSelectedValue);
                                ((VariableSelectionList)stepActivityVar).OptionalValuesList.Add(stepActivityVarOptionalVar);
                                if (isflowControlParam == true)
                                    stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because new param value was added
                            }
                            //set the selected value
                            ((VariableSelectionList)stepActivityVar).SelectedValue = stepActivityVarOptionalVar.Value;
                        }
                        else
                        {
                            //try just to set the value
                            try
                            {
                                stepActivityVar.Value = paramSelectedValue;
                                if (stepActivityVar is VariableString)
                                    ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                            }
                            catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
                        }

                        //add linked variable if needed
                        if (string.IsNullOrEmpty(linkedVariable) == false)
                        {
                            stepActivityVar.LinkedVariableName = linkedVariable;
                        }
                        else
                            stepActivityVar.LinkedVariableName = string.Empty;//clear old links
                    }
                }

                //order the Activities Group activities according to the order of the matching steps in the TC
                try
                {
                    foreach (JiraTestStep step in tc.Steps)
                    {
                        int stepIndx = tc.Steps.IndexOf(step) + 1;
                        ActivityIdentifiers actIdent = (ActivityIdentifiers)tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.StepID).FirstOrDefault();
                        if (actIdent == null || actIdent.IdentifiedActivity == null) break;//something wrong- shouldnt be null
                        Activity act = (Activity)actIdent.IdentifiedActivity;
                        int groupActIndx = tcActivsGroup.ActivitiesIdentifiers.IndexOf(actIdent);
                        int bfActIndx = busFlow.Activities.IndexOf(act);

                        //set it in the correct place in the group
                        int numOfSeenSteps = 0;
                        int groupIndx = -1;
                        foreach (ActivityIdentifiers ident in tcActivsGroup.ActivitiesIdentifiers)
                        {
                            groupIndx++;
                            if (string.IsNullOrEmpty(ident.ActivityExternalID) ||
                                    tc.Steps.Where(x => x.StepID == ident.ActivityExternalID).FirstOrDefault() == null)
                                continue;//activity which not originaly came from the TC
                            numOfSeenSteps++;

                            if (numOfSeenSteps >= stepIndx) break;
                        }
                        ActivityIdentifiers identOnPlace = (ActivityIdentifiers)tcActivsGroup.ActivitiesIdentifiers[groupIndx];
                        if (identOnPlace.ActivityGuid != act.Guid)
                        {
                            //replace places in group
                            tcActivsGroup.ActivitiesIdentifiers.Move(groupActIndx, groupIndx);
                            //replace places in business flow
                            busFlow.Activities.Move(bfActIndx, startGroupActsIndxInBf + groupIndx);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                    //failed to re order the activities to match the tc steps order, not worth breaking the import because of this
                }
            }
            return;
        }
        public void UpdateBussinessFlow(ref BusinessFlow busFlow)
        {
            JiraTestSet testSet = GetTestSetData(new JiraTestSet { Key = busFlow.ExternalID });
            Dictionary<string, JiraTest> activitiesGroupToUpdatedData = GetJiraSelectedTestsData(busFlow.ExternalID, busFlow.ActivitiesGroups.Select(actid => actid.ExternalID).ToList());
            if (busFlow == null) return;
            busFlow.Name = testSet.Name;
            busFlow.Description = testSet.Description;
            UpdateBFSelectedAG(ref busFlow, activitiesGroupToUpdatedData);
        }
        public Dictionary<string,JiraTest> GetJiraSelectedTestsData(string testSetID, List<string> TCsIds = null)
        {
            Dictionary<string,JiraTest> existsTestInJira = new Dictionary<string, JiraTest>();

            JiraTestSet testSet = GetTestSetData(new JiraTestSet { Key = testSetID });
            if (testSet != null && testSet.Tests.Count > 0)
            {
                foreach(string tc in TCsIds)
                {
                    existsTestInJira.Add(tc,testSet.Tests.Where(tst => tst.TestKey.Equals(tc)).FirstOrDefault());
                }
            }

            return existsTestInJira;
        }
    }
}
