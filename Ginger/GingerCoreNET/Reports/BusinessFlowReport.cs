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

using System;
using System.Collections.Generic;
using System.Linq;
using GingerCore.Variables;
using Newtonsoft.Json;
using System.Data;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Utility;
using Ginger.Run;
using GingerCore;
using Amdocs.Ginger.Run;

namespace Ginger.Reports
{
    //This is special class for report generation
    // it is in separate class from the following reason
    // it is contract - user can change the report we need to keep the fields name
    // it is faster to flatten the data instead of creating sub reports
    // it is more simple to use for users
    // it use get for each item - so on demand calculation from real object = faster
    // We create fields only for the fields we do want to expose, not exposing internal fields
    // we save on memory as we provide ref object and return on demand
    // Json will serialize only marked attr and not all
    enum ActStatus { Passed, Other }
    [JsonObject(MemberSerialization.OptIn)]
    public class BusinessFlowReport
    {
        public static partial class Fields
        {
            public static string Seq = "Seq";
            public static string Name = "Name";
            public static string Description = "Description";
            public static string RunDescription = "RunDescription";
            public static string StartTimeStamp = "StartTimeStamp";
            public static string EndTimeStamp = "EndTimeStamp";
            public static string ExecutionDuration = "ExecutionDuration";
            public static string Elapsed = "Elapsed";
            public static string RunStatus = "RunStatus";
            public static string NumberOfActivities = "NumberOfActivities";
            public static string PassPercent = "PassPercent";
            public static string VariablesDetails = "VariablesDetails";
            public static string ActivityDetails = "ActivityDetails";
            public static string SolutionVariablesDetails = "SolutionVariablesDetails";
            public static string BFFlowControls = "BFFlowControls";
            public static string BFFlowControlDT = "BFFlowControlDT";
        }

        private bool _showAllIterationsElements = false;

        public bool AllIterationElements
        {
            get { return _showAllIterationsElements; }
            set { _showAllIterationsElements = value; }
        }

       private BusinessFlowExecutionSummary mBusinessFlowExecutionSummary;
        private BusinessFlow mBusinessFlow;

        /// <summary>
        /// This tag currently used due to common use of this class by new and old report!!! Should be removed in future!
        /// </summary>
        public bool ExecutionLoggerIsEnabled = false;

        public string LogFolder { get; set; }

        // We use empty constructor when we load from file via Json
        public BusinessFlowReport()
        {
            mBusinessFlow = new BusinessFlow();
        }

        public BusinessFlowReport(BusinessFlowExecutionSummary BusinessFlowExecutionSummary)
        {
            mBusinessFlowExecutionSummary = BusinessFlowExecutionSummary;
            mBusinessFlow = BusinessFlowExecutionSummary.BusinessFlow;
        }

        public BusinessFlowReport(BusinessFlow BF)
        {
            mBusinessFlow = BF;
        }

        // All below are public fields like contract which should not change as user is customizing in their reports 

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Execution Sequence")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(false)]
        [FieldParamsIsSelected(true)]
        public int Seq { get; set; }

        [JsonProperty]
        public string GUID { get { return mBusinessFlow.Guid.ToString(); } }

        [JsonProperty]
        public Guid InstanceGUID { get { return mBusinessFlow.InstanceGuid ; } set { mBusinessFlow.InstanceGuid = value; } }
        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Business Flow Name")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(false)]
        [FieldParamsIsSelected(true)]
        public string Name { get { return mBusinessFlow.Name; } set { mBusinessFlow.Name = value; } }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Business Flow Description")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string Description { get { return mBusinessFlow.Description; } set { mBusinessFlow.Description = value; } }


        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Business Flow Run Description")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string RunDescription { get; set; }

        //[JsonProperty]
        //[FieldParams]
        //[FieldParamsNameCaption("Business Flow Execution Description")]
        //[FieldParamsFieldType(FieldsType.Field)]
        //[FieldParamsIsNotMandatory(true)]
        //[FieldParamsIsSelected(true)]
        //public string ExecutionDescription { get { return mBusinessFlow.RunDescription; } set { mBusinessFlow.RunDescription = value; } } //Duplicated with RunDescription field

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Environment Used")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string Environment { get { return mBusinessFlow.Environment; } set { mBusinessFlow.Environment = value; } }

