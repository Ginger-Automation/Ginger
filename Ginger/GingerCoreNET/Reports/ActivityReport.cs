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
using Newtonsoft.Json;
using System.IO;
using System.Data;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Utility;
using GingerCore;

namespace Ginger.Reports
{
    // Json will serialize only marked attr and not all
    [JsonObject(MemberSerialization.OptIn)]
    public class ActivityReport
    {
        enum ActStatus {  Passed, Other }
        public static partial class Fields
        {
            public static string ActivityGroupName = "ActivityGroupName";
            public static string Seq = "Seq";
            public static string ActivityName = "ActivityName";
            public static string Description = "Description";
            public static string RunDescription = "RunDescription";
            public static string StartTimeStamp = "StartTimeStamp";
            public static string EndTimeStamp = "EndTimeStamp";
            public static string ElapsedSecs = "ElapsedSecs";
            public static string RunStatus = "RunStatus";
            public static string NumberOfActions = "NumberOfActions";
            public static string PassPercent = "PassPercent";
            public static string VariablesDetails = "VariablesDetails";
            public static string ErrorDetails = "ErrorDetails";
            public static string ExtraDetails = "ExtraDetails";
            public static string ActionsDetails = "ActionsDetails";
        }

        private bool _showAllIterationsElements = false;
        private Activity mActivity;
        private string _localFolder;

        public bool AllIterationElements
        {
            get { return _showAllIterationsElements; }
            set { _showAllIterationsElements = value; }
        }

        // use empty constructor when we load from file - Json
       public ActivityReport()
        {
            mActivity = new Activity();
        }

        public ActivityReport(Activity Activity)
        {
            mActivity = Activity;
        }

        // Put here everything we want to make public for users customizing the reports, never give direct access to the Activity itself.
        // serve as facade to expose only what we want
        // must not change as it will break existing reports, no compile check on XAML

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Execution Sequence")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(false)]
        [FieldParamsIsSelected(true)]
        public int Seq { get; set; }

        [JsonProperty]
        public string GUID { get { return mActivity != null ? mActivity.Guid.ToString() : SourceGuid; } set { SourceGuid = value; } }

        public string SourceGuid = string.Empty;

        // no need to serialize since if it is not active it will not be executed anyhow
        public Boolean Active { get { return mActivity.Active; } }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Activity Group Name")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(false)]
        [FieldParamsIsSelected(true)]
        public string ActivityGroupName { get; set; }
        public int ActivityGroupSeq { get; set; }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Activity Name")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(false)]
        [FieldParamsIsSelected(true)]
        public string ActivityName { get { return mActivity.ActivityName; } set { mActivity.ActivityName = value; }}

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Description")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string Description { get { return mActivity.Description; } set { mActivity.Description = value; } }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Run Description")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string RunDescription { get; set; }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Execution Start Time")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        [UsingUTCTimeFormat]
        public DateTime StartTimeStamp { get { return mActivity.StartTimeStamp; } set { mActivity.StartTimeStamp = value; } }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Execution End Time")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        [UsingUTCTimeFormat]
        public DateTime EndTimeStamp { get { return mActivity.EndTimeStamp; } set { mActivity.EndTimeStamp = value; } }

        [JsonProperty]
        public long? Elapsed { get { return mActivity.Elapsed; } set { mActivity.Elapsed = value; }}

