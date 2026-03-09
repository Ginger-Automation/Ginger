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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.AnalyzerLib;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Common;
using GingerCore.DataSource;
using GingerCore.Drivers;
using GingerCore.FlowControlLib;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

#nullable enable
namespace Ginger.AnalyzerLib
{
    public class AnalyzeAction : AnalyzerItemBase
    {
        public BusinessFlow mBusinessFlow { get; set; }
        public Activity mActivity { get; set; }
        public Act mAction { get; set; }
        private ePlatformType ActivitySourcePlatform { get; set; }

        public static List<AnalyzerItemBase> Analyze(BusinessFlow BusinessFlow, Activity parentActivity, Act a, ObservableList<DataSourceBase> DSList, DriverBase? driver = null)
        {
            // Put all tests on Action here
            List<string> ActivityUsedVariables = [];
            List<string> mUsedGlobalParameters = [];
            List<string> mMissingStoreToGlobalParameters = [];
            List<AnalyzerItemBase> IssuesList = [];
            ObservableList<GlobalAppModelParameter> mModelsGlobalParamsList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GlobalAppModelParameter>();

            AnalyzeValueExpInAction(a, BusinessFlow, parentActivity, ref IssuesList);

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
                                AA.Description = "Flow control is mapped to Action which does not exist";
                                AA.Details = $"'{ GoToActionName }' Action does not exist in '{ parentActivity.ActivityName }' { GingerDicser.GetTermResValue(eTermResKey.Activity)}";
                                AA.HowToFix = "Remap the Flow Control Action";

                                if (parentActivity.Acts.FirstOrDefault(x => x.Description == f.GetNameFromValue()) != null)
                                {
                                    //can be auto fix
                                    AA.HowToFix = "Remap Flow Control Action. Auto fix found other Action with the same name so it will fix the mapping to this Action.";
                                    AA.ErrorInfoObject = f;
                                    AA.CanAutoFix = AnalyzerItemBase.eCanFix.Yes;
                                    AA.FixItHandler = FixFlowControlWrongActionMapping;
                                }
                                else
                                {
                                    AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                                }

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
                                AA.Description = $"Flow control is mapped to { GingerDicser.GetTermResValue(eTermResKey.Activity) } which does not exist"; ;
                                AA.Details = $"'{ GoToActivity }'{ GingerDicser.GetTermResValue(eTermResKey.Activity) } does not exist in the '{ BusinessFlow.Name } ' { GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)}";
                                AA.HowToFix = "Remap the Flow Control Action";

                                if (BusinessFlow.Activities.FirstOrDefault(x => x.ActivityName == f.GetNameFromValue()) != null)
                                {
                                    //can be auto fix
                                    AA.HowToFix = $"Remap Flow Control { GingerDicser.GetTermResValue(eTermResKey.Activity) }. Auto fix found other { GingerDicser.GetTermResValue(eTermResKey.Activity) } with the same name so it will fix the mapping to this { GingerDicser.GetTermResValue(eTermResKey.Activity) }.";
                                    AA.ErrorInfoObject = f;
                                    AA.CanAutoFix = AnalyzerItemBase.eCanFix.Yes;
                                    AA.FixItHandler = FixFlowControlWrongActivityMapping;
                                }
                                else
                                {
                                    AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                                }

                                AA.IssueType = eType.Error;
                                AA.Impact = "Flow Control will fail on run time";
                                AA.Severity = eSeverity.High;

                                IssuesList.Add(AA);
                            }
                        }
                        if (f.FlowControlAction == eFlowControlAction.GoToNextActivity)
                        {
                            if (BusinessFlow.Activities.IndexOf(parentActivity) == (BusinessFlow.Activities.Count - 1))
                            {
                                AnalyzeAction AA = CreateNewIssue(BusinessFlow, parentActivity, a);
                                AA.Description = $"Flow control is mapped to { GingerDicser.GetTermResValue(eTermResKey.Activity) } which does not exist";
                                AA.Details = $"Flow Control is set to 'GoToNextActivity' but the parent { GingerDicser.GetTermResValue(eTermResKey.Activity) } is last one in flow.";
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
                                    AA.Description = $"Flow control mapped to { GingerDicser.GetTermResValue(eTermResKey.Variable) } which does not exist"; ;
                                    AA.Details = $"'{ vals[0].Trim() }' { GingerDicser.GetTermResValue(eTermResKey.Variable) } does not exist in parent items";
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
                                if (activities.FirstOrDefault(x => x.ActivityName == RunSharedRepositoryActivity) == null)
                                {
                                    AnalyzeAction AA = CreateNewIssue(BusinessFlow, parentActivity, a);
                                    AA.Description = $"Flow control mapped to Shared Repository { GingerDicser.GetTermResValue(eTermResKey.Activity) } which does not exist";
                                    AA.Details = $"'{ RunSharedRepositoryActivity }' { GingerDicser.GetTermResValue(eTermResKey.Activity) } does not exist in Shared Repository";
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
                                if (BusinessFlow.Activities.FirstOrDefault(x => x.ActivityName == activityToGoTo) == null)
                                {
                                    AnalyzeAction AA = CreateNewIssue(BusinessFlow, parentActivity, a);
                                    AA.Description = $"Flow control mapped to { GingerDicser.GetTermResValue(eTermResKey.Activity) } which does not exist";
                                    AA.Details = $"'{ activityToGoTo }' { GingerDicser.GetTermResValue(eTermResKey.Activity) } does not exist in the parent { GingerDicser.GetTermResValue(eTermResKey.BusinessFlow)}";
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
                                    if (BusinessFlow.GetAllVariables(parentActivity).FirstOrDefault(x => x.SupportSetValue && x.Name == ARV.StoreToValue) == null)
                                    {
                                        issueFound = true;
                                    }
                                    break;

                                case ActReturnValue.eStoreTo.GlobalVariable:
                                    if (WorkSpace.Instance.Solution.Variables.FirstOrDefault(x => x.SupportSetValue && x.Guid.ToString() == ARV.StoreToValue) == null)
                                    {
                                        issueFound = true;
                                    }
                                    break;

                                case ActReturnValue.eStoreTo.ApplicationModelParameter:
                                    if (Guid.TryParse(ARV.StoreToValue, out Guid gampGuid))
                                    {
                                        if (WorkSpace.Instance.SolutionRepository.GetRepositoryItemByGuid<GlobalAppModelParameter>(gampGuid) == null)
                                        {
                                            issueFound = true;
                                        }
                                    }
                                    else
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
                        AA.Description = $"The { GingerDicser.GetTermResValue(eTermResKey.Variable) } '{ Var }' is missing";
                        AA.Details = $"The { GingerDicser.GetTermResValue(eTermResKey.Variable) }: '{ Var }' Does not exist In { GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) } '{ BusinessFlow.Name }' => { GingerDicser.GetTermResValue(eTermResKey.Activity) } '{ parentActivity.ActivityName }' => Action '{ a.Description }' ";
                        AA.HowToFix = $" Create new { GingerDicser.GetTermResValue(eTermResKey.Variable) } or Delete it from the action";
                        AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                        AA.IssueType = eType.Error;
                        AA.Impact = $"{GingerDicser.GetTermResValue(eTermResKey.Activity) } will fail due to missing { GingerDicser.GetTermResValue(eTermResKey.Variable)}";
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
                    GlobalAppModelParameter globalParam = mModelsGlobalParamsList.FirstOrDefault(x => x.PlaceHolder == Param);
                    if (globalParam == null)
                    {
                        AnalyzeAction AA = CreateNewIssue(BusinessFlow, parentActivity, a);
                        AA.Description = $"The Application Global Parameter { Param } is missing";
                        AA.Details = $"The Application Global Parameter: '{ Param }' Does not exist in Models Global Parameters";
                        AA.HowToFix = " Create new Application Global Parameter or Delete it from the action.";
                        AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                        AA.IssueType = eType.Error;
                        AA.Impact = $"{GingerDicser.GetTermResValue(eTermResKey.Activity)} will fail due to missing { GingerDicser.GetTermResValue(eTermResKey.Variable)}";
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
                    AA.Description = $"The Output Value with Parameter '{ Param }' is having store to Parameter which doesn't exist anymore";
                    AA.Details = $"The Output Value with Parameter: '{ Param }' can be found at { GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) } '{ BusinessFlow.Name }' => { GingerDicser.GetTermResValue(eTermResKey.Activity) } '{ parentActivity.ActivityName }' => Action '{ a.Description }' ";
                    AA.HowToFix = " Create new Parameter and change to it in the 'Store to' drop-down under the above path";
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
                    AA.Description = $"{GingerDicser.GetTermResValue(eTermResKey.Activity) } has forbidden combinations";
                    AA.Details = $"{GingerDicser.GetTermResValue(eTermResKey.Activity) } has { a.ActionDescription } Action with the following platform actions: { list }.\nPlatform action inside this current { GingerDicser.GetTermResValue(eTermResKey.Activity) } will try to activate the agent before the application is launch(will cause agent issue).";
                    AA.HowToFix = $"Open the { GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) } { GingerDicser.GetTermResValue(eTermResKey.Activity) } and put { a.ActionDescription } Action in a separate { GingerDicser.GetTermResValue(eTermResKey.Activity)}";
                    AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;    // we can autofix by delete, but don't want to                
                    AA.IssueType = eType.Error;
                    AA.Impact = $"{GingerDicser.GetTermResValue(eTermResKey.Activity) } will be executed and will fail due to java agent connection";
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
                        AA.Details = $"Action { a.ActionDescription } has mapped Page Objects Model which is missing, reason can be that the Page Objects Model has been deleted after mapping it to this action.";
                        AA.HowToFix = $"Open the { GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) } { GingerDicser.GetTermResValue(eTermResKey.Activity) } and the Action in order to map different Page Objects Model and Element";
                        AA.CanAutoFix = AnalyzerItemBase.eCanFix.No;
                        AA.IssueType = eType.Error;
                        AA.Impact = "Action will fail during execution";
                        AA.Severity = eSeverity.High;

                        IssuesList.Add(AA);
                    }
                    else
                    {
                        Guid selectedPOMElementGUID = new Guid(pOMandElementGUIDs[1]);
                        ElementInfo selectedPOMElement = POM.MappedUIElements.FirstOrDefault(z => z.Guid == selectedPOMElementGUID);
                        if (selectedPOMElement == null)
                        {
                            AnalyzeAction AA = CreateNewIssue(BusinessFlow, parentActivity, a);
                            AA.Description = "Page Objects Model Element which mapped to this action is missing";
                            AA.Details = $"Action { a.ActionDescription } has mapped Page Objects Model Element which is missing, reason can be that the Element has been deleted after mapping it to this action.";
                            AA.HowToFix = $"Open the { GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) } { GingerDicser.GetTermResValue(eTermResKey.Activity) } and the Action in order to map different Element";
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
                    AA.Details = $"Action { a.ActionDescription } has invalid mapped Page Objects Model or Element.";
                    AA.HowToFix = $"Open the { GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) } { GingerDicser.GetTermResValue(eTermResKey.Activity) } and the Action in order to map different Page Objects Model and Element";
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
                        AA.Description = $"The Input Value Parameter { AIV.Param } is Duplicate";
                        AA.Details = $"The Input Value Parameter: '{ AIV.Param }' is duplicate in the ActInputValues";
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

            if (driver is not null and IIncompleteDriver incompleteDriver)
            {
                if (!incompleteDriver.IsActionSupported(a, out string message))
                {
                    AnalyzeAction issue = CreateNewIssue(BusinessFlow, parentActivity, a);
                    issue.Description = "Action not supported by Driver";
                    issue.Details = message;
                    issue.CanAutoFix = eCanFix.No;
                    issue.HowToFix = "Please choose a compatible driver or modify the action to ensure it is supported by the current driver.";
                    issue.IssueType = eType.Error;
                    issue.Impact = "Action execution will fail.";
                    issue.Severity = eSeverity.Critical;

                    IssuesList.Add(issue);
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
                GlobalAppModelParameter ExistGlobalParam = modelsGlobalParamsList.FirstOrDefault(x => x.Guid.ToString() == returnValue.StoreToValue);
                if (ExistGlobalParam == null)
                {
                    missingStoreToGlobalParameters.Add(returnValue.Param);
                }
            }
        }

        public static void AnalyzeValueExpInAction(Act action, BusinessFlow businessFlow, Activity activity, ref List<AnalyzerItemBase> issues)
        {
            /// Description Example : "Cannot Calculate Value Expression : <ValueExpression> used in Custom Condition in the Flow Control Tab"


            var ValueExpsNotInCurrEnv = action.ActInputValues
                                .Where((actInputValue) =>
                                !AnalyzeEnvApplication.DoesEnvParamOrURLExistInValueExp(actInputValue.Value, businessFlow.Environment)
                                ).Select((filteredActInputVal) => $"{filteredActInputVal.Value} used in Operation Settings")
                                .ToList();


            if (!AnalyzeEnvApplication.DoesEnvParamOrURLExistInValueExp(action.RunDescription, businessFlow.Environment))
            {

                ValueExpsNotInCurrEnv.Add("used in Run Description");
            }


            var FlowControlValues = action
                                    .ActFlowControls
                                    .Select((actFlowControl) =>
                                    {
                                        if (!AnalyzeEnvApplication.DoesEnvParamOrURLExistInValueExp(actFlowControl.Condition, businessFlow.Environment))
                                        {
                                            return $"{actFlowControl.Condition} used in Custom Condition in the Flow Control Tab";
                                        }

                                        if (!AnalyzeEnvApplication.DoesEnvParamOrURLExistInValueExp(actFlowControl.Value, businessFlow.Environment))
                                        {
                                            return $"{actFlowControl.Value} used in the Flow Control Tab";
                                        }
                                        return string.Empty;
                                    })
                                    .Where((filteredFlowControl) => !string.Equals(filteredFlowControl, string.Empty));



            var ReturnValues = action
                                .ActReturnValues
                                .Select((actReturnValue) =>
                                {

                                    if (!AnalyzeEnvApplication.DoesEnvParamOrURLExistInValueExp(actReturnValue.Param, businessFlow.Environment))
                                    {
                                        return $"{actReturnValue.Param} used in Param in Output Values Tab";
                                    }

                                    if (!AnalyzeEnvApplication.DoesEnvParamOrURLExistInValueExp(actReturnValue.Path, businessFlow.Environment))
                                    {
                                        return $"{actReturnValue.Path} used in Path in the Output Values Tab";
                                    }

                                    if (!AnalyzeEnvApplication.DoesEnvParamOrURLExistInValueExp(actReturnValue.Expected, businessFlow.Environment))
                                    {
                                        return $"{actReturnValue.Expected} used in Expected Value in the Output Values Tab";
                                    }
                                    return string.Empty;

                                })
                                .Where((filteredReturnValue) => !string.Equals(filteredReturnValue, string.Empty));

            ValueExpsNotInCurrEnv.AddRange(FlowControlValues);
            ValueExpsNotInCurrEnv.AddRange(ReturnValues);


            foreach (var filteredValueExp in ValueExpsNotInCurrEnv)
            {
                AnalyzeAction AA = new AnalyzeAction
                {
                    Status = eStatus.NeedFix,
                    mActivity = activity,
                    Description = $"Cannot Calculate Value Expression: {filteredValueExp}",
                    ItemName = action.Description,
                    ItemParent = $"{businessFlow.Name } > { activity.ActivityName}",
                    mAction = action,
                    mBusinessFlow = businessFlow,
                    ItemClass = "Action",
                    CanAutoFix = eCanFix.No,
                    Severity = eSeverity.High,
                    HowToFix = $"Please ensure that you have selected the appropriate environment and that the parameter/URL exists in the chosen environment: '{businessFlow.Environment}'"
                };
                issues.Add(AA);
            }



        }

        public static List<string> GetUsedVariableFromAction(Act action)
        {
            List<string> actionUsedVariables = [];
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
                Act similarNameAct = (Act)AA.mActivity.Acts.FirstOrDefault(x => x.Description == flowControl.GetNameFromValue());
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
                Activity similarNameActivity = AA.mBusinessFlow.Activities.FirstOrDefault(x => x.ActivityName == flowControl.GetNameFromValue());
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
            AnalyzeAction AA = new AnalyzeAction
            {
                Status = AnalyzerItemBase.eStatus.NeedFix,
                mActivity = Activity,
                ItemName = action.Description,
                ItemParent = BusinessFlow.Name + " > " + Activity.ActivityName,
                mAction = action,
                mBusinessFlow = BusinessFlow,
                ItemClass = "Action"
            };
            return AA;
        }


    }
}
