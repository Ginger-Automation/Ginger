//#region License
///*
//Copyright © 2014-2018 European Support Limited

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

//using Amdocs.Ginger.Common;
//using Amdocs.Ginger.CoreNET.Execution;
//using Amdocs.Ginger.Repository;
//using GingerCoreNET.GeneralLib;
//using GingerCoreNET.ReporterLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActionsLib.DataSourceLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.BusinessFlowLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.EnvironmentsLib;
//using System;
//using System.Reflection;

//namespace GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActionsLib.Common
//{
//    public class FlowControl : RepositoryItem
//    {
//        public static partial class Fields
//        {
//            public static string Active = "Active";
//            public static string Condition = "Condition";
//            public static string ConditionCalculated = "ConditionCalculated";
//            public static string FlowControlAction = "FlowControlAction";
//            public static string Value = "Value";
//            public static string ValueCalculated = "ValueCalculated";
//            public static string Status = "Status";
//            public static string ConditionVE = "ConditionVE";
//        }

//        public enum eFlowControlAction
//        {
//            // Put here ONLY items which do flow control like skip actions or goto action etc... all the rest should be regualr actions
//            // Only actions which move the Instruction pointer of the flow, with one exception of messagebox

//            [EnumValueDescription("GoTo Action")]
//            GoToAction,
//            [EnumValueDescription("GoTo Activity")]
//            GoToActivity,
//            [EnumValueDescription("GoTo Next Action")]
//            GoToNextAction,
//            [EnumValueDescription("GoTo Next Activity")]
//            GoToNextActivity,
//            [EnumValueDescription("Stop Business Flow")]
//            StopBusinessFlow,
//            [EnumValueDescription("Rerun Activity")]
//            RerunActivity,
//            [EnumValueDescription("Rerun Action")]
//            RerunAction,
//            [EnumValueDescription("Show Message Box")]
//            MessageBox,
//            [EnumValueDescription("Stop Run")]
//            StopRun,
//            [EnumValueDescription("Set Variable Value")]
//            SetVariableValue,
//            [EnumValueDescription("Run Shared Repository Activity")]
//            RunSharedRepositoryActivity
//        }

//        public enum eStatus
//        {
//            [EnumValueDescription("Pending")]
//            Pending,
//            [EnumValueDescription("Executed - condition true")]
//            Executed_Action,
//            [EnumValueDescription("Executed - condition false")]
//            Executed_NoAction,
//            [EnumValueDescription("No Action")]
//            Skipped,
//            [EnumValueDescription("Execution Failed - Error")]
//            Execution_Failed,
//        }

//        public string GUID_NAME_SEPERATOR = "#GUID_NAME#";

//        private bool mActive = true;
//        [IsSerializedForLocalRepository(DefaultValue:true)]
//        public bool Active { get { return mActive; } set { mActive = value; } }
        
//        private string mCondition;

//        [IsSerializedForLocalRepository]
//        public string Condition { get { return mCondition; } set { mCondition = value; OnPropertyChanged(Fields.Condition); } }
        
//        /// <summary>
//        /// Enable getting the condition as VE - used in Grid cell for example
//        /// </summary>
//        public ValueExpression ConditionVE
//        {
//            get
//            {
//                ValueExpression ve = new ValueExpression(this, Fields.Condition);
//                return ve;
//            }
//        }

//        public ValueExpression ValueVE
//        {
//            get
//            {
//                ValueExpression ve = new ValueExpression(this, Fields.Value);
//                return ve;
//            }
//        }

//        public FlowControl ActionForEdit
//        {
//            get
//            {
//                return this;
//            }
//        }


//        private string mConditionCalculated { get; set; }
//        public string ConditionCalculated { get { return mConditionCalculated; } set { mConditionCalculated = value; OnPropertyChanged(Fields.ConditionCalculated); } }
        
//        private eFlowControlAction mFlowControlAction = eFlowControlAction.GoToAction;
//        [IsSerializedForLocalRepository(DefaultValue: eFlowControlAction.GoToAction)]
//        public eFlowControlAction FlowControlAction { get { return mFlowControlAction; } set { mFlowControlAction = value; } }

