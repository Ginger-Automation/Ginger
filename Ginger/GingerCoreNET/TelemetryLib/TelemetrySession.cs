#region License
/*
Copyright © 2014-2022 European Support Limited

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
using Amdocs.Ginger.Common.GeneralLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using Newtonsoft.Json;
using Amdocs.Ginger.Common;

namespace Amdocs.Ginger.CoreNET.TelemetryLib
{
    public class TelemetrySession
    {
        public Guid Id { get; set; }
        public string WorkStationId { get; set; }
        public string TimeZone { get; set; }
        public string GingerVersion { get; set; }
        [JsonIgnore]
        public bool IsUserSession
        {
            get
            {
                if (Debugger == true || Runtime == "Debug")
                {
                    return true;
                    //return false;
                }
                else
                {
                    return true;
                }
            }
        }
        [JsonIgnore]
        public string Runtime { get; set; }
        [JsonIgnore]
        public bool Debugger { get; set; }
        public bool Is64BitProcess { get; set; }
        public string OSVersion { get; set; }
        public bool IsAmdocs { get; set; }
        public string Terminology { get; set; }
        public string UserType { get; set; }
        public string UserRole { get; set; }
        public string ExecutionContext { get; set; }
        public string CliType { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string OverallSessionTime { get; set; }
        [JsonIgnore]
        public double? OverallExecutionTimeNumber { get; set; } = 0;
        public string OverallExecutionTime { get; set; }
        public int OverallExecutedRunsets { get; set; } = 0;
        public int OverallExecutedBusinessFlows { get; set; } = 0;
        public int OverallExecutedActivityGroups { get; set; } = 0;
        public int OverallExecutedActivities { get; set; } = 0;
        public int OverallExecutedActions { get; set; } = 0;
        public int PassedActionsCount { get; set; } = 0;
        public int FailedActionsCount { get; set; } = 0;
        public int PassedBusinessFlowsCount { get; set; } = 0;
        public int FailedBusinessFlowsCount { get; set; } = 0;
        public List<UsedActionDetail> ExecutedActionTypes { get; set; } = new List<UsedActionDetail>();
        public HashSet<string> ExecutedAutomatedPlatforms { get; set; } = new HashSet<string>();
        public List<UsedFeatureDetail> UsedFeatures { get; set; } = new List<UsedFeatureDetail>();
        public List<string> ExceptionErrors { get; set; } = new List<string>();


        public enum GingerExecutionContext
        {
            UI,
            CLI,
            UnitTest
        }

        public enum GingerUsedFeatures
        {
            [Description("POM")]
            POM, //Need to specify when action of POM is running
            [Description("ApiModel")]
            ApiModel, //Done
            [Description("DataSource")]
            DataSource,//Done
            [Description("SharedRepository")]
            SharedRepository,//Done
            [Description("ALM")]
            ALM, //Done
            [Description("SourceControl")]
            SourceControl,//Done
            [Description("AlmImport")]
            AlmImport,//Done
            [Description("AlmExport")]
            AlmExport,//Done
            [Description("SourceControlDownload")]
            SourceControlDownload,//Done
            [Description("SourceControlUpload")]
            SourceControlUpload,//Done
            [Description("Analyzer")]
            Analyzer,//Done
            [Description("SelfHealing")]
            SelfHealing,//Done
            [Description("Search")]
            Search,//Done
            [Description("ParallelExecution")]
            ParallelExecution, //Need to check if it's working correctly
            [Description("RunsetOperationsMailReport")]
            RunsetOperationsMailReport,//Done
            [Description("RunsetOperationsExportToAlm")]
            RunsetOperationsExportToAlm,//Done
            [Description("RunSetActionHTMLReport")]
            RunSetActionHTMLReport,//Done
            [Description("RunSetActionHTMLReportSendEmail")]
            RunSetActionHTMLReportSendEmail,//Done
            [Description("RunSetActionJSONSummaryOperations")]
            RunSetActionJSONSummary,//Done
            [Description("RunSetActionGenerateTestNGReportOperations")]
            RunSetActionGenerateTestNGReport,//Done
            [Description("RunSetActionSendFreeEmailOperations")]
            RunSetActionSendFreeEmail,//Done
            [Description("RunSetActionSendSMSOperations")]
            RunSetActionSendSMS,//Done
            [Description("RunSetActionPublishToQCOperations")]
            RunSetActionPublishToQC,//Done
            [Description("RunSetActionAutomatedALMDefects")]
            RunSetActionAutomatedALMDefects,//Done
            [Description("RunSetActionScriptOperations")]
            RunSetActionScript,//Done
            [Description("RunSetActionSendDataToExternalSourceOperations")]
            RunSetActionSendDataToExternalSource,//Done
            CustomizedReportTemplates,//Done
            [Description("LiteDBLogger")]
            LiteDB,//Done
            [Description("TextFile")]
            TextfileLogger,//Done
            [Description("CentralizedReportLoggerPost")]
            PostExecution,//Done
            [Description("CentralizedReportLoggerDuring")]
            DuringExecution,//Done
            [Description("Tags")]
            Tags,//Done
            [Description("VRT")]
            VRT,//Done
            [Description("Applitools")]
            Applitools, //Done
            [Description("Sealights")]
            Sealights,//Done
            [Description("ModelParameters")]
            ModelParameters,//Done
            [Description("Environments")]
            Environments, //Done
            [Description("GlobalVaraibles")]
            GlobalVaraibles,//Done
            [Description("Documents")]
            Documents,//Done
            [Description("Plugins")]
            Plugins, //Done
            [Description("ActionResultSimulation")]
            ActionResultSimulation 
        }

        public TelemetrySession(Guid guid)
        {
            Id = guid;
            WorkStationId = guid.ToString();
            StartTime = Telemetry.Time;
            TimeZone = TimeZoneInfo.Local.DisplayName;
            GingerVersion = ApplicationInfo.ApplicationVersion;

#if DEBUG
            Runtime = "Debug";
#else
            Runtime = "Release";
#endif

            Debugger = System.Diagnostics.Debugger.IsAttached;
            Is64BitProcess = Environment.Is64BitProcess;
            OSVersion = Environment.OSVersion.ToString();

            if (System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties().DomainName.Contains("amdocs"))
            {
                IsAmdocs = true;
            }
            else
            {
                IsAmdocs = false;
            }

            Assembly assembly = Assembly.GetEntryAssembly();
            if (assembly != null)
            {
                if (assembly.GetName().Name == "testhost")
                {
                    ExecutionContext = GingerExecutionContext.UnitTest.ToString();
                }
                else if (assembly.GetName().Name == "Ginger")
                {
                    ExecutionContext = GingerExecutionContext.UI.ToString();
                }
                else
                {
                    ExecutionContext = GingerExecutionContext.CLI.ToString();
                }
            }
        }

    }
}
