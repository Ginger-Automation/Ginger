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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCore.DataSource;
using GingerCore.Environments;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Ginger.Reports
{
    // Json will serialize only marked attr and not all
    [JsonObject(MemberSerialization.OptIn)]
    public class ActionReport
    {
        public static partial class Fields
        {
            public static string Seq = "Seq";
            public static string Description = "Description";
            public static string RunDescription = "RunDescription";
            public static string StartTimeStamp = "StartTimeStamp";
            public static string EndTimeStamp = "EndTimeStamp";
            public static string ElapsedSecs = "ElapsedSecs";
            public static string CurrentRetryIteration = "CurrentRetryIteration";
            public static string Status = "Status";
            public static string Error = "Error";
            public static string ExInfo = "ExInfo";
            public static string InputValues = "InputValues";
            public static string InputValuesDT = "InputValuesDT";
            public static string OutputValues = "OutputValues";
            public static string OutputValuesDT = "OutputValuesDT";
            public static string FlowControls = "FlowControls";
            public static string FlowControlDT = "FlowControlDT";
            public static string ScreenShot = "ScreenShot";
            public static string ScreenShots = "ScreenShots";
        }
        
        private IAct mAction;
        Context mContext;
        //private ProjEnvironment mExecutionEnviroment;
        private string _localFolder;

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Execution Sequence")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(false)]
        [FieldParamsIsSelected(true)]
        public int Seq { get; set; }

        [JsonProperty]
        public string GUID { get { return mAction != null ? mAction.Guid.ToString() : SourceGuid; } set { SourceGuid = value; } }

        public string SourceGuid = string.Empty;

        [JsonProperty]
        public string Name { get { return mAction != null ? mAction.ItemName : name; } set { name = value; } }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Description")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(false)]
        [FieldParamsIsSelected(true)]
        public string Description { get { return mAction != null ? mAction.Description : description; } set { description = value; } }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Run Description")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(false)]
        [FieldParamsIsSelected(true)]
        public string RunDescription { get; set; }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Action Type")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(false)]
        [FieldParamsIsSelected(true)]
        public string ActionType { get { return mAction != null ? mAction.ActionType : actionType; } set { actionType = value; } }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Execution Start Time")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public DateTime StartTimeStamp { get { return mAction != null ? mAction.StartTimeStamp : startTimeStamp; } set { startTimeStamp = value; } }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Execution End Time")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public DateTime EndTimeStamp { get { return mAction != null ? mAction.EndTimeStamp : endTimeStamp; } set { endTimeStamp = value; } }

        [FieldParams]
        [FieldParamsNameCaption("Elapsed (Seconds)")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public float? ElapsedSecs { get { return elapsedSecs; } }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Current Retry Iteration")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public int CurrentRetryIteration { get { return mAction != null ? mAction.RetryMechanismCount : currentRetryIteration; } set { currentRetryIteration = value; } }        

        [JsonProperty]
        public long? Elapsed { get { return mAction != null ? mAction.Elapsed : elapsed; } set { elapsed = value; } }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Execution Status")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string Status 
        {
            get
            {
                if (mAction != null)
                {
                    if (mAction.StatusConverter == eStatusConverterOptions.IgnoreFail && mAction.Status==Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed)
                    {
                        return Amdocs.Ginger.CoreNET.Execution.eRunStatus.FailIgnored.ToString();
                    }
                    else
                    {
                        return mAction.Status.ToString();
                    }
                }
                else
                {
                    return status;
                }
            }
            set { status = value; }
        }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Error Details")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string Error { get { return mAction != null ? mAction.Error : error; } set { error = value; } } 
                                                                                                                 
        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Extra Details")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string ExInfo { get { return mAction != null ? mAction.ExInfo : exInfo; } set { exInfo = value; } }
        [JsonProperty]
        public List<string> InputValues
         {
             get
             {
                 if (inputValues == null)
                 {
                    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                     inputValues = mAction.InputValues.Select(a => Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(a.Param + "_:_" + a.Value + "_:_" + GetValueForDriverWithoutDescrypting(a.Value))).ToList();

                     if ((mAction.GetInputValueListForVEProcessing() != null) && (mAction.GetInputValueListForVEProcessing().Count > 0))
                     {
                         mAction.GetInputValueListForVEProcessing().ForEach(x => x.Select(a => Ginger.Reports.GingerExecutionReport.ExtensionMethods.OverrideHTMLRelatedCharacters(a.Param + "_:_" + a.Value + "_:_" + GetValueForDriverWithoutDescrypting(a.Value))).ToList().ForEach(z => inputValues.Add(z)));
                     }
                 }
                 return inputValues;
             }
             set { inputValues = value; }
         }
        private List<string> inputValues;

        [FieldParams]
        [FieldParamsNameCaption("Input Values")]
        [FieldParamsFieldType(FieldsType.Section)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public DataTable InputValuesDT
        {
            get
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("ParameterName");
                dt.Columns["ParameterName"].Caption = "Parameter Name";
                dt.Columns.Add("Value");
                dt.Columns["Value"].Caption = "Value";
                dt.Columns.Add("ValueCalculated");
                dt.Columns["ValueCalculated"].Caption = "Value Calculated";

                foreach (string inputValue in InputValues)
                {
                    String[] elementsAfter = inputValue.Split(new string[] { "_:_" }, StringSplitOptions.None);
                    DataRow dr = dt.NewRow();
                    dr["ParameterName"] = elementsAfter[0];
                    dr["Value"] = elementsAfter[1];
                    dr["ValueCalculated"] = elementsAfter[2];
                    dt.Rows.Add(dr);
                }
                return dt;
            }
        }

        [JsonProperty]
        public List<string> OutputValues
        {            
            get
            {
                if (outputValues == null)
                {
                    outputValues = mAction.ReturnValues.Select(a => a.Param + "_:_" + a.Actual + "_:_" + a.ExpectedCalculated + "_:_" + a.Status).ToList();
                }
                return outputValues;
            }
            set { outputValues = value; }
        }
        private List<string> outputValues;

        [FieldParams]
        [FieldParamsNameCaption("Output Values")]
        [FieldParamsFieldType(FieldsType.Section)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public DataTable OutputValuesDT
        {
            get
            {               
                DataTable dt = new DataTable();
                dt.Columns.Add("ParameterName");
                dt.Columns["ParameterName"].Caption = "Parameter Name";
                dt.Columns.Add("ActualValue");
                dt.Columns["ActualValue"].Caption = "Actual Value";
                dt.Columns.Add("ExpectedValue");
                dt.Columns["ExpectedValue"].Caption = "Expected Value";
                dt.Columns.Add("Status");
                dt.Columns["Status"].Caption = "Status";

                foreach (string outputValues in OutputValues)
                {
                    String[] elementsAfter = outputValues.Split(new string[] { "_:_" }, StringSplitOptions.None);
                    DataRow dr = dt.NewRow();
                    dr["ParameterName"] = elementsAfter[0];
                    dr["ActualValue"] = elementsAfter[1];                    
                    dr["ExpectedValue"] = elementsAfter[2];
                    dr["Status"] = elementsAfter[3];
                    dt.Rows.Add(dr);
                }
                return dt;
            }
        }

        [JsonProperty]
        public List<string> FlowControls
        {
            get
            {
                if (flowControls == null)
                {
                    flowControls = mAction.FlowControls.Select(a => a.Condition + "_:_" + a.ConditionCalculated + "_:_" + a.FlowControlAction + "_:_" + a.Status).ToList();
                }
                return flowControls;
            }
            set { flowControls = value; }
        }
        private List<string> flowControls;

        [FieldParams]
        [FieldParamsNameCaption("Flow Control")]
        [FieldParamsFieldType(FieldsType.Section)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public DataTable FlowControlDT
        {
            get
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("Condition");
                dt.Columns["Condition"].Caption = "Condition";
                dt.Columns.Add("ConditionCalculated");
                dt.Columns["ConditionCalculated"].Caption = "Condition Calculated";
                dt.Columns.Add("Action");
                dt.Columns["Action"].Caption = "Action";
                dt.Columns.Add("Status");
                dt.Columns["Status"].Caption = "Status";

                foreach (string flowControl in FlowControls)
                {
                    String[] elementsAfter = flowControl.Split(new string[] { "_:_" }, StringSplitOptions.None);
                    DataRow dr = dt.NewRow();
                    dr["Condition"] = elementsAfter[0];
                    dr["ConditionCalculated"] = elementsAfter[1];
                    dr["Action"] = elementsAfter[2];
                    dr["Status"] = elementsAfter[3];
                    dt.Rows.Add(dr);
                }
                return dt;
            }
        }

        [FieldParams]
        [FieldParamsNameCaption("Screenshot")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string ScreenShot { get { return mAction != null && mAction.ScreenShots.Count > 0 ? mAction.ScreenShots[0] : string.Empty; } }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("ScreenShots List")]
        [FieldParamsFieldType(FieldsType.Section)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public List<string> ScreenShots { get { return mAction != null ? mAction.ScreenShots : screenShotsList; } set { screenShotsList = value; } }

        public bool Active { get { return mAction.Active; } }

        public string LogFolder { get; set; }

        private string name = string.Empty;
        private string description = string.Empty;
        private string runDescription = string.Empty;
        private string actionType = string.Empty;
        private DateTime startTimeStamp;
        private DateTime endTimeStamp;
        private int currentRetryIteration = 0;
        private string status = string.Empty;
        private string exInfo = string.Empty;
        private string error = string.Empty;
        private long? elapsed = 0;
        public string screenShots = string.Empty;
        public List<string> screenShotsList = new List<string>();
        
        public bool IsPassed { get { return mAction.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed; } }
        public bool IsFailed { get { return mAction.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed; } }
        public bool IsPending { get { return mAction.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending; } }
        public bool IsRunning { get { return mAction.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running; } }
        public bool IsStopped { get { return mAction.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped; } }
        public bool IsSkipped { get { return mAction.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped; } }
        public bool IsBlocked { get { return mAction.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked; } }
   
        public ActionReport(IAct Act, Context context)
        {
            this.mAction = Act;
            mContext = context;
            //this.mExecutionEnviroment = environment;            

        }

        public string LocalFolder
        {
            get { return _localFolder; }
            set { _localFolder = value; }
        }

        private Single? elapsedSecs
        {
            get
            {
                if (Elapsed != null)
                {
                    return ((Single)Elapsed / 1000);
                }
                else
                {
                    if (elapsed != null)
                    {
                        return ((Single)elapsed / 1000);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }

        public List<ReturnValueReport> ReturnValueReport
        {
            get
            {
                List<ReturnValueReport> list = new List<ReturnValueReport>();
                int i = 0;
                foreach (ActReturnValue ARV in mAction.ReturnValues)
                {
                    i++;
                    ReturnValueReport ar = new ReturnValueReport(ARV);
                    ar.Seq = i;
                    list.Add(ar);
                }

                return list;
            }
        }
    
        //todo !!!!!!!!!!!!!!!!!!!! Why??

        private string GetValueForDriverWithoutDescrypting(string value)
        {
            if (mContext != null)
            {               
                ValueExpression VE = new ValueExpression(mContext.Environment, mContext.BusinessFlow, WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>(), false, "", false);
                VE.DecryptFlag = false;
                VE.Value = value;

                return VE.ValueCalculated;
            }
            else
            {
                return value;
            }
        }
  
    }
}
 