        [FieldParams]
        [FieldParamsNameCaption("Elapsed")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public float? ElapsedSecs { get { return mActivity.ElapsedSecs; } }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Execution Status")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string RunStatus
        {
            get { return mActivity.Status.ToString(); }
            set { mActivity.Status = (Amdocs.Ginger.CoreNET.Execution.eRunStatus)Enum.Parse(typeof(Amdocs.Ginger.CoreNET.Execution.eRunStatus), value); }
        }

        [FieldParams]
        [FieldParamsNameCaption("Number Of Actions")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public int TotalActions
        {
            get
            {
                return ActionReports.Count();
            }
        }

        public int TotalActionsPassed
        {
            get
            {
                int count = (from x in ActionReports where x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed.ToString() select x).Count();
                return count;
            }
        }

        public int TotalActionsStopped
        {
            get
            {
                int count = (from x in ActionReports where x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped.ToString() select x).Count();
                return count;
            }
        }

        public int TotalActionsFailed
        {
            get
            {
                int count = (from x in ActionReports where x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed.ToString() select x).Count();
                return count;
            }
        }

        public int TotalActionsOther
        {
            get
            {
                int count = TotalActions - TotalActionsFailed - TotalActionsPassed - TotalActionsStopped;
                return count;
            }
        }

        [FieldParams]
        [FieldParamsNameCaption("Actions Pass Rate")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public double PassPercent { get { return ActionReports.Count != 0 ? Math.Round((double)TotalActionsPassed * 100 / ActionReports.Count, MidpointRounding.AwayFromZero) + AddOnePercent(ActStatus.Passed) : 0; } }
        
        public double FailPercent { get { return ActionReports.Count != 0 ? Math.Round((double)TotalActionsFailed * 100 / ActionReports.Count, MidpointRounding.AwayFromZero) : 0; } }
        
        public double StoppedPercent { get { return ActionReports.Count != 0 ? Math.Round((double)TotalActionsStopped * 100 / ActionReports.Count, MidpointRounding.AwayFromZero) : 0; } }
        
        public double OtherPercent { get { return ActionReports.Count != 0 ? Math.Round((double)TotalActionsOther * 100 / ActionReports.Count, MidpointRounding.AwayFromZero) + AddOnePercent(ActStatus.Other) : 0; } }

        [JsonProperty]
        public List<string> VariablesBeforeExec { get; set; }

        [JsonProperty]
        public List<string> VariablesAfterExec
        {
            get
            {
                if (variablesAfterExec == null)
                {
                    variablesAfterExec = mActivity.Variables.Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description).ToList();
                }
                return variablesAfterExec;
            }
            set { variablesAfterExec = value; }
        }
        private List<string> variablesAfterExec;
        
        [FieldParams]
        [FieldParamsNameCaption("Variables Details")]
        [FieldParamsFieldType(FieldsType.Section)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public DataTable VariablesDetails
        {
            get
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("Name");
                dt.Columns["Name"].Caption = "Name";
                dt.Columns.Add("Description");
                dt.Columns["Description"].Caption = "Description";
                dt.Columns.Add("ValueBeforeExec");
                dt.Columns["ValueBeforeExec"].Caption = "Value on Execution Start";
                dt.Columns.Add("ValueAfterExec");
                dt.Columns["ValueAfterExec"].Caption = "Value on Execution End";
              
                if(VariablesBeforeExec!=null)
                {
                    foreach (string variable in VariablesBeforeExec)
                    {
                        String[] elementsBefore = variable.Split(new string[] { "_:_" }, StringSplitOptions.None);
                        if (elementsBefore.Count() >= 3)
                        {
                            DataRow dr = dt.NewRow();
                            dr["Name"] = elementsBefore[0];
                            dr["ValueBeforeExec"] = elementsBefore[1];
                            dr["Description"] = elementsBefore[2];
                            dt.Rows.Add(dr);
                        }
                    }
                }     
                if(VariablesAfterExec!=null)
                {
                    foreach (string variable in VariablesAfterExec)
                    {
                        String[] elementsAfter = variable.Split(new string[] { "_:_" }, StringSplitOptions.None);
                        if (elementsAfter.Count() >= 2)
                        {
                          

                            dt.Select("Name = '" + elementsAfter[0]+"'").FirstOrDefault()["ValueAfterExec"] = elementsAfter[1];
                          
                        }
                    }                
                }                    
                return dt;
            }
        }

        [FieldParams]
        [FieldParamsNameCaption("Actions Details")]
        [FieldParamsFieldType(FieldsType.Section)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string ActionsDetails { get; set; }

        public string LogFolder { get; set; }

        // no need to Json serialized, for backward compatibility
        public string Status { get { return mActivity.Status.ToString(); } }

        public int ExecutionLogActionsCounter { get; set; }

        public string LocalFolder
        {
            get { return _localFolder; }
            set { _localFolder = value; }
        }

        private List<ActionReport> actionReports;
        public List<ActionReport> ActionReports
        {
            get
            {
                actionReports = new List<ActionReport>();
                if (LogFolder != null)
                {
                    foreach (string folder in System.IO.Directory.GetDirectories(LogFolder))
                    {
                        FileAttributes attr = File.GetAttributes(folder);
                        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            try
                            {
                                ActionReport actrR = (ActionReport)JsonLib.LoadObjFromJSonFile(Path.Combine(folder,"Action.txt"), typeof(ActionReport));
                                actrR.LogFolder = folder;
                                actionReports.Add(actrR);
                            }
                            catch (Exception ECx)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, "Error De-serializing ActivityReport", ECx);


                            }
                        }
                    }
                }
                if (_showAllIterationsElements)
                {
                    var actionReportsSortedBySeq = actionReports.OrderBy(item => item.Seq);
                    return (List<ActionReport>)actionReportsSortedBySeq.ToList();
                }
                else
                {
                    var actionReportsLastIterationsSortedBySeq = actionReports.GroupBy(x => x.GUID).Select(x => x.Last()).OrderBy(item => item.Seq);
                    return (List<ActionReport>)actionReportsLastIterationsSortedBySeq.ToList();
                }
            }
        }

        public List<ActionReport> Actions
        {
            get
            {
                if ((mActivity.Acts != null) && (mActivity.Acts.Count > 0))
                {
                    List<ActionReport> list = new List<ActionReport>();
                    int i = 0;
                    foreach (IAct a in mActivity.Acts)
                    {
                        i++;
                        ActionReport ar = new ActionReport(a, null);//need to provide valid Context
                        ar.Seq = i;
                        list.Add(ar);
                    }

                    return list;
                }
                else
                {
                    return new List<ActionReport>();
                }

            }
        }
        private int AddOnePercent(ActStatus actionsStatus)
        {
            double totalActPassed = Math.Round((double)TotalActionsPassed * 100 / ActionReports.Count, MidpointRounding.AwayFromZero);
            if (actionsStatus.Equals(ActStatus.Other) && totalActPassed > 0) return 0;
            double totalActFailed = Math.Round((double)TotalActionsFailed * 100 / ActionReports.Count, MidpointRounding.AwayFromZero);
            double totalActStopped = Math.Round((double)TotalActionsStopped * 100 / ActionReports.Count, MidpointRounding.AwayFromZero);
            double totalActOther = Math.Round((double)TotalActionsOther * 100 / ActionReports.Count, MidpointRounding.AwayFromZero);
            if ((totalActFailed + totalActPassed + totalActStopped + totalActOther) == 99 && totalActPassed != 0) return 1;
            return 0;
        }
    }
}
