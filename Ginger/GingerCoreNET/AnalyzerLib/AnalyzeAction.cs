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
using System;
using System.Collections.Generic;
using System.Linq;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Variables;
using GingerCore.FlowControlLib;
using GingerCore.DataSource;
using System.Text.RegularExpressions;
using Amdocs.Ginger.Repository;
using amdocs.ginger.GingerCoreNET;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using Amdocs.Ginger.Common.UIElement;
using GingerCore.Actions.Common;
using Amdocs.Ginger.Common.InterfacesLib;

namespace Ginger.AnalyzerLib
{
    public class AnalyzeAction : AnalyzerItemBase
    {
        public BusinessFlow mBusinessFlow { get; set; }
        public Activity mActivity { get; set; }
        public Act mAction { get; set; }
        private ePlatformType ActivitySourcePlatform { get; set; }

        public static List<AnalyzerItemBase> Analyze(BusinessFlow BusinessFlow, Activity parentActivity, Act a, ObservableList<DataSourceBase> DSList)
        {
            // Put all tests on Action here
            List<string> ActivityUsedVariables = new List<string>();
            List<string> mUsedGlobalParameters = new List<string>();
            List<string> mMissingStoreToGlobalParameters = new List<string>();
            List<AnalyzerItemBase> IssuesList = new List<AnalyzerItemBase>();
            ObservableList<GlobalAppModelParameter> mModelsGlobalParamsList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GlobalAppModelParameter>();
            // Check if the action is obsolete and suggest conversion/upgrade/delete
            //if (a is IObsoleteAction)
            //{                
            //    if (a.Active)
            //    {
            //        // TODO: get platform from activity in test
            //        Platform.eType ActivitySourcePlatform = Platform.eType.AndroidDevice;  //FIXME temp

            //        // if it is active then create conversion issue
            //        if (((IObsoleteAction)a).IsObsoleteForPlatform(ActivitySourcePlatform))
            //        {                            
            //            AnalyzeAction AA = CreateNewIssue(IssuesList, BusinessFlow, Activity, a);
            //            AA.Description = GingerDicser.GetTermResValue(eTermResKey.Activity) + " Contains Obsolete action"; ;
            //            AA.Details = a.Description + " Old Class=" + a.ActClass;
            //            AA.HowToFix = "Convert to new action"; // TODO: get name of new action
            //            AA.CanAutoFix = AnalyzerItemBase.eCanFix.Yes;
            //            AA.IssueType = eType.Warning;
            //            AA.Impact = "New action can have more capabilities and more stable, good to upgrade";
            //            AA.Severity = eSeverity.Medium;
            //            AA.FixItHandler = UpgradeAction;
            //            AA.ActivitySourcePlatform = ActivitySourcePlatform;                        
            //        }
            //    }
            //    else
            //    {
            //        // old action but not active so create issue of delete old unused action
            //        AnalyzeAction AA = CreateNewIssue(IssuesList, BusinessFlow, Activity, a);
            //        AA.Description = GingerDicser.GetTermResValue(eTermResKey.Activity) + " Contains Obsolete action which is not used"; ;
            //        AA.Details = a.Description + " Old Class=" + a.ActClass;
            //        AA.HowToFix = "Delete action"; 
            //        AA.CanAutoFix = AnalyzerItemBase.eCanFix.Yes;
            //        AA.IssueType = eType.Warning;
            //        AA.Impact = "slower execution, disk space";
            //        AA.Severity = eSeverity.Low;
            //        AA.FixItHandler = DeleteAction;                        
            //    }
            //}
            //Flow Control -> GoToAction , Check if Action u want to go to exist
            if (a.FlowControls.Count > 0)
            {
                foreach (GingerCore.FlowControlLib.FlowControl f in a.FlowControls)
                {
                    if (f.Active == true)
                    {
                        if (f.FlowControlAction == eFlowControlAction.GoToAction)
                        {
                            string GoToActionName = f.GetNameFromValue();
                            if (parentActivity.GetAct(f.GetGuidFromValue(true), f.GetNameFromValue(true)) == null)
                            {
                                AnalyzeAction AA = CreateNewIssue(BusinessFlow, parentActivity, a);
                                AA.Description = "Flow control is mapped to Action which does not exist"; ;
                                AA.Details = "'" + GoToActionName + "' Action does not exist in '" + parentActivity.ActivityName + "' " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                                AA.HowToFix = "Remap the Flow Control Action";

                                if (parentActivity.Acts.Where(x => x.Description == f.GetNameFromValue()).FirstOrDefault() != null)
                                {
                                    //can be auto fix
                                    AA.HowToFix = "Remap Flow Control Action. Auto fix found other Action with the same name so it will fix the mapping to this Action.";
                                    AA.ErrorInfoObject = f;
                                    AA.CanAutoFix = AnalyzerItemBase.eCanFix.Yes;
                                    AA.FixItHandler = FixFlowControlWrongActionMapping;
                                }
                                else
                                    AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;

                                AA.IssueType = eType.Error;
                                AA.Impact = "Flow Control will fail on run time";
                                AA.Severity = eSeverity.High;

                                IssuesList.Add(AA);
                            }
                        }
                        if (f.FlowControlAction == eFlowControlAction.GoToActivity)
                        {
                            string GoToActivity = f.GetNameFromValue();
                            //if (BusinessFlow.Activities.Where(x => (x.ActivityName == GoToActivity)).FirstOrDefault() == null)
                            if (BusinessFlow.GetActivity(f.GetGuidFromValue(true), f.GetNameFromValue(true)) == null)
                            {
                                AnalyzeAction AA = CreateNewIssue(BusinessFlow, parentActivity, a);
                                AA.Description = "Flow control is mapped to " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " which does not exist"; ;
                                AA.Details = "'" + GoToActivity + "' " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " does not exist in the '" + BusinessFlow.Name + " ' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
                                AA.HowToFix = "Remap the Flow Control Action";

                                if (BusinessFlow.Activities.Where(x => x.ActivityName == f.GetNameFromValue()).FirstOrDefault() != null)
                                {
                                    //can be auto fix
                                    AA.HowToFix = "Remap Flow Control " + GingerDicser.GetTermResValue(eTermResKey.Activity) + ". Auto fix found other " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " with the same name so it will fix the mapping to this " + GingerDicser.GetTermResValue(eTermResKey.Activity) + ".";
                                    AA.ErrorInfoObject = f;
                                    AA.CanAutoFix = AnalyzerItemBase.eCanFix.Yes;
                                    AA.FixItHandler = FixFlowControlWrongActivityMapping;
                                }
                                else
                                    AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;

                                AA.IssueType = eType.Error;
                                AA.Impact = "Flow Control will fail on run time";
                                AA.Severity = eSeverity.High;

                                IssuesList.Add(AA);
                            }
                        }
                        if (f.FlowControlAction == eFlowControlAction.GoToNextActivity)
                        {
                            if (BusinessFlow.Activities.IndexOf(parentActivity) == (BusinessFlow.Activities.Count() - 1))
                            {
                                AnalyzeAction AA = CreateNewIssue(BusinessFlow, parentActivity, a);
                                AA.Description = "Flow control is mapped to " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " which does not exist"; ;
                                AA.Details = "Flow Control is set to 'GoToNextActivity' but the parent " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " is last one in flow.";
                                AA.HowToFix = "Remap the Flow Control Action";
                                AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                                AA.IssueType = eType.Error;
                                AA.Impact = "Flow Control will fail on run time";
                                AA.Severity = eSeverity.High;

                                IssuesList.Add(AA);
                            }
                        }
                        if (f.FlowControlAction == eFlowControlAction.SetVariableValue)
                        {
                            if (string.IsNullOrEmpty(f.Value) || ValueExpression.IsThisDynamicVE(f.Value) == false)
                            {
                                string SetVariableValue = f.GetNameFromValue();
                                string[] vals = SetVariableValue.Split(new char[] { '=' });
                                if (!BusinessFlow.CheckIfVariableExists(vals[0].ToString().Trim(), parentActivity))
                                {
                                    AnalyzeAction AA = CreateNewIssue(BusinessFlow, parentActivity, a);
                                    AA.Description = "Flow control mapped to " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " which does not exist"; ;
                                    AA.Details = "'" + vals[0].Trim() + "' " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " does not exist in parent items";
                                    AA.HowToFix = "Remap the Flow Control Action";
                                    AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                                    AA.IssueType = eType.Error;
                                    AA.Impact = "Flow Control will fail on run time";
                                    AA.Severity = eSeverity.High;

                                    IssuesList.Add(AA);
                                }
                            }
                        }
                        if (f.FlowControlAction == eFlowControlAction.RunSharedRepositoryActivity)
                        {
                            if (string.IsNullOrEmpty(f.Value) || ValueExpression.IsThisDynamicVE(f.Value) == false)
                            {
                                //f.CalcualtedValue(BusinessFlow, App.ProjEnvironment, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>());
                                string RunSharedRepositoryActivity = f.GetNameFromValue();
                                ObservableList<Activity> activities = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Activity>();
                                if (activities.Where(x => x.ActivityName == RunSharedRepositoryActivity).FirstOrDefault() == null)
                                {
                                    AnalyzeAction AA = CreateNewIssue(BusinessFlow, parentActivity, a);
                                    AA.Description = "Flow control mapped to Shared Repository " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " which does not exist";
                                    AA.Details = "'" + RunSharedRepositoryActivity + "' " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " does not exist in Shared Repository";
                                    AA.HowToFix = "Remap the Flow Control Action";
                                    AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                                    AA.IssueType = eType.Error;
                                    AA.Impact = "Flow Control will fail on run time";
                                    AA.Severity = eSeverity.High;

                                    IssuesList.Add(AA);
                                }
                            }
                        }
                        if (f.FlowControlAction == eFlowControlAction.GoToActivityByName)
                        {
                            if (string.IsNullOrEmpty(f.Value) || ValueExpression.IsThisDynamicVE(f.Value) == false)
                            {
                                string activityToGoTo = f.GetNameFromValue();
                                if (BusinessFlow.Activities.Where(x => x.ActivityName == activityToGoTo).FirstOrDefault() == null)
                                {
                                    AnalyzeAction AA = CreateNewIssue(BusinessFlow, parentActivity, a);
                                    AA.Description = "Flow control mapped to " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " which does not exist";
                                    AA.Details = "'" + activityToGoTo + "' " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " does not exist in the parent " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow);
                                    AA.HowToFix = "Remap the Flow Control Action";
                                    AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                                    AA.IssueType = eType.Error;
                                    AA.Impact = "Flow Control will fail on run time";
                                    AA.Severity = eSeverity.High;

                                    IssuesList.Add(AA);
                                }
                            }
                        }
                    }
                }
            }

