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

using System;
using System.Collections.Generic;
using System.Linq;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using GingerCore.DataSource;

namespace GingerCore.FlowControlLib
{

    public enum eStatus
    {
        [EnumValueDescription("Pending")]
        Pending,
        [EnumValueDescription("Action Executed")]
        Action_Executed,
        [EnumValueDescription("Action Not Executed (Condition False)")]
        Action_Not_Executed,
        [EnumValueDescription("Skipped")]
        Skipped,
        [EnumValueDescription("Action Execution Failed (Error)")]
        Action_Execution_Failed,
    }

    public enum eFlowControlAction
    {
        // Put here ONLY items which do flow control like skip actions or goto action etc... all the rest should be regular actions
        // Only actions which move the Instruction pointer of the flow, with one exception of messagebox

        [EnumValueDescription("GoTo Action")]
        GoToAction,
        [EnumValueDescription("GoTo Activity")]
        GoToActivity,
        [EnumValueDescription("GoTo Next Action")]
        GoToNextAction,
        [EnumValueDescription("GoTo Next Activity")]
        GoToNextActivity,
        [EnumValueDescription("Stop Business Flow")]
        StopBusinessFlow,
        [EnumValueDescription("Rerun Activity")]
        RerunActivity,
        [EnumValueDescription("Rerun Action")]
        RerunAction,
        [EnumValueDescription("Show Message Box")]
        MessageBox,
        [EnumValueDescription("Stop Run")]
        StopRun,
        [EnumValueDescription("Set Variable Value")]
        SetVariableValue,
        [EnumValueDescription("Run Shared Repository Activity")]
        RunSharedRepositoryActivity,
        [EnumValueDescription("Fail Action & Stop Business Flow")]
        FailActionAndStopBusinessFlow,
        [EnumValueDescription("GoTo Activity By Name")]
        GoToActivityByName,
        [EnumValueDescription("Set Failure to be Auto-Opened Defect")]
        FailureIsAutoOpenedDefect
    }
    public enum eBusinessFlowControlAction
    {
        [EnumValueDescription("GoTo Business Flow")]
        GoToBusinessFlow,
        [EnumValueDescription("Rerun Business Flow")]
        RerunBusinessFlow,
        [EnumValueDescription("Stop Runner")]
        StopRun,
        [EnumValueDescription("Set Variable Value")]
        SetVariableValue,
    }
    public enum eFCOperator
    {
        [EnumValueDescription("Action Passed")]
        ActionPassed,
        [EnumValueDescription("Action Failed")]
        ActionFailed,
        [EnumValueDescription("Last Activity Passed")]
        LastActivityPassed,
        [EnumValueDescription("Last Activity Failed")]
        LastActivityFailed,
        [EnumValueDescription("Custom Condition")]
        CSharp,
        [EnumValueDescription("Legacy Custom Condition")]
        Legacy,
        [EnumValueDescription("Business Flow Passed")]
        BusinessFlowPassed,
        [EnumValueDescription("Business Flow Failed")]
        BusinessFlowFailed,

    }

    public class FlowControl : RepositoryItemBase
    {
        public static readonly List<object> BusinessFlowFlowControls = new List<object> { eFCOperator.BusinessFlowPassed,eFCOperator.BusinessFlowFailed,eFCOperator.CSharp,eFCOperator.Legacy };
        public static readonly List<object> ActionFlowControls = new List<object> { eFCOperator.ActionPassed, eFCOperator.ActionFailed,eFCOperator.LastActivityPassed,eFCOperator.LastActivityFailed, eFCOperator.CSharp, eFCOperator.Legacy };
        public  static partial class Fields
        {
            public static string Active = "Active";
            public static string Condition = "Condition";
            public static string ConditionCalculated = "ConditionCalculated";
            public static string FlowControlAction = "FlowControlAction";
            public static string BusinessFlowControlAction = "BusinessFlowControlAction";
            public static string Value = "Value";
            public static string ValueCalculated = "ValueCalculated";
            public static string Status = "Status";
            
        }


       
        public string GUID_NAME_SEPERATOR = "#GUID_NAME#";

        private bool mActive;
        [IsSerializedForLocalRepository]
        public bool Active { get { return mActive; } set { if (mActive != value) { mActive = value; OnPropertyChanged(nameof(Active)); } } }

        private string mCondition;

        [IsSerializedForLocalRepository]
        public string Condition { get { return mCondition; } set { if (mCondition != value) { mCondition = value; OnPropertyChanged(Fields.Condition); } } }


