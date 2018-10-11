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
using System.Threading.Tasks;
using GingerCore.Activities;
using System.Text.RegularExpressions;
using System.Web;
using GingerCore.Variables;
using System.Xml;
using RQM_Repository.Data_Contracts;
using RQM_Repository;
using System.IO;
using ALM_Common.DataContracts;
using ALM_Common.Abstractions;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using Newtonsoft.Json;
using GingerCore.External;
using Amdocs.Ginger.Repository;

namespace GingerCore.ALM.RQM
{
    public enum eRQMItemType { TestPlan, TestCase, TestScript }

    public static class ImportFromRQM
    {
        public static ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo { get; set; }
        public static ObservableList<Activity> GingerActivitiesRepo { get; set; }

        public static int totalValues = 0;
        public static string populatedValue = string.Empty;

        public static BusinessFlow ConvertRQMTestPlanToBF(RQMTestPlan testPlan)
        {
            try
            {
                if (testPlan == null) return null;

                //Creat Business Flow
                BusinessFlow busFlow = new BusinessFlow();
                busFlow.Name = testPlan.Name;
                busFlow.ExternalID = "RQMID=" + testPlan.RQMID;
                busFlow.Status = BusinessFlow.eBusinessFlowStatus.Development;
                busFlow.Activities = new ObservableList<Activity>();
                busFlow.Variables = new ObservableList<VariableBase>();
 
                //Create Activities Group + Activities for each TC
                foreach (RQMTestCase tc in testPlan.TestCases)
                {
                    //Add the TC steps as Activities if not already on the Activties group
                    RQMTestScript selectedScript = tc.TestScripts.Where(y => y.Name == tc.SelectedTestScriptName).ToList().FirstOrDefault();
                    if (selectedScript == null)
                    {
                        continue;
                    }

                    RQMExecutionRecord selectedExecutionRecord = testPlan.RQMExecutionRecords.Where(x => x.RelatedTestCaseRqmID == tc.RQMID && x.RelatedTestScriptRqmID == selectedScript.RQMID).FirstOrDefault();
                    string RQMRecordID = selectedExecutionRecord == null ? string.Empty : selectedExecutionRecord.RQMID.ToString();

                    //check if the TC is already exist in repository
                    ActivitiesGroup tcActivsGroup;
                    ActivitiesGroup repoActivsGroup = null;
                    if (repoActivsGroup == null)
                        repoActivsGroup = GingerActivitiesGroupsRepo.Where(x => x.ExternalID != null ? x.ExternalID.Split('|').First().Split('=').Last() == tc.RQMID : false).FirstOrDefault();
                    if (repoActivsGroup != null)
                    {
                        tcActivsGroup = (ActivitiesGroup)repoActivsGroup.CreateInstance(true);
                        tcActivsGroup.ExternalID = tcActivsGroup.ExternalID.Replace("RQMRecordID=" + ExportToRQM.GetExportedIDString(tcActivsGroup.ExternalID, "RQMRecordID"), "RQMRecordID=");
                        busFlow.AddActivitiesGroup(tcActivsGroup);
                        busFlow.ImportActivitiesGroupActivitiesFromRepository(tcActivsGroup, GingerActivitiesRepo, true, true);
                        busFlow.AttachActivitiesGroupsAndActivities();
                        continue;
                    }
                    else // TC not exist in Ginger repository so create new one
                    {
                        tcActivsGroup = new ActivitiesGroup
                        {
                            Name = tc.Name,
                            ExternalID = "RQMID=" + tc.RQMID + "|RQMScriptID=" + selectedScript.RQMID +
                                         "|RQMRecordID=" + RQMRecordID + "|AtsID=" + tc.BTSID,
                            TestSuiteId = tc.TestSuiteId,
                            TestSuiteTitle = tc.TestSuiteTitle
                        };
                        busFlow.AddActivitiesGroup(tcActivsGroup);
                    }


                    // get BTS IDs if exists (ID per step)
                    Dictionary<string, string> strBtsIDs = new Dictionary<string, string>();
                    string[] stringSeparators = new string[] { "***" };
                    string[] results = selectedScript.BTSStepsIDs.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string result in results)
                    {
                        try
                        {
                            strBtsIDs.Add(result.Split('=').First().TrimEnd().Split(' ').Last(), result.Split('=').Last().ToString());
                        }
                        catch { }
                    }
                    //

                    foreach (RQMStep step in selectedScript.Steps)
                    {
                        Activity stepActivity;
                        bool toAddStepActivity = false;

                        // check if mapped activity exist in repository
                        Activity repoStepActivity = GingerActivitiesRepo.Where(x => x.ExternalID != null ? x.ExternalID.Split('|').First().Split('=').Last() == step.RQMIndex : false).FirstOrDefault();
                        if (repoStepActivity != null)
                        {
                            //check if it is part of the Activities Group
                            //ActivityIdentifiers groupStepActivityIdent = tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.RQMIndex).FirstOrDefault();
                            ActivityIdentifiers groupStepActivityIdent = tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID != null ? x.ActivityExternalID.Split('|').First().Split('=').Last() == step.RQMIndex : false).FirstOrDefault();
                            if (groupStepActivityIdent != null)
                            {
                                //already in Activities Group so get link to it
                                stepActivity = busFlow.Activities.Where(x => x.Guid == groupStepActivityIdent.ActivityGuid).FirstOrDefault();
                            }
                            else // not in ActivitiesGroup so get instance from repo
                            {
                                stepActivity = (Activity)repoStepActivity.CreateInstance();
                                toAddStepActivity = true;
                            }
                        }
                        else //Step not exist in Ginger repository so create new one
                        {
                            stepActivity = new Activity();
                            stepActivity.ActivityName = tc.Name + ">" + step.RQMIndex.Split('_')[1];
                            stepActivity.Description = StripHTML(step.Description);
                            stepActivity.Expected = StripHTML(step.ExpectedResult);

                            string currentStepATSId = string.Empty;
                            if (strBtsIDs.TryGetValue((selectedScript.Steps.IndexOf(step) + 1).ToString(), out currentStepATSId))
                            {
                                stepActivity.ExternalID = "RQMID=" + step.RQMIndex + "|AtsID=" + currentStepATSId;
                            }
                            else
                            {
                                stepActivity.ExternalID = "RQMID=" + step.RQMIndex;
                            }

                            toAddStepActivity = true;
                        }

                        if (toAddStepActivity)
                        {
                            // not in group- need to add it
                            busFlow.AddActivity(stepActivity);
                            tcActivsGroup.AddActivityToGroup(stepActivity);
                        }

                        //pull TC-Step parameters and add them to the Activity level
                        foreach (RQMTestParameter param in selectedScript.Parameters)   // Params taken from TestScriptLevel only!!!! Also exists parapameters at TestCase, to check if them should be taken!!!
                        {                                                               
                            bool? isflowControlParam = null;

                            //detrmine if the param is Flow Control Param or not based on it value and agreed sign "$$_"
                            if (param.Value.ToString().StartsWith("$$_"))
                            {
                                isflowControlParam = false;
                                if (param.Value.ToString().StartsWith("$$_"))
                                    param.Value = param.Value.ToString().Substring(3); //get value without "$$_"
                            }
                            else if (param.Value.ToString() != "<Empty>")
                                isflowControlParam = true;

                            //check if already exist param with that name
                            VariableBase stepActivityVar = stepActivity.Variables.Where(x => x.Name.ToUpper() == param.Name.ToUpper()).FirstOrDefault();
                            if (stepActivityVar == null)
                            {
                                //#Param not exist so add it
                                if (isflowControlParam == true)
                                {
                                    //add it as selection list param                               
                                    stepActivityVar = new VariableSelectionList();
                                    stepActivityVar.Name = param.Name;
                                    stepActivity.AddVariable(stepActivityVar);
                                    stepActivity.AutomationStatus = Activity.eActivityAutomationStatus.Development;//reset status because new flow control param was added
                                }
                                else
                                {
                                    //add as String param
                                    stepActivityVar = new VariableString();
                                    stepActivityVar.Name = param.Name;
                                    ((VariableString)stepActivityVar).InitialStringValue = param.Value;
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
                                        stepActivityVar.Name = param.Name;
                                        stepActivity.AddVariable(stepActivityVar);
                                        stepActivity.AutomationStatus = Activity.eActivityAutomationStatus.Development;//reset status because flow control param was added
                                    }
                                }
                                else if (isflowControlParam == false)
                                {
                                    if (stepActivityVar is VariableSelectionList)
                                    {
                                        //change it to be string variable
                                        stepActivity.Variables.Remove(stepActivityVar);
                                        stepActivityVar = new VariableString();
                                        stepActivityVar.Name = param.Name;
                                        ((VariableString)stepActivityVar).InitialStringValue = param.Value;
                                        stepActivity.AddVariable(stepActivityVar);
                                        stepActivity.AutomationStatus = Activity.eActivityAutomationStatus.Development;//reset status because flow control param was removed
                                    }
                                }
                            }

                            //add the variable selected value                          
                            if (stepActivityVar is VariableSelectionList)
                            {
                                OptionalValue stepActivityVarOptionalVar = ((VariableSelectionList)stepActivityVar).OptionalValuesList.Where(x => x.Value == param.Value).FirstOrDefault();
                                if (stepActivityVarOptionalVar == null)
                                {
                                    //no such variable value option so add it
                                    stepActivityVarOptionalVar = new OptionalValue(param.Value);
                                    ((VariableSelectionList)stepActivityVar).OptionalValuesList.Add(stepActivityVarOptionalVar);
                                    ((VariableSelectionList)stepActivityVar).SyncOptionalValuesListAndString();
                                    if (isflowControlParam == true)
                                        stepActivity.AutomationStatus = Activity.eActivityAutomationStatus.Development;//reset status because new param value was added
                                }
                                //set the selected value
                                ((VariableSelectionList)stepActivityVar).SelectedValue = stepActivityVarOptionalVar.Value;
                            }
                            else
                            {
                                //try just to set the value
                                try
                                {
                                    stepActivityVar.Value = param.Value;
                                    if (stepActivityVar is VariableString)
                                        ((VariableString)stepActivityVar).InitialStringValue = param.Value;
                                }
                                catch (Exception ex) { Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}"); }
                            }
                        }
                    }
                }
                return busFlow;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to import QC test set and convert it into " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), ex);
                return null;
            }
        }

        public static void UpdatedRQMTestInBF(ref BusinessFlow busFlow, RQMTestPlan testPlan, List<string> TCsIDs)
        {
            try
            {
                if ((testPlan == null) || (busFlow == null)) return;

                // removing activityGroup/activities that going to be updated from BusinessFlow
                var activitiesToRemove = busFlow.Activities.Where(x => TCsIDs.Select(y => { y = ExportToRQM.GetExportedIDString(y, "RQMScriptID"); return y; }).ToList()
                                                           .Contains(x.ExternalID.Split('|').First().Split('=').Last().Split('_').First())).ToList();

                int startGroupActsIndxInBf = 0;
                Dictionary<string, int> activityGroupsToRemoveIndexes = new Dictionary<string, int>();
                foreach (Activity activityToRemove in activitiesToRemove)
                {
                    if (startGroupActsIndxInBf < busFlow.Activities.IndexOf(activityToRemove))
                        startGroupActsIndxInBf = busFlow.Activities.IndexOf(activityToRemove);
                    busFlow.Activities.Remove(activityToRemove);
                }
                var activityGroupsToRemove = busFlow.ActivitiesGroups.Where(x => TCsIDs.Contains(x.ExternalID)).ToList();
                foreach (ActivitiesGroup activityGroupToRemove in activityGroupsToRemove)
                {
                    activityGroupsToRemoveIndexes.Add(activityGroupToRemove.ExternalID, busFlow.ActivitiesGroups.IndexOf(activityGroupToRemove));
                }
                foreach (ActivitiesGroup activityGroupToRemove in activityGroupsToRemove)
                {
                    busFlow.ActivitiesGroups.Remove(activityGroupToRemove);
                }

                int activityGroupToRemoveIndex;
                foreach (string tcToBeUpdatedID in TCsIDs)
                {
                    activityGroupsToRemoveIndexes.TryGetValue(tcToBeUpdatedID, out activityGroupToRemoveIndex);

                    foreach (RQMTestCase tc in testPlan.TestCases)
                    {
                        if (ExportToRQM.GetExportedIDString(tcToBeUpdatedID, "RQMID") == tc.RQMID)
                        {
                            //Add the TC steps as Activities if not already on the Activties group
                            RQMTestScript selectedScript = tc.TestScripts.Where(y => y.Name == tc.SelectedTestScriptName).ToList().FirstOrDefault();
                            if (selectedScript == null)
                            {
                                continue;
                            }

                            RQMExecutionRecord selectedExecutionRecord = testPlan.RQMExecutionRecords.Where(x => x.RelatedTestCaseRqmID == tc.RQMID && x.RelatedTestScriptRqmID == selectedScript.RQMID).FirstOrDefault();
                            string RQMRecordID = selectedExecutionRecord == null ? string.Empty : selectedExecutionRecord.RQMID.ToString();

                            //check if the TC is already exist in repository
                            ActivitiesGroup tcActivsGroup;
                            ActivitiesGroup repoActivsGroup = null;
                            if (repoActivsGroup == null)
                                repoActivsGroup = GingerActivitiesGroupsRepo.Where(x => x.ExternalID != null ? x.ExternalID.Split('|').First().Split('=').Last() == tc.RQMID : false).FirstOrDefault();
                            if (repoActivsGroup != null)
                            {
                                tcActivsGroup = (ActivitiesGroup)repoActivsGroup.CreateInstance();
                                busFlow.InsertActivitiesGroup(tcActivsGroup, activityGroupToRemoveIndex);
                                busFlow.ImportActivitiesGroupActivitiesFromRepository(tcActivsGroup, GingerActivitiesRepo, true, true);
                                busFlow.AttachActivitiesGroupsAndActivities();
                                continue;
                            }
                            else // TC not exist in Ginger repository so create new one
                            {
                                tcActivsGroup = new ActivitiesGroup();
                                tcActivsGroup.Name = tc.Name;
                                tcActivsGroup.ExternalID = "RQMID=" + tc.RQMID + "|RQMScriptID=" + selectedScript.RQMID + "|RQMRecordID=" + RQMRecordID + "|AtsID=" + tc.BTSID;
                                busFlow.InsertActivitiesGroup(tcActivsGroup, activityGroupToRemoveIndex);
                            }

                            // get BTS IDs if exists (ID per step)
                            Dictionary<string, string> strBtsIDs = new Dictionary<string, string>();
                            string[] stringSeparators = new string[] { "***" };
                            string[] results = selectedScript.BTSStepsIDs.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string result in results)
                            {
                                try
                                {
                                    strBtsIDs.Add(result.Split('=').First().TrimEnd().Split(' ').Last(), result.Split('=').Last().ToString());
                                }
                                catch { }
                            }
                            //

                            foreach (RQMStep step in selectedScript.Steps)
                            {
                                Activity stepActivity;
                                bool toAddStepActivity = false;

                                // check if mapped activity exist in repository
                                Activity repoStepActivity = GingerActivitiesRepo.Where(x => x.ExternalID != null ? x.ExternalID.Split('|').First().Split('=').Last() == step.RQMIndex : false).FirstOrDefault();
                                if (repoStepActivity != null)
                                {
                                    //check if it is part of the Activities Group
                                    ActivityIdentifiers groupStepActivityIdent = tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.RQMIndex).FirstOrDefault();
                                    if (groupStepActivityIdent != null)
                                    {
                                        //already in Activities Group so get link to it
                                        stepActivity = busFlow.Activities.Where(x => x.Guid == groupStepActivityIdent.ActivityGuid).FirstOrDefault();
                                    }
                                    else // not in ActivitiesGroup so get instance from repo
                                    {
                                        stepActivity = (Activity)repoStepActivity.CreateInstance();
                                        toAddStepActivity = true;
                                    }
                                }
                                else //Step not exist in Ginger repository so create new one
                                {
                                    stepActivity = new Activity
                                    {
                                        ActivityName = tc.Name + ">" + step.Name,
                                        Description = StripHTML(step.Description),
                                        Expected = StripHTML(step.ExpectedResult)
                                    };

                                    string currentStepATSId = string.Empty;
                                    if (strBtsIDs.TryGetValue((selectedScript.Steps.IndexOf(step) + 1).ToString(), out currentStepATSId))
                                    {
                                        stepActivity.ExternalID = "RQMID=" + step.RQMIndex + "|AtsID=" + currentStepATSId;
                                    }
                                    else
                                    {
                                        stepActivity.ExternalID = "RQMID=" + step.RQMIndex;
                                    }

                                    toAddStepActivity = true;
                                }

                                if (toAddStepActivity)
                                {
                                    // not in group- need to add it
                                    busFlow.InsertActivity(stepActivity, startGroupActsIndxInBf++);
                                    tcActivsGroup.AddActivityToGroup(stepActivity);
                                }

                                //pull TC-Step parameters and add them to the Activity level
                                foreach (RQMTestParameter param in selectedScript.Parameters)   // Params taken from TestScriptLevel only!!!! Also exists parapameters at TestCase, to check if them should be taken!!!
                                {                                                               
                                    bool? isflowControlParam = null;

                                    //detrmine if the param is Flow Control Param or not based on it value and agreed sign "$$_"
                                    if (param.Value.ToString().StartsWith("$$_"))
                                    {
                                        isflowControlParam = false;
                                        if (param.Value.ToString().StartsWith("$$_"))
                                            param.Value = param.Value.ToString().Substring(3); //get value without "$$_"
                                    }
                                    else if (param.Value.ToString() != "<Empty>")
                                        isflowControlParam = true;

                                    //check if already exist param with that name
                                    VariableBase stepActivityVar = stepActivity.Variables.Where(x => x.Name.ToUpper() == param.Name.ToUpper()).FirstOrDefault();
                                    if (stepActivityVar == null)
                                    {
                                        //#Param not exist so add it
                                        if (isflowControlParam == true)
                                        {
                                            //add it as selection list param                               
                                            stepActivityVar = new VariableSelectionList();
                                            stepActivityVar.Name = param.Name;
                                            stepActivity.AddVariable(stepActivityVar);
                                            stepActivity.AutomationStatus = Activity.eActivityAutomationStatus.Development;//reset status because new flow control param was added
                                        }
                                        else
                                        {
                                            //add as String param
                                            stepActivityVar = new VariableString();
                                            stepActivityVar.Name = param.Name;
                                            ((VariableString)stepActivityVar).InitialStringValue = param.Value;
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
                                                stepActivityVar = new VariableSelectionList {Name = param.Name};
                                                stepActivity.AddVariable(stepActivityVar);
                                                stepActivity.AutomationStatus = Activity.eActivityAutomationStatus.Development;//reset status because flow control param was added
                                            }
                                        }
                                        else if (isflowControlParam == false)
                                        {
                                            if (stepActivityVar is VariableSelectionList)
                                            {
                                                //change it to be string variable
                                                stepActivity.Variables.Remove(stepActivityVar);
                                                stepActivityVar = new VariableString {Name = param.Name};
                                                ((VariableString)stepActivityVar).InitialStringValue = param.Value;
                                                stepActivity.AddVariable(stepActivityVar);
                                                stepActivity.AutomationStatus = Activity.eActivityAutomationStatus.Development;//reset status because flow control param was removed
                                            }
                                        }
                                    }

                                    //add the variable selected value                          
                                    if (stepActivityVar is VariableSelectionList)
                                    {
                                        OptionalValue stepActivityVarOptionalVar = ((VariableSelectionList)stepActivityVar).OptionalValuesList.Where(x => x.Value == param.Value).FirstOrDefault();
                                        if (stepActivityVarOptionalVar == null)
                                        {
                                            //no such variable value option so add it
                                            stepActivityVarOptionalVar = new OptionalValue(param.Value);
                                            ((VariableSelectionList)stepActivityVar).OptionalValuesList.Add(stepActivityVarOptionalVar);
                                            ((VariableSelectionList)stepActivityVar).SyncOptionalValuesListAndString();
                                            if (isflowControlParam == true)
                                                stepActivity.AutomationStatus = Activity.eActivityAutomationStatus.Development;//reset status because new param value was added
                                        }
                                        //set the selected value
                                        ((VariableSelectionList)stepActivityVar).SelectedValue = stepActivityVarOptionalVar.Value;
                                    }
                                    else
                                    {
                                        //try just to set the value
                                        try
                                        {
                                            stepActivityVar.Value = param.Value;
                                            if (stepActivityVar is VariableString)
                                                ((VariableString)stepActivityVar).InitialStringValue = param.Value;
                                        }
                                        catch (Exception ex) { Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}"); }
                                    }
                                }
                            }
                        }
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to import QC test set and convert it into " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), ex);
                return;
            }
        }

        public static void UpdateBusinessFlow(ref BusinessFlow busFlow, RQMTestPlan testPlan)
        {
            try
            {
                if ((testPlan == null) || (busFlow == null)) return;

                int startGroupActsIndxInBf = 0;
                busFlow.Activities.Clear();
                busFlow.ActivitiesGroups.Clear();

                foreach (RQMTestCase tc in testPlan.TestCases)
                {

                    //Add the TC steps as Activities if not already on the Activties group
                    RQMTestScript selectedScript = tc.TestScripts.Where(y => y.Name == tc.SelectedTestScriptName).ToList().FirstOrDefault();
                    if (selectedScript == null)
                    {
                        continue;
                    }

                    RQMExecutionRecord selectedExecutionRecord = testPlan.RQMExecutionRecords.Where(x => x.RelatedTestCaseRqmID == tc.RQMID && x.RelatedTestScriptRqmID == selectedScript.RQMID).FirstOrDefault();
                    string RQMRecordID = selectedExecutionRecord == null ? string.Empty : selectedExecutionRecord.RQMID.ToString();

                    //check if the TC is already exist in repository
                    ActivitiesGroup tcActivsGroup;
                    ActivitiesGroup repoActivsGroup = null;
                    if (repoActivsGroup == null)
                        repoActivsGroup = GingerActivitiesGroupsRepo.Where(x => x.ExternalID != null ? x.ExternalID.Split('|').First().Split('=').Last() == tc.RQMID : false).FirstOrDefault();
                    if (repoActivsGroup != null)
                    {
                        tcActivsGroup = (ActivitiesGroup)repoActivsGroup.CreateInstance();
                        busFlow.AddActivitiesGroup(tcActivsGroup);
                        busFlow.ImportActivitiesGroupActivitiesFromRepository(tcActivsGroup, GingerActivitiesRepo, true, true);
                        busFlow.AttachActivitiesGroupsAndActivities();
                        continue;
                    }
                    else // TC not exist in Ginger repository so create new one
                    {
                        tcActivsGroup = new ActivitiesGroup();
                        tcActivsGroup.Name = tc.Name;
                        tcActivsGroup.ExternalID = "RQMID=" + tc.RQMID + "|RQMScriptID=" + selectedScript.RQMID + "|RQMRecordID=" + RQMRecordID + "|AtsID=" + tc.BTSID;
                        busFlow.AddActivitiesGroup(tcActivsGroup);
                    }

                    // get BTS IDs if exists (ID per step)
                    Dictionary<string, string> strBtsIDs = new Dictionary<string, string>();
                    string[] stringSeparators = new string[] { "***" };
                    string[] results = selectedScript.BTSStepsIDs.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string result in results)
                    {
                        try
                        {
                            strBtsIDs.Add(result.Split('=').First().TrimEnd().Split(' ').Last(), result.Split('=').Last().ToString());
                        }
                        catch { }
                    }
                    //

                    foreach (RQMStep step in selectedScript.Steps)
                    {
                        Activity stepActivity;
                        bool toAddStepActivity = false;

                        // check if mapped activity exist in repository
                        Activity repoStepActivity = GingerActivitiesRepo.Where(x => x.ExternalID != null ? x.ExternalID.Split('|').First().Split('=').Last() == step.RQMIndex : false).FirstOrDefault();
                        if (repoStepActivity != null)
                        {
                            //check if it is part of the Activities Group
                            ActivityIdentifiers groupStepActivityIdent = tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.RQMIndex).FirstOrDefault();
                            if (groupStepActivityIdent != null)
                            {
                                //already in Activities Group so get link to it
                                stepActivity = busFlow.Activities.Where(x => x.Guid == groupStepActivityIdent.ActivityGuid).FirstOrDefault();
                            }
                            else // not in ActivitiesGroup so get instance from repo
                            {
                                stepActivity = (Activity)repoStepActivity.CreateInstance();
                                toAddStepActivity = true;
                            }
                        }
                        else //Step not exist in Ginger repository so create new one
                        {
                            stepActivity = new Activity();
                            stepActivity.ActivityName = tc.Name + ">" + step.Name;
                            stepActivity.Description = StripHTML(step.Description);
                            stepActivity.Expected = StripHTML(step.ExpectedResult);

                            string currentStepATSId = string.Empty;
                            if (strBtsIDs.TryGetValue((selectedScript.Steps.IndexOf(step) + 1).ToString(), out currentStepATSId))
                            {
                                stepActivity.ExternalID = "RQMID=" + step.RQMIndex + "|AtsID=" + currentStepATSId;
                            }
                            else
                            {
                                stepActivity.ExternalID = "RQMID=" + step.RQMIndex;
                            }

                            toAddStepActivity = true;
                        }

                        if (toAddStepActivity)
                        {
                            // not in group- need to add it
                            busFlow.InsertActivity(stepActivity, startGroupActsIndxInBf++);
                            tcActivsGroup.AddActivityToGroup(stepActivity);
                        }

                        //pull TC-Step parameters and add them to the Activity level
                        foreach (RQMTestParameter param in selectedScript.Parameters)   // Params taken from TestScriptLevel only!!!! Also exists parapameters at TestCase, to check if them should be taken!!!
                        {                                                               
                            bool? isflowControlParam = null;

                            //detrmine if the param is Flow Control Param or not based on it value and agreed sign "$$_"
                            if (param.Value.ToString().StartsWith("$$_"))
                            {
                                isflowControlParam = false;
                                if (param.Value.ToString().StartsWith("$$_"))
                                    param.Value = param.Value.ToString().Substring(3); //get value without "$$_"
                            }
                            else if (param.Value.ToString() != "<Empty>")
                                isflowControlParam = true;

                            //check if already exist param with that name
                            VariableBase stepActivityVar = stepActivity.Variables.Where(x => x.Name.ToUpper() == param.Name.ToUpper()).FirstOrDefault();
                            if (stepActivityVar == null)
                            {
                                //#Param not exist so add it
                                if (isflowControlParam == true)
                                {
                                    //add it as selection list param                               
                                    stepActivityVar = new VariableSelectionList();
                                    stepActivityVar.Name = param.Name;
                                    stepActivity.AddVariable(stepActivityVar);
                                    stepActivity.AutomationStatus = Activity.eActivityAutomationStatus.Development;//reset status because new flow control param was added
                                }
                                else
                                {
                                    //add as String param
                                    stepActivityVar = new VariableString();
                                    stepActivityVar.Name = param.Name;
                                    ((VariableString)stepActivityVar).InitialStringValue = param.Value;
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
                                        stepActivityVar.Name = param.Name;
                                        stepActivity.AddVariable(stepActivityVar);
                                        stepActivity.AutomationStatus = Activity.eActivityAutomationStatus.Development;//reset status because flow control param was added
                                    }
                                }
                                else if (isflowControlParam == false)
                                {
                                    if (stepActivityVar is VariableSelectionList)
                                    {
                                        //change it to be string variable
                                        stepActivity.Variables.Remove(stepActivityVar);
                                        stepActivityVar = new VariableString();
                                        stepActivityVar.Name = param.Name;
                                        ((VariableString)stepActivityVar).InitialStringValue = param.Value;
                                        stepActivity.AddVariable(stepActivityVar);
                                        stepActivity.AutomationStatus = Activity.eActivityAutomationStatus.Development;//reset status because flow control param was removed
                                    }
                                }
                            }

                            //add the variable selected value                          
                            if (stepActivityVar is VariableSelectionList)
                            {
                                OptionalValue stepActivityVarOptionalVar = ((VariableSelectionList)stepActivityVar).OptionalValuesList.Where(x => x.Value == param.Value).FirstOrDefault();
                                if (stepActivityVarOptionalVar == null)
                                {
                                    //no such variable value option so add it
                                    stepActivityVarOptionalVar = new OptionalValue(param.Value);
                                    ((VariableSelectionList)stepActivityVar).OptionalValuesList.Add(stepActivityVarOptionalVar);
                                    ((VariableSelectionList)stepActivityVar).SyncOptionalValuesListAndString();
                                    if (isflowControlParam == true)
                                        stepActivity.AutomationStatus = Activity.eActivityAutomationStatus.Development;//reset status because new param value was added
                                }
                                //set the selected value
                                ((VariableSelectionList)stepActivityVar).SelectedValue = stepActivityVarOptionalVar.Value;
                            }
                            else
                            {
                                //try just to set the value
                                try
                                {
                                    stepActivityVar.Value = param.Value;
                                    if (stepActivityVar is VariableString)
                                        ((VariableString)stepActivityVar).InitialStringValue = param.Value;
                                }
                                catch (Exception ex) { Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}"); }
                            }
                        }
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Failed to import QC test set and convert it into " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), ex);
                return;
            }
        }

        private static void GetStepParameters(string stepText, ref List<string> stepParamsList)
        {
            try
            {
                MatchCollection stepParams = Regex.Matches(stepText, @"\<<<([^>]*)\>>>");

                foreach (var param in stepParams)
                {
                    string strParam = param.ToString().TrimStart(new char[] { '<' });
                    strParam = strParam.TrimEnd(new char[] { '>' });
                    stepParamsList.Add(strParam);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error occurred while pulling the parameters names from QC TC Step Description/Expected", ex);
            }
        }

        public static string StripHTML(string HTMLText, bool toDecodeHTML = true)
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
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error occured while stripping the HTML from QC TC Step Description/Expected", ex);
                return HTMLText;
            }
        }

        public static ObservableList<ExternalItemFieldBase> GetALMItemFields(BackgroundWorker bw, bool online)
        {
            if (online)
                return GetOnlineItemFields(bw);
            else return GetLocalSavedPossibleValues();
        }

        private static ObservableList<ExternalItemFieldBase> GetLocalSavedPossibleValues()
        {
            ObservableList<ExternalItemFieldBase> ItemFieldsPossibleValues = new ObservableList<ExternalItemFieldBase>();
            try
            {
                ObservableList<JsonExternalItemField> JsonItemFieldsPossibleValues = new ObservableList<JsonExternalItemField>();
                string jsonItemsFieldsFile = System.IO.Path.Combine(RQMCore.ConfigPackageFolderPath, "RQM_Fields", "ExternalItemsFields.json");
                if (!File.Exists(jsonItemsFieldsFile))
                {
                    Reporter.ToLog(eAppReporterLogLevel.INFO, "ALM RQM, Restoring External Items Fields from ExternalItemsFields.json, file hasn't been found at: " + jsonItemsFieldsFile);
                    return ItemFieldsPossibleValues;
                }

                string strItemsFields = System.IO.File.ReadAllText(jsonItemsFieldsFile);
                JsonItemFieldsPossibleValues = JsonConvert.DeserializeObject<ObservableList<JsonExternalItemField>>(strItemsFields);


                foreach (JsonExternalItemField jsonItemField in JsonItemFieldsPossibleValues)
                {
                    ExternalItemFieldBase itemField = new ExternalItemFieldBase();
                    itemField.ID = jsonItemField.ID;
                    itemField.Name = jsonItemField.Name;
                    itemField.ItemType = jsonItemField.ItemType;
                    itemField.Mandatory = jsonItemField.Mandatory;
                    itemField.PossibleValues = jsonItemField.PossibleValues;
                    itemField.ToUpdate = jsonItemField.ToUpdate;
                    itemField.SelectedValue = jsonItemField.Selected;

                    if (jsonItemField.PossibleValues.Count > 0)
                        itemField.SelectedValue = jsonItemField.PossibleValues[0];
                    else
                        itemField.SelectedValue = "Unassigned";

                    ItemFieldsPossibleValues.Add(itemField);
                }
            }
            catch (Exception e) { Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}"); }

            return ItemFieldsPossibleValues;
        }

        private static void SaveItemFields(ObservableList<ExternalItemFieldBase> refreshedFields)
        {
            ObservableList<JsonExternalItemField> externalItemsListForJson = new ObservableList<JsonExternalItemField>();
            foreach (ExternalItemFieldBase field in refreshedFields)
            {
                JsonExternalItemField JEIF = new JsonExternalItemField();

                JEIF.ID = field.ID;
                JEIF.Name = field.Name;
                JEIF.ItemType = field.ItemType;
                JEIF.Mandatory = field.Mandatory;
                JEIF.PossibleValues = field.PossibleValues;
                JEIF.Selected = field.SelectedValue;
                JEIF.ToUpdate = field.ToUpdate;

                externalItemsListForJson.Add(JEIF);
            }
            string jsonString = JsonConvert.SerializeObject(externalItemsListForJson);
            System.IO.File.WriteAllText(Path.Combine(RQMCore.ConfigPackageFolderPath, "RQM_Fields", "ExternalItemsFields.json"), jsonString);
        }

        public static ObservableList<ExternalItemFieldBase> GetOnlineItemFields(BackgroundWorker bw)
        {
            ObservableList<ExternalItemFieldBase> fields = new ObservableList<ExternalItemFieldBase>();

            //TODO : receive as parameters:

            RqmRepository rqmRep = new RqmRepository(RQMCore.ConfigPackageFolderPath);
            List<IProjectDefinitions> rqmProjectsDataList;
            //string rqmSserverUrl = loginData.Server.ToString() + "/";
            string rqmSserverUrl = ALMCore.AlmConfig.ALMServerURL + "/";
            LoginDTO loginData = new LoginDTO() { User = ALMCore.AlmConfig.ALMUserName, Password = ALMCore.AlmConfig.ALMPassword, Server = ALMCore.AlmConfig.ALMServerURL };
            IProjectData rqmProjectsData = rqmRep.GetVisibleProjects(loginData);
            rqmProjectsDataList = rqmProjectsData.IProjectDefinitions;
            IProjectDefinitions currentProj = rqmProjectsDataList.FirstOrDefault();
            string rqmDomain = currentProj.Prefix;
            string rqmProject = currentProj.ProjectName;

            //------------------------------- Improved solution

            string baseUri_ = string.Empty;
            string selfLink_ = string.Empty;
            int maxPageNumber_ = 0;
            int totalCategoryTypeCount = 0;


            string categoryValue = string.Empty;  // --> itemfield.PossibleValues.Add(ccNode.Name);
            string categoryTypeID = string.Empty; //--> itemfield.ID
            try
            {
                //TODO: Populate list fields with CategoryTypes
                populatedValue = "Starting fields retrieve process... ";
                bw.ReportProgress(totalValues, populatedValue);
                RqmResponseData categoryType = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(rqmSserverUrl + rqmDomain + "/service/com.ibm.rqm.integration.service.IIntegrationService/resources/" + rqmProject + "/categoryType"));
                XmlDocument categoryTypeList = new XmlDocument();


                if (!string.IsNullOrEmpty(categoryType.responseText))
                {
                    categoryTypeList.LoadXml(categoryType.responseText);
                }

                //TODO: Get 'next' and 'last links
                XmlNodeList linkList_ = categoryTypeList.GetElementsByTagName("link");

                if (linkList_.Count > 0)
                {
                    XmlNode selfPage = linkList_.Item(1);
                    XmlNode lastPage_ = linkList_.Item(3);

                    if (selfPage.Attributes["rel"].Value.ToString() == "self") //verify self link is present
                    {
                        selfLink_ = selfPage.Attributes["href"].Value.ToString();
                        //baseUri_ = selfLink_.Substring(0, selfLink_.Length - 1);
                        baseUri_ = selfLink_;
                    }

                    if (lastPage_.Attributes["rel"].Value.ToString() == "last") //verify there is more than one page
                    {
                        if (selfPage.Attributes["rel"].Value.ToString() == "self") //verify self link is present
                        {
                            selfLink_ = selfPage.Attributes["href"].Value.ToString();
                            baseUri_ = selfLink_.Substring(0, selfLink_.Length - 1);
                        }

                        string tempString_ = lastPage_.Attributes["href"].Value.ToString();
                        maxPageNumber_ = System.Convert.ToInt32(tempString_.Substring(tempString_.LastIndexOf('=') + 1));
                    }
                    string newUri_ = string.Empty;
                    List<string> categoryTypeUriPages = new List<string>();
                    ConcurrentBag<ExternalItemFieldBase> catTypeRsult = new ConcurrentBag<ExternalItemFieldBase>();

                    for (int k = 0; k <= maxPageNumber_; k++)
                    {
                        if (maxPageNumber_ > 0)
                        {
                            newUri_ = baseUri_ + k.ToString();
                            categoryTypeUriPages.Add(newUri_);
                        }
                        else
                        {
                            newUri_ = baseUri_;
                            categoryTypeUriPages.Add(newUri_);
                        }
                    }

                    //Parallel computing solution
                    List<XmlNode> entryList = new List<XmlNode>();
                    if (categoryTypeUriPages.Count > 1)
                    {
                        Parallel.ForEach(categoryTypeUriPages.AsParallel(), categoryTypeUri =>
                        {

                            //System.Diagnostics.Debug.WriteLine("parallel foreach #1");
                            newUri_ = categoryTypeUri;
                            //categoryType = rqmRep.GetRqmResponse(loginData, new Uri(newUri_));
                            categoryType = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(newUri_));
                            if (!string.IsNullOrEmpty(categoryType.responseText))
                            {
                                categoryTypeList.LoadXml(categoryType.responseText);
                            }

                            //TODO: Get all ID links under entry:
                            XmlNodeList categoryTypeEntry_ = categoryTypeList.GetElementsByTagName("entry");

                            foreach (XmlNode entryNode in categoryTypeEntry_)
                            {
                                entryList.Add(entryNode);
                            }

                            ParallelLoopResult innerResult = Parallel.ForEach(entryList.AsParallel(), singleEntry =>
                            {

                                XmlNodeList innerNodes = singleEntry.ChildNodes;
                                XmlNode linkNode = innerNodes.Item(4);
                                ExternalItemFieldBase itemfield = new ExternalItemFieldBase();

                                string getIDlink = string.Empty;
                                getIDlink = linkNode.Attributes["href"].Value.ToString(); // retrived CategoryType link


                                RqmResponseData categoryTypeDetail = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(getIDlink));

                                XmlDocument categoryTypeListing = new XmlDocument();
                                if (!string.IsNullOrEmpty(categoryTypeDetail.responseText))
                                {
                                    categoryTypeListing.LoadXml(categoryTypeDetail.responseText);
                                }


                                string categoryTypeName = string.Empty; // -->itemfield.Name
                                string categoryTypeItemType = string.Empty; //-->itemfield.ItemType
                                string categoryTypeMandatory = string.Empty; // --> itemfield.Mandatory & initial value for : --> itemfield.ToUpdate

                                string typeIdentifier = categoryTypeListing.GetElementsByTagName("ns3:identifier").Item(0).InnerText;
                                categoryTypeID = typeIdentifier.Substring(typeIdentifier.LastIndexOf(':') + 1); 
                                categoryTypeName = categoryTypeListing.GetElementsByTagName("ns3:title").Item(0).InnerText;
                                categoryTypeItemType = categoryTypeListing.GetElementsByTagName("ns2:scope").Item(0).InnerText;
                                categoryTypeMandatory = categoryTypeListing.GetElementsByTagName("ns2:required").Item(0).InnerText;


                                itemfield.ItemType = categoryTypeItemType;
                                itemfield.ID = categoryTypeID;
                                itemfield.Name = categoryTypeName;
                                if (itemfield.SelectedValue == null)
                                {
                                    itemfield.SelectedValue = "Unassigned";
                                }

                                if (categoryTypeMandatory == "true")
                                {
                                    itemfield.ToUpdate = true;
                                    itemfield.Mandatory = true;
                                }
                                else
                                {
                                    itemfield.ToUpdate = false;
                                    itemfield.Mandatory = false;
                                }

                                catTypeRsult.Add(itemfield);
                                populatedValue = "Populating field :" + categoryTypeName + " \r\nNumber of fields populated :" + catTypeRsult.Count;
                                bw.ReportProgress(catTypeRsult.Count, populatedValue);

                            });
                        });
                    }
                    else
                    {
                        populatedValue = string.Empty;
                        newUri_ = baseUri_;
                        categoryType = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(newUri_));

                        if (!string.IsNullOrEmpty(categoryType.responseText))
                        {
                            categoryTypeList.LoadXml(categoryType.responseText);
                        }

                        //TODO: Get all ID links under entry:
                        XmlNodeList categoryTypeEntry_ = categoryTypeList.GetElementsByTagName("entry");

                        foreach (XmlNode entryNode in categoryTypeEntry_)
                        {
                            entryList.Add(entryNode);
                        }

                        ParallelLoopResult innerResult = Parallel.ForEach(entryList.AsParallel(), singleEntry =>
                        {

                            XmlNodeList innerNodes = singleEntry.ChildNodes;
                            XmlNode linkNode = innerNodes.Item(4);
                            ExternalItemFieldBase itemfield = new ExternalItemFieldBase();

                            string getIDlink = string.Empty;
                            getIDlink = linkNode.Attributes["href"].Value.ToString(); // retrived CategoryType link

                            RqmResponseData categoryTypeDetail = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(getIDlink));

                            XmlDocument categoryTypeListing = new XmlDocument();

                            if (!string.IsNullOrEmpty(categoryTypeDetail.responseText))
                            {
                                categoryTypeListing.LoadXml(categoryTypeDetail.responseText);
                            }

                            string categoryTypeName = string.Empty; // -->itemfield.Name
                            string categoryTypeItemType = string.Empty; //-->itemfield.ItemType
                            string categoryTypeMandatory = string.Empty; // --> itemfield.Mandatory & initial value for : --> itemfield.ToUpdate

                            string typeIdentifier = categoryTypeListing.GetElementsByTagName("ns3:identifier").Item(0).InnerText;
                            categoryTypeID = typeIdentifier.Substring(typeIdentifier.LastIndexOf(':') + 1); 
                            categoryTypeName = categoryTypeListing.GetElementsByTagName("ns3:title").Item(0).InnerText;
                            categoryTypeItemType = categoryTypeListing.GetElementsByTagName("ns2:scope").Item(0).InnerText;
                            categoryTypeMandatory = categoryTypeListing.GetElementsByTagName("ns2:required").Item(0).InnerText;

                            itemfield.ItemType = categoryTypeItemType;
                            itemfield.ID = categoryTypeID;
                            itemfield.Name = categoryTypeName;
                            if (itemfield.SelectedValue == null)
                            {
                                itemfield.SelectedValue = "Unassigned";
                            }

                            if (categoryTypeMandatory == "true")
                            {
                                itemfield.ToUpdate = true;
                                itemfield.Mandatory = true;
                            }
                            else
                            {
                                itemfield.ToUpdate = false;
                                itemfield.Mandatory = false;
                            }

                            catTypeRsult.Add(itemfield);
                            populatedValue = "Populating field :" + categoryTypeName + " \r\n Number of fields populated :" + catTypeRsult.Count;
                            bw.ReportProgress(catTypeRsult.Count, populatedValue);
                        });
                    }

                    foreach (ExternalItemFieldBase field in catTypeRsult)
                    {
                        fields.Add(field);
                        totalCategoryTypeCount++;
                        System.Diagnostics.Debug.WriteLine("Number of retrieved fields:" + totalCategoryTypeCount);
                    }

                    //TODO: Add Values to CategoryTypes Parallel
                    populatedValue = "Starting values retrieve process... ";
                    bw.ReportProgress(totalValues, populatedValue);

                    RqmResponseData category = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(rqmSserverUrl + rqmDomain + "/service/com.ibm.rqm.integration.service.IIntegrationService/resources/" + rqmProject + "/category"));
                    XmlDocument CategoryList = new XmlDocument();
                    CategoryList.LoadXml(category.responseText);
                    totalValues = 0;
                    populatedValue = string.Empty;

                    //TODO: Get 'next' and 'last links
                    XmlNodeList linkList = CategoryList.GetElementsByTagName("link");
                    XmlNode selfPageNode = linkList.Item(1);
                    XmlNode lastPageNode = linkList.Item(3);

                    string selfLink = selfPageNode.Attributes["href"].Value.ToString();
                    string baseUri = selfLink.Substring(0, selfLink.Length - 1);

                    string tempString = lastPageNode.Attributes["href"].Value.ToString();
                    int maxPageNumber = System.Convert.ToInt32(tempString.Substring(tempString.LastIndexOf('=') + 1));
                    string newUri = string.Empty;
                    List<string> categoryUriPages = new List<string>();

                    for (int i = 0; i <= maxPageNumber; i++) //scale testing
                    {
                        if (maxPageNumber > 0)
                        {
                            newUri = baseUri + i.ToString();
                            categoryUriPages.Add(newUri);
                        }
                        else
                        {
                            newUri = baseUri;
                            categoryUriPages.Add(newUri);

                        }
                    }

                    //Improved with Parallel GetRQMData
                    if (categoryUriPages.Count > 0)
                    {
                        int iDCount = 0;

                        List<Uri> uriList = new List<Uri>();
                        foreach (string pageUri in categoryUriPages)
                        {
                            Uri listURIEntry = new Uri(pageUri);
                            uriList.Add(listURIEntry);
                        }

                        //Get all Pages of values:
                        populatedValue = "Retrieving value pages... ";
                        List<RqmResponseData> XmlPageList = RQMConnect.Instance.RQMRep.GetRqmDataParallel(loginData, uriList);

                        //For each category page
                        foreach (RqmResponseData category_ in XmlPageList)
                        {
                            XmlDocument CategoryList_ = new XmlDocument();
                            if (!string.IsNullOrEmpty(category_.responseText))
                            {
                                CategoryList_.LoadXml(category_.responseText);
                            }

                            XmlNodeList categoryIDs = CategoryList_.GetElementsByTagName("id");
                            
                            iDCount += categoryIDs.Count;

                            //Make a list of Category ID links (uri's)
                            if (categoryIDs.Count > 0)
                            {
                                List<Uri> idLinkList = new List<Uri>();
                                for (int n = 1; n < categoryIDs.Count; n++)
                                {
                                    Uri idLink = new Uri(categoryIDs.Item(n).InnerText);
                                    //idLinkList.Add(categoryIDs.Item(n).InnerText);
                                    idLinkList.Add(idLink);
                                }

                                //Retrieves Category XML Pages in parallel per Page
                                List<RqmResponseData> CategoryIDLink = RQMConnect.Instance.RQMRep.GetRqmDataParallel(loginData, idLinkList);

                                ExternalItemFieldBase valuesItemfield = new ExternalItemFieldBase();

                                //get all category and their values -- shold be changed to ForeachParallel for faster performance:
                                populatedValue = "Populating values... ";
                                foreach (RqmResponseData LinkData in CategoryIDLink)
                                //Parallel.ForEach(CategoryIDLink.AsParallel(), singleLink =>
                                {
                                    if (!string.IsNullOrEmpty(LinkData.responseText))
                                    {
                                        XmlDocument categoryValueXML = new XmlDocument();

                                        categoryValueXML.LoadXml(LinkData.responseText);

                                        XmlNode categoryTypeNode;
                                        string catTypeLink = string.Empty;


                                        if (!string.IsNullOrEmpty(categoryValueXML.InnerText.ToString()))
                                        {
                                            categoryTypeNode = categoryValueXML.GetElementsByTagName("ns2:categoryType").Item(0); //need to consider changes in tag i.e. ns3/ns4...
                                            catTypeLink = categoryTypeNode.Attributes["href"].Value.ToString();

                                            categoryTypeID = catTypeLink.Substring(catTypeLink.LastIndexOf(':') + 1); 
                                            categoryValue = categoryValueXML.GetElementsByTagName("ns3:title").Item(0).InnerText;  // --> itemfield.PossibleValues.Add(ccNode.Name);

                                            valuesItemfield.ID = categoryTypeID;
                                            
                                            if (fields.Count > 0) //category list has at least 1 entry
                                            {
                                                for (int j = 0; j < fields.Count; j++) //run through list
                                                {
                                                    if ((fields[j].ID.ToString() == categoryTypeID))
                                                    {
                                                        fields[j].PossibleValues.Add(categoryValue);
                                                        fields[j].SelectedValue = fields[j].PossibleValues[0];
                                                    }
                                                }
                                            }

                                            totalValues++;
                                            
                                            System.Diagnostics.Debug.WriteLine("Total number of populated values is :" + totalValues + "/" + iDCount * (categoryUriPages.Count + 1)); //TODO pass this to a string to print in the UI
                                                                                                                                                                                      //bw.ReportProgress(totalValues);
                                            populatedValue = "Populating value:" + categoryValue + " \r\n Total Values:" + totalValues;
                                            bw.ReportProgress(totalValues, populatedValue);
                                        }
                                    }
                                } //simple foreach closing                                                   
                            }
                        }
                    }
                }
            }
            catch (Exception e) { Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}"); }

            SaveItemFields(fields);
            return fields;
        }

        public static XmlNodeList Readxmlfile(string fieldType, string solutionFolder)
        {
            XmlNodeList xlist = null;
            XmlDocument doc = new XmlDocument();
            if (System.IO.File.Exists(System.IO.Path.Combine(solutionFolder, @"Documents\ALM\RQM_Configs\FieldMapping.xml")))
            {
                doc.Load(System.IO.Path.Combine(solutionFolder, @"Documents\ALM\RQM_Configs\FieldMapping.xml"));

                if (fieldType == "TestPlan")
                {
                    xlist = doc.SelectNodes("//TestPlan/*");

                }
                if (fieldType == "TestCase")
                {
                    xlist = doc.SelectNodes("//TestCase/*");
                }
                if (fieldType == "TestScript")
                {
                    xlist = doc.SelectNodes("//TestScript/*");
                }
            }
            else
            {
                //TODO : build FieldMapping.xml
            }
            
            return xlist;
        }
    }
}