            if (a.ActReturnValues.Count > 0)
            {
                foreach (ActReturnValue ARV in a.ActReturnValues)
                {
                    if (ARV.StoreTo != ActReturnValue.eStoreTo.None)
                    {
                        bool issueFound = false;
                        if (string.IsNullOrEmpty(ARV.StoreToValue))
                        {
                            issueFound = true;
                        }
                        else
                        {
                            switch (ARV.StoreTo)
                            {
                                case ActReturnValue.eStoreTo.Variable:
                                    if (BusinessFlow.GetAllVariables(parentActivity).Where(x => x.SupportSetValue && x.Name == ARV.StoreToValue).FirstOrDefault() == null)
                                    {
                                        issueFound = true;
                                    }
                                    break;

                                case ActReturnValue.eStoreTo.GlobalVariable:
                                    if (WorkSpace.Instance.Solution.Variables.Where(x => x.SupportSetValue && x.Guid.ToString() == ARV.StoreToValue).FirstOrDefault() == null)
                                    {
                                        issueFound = true;
                                    }
                                    break;

                                case ActReturnValue.eStoreTo.ApplicationModelParameter:
                                    if (WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GlobalAppModelParameter>().Where(x => x.Guid.ToString() == ARV.StoreToValue).FirstOrDefault() == null)
                                    {
                                        issueFound = true;
                                    }
                                    break;

                                case ActReturnValue.eStoreTo.DataSource:
                                    Regex rxDSPattern = new Regex(@"{(\bDS Name=)\w+\b[^{}]*}", RegexOptions.Compiled);
                                    MatchCollection matches = rxDSPattern.Matches(ARV.StoreToValue);
                                    foreach (Match match in matches)
                                    {
                                        if (GingerCoreNET.GeneralLib.General.CheckDataSource(match.Value, DSList) != "")
                                        {
                                            issueFound = true;
                                        }
                                    }
                                    break;
                            }
                        }
                        if (issueFound)
                        {
                            AnalyzeAction AA = CreateNewIssue(BusinessFlow, parentActivity, a);
                            AA.Description = string.Format("Output Value StoreTo has in-valid configuration");
                            AA.Details = string.Format("The '{0}' Output Value configured StoreTo from type '{1}' and value '{2}' is not valid", ARV.Param, ARV.StoreTo, ARV.StoreToValue);
                            AA.HowToFix = "Re-configure StoreTo for the Output Value";
                            AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                            AA.IssueType = eType.Error;
                            AA.Impact = "Execution might fail in run time";
                            AA.Severity = eSeverity.High;
                            IssuesList.Add(AA);
                        }
                    }
                }
            }
            //Disabling the below because there are many actions which shows Locate By/Value but it is not needed for most operation types
            //if (a.ObjectLocatorConfigsNeeded == true && a.ActionDescription != "Script Action" && a.ActionDescription != "File Operations")
            //{
            //    if (a.LocateBy.ToString() == "NA")
            //    {
            //        AnalyzeAction AA = CreateNewIssue(IssuesList, BusinessFlow, parentActivity, a);
            //        AA.Description = "Action is missing LocateBy value";
            //        AA.Details = "Action '" + a.Description + "' In " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " '" + parentActivity.ActivityName + "' is missing LocateBy value";
            //        AA.HowToFix = " Add LocateBy value to '" + a.Description + "'";
            //        AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;    // yes if we have one target app, or just set the first 
            //        AA.IssueType = eType.Warning;
            //        AA.Impact = GingerDicser.GetTermResValue(eTermResKey.Activity) + " will not be executed and will fail";
            //        AA.Severity = eSeverity.Medium;
            //    }
            //    if (a.LocateValue == null || a.LocateValue == "")
            //    {
            //        AnalyzeAction AA = CreateNewIssue(IssuesList, BusinessFlow, parentActivity, a);
            //        AA.Description = "Action is missing Locate Value ";
            //        AA.Details = "Action '" + a.Description + "' In " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " '" + parentActivity.ActivityName + "' is missing Locate value";
            //        AA.HowToFix = " Add Locate Value to '" + a.Description + "'";
            //        AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;    // yes if we have one target app, or just set the first 
            //        AA.IssueType = eType.Warning;
            //        AA.Impact = GingerDicser.GetTermResValue(eTermResKey.Activity) + " will not be executed and will fail";
            //        AA.Severity = eSeverity.Medium;
            //    }

