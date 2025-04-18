#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Activities;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using JiraRepositoryStd.Data_Contracts;
using JiraRepositoryStd.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace GingerCore.ALM.JIRA
{
    public class JiraImportManager
    {
        private JiraRepositoryStd.JiraRepositoryStd jiraRepObj;

        public JiraImportManager(JiraRepositoryStd.JiraRepositoryStd jiraRepObj)
        {
            this.jiraRepObj = jiraRepObj;
        }

        public JiraRepositoryStd.JiraRepositoryStd JiraRepObj()
        {
            return this.jiraRepObj;
        }

        public ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo { get; set; }
        public ObservableList<Activity> GingerActivitiesRepo { get; set; }
        public ObservableList<ApplicationPlatform> ApplicationPlatforms { get; set; }

        internal ObservableList<ExternalItemFieldBase> GetALMItemFields(ResourceType resourceType, BackgroundWorker bw, bool online)
        {
            ObservableList<ExternalItemFieldBase> fields = [];
            try
            {
                JiraRepositoryStd.JiraRepositoryStd jiraRep = new JiraRepositoryStd.JiraRepositoryStd();
                LoginDTO loginData = new LoginDTO() { User = ALMCore.DefaultAlmConfig.ALMUserName, Password = ALMCore.DefaultAlmConfig.ALMPassword, Server = ALMCore.DefaultAlmConfig.ALMServerURL };
                if (resourceType == ResourceType.DEFECT)
                {
                    AlmResponseWithData<JiraFieldColl> testDefectFieldsList;
                    testDefectFieldsList = jiraRep.GetIssueFields(loginData.User, loginData.Password, loginData.Server, ALMCore.DefaultAlmConfig.ALMProjectKey, ResourceType.DEFECT, ALMCore.DefaultAlmConfig.UseToken);
                    fields.Append(SetALMItemsFields(testDefectFieldsList, ResourceType.DEFECT));
                }
                else
                {
                    AlmResponseWithData<JiraFieldColl> testCaseFieldsList;
                    AlmResponseWithData<JiraFieldColl> testSetFieldsList;
                    AlmResponseWithData<JiraFieldColl> testExecutionFieldsList;

                    testSetFieldsList = jiraRep.GetIssueFields(loginData.User, loginData.Password, loginData.Server, ALMCore.DefaultAlmConfig.ALMProjectKey, ResourceType.TEST_SET, ALMCore.DefaultAlmConfig.UseToken);
                    testCaseFieldsList = jiraRep.GetIssueFields(loginData.User, loginData.Password, loginData.Server, ALMCore.DefaultAlmConfig.ALMProjectKey, ResourceType.TEST_CASE, ALMCore.DefaultAlmConfig.UseToken);
                    testExecutionFieldsList = jiraRep.GetIssueFields(loginData.User, loginData.Password, loginData.Server, ALMCore.DefaultAlmConfig.ALMProjectKey, ResourceType.TEST_CASE_EXECUTION_RECORDS, ALMCore.DefaultAlmConfig.UseToken);

                    fields.Append(SetALMItemsFields(testSetFieldsList, ResourceType.TEST_SET));
                    fields.Append(SetALMItemsFields(testCaseFieldsList, ResourceType.TEST_CASE));
                    fields.Append(SetALMItemsFields(testExecutionFieldsList, ResourceType.TEST_CASE_EXECUTION_RECORDS));
                }
            }
            catch (Exception e) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e); }

            return fields;
        }

        private ObservableList<ExternalItemFieldBase> SetALMItemsFields(AlmResponseWithData<JiraFieldColl> testCaseFieldsList, ResourceType fieldResourceType)
        {
            ObservableList<ExternalItemFieldBase> resourceFields = [];
            string fieldResourceTypeToString = fieldResourceType.ToString();
            if (fieldResourceType == ResourceType.TEST_CASE_EXECUTION_RECORDS)
            {
                fieldResourceTypeToString = "TEST_EXECUTION";
            }
            foreach (var field in testCaseFieldsList.DataResult)
            {
                if (string.IsNullOrEmpty(field.name))
                {
                    continue;
                }

                ExternalItemFieldBase itemfield = new ExternalItemFieldBase
                {
                    ID = field.name,
                    Name = field.name,
                    Mandatory = field.required,
                    ItemType = fieldResourceTypeToString
                };

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
                if (testSet == null)
                {
                    return null;
                }

                //Create Business Flow
                BusinessFlow busFlow = new BusinessFlow
                {
                    Name = testSet.Name,
                    ExternalID = testSet.Key,
                    Status = BusinessFlow.eBusinessFlowStatus.Development,
                    Activities = [],
                    Variables = []
                };
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
                        List<string> stepParamsList = [];
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
                            ActivityIdentifiers actIdent = tcActivsGroup.ActivitiesIdentifiers.FirstOrDefault(x => x.ActivityExternalID == step.StepID);
                            if (actIdent == null || actIdent.IdentifiedActivity == null)
                            {
                                break;//something wrong- shouldnt be null
                            }

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
                                        tc.Steps.FirstOrDefault(x => x.StepID == ident.ActivityExternalID) == null)
                                {
                                    continue;//activity which not originaly came from the TC
                                }

                                numOfSeenSteps++;

                                if (numOfSeenSteps >= stepIndx)
                                {
                                    break;
                                }
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

        internal BusinessFlow ConvertJiraZypherCycleToBF(JiraZephyrCycle cycle)
        {
            try
            {
                if (cycle == null)
                {
                    return null;
                }

                //Create Business Flow
                BusinessFlow busFlow = new BusinessFlow
                {
                    Name = cycle.name,
                    ExternalID = cycle.id.ToString(),
                    Status = BusinessFlow.eBusinessFlowStatus.Development,
                    Activities = [],
                    Variables = []
                };
                //Create Activities Group + Activities for each TC
                foreach (JiraZephyrIssue issue in cycle.IssuesList)
                {
                    // converting JiraZephyrIssue to JiraTest
                    // and JiraZephyrTeststep to JiraTestStep - in order to re-use existing code
                    List<JiraTestStep> steps = [];
                    issue.Steps.ForEach(z => steps.Add(new JiraTestStep() { StepID = z.id.ToString(), StepName = z.step, Description = z.data }));
                    JiraTest tc = new JiraTest(issue.id.ToString(), issue.name, issue.name, steps);

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
                        List<string> stepParamsList = [];
                        if ((step.Expected != null) && ((step.Expected == string.Empty)))
                        {
                            GetStepParameters(StripHTML(step.Expected), ref stepParamsList);
                        }
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
                            ActivityIdentifiers actIdent = tcActivsGroup.ActivitiesIdentifiers.FirstOrDefault(x => x.ActivityExternalID == step.StepID);
                            if (actIdent == null || actIdent.IdentifiedActivity == null)
                            {
                                break;//something wrong- shouldnt be null
                            }

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
                                        tc.Steps.FirstOrDefault(x => x.StepID == ident.ActivityExternalID) == null)
                                {
                                    continue;//activity which not originaly came from the TC
                                }

                                numOfSeenSteps++;

                                if (numOfSeenSteps >= stepIndx)
                                {
                                    break;
                                }
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

        internal BusinessFlow ConvertJiraZephyrCycleToBF(JiraTestSet testSet)
        {
            try
            {
                if (testSet == null)
                {
                    return null;
                }

                //Create Business Flow
                BusinessFlow busFlow = new BusinessFlow
                {
                    Name = testSet.Name,
                    ExternalID = testSet.Key,
                    Status = BusinessFlow.eBusinessFlowStatus.Development,
                    Activities = [],
                    Variables = []
                };
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
                        List<string> stepParamsList = [];
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
                            ActivityIdentifiers actIdent = tcActivsGroup.ActivitiesIdentifiers.FirstOrDefault(x => x.ActivityExternalID == step.StepID);
                            if (actIdent == null || actIdent.IdentifiedActivity == null)
                            {
                                break;//something wrong- shouldnt be null
                            }

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
                                        tc.Steps.FirstOrDefault(x => x.StepID == ident.ActivityExternalID) == null)
                                {
                                    continue;//activity which not originaly came from the TC
                                }

                                numOfSeenSteps++;

                                if (numOfSeenSteps >= stepIndx)
                                {
                                    break;
                                }
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
            JiraTestParameter tcParameter = tc.Parameters.FirstOrDefault(x => x.Name.ToUpper() == param.ToUpper());

            //get the param value
            if (tcParameter != null && tcParameter.Value != null && tcParameter.Value != string.Empty)
            {
                paramSelectedValue = tcParameter.Value;
            }
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
                if (valueParts.Length == 3)
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
                {
                    paramSelectedValue = paramSelectedValue[3..];//get value without "$$_"
                }
            }
            else if (paramSelectedValue != "<Empty>")
            {
                isflowControlParam = true;
            }

            //check if already exist param with that name
            VariableBase stepActivityVar = stepActivity.Variables.FirstOrDefault(x => x.Name.ToUpper() == param.ToUpper());
            if (stepActivityVar == null)
            {
                //#Param not exist so add it
                if (isflowControlParam == true)
                {
                    //add it as selection list param                               
                    stepActivityVar = new VariableSelectionList
                    {
                        Name = param
                    };
                    stepActivity.AddVariable(stepActivityVar);
                    stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because new flow control param was added
                }
                else
                {
                    //add as String param
                    stepActivityVar = new VariableString
                    {
                        Name = param
                    };
                    ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                    stepActivity.AddVariable(stepActivityVar);
                }
            }
            else
            {
                //#param exist
                if (isflowControlParam == true)
                {
                    if (stepActivityVar is not VariableSelectionList)
                    {
                        //flow control param must be Selection List so transform it
                        stepActivity.Variables.Remove(stepActivityVar);
                        stepActivityVar = new VariableSelectionList
                        {
                            Name = param
                        };
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
                        stepActivityVar = new VariableString
                        {
                            Name = param
                        };
                        ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                        stepActivity.AddVariable(stepActivityVar);
                        stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because flow control param was removed
                    }
                }
            }

            //add the variable selected value                          
            if (stepActivityVar is VariableSelectionList)
            {
                OptionalValue stepActivityVarOptionalVar = ((VariableSelectionList)stepActivityVar).OptionalValuesList.FirstOrDefault(x => x.Value == paramSelectedValue);
                if (stepActivityVarOptionalVar == null)
                {
                    //no such variable value option so add it
                    stepActivityVarOptionalVar = new OptionalValue(paramSelectedValue);
                    ((VariableSelectionList)stepActivityVar).OptionalValuesList.Add(stepActivityVarOptionalVar);
                    if (isflowControlParam == true)
                    {
                        stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because new param value was added
                    }
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
                    {
                        ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                    }
                }
                catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
            }

            //add linked variable if needed
            if (string.IsNullOrEmpty(linkedVariable) == false)
            {
                stepActivityVar.LinkedVariableName = linkedVariable;
            }
            else
            {
                stepActivityVar.LinkedVariableName = string.Empty;//clear old links
            }
        }

        private void ConvertJiraStepToActivity(BusinessFlow busFlow, JiraTest tc, ActivitiesGroup tcActivsGroup, JiraTestStep step, out Activity stepActivity, out bool toAddStepActivity)
        {
            toAddStepActivity = false;

            //check if mapped activity exist in repository
            Activity repoStepActivity = GingerActivitiesRepo.FirstOrDefault(x => x.ExternalID == step.StepID);
            if (repoStepActivity != null)
            {
                //check if it is part of the Activities Group
                ActivityIdentifiers groupStepActivityIdent = tcActivsGroup.ActivitiesIdentifiers.FirstOrDefault(x => x.ActivityExternalID == step.StepID);
                if (groupStepActivityIdent != null)
                {
                    //already in Activities Group so get link to it
                    stepActivity = busFlow.Activities.FirstOrDefault(x => x.Guid == groupStepActivityIdent.ActivityGuid);
                    // in any case update description/expected/name - even if "step" was taken from repository
                    stepActivity.Description = StripHTML(step.Description);
                    //stepActivity.Expected = StripHTML(step.Expected);
                    stepActivity.ActivityName = tc.TestName + ">" + step.StepName;
                }
                else//not in ActivitiesGroup so get instance from repo
                {
                    stepActivity = (Activity)repoStepActivity.CreateInstance();
                    stepActivity.ExternalID = step.StepID;
                    toAddStepActivity = true;
                }
            }
            else//Step not exist in Ginger repository so create new one
            {
                stepActivity = new Activity
                {
                    ActivityName = tc.TestName + ">" + step.StepName,
                    ExternalID = step.StepID,
                    Description = StripHTML(step.Description)
                };
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
            {
                repoActivsGroup = GingerActivitiesGroupsRepo.FirstOrDefault(x => x.ExternalID == tc.TestKey);
            }

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
                busFlow.ImportActivitiesGroupActivitiesFromRepository(tcActivsGroup, GingerActivitiesRepo, ApplicationPlatforms, true);

                busFlow.AttachActivitiesGroupsAndActivities();
            }
            else //TC not exist in Ginger repository so create new one
            {
                tcActivsGroup = new ActivitiesGroup
                {
                    Name = tc.TestName
                };
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
                if (string.IsNullOrEmpty(HTMLText))
                {
                    return HTMLText;
                }
                HTMLText = HTMLText.Replace("<br />", Environment.NewLine);
                Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
                var stripped = reg.Replace(HTMLText, "");
                if (toDecodeHTML)
                {
                    stripped = HttpUtility.HtmlDecode(stripped);
                }

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
            WhereDataList filterData = [];
            JiraTestSet issue = new JiraTestSet();
            filterData.Add(new WhereData() { Name = "id", Values = [currentTS.Key], Operator = WhereOperator.And });
            AlmResponseWithData<List<JiraIssue>> getTestsSet = jiraRepObj.GetJiraIssues(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL, ALMCore.DefaultAlmConfig.ALMProjectName, ResourceType.TEST_SET, filterData, ALMCore.DefaultAlmConfig.UseToken);
            List<FieldSchema> templates = ExportSettings.Instance.GetSchemaByProject(ALMCore.DefaultAlmConfig.ALMProjectName, ResourceType.TEST_SET);
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
                                    issue.Tests = [];
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
            WhereDataList filterData = [];
            foreach (JiraTest test in tests)
            {
                filterData.Clear();
                filterData.Add(new WhereData() { Name = "id", Values = [test.TestID], Operator = WhereOperator.And });
                AlmResponseWithData<List<JiraIssue>> getTest = jiraRepObj.GetJiraIssues(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL, ALMCore.DefaultAlmConfig.ALMProjectName, ResourceType.TEST_CASE, filterData, ALMCore.DefaultAlmConfig.UseToken);
                ObservableList<JiraTest> jiratests = [];
                List<FieldSchema> templates = ExportSettings.Instance.GetSchemaByProject(ALMCore.DefaultAlmConfig.ALMProjectName, ResourceType.TEST_CASE);
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
                                        test.Steps = [];
                                        var stepAnonymousTypeDef = new { id = 0, index = 0, step = string.Empty, data = string.Empty };
                                        foreach (var val in fieldValue)
                                        {
                                            var stepAnonymous = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(val, stepAnonymousTypeDef);
                                            string[] stepDescription = new[] { "", "" };
                                            string[] stepVariables = new[] { "", "" };
                                            if (!string.IsNullOrEmpty(stepAnonymous.data) && stepAnonymous.data.Contains("=>"))
                                            {
                                                string[] getStepData = (stepAnonymous.data).Split(new[] { "=>" }, StringSplitOptions.None);
                                                if (getStepData.Length > 1 && getStepData[1].Contains("Description:"))
                                                {
                                                    stepDescription = getStepData[1].Split(new[] { "Description:" }, StringSplitOptions.None);
                                                }
                                                if (getStepData.Length > 2 && getStepData[2].Contains("Variables:"))
                                                {
                                                    stepVariables = getStepData[2].Split(new[] { "Variables:" }, StringSplitOptions.None);
                                                }
                                            }
                                            else
                                            {
                                                stepDescription[1] = stepAnonymous.data;
                                            }
                                            test.Steps.Add(new JiraTestStep() { StepID = stepAnonymous.id.ToString(), StepName = stepAnonymous.step, Description = StripHTML(stepDescription[1]), Variables = stepVariables[1] });
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
            WhereDataList filterData = [];
            List<string> testSetKeys = ["reporter", "created", "summary", "project"];
            filterData.Add(new WhereData() { Name = "fields", Values = testSetKeys, Operator = WhereOperator.Ampersand });
            AlmResponseWithData<List<JiraIssue>> getTestsSet = jiraRepObj.GetJiraIssues(ALMCore.DefaultAlmConfig.ALMUserName, ALMCore.DefaultAlmConfig.ALMPassword, ALMCore.DefaultAlmConfig.ALMServerURL, ALMCore.DefaultAlmConfig.ALMProjectName, ResourceType.TEST_SET, filterData, ALMCore.DefaultAlmConfig.UseToken);

            ObservableList<JiraTestSet> jiratestset = [];
            List<FieldSchema> templates = ExportSettings.Instance.GetSchemaByProject(ALMCore.DefaultAlmConfig.ALMProjectName, ResourceType.TEST_SET);
            foreach (var item in getTestsSet.DataResult)
            {
                JiraTestSet issue = new JiraTestSet
                {
                    ID = item.id.ToString(),
                    URLPath = item.self,
                    Key = item.key
                };

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
            List<string> valuesList = [];
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
            if ((busFlow == null) || (activitiesGroupToUpdatedData.Count == 0))
            {
                return;
            }

            Dictionary<string, string> busVariables = [];

            int startGroupActsIndxInBf = 0;
            Dictionary<string, int> activityGroupsToRemoveIndexes = [];
            foreach (JiraTest tc in activitiesGroupToUpdatedData.Values)
            {
                var activitiesToRemove = busFlow.Activities.Where(x => tc.Steps.Any(stepid => stepid.StepID.Equals(x.ExternalID))).ToList();
                foreach (Activity activityToRemove in activitiesToRemove)
                {
                    if (startGroupActsIndxInBf < busFlow.Activities.IndexOf(activityToRemove))
                    {
                        startGroupActsIndxInBf = busFlow.Activities.IndexOf(activityToRemove);
                    }

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

                tcActivsGroup = new ActivitiesGroup
                {
                    Name = tc.TestName,
                    ExternalID = tc.TestKey,
                    Description = tc.Description
                };
                busFlow.AddActivitiesGroup(tcActivsGroup, activityGroupToRemoveIndex);

                //Add the TC steps as Activities if not already on the Activities group
                foreach (JiraTestStep step in tc.Steps)
                {
                    Activity stepActivity;
                    bool toAddStepActivity = false;

                    //check if mapped activity exist in repository
                    Activity repoStepActivity = GingerActivitiesRepo.FirstOrDefault(x => x.ExternalID == step.StepID);
                    if (repoStepActivity != null)
                    {
                        //check if it is part of the Activities Group
                        ActivityIdentifiers groupStepActivityIdent = tcActivsGroup.ActivitiesIdentifiers.FirstOrDefault(x => x.ActivityExternalID == step.StepID);
                        if (groupStepActivityIdent != null)
                        {
                            //already in Activities Group so get link to it
                            stepActivity = busFlow.Activities.FirstOrDefault(x => x.Guid == groupStepActivityIdent.ActivityGuid);
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
                        stepActivity = new Activity
                        {
                            ActivityName = tc.TestName + ">" + step.StepName,
                            ExternalID = step.StepID,
                            Description = StripHTML(step.Description)
                        };
                        //stepActivity.Expected = StripHTML(step.Expected);

                        toAddStepActivity = true;
                    }

                    if (toAddStepActivity)
                    {
                        //not in group- need to add it
                        busFlow.AddActivity(stepActivity, tcActivsGroup, startGroupActsIndxInBf++);
                    }

                    //pull TC-Step parameters and add them to the Activity level
                    List<string> stepParamsList = [];
                    GetStepParameters(StripHTML(step.Variables), ref stepParamsList);
                    //GetStepParameters(StripHTML(step.Expected), ref stepParamsList);
                    foreach (string param in stepParamsList)
                    {
                        //get the param value
                        string paramSelectedValue = string.Empty;
                        bool? isflowControlParam = null;
                        JiraTestParameter tcParameter = tc.Parameters.FirstOrDefault(x => x.Name.ToUpper() == param.ToUpper());

                        //get the param value
                        if (tcParameter != null && tcParameter.Value != null && tcParameter.Value != string.Empty)
                        {
                            paramSelectedValue = tcParameter.Value;
                        }
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
                            if (valueParts.Length == 3)
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
                            {
                                paramSelectedValue = paramSelectedValue[3..];//get value without "$$_"
                            }
                        }
                        else if (paramSelectedValue != "<Empty>")
                        {
                            isflowControlParam = true;
                        }

                        //check if already exist param with that name
                        VariableBase stepActivityVar = stepActivity.Variables.FirstOrDefault(x => x.Name.ToUpper() == param.ToUpper());
                        if (stepActivityVar == null)
                        {
                            //#Param not exist so add it
                            if (isflowControlParam == true)
                            {
                                //add it as selection list param                               
                                stepActivityVar = new VariableSelectionList
                                {
                                    Name = param
                                };
                                stepActivity.AddVariable(stepActivityVar);
                                stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because new flow control param was added
                            }
                            else
                            {
                                //add as String param
                                stepActivityVar = new VariableString
                                {
                                    Name = param
                                };
                                ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                                stepActivity.AddVariable(stepActivityVar);
                            }
                        }
                        else
                        {
                            //#param exist
                            if (isflowControlParam == true)
                            {
                                if (stepActivityVar is not VariableSelectionList)
                                {
                                    //flow control param must be Selection List so transform it
                                    stepActivity.Variables.Remove(stepActivityVar);
                                    stepActivityVar = new VariableSelectionList
                                    {
                                        Name = param
                                    };
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
                                    stepActivityVar = new VariableString
                                    {
                                        Name = param
                                    };
                                    ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                                    stepActivity.AddVariable(stepActivityVar);
                                    stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because flow control param was removed
                                }
                            }
                        }

                        //add the variable selected value                          
                        if (stepActivityVar is VariableSelectionList)
                        {
                            OptionalValue stepActivityVarOptionalVar = ((VariableSelectionList)stepActivityVar).OptionalValuesList.FirstOrDefault(x => x.Value == paramSelectedValue);
                            if (stepActivityVarOptionalVar == null)
                            {
                                //no such variable value option so add it
                                stepActivityVarOptionalVar = new OptionalValue(paramSelectedValue);
                                ((VariableSelectionList)stepActivityVar).OptionalValuesList.Add(stepActivityVarOptionalVar);
                                if (isflowControlParam == true)
                                {
                                    stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because new param value was added
                                }
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
                                {
                                    ((VariableString)stepActivityVar).InitialStringValue = paramSelectedValue;
                                }
                            }
                            catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
                        }

                        //add linked variable if needed
                        if (string.IsNullOrEmpty(linkedVariable) == false)
                        {
                            stepActivityVar.LinkedVariableName = linkedVariable;
                        }
                        else
                        {
                            stepActivityVar.LinkedVariableName = string.Empty;//clear old links
                        }
                    }
                }

                //order the Activities Group activities according to the order of the matching steps in the TC
                try
                {
                    foreach (JiraTestStep step in tc.Steps)
                    {
                        int stepIndx = tc.Steps.IndexOf(step) + 1;
                        ActivityIdentifiers actIdent = tcActivsGroup.ActivitiesIdentifiers.FirstOrDefault(x => x.ActivityExternalID == step.StepID);
                        if (actIdent == null || actIdent.IdentifiedActivity == null)
                        {
                            break;//something wrong- shouldnt be null
                        }

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
                                    tc.Steps.FirstOrDefault(x => x.StepID == ident.ActivityExternalID) == null)
                            {
                                continue;//activity which not originaly came from the TC
                            }

                            numOfSeenSteps++;

                            if (numOfSeenSteps >= stepIndx)
                            {
                                break;
                            }
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
            return;
        }
        public void UpdateBussinessFlow(ref BusinessFlow busFlow)
        {
            JiraTestSet testSet = GetTestSetData(new JiraTestSet { Key = busFlow.ExternalID });
            Dictionary<string, JiraTest> activitiesGroupToUpdatedData = GetJiraSelectedTestsData(busFlow.ExternalID, busFlow.ActivitiesGroups.Select(actid => actid.ExternalID).ToList());
            if (busFlow == null)
            {
                return;
            }

            busFlow.Name = testSet.Name;
            busFlow.Description = testSet.Description;
            UpdateBFSelectedAG(ref busFlow, activitiesGroupToUpdatedData);
        }
        public Dictionary<string, JiraTest> GetJiraSelectedTestsData(string testSetID, List<string> TCsIds = null)
        {
            Dictionary<string, JiraTest> existsTestInJira = [];

            JiraTestSet testSet = GetTestSetData(new JiraTestSet { Key = testSetID });
            if (testSet != null && testSet.Tests.Count > 0)
            {
                foreach (string tc in TCsIds)
                {
                    existsTestInJira.Add(tc, testSet.Tests.FirstOrDefault(tst => tst.TestKey.Equals(tc)));
                }
            }

            return existsTestInJira;
        }
    }
}