        /// <summary>
        /// Enable getting the condition as VE - used in Grid cell for example
        /// </summary>
        public IValueExpression ConditionVE
        {
            get
            {
                IValueExpression ve = TargetFrameworkHelper.Helper.CreateValueExpression(this, nameof(Condition));
                return ve;                
            }
        }

        public FlowControl ActionForEdit
        {
            get
            {
                return this;
            }
        }

        private string mConditionCalculated { get; set; }
        public string ConditionCalculated { get { return mConditionCalculated; } set { if (mConditionCalculated != value) { mConditionCalculated = value; OnPropertyChanged(Fields.ConditionCalculated); } } }

        private eFlowControlAction mFlowControlAction;
        [IsSerializedForLocalRepository]
        public eFlowControlAction FlowControlAction { get { return mFlowControlAction; } set { if (mFlowControlAction != value) { mFlowControlAction = value; OnPropertyChanged(nameof(FlowControlAction)); } } }

        private eBusinessFlowControlAction mBusinessFlowControlAction;
        [IsSerializedForLocalRepository]
        public eBusinessFlowControlAction BusinessFlowControlAction { get { return mBusinessFlowControlAction; } set { if (mBusinessFlowControlAction != value) { mBusinessFlowControlAction = value; OnPropertyChanged(nameof(BusinessFlowControlAction)); } } }

        private string mValue { set; get; }
        [IsSerializedForLocalRepository]
        public string Value
        {
            get
            {
                return mValue;
            }
            set
            {
                if (mValue != value)
                {
                    mValue = value;
                    OnPropertyChanged(Fields.Value);
                }
            }
        }


        private eFCOperator? mOperator;
        [IsSerializedForLocalRepository]
        public eFCOperator Operator
        {
            get
            {
                if (mOperator == null)
                {
                    return eFCOperator.Legacy;
                }

                return mOperator.Value;
            }

            set
            {
                if (mOperator != value)
                {
                    mOperator = value;
                    OnPropertyChanged(nameof(Operator));
                }
                if(!(mOperator.Value==eFCOperator.Legacy||mOperator.Value==eFCOperator.CSharp))
                {
                    Condition = string.Empty;
                }
            }
        }


        private string mValueCalculated { get; set; }
        public string ValueCalculated
        {
            get
            {
                if (string.IsNullOrEmpty(mValueCalculated) == false)
                    return mValueCalculated;
                else
                    return Value;
            }
            set
            {
                if (mValueCalculated != value)
                {
                    mValueCalculated = value;
                    OnPropertyChanged(Fields.ValueCalculated);
                }
            }
        }

        private eStatus mStatus { get; set; }
        [IsSerializedForLocalRepository]
        public eStatus Status { get { return mStatus; } set { if (mStatus != value) { mStatus = value; OnPropertyChanged(Fields.Status); } } }


        public void CalculateCondition(BusinessFlow BusinessFlow, Environments.ProjEnvironment ProjEnvironment, Act act, Activity LastExecutedActivity, ObservableList<DataSourceBase> DSList)
        {
            if (Condition == null)
            {
                ConditionCalculated = "";
                return;
            }
            // We changed enum name from Pass to Passed. Below is to support existing users flow control having pass. 
            // To be removed later
            if (Condition.Equals("\"{ActionStatus}\" = \"Pass\""))
            {
                Condition = Condition.Replace("Pass", "Passed");
            }
            else if (Condition.Equals("\"{ActionStatus}\" = \"Fail\""))
            {
                Condition = Condition.Replace("Fail", "Failed");
            }

            IValueExpression VE = TargetFrameworkHelper.Helper.CreateValueExpression(ProjEnvironment, BusinessFlow, DSList);  
            VE.Value = Condition;
            
            foreach (ActReturnValue ARC in act.ReturnValues)
            {
                if (!string.IsNullOrEmpty(ARC.Actual))
                {
                    if (VE.Value.Contains("{Actual}"))
                    {
                        if ((ARC.Actual != null) && General.IsNumeric(ARC.Actual))
                        {
                            VE.Value = VE.Value.Replace("{Actual}", ARC.Actual.ToString());
                        }
                        else
                        {
                            VE.Value = VE.Value.Replace("{Actual}", "\"" + ARC.Actual + "\"");
                        }
                    }
                }
            }
            if (VE.Value.Contains("{ActionStatus}"))
            {
                VE.Value = VE.Value.Replace("{ActionStatus}", (act.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.FailIgnored ? Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed : act.Status).ToString());
            }
            else if(VE.Value.Contains("{LastActivityStatus}"))
            {
                VE.Value = VE.Value.Replace("{LastActivityStatus}", LastExecutedActivity != null ? LastExecutedActivity.Status.ToString() : "Last executed Activity Status not available");
            }
            ConditionCalculated = VE.ValueCalculated;
        }