            //}
            VariableBase.GetListOfUsedVariables(a, ref ActivityUsedVariables);
            if (ActivityUsedVariables.Count > 0)
            {
                foreach (string Var in ActivityUsedVariables)
                {
                    if (BusinessFlow.GetAllVariables(parentActivity).Where(x => x.Name == Var).Select(x => x.Name).FirstOrDefault() == null)
                    {
                        AnalyzeAction AA = CreateNewIssue(BusinessFlow, parentActivity, a);
                        AA.Description = "The " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " '" + Var + "' is missing";
                        AA.Details = "The " + GingerDicser.GetTermResValue(eTermResKey.Variable) + ": '" + Var + "' Does not exist In " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " '" + BusinessFlow.Name + "' => " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " '" + parentActivity.ActivityName + "' =>" + "Action '" + a.Description + "' ";
                        AA.HowToFix = " Create new " + GingerDicser.GetTermResValue(eTermResKey.Variable) + " or Delete it from the action";
                        AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                        AA.IssueType = eType.Error;
                        AA.Impact = GingerDicser.GetTermResValue(eTermResKey.Activity) + " will fail due to missing " + GingerDicser.GetTermResValue(eTermResKey.Variable);
                        AA.Severity = eSeverity.High;

                        AA.IssueCategory = eIssueCategory.MissingVariable;
                        AA.IssueReferenceObject = Var;

                        IssuesList.Add(AA);
                    }

                }
            }

