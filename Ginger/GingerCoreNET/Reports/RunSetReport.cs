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

using System;
using System.Collections.Generic;
using System.Linq;

using Newtonsoft.Json;
using System.IO;
using Amdocs.Ginger.CoreNET.Utility;
using Amdocs.Ginger.CoreNET.LiteDBFolder;

namespace Ginger.Reports
{
    [JsonObject(MemberSerialization.OptIn)]
    public class RunSetReport
    {
        public static partial class Fields
        {
            public static string Name = "Name";
            public static string Description = "Description";
            public static string StartTimeStamp = "StartTimeStamp";
            public static string EndTimeStamp = "EndTimeStamp";
            public static string Elapsed = "Elapsed";
            public static string ExecutionDuration = "ExecutionDuration";
            public static string RunSetExecutionStatus = "RunSetExecutionStatus";
            public static string GingerRunnersPassRate = "GingerRunnersPassRate";
            public static string RunSetExecutionRate = "RunSetExecutionRate";
            public static string EnvironmentsDetails = "EnvironmentsDetails";
            public static string ExecutionStatisticsDetails = "ExecutionStatisticsDetails";
            public static string ExecutedBusinessFlowsDetails = "ExecutedBusinessFlowsDetails";
            public static string ExecutedActivitiesDetails = "ExecutedActivitiesDetails";
            public static string FailuresDetails = "FailuresDetails";
            public static string DataRepMethod = "DataRepMethod";
        }

        public int Seq { get; set; }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("RunSet Name")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(false)]
        [FieldParamsIsSelected(true)]
        public string Name { get; set; }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("RunSet Description")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string Description { get; set; }
        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Execution Start Time")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public DateTime StartTimeStamp { get; set; }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Execution End Time")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public DateTime EndTimeStamp { get; set; }
        
        [JsonProperty]
        public string GUID { get; set; }

        public float? ElapsedSecs { get; set; }


        [FieldParams]
        [FieldParamsNameCaption("Execution Duration")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public double? ExecutionDuration
        {
            get
            {
                return Elapsed;
            }
        }

        [FieldParams]
        [FieldParamsNameCaption("RunSet Execution Pass Rate")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string GingerRunnersPassRate { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("RunSet Execution Rate")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string RunSetExecutionRate { get; set; }
        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Used Environment/s")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string EnvironmentsDetails { get; set; }
        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Executed on Ginger Version")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string GingerVersion { get; set; }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Executed on Machine")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string MachineName { get; set; }

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Executed by User")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string ExecutedbyUser { get; set; }


        [FieldParams]
        [FieldParamsNameCaption("Execution Statistics Details")]
        [FieldParamsFieldType(FieldsType.Section)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string ExecutionStatisticsDetails { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("Executed Business Flows Details")]
        [FieldParamsFieldType(FieldsType.Section)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string ExecutedBusinessFlowsDetails { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("Executed Activities Details")]
        [FieldParamsFieldType(FieldsType.Section)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(false)]
        public string ExecutedActivitiesDetails { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("Failed Actions Details")]
        [FieldParamsFieldType(FieldsType.Section)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string FailuresDetails { get; set; }

        [JsonProperty]
        public double? Elapsed { get; set; }

        public string LogFolder { get; set; }

        public System.Diagnostics.Stopwatch Watch = new System.Diagnostics.Stopwatch();
        public ExecutionLoggerConfiguration.DataRepositoryMethod DataRepMethod { get; set; }
        private List<GingerReport> gingerReports;
        public List<GingerReport> GingerReports
        {
            get
            {
                gingerReports = new List<GingerReport>();
                if (LogFolder != null)
                {
                    foreach (string folder in System.IO.Directory.GetDirectories(LogFolder))
                    {
                        FileAttributes attr = File.GetAttributes(folder);
                        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            try
                            {
                                GingerReport gr = (GingerReport)JsonLib.LoadObjFromJSonFile(Path.Combine(folder,"Ginger.txt"), typeof(GingerReport));
                                gr.LogFolder = folder;
                                gingerReports.Add(gr);
                            }
                            catch { }
                        }
                    }
                }
                return gingerReports;
            }
        }
        private Amdocs.Ginger.CoreNET.Execution.eRunStatus runSetExecutionStatus;

        public void SetDataForAutomateTab()
        {
            Name = "Automate Run Set";
            GUID = Guid.NewGuid().ToString();
            StartTimeStamp = DateTime.Now.ToUniversalTime();
            RunSetExecutionStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Automated;
            DataRepMethod = ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB;
        }

        public Amdocs.Ginger.CoreNET.Execution.eRunStatus RunSetExecutionStatus
        {
            get
            {
                if (DataRepMethod == ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB)
                {
                    return runSetExecutionStatus;
                }
                if (TotalGingerRunnersFailed > 0)
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                }
                else if ((from x in GingerReports where x.IsBlocked == true select x).Count() > 0)
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked;
                }
                else if ((from x in GingerReports where x.IsStopped == true select x).Count() > 0)
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped;
                }
                else if ((from x in GingerReports where (x.IsPassed == true || x.IsSkipped == true) select x).Count() == TotalGingerRunners)
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed;
                }
                else
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
                }
            }
            set
            {
                runSetExecutionStatus = value;
            }
        }

        public void SetLiteDBData(LiteDbRunSet runSet)
        {
            GUID = runSet._id.ToString();
            Name = runSet.Name;
            Description = runSet.Description;
            StartTimeStamp = runSet.StartTimeStamp;
            EndTimeStamp = runSet.EndTimeStamp;
            Elapsed = runSet.Elapsed;
            Amdocs.Ginger.CoreNET.Execution.eRunStatus myStatus;
            if(Enum.TryParse(runSet.RunStatus, out myStatus))
            {
                RunSetExecutionStatus = myStatus;
            }
        }

        public int TotalGingerRunners
        {
            get
            {
                return GingerReports.Count();
            }
        }

        public double GingerRunnersFailRate
        {
            get
            {
                return TotalGingerRunnersFailed * 100 / TotalGingerRunners;
            }
        }

        public double GingerRunnersStoppedRate
        {
            get
            {
                return TotalGingerRunnersStopped * 100 / TotalGingerRunners;
            }
        }

        public double GingerRunnersOtherRate
        {
            get
            {
                return TotalGingerRunnersOther * 100 / TotalGingerRunners;
            }
        }

        public int TotalGingerRunnersPassed
        {
            get
            {
                int count = (from x in GingerReports where x.IsPassed == true select x).Count();
                return count;
            }
        }

        public int TotalGingerRunnersFailed
        {
            get
            {
                int count = (from x in GingerReports where x.IsFailed == true select x).Count();
                return count;
            }
        }

        public int TotalGingerRunnersStopped
        {
            get
            {
                int count = (from x in GingerReports where x.IsStopped select x).Count();
                return count;
            }
        }

        public int TotalGingerRunnersOther
        {
            get
            {
                int count = TotalGingerRunners - TotalGingerRunnersFailed - TotalGingerRunnersPassed - TotalGingerRunnersStopped;
                return count;
            }
        }

        public int TotalGingerRunnersBlocked
        {
            get
            {
                return (TotalGingerRunners - (TotalGingerRunnersFailed + TotalGingerRunnersPassed));
            }
        }
        public List<LiteDbRunner> liteDbRunnerList = new List<LiteDbRunner>();
    }
}