        public void CalculateCondition(BusinessFlow BusinessFlow, Environments.ProjEnvironment ProjEnvironment, ObservableList<DataSourceBase> DSList)
        {
            if (Condition == null)
            {
                ConditionCalculated = "";
                return;
            }
            // We changed enum name from Pass to Passed. Below is to support existing users flow control having pass. 
            // To be removed later
            if (Condition.Equals("\"{BusinessFlowStatus}\" = \"Pass\""))
            {
                Condition = Condition.Replace("Pass", "Passed");
            }
            else if (Condition.Equals("\"{BusinessFlowStatus}\" = \"Fail\""))
            {
                Condition = Condition.Replace("Fail", "Failed");
            }

            IValueExpression VE = TargetFrameworkHelper.Helper.CreateValueExpression(ProjEnvironment, BusinessFlow,DSList);

            VE.Value = Condition;

            
            VE.Value = VE.Value.Replace("{BusinessFlowStatus}", (BusinessFlow.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed ? Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed : BusinessFlow.RunStatus).ToString());

            ConditionCalculated = VE.ValueCalculated;
        }

        public void CalcualtedValue(BusinessFlow BusinessFlow, Environments.ProjEnvironment ProjEnvironment, ObservableList<DataSourceBase> DSList)
        {
            IValueExpression VE = TargetFrameworkHelper.Helper.CreateValueExpression(ProjEnvironment, BusinessFlow, DSList);
            VE.Value = Value;
            ValueCalculated = VE.ValueCalculated;
        }

        public override string ItemName
        {
            get
            {
                return string.Empty;
            }
            set
            {
                return;
            }
        }
     
        public Guid GetGuidFromValue(bool doNotUseValueCalculated = false)
        {
            try
            {
                string fcValue;
                if (doNotUseValueCalculated)
                    fcValue = Value;
                else
                    fcValue = ValueCalculated;

                if (!string.IsNullOrWhiteSpace(fcValue))
                {
                    if (fcValue.Contains(GUID_NAME_SEPERATOR))
                    {
                        string[] vals = fcValue.Split(new string[] { GUID_NAME_SEPERATOR }, StringSplitOptions.None);
                        if (vals.Length > 0)
                            return Guid.Parse(vals[0]);
                        else
                            return Guid.Empty;
                    }
                    else
                    {
                        return Guid.Parse(fcValue);
                    }
                }
                else
                    return Guid.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return Guid.Empty;
            }
        }

        public string GetNameFromValue(bool doNotUseValueCalculated = false)
        {
            try
            {
                string fcValue;
                if (doNotUseValueCalculated)
                    fcValue = Value;
                else
                    fcValue = ValueCalculated;

                if (!string.IsNullOrWhiteSpace(fcValue))
                {
                    if (fcValue.Contains(GUID_NAME_SEPERATOR))
                    {
                        string[] vals = fcValue.Split(new string[] { GUID_NAME_SEPERATOR }, StringSplitOptions.None);
                        if (vals.Length > 1)
                            return vals[1];
                        else
                            return string.Empty;
                    }
                    else
                    {
                        return fcValue;
                    }
                }
                else
                    return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return string.Empty;
            }
        }
        public List<eFlowControlAction> GetFlowControlActionsForErrorAndPopupHandler()
        {
            List<eFlowControlAction> errorFlowControlActions = new List<eFlowControlAction>();
            errorFlowControlActions.Add(eFlowControlAction.FailActionAndStopBusinessFlow);
            errorFlowControlActions.Add(eFlowControlAction.FailureIsAutoOpenedDefect);
            errorFlowControlActions.Add(eFlowControlAction.GoToAction);
            errorFlowControlActions.Add(eFlowControlAction.GoToNextAction);
            errorFlowControlActions.Add(eFlowControlAction.MessageBox);
            errorFlowControlActions.Add(eFlowControlAction.RerunAction);
            errorFlowControlActions.Add(eFlowControlAction.SetVariableValue);
            errorFlowControlActions.Add(eFlowControlAction.StopBusinessFlow);
            errorFlowControlActions.Add(eFlowControlAction.StopRun);

            return errorFlowControlActions;
        }
    }
}