            //App Model Global Params
            GlobalAppModelParameter.GetListOfUsedAppModelGlobalParameters(a, ref mUsedGlobalParameters);
            if (mUsedGlobalParameters.Count > 0)
            {
                foreach (string Param in mUsedGlobalParameters)
                {
                    GlobalAppModelParameter globalParam = mModelsGlobalParamsList.Where(x => x.PlaceHolder == Param).FirstOrDefault();
                    if (globalParam == null)
                    {
                        AnalyzeAction AA = CreateNewIssue(BusinessFlow, parentActivity, a);
                        AA.Description = "The Application Global Parameter " + Param + " is missing";
                        AA.Details = "The Application Global Parameter: '" + Param + "' Does not exist in Models Global Parameters";
                        AA.HowToFix = " Create new Application Global Parameter or Delete it from the action.";
                        AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                        AA.IssueType = eType.Error;
                        AA.Impact = GingerDicser.GetTermResValue(eTermResKey.Activity) + " will fail due to missing " + GingerDicser.GetTermResValue(eTermResKey.Variable);
                        AA.Severity = eSeverity.High;
                        IssuesList.Add(AA);
                    }
                }
            }

            GetAllStoreToGlobalParameters(a, mModelsGlobalParamsList, ref mMissingStoreToGlobalParameters);
            if (mMissingStoreToGlobalParameters.Count > 0)
            {
                foreach (string Param in mMissingStoreToGlobalParameters)
                {
                    AnalyzeAction AA = CreateNewIssue(BusinessFlow, parentActivity, a);
                    AA.Description = "The Output Value with Parameter '" + Param + "' is having store to Parameter which doesn't exist anymore";
                    AA.Details = "The Output Value with Parameter: '" + Param + "' can be found at " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " '" + BusinessFlow.Name + "' => " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " '" + parentActivity.ActivityName + "' =>" + "Action '" + a.Description + "' ";
                    AA.HowToFix = " Create new Parameter and change to it in the 'Store to' dropdown under the above path";
                    AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                    AA.IssueType = eType.Error;
                    AA.Impact = GingerDicser.GetTermResValue(eTermResKey.Activity) + " will fail due to missing Parameter";
                    AA.Severity = eSeverity.High;

                    IssuesList.Add(AA);
                }
            }
            // Put All Special Actions Analyze Here
            //actions combination
            if (a.GetType().ToString() == "ActLaunchJavaWSApplication")//forbidden combination: Launch & (platform\agent manipulation action)
            {
                List<IAct> driverActs = parentActivity.Acts.Where(x => ((x is ActWithoutDriver && x.GetType() != typeof(ActAgentManipulation)) == false) && x.Active == true).ToList();
                if (driverActs.Count > 0)
                {
                    string list = string.Join(",", driverActs.Select(x => x.ActionDescription).ToList().ToArray());
                    AnalyzeAction AA = CreateNewIssue(BusinessFlow, parentActivity, a);
                    AA.Description = GingerDicser.GetTermResValue(eTermResKey.Activity) + " has forbidden combinations";
                    AA.Details = GingerDicser.GetTermResValue(eTermResKey.Activity) + " has " + a.ActionDescription + " Action with the following platform actions: " + list + ".\nPlatform action inside this current " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " will try to activate the agent before the application is launch(will cause agent issue).";
                    AA.HowToFix = "Open the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " and put " + a.ActionDescription + " Action in a separate " + GingerDicser.GetTermResValue(eTermResKey.Activity);
                    AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;    // we can autofix by delete, but don't want to                
                    AA.IssueType = eType.Error;
                    AA.Impact = GingerDicser.GetTermResValue(eTermResKey.Activity) + " will be executed and will fail due to java agent connection";
                    AA.Severity = eSeverity.High;

                    IssuesList.Add(AA);
                }
            }

