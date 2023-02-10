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

using Amdocs.Ginger.CoreNET.Utility;
using Ginger.Run;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace Ginger.Reports
{
    [JsonObject(MemberSerialization.OptIn)]
    public class GingerReport
    {
        public static partial class Fields
        {
            public static string Seq = "Seq";
            public static string Name = "Name";
            public static string Description = "Description";
            public static string ExecutionDescription = "ExecutionDescription";
            public static string EnvironmentName = "EnvironmentName";
            public static string StartTimeStamp = "StartTimeStamp";
            public static string EndTimeStamp = "EndTimeStamp";
            public static string Elapsed = "Elapsed";
            public static string GingerExecutionStatus = "GingerExecutionStatus";
            public static string NumberOfBusinessFlows = "NumberOfBusinessFlows";
            public static string BusinessFlowsPassRate = "BusinessFlowsPassRate";
            public static string BusinessFlowsDetails = "BusinessFlowsDetails";
            public static string ApplicationAgentsMapping = "ApplicationAgentsMapping";
        }

        public GingerRunner GingerRunner { get; set; }

        private bool _showAllIterationsElements = false;
        public bool AllIterationElements
        {
            get { return _showAllIterationsElements; }
            set { _showAllIterationsElements = value; }
        }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Business Flow Execution Sequence")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(false)]
        [FieldParamsIsSelected(true)]
        public int Seq { get; set; }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Ginger Runner Name")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(false)]
        [FieldParamsIsSelected(true)]
        public string Name { get; set; }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Description")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string Description { get; set; }

        [JsonProperty]
        public string GUID { get; set; }

        public float? ElapsedSecs { get; set; }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Ginger Runner Environment Name")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string EnvironmentName { get; set; }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Execution Start Time")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        [UsingUTCTimeFormat]
        public DateTime StartTimeStamp { get; set; }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Execution End Time")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        [UsingUTCTimeFormat]
        public DateTime EndTimeStamp { get; set; }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Elapsed")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public double? Elapsed { get; set; }

        private List<BusinessFlowReport> businessFlowReports;
        public List<BusinessFlowReport> BusinessFlowReports
        {
            get
            {
                businessFlowReports = new List<BusinessFlowReport>();
                if (System.IO.Directory.Exists(LogFolder))
                {
                    foreach (string folder in System.IO.Directory.GetDirectories(LogFolder))
                    {
                        FileAttributes attr = File.GetAttributes(folder);
                        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            try
                            {
                                BusinessFlowReport br = (BusinessFlowReport)JsonLib.LoadObjFromJSonFile(Path.Combine(folder,"BusinessFlow.txt"), typeof(BusinessFlowReport));
                                br.LogFolder = folder;
                                br.ExecutionLoggerIsEnabled = true;
                                businessFlowReports.Add(br);
                            }
                            catch { }
                        }
                    }
                }
                if (_showAllIterationsElements)
                {
                    var businessFlowSortedBySeq = businessFlowReports.OrderBy(item => item.Seq);
                    return (List<BusinessFlowReport>)businessFlowSortedBySeq.ToList();
                }
                else
                {
                    var businessFlowLastIterationSortedBySeq = businessFlowReports.GroupBy(x => x.InstanceGUID).Select(x => x.Last()).OrderBy(item => item.Seq);
                    return (List<BusinessFlowReport>)businessFlowLastIterationSortedBySeq.ToList();
                }
            }
        }

        public string LogFolder { get; set; }

        public int ExecutionLogBussinesFlowsCounter { get; set; }

        // Remove watch !!!!!!!!!!!!!!!!!
        public System.Diagnostics.Stopwatch Watch = new System.Diagnostics.Stopwatch();

        [FieldParams]
        [FieldParamsNameCaption("Execution Status")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public Amdocs.Ginger.CoreNET.Execution.eRunStatus GingerExecutionStatus
        {
            get
            {
                if (BusinessFlowReports != null && BusinessFlowReports.Count > 0)
                {
                    if (TotalBusinessFlowsFailed > 0)
                    {
                        return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                    }
                    else if ((from x in BusinessFlowReports where x.IsBlocked == true select x).Count() > 0)
                    {
                        return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked;
                    }
                    else if ((from x in BusinessFlowReports where x.IsStopped == true select x).Count() > 0)
                    {
                        return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped;
                    }
                    else if ((from x in BusinessFlowReports where (x.IsPassed == true || x.IsSkipped == true) select x).Count() == TotalBusinessFlows)
                    {
                        return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
                    }
                    else
                    {
                        return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
                    }
                }
                else if(GingerRunner != null)
                {
                    return GingerRunner.Status;
                }
                else
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
                }
            }
        }
        public bool IsPassed { get { return GingerExecutionStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed; } }
        public bool IsFailed { get { return GingerExecutionStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed; } }
        public bool IsPending { get { return GingerExecutionStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending; } }
        public bool IsRunning { get { return GingerExecutionStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Running; } }
        public bool IsStopped { get { return GingerExecutionStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped; } }
        public bool IsSkipped { get { return GingerExecutionStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped; } }
        public bool IsBlocked { get { return GingerExecutionStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked; } }

        [FieldParams]
        [FieldParamsNameCaption("Number Of Business Flows")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public int TotalBusinessFlows
        {
            get
            {
                if (BusinessFlowReports != null && BusinessFlowReports.Count > 0)
                {
                    return BusinessFlowReports.Count();
                }
                else if (GingerRunner != null)
                {
                    return GingerRunner.Executor.BusinessFlows.Where(x => x.Active == true).ToList().Count();
                }
                else
                {
                    return 0;
                }
            }
        }

        [FieldParams]
        [FieldParamsNameCaption("Business Flows Pass Rate")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public double BusinessFlowsPassRate
        {
            get
            {
                if (TotalBusinessFlows != 0)
                    return TotalBusinessFlowsPassed * 100 / TotalBusinessFlows;
                else
                    return 0;
            }
        }

        [JsonProperty]
        public List<string> ApplicationAgentsMappingList { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("Target Application - Agents Mapping")]
        [FieldParamsFieldType(FieldsType.Section)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public DataTable ApplicationAgentsMapping
        {
            get
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("TargetApplication");
                dt.Columns["TargetApplication"].Caption = "Target Application";
                dt.Columns.Add("Agents");
                dt.Columns["Agents"].Caption = "Agents Mapping";

                if (ApplicationAgentsMappingList != null)
                {
                    foreach (string mapping in ApplicationAgentsMappingList)
                    {
                        String[] elementsAfter = mapping.Split(new string[] { "_:_" }, StringSplitOptions.None);
                        DataRow dr = dt.NewRow();
                        dr["TargetApplication"] = elementsAfter[0];
                        dr["Agents"] = elementsAfter[1];
                        dt.Rows.Add(dr);
                    }
                }
                return dt;
            }
        }

        [FieldParams]
        [FieldParamsNameCaption("Business Flow Details")]
        [FieldParamsFieldType(FieldsType.Section)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string BusinessFlowsDetails { get; set; }

        public double BusinessFlowsFailRate
        {
            get
            {
                return TotalBusinessFlowsFailed * 100 / TotalBusinessFlows;
            }
        }


        public double BusinessFlowsStoppedRate
        {
            get
            {
                return TotalBusinessFlowsStopped * 100 / TotalBusinessFlows;
            }
        }


        public double BusinessFlowsOtherRate
        {
            get
            {
                return TotalBusinessFlowsOther * 100 / TotalBusinessFlows;
            }
        }

        public int TotalBusinessFlowsPassed
        {
            get
            {
                int count = 0;
                if (BusinessFlowReports != null && BusinessFlowReports.Count > 0)
                {
                    count = (from x in BusinessFlowReports where x.IsPassed == true select x).Count();
                }
                else if (GingerRunner != null)
                {
                    count = (from x in GingerRunner.Executor.BusinessFlows where x.RunStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed select x).Count();
                }
                return count;
            }
        }

        public int TotalBusinessFlowsFailed
        {
            get
            {
                int count = (from x in BusinessFlowReports where x.IsFailed == true select x).Count();
                return count;
            }
        }

        public int TotalBusinessFlowsStopped
        {
            get
            {
                int count = (from x in BusinessFlowReports where x.IsStopped select x).Count();
                return count;
            }
        }

        public int TotalBusinessFlowsOther
        {
            get
            {
                int count = TotalBusinessFlows - TotalBusinessFlowsFailed - TotalBusinessFlowsPassed - TotalBusinessFlowsStopped;
                return count;
            }
        }

        public int TotalBusinessFlowsBlocked
        {
            get
            {
                return (TotalBusinessFlows - (TotalBusinessFlowsFailed + TotalBusinessFlowsPassed));
            }
        }

    }
}
