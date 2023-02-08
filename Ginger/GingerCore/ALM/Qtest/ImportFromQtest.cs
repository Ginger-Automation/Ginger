#region License
/*
Copyright © 2014-2023 European Support Limited

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
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCore.Activities;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using QCRestClientStd;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using TDAPIOLELib;

namespace GingerCore.ALM.Qtest
{
    public static class ImportFromQtest
    {
        static QTestAPIStd.LoginApi connObj = new QTestAPIStd.LoginApi();
        static QTestAPIStd.ProjectApi projectsApi = new QTestAPIStd.ProjectApi();
        static QTestAPIStd.FieldApi fieldApi = new QTestAPIStd.FieldApi();

        static QTestAPIStdClient.ApiClient apiClient = new QTestAPIStdClient.ApiClient();
        static QTestAPIStdClient.Configuration configuration = new QTestAPIStdClient.Configuration();

        public static ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo { get; set; }
        public static ObservableList<Activity> GingerActivitiesRepo { get; set; }
        public static ObservableList<ApplicationPlatform> ApplicationPlatforms { get; set; }
        
        
        public static QtestTestSuite ImportTestSuiteData(QtestTestSuite TS, long projectId)
        {
            QTestAPIStd.TestsuiteApi testsuiteApi = new QTestAPIStd.TestsuiteApi(connObj.Configuration);
            QTestAPIStdModel.TestSuiteWithCustomFieldResource testSuite = testsuiteApi.GetTestSuite(projectId, (long)Convert.ToInt32(TS.ID));
            return TS;
        }

        public static QtestTest ImportTSTest(QtestTest tsTest)
        {
            QtestTest newTSTest = new QtestTest();
            string linkedTest = string.Empty;

            //Get the TC general details         
            newTSTest.TestID = tsTest.TestID;
            newTSTest.TestName = tsTest.TestName;

            //Get the TC design steps
            foreach (QtestTestStep tsStep in tsTest.Steps)
            {
                QtestTestStep newtsStep = new QtestTestStep();
                newtsStep.StepID = tsStep.StepID.ToString();
                newtsStep.StepName = tsStep.StepName;
                newtsStep.Description = tsStep.Description;
                newtsStep.Expected = tsStep.Expected;
                newTSTest.Steps.Add(newtsStep);
            }

            //Get the TC execution history
            try
            {
                List<QtestTestRun> TSTestRuns = GetListTSTestRuns(tsTest);
                newTSTest.Runs = new List<QtestTestRun>();
                foreach (Run run in TSTestRuns)
                {
                    QtestTestRun newtsRun = new QtestTestRun();
                    newtsRun.RunID = run.ID.ToString();
                    newtsRun.RunName = run.Name;
                    newtsRun.Status = run.Status;
                    newtsRun.ExecutionDate = (run["RN_EXECUTION_DATE"]).ToString();
                    newtsRun.ExecutionTime = (run["RN_EXECUTION_TIME"]).ToString();
                    newtsRun.Tester = (run["RN_TESTER_NAME"]).ToString();
                    newTSTest.Runs.Add(newtsRun);
                }
            }catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to pull QC test case RUN info", ex);
                newTSTest.Runs = new List<QtestTestRun>();
            }

            return newTSTest;
        }

        private static List<QtestTestRun> GetListTSTestRuns(QtestTest tsTest)
        {
            return new List<QtestTestRun>();
        }

        public static BusinessFlow ConvertQtestTestSuiteToBF(QtestTestSuite testSuite)
        {
            try
            {
                if (testSuite == null)
                {
                    return null;
                }

                //Create Business Flow
                BusinessFlow busFlow = new BusinessFlow();
                busFlow.Name = testSuite.Name;
                busFlow.ExternalID = testSuite.ID;
                busFlow.Status = BusinessFlow.eBusinessFlowStatus.Development;
                busFlow.Activities = new ObservableList<Activity>();
                busFlow.Variables = new ObservableList<VariableBase>();
                Dictionary<string, string> busVariables = new Dictionary<string, string>();//will store linked variables

                //Create Activities Group + Activities for each TC
                foreach (QtestTest tc in testSuite.Tests)
                {
                    //check if the TC is already exist in repository
                    ActivitiesGroup tcActivsGroup;
                    ActivitiesGroup repoActivsGroup = null;
                    if (tc.LinkedTestID != null && tc.LinkedTestID != string.Empty)
                    {
                        repoActivsGroup = GingerActivitiesGroupsRepo.Where(x => x.ExternalID == tc.LinkedTestID).FirstOrDefault();
                    }
                    if (repoActivsGroup == null)
                    {
                        repoActivsGroup = GingerActivitiesGroupsRepo.Where(x => x.ExternalID == tc.TestID).FirstOrDefault();
                    }
                    if (repoActivsGroup != null)
                    {
                        List<Activity> repoNotExistsStepActivity = GingerActivitiesRepo.Where(z => repoActivsGroup.ActivitiesIdentifiers.Select(y => y.ActivityExternalID).ToList().Contains(z.ExternalID))
                                                                                       .Where(x => !tc.Steps.Select(y => y.StepID).ToList().Contains(x.ExternalID)).ToList();

                        tcActivsGroup = (ActivitiesGroup)(repoActivsGroup).CreateInstance(true);

                        var ActivitySIdentifiersToRemove = tcActivsGroup.ActivitiesIdentifiers.Where(x => repoNotExistsStepActivity.Select(z => z.ExternalID).ToList().Contains(x.ActivityExternalID)); 
                        for (int indx = 0; indx < tcActivsGroup.ActivitiesIdentifiers.Count; indx++)
                        {
                            if ((indx < tcActivsGroup.ActivitiesIdentifiers.Count) && (ActivitySIdentifiersToRemove.Contains(tcActivsGroup.ActivitiesIdentifiers[indx])))
                            {
                                tcActivsGroup.ActivitiesIdentifiers.Remove(tcActivsGroup.ActivitiesIdentifiers[indx]);
                                indx--;
                            }
                        }

                        tcActivsGroup.ExternalID2 = tc.TestID;
                        busFlow.AddActivitiesGroup(tcActivsGroup);
                        busFlow.ImportActivitiesGroupActivitiesFromRepository(tcActivsGroup,GingerActivitiesRepo, ApplicationPlatforms, true);
                        busFlow.AttachActivitiesGroupsAndActivities();
                    }
                    else //TC not exist in Ginger repository so create new one
                    {
                        tcActivsGroup = new ActivitiesGroup();
                        tcActivsGroup.Name = tc.TestName;
                        if (tc.LinkedTestID == null || tc.LinkedTestID == string.Empty)
                        {
                            tcActivsGroup.ExternalID = tc.TestID;
                            tcActivsGroup.ExternalID2 = tc.TestID;
                        }
                        else
                        {
                            tcActivsGroup.ExternalID = tc.LinkedTestID;
                            tcActivsGroup.ExternalID2 = tc.TestID; //original TC ID will be used for uploading the execution details back to QC
                            tcActivsGroup.Description = tc.Description;
                        }
                        busFlow.AddActivitiesGroup(tcActivsGroup);
                    }

                    //Add the TC steps as Activities if not already on the Activities group
                    foreach (QtestTestStep step in tc.Steps)
                    {
                        Activity stepActivity;
                        bool toAddStepActivity = false;

                        //check if mapped activity exist in repository
                        Activity repoStepActivity = GingerActivitiesRepo.Where(x => x.ExternalID == step.StepID).FirstOrDefault();
                        if (repoStepActivity != null)
                        {
                            //check if it is part of the Activities Group
                            ActivityIdentifiers groupStepActivityIdent = (ActivityIdentifiers)tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.StepID).FirstOrDefault();
                            if (groupStepActivityIdent != null)
                            {
                                //already in Activities Group so get link to it
                                stepActivity =(Activity)busFlow.Activities.Where(x => x.Guid == groupStepActivityIdent.ActivityGuid).FirstOrDefault();
                                // in any case update description/expected/name - even if "step" was taken from repository
                                stepActivity.Description = StripHTML(step.Description);
                                stepActivity.Expected = StripHTML(step.Expected);
                                stepActivity.ActivityName = tc.TestName + ">" + step.StepName;
                            }
                            else // not in ActivitiesGroup so get instance from repo
                            {
                                stepActivity = (Activity)(repoStepActivity).CreateInstance(true);
                                toAddStepActivity = true;
                            }
                        }
                        else//Step not exist in Ginger repository so create new one
                        {
                            stepActivity = new Activity();
                            stepActivity.ActivityName = step.StepName;
                            stepActivity.ExternalID = step.StepID;
                            stepActivity.ExternalID2 = step.CalledTestCaseId;
                            stepActivity.Description = step.Description;
                            stepActivity.Expected = step.Expected;

                            toAddStepActivity = true;
                        }

                        if (toAddStepActivity)
                        {
                            //not in group- need to add it
                            busFlow.AddActivity(stepActivity, tcActivsGroup);                            
                        }

                        //pull TC-Step parameters and add them to the Activity level
                        if (step.Params != null)
                        {
                            VariableSelectionList stepActivityVar = (VariableSelectionList)stepActivity.Variables.Where(x => x.Name.ToUpper() == step.Params.Name.ToUpper()).FirstOrDefault();
                            if (stepActivityVar != null)
                            {
                                stepActivity.Variables.Remove(stepActivityVar);
                            }
                            stepActivityVar = new VariableSelectionList();
                            stepActivityVar.Name = step.Params.Name;
                            ObservableList<OptionalValue> optionalValuesList = new ObservableList<OptionalValue>();
                            step.Params.Values.ForEach(z => optionalValuesList.Add(new OptionalValue(z)));
                            (stepActivityVar).OptionalValuesList = optionalValuesList;
                            (stepActivityVar).SetValue(step.Params.Value);
                            stepActivity.AddVariable(stepActivityVar);
                        }                                
                    }

                    //order the Activities Group activities according to the order of the matching steps in the TC
                    try
                    {
                        int startGroupActsIndxInBf = busFlow.Activities.IndexOf(tcActivsGroup.ActivitiesIdentifiers[0].IdentifiedActivity);
                        foreach (QtestTestStep step in tc.Steps)
                        {
                            int stepIndx = tc.Steps.IndexOf(step) + 1;
                            ActivityIdentifiers actIdent = (ActivityIdentifiers)tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.StepID).FirstOrDefault();
                            if (actIdent == null || actIdent.IdentifiedActivity == null)
                            {
                                break; // something wrong- shouldn't be null
                            }
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
                                {
                                    continue; // activity which not originally came from the TC
                                }
                                numOfSeenSteps++;

                                if (numOfSeenSteps >= stepIndx)
                                {
                                    break;
                                }
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

                //Add the BF variables (linked variables)
                if (busVariables.Keys.Count > 0)
                {
                    foreach (KeyValuePair<string, string> var in busVariables)
                    {
                        //add as String param
                        VariableString busVar = new VariableString();
                        busVar.Name = var.Key;
                        (busVar).InitialStringValue = var.Value;
                        busFlow.AddVariable(busVar);
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

        public static void UpdatedQCTestInBF(ref BusinessFlow busFlow, List<QtestTest> tcsList)
        {
            if ((busFlow == null) || (tcsList == null) || (tcsList.Count < 1))
            {
                return;
            }
            Dictionary<string, string> busVariables = new Dictionary<string, string>();

            int startGroupActsIndxInBf = 0;
            Dictionary<string, int> activityGroupsToRemoveIndexes = new Dictionary<string, int>();
            foreach (QtestTest tc in tcsList)
            {
                var activitiesToRemove = busFlow.Activities.Where(x => x.ActivitiesGroupID == tc.TestName).ToList();
                foreach (Activity activityToRemove in activitiesToRemove)
                {
                    if (startGroupActsIndxInBf < busFlow.Activities.IndexOf(activityToRemove))
                    {
                        startGroupActsIndxInBf = busFlow.Activities.IndexOf(activityToRemove);
                    }
                    busFlow.Activities.Remove(activityToRemove);
                }
                var activityGroupsToRemove = busFlow.ActivitiesGroups.Where(x => x.ExternalID2 == tc.TestID).ToList();
                foreach (ActivitiesGroup activityGroupToRemove in activityGroupsToRemove)
                {
                    activityGroupsToRemoveIndexes.Add(activityGroupToRemove.ExternalID2, busFlow.ActivitiesGroups.IndexOf(activityGroupToRemove));
                }
                foreach (ActivitiesGroup activityGroupToRemove in activityGroupsToRemove)
                {
                    busFlow.ActivitiesGroups.Remove(activityGroupToRemove);
                }
            }

            int activityGroupToRemoveIndex;
            foreach (QtestTest tc in tcsList)
            {
                activityGroupsToRemoveIndexes.TryGetValue(tc.TestID, out activityGroupToRemoveIndex);

                //check if the TC is already exist in repository
                ActivitiesGroup tcActivsGroup = new ActivitiesGroup();
                tcActivsGroup.Name = tc.TestName;
                if (tc.LinkedTestID == null || tc.LinkedTestID == string.Empty)
                {
                    tcActivsGroup.ExternalID = tc.TestID;
                    tcActivsGroup.ExternalID2 = tc.TestID;
                }
                else
                {
                    tcActivsGroup.ExternalID = tc.LinkedTestID;
                    tcActivsGroup.ExternalID2 = tc.TestID; //original TC ID will be used for uploading the execution details back to QC
                    tcActivsGroup.Description = tc.Description;
                }
                busFlow.AddActivitiesGroup(tcActivsGroup, activityGroupToRemoveIndex);

                //Add the TC steps as Activities if not already on the Activities group
                foreach (QtestTestStep step in tc.Steps)
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
                        stepActivity.Expected = StripHTML(step.Expected);

                        toAddStepActivity = true;
                    }

                    if (toAddStepActivity)
                    {
                        //not in group- need to add it
                        busFlow.AddActivity(stepActivity, tcActivsGroup, startGroupActsIndxInBf++);                        
                    }

                    //pull TC-Step parameters and add them to the Activity level
                    List<string> stepParamsList = new List<string>();
                    GetStepParameters(StripHTML(step.Description), ref stepParamsList);
                    GetStepParameters(StripHTML(step.Expected), ref stepParamsList);
                    foreach (string param in stepParamsList)
                    {
                        //get the param value
                        string paramSelectedValue = string.Empty;
                        bool? isflowControlParam = null;
                        QtestTestParameter tcParameter = tc.Parameters.Where(x => x.Name.ToUpper() == param.ToUpper()).FirstOrDefault();

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

                        //determine if the param is Flow Control Param or not based on it value and agreed sign "$$_"
                        if (paramSelectedValue.StartsWith("$$_"))
                        {
                            isflowControlParam = false;
                            if (paramSelectedValue.StartsWith("$$_"))
                            {
                                paramSelectedValue = paramSelectedValue.Substring(3);//get value without "$$_"
                            }
                        }
                        else if (paramSelectedValue != "<Empty>")
                        {
                            isflowControlParam = true;
                        }
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
                            stepActivityVar.LinkedVariableName = string.Empty; // clear old links
                        }
                    }
                }

                //order the Activities Group activities according to the order of the matching steps in the TC
                try
                {
                    foreach (QtestTestStep step in tc.Steps)
                    {
                        int stepIndx = tc.Steps.IndexOf(step) + 1;
                        ActivityIdentifiers actIdent = (ActivityIdentifiers)tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.StepID).FirstOrDefault();
                        if (actIdent == null || actIdent.IdentifiedActivity == null)
                        {
                            break; // something wrong - shouldn't be null
                        }
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
                            {
                                continue; // activity which not originally came from the TC
                            }
                            numOfSeenSteps++;

                            if (numOfSeenSteps >= stepIndx)
                            {
                                break;
                            }
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

        public static void UpdateBusinessFlow(ref BusinessFlow busFlow, List<QtestTest> tcsList)
        {
            if ((busFlow == null) || (tcsList == null) || (tcsList.Count < 1))
            {
                return;
            }
            Dictionary<string, string> busVariables = new Dictionary<string, string>();
            int startGroupActsIndxInBf = 0;

            busFlow.Activities.Clear();
            busFlow.ActivitiesGroups.Clear();

            foreach (QtestTest tc in tcsList)
            {
                //check if the TC is already exist in repository
                ActivitiesGroup tcActivsGroup = new ActivitiesGroup();
                tcActivsGroup.Name = tc.TestName;
                if (tc.LinkedTestID == null || tc.LinkedTestID == string.Empty)
                {
                    tcActivsGroup.ExternalID = tc.TestID;
                    tcActivsGroup.ExternalID2 = tc.TestID;
                }
                else
                {
                    tcActivsGroup.ExternalID = tc.LinkedTestID;
                    tcActivsGroup.ExternalID2 = tc.TestID; //original TC ID will be used for uploading the execution details back to QC
                    tcActivsGroup.Description = tc.Description;
                }
                busFlow.AddActivitiesGroup(tcActivsGroup);

                //Add the TC steps as Activities if not already on the Activities group
                foreach (QtestTestStep step in tc.Steps)
                {
                    Activity stepActivity;
                    bool toAddStepActivity = false;

                    //check if mapped activity exist in repository
                    Activity repoStepActivity =(Activity) GingerActivitiesRepo.Where(x => x.ExternalID == step.StepID).FirstOrDefault();
                    if (repoStepActivity != null)
                    {
                        //check if it is part of the Activities Group
                        ActivityIdentifiers groupStepActivityIdent = (ActivityIdentifiers)tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.StepID).FirstOrDefault();
                        if (groupStepActivityIdent != null)
                        {
                            //already in Activities Group so get link to it
                            stepActivity =(Activity) busFlow.Activities.Where(x => x.Guid == groupStepActivityIdent.ActivityGuid).FirstOrDefault();
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
                        stepActivity.Expected = StripHTML(step.Expected);

                        toAddStepActivity = true;
                    }

                    if (toAddStepActivity)
                    {
                        //not in group- need to add it
                        busFlow.AddActivity(stepActivity, tcActivsGroup, startGroupActsIndxInBf++);                        
                    }

                    //pull TC-Step parameters and add them to the Activity level
                    List<string> stepParamsList = new List<string>();
                    GetStepParameters(StripHTML(step.Description), ref stepParamsList);
                    GetStepParameters(StripHTML(step.Expected), ref stepParamsList);
                    foreach (string param in stepParamsList)
                    {
                        //get the param value
                        string paramSelectedValue = string.Empty;
                        bool? isflowControlParam = null;
                        QtestTestParameter tcParameter = tc.Parameters.Where(x => x.Name.ToUpper() == param.ToUpper()).FirstOrDefault();

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
                            string[] valueParts = paramSelectedValue.Split(new [] { "#$#" }, StringSplitOptions.None);
                            if (valueParts.Count() == 3)
                            {
                                linkedVariable = valueParts[1];
                                paramSelectedValue = "$$_" + valueParts[2];//so it still will be considered as non-flow control

                                if (!busVariables.Keys.Contains(linkedVariable))
                                {
                                    busVariables.Add(linkedVariable, valueParts[2]);
                                }
                            }
                        }

                        //determine if the param is Flow Control Param or not based on it value and agreed sign "$$_"
                        if (paramSelectedValue.StartsWith("$$_"))
                        {
                            isflowControlParam = false;
                            if (paramSelectedValue.StartsWith("$$_"))
                            {
                                paramSelectedValue = paramSelectedValue.Substring(3);//get value without "$$_"
                            }
                        }
                        else if (paramSelectedValue != "<Empty>")
                        {
                            isflowControlParam = true;
                        }

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
                        if (!string.IsNullOrEmpty(linkedVariable))
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
                    foreach (QtestTestStep step in tc.Steps)
                    {
                        int stepIndx = tc.Steps.IndexOf(step) + 1;
                        ActivityIdentifiers actIdent = (ActivityIdentifiers)tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.StepID).FirstOrDefault();
                        if (actIdent == null || actIdent.IdentifiedActivity == null)
                        {
                            break; // something wrong- shouldn't be null
                        }
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
                            {
                                continue;//activity which not originally came from the TC
                            }
                            numOfSeenSteps++;

                            if (numOfSeenSteps >= stepIndx)
                            {
                                break;
                            }
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

        private static string StripHTML(string HTMLText, bool toDecodeHTML = true)
        {
            try
            {
                HTMLText = HTMLText.Replace("<br />", Environment.NewLine);
                Regex reg = new Regex("<[^>]+>", RegexOptions.IgnoreCase);
                var stripped = reg.Replace(HTMLText, "");
                if (toDecodeHTML)
                {
                    stripped = HttpUtility.HtmlDecode(stripped);
                }
                stripped = stripped.TrimStart(new [] { '\r', '\n' });
                stripped = stripped.TrimEnd(new [] { '\r', '\n' });

                return stripped;
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while stripping the HTML from QC TC Step Description/Expected", ex);
                return HTMLText;
            }
        }

        private static void GetStepParameters(string stepText, ref List<string> stepParamsList)
        {
            try
            {
                MatchCollection stepParams = Regex.Matches(stepText, @"\<<<([^>]*)\>>>");

                foreach (var param in stepParams)
                {
                    string strParam = param.ToString().TrimStart(new [] {'<'});
                    strParam = strParam.TrimEnd(new [] { '>' });
                    stepParamsList.Add(strParam);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while pulling the parameters names from QC TC Step Description/Expected", ex);
            }
        }

        public static QtestTest GetQtestTest(string testID)
        {
           
            return new QtestTest();
        }

        public static ObservableList<ExternalItemFieldBase> GetALMItemFields(AlmDataContractsStd.Enums.ResourceType resourceType)
        {
            ObservableList<ExternalItemFieldBase> fields = new ObservableList<ExternalItemFieldBase>();

            if (resourceType == AlmDataContractsStd.Enums.ResourceType.ALL)
            {
                return GetALMItemFields();
            }
            else
            {
                List<QTestAPIStdModel.FieldResource> fieldsCollection = fieldApi.GetFields((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), resourceType.ToString());

                fields.Append(AddFieldsValues(fieldsCollection, resourceType.ToString()));
            }

            return fields;
        }

        private static ObservableList<ExternalItemFieldBase> GetALMItemFields()
        {
            ObservableList<ExternalItemFieldBase> fields = new ObservableList<ExternalItemFieldBase>();
            fieldApi = new QTestAPIStd.FieldApi(connObj.Configuration);

            //QC   ->testSet,    testCase,  designStep,testInstance,designStep,run
            //QTest->test-suites,test-cases,

            List<QTestAPIStdModel.FieldResource> testSetfieldsCollection = fieldApi.GetFields((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), "test-suites");
            List<QTestAPIStdModel.FieldResource> testCasefieldsCollection = fieldApi.GetFields((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), "test-cases");
            List<QTestAPIStdModel.FieldResource> runfieldsCollection = fieldApi.GetFields((long)Convert.ToInt32(ALMCore.DefaultAlmConfig.ALMProjectKey), "test-runs");

            fields.Append(AddFieldsValues(testSetfieldsCollection, "test-suites"));
            fields.Append(AddFieldsValues(testCasefieldsCollection, "test-cases"));
            fields.Append(AddFieldsValues(runfieldsCollection, "test-runs"));

            return fields;
        }
        private static ObservableList<ExternalItemFieldBase> AddFieldsValues(List<QTestAPIStdModel.FieldResource> testSetfieldsCollection, string testSetfieldInRestSyntax)
        {
            ObservableList<ExternalItemFieldBase> fields = new ObservableList<ExternalItemFieldBase>();

            if ((testSetfieldsCollection != null) && (testSetfieldsCollection.Count > 0))
            {
                foreach (QTestAPIStdModel.FieldResource field in testSetfieldsCollection)
                {
                    if (string.IsNullOrEmpty(field.Label))
                    {
                        continue;
                    }

                    ExternalItemFieldBase itemfield = new ExternalItemFieldBase();
                    itemfield.ID = field.OriginalName;
                    itemfield.ExternalID = field.OriginalName;  // Temp ??? Check if ExternalID has other use in this case
                    itemfield.Name = field.Label;
                    bool isCheck;
                    itemfield.Mandatory = bool.TryParse(field.Required.ToString(), out isCheck);
                    itemfield.SystemFieled = bool.TryParse(field.SystemField.ToString(), out isCheck);
                    if (itemfield.Mandatory)
                    {
                        itemfield.ToUpdate = true;
                    }
                    itemfield.ItemType = testSetfieldInRestSyntax;
                    itemfield.Type = field.DataType;

                    if (itemfield.PossibleValues.Count > 0)
                    {
                        itemfield.SelectedValue = field.DefaultValue;
                    }
                   
                    fields.Add(itemfield);
                }
            }

            return fields;
        }
    }
}