//        private string mValue { set; get; }
//        [IsSerializedForLocalRepository]
//        public string Value { get { return mValue; } set { mValue = value; OnPropertyChanged(Fields.Value); } }

//        private string mValueCalculated { get; set; }
//        public string ValueCalculated
//        {
//            get
//            {
//                if (string.IsNullOrEmpty(mValueCalculated) == false)
//                    return mValueCalculated;
//                else
//                    return Value;
//            }
//            set { mValueCalculated = value; OnPropertyChanged(Fields.ValueCalculated); }
//        }
        
//        private eStatus mStatus { get; set; }
//        public eStatus Status { get { return mStatus; } set { mStatus = value; OnPropertyChanged(Fields.Status); } }
        
//        public void CalculateCondition(BusinessFlow BusinessFlow, ProjEnvironment ProjEnvironment, Act act, ObservableList<DataSourceBase> DSList)
//        {
//            if (Condition == null)
//            {
//                ConditionCalculated = "";
//                return;
//            }
//            // We changed enum name from Pass to Passed. Below is to support existing users flow control having pass. 
//            // To be removed later
//            if (Condition.Equals("\"{ActionStatus}\" = \"Pass\""))
//            {
//                Condition = Condition.Replace("Pass", "Passed");
//            }
//            else if (Condition.Equals("\"{ActionStatus}\" = \"Fail\""))
//            {
//                Condition = Condition.Replace("Fail", "Failed");
//            }

//            ValueExpression VE = new ValueExpression(ProjEnvironment, BusinessFlow,DSList);
//            VE.Value = Condition;

//            // replace place holder for known props
//            //Change this to save effort of chaging 100+ existing flows 

//            foreach (ActReturnValue ARC in act.ReturnValues)
//            {
//                if (!string.IsNullOrEmpty(ARC.Actual))
//                {
//                    if (VE.Value.Contains("{Actual}"))
//                    {
//                        if ((ARC.Actual != null) && General.IsNumeric(ARC.Actual))
//                        {
//                            VE.Value = VE.Value.Replace("{Actual}", ARC.Actual.ToString());
//                        }
//                        else
//                        {
//                            VE.Value = VE.Value.Replace("{Actual}", "\"" + ARC.Actual + "\"");
//                        }
//                    }
//                }
//            }
//            VE.Value = VE.Value.Replace("{ActionStatus}", (act.Status == eRunStatus.FailIgnored ? eRunStatus.Failed : act.Status).ToString());

//            ConditionCalculated = VE.ValueCalculated;
//        }

//        public void CalcualtedValue(BusinessFlow BusinessFlow, ProjEnvironment ProjEnvironment, ObservableList<DataSourceBase> DSList)
//        {
//            ValueExpression VE = new ValueExpression(ProjEnvironment, BusinessFlow, DSList);
//            VE.Value = Value;
//            ValueCalculated = VE.ValueCalculated;
//        }

//        public override string ItemName
//        {
//            get
//            {
//                return string.Empty;
//            }
//            set
//            {
//                return;
//            }
//        }

//        public Guid GetGuidFromValue()
//        {
//            try
//            {
//                if (ValueCalculated.Contains(GUID_NAME_SEPERATOR))
//                {
//                    string[] vals = ValueCalculated.Split(new string[] { GUID_NAME_SEPERATOR }, StringSplitOptions.None);
//                    if (vals.Length > 0)
//                        return Guid.Parse(vals[0]);
//                    else
//                        return Guid.Empty;
//                }
//                else
//                {
//                    return Guid.Parse(ValueCalculated);
//                }
//            }
//            catch (Exception ex)
//            {
//                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
//                return Guid.Empty;
//            }
//        }

//        public string GetNameFromValue()
//        {
//            try
//            {
//                if (ValueCalculated.Contains(GUID_NAME_SEPERATOR))
//                {
//                    string[] vals = ValueCalculated.Split(new string[] { GUID_NAME_SEPERATOR }, StringSplitOptions.None);
//                    if (vals.Length > 1)
//                        return vals[1];
//                    else
//                        return string.Empty;
//                }
//                else
//                {
//                    return ValueCalculated;
//                }
//            }
//            catch (Exception ex)
//            {
//                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
//                return string.Empty;
//            }
//        }
//    }
//}
