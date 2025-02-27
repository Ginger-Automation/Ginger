#region License
/*
Copyright © 2014-2025 European Support Limited

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

using Amdocs.Ginger.CoreNET.LiteDBFolder;
using Amdocs.Ginger.CoreNET.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Ginger.Reports
{
    [JsonObject(MemberSerialization.OptIn)]
    public class RunSetReport
    {
        public static partial class Fields
        {
            public static string Name = "Name";
            public static string Description = "Description";
            public static string SourceApplication = "SourceApplication";
            public static string SourceApplicationUser = "SourceApplicationUser";
            public static string StartTimeStamp = "StartTimeStamp";
            public static string EndTimeStamp = "EndTimeStamp";
            public static string Elapsed = "Elapsed";
            public static string ExecutionDuration = "ExecutionDuration";
            public static string ExecutionDurationHHMMSS = "ExecutionDurationHHMMSS";
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
        public string GUID { get; set; }

        public Guid RunSetGuid { get; set; }

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

        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Requested From")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string SourceApplication { get; set; }


        [JsonProperty]
        [FieldParams]
        [FieldParamsNameCaption("Requested By")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string SourceApplicationUser { get; set; }

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

        public string ExecutionDurationHHMMSS { get; set; }


        public string LogFolder { get; set; }

        public System.Diagnostics.Stopwatch Watch = new System.Diagnostics.Stopwatch();
        public ExecutionLoggerConfiguration.DataRepositoryMethod DataRepMethod { get; set; }
        private List<GingerReport> gingerReports;
        public List<GingerReport> GingerReports
        {
            get
            {
                gingerReports = [];
                if (LogFolder != null)
                {
                    foreach (string folder in System.IO.Directory.GetDirectories(LogFolder))
                    {
                        FileAttributes attr = File.GetAttributes(folder);
                        if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                        {
                            try
                            {
                                GingerReport gr = (GingerReport)JsonLib.LoadObjFromJSonFile(Path.Combine(folder, "Ginger.txt"), typeof(GingerReport));
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
        public string RunDescription { get; set; }
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
                if (DataRepMethod is ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB or ExecutionLoggerConfiguration.DataRepositoryMethod.Remote)
                {
                    return runSetExecutionStatus;
                }
                if (TotalGingerRunnersFailed > 0)
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
                }
                else if (GingerReports.Any(x => x.IsBlocked))
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked;
                }
                else if (GingerReports.Any(x => x.IsStopped))
                {
                    return Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped;
                }
                else if (GingerReports.Count(x => x.IsPassed || x.IsSkipped) == TotalGingerRunners)
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
            RunSetGuid = runSet.GUID;
            Name = runSet.Name;
            Description = runSet.Description;
            StartTimeStamp = runSet.StartTimeStamp;
            EndTimeStamp = runSet.EndTimeStamp;
            Elapsed = runSet.Elapsed;
            ExecutionDurationHHMMSS = GingerCoreNET.GeneralLib.General.TimeConvert(Elapsed.ToString());
            Amdocs.Ginger.CoreNET.Execution.eRunStatus myStatus;
            if (Enum.TryParse(runSet.RunStatus, out myStatus))
            {
                RunSetExecutionStatus = myStatus;
            }
        }

        public int TotalGingerRunners
        {
            get
            {
                return GingerReports.Count;
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
                return GingerReports.Count(x => x.IsPassed);
            }
        }

        public int TotalGingerRunnersFailed
        {
            get
            {
                return GingerReports.Count(x => x.IsFailed);
            }
        }

        public int TotalGingerRunnersStopped
        {
            get
            {
                return GingerReports.Count(x => x.IsStopped);
            }
        }

        public int TotalGingerRunnersOther
        {
            get
            {
                return TotalGingerRunners - TotalGingerRunnersFailed - TotalGingerRunnersPassed - TotalGingerRunnersStopped;
            }
        }

        public int TotalGingerRunnersBlocked
        {
            get
            {
                return TotalGingerRunners - (TotalGingerRunnersFailed + TotalGingerRunnersPassed);
            }
        }
        public List<LiteDbRunner> liteDbRunnerList = [];
    }
}
