using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using Ginger.Reports.GingerExecutionReport;
using GingerCore.FlowControlLib;
using Ginger.Reports;

namespace Amdocs.Ginger.CoreNET.LiteDBFolder
{
    public class LiteDbReportBase
    {
        public int Seq { get; set; }
        public Guid GUID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string RunDescription { get; set; }
        public string Environment { get; set; }
        public DateTime StartTimeStamp { get; set; }
        public DateTime EndTimeStamp { get; set; }
        public double? Elapsed { get; set; }
        public string RunStatus { get; set; }
        public List<string> VariablesBeforeExec { get; set; }
        public List<string> VariablesAfterExec { get; set; }      
        public LiteDB.ObjectId _id { get; set; }
        public LiteDbReportBase()
        {
            _id = LiteDB.ObjectId.NewObjectId();
        }
        public void SetReportData(RepositoryItemBase item)
        { }
    }
    public class LiteDbRunSet : LiteDbReportBase
    {
        public string GingerVersion { get; set; }
        public string MachineName { get; set; }
        public string ExecutedbyUser { get; set; }
        public List<LiteDbRunner> RunnersColl { get; set; }
        public LiteDbRunSet()
        {
            RunnersColl = new List<LiteDbRunner>();
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
            GingerVersion = WorkSpace.AppVersion.ToString();
            RunStatus = runSetReport.RunSetExecutionStatus.ToString();
        }
    }
    public class LiteDbRunner : LiteDbReportBase
    {
        public List<string> ApplicationAgentsMappingList { get; set; }
        public List<LiteDbBusinessFlow> BusinessFlowsColl { get; set; }
        public LiteDbRunner()
        {
            BusinessFlowsColl = new List<LiteDbBusinessFlow>();
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
            RunStatus = gingerReport.GingerExecutionStatus.ToString();
        }
    }
    public class LiteDbBusinessFlow : LiteDbReportBase
    {
        public Guid InstanceGUID { get; set; }
        public List<string> SolutionVariablesBeforeExec { get; set; }
        public List<string> SolutionVariablesAfterExec { get; set; }
        public List<LiteDbActivity> ActivitiesColl { get; set; }
        public List<LiteDbActivityGroup> ActivitiesGroupsColl { get; set; }
        public List<string> BFFlowControlDT { get; set; }
        public LiteDbBusinessFlow()
        {
            ActivitiesGroupsColl = new List<LiteDbActivityGroup>();
            ActivitiesColl = new List<LiteDbActivity>();
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
        public List<Guid> ExecutedActivitiesGUID { get; set; }
        public string AutomationPrecentage { get; set; }
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
        public string ActivityGroupName { get; set; }
        public List<LiteDbAction> ActionsColl { get; set; }
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
            //VariablesBeforeExec = activityReport.VariablesBeforeExec;
        }
    }
    public class LiteDbAction : LiteDbReportBase
    {
        public string ActionType { get; set; }
        public int CurrentRetryIteration { get; set; }
        public string Error { get; set; }
        public string ExInfo { get; set; }
        public List<string> InputValues { get; set; }
        public List<string> OutputValues { get; set; }
        public List<string> FlowControls { get; set; }
        public List<string> ScreenShots { get; set; }
        public int Wait { get; set; }
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