        // no set since this is calculated from elapsedms
        public float? ElapsedSecs { get { return mBusinessFlow.ElapsedSecs; } }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Execution Start Time")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public DateTime StartTimeStamp { get{ return mBusinessFlow.StartTimeStamp; }  set { mBusinessFlow.StartTimeStamp = value; }
        }
        
        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Execution End Time")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public DateTime EndTimeStamp { get { return mBusinessFlow.EndTimeStamp; } set { mBusinessFlow.EndTimeStamp = value; }}

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Elapsed")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public double? Elapsed { get { return mBusinessFlow.ElapsedSecs; } set { mBusinessFlow.ElapsedSecs = (Single)value; } }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Business Flow Execution Status")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string RunStatus
        {
            get { return mBusinessFlow.RunStatus.ToString(); }
            set { mBusinessFlow.RunStatus = (Amdocs.Ginger.CoreNET.Execution.eRunStatus)Enum.Parse(typeof(Amdocs.Ginger.CoreNET.Execution.eRunStatus), value); }
        }

        public bool IsPassed { get { return mBusinessFlow.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed; } }
        public bool IsFailed { get { return mBusinessFlow.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed; } }
        public bool IsPending { get { return mBusinessFlow.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending; } }
        public bool IsRunning { get { return mBusinessFlow.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running; } }
        public bool IsStopped { get { return mBusinessFlow.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped; } }
        public bool IsSkipped { get { return mBusinessFlow.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped; } }
        public bool IsBlocked { get { return mBusinessFlow.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked; } }

        public BusinessFlow GetBusinessFlow()
        {
            return mBusinessFlow;
        }

        private List<ActivityReport> mActivities = null;
        public List<ActivityReport> Activities
        {
            get
            {

                if (this.ExecutionLoggerIsEnabled)
                {
                    if (mActivities == null)
                    {
                        mActivities = new List<ActivityReport>();
                        // TODO: Load in parallel and verify we keep the original order

                        foreach (string folder in System.IO.Directory.GetDirectories(LogFolder))
                        {
                            try
                            {
                                ActivityReport AR = (ActivityReport)JsonLib.LoadObjFromJSonFile(folder + @"\Activity.txt", typeof(ActivityReport));
                                AR.LogFolder = folder;
                                if (ActivitiesGroupReports != null)    // !!!!!!!!!!!!!!!!!!!!!!!!
                                {
                                    ActivityGroupReport CurrentActivitiesGroupReport = this.ActivitiesGroupReports.Where(x => x.ExecutedActivitiesGUID.Select(y => y.ToString()).Contains(AR.SourceGuid)).ToList().FirstOrDefault();
                                    if (CurrentActivitiesGroupReport != null)
                                    {
                                        AR.ActivityGroupName = CurrentActivitiesGroupReport.Name;
                                        AR.ActivityGroupSeq = CurrentActivitiesGroupReport.Seq;
                                    }
                                }
                                mActivities.Add(AR);
                            }
                            catch { } // !!!!!!!!!!!!!!!!!
                        }
                    }

                    if (_showAllIterationsElements)
                    {
                        var activitiesSortedBySeq = mActivities.OrderBy(item => item.Seq);
                        return (List<ActivityReport>)activitiesSortedBySeq.ToList();
                    }
                    else
                    {
                        var activitiesLastIterationsSortedBySeq = mActivities.GroupBy(x => x.SourceGuid).Select(x => x.Last()).OrderBy(item => item.Seq);
                        return (List<ActivityReport>)activitiesLastIterationsSortedBySeq.ToList();
                    }
                }
                else
                {
                    // below is the old way which is not history //TODO: Delete when flag above is obsolete
                    List<ActivityReport> list = new List<ActivityReport>();
                    int i = 0;
                    if (mBusinessFlow.Activities != null)
                    {
                        foreach (Activity activity in mBusinessFlow.Activities)
                        {
                            i++;
                            ActivityReport ar = new ActivityReport(activity);
                            ar.Seq = i;
                            list.Add(ar);
                        }
                    }
                    return list;
                }
            }
            set { mActivities = value; }
        }

        List<ActivityGroupReport> mActivitiesGroups = null;
        public List<ActivityGroupReport> ActivitiesGroupReports
        {
            get
            {
                if (mActivitiesGroups == null)
                {
                    mActivitiesGroups = new List<ActivityGroupReport>();
                    try
                    {
                        string[] linesActivityGroup = System.IO.File.ReadAllLines(LogFolder + @"\ActivityGroups.txt");
                        foreach (string lineActivityGroup in linesActivityGroup)
                        {
                            ActivityGroupReport ARG = (ActivityGroupReport)JsonLib.LoadObjFromJSonString(lineActivityGroup, typeof(ActivityGroupReport));
                            mActivitiesGroups.Add(ARG);
                        }
                    }
                    catch
                    {
                        mActivitiesGroups = null;
                    }
                }
                return mActivitiesGroups;
            }
            set { mActivitiesGroups = value; }
        }

        public List<VariableReport> Variables
        {
            get
            {
                List<VariableReport> list = new List<VariableReport>();
                int i = 0;
                foreach (VariableBase v in mBusinessFlow.Variables)
                {
                    i++;
                    VariableReport VR = new VariableReport(v);
                    VR.Seq = i;
                    list.Add(VR);
                }

                return list;
            }
        }

        [FieldParams]
        [FieldParamsNameCaption("Number Of Activities")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public int TotalActivities
        {
            get
            {
                return Activities.Count();
            }
        }

        public int TotalActivitiesPassed
        {
            get
            {
                if (this.ExecutionLoggerIsEnabled)
                {
                    int count = (from x in this.Activities where (Amdocs.Ginger.CoreNET.Execution.eRunStatus)Enum.Parse(typeof(Amdocs.Ginger.CoreNET.Execution.eRunStatus), x.RunStatus) == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed select x).Count();
                    return count;
                }
                else
                {
                    int count = (from x in mBusinessFlow.Activities where x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed select x).Count();
                    return count;
                }
            }
        }

        public int TotalActivitiesFailed
        {
            get
            {
                if (this.ExecutionLoggerIsEnabled)
                {
                    int count = (from x in Activities where (Amdocs.Ginger.CoreNET.Execution.eRunStatus)Enum.Parse(typeof(Amdocs.Ginger.CoreNET.Execution.eRunStatus), x.RunStatus) == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed select x).Count();
                    return count;
                }
                else
                {
                    int count = (from x in mBusinessFlow.Activities where x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed select x).Count();
                    return count;
                }
            }
        }

        public int TotalActivitiesStopped
        {
            get
            {
                int count = (from x in Activities where x.Status == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped.ToString() select x).Count();
                return count;
            }
        }

        public int TotalActivitiesOther
        {
            get
            {
                int count = TotalActivities - TotalActivitiesFailed - TotalActivitiesPassed - TotalActivitiesStopped;
                return count;
            }
        }

        [FieldParams]
        [FieldParamsNameCaption("Business Flow Activities Passed Rate")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        
        public double PassPercent { get { return Activities.Count != 0 ? Math.Round((double)TotalActivitiesPassed * 100 / Activities.Count, MidpointRounding.AwayFromZero) + AddOnePercent(ActStatus.Passed) : 0; } }

        public double FailPercent { get { return Activities.Count != 0 ? Math.Round((double)TotalActivitiesFailed * 100 / Activities.Count, MidpointRounding.AwayFromZero) : 0; } }

        public double StoppedPercent { get { return Activities.Count != 0 ? Math.Round((double)TotalActivitiesStopped * 100 / Activities.Count, MidpointRounding.AwayFromZero) : 0; } }
        
        public double OtherPercent { get { return Activities.Count != 0 ? Math.Round((double)TotalActivitiesOther * 100 / Activities.Count, MidpointRounding.AwayFromZero) + AddOnePercent(ActStatus.Other) : 0; } }

        [JsonProperty]
        public List<string> VariablesBeforeExec { get; set; } = new List<string>();

        [JsonProperty]
        public List<string> VariablesAfterExec
        {
            get
            {
                if (variablesAfterExec == null)
                {
                    variablesAfterExec = mBusinessFlow.Variables.Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description).ToList();                    
                }
                return variablesAfterExec;
            }
            set { variablesAfterExec = value; }
        }
        private List<string> variablesAfterExec = new List<string>();

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
                
                    foreach (string variable in VariablesBeforeExec)
                    {
                        String[] elementsAfter = variable.Split(new string[] { "_:_" }, StringSplitOptions.None);
                        DataRow dr = dt.NewRow();
                        dr["Name"] = elementsAfter[0];
                        dr["ValueBeforeExec"] = elementsAfter[1];
                        dr["Description"] = elementsAfter[2];
                        dt.Rows.Add(dr);
                    }
                    foreach (string variable in VariablesAfterExec)
                    {
                        String[] elementsAfter = variable.Split(new string[] { "_:_" }, StringSplitOptions.None);




                    dt.Select("Name  = '" + elementsAfter[0] + "'").FirstOrDefault()["ValueAfterExec"] = elementsAfter[1];
                }
                return dt;
            }
        }

        [JsonProperty]
        public List<string> SolutionVariablesBeforeExec { get; set; }

        [JsonProperty]
        public List<string> SolutionVariablesAfterExec
        {
            get
            {
                if (solutionvariablesAfterExec == null)
                {                    
                    solutionvariablesAfterExec = mBusinessFlow.GetSolutionVariables().Select(a => a.Name + "_:_" + a.Value + "_:_" + a.Description).ToList();                    
                }
                return solutionvariablesAfterExec;
            }
            set { solutionvariablesAfterExec = value; }
        }
        private List<string> solutionvariablesAfterExec;

        [FieldParams]
        [FieldParamsNameCaption("Solution Variables Details")]
        [FieldParamsFieldType(FieldsType.Section)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(false)]
        public DataTable SolutionVariablesDetails
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

                if (SolutionVariablesBeforeExec != null)
                {
                    foreach (string variable in SolutionVariablesBeforeExec)
                    {
                        String[] elementsAfter = variable.Split(new string[] { "_:_" }, StringSplitOptions.None);
                        DataRow dr = dt.NewRow();
                        dr["Name"] = elementsAfter[0];
                        dr["ValueBeforeExec"] = elementsAfter[1];
                        dr["Description"] = elementsAfter[2];
                        dt.Rows.Add(dr);
                    }
                    foreach (string variable in SolutionVariablesAfterExec)
                    {
                        String[] elementsAfter = variable.Split(new string[] { "_:_" }, StringSplitOptions.None);



                        dt.Select("Name  = '" + elementsAfter[0] + "'").FirstOrDefault()["ValueAfterExec"] = elementsAfter[1];
                    }
                }
                return dt;
            }
        }

        [FieldParams]
        [FieldParamsNameCaption("Activities Details")]
        [FieldParamsFieldType(FieldsType.Section)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string ActivityDetails { get; set; }



        //For BF Flow Control
        [JsonProperty]
        public List<string> BFFlowControls
        {
            get
            {
                if (bfFlowControls == null)
                {
                    bfFlowControls = mBusinessFlow.BFFlowControls.Select(a => a.Condition + "_:_" + a.ConditionCalculated + "_:_" + a.BusinessFlowControlAction + "_:_" + a.Status).ToList();
                }
                return bfFlowControls;
            }
            set { bfFlowControls = value; }
        }
        private List<string> bfFlowControls;

        [FieldParams]
        [FieldParamsNameCaption("Business Flow Control")]
        [FieldParamsFieldType(FieldsType.Section)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public DataTable BFFlowControlDT
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

                foreach (string bfFlowControl in BFFlowControls)
                {
                    String[] elementsAfter = bfFlowControl.Split(new string[] { "_:_" }, StringSplitOptions.None);
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
        private int AddOnePercent(ActStatus activitiesStatus)
        {
            double totalActPassed = Math.Round((double)TotalActivitiesPassed * 100 / Activities.Count, MidpointRounding.AwayFromZero);
            if (activitiesStatus.Equals(ActStatus.Other) && totalActPassed > 0) return 0;
            double totalActFailed = Math.Round((double)TotalActivitiesFailed * 100 / Activities.Count, MidpointRounding.AwayFromZero);
            double totalActStopped = Math.Round((double)TotalActivitiesStopped * 100 / Activities.Count, MidpointRounding.AwayFromZero);
            double totalActOther = Math.Round((double)TotalActivitiesOther * 100 / Activities.Count, MidpointRounding.AwayFromZero);
            if ((totalActFailed + totalActPassed + totalActStopped + totalActOther) == 99 && totalActPassed != 0) return 1;
            return 0;
        }
    }
}
