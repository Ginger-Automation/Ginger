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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Amdocs.Ginger.CoreNET.LiteDBFolder
{


    // TODO: split each report class to its own file !!!

    public class LiteDbReportBase
    {
        [FieldParams]
        [FieldParamsNameCaption("Seq")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public int Seq { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("Guid")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public Guid GUID { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("Name")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string Name { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("Description")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string Description { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("RunDescription")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string RunDescription { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("Environment")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string Environment { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("StartTimeStamp")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public DateTime StartTimeStamp { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("EndTimeStamp")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public DateTime EndTimeStamp { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("Elapsed")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public double? Elapsed { get; set; } = 0.0;

        [FieldParams]
        [FieldParamsNameCaption("RunStatus")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string RunStatus { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("VariablesBeforeExec")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public List<string> VariablesBeforeExec { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("VariablesAfterExec")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public List<string> VariablesAfterExec { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("ExecutionId")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public LiteDB.ObjectId _id { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("ExecutionRate")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string ExecutionRate { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("PassRate")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string PassRate { get; set; }

        public LiteDbReportBase()
        {
            _id = LiteDB.ObjectId.NewObjectId();
        }
        public void SetReportData(RepositoryItemBase item)
        { }
        public string SetStatus<T>(List<T> reportColl)
        {
            if (reportColl.Any(rp => (rp as LiteDbReportBase).RunStatus.Equals(eRunStatus.Stopped.ToString())))
            {
                return eRunStatus.Stopped.ToString();
            }
            if (reportColl.Any(rp => (rp as LiteDbReportBase).RunStatus.Equals(eRunStatus.Failed.ToString())))
            {
                return eRunStatus.Failed.ToString();
            }
            if (reportColl.Any(rp => (rp as LiteDbReportBase).RunStatus.Equals(eRunStatus.Blocked.ToString())))
            {
                return eRunStatus.Blocked.ToString();
            }
            if (reportColl.Count(rp => (rp as LiteDbReportBase).RunStatus.Equals(eRunStatus.Passed.ToString()) || (rp as LiteDbReportBase).RunStatus.Equals(eRunStatus.Skipped.ToString())) == reportColl.Count())
            {
                return eRunStatus.Passed.ToString();
            }
            return eRunStatus.Pending.ToString();
        }

        public void RemoveObjFromLiteDB(Object o)
        {
            if(o is Amdocs.Ginger.Repository.RepositoryItemBase && (o as Amdocs.Ginger.Repository.RepositoryItemBase).LiteDbId != null)
            {
                LiteDbManager liteDbManager = new LiteDbManager(WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder);
                if (o.GetType().FullName.Contains("Actions"))
                {
                    var actionData = liteDbManager.GetActionsLiteData();
                    actionData.Delete((o as Amdocs.Ginger.Repository.RepositoryItemBase).LiteDbId);
                }
            }
            return;
        }
    }
    public class LiteDbRunSet : LiteDbReportBase
    {
        [FieldParams]
        [FieldParamsNameCaption("GingerVersion")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string GingerVersion { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("MachineName")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string MachineName { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("ExecutedbyUser")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string ExecutedbyUser { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("Runners")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public List<LiteDbRunner> RunnersColl { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("ChildExecutableItemsCount")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public Dictionary<string, int> ChildExecutableItemsCount { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("ChildExecutedItemsCount")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public Dictionary<string, int> ChildExecutedItemsCount { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("ChildPassedItemsCount")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public Dictionary<string, int> ChildPassedItemsCount { get; set; }
        public LiteDbRunSet()
        {
            RunnersColl = new List<LiteDbRunner>();
            ChildExecutableItemsCount = new Dictionary<string, int>();
            ChildExecutedItemsCount = new Dictionary<string, int>();
            ChildPassedItemsCount = new Dictionary<string, int>();
        }

        internal void SetReportData(RunSetReport runSetReport)
        {
            Seq = runSetReport.Seq;
            GUID = Guid.Parse(runSetReport.GUID);
            Name = runSetReport.Name;
            Description = runSetReport.Description;
            Environment = runSetReport.EnvironmentsDetails;
            StartTimeStamp = runSetReport.StartTimeStamp;
            EndTimeStamp = runSetReport.EndTimeStamp;
            Elapsed = runSetReport.Elapsed;
            MachineName = System.Environment.MachineName.ToString();
            ExecutedbyUser = System.Environment.UserName.ToString();
            GingerVersion = ApplicationInfo.ApplicationVersion;
            RunStatus = (runSetReport.RunSetExecutionStatus == eRunStatus.Automated) ? eRunStatus.Automated.ToString() : SetStatus(RunnersColl);
        }
    }
    public class LiteDbRunner : LiteDbReportBase
    {

        [FieldParams]
        [FieldParamsNameCaption("ApplicationAgentsMappingList")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public List<string> ApplicationAgentsMappingList { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("BusinessFlows")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public List<LiteDbBusinessFlow> BusinessFlowsColl { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("ChildExecutableItemsCount")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public Dictionary<string, int> ChildExecutableItemsCount { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("ChildExecutedItemsCount")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public Dictionary<string, int> ChildExecutedItemsCount { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("ChildPassedItemsCount")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public Dictionary<string, int> ChildPassedItemsCount { get; set; }
        public LiteDbRunner()
        {
            BusinessFlowsColl = new List<LiteDbBusinessFlow>();
            ChildExecutableItemsCount = new Dictionary<string, int>();
            ChildExecutedItemsCount = new Dictionary<string, int>();
            ChildPassedItemsCount = new Dictionary<string, int>();
        }

        internal void SetReportData(GingerReport gingerReport)
        {
            Seq = gingerReport.Seq;
            GUID = Guid.Parse(gingerReport.GUID);
            Name = gingerReport.Name;
            Description = gingerReport.Description;
            Environment = gingerReport.EnvironmentName;
            StartTimeStamp = gingerReport.StartTimeStamp;
            EndTimeStamp = gingerReport.EndTimeStamp;
            Elapsed = gingerReport.Elapsed;
            ApplicationAgentsMappingList = gingerReport.ApplicationAgentsMappingList;
            RunStatus = SetStatus(BusinessFlowsColl);
        }
    }
    public class LiteDbBusinessFlow : LiteDbReportBase
    {
        [FieldParams]
        [FieldParamsNameCaption("InstanceGUID")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public Guid InstanceGUID { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("SolutionVariablesBeforeExec")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public List<string> SolutionVariablesBeforeExec { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("SolutionVariablesAfterExec")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public List<string> SolutionVariablesAfterExec { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("Activities")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public List<LiteDbActivity> ActivitiesColl { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("ActivitiesGroups")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public List<LiteDbActivityGroup> ActivitiesGroupsColl { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("BFFlowControlDT")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public List<string> BFFlowControlDT { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("ChildExecutableItemsCount")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public Dictionary<string, int> ChildExecutableItemsCount { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("ChildExecutedItemsCount")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public Dictionary<string, int> ChildExecutedItemsCount { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("ChildPassedItemsCount")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public Dictionary<string, int> ChildPassedItemsCount { get; set; }
        public LiteDbBusinessFlow()
        {
            ActivitiesGroupsColl = new List<LiteDbActivityGroup>();
            ActivitiesColl = new List<LiteDbActivity>();
            ChildExecutableItemsCount = new Dictionary<string, int>();
            ChildExecutedItemsCount = new Dictionary<string, int>();
            ChildPassedItemsCount = new Dictionary<string, int>();
        }
        public void SetReportData(BusinessFlowReport bfReport)
        {
            this.Seq = bfReport.Seq;
            this.GUID = Guid.Parse(bfReport.GUID);
            InstanceGUID = bfReport.InstanceGUID;
            this.Name = bfReport.Name;
            this.Description = bfReport.Description;
            RunDescription = bfReport.RunDescription;
            Environment = bfReport.Environment;
            StartTimeStamp = bfReport.StartTimeStamp;
            EndTimeStamp = bfReport.EndTimeStamp;
            Elapsed = bfReport.Elapsed;
            this.RunStatus = bfReport.RunStatus; 
            VariablesBeforeExec = bfReport.VariablesBeforeExec;
            VariablesAfterExec = bfReport.VariablesAfterExec;
            SolutionVariablesBeforeExec = bfReport.SolutionVariablesBeforeExec;
            SolutionVariablesAfterExec = bfReport.SolutionVariablesAfterExec;
            BFFlowControlDT = bfReport.BFFlowControls;
        }
    }

    public class LiteDbActivityGroup : LiteDbReportBase
    {
        [FieldParams]
        [FieldParamsNameCaption("ExecutedActivitiesGUID")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public List<Guid> ExecutedActivitiesGUID { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("AutomationPrecentage")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string AutomationPrecentage { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("Activities")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public List<LiteDbActivity> ActivitiesColl { get; set; }

        public LiteDbActivityGroup()
        {
            ActivitiesColl = new List<LiteDbActivity>();
        }

        internal void SetReportData(ActivityGroupReport agReport)
        {
            //this.Seq = businessFlow.ActivitiesGroups.IndexOf(activityGroup) + 1;
            //this.ExecutionLogFolder = executionLogFolder + bf.ExecutionLogFolder;
            Seq = agReport.Seq;
            GUID = Guid.Parse(agReport.GUID);
            Name = agReport.Name;
            Description = agReport.Description;
            AutomationPrecentage = agReport.AutomationPrecentage;
            StartTimeStamp = agReport.StartTimeStamp;
            EndTimeStamp = agReport.EndTimeStamp;
            Elapsed = agReport.Elapsed;
            this.RunStatus = agReport.RunStatus;
            ExecutedActivitiesGUID = agReport.ExecutedActivitiesGUID;
        }
    }
    public class LiteDbActivity : LiteDbReportBase
    {
        [FieldParams]
        [FieldParamsNameCaption("ActivityGroupName")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string ActivityGroupName { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("Actions")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public List<LiteDbAction> ActionsColl { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("ChildExecutableItemsCount")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public int ChildExecutableItemsCount { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("ChildExecutedItemsCount")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public int ChildExecutedItemsCount { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("ChildPassedItemsCount")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public int ChildPassedItemsCount { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("ErrorDetails")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string ErrorDetails { get; set; }

        public LiteDbActivity()
        {
            ActionsColl = new List<LiteDbAction>();
        }

        public void SetReportData(ActivityReport activityReport)
        {
            Seq = activityReport.Seq;
            GUID = Guid.Parse(activityReport.GUID);
            Name = activityReport.ActivityName;
            Description = activityReport.Description;
            RunDescription = activityReport.RunDescription;
            StartTimeStamp = activityReport.StartTimeStamp;
            EndTimeStamp = activityReport.EndTimeStamp;
            Elapsed = activityReport.Elapsed;
            RunStatus = activityReport.RunStatus;
            VariablesAfterExec = activityReport.VariablesAfterExec;
            VariablesBeforeExec = activityReport.VariablesBeforeExec;
        }
    }
    
    public class LiteDbAction : LiteDbReportBase
    {
        [FieldParams]
        [FieldParamsNameCaption("ActionType")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string ActionType { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("CurrentRetryIteration")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public int CurrentRetryIteration { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("Error")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string Error { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("ExInfo")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string ExInfo { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("InputValues")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public List<string> InputValues { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("OutputValues")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public List<string> OutputValues { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("FlowControls")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public List<string> FlowControls { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("ScreenShots")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public List<string> ScreenShots { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("Wait")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public int Wait { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("TimeOut")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public int? TimeOut { get; set; }

        public LiteDbAction()
        {

        }
        public void SetReportData(ActionReport actionReport)
        {
            Seq = actionReport.Seq;
            GUID = Guid.Parse(actionReport.GUID);
            Name = actionReport.Name;
            ActionType = actionReport.ActionType;
            Description = actionReport.Description;
            RunDescription = actionReport.RunDescription;
            ActionType = actionReport.ActionType;
            StartTimeStamp = actionReport.StartTimeStamp;
            EndTimeStamp = actionReport.EndTimeStamp;
            Elapsed = actionReport.Elapsed;
            RunStatus = actionReport.Status;
            InputValues = new List<string>(actionReport.InputValues);
            OutputValues = new List<string>(actionReport.OutputValues);
            FlowControls = new List<string>(actionReport.FlowControls);
            CurrentRetryIteration = actionReport.CurrentRetryIteration;
            Error = actionReport.Error;
            ExInfo = actionReport.ExInfo;
        }
    }
}