            if (a.LocateBy == eLocateBy.POMElement || ((a is ActUIElement) && ((ActUIElement)a).ElementLocateBy == eLocateBy.POMElement))
            {
                try
                {
                    string[] pOMandElementGUIDs;
                    if (a is ActUIElement)
                    {
                        pOMandElementGUIDs = ((ActUIElement)a).ElementLocateValue.Split('_');
                    }
                    else
                    {
                        pOMandElementGUIDs = a.LocateValue.Split('_');
                    }
                    Guid selectedPOMGUID = new Guid(pOMandElementGUIDs[0]);
                    ApplicationPOMModel POM = WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<ApplicationPOMModel>(selectedPOMGUID);

                    if (POM == null)
                    {
                        AnalyzeAction AA = CreateNewIssue(BusinessFlow, parentActivity, a);
                        AA.Description = "Action's mapped Page Objects Model is missing";
                        AA.Details = "Action " + a.ActionDescription + " has mapped Page Objects Model which is missing, reason can be that the Page Objects Model has been deleted after mapping it to this action.";
                        AA.HowToFix = "Open the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " and the Action in order to map different Page Objects Model and Element";
                        AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                        AA.IssueType = eType.Error;
                        AA.Impact = "Action will fail during execution";
                        AA.Severity = eSeverity.High;

                        IssuesList.Add(AA);
                    }
                    else
                    {
                        Guid selectedPOMElementGUID = new Guid(pOMandElementGUIDs[1]);
                        ElementInfo selectedPOMElement = (ElementInfo)POM.MappedUIElements.Where(z => z.Guid == selectedPOMElementGUID).FirstOrDefault();
                        if (selectedPOMElement == null)
                        {
                            AnalyzeAction AA = CreateNewIssue(BusinessFlow, parentActivity, a);
                            AA.Description = "Page Objects Model Element which mapped to this action is missing";
                            AA.Details = "Action " + a.ActionDescription + " has mapped Page Objects Model Element which is missing, reason can be that the Element has been deleted after mapping it to this action.";
                            AA.HowToFix = "Open the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " and the Action in order to map different Element";
                            AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                            AA.IssueType = eType.Error;
                            AA.Impact = "Action will fail during execution";
                            AA.Severity = eSeverity.High;

                            IssuesList.Add(AA);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Analyzer" + ex.Message);
                    //TODO: try to find the failure outside exception
                    AnalyzeAction AA = CreateNewIssue(BusinessFlow, parentActivity, a);
                    AA.Description = "Action's mapped Page Objects Model or Element is invalid";
                    AA.Details = "Action " + a.ActionDescription + " has invalid mapped Page Objects Model or Element.";
                    AA.HowToFix = "Open the " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " and the Action in order to map different Page Objects Model and Element";
                    AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                    AA.IssueType = eType.Error;
                    AA.Impact = "Action will fail during execution";
                    AA.Severity = eSeverity.High;

                    IssuesList.Add(AA);
                }
            }

            //Check for duplicate ActInputValues
            if (a.InputValues.Count > 0)
            {
                foreach (ActInputValue AIV in a.InputValues.ToList())
                {
                    if (a.InputValues.Where(aiv => AIV != null && aiv != null && aiv.Param == AIV.Param).ToList().Count > 1)
                    {
                        AnalyzeAction AA = CreateNewIssue(BusinessFlow, parentActivity, a);
                        AA.Description = "The Input Value Parameter " + AIV.Param + " is Duplicate";
                        AA.Details = "The Input Value Parameter: '" + AIV.Param + "' is duplicate in the ActInputValues";
                        AA.HowToFix = "Open action Edit page and save it.";
                        AA.CanAutoFix = AnalyzerItemBase.eCanFix.Yes;
                        AA.IssueType = eType.Warning;
                        AA.Impact = "Duplicate input values will present in the report.";
                        AA.Severity = eSeverity.Low;
                        AA.ErrorInfoObject = AIV;
                        AA.FixItHandler = FixRemoveDuplicateActInputValues;
                        IssuesList.Add(AA);
                    }
                }
            }

            return IssuesList;
        }

        private static void GetAllStoreToGlobalParameters(Act a, ObservableList<GlobalAppModelParameter> modelsGlobalParamsList, ref List<string> missingStoreToGlobalParameters)
        {
            ObservableList<ActReturnValue> ReturnValues = a.ReturnValues;
            List<ActReturnValue> ReturnValuesStoredToGlobalParameter = ReturnValues.Where(x => x.StoreTo == ActReturnValue.eStoreTo.ApplicationModelParameter).ToList();
            foreach (ActReturnValue returnValue in ReturnValuesStoredToGlobalParameter)
            {
                GlobalAppModelParameter ExistGlobalParam = modelsGlobalParamsList.Where(x => x.Guid.ToString() == returnValue.StoreToValue).FirstOrDefault();
                if (ExistGlobalParam == null)
                {
                    missingStoreToGlobalParameters.Add(returnValue.Param);
                }
            }
        }

        public static List<string> GetUsedVariableFromAction(Act action)
        {
            List<string> actionUsedVariables = new List<string>();
            VariableBase.GetListOfUsedVariables(action, ref actionUsedVariables);
            return actionUsedVariables;
        }


        private static void FixFlowControlWrongActionMapping(object sender, EventArgs e)
        {
            //look for Action with same name and re-map the Flow Control            
            AnalyzeAction AA = (AnalyzeAction)sender;
            if (AA.ErrorInfoObject == null)
            {
                AA.Status = eStatus.CannotFix;
                return;
            }
            FlowControl flowControl = (FlowControl)AA.ErrorInfoObject;
            if (AA.mActivity.GetAct(flowControl.GetGuidFromValue(true), flowControl.GetNameFromValue(true)) == null)
            {
                Act similarNameAct = (Act)AA.mActivity.Acts.Where(x => x.Description == flowControl.GetNameFromValue()).FirstOrDefault();
                if (similarNameAct != null)
                {
                    string updatedMappingValue = similarNameAct.Guid + flowControl.GUID_NAME_SEPERATOR + similarNameAct.Description;
                    flowControl.Value = updatedMappingValue;
                    flowControl.ValueCalculated = string.Empty;
                    AA.Status = eStatus.Fixed;
                    return;
                }
            }

            AA.Status = eStatus.CannotFix;
            return;
        }

        private static void FixFlowControlWrongActivityMapping(object sender, EventArgs e)
        {
            //look for Activity with same name and re-map the Flow Control            
            AnalyzeAction AA = (AnalyzeAction)sender;
            if (AA.ErrorInfoObject == null)
            {
                AA.Status = eStatus.CannotFix;
                return;
            }
            FlowControl flowControl = (FlowControl)AA.ErrorInfoObject;
            if (AA.mBusinessFlow.GetActivity(flowControl.GetGuidFromValue(true), flowControl.GetNameFromValue(true)) == null)
            {
                Activity similarNameActivity = (Activity)AA.mBusinessFlow.Activities.Where(x => x.ActivityName == flowControl.GetNameFromValue()).FirstOrDefault();
                if (similarNameActivity != null)
                {
                    string updatedMappingValue = similarNameActivity.Guid + flowControl.GUID_NAME_SEPERATOR + similarNameActivity.Description;
                    flowControl.Value = updatedMappingValue;
                    flowControl.ValueCalculated = string.Empty;
                    AA.Status = eStatus.Fixed;
                    return;
                }
            }

            AA.Status = eStatus.CannotFix;
            return;
        }
        private static void FixRemoveDuplicateActInputValues(object sender, EventArgs e)
        {
            AnalyzeAction AA = (AnalyzeAction)sender;
            if (AA.ErrorInfoObject == null)
            {
                AA.Status = eStatus.CannotFix;
                return;
            }
            ActInputValue AIV = (ActInputValue)AA.ErrorInfoObject;
            while (AA.mAction.InputValues.Where(aiv => aiv.Param == AIV.Param).ToList().Count > 1)
            {
                AA.mAction.InputValues.Remove((from aiv in AA.mAction.InputValues where aiv.Param == AIV.Param select aiv).LastOrDefault());
            }
            AA.Status = eStatus.Fixed;
            return;
        }
        static AnalyzeAction CreateNewIssue(BusinessFlow BusinessFlow, Activity Activity, Act action)
        {
            AnalyzeAction AA = new AnalyzeAction();
            AA.Status = AnalyzerItemBase.eStatus.NeedFix;
            AA.mActivity = Activity;
            AA.ItemName = action.Description;
            AA.ItemParent = BusinessFlow.Name + " > " + Activity.ActivityName;
            AA.mAction = action;
            AA.mBusinessFlow = BusinessFlow;
            AA.ItemClass = "Action";
            return AA;
        }


    }
}
