#region License
/*
Copyright © 2014-2024 European Support Limited

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

using AccountReport.Contracts.Helpers;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using LiteDB;
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
        public string SetStatus<T>(IEnumerable<T> reportColl)
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
            if (o is Amdocs.Ginger.Repository.RepositoryItemBase && (o as Amdocs.Ginger.Repository.RepositoryItemBase).LiteDbId != null)
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
            RunnersColl = [];
            ChildExecutableItemsCount = [];
            ChildExecutedItemsCount = [];
            ChildPassedItemsCount = [];
        }

        public void SetAllIterationElementsRecursively(bool value)
        {
            foreach (LiteDbRunner runner in RunnersColl)
            {
                if (runner != null)
                {
                    runner.SetAllIterationElementsRecursively(value);
                }
            }
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
            GingerVersion = ApplicationInfo.ApplicationUIversion;
            RunStatus = (runSetReport.RunSetExecutionStatus == eRunStatus.Automated) ? eRunStatus.Automated.ToString() : SetStatus(RunnersColl);
            RunDescription = runSetReport.RunDescription;
        }
        public static ILiteCollection<LiteDbRunSet> IncludeAllReferences(ILiteCollection<LiteDbRunSet> liteCollection)
        {
            return liteCollection
                .Include(runset => runset.RunnersColl)
                .Include(runset => runset.RunnersColl.Select((runner) => runner.AllBusinessFlowsColl))
                .Include(runset => runset.RunnersColl.Select((runner) => runner.AllBusinessFlowsColl.Select(businessFlow => businessFlow.AllActivitiesColl)))
                .Include(runset => runset.RunnersColl.Select((runner) => runner.AllBusinessFlowsColl.Select(businessFlow => businessFlow.ActivitiesGroupsColl)))
                .Include(runset => runset.RunnersColl.Select((runner) => runner.AllBusinessFlowsColl.Select(businessFlow => businessFlow.AllActivitiesColl.Select(activity => activity.AllActionsColl))))
                .Include(runset => runset.RunnersColl.Select((runner) => runner.AllBusinessFlowsColl.Select(businessFlow => businessFlow.ActivitiesGroupsColl.Select(activityGroup => activityGroup.AllActivitiesColl.Select(activity => activity.AllActionsColl)))))
                ;

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

        public bool AllIterationElements { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("BusinessFlows")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public IReadOnlyList<LiteDbBusinessFlow> BusinessFlowsColl
        {
            get
            {
                if (AllIterationElements || !AllBusinessFlowsColl.Any())
                {
                    return AllBusinessFlowsColl;
                }
                else
                {
                    return AllBusinessFlowsColl.GroupBy(b => b.InstanceGUID).Select(group => group.Last()).OrderBy(b => b.Seq).ToList().AsReadOnly();
                }
            }
        }

        public List<LiteDbBusinessFlow> AllBusinessFlowsColl { get; set; }

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
            AllBusinessFlowsColl = [];
            ChildExecutableItemsCount = [];
            ChildExecutedItemsCount = [];
            ChildPassedItemsCount = [];
        }

        public void SetAllIterationElementsRecursively(bool value)
        {
            AllIterationElements = value;
            foreach (LiteDbBusinessFlow businessFlow in BusinessFlowsColl)
            {
                if (businessFlow != null)
                {
                    businessFlow.SetAllIterationElementsRecursively(value);
                }
            }
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
        public static ILiteCollection<LiteDbRunner> IncludeAllReferences(ILiteCollection<LiteDbRunner> liteCollection)
        {
            return liteCollection
                .Include((runner) => runner.BusinessFlowsColl)
                .Include((runner) => runner.AllBusinessFlowsColl.Select((businessFlow) => businessFlow.AllActivitiesColl.Select((activity) => activity.AllActionsColl)))
                .Include((runner) => runner.AllBusinessFlowsColl.Select((businessFlow) => businessFlow.ActivitiesGroupsColl.Select((activityGroup) => activityGroup.AllActivitiesColl.Select((activity) => activity.AllActionsColl))));
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

        public bool AllIterationElements { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("Activities")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public IReadOnlyList<LiteDbActivity> ActivitiesColl
        {
            get
            {
                if (AllIterationElements || !AllActivitiesColl.Any())
                {
                    return AllActivitiesColl;
                }
                else
                {
                    return AllActivitiesColl.GroupBy(a => a.GUID).Select(group => group.Last()).OrderBy(a => a.Seq).ToList();
                }
            }
        }

        public List<LiteDbActivity> AllActivitiesColl { get; set; }

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

        [FieldParams]
        [FieldParamsNameCaption("Mapped ALM Entity ID")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string ExternalID { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("Mapped ALM Entity ID 2")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string ExternalID2 { get; set; }

        public LiteDbBusinessFlow()
        {
            ActivitiesGroupsColl = [];
            AllActivitiesColl = [];
            ChildExecutableItemsCount = [];
            ChildExecutedItemsCount = [];
            ChildPassedItemsCount = [];
        }

        public void SetAllIterationElementsRecursively(bool value)
        {
            AllIterationElements = value;
            foreach (LiteDbActivity activity in ActivitiesColl)
            {
                if (activity != null)
                {
                    activity.SetAllIterationElementsRecursively(value);
                }
            }
        }

        public void SetReportData(BusinessFlowReport bfReport)
        {
            AllIterationElements = bfReport.AllIterationElements;
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
            ExternalID = bfReport.ExternalID;
            ExternalID2 = bfReport.ExternalID2;

        }
        public static ILiteCollection<LiteDbBusinessFlow> IncludeAllReferences(ILiteCollection<LiteDbBusinessFlow> liteCollection)
        {
            return liteCollection
                .Include((businessFlow) => businessFlow.ActivitiesGroupsColl)
                .Include((businessFlow) => businessFlow.AllActivitiesColl)
                .Include((businessFlow) => businessFlow.ActivitiesGroupsColl.Select((activityGroup) => activityGroup.AllActivitiesColl.Select((activity) => activity.AllActionsColl)))
                .Include((businessFlow) => businessFlow.AllActivitiesColl.Select((activity) => activity.AllActionsColl));
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

        public bool AllIterationElements { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("Activities")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public IReadOnlyList<LiteDbActivity> ActivitiesColl
        {
            get
            {
                if (AllIterationElements || !AllActivitiesColl.Any())
                {
                    return AllActivitiesColl;
                }
                else
                {
                    return AllActivitiesColl.GroupBy(a => a.GUID).Select(group => group.Last()).OrderBy(a => a.Seq).ToList();
                }
            }
        }

        public List<LiteDbActivity> AllActivitiesColl { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("Mapped ALM Entity ID")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string ExternalID { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("Mapped ALM Entity ID 2")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string ExternalID2 { get; set; }


        public LiteDbActivityGroup()
        {
            AllActivitiesColl = [];
        }

        public void SetAllIterationElementsRecursively(bool value)
        {
            AllIterationElements = value;
            foreach (LiteDbActivity activity in ActivitiesColl)
            {
                if (activity != null)
                {
                    activity.SetAllIterationElementsRecursively(value);
                }
            }
        }

        internal void SetReportData(ActivityGroupReport agReport)
        {
            //this.Seq = businessFlow.ActivitiesGroups.IndexOf(activityGroup) + 1;
            //this.ExecutionLogFolder = executionLogFolder + bf.ExecutionLogFolder;
            AllIterationElements = agReport.AllIterationElements;
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
            ExternalID = agReport.ExternalID;
            ExternalID2 = agReport.ExternalID2;
        }
        public static ILiteCollection<LiteDbActivityGroup> IncludeAllReferences(ILiteCollection<LiteDbActivityGroup> liteCollection)
        {
            return liteCollection
                .Include((activityGroup) => activityGroup.AllActivitiesColl)
                .Include((activityGroup) => activityGroup.AllActivitiesColl.Select((activity) => activity.AllActionsColl));
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

        public bool AllIterationElements { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("Actions")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public IReadOnlyList<LiteDbAction> ActionsColl
        {
            get
            {
                if (AllIterationElements || !AllActionsColl.Any())
                {
                    return AllActionsColl;
                }
                else
                {
                    return AllActionsColl.GroupBy(a => a.GUID).Select(group => group.Last()).OrderBy(a => a.Seq).ToList();
                }
            }
        }

        public List<LiteDbAction> AllActionsColl { get; set; }

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

        [FieldParams]
        [FieldParamsNameCaption("Mapped ALM Entity ID")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string ExternalID { get; set; }

        [FieldParams]
        [FieldParamsNameCaption("Mapped ALM Entity ID 2")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public string ExternalID2 { get; set; }


        public LiteDbActivity()
        {
            AllActionsColl = [];
        }

        public void SetAllIterationElementsRecursively(bool value)
        {
            AllIterationElements = value;
        }

        public void SetReportData(ActivityReport activityReport)
        {
            AllIterationElements = activityReport.AllIterationElements;
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
            ExternalID = activityReport.ExternalID;
            ExternalID2 = activityReport.ExternalID2;
        }
        public static ILiteCollection<LiteDbActivity> IncludeAllReferences(ILiteCollection<LiteDbActivity> liteCollection)
        {
            return liteCollection.Include((activity) => activity.AllActionsColl);
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
        [FieldParamsNameCaption("Artifacts")]
        [FieldParamsFieldType(FieldsType.Field)]
        [FieldParamsIsNotMandatory(true)]
        [FieldParamsIsSelected(true)]
        public IList<DictObject> Artifacts { get; set; }

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
