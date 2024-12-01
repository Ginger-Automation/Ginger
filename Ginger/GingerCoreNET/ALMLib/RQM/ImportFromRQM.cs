#region License
/*
Copyright © 2014-2024 European Support Limited

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

//#region License
///*
//Copyright © 2014-2024 European Support Limited

//Licensed under the Apache License, Version 2.0 (the "License")
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at 

//http://www.apache.org/licenses/LICENSE-2.0 

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS, 
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and 
//limitations under the License. 
//*/
//#endregion

using ALM_CommonStd.Abstractions;
using ALM_CommonStd.DataContracts;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Activities;
using GingerCore.External;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Newtonsoft.Json;
using RQM_RepositoryStd;
using RQM_RepositoryStd.Data_Contracts;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Linq;

namespace GingerCore.ALM.RQM
{
    public enum eRQMItemType { TestPlan, TestCase, TestScript }

    public static class ImportFromRQM
    {
        public static ObservableList<ActivitiesGroup> GingerActivitiesGroupsRepo { get; set; }
        public static ObservableList<Activity> GingerActivitiesRepo { get; set; }
        public static ObservableList<ApplicationPlatform> ApplicationPlatforms { get; set; }

        public static int totalValues = 0;
        public static string populatedValue = string.Empty;

        public static BusinessFlow ConvertRQMTestPlanToBF(RQMTestPlan testPlan)
        {
            System.Diagnostics.Trace.WriteLine("in ConvertRQMTestPlanToBF :");
            try
            {
                if (testPlan == null)
                {
                    return null;
                }

                //Create Business Flow
                BusinessFlow busFlow = new BusinessFlow
                {
                    Name = testPlan.Name,
                    ExternalID = $"RQMID={testPlan.RQMID}",
                    Status = BusinessFlow.eBusinessFlowStatus.Development,
                    Activities = [],
                    Variables = []
                };

                //Create Activities Group + Activities for each TC
                foreach (RQMTestCase tc in testPlan.TestCases)
                {
                    //Add the TC steps as Activities if not already on the Activities group
                    RQMTestScript selectedScript = tc.TestScripts.Where(y => y.Name == tc.SelectedTestScriptName).ToList().FirstOrDefault();
                    if (selectedScript == null)
                    {
                        continue;
                    }

                    RQMExecutionRecord selectedExecutionRecord = testPlan.RQMExecutionRecords.FirstOrDefault(x => x.RelatedTestCaseRqmID == tc.RQMID && x.RelatedTestScriptRqmID == selectedScript.RQMID);
                    string RQMRecordID = selectedExecutionRecord == null ? string.Empty : selectedExecutionRecord.RQMID.ToString();

                    //check if the TC is already exist in repository
                    ActivitiesGroup tcActivsGroup;
                    ActivitiesGroup repoActivsGroup = null;
                    if (repoActivsGroup == null)
                    {
                        repoActivsGroup = GingerActivitiesGroupsRepo.FirstOrDefault(x => x.ExternalID != null && x.ExternalID.Split('|').First().Split('=').Last() == tc.RQMID);
                    }

                    if (repoActivsGroup != null)
                    {
                        tcActivsGroup = (ActivitiesGroup)repoActivsGroup.CreateInstance(true);
                        tcActivsGroup.ExternalID = tcActivsGroup.ExternalID.Replace($"RQMRecordID={ExportToRQM.GetExportedIDString(tcActivsGroup.ExternalID, "RQMRecordID")}", "RQMRecordID=");
                        busFlow.AddActivitiesGroup(tcActivsGroup);
                        busFlow.ImportActivitiesGroupActivitiesFromRepository(tcActivsGroup, GingerActivitiesRepo, ApplicationPlatforms, true);
                        busFlow.AttachActivitiesGroupsAndActivities();
                        continue;
                    }
                    else // TC not exist in Ginger repository so create new one
                    {
                        tcActivsGroup = new ActivitiesGroup
                        {
                            Name = tc.Name,
                            ExternalID = $"RQMID={tc.RQMID}|RQMScriptID={selectedScript.RQMID}|RQMRecordID={RQMRecordID}|AtsID={tc.BTSID}",
                            TestSuiteId = tc.TestSuiteId,
                            TestSuiteTitle = tc.TestSuiteTitle
                        };
                        busFlow.AddActivitiesGroup(tcActivsGroup);
                    }


                    // get BTS IDs if exists (ID per step)
                    Dictionary<string, string> strBtsIDs = [];
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
                        Activity repoStepActivity = GingerActivitiesRepo.FirstOrDefault(x => x.ExternalID != null && x.ExternalID.Split('|').First().Split('=').Last() == step.RQMIndex);
                        if (repoStepActivity != null)
                        {
                            //check if it is part of the Activities Group
                            //ActivityIdentifiers groupStepActivityIdent = tcActivsGroup.ActivitiesIdentifiers.Where(x => x.ActivityExternalID == step.RQMIndex).FirstOrDefault();
                            ActivityIdentifiers groupStepActivityIdent = tcActivsGroup.ActivitiesIdentifiers.FirstOrDefault(x => x.ActivityExternalID != null && x.ActivityExternalID.Split('|').First().Split('=').Last() == step.RQMIndex);
                            if (groupStepActivityIdent != null)
                            {
                                //already in Activities Group so get link to it
                                stepActivity = busFlow.Activities.FirstOrDefault(x => x.Guid == groupStepActivityIdent.ActivityGuid);
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
                                ActivityName = tc.Name + ">" + step.RQMIndex.Split('_')[1],
                                Description = StripHTML(step.Description),
                                Expected = StripHTML(step.ExpectedResult)
                            };

                            string currentStepATSId = string.Empty;
                            if (strBtsIDs.TryGetValue((selectedScript.Steps.IndexOf(step) + 1).ToString(), out currentStepATSId))
                            {
                                stepActivity.ExternalID = $"RQMID={step.RQMIndex}|AtsID={currentStepATSId}";
                            }
                            else
                            {
                                stepActivity.ExternalID = $"RQMID={step.RQMIndex}";
                            }

                            toAddStepActivity = true;
                        }

                        if (toAddStepActivity)
                        {
                            // not in group- need to add it
                            busFlow.AddActivity(stepActivity, tcActivsGroup);
                        }

                        //pull TC-Step parameters and add them to the Activity level
                        foreach (RQMTestParameter param in selectedScript.Parameters)   // Params taken from TestScriptLevel only!!!! Also exists parameters at TestCase, to check if them should be taken!!!
                        {
                            bool? isflowControlParam = null;

                            //determine if the param is Flow Control Param or not based on it value and agreed sign "$$_"
                            if (param.Value.ToString().StartsWith("$$_"))
                            {
                                isflowControlParam = false;
                                if (param.Value.ToString().StartsWith("$$_"))
                                {
                                    param.Value = param.Value.ToString()[3..]; //get value without "$$_"
                                }
                            }
                            else if (param.Value.ToString() != "<Empty>")
                            {
                                isflowControlParam = true;
                            }

                            //check if already exist param with that name
                            VariableBase stepActivityVar = stepActivity.Variables.FirstOrDefault(x => x.Name.ToUpper() == param.Name.ToUpper());
                            if (stepActivityVar == null)
                            {
                                //#Param not exist so add it
                                if (isflowControlParam == true)
                                {
                                    //add it as selection list param                               
                                    stepActivityVar = new VariableSelectionList
                                    {
                                        Name = param.Name
                                    };
                                    stepActivity.AddVariable(stepActivityVar);
                                    stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because new flow control param was added
                                }
                                else
                                {
                                    //add as String param
                                    stepActivityVar = new VariableString
                                    {
                                        Name = param.Name
                                    };
                                    ((VariableString)stepActivityVar).InitialStringValue = param.Value;
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
                                            Name = param.Name
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
                                            Name = param.Name
                                        };
                                        ((VariableString)stepActivityVar).InitialStringValue = param.Value;
                                        stepActivity.AddVariable(stepActivityVar);
                                        stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because flow control param was removed
                                    }
                                }
                            }

                            //add the variable selected value                          
                            if (stepActivityVar is VariableSelectionList)
                            {
                                OptionalValue stepActivityVarOptionalVar = ((VariableSelectionList)stepActivityVar).OptionalValuesList.FirstOrDefault(x => x.Value == param.Value);
                                if (stepActivityVarOptionalVar == null)
                                {
                                    //no such variable value option so add it
                                    stepActivityVarOptionalVar = new OptionalValue(param.Value);
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
                                    stepActivityVar.Value = param.Value;
                                    if (stepActivityVar is VariableString)
                                    {
                                        ((VariableString)stepActivityVar).InitialStringValue = param.Value;
                                    }
                                }
                                catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
                            }
                        }
                    }
                }
                return busFlow;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to import QC test set and convert it into {GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)}", ex);
                return null;
            }
        }

        public static void UpdatedRQMTestInBF(ref BusinessFlow busFlow, RQMTestPlan testPlan, List<string> TCsIDs)
        {
            try
            {
                if ((testPlan == null) || (busFlow == null))
                {
                    return;
                }

                // removing activityGroup/activities that going to be updated from BusinessFlow
                var activitiesToRemove = busFlow.Activities.Where(x => TCsIDs.Select(y => { y = ExportToRQM.GetExportedIDString(y, "RQMScriptID"); return y; }).ToList()
                                                           .Contains(x.ExternalID.Split('|').First().Split('=').Last().Split('_').First())).ToList();

                int startGroupActsIndxInBf = 0;
                Dictionary<string, int> activityGroupsToRemoveIndexes = [];
                foreach (Activity activityToRemove in activitiesToRemove)
                {
                    if (startGroupActsIndxInBf < busFlow.Activities.IndexOf(activityToRemove))
                    {
                        startGroupActsIndxInBf = busFlow.Activities.IndexOf(activityToRemove);
                    }

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
                            //Add the TC steps as Activities if not already on the Activities group
                            RQMTestScript selectedScript = tc.TestScripts.Where(y => y.Name == tc.SelectedTestScriptName).ToList().FirstOrDefault();
                            if (selectedScript == null)
                            {
                                continue;
                            }

                            RQMExecutionRecord selectedExecutionRecord = testPlan.RQMExecutionRecords.FirstOrDefault(x => x.RelatedTestCaseRqmID == tc.RQMID && x.RelatedTestScriptRqmID == selectedScript.RQMID);
                            string RQMRecordID = selectedExecutionRecord == null ? string.Empty : selectedExecutionRecord.RQMID.ToString();

                            //check if the TC is already exist in repository
                            ActivitiesGroup tcActivsGroup;
                            ActivitiesGroup repoActivsGroup = null;
                            if (repoActivsGroup == null)
                            {
                                repoActivsGroup = GingerActivitiesGroupsRepo.FirstOrDefault(x => x.ExternalID != null && x.ExternalID.Split('|').First().Split('=').Last() == tc.RQMID);
                            }

                            if (repoActivsGroup != null)
                            {
                                tcActivsGroup = (ActivitiesGroup)repoActivsGroup.CreateInstance();
                                busFlow.AddActivitiesGroup(tcActivsGroup, activityGroupToRemoveIndex);
                                busFlow.ImportActivitiesGroupActivitiesFromRepository(tcActivsGroup, GingerActivitiesRepo, ApplicationPlatforms, true);
                                busFlow.AttachActivitiesGroupsAndActivities();
                                continue;
                            }
                            else // TC not exist in Ginger repository so create new one
                            {
                                tcActivsGroup = new ActivitiesGroup
                                {
                                    Name = tc.Name,
                                    ExternalID = $"RQMID={tc.RQMID}|RQMScriptID={selectedScript.RQMID}|RQMRecordID={RQMRecordID}|AtsID={tc.BTSID}"
                                };
                                busFlow.AddActivitiesGroup(tcActivsGroup, activityGroupToRemoveIndex);
                            }

                            // get BTS IDs if exists (ID per step)
                            Dictionary<string, string> strBtsIDs = [];
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
                                Activity repoStepActivity = GingerActivitiesRepo.FirstOrDefault(x => x.ExternalID != null && x.ExternalID.Split('|').First().Split('=').Last() == step.RQMIndex);
                                if (repoStepActivity != null)
                                {
                                    //check if it is part of the Activities Group
                                    ActivityIdentifiers groupStepActivityIdent = tcActivsGroup.ActivitiesIdentifiers.FirstOrDefault(x => x.ActivityExternalID == step.RQMIndex);
                                    if (groupStepActivityIdent != null)
                                    {
                                        //already in Activities Group so get link to it
                                        stepActivity = busFlow.Activities.FirstOrDefault(x => x.Guid == groupStepActivityIdent.ActivityGuid);
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
                                        stepActivity.ExternalID = $"RQMID={step.RQMIndex}|AtsID={currentStepATSId}";
                                    }
                                    else
                                    {
                                        stepActivity.ExternalID = $"RQMID={step.RQMIndex}";
                                    }

                                    toAddStepActivity = true;
                                }

                                if (toAddStepActivity)
                                {
                                    // not in group- need to add it
                                    busFlow.AddActivity(stepActivity, tcActivsGroup, startGroupActsIndxInBf++);
                                }

                                //pull TC-Step parameters and add them to the Activity level
                                foreach (RQMTestParameter param in selectedScript.Parameters)   // Params taken from TestScriptLevel only!!!! Also exists parameters at TestCase, to check if them should be taken!!!
                                {
                                    bool? isflowControlParam = null;

                                    //determine if the param is Flow Control Param or not based on it value and agreed sign "$$_"
                                    if (param.Value.ToString().StartsWith("$$_"))
                                    {
                                        isflowControlParam = false;
                                        if (param.Value.ToString().StartsWith("$$_"))
                                        {
                                            param.Value = param.Value.ToString()[3..]; //get value without "$$_"
                                        }
                                    }
                                    else if (param.Value.ToString() != "<Empty>")
                                    {
                                        isflowControlParam = true;
                                    }

                                    //check if already exist param with that name
                                    VariableBase stepActivityVar = stepActivity.Variables.FirstOrDefault(x => x.Name.ToUpper() == param.Name.ToUpper());
                                    if (stepActivityVar == null)
                                    {
                                        //#Param not exist so add it
                                        if (isflowControlParam == true)
                                        {
                                            //add it as selection list param                               
                                            stepActivityVar = new VariableSelectionList
                                            {
                                                Name = param.Name
                                            };
                                            stepActivity.AddVariable(stepActivityVar);
                                            stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because new flow control param was added
                                        }
                                        else
                                        {
                                            //add as String param
                                            stepActivityVar = new VariableString
                                            {
                                                Name = param.Name
                                            };
                                            ((VariableString)stepActivityVar).InitialStringValue = param.Value;
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
                                                stepActivityVar = new VariableSelectionList { Name = param.Name };
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
                                                stepActivityVar = new VariableString { Name = param.Name };
                                                ((VariableString)stepActivityVar).InitialStringValue = param.Value;
                                                stepActivity.AddVariable(stepActivityVar);
                                                stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because flow control param was removed
                                            }
                                        }
                                    }

                                    //add the variable selected value                          
                                    if (stepActivityVar is VariableSelectionList)
                                    {
                                        OptionalValue stepActivityVarOptionalVar = ((VariableSelectionList)stepActivityVar).OptionalValuesList.FirstOrDefault(x => x.Value == param.Value);
                                        if (stepActivityVarOptionalVar == null)
                                        {
                                            //no such variable value option so add it
                                            stepActivityVarOptionalVar = new OptionalValue(param.Value);
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
                                            stepActivityVar.Value = param.Value;
                                            if (stepActivityVar is VariableString)
                                            {
                                                ((VariableString)stepActivityVar).InitialStringValue = param.Value;
                                            }
                                        }
                                        catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
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
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to import QC test set and convert it into {GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)}", ex);
                return;
            }
        }

        public static void UpdateBusinessFlow(ref BusinessFlow busFlow, RQMTestPlan testPlan)
        {
            try
            {
                if ((testPlan == null) || (busFlow == null))
                {
                    return;
                }

                int startGroupActsIndxInBf = 0;
                busFlow.Activities.Clear();
                busFlow.ActivitiesGroups.Clear();

                foreach (RQMTestCase tc in testPlan.TestCases)
                {

                    //Add the TC steps as Activities if not already on the Activities group
                    RQMTestScript selectedScript = tc.TestScripts.Where(y => y.Name == tc.SelectedTestScriptName).ToList().FirstOrDefault();
                    if (selectedScript == null)
                    {
                        continue;
                    }

                    RQMExecutionRecord selectedExecutionRecord = testPlan.RQMExecutionRecords.FirstOrDefault(x => x.RelatedTestCaseRqmID == tc.RQMID && x.RelatedTestScriptRqmID == selectedScript.RQMID);
                    string RQMRecordID = selectedExecutionRecord == null ? string.Empty : selectedExecutionRecord.RQMID.ToString();

                    //check if the TC is already exist in repository
                    ActivitiesGroup tcActivsGroup;
                    ActivitiesGroup repoActivsGroup = null;
                    if (repoActivsGroup == null)
                    {
                        repoActivsGroup = GingerActivitiesGroupsRepo.FirstOrDefault(x => x.ExternalID != null && x.ExternalID.Split('|').First().Split('=').Last() == tc.RQMID);
                    }

                    if (repoActivsGroup != null)
                    {
                        tcActivsGroup = (ActivitiesGroup)repoActivsGroup.CreateInstance();
                        busFlow.AddActivitiesGroup(tcActivsGroup);
                        busFlow.ImportActivitiesGroupActivitiesFromRepository(tcActivsGroup, GingerActivitiesRepo, ApplicationPlatforms, true);
                        busFlow.AttachActivitiesGroupsAndActivities();
                        continue;
                    }
                    else // TC not exist in Ginger repository so create new one
                    {
                        tcActivsGroup = new ActivitiesGroup
                        {
                            Name = tc.Name,
                            ExternalID = $"RQMID={tc.RQMID}|RQMScriptID={selectedScript.RQMID}|RQMRecordID={RQMRecordID}|AtsID={tc.BTSID}"
                        };
                        busFlow.AddActivitiesGroup(tcActivsGroup);
                    }

                    // get BTS IDs if exists (ID per step)
                    Dictionary<string, string> strBtsIDs = [];
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
                        Activity repoStepActivity = GingerActivitiesRepo.FirstOrDefault(x => x.ExternalID != null && x.ExternalID.Split('|').First().Split('=').Last() == step.RQMIndex);
                        if (repoStepActivity != null)
                        {
                            //check if it is part of the Activities Group
                            ActivityIdentifiers groupStepActivityIdent = tcActivsGroup.ActivitiesIdentifiers.FirstOrDefault(x => x.ActivityExternalID == step.RQMIndex);
                            if (groupStepActivityIdent != null)
                            {
                                //already in Activities Group so get link to it
                                stepActivity = busFlow.Activities.FirstOrDefault(x => x.Guid == groupStepActivityIdent.ActivityGuid);
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
                                stepActivity.ExternalID = $"RQMID={step.RQMIndex}|AtsID={currentStepATSId}";
                            }
                            else
                            {
                                stepActivity.ExternalID = $"RQMID={step.RQMIndex}";
                            }

                            toAddStepActivity = true;
                        }

                        if (toAddStepActivity)
                        {
                            // not in group- need to add it
                            busFlow.AddActivity(stepActivity, tcActivsGroup, startGroupActsIndxInBf++);
                        }

                        //pull TC-Step parameters and add them to the Activity level
                        foreach (RQMTestParameter param in selectedScript.Parameters)   // Params taken from TestScriptLevel only!!!! Also exists parameters at TestCase, to check if them should be taken!!!
                        {
                            bool? isflowControlParam = null;

                            //determine if the param is Flow Control Param or not based on it value and agreed sign "$$_"
                            if (param.Value.ToString().StartsWith("$$_"))
                            {
                                isflowControlParam = false;
                                if (param.Value.ToString().StartsWith("$$_"))
                                {
                                    param.Value = param.Value.ToString()[3..]; //get value without "$$_"
                                }
                            }
                            else if (param.Value.ToString() != "<Empty>")
                            {
                                isflowControlParam = true;
                            }

                            //check if already exist param with that name
                            VariableBase stepActivityVar = stepActivity.Variables.FirstOrDefault(x => x.Name.ToUpper() == param.Name.ToUpper());
                            if (stepActivityVar == null)
                            {
                                //#Param not exist so add it
                                if (isflowControlParam == true)
                                {
                                    //add it as selection list param                               
                                    stepActivityVar = new VariableSelectionList
                                    {
                                        Name = param.Name
                                    };
                                    stepActivity.AddVariable(stepActivityVar);
                                    stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because new flow control param was added
                                }
                                else
                                {
                                    //add as String param
                                    stepActivityVar = new VariableString
                                    {
                                        Name = param.Name
                                    };
                                    ((VariableString)stepActivityVar).InitialStringValue = param.Value;
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
                                            Name = param.Name
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
                                            Name = param.Name
                                        };
                                        ((VariableString)stepActivityVar).InitialStringValue = param.Value;
                                        stepActivity.AddVariable(stepActivityVar);
                                        stepActivity.AutomationStatus = eActivityAutomationStatus.Development;//reset status because flow control param was removed
                                    }
                                }
                            }

                            //add the variable selected value                          
                            if (stepActivityVar is VariableSelectionList)
                            {
                                OptionalValue stepActivityVarOptionalVar = ((VariableSelectionList)stepActivityVar).OptionalValuesList.FirstOrDefault(x => x.Value == param.Value);
                                if (stepActivityVarOptionalVar == null)
                                {
                                    //no such variable value option so add it
                                    stepActivityVarOptionalVar = new OptionalValue(param.Value);
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
                                    stepActivityVar.Value = param.Value;
                                    if (stepActivityVar is VariableString)
                                    {
                                        ((VariableString)stepActivityVar).InitialStringValue = param.Value;
                                    }
                                }
                                catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex); }
                            }
                        }
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Failed to import QC test set and convert it into {GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)}", ex);
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
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while pulling the parameters names from QC TC Step Description/Expected", ex);
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
                {
                    stripped = HttpUtility.HtmlDecode(stripped);
                }

                stripped = stripped.TrimStart(new char[] { '\r', '\n' });
                stripped = stripped.TrimEnd(new char[] { '\r', '\n' });

                return stripped;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while stripping the HTML from QC TC Step Description/Expected", ex);
                return HTMLText;
            }
        }

        public static ObservableList<ExternalItemFieldBase> GetALMItemFields(BackgroundWorker bw, bool online)
        {
            if (online)
            {
                return GetOnlineItemFields(bw);
            }
            else
            {
                return GetLocalSavedPossibleValues();
            }
        }

        public static ObservableList<ExternalItemFieldBase> GetALMItemFieldsForDefect(BackgroundWorker bw, bool online)
        {
            if (online)
            {

                return GetOnlineItemFieldsForDefect(bw);


            }
            else
            {
                return GetLocalSavedPossibleValues();
            }
        }
        private static ObservableList<ExternalItemFieldBase> GetLocalSavedPossibleValues()
        {
            ObservableList<ExternalItemFieldBase> ItemFieldsPossibleValues = [];
            try
            {
                ObservableList<JsonExternalItemField> JsonItemFieldsPossibleValues = [];
                string jsonItemsFieldsFile = System.IO.Path.Combine(RQMCore.ConfigPackageFolderPath, "RQM_Fields", "ExternalItemsFields.json");
                if (!File.Exists(jsonItemsFieldsFile))
                {
                    Reporter.ToLog(eLogLevel.DEBUG, $"ALM RQM, Restoring External Items Fields from ExternalItemsFields.json, file hasn't been found at: {jsonItemsFieldsFile}");
                    return ItemFieldsPossibleValues;
                }

                string strItemsFields = System.IO.File.ReadAllText(jsonItemsFieldsFile);
                JsonItemFieldsPossibleValues = JsonConvert.DeserializeObject<ObservableList<JsonExternalItemField>>(strItemsFields);


                foreach (JsonExternalItemField jsonItemField in JsonItemFieldsPossibleValues)
                {
                    ExternalItemFieldBase itemField = new ExternalItemFieldBase
                    {
                        ID = jsonItemField.ID,
                        Name = jsonItemField.Name,
                        ItemType = jsonItemField.ItemType,
                        Mandatory = jsonItemField.Mandatory,
                        PossibleValues = jsonItemField.PossibleValues,
                        ToUpdate = jsonItemField.ToUpdate,
                        SelectedValue = jsonItemField.Selected
                    };

                    if (jsonItemField.PossibleValues.Count > 0)
                    {
                        itemField.SelectedValue = jsonItemField.PossibleValues[0];
                    }
                    else
                    {
                        itemField.SelectedValue = "Unassigned";
                    }
                    Reporter.ToLog(eLogLevel.DEBUG, "Item : " + Newtonsoft.Json.JsonConvert.SerializeObject(itemField));
                    ItemFieldsPossibleValues.Add(itemField);
                }
            }
            catch (Exception e) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e); }

            return ItemFieldsPossibleValues;
        }

        private static void SaveItemFields(ObservableList<ExternalItemFieldBase> refreshedFields)
        {
            ObservableList<JsonExternalItemField> externalItemsListForJson = [];
            foreach (ExternalItemFieldBase field in refreshedFields)
            {
                JsonExternalItemField JEIF = new JsonExternalItemField
                {
                    ID = field.ID,
                    Name = field.Name,
                    ItemType = field.ItemType,
                    Mandatory = field.Mandatory,
                    PossibleValues = field.PossibleValues,
                    Selected = field.SelectedValue,
                    ToUpdate = field.ToUpdate,
                    IsCustomField = field.IsCustomField,
                };

                externalItemsListForJson.Add(JEIF);
            }
            string jsonString = JsonConvert.SerializeObject(externalItemsListForJson);
            System.IO.File.WriteAllText(Path.Combine(RQMCore.ConfigPackageFolderPath, "RQM_Fields", "ExternalItemsFields.json"), jsonString);
        }

        public static ObservableList<ExternalItemFieldBase> GetOnlineItemFields(BackgroundWorker bw)
        {

            ObservableList<ExternalItemFieldBase> fields = [];
            fields = GetOnlineFields(bw);

            SaveItemFields(fields);
            return fields;
        }

        public static ObservableList<ExternalItemFieldBase> GetOnlineFields(BackgroundWorker bw)
        {
            ObservableList<ExternalItemFieldBase> fields = [];
            GetItemFieldsAll(bw, fields);
            return fields;
        }

        private static void GetItemFieldsAll(BackgroundWorker bw, ObservableList<ExternalItemFieldBase> fields)
        {
            try
            {
                //TODO : receive as parameters:

                RqmRepository rqmRep = new RqmRepository(RQMCore.ConfigPackageFolderPath);
                List<IProjectDefinitions> rqmProjectsDataList;
                string rqmSserverUrl = ALMCore.DefaultAlmConfig.ALMServerURL + "/";
                LoginDTO loginData = new LoginDTO() { User = ALMCore.DefaultAlmConfig.ALMUserName, Password = ALMCore.DefaultAlmConfig.ALMPassword, Server = ALMCore.DefaultAlmConfig.ALMServerURL };

                //------------------------------- Improved solution

                string baseUri_ = string.Empty;
                string selfLink_ = string.Empty;
                int maxPageNumber_ = 0;


                string categoryValue = string.Empty;  // --> itemfield.PossibleValues.Add(ccNode.Name);
                string categoryTypeID = string.Empty; //--> itemfield.ID

                //TODO: Populate list fields with CategoryTypes
                Reporter.ToLog(eLogLevel.DEBUG, $"Starting fields retrieve process... ");
                PopulateLogOnFieldMappingwinodw(bw, "Starting fields retrieve process... ");
                RqmResponseData categoryType = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(rqmSserverUrl + RQMCore.ALMProjectGroupName + "/service/com.ibm.rqm.integration.service.IIntegrationService/resources/" + ALMCore.DefaultAlmConfig.ALMProjectGUID + "/categoryType"));
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
                        baseUri_ = selfLink_;
                    }

                    if (lastPage_.Attributes["rel"].Value.ToString() == "last") //verify there is more than one page
                    {
                        if (selfPage.Attributes["rel"].Value.ToString() == "self") //verify self link is present
                        {
                            selfLink_ = selfPage.Attributes["href"].Value.ToString();
                            baseUri_ = selfLink_[..^1];
                        }

                        string tempString_ = lastPage_.Attributes["href"].Value.ToString();
                        maxPageNumber_ = System.Convert.ToInt32(tempString_[(tempString_.LastIndexOf('=') + 1)..]);
                    }
                    string newUri_ = string.Empty;
                    List<string> categoryTypeUriPages = [];
                    ConcurrentBag<ExternalItemFieldBase> catTypeRsult = [];

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
                    List<XmlNode> entryList = [];
                    if (categoryTypeUriPages.Count > 1)
                    {
                        Parallel.ForEach(categoryTypeUriPages.AsParallel(), new ParallelOptions { MaxDegreeOfParallelism = 5 }, categoryTypeUri =>
                        {
                            newUri_ = categoryTypeUri;
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
                            ParallelLoopResult innerResult = Parallel.ForEach(entryList.AsParallel(), new ParallelOptions { MaxDegreeOfParallelism = 5 }, singleEntry =>
                            {

                                XmlNodeList innerNodes = singleEntry.ChildNodes;
                                XmlNode linkNode = innerNodes.Item(4);
                                ExternalItemFieldBase itemfield = new ExternalItemFieldBase();

                                string getIDlink = string.Empty;
                                getIDlink = linkNode.Attributes["href"].Value.ToString(); // retrived CategoryType link


                                RqmResponseData categoryTypeDetail = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(getIDlink));

                                XmlDocument categoryTypeListing = new();
                                if (!string.IsNullOrEmpty(categoryTypeDetail.responseText))
                                {
                                    categoryTypeListing.LoadXml(categoryTypeDetail.responseText);
                                }


                                string categoryTypeName = string.Empty; // -->itemfield.Name
                                string categoryTypeItemType = string.Empty; //-->itemfield.ItemType
                                string categoryTypeMandatory = string.Empty; // --> itemfield.Mandatory & initial value for : --> itemfield.ToUpdate

                                string typeIdentifier = categoryTypeListing.GetElementsByTagName("ns4:identifier").Item(0).InnerText;
                                categoryTypeID = typeIdentifier[(typeIdentifier.LastIndexOf(':') + 1)..];
                                categoryTypeName = categoryTypeListing.GetElementsByTagName("ns4:title").Item(0).InnerText;
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
                                itemfield.ProjectGuid = ALMCore.DefaultAlmConfig.ALMProjectGUID;
                                catTypeRsult.Add(itemfield);
                                PopulateLogOnFieldMappingwinodw(bw, $"Populating field :{categoryTypeName} \r\nNumber of fields populated :{catTypeRsult.Count}");

                            }
                            );
                        }
                        );
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
                        ParallelLoopResult innerResult = Parallel.ForEach(entryList.AsParallel(), new ParallelOptions { MaxDegreeOfParallelism = 5 }, singleEntry =>
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

                            string typeIdentifier = categoryTypeListing.GetElementsByTagName("ns4:identifier").Item(0).InnerText;
                            categoryTypeID = typeIdentifier[(typeIdentifier.LastIndexOf(':') + 1)..];
                            categoryTypeName = categoryTypeListing.GetElementsByTagName("ns4:title").Item(0).InnerText;
                            categoryTypeItemType = categoryTypeListing.GetElementsByTagName("ns2:scope").Item(0).InnerText;
                            categoryTypeMandatory = categoryTypeListing.GetElementsByTagName("ns2:required").Item(0).InnerText;

                            itemfield.ItemType = categoryTypeItemType;
                            itemfield.ID = categoryTypeID;
                            itemfield.TypeIdentifier = typeIdentifier;
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
                            itemfield.ProjectGuid = ALMCore.DefaultAlmConfig.ALMProjectGUID;
                            catTypeRsult.Add(itemfield);
                            PopulateLogOnFieldMappingwinodw(bw, $"Populating field :{categoryTypeName} \r\nNumber of fields populated :{catTypeRsult.Count}");
                        }
                        );
                    }
                    foreach (ExternalItemFieldBase field in catTypeRsult)
                    {
                        fields.Add(field);
                    }//TODO: Add Values to CategoryTypes Parallel
                    PopulateLogOnFieldMappingwinodw(bw, $"Starting values retrieve process... ");
                    #region new Gat Values by filed Category Type
                    foreach (ExternalItemFieldBase field in fields)
                    {
                        string baseUrl = $"{rqmSserverUrl}{RQMCore.ALMProjectGroupName}/service/com.ibm.rqm.integration.service.IIntegrationService/resources/{ALMCore.DefaultAlmConfig.ALMProjectGUID}/category/?fields=feed/entry/content/category/";


                        // Construct URL
                        string fullUrl = $"{baseUrl}(categoryType[@href='{field.TypeIdentifier}']|*))";
                        Reporter.ToLog(eLogLevel.DEBUG, $"fullUrl : {fullUrl}");
                        RqmResponseData categoryfieldlist = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData,
                        new Uri(fullUrl));

                        XDocument doc = XDocument.Parse(categoryfieldlist.responseText);
                        XNamespace ns = "http://www.w3.org/2005/Atom";

                        // Query the XML to get all titles inside entry nodes
                        var titles = doc.Descendants(ns + "entry")
                                        .Select(entry => entry.Element(ns + "title")?.Value)
                                        .Where(title => title != null);

                        PopulateLogOnFieldMappingwinodw(bw, $"Number of values populated :{titles.Count()}");
                        if (bw != null)
                        {
                            bw.ReportProgress(catTypeRsult.Count, populatedValue);
                        }

                        if (titles != null && titles.Any())
                        {
                            foreach (var title in titles)
                            {
                                field.PossibleValues.Add(title);
                            }

                            // Set the first item as SelectedValue if PossibleValues is not empty
                            if (field.PossibleValues.Count > 0)
                            {
                                field.SelectedValue = field.PossibleValues[0];
                            }
                        }
                    }
                    #endregion

                }


                //step 1. Get Custom attribute list by API
                //step 2. Get all the custom attribute link as per the response
                //setp 3. Get all Custom Attribute details by API 
                //step 4. Get All custom Attribute Possible values
                ObservableList<ExternalItemFieldBase> Customfields;
                Customfields = GetCustomAttributes(bw, rqmSserverUrl, loginData, ref baseUri_, ref selfLink_, ref maxPageNumber_);
                foreach (var CustomfieldItem in Customfields)
                {
                    fields.Add(CustomfieldItem);
                }
            }
            catch (Exception e) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e); }
        }

        private static ObservableList<ExternalItemFieldBase> GetCustomAttributes(BackgroundWorker bw, string rqmSserverUrl, LoginDTO loginData, ref string baseUri_, ref string selfLink_, ref int maxPageNumber_)
        {
            ObservableList<ExternalItemFieldBase> fields = new ObservableList<ExternalItemFieldBase>();
            try
            {
                Reporter.ToLog(eLogLevel.DEBUG, $"starting Custom attribute fields retrieve process...");
                PopulateLogOnFieldMappingwinodw(bw, "starting Custom attribute fields retrieve process...");
                RqmResponseData CustomAttribute = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(rqmSserverUrl + RQMCore.ALMProjectGroupName + "/service/com.ibm.rqm.integration.service.IIntegrationService/resources/" + ALMCore.DefaultAlmConfig.ALMProjectGUID + "/customAttribute"));
                XmlDocument CustomAttributeList = new XmlDocument();
                if (!string.IsNullOrEmpty(CustomAttribute.responseText))
                {

                    CustomAttributeList.LoadXml(CustomAttribute.responseText);
                }
                //TODO: Get 'next' and 'last links
                XmlNodeList CustomAttributelinkList_ = CustomAttributeList.GetElementsByTagName("link");
                if (CustomAttributelinkList_.Count > 0)
                {
                    XmlNode selfPage = CustomAttributelinkList_.Item(1);
                    XmlNode lastPage_ = CustomAttributelinkList_.Item(3);

                    if (selfPage.Attributes["rel"].Value.ToString() == "self") //verify self link is present
                    {
                        selfLink_ = selfPage.Attributes["href"].Value.ToString();
                        baseUri_ = selfLink_;
                    }

                    if (lastPage_.Attributes["rel"].Value.ToString() == "last") //verify there is more than one page
                    {
                        if (selfPage.Attributes["rel"].Value.ToString() == "self") //verify self link is present
                        {
                            selfLink_ = selfPage.Attributes["href"].Value.ToString();
                            baseUri_ = selfLink_[..^1];
                        }

                        string tempString_ = lastPage_.Attributes["href"].Value.ToString();
                        maxPageNumber_ = System.Convert.ToInt32(tempString_[(tempString_.LastIndexOf('=') + 1)..]);
                    }
                    string newUri_ = string.Empty;
                    List<string> CustomAttributeUriPages = [];
                    ConcurrentBag<ExternalItemFieldBase> CustomAttributeRsult = [];

                    for (int k = 0; k <= maxPageNumber_; k++)
                    {
                        if (maxPageNumber_ > 0)
                        {
                            newUri_ = baseUri_ + k.ToString();
                            CustomAttributeUriPages.Add(newUri_);
                        }
                        else
                        {
                            newUri_ = baseUri_;
                            CustomAttributeUriPages.Add(newUri_);
                        }
                    }

                    //Parallel computing solution CustomAttribute
                    List<XmlNode> entryList = [];
                    if (CustomAttributeUriPages.Count > 1)
                    {
                        Parallel.ForEach(CustomAttributeUriPages.AsParallel(), new ParallelOptions { MaxDegreeOfParallelism = 5 }, CustomAttributeUri =>
                        {
                            newUri_ = CustomAttributeUri;
                            CustomAttribute = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(newUri_));
                            if (!string.IsNullOrEmpty(CustomAttribute.responseText))
                            {
                                CustomAttributeList.LoadXml(CustomAttribute.responseText);
                            }
                            //TODO: Get all ID links under entry:
                            XmlNodeList CustomAttributeEntry_ = CustomAttributeList.GetElementsByTagName("entry");

                            foreach (XmlNode entryNode in CustomAttributeEntry_)
                            {
                                entryList.Add(entryNode);
                            }
                            ParallelLoopResult innerResult = Parallel.ForEach(entryList.AsParallel(), new ParallelOptions { MaxDegreeOfParallelism = 5 }, singleEntry =>
                            {

                                XmlNodeList innerNodes = singleEntry.ChildNodes;
                                XmlNode linkNode = innerNodes.Item(4);
                                ExternalItemFieldBase itemfield = new ExternalItemFieldBase();

                                string getIDlink = string.Empty;
                                getIDlink = linkNode.Attributes["href"].Value.ToString(); // retrived CategoryType link


                                RqmResponseData CustomAttributeDetail = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(getIDlink));

                                XmlDocument CustomAttributeListing = new();
                                if (!string.IsNullOrEmpty(CustomAttributeDetail.responseText))
                                {
                                    CustomAttributeListing.LoadXml(CustomAttributeDetail.responseText);
                                }
                                string CustomAttributeName = string.Empty; // -->itemfield.Name
                                string CustomAttributeItemType = string.Empty; //-->itemfield.ItemType
                                string CustomAttributeMandatory = string.Empty; // --> itemfield.Mandatory & initial value for : --> itemfield.ToUpdate
                                string CustomAttributeID = string.Empty;

                                string typeIdentifier = CustomAttributeListing.GetElementsByTagName("ns4:identifier").Item(0).InnerText;
                                CustomAttributeID = typeIdentifier[(typeIdentifier.LastIndexOf(':') + 1)..];
                                CustomAttributeName = CustomAttributeListing.GetElementsByTagName("ns4:title").Item(0).InnerText;
                                CustomAttributeItemType = CustomAttributeListing.GetElementsByTagName("ns2:scope").Item(0).InnerText;
                                string CustomAttributefieldType = CustomAttributeListing.GetElementsByTagName("ns2:type").Item(0).InnerText;
                                // Define the namespace manager for the XML document
                                XmlNamespaceManager nsManager = new XmlNamespaceManager(CustomAttributeListing.NameTable);
                                nsManager.AddNamespace("ns2", "http://jazz.net/xmlns/alm/qm/v0.1/");

                                // XPath query to find ns2:required
                                string xpath = "//ns2:required";

                                // Use SelectSingleNode to check if ns2:required element exists
                                XmlNode requiredNode = CustomAttributeListing.SelectSingleNode(xpath, nsManager);

                                if (requiredNode != null)
                                {
                                    CustomAttributeMandatory = requiredNode.InnerText;
                                }
                                else
                                {
                                    CustomAttributeMandatory = "false";
                                }



                                itemfield.ItemType = CustomAttributeItemType;
                                itemfield.ID = CustomAttributeID;
                                itemfield.Name = CustomAttributeName;
                                itemfield.Type = CustomAttributefieldType;
                                itemfield.TypeIdentifier = typeIdentifier;
                                if (CustomAttributeMandatory.Equals("true", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    itemfield.ToUpdate = true;
                                    itemfield.Mandatory = true;
                                    if (itemfield.Type.Equals("INTEGER", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        itemfield.SelectedValue = "1";
                                    }
                                    else if (itemfield.Type.Equals("MEDIUMSTRING", StringComparison.CurrentCultureIgnoreCase) || itemfield.Type.Equals("SMALLSTRING", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        itemfield.SelectedValue = "dummy";
                                    }
                                    else if (itemfield.Type.Equals("TIMESTAMP", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        itemfield.SelectedValue = DateTime.Now.ToString("yyyy-MM-dd");
                                    }
                                    Reporter.ToLog(eLogLevel.INFO, $" CustomAttributeMandatory {CustomAttributeMandatory} itemfield.Name {itemfield.Name} itemfield.Type {itemfield.Type} itemfield..SelectedValue {itemfield.SelectedValue}");
                                }
                                else
                                {
                                    itemfield.ToUpdate = false;
                                    itemfield.Mandatory = false;
                                }

                                if (itemfield.SelectedValue == null)
                                {
                                    itemfield.SelectedValue = "Unassigned";
                                }

                                if (CustomAttributeMandatory == "true")
                                {
                                    itemfield.ToUpdate = true;
                                    itemfield.Mandatory = true;
                                }
                                else
                                {
                                    itemfield.ToUpdate = false;
                                    itemfield.Mandatory = false;
                                }
                                itemfield.ProjectGuid = ALMCore.DefaultAlmConfig.ALMProjectGUID;
                                CustomAttributeRsult.Add(itemfield);
                                PopulateLogOnFieldMappingwinodw(bw, $"Populating field :{CustomAttributeName} \r\nNumber of fields populated :{CustomAttributeRsult.Count}");

                            }
                            );
                        }
                        );
                    }
                    else
                    {
                        populatedValue = string.Empty;
                        newUri_ = baseUri_;
                        CustomAttribute = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(newUri_));

                        if (!string.IsNullOrEmpty(CustomAttribute.responseText))
                        {
                            CustomAttributeList.LoadXml(CustomAttribute.responseText);
                        }

                        //TODO: Get all ID links under entry:
                        XmlNodeList CustomAttributeEntry_ = CustomAttributeList.GetElementsByTagName("entry");

                        foreach (XmlNode entryNode in CustomAttributeEntry_)
                        {
                            entryList.Add(entryNode);
                        }
                        ParallelLoopResult innerResult = Parallel.ForEach(entryList.AsParallel(), new ParallelOptions { MaxDegreeOfParallelism = 5 }, singleEntry =>
                        {
                            XmlNodeList innerNodes = singleEntry.ChildNodes;
                            XmlNode linkNode = innerNodes.Item(4);
                            ExternalItemFieldBase itemfield = new ExternalItemFieldBase();

                            string getIDlink = string.Empty;
                            getIDlink = linkNode.Attributes["href"].Value.ToString(); // retrived CategoryType link

                            RqmResponseData CustomAttributeDetail = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(getIDlink));

                            XmlDocument CustomAttributeListing = new XmlDocument();

                            if (!string.IsNullOrEmpty(CustomAttributeDetail.responseText))
                            {
                                CustomAttributeListing.LoadXml(CustomAttributeDetail.responseText);
                            }

                            string CustomAttributeName = string.Empty; // -->itemfield.Name
                            string CustomAttributeItemType = string.Empty; //-->itemfield.ItemType
                            string CustomAttributeMandatory = string.Empty; // --> itemfield.Mandatory & initial value for : --> itemfield.ToUpdate
                            string CustomAttributeID = string.Empty;

                            string typeIdentifier = CustomAttributeListing.GetElementsByTagName("ns4:identifier").Item(0).InnerText;
                            CustomAttributeID = typeIdentifier[(typeIdentifier.LastIndexOf(':') + 1)..];
                            CustomAttributeName = CustomAttributeListing.GetElementsByTagName("ns4:title").Item(0).InnerText;
                            CustomAttributeItemType = CustomAttributeListing.GetElementsByTagName("ns2:scope").Item(0).InnerText;
                            string CustomAttributefieldType = CustomAttributeListing.GetElementsByTagName("ns2:type").Item(0).InnerText;
                            // Define the namespace manager for the XML document
                            XmlNamespaceManager nsManager = new XmlNamespaceManager(CustomAttributeListing.NameTable);
                            nsManager.AddNamespace("ns2", "http://jazz.net/xmlns/alm/qm/v0.1/");

                            // XPath query to find ns2:required
                            string xpath = "//ns2:required";

                            // Use SelectSingleNode to check if ns2:required element exists
                            XmlNode requiredNode = CustomAttributeListing.SelectSingleNode(xpath, nsManager);

                            if (requiredNode != null)
                            {
                                CustomAttributeMandatory = requiredNode.InnerText;
                            }
                            else
                            {
                                CustomAttributeMandatory = "false";
                            }

                            
                            itemfield.ItemType = CustomAttributeItemType;
                            itemfield.ID = CustomAttributeID;
                            itemfield.TypeIdentifier = typeIdentifier;
                            itemfield.Name = CustomAttributeName;
                            itemfield.Type = CustomAttributefieldType;
                            if (CustomAttributeMandatory.Equals("true",StringComparison.CurrentCultureIgnoreCase))
                            {
                                itemfield.ToUpdate = true;
                                itemfield.Mandatory = true;
                                if(itemfield.Type.Equals("INTEGER",StringComparison.CurrentCultureIgnoreCase))
                                {
                                    itemfield.SelectedValue = "1";
                                }
                                else if(itemfield.Type.Equals("MEDIUMSTRING", StringComparison.CurrentCultureIgnoreCase) || itemfield.Type.Equals("SMALLSTRING", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    itemfield.SelectedValue = "dummy";
                                }
                                else if(itemfield.Type.Equals("TIMESTAMP", StringComparison.CurrentCultureIgnoreCase))
                                {
                                    itemfield.SelectedValue = DateTime.Now.ToString("yyyy-MM-dd");
                                }
                                Reporter.ToLog(eLogLevel.INFO, $" CustomAttributeMandatory {CustomAttributeMandatory} itemfield.Name {itemfield.Name} itemfield.Type {itemfield.Type} itemfield..SelectedValue {itemfield.SelectedValue}");
                            }
                            else
                            {
                                itemfield.ToUpdate = false;
                                itemfield.Mandatory = false;
                            }
                            if (itemfield.SelectedValue == null)
                            {
                                itemfield.SelectedValue = "Unassigned";
                            }

                            
                            itemfield.IsCustomField = true;
                            itemfield.ProjectGuid = ALMCore.DefaultAlmConfig.ALMProjectGUID;
                            CustomAttributeRsult.Add(itemfield);
                            PopulateLogOnFieldMappingwinodw(bw, $"Populating field :{CustomAttributeName} \r\nNumber of fields populated :{CustomAttributeRsult.Count}");
                        }
                        );
                    }
                    foreach (ExternalItemFieldBase field in CustomAttributeRsult)
                    {
                        fields.Add(field);
                    }//TODO: Add Values to CategoryTypes Parallel
                    PopulateLogOnFieldMappingwinodw(bw, $"Starting values retrieve process... ");
                    #region new Get Values by filed Custom Attributes
                    foreach (ExternalItemFieldBase field in fields)
                    {
                        if(field.IsMultiple)
                        {
                            string baseUrl = $"{rqmSserverUrl}{RQMCore.ALMProjectGroupName}/service/com.ibm.rqm.integration.service.IIntegrationService/resources/{ALMCore.DefaultAlmConfig.ALMProjectGUID}/customAttribute/?fields=feed/entry/content/customAttribute/";

                            // Construct URL
                            string fullUrl = $"{baseUrl}(customAttribute[@href='{field.TypeIdentifier}']|*))";
                            Reporter.ToLog(eLogLevel.DEBUG, $"fullUrl : {fullUrl}");
                            RqmResponseData CustomAttributefieldlist = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData,
                            new Uri(fullUrl));

                            XDocument doc = XDocument.Parse(CustomAttributefieldlist.responseText);
                            XNamespace ns = "http://www.w3.org/2005/Atom";

                            // Query the XML to get all titles inside entry nodes
                            var titles = doc.Descendants(ns + "entry")
                                            .Select(entry => entry.Element(ns + "title")?.Value)
                                            .Where(title => title != null);

                            PopulateLogOnFieldMappingwinodw(bw, $"Number of values populated :{titles.Count()}");
                            if (bw != null)
                            {
                                bw.ReportProgress(CustomAttributeRsult.Count, populatedValue);
                            }

                            if (titles != null && titles.Any())
                            {
                                foreach (var title in titles)
                                {
                                    field.PossibleValues.Add(title);
                                }

                                // Set the first item as SelectedValue if PossibleValues is not empty
                                if (field.PossibleValues.Count > 0)
                                {
                                    field.SelectedValue = field.PossibleValues[0];
                                }
                            }
                        }
                        else
                        {
                            if(field.Type != null && (field.Type.Equals("MEDIUM_STRING",StringComparison.CurrentCultureIgnoreCase) || field.Type.Equals("SMALL_STRING", StringComparison.CurrentCultureIgnoreCase)))
                            {
                                field.SelectedValue = string.Empty;
                            }
                            else
                            {
                                field.SelectedValue = null;
                            }
                            
                        }
                    }
                    #endregion
                }

            }
            catch (Exception e) 
            { 
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e);
            }
            return fields;
        }

        private static void PopulateLogOnFieldMappingwinodw(BackgroundWorker bw, string msg)
        {
            populatedValue = msg;
            if (bw != null)
            {
                bw.ReportProgress(totalValues, populatedValue);
            }
        }

        public static ObservableList<ExternalItemFieldBase> GetOnlineItemFieldsForDefect(BackgroundWorker bw)
        {
            ObservableList<ExternalItemFieldBase> fields = [];

            //TODO : receive as parameters:

            RqmRepository rqmRep = new RqmRepository(RQMCore.ConfigPackageFolderPath);
            List<IProjectDefinitions> rqmProjectsDataList;
            string rqmSserverUrl = ALMCore.DefaultAlmConfig.ALMServerURL.EndsWith("/") ? ALMCore.DefaultAlmConfig.ALMServerURL : Path.Combine(ALMCore.DefaultAlmConfig.ALMServerURL, "/");
            LoginDTO loginData = new LoginDTO() { User = ALMCore.DefaultAlmConfig.ALMUserName, Password = ALMCore.DefaultAlmConfig.ALMPassword, Server = ALMCore.DefaultAlmConfig.ALMServerURL };
            string rqmDomain = RQMCore.ALMProjectGroupName;
            string rqmProject = ALMCore.DefaultAlmConfig.ALMProjectName;
            string rqmProjectGuid = ALMCore.DefaultAlmConfig.ALMProjectGUID;

            //------------------------------- Improved solution

            string baseUri_ = string.Empty;
            string selfLink_ = string.Empty;
            int maxPageNumber_ = 0;
            int totalCategoryTypeCount = 0;


            string categoryValue = string.Empty;  // --> itemfield.PossibleValues.Add(ccNode.Name);
            //string categoryTypeID = string.Empty; //--> itemfield.ID
            try
            {
                //TODO: Populate list fields with CategoryTypes
                PopulateLogOnFieldMappingwinodw(bw, "Starting fields retrieve process... ");
                string defectfieldurl = ALMCore.DefaultAlmConfig.DefectFieldAPI;
                RqmResponseData categoryType = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(defectfieldurl), true);
                XmlDocument categoryTypeList = new XmlDocument();


                if (!string.IsNullOrEmpty(categoryType.responseText))
                {
                    Reporter.ToLog(eLogLevel.DEBUG, $"ImportFromRQM GetOnlineItemFieldsForDefect categoryType.responseText : {categoryType.responseText}");
                    categoryTypeList.LoadXml(categoryType.responseText);
                }

                //TODO: Get 'next' and 'last links
                XmlNodeList linkList_ = categoryTypeList.GetElementsByTagName("rdf:Description");
                if (linkList_.Count > 0)
                {
                    foreach (XmlNode entryNode in linkList_)
                    {
                        try
                        {
                            ExternalItemFieldBase itemfield = new ExternalItemFieldBase();
                            XmlNodeList innerNodes = entryNode.ChildNodes;


                            string categoryTypeName = string.Empty; // -->itemfield.Name
                            string categoryTypeItemType = string.Empty; //-->itemfield.ItemType
                            string categoryTypeMandatory = string.Empty; // --> itemfield.Mandatory & initial value for : --> itemfield.ToUpdate
                            string categorydefaultvaluelink = string.Empty;
                            string categoryTypeID = string.Empty;
                            foreach (XmlNode node in innerNodes)
                            {
                                if (node.Name.Equals("dcterms:title", StringComparison.OrdinalIgnoreCase))
                                {
                                    categoryTypeName = node.InnerText;
                                }
                                if (node.Name.Equals("oslc:occurs", StringComparison.OrdinalIgnoreCase))
                                {
                                    categoryTypeMandatory = node.Attributes["rdf:resource"].Value.Contains("#Exactly-one") ? "true" : "false";
                                }
                                if (node.Name.Equals("oslc:name", StringComparison.OrdinalIgnoreCase))
                                {
                                    categoryTypeItemType = !string.IsNullOrEmpty(node.Attributes["rdf:datatype"].Value) ? node.Attributes["rdf:datatype"].Value.Split("#")[1] : "string";
                                    categoryTypeID = node.InnerText;
                                }

                                if (node.Name.Equals("oslc:defaultValue", StringComparison.OrdinalIgnoreCase))
                                {
                                    string categorydefaultvalue = string.Empty;
                                    categorydefaultvaluelink = node.Attributes.Count > 0 ? node.Attributes["rdf:resource"].Value : string.Empty;
                                    if (!string.IsNullOrEmpty(categorydefaultvaluelink))
                                    {
                                        try
                                        {
                                            RqmResponseData categorydefault = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(categorydefaultvaluelink), true);
                                            XmlDocument categorydefaultData = new XmlDocument();

                                            if (!string.IsNullOrEmpty(categorydefault.responseText))
                                            {
                                                categorydefaultData.LoadXml(categorydefault.responseText);
                                                try
                                                {
                                                    categorydefaultvalue = categorydefaultData.GetElementsByTagName("dcterms:title").Item(0).InnerText;
                                                }
                                                catch (Exception ex)
                                                {
                                                    categorydefaultvalue = categorydefaultData.GetElementsByTagName("foaf:name").Item(0).InnerText;
                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                                        }
                                    }

                                    itemfield.SelectedValue = !string.IsNullOrEmpty(categorydefaultvalue) ? categorydefaultvalue : string.Empty;
                                }
                                if (node.Name.Equals("oslc:allowedValues", StringComparison.OrdinalIgnoreCase))
                                {
                                    string allowedvalueslink = node.Attributes["rdf:resource"].Value;
                                    RqmResponseData allowedValues = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(allowedvalueslink), true);
                                    XmlDocument allowedValuesData = new XmlDocument();


                                    if (!string.IsNullOrEmpty(allowedValues.responseText))
                                    {
                                        allowedValuesData.LoadXml(allowedValues.responseText);
                                    }
                                    XmlNodeList allowedValuesList = allowedValuesData.GetElementsByTagName("oslc:allowedValue");

                                    foreach (XmlNode allowedValuData in allowedValuesList)
                                    {
                                        if (allowedValuData.Name.Equals("oslc:allowedValue", StringComparison.OrdinalIgnoreCase))
                                        {
                                            string singleallowedvaluelink = allowedValuData.Attributes["rdf:resource"].Value;
                                            try
                                            {
                                                RqmResponseData singleallowedValue = RQM.RQMConnect.Instance.RQMRep.GetRqmResponse(loginData, new Uri(singleallowedvaluelink), true);
                                                XmlDocument singleallowedValueData = new XmlDocument();

                                                if (!string.IsNullOrEmpty(singleallowedValue.responseText))
                                                {
                                                    singleallowedValueData.LoadXml(singleallowedValue.responseText);
                                                    string fieldValue = string.Empty;
                                                    try
                                                    {
                                                        fieldValue = singleallowedValueData.GetElementsByTagName("dcterms:title").Item(0).InnerText;
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        fieldValue = singleallowedValueData.GetElementsByTagName("foaf:name").Item(0).InnerText;
                                                    }
                                                    if (!string.IsNullOrEmpty(fieldValue))
                                                    {
                                                        itemfield.PossibleValues.Add(fieldValue);
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                                            }
                                        }
                                    }

                                }
                            }
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
                            if (!string.IsNullOrEmpty(itemfield.Name))
                            {
                                fields.Add(itemfield);
                            }
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                        }
                    }
                }
            }
            catch (Exception e) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {e.Message}", e); }
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
