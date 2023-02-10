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

using AccountReport.Contracts;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib.CenteralizedExecutionLogger;
using Ginger.Reports;
using Ginger.Run;
using GingerCore;
using GingerCore.Activities;
using LiteDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ginger.Reports.ExecutionLoggerConfiguration;

namespace Amdocs.Ginger.CoreNET.Run.RunListenerLib
{
    public class LiteDBRepository : ExecutionLogger
    {
        public LiteDbManager liteDbManager;

        public List<LiteDbRunSet> liteDbRunSetList = new List<LiteDbRunSet>();


        public List<LiteDbBusinessFlow> liteDbBFList = new List<LiteDbBusinessFlow>();
        public List<LiteDbActivityGroup> liteDbAGList = new List<LiteDbActivityGroup>();
        public List<LiteDbActivity> liteDbActivityList = new List<LiteDbActivity>();
        public List<LiteDbAction> liteDbActionList = new List<LiteDbAction>();
        private eRunStatus lastBfStatus;
        private eRunStatus lastRunnertStatus;
        private ObjectId lastBfObjId;
        private int actionSeq = 0;
        private int activitySeq = 0;
        private int acgSeq = 0;
        private int bfSeq = 0;
        // private int runsetSeq = 0;

        public LiteDBRepository()
        {
            liteDbManager = new LiteDbManager(executionLoggerHelper.GetLoggerDirectory(WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder));
        }

        public override void SaveObjToReporsitory(object obj, string FileName = "", bool toAppend = false)
        {
            liteDbManager.WriteToLiteDb(FileName, new List<LiteDbReportBase>() { (LiteDbReportBase)obj });
        }
        public override object SetReportAction(GingerCore.Actions.Act action, Context context, Amdocs.Ginger.Common.eExecutedFrom executedFrom, bool offlineMode = false)
        {
            //save screenshots
            string executionLogFolder = executionLoggerHelper.GetLoggerDirectory(WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder);
            string completeSSPath = string.Empty;
            int screenShotCountPerAction = 0;
            for (var s = 0; s < action.ScreenShots.Count; s++)
            {
                try
                {
                    screenShotCountPerAction++;
                    string imagesFolderName = Path.Combine(executionLogFolder, "LiteDBImages");
                    var screenShotName = string.Concat(@"ScreenShot_", action.Guid, "_", action.StartTimeStamp.ToString("hhmmss"), "_" + screenShotCountPerAction.ToString(), ".png");

                    completeSSPath = Path.Combine(imagesFolderName, screenShotName);
                    if (!System.IO.Directory.Exists(imagesFolderName))
                    {
                        System.IO.Directory.CreateDirectory(imagesFolderName);
                    }
                    if (executedFrom == Amdocs.Ginger.Common.eExecutedFrom.Automation)
                    {
                        System.IO.File.Copy(action.ScreenShots[s], completeSSPath, true);
                    }
                    else
                    {
                        if (File.Exists(completeSSPath))
                        {
                            continue;
                        }
                        File.Move(action.ScreenShots[s], completeSSPath);
                        action.ScreenShots[s] = completeSSPath;
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to move screen shot with path: " + completeSSPath + " of the action:'" + action.Description + "' to the Execution Logger folder", ex);
                    screenShotCountPerAction--;
                }
            }

            return GetActionReportData(action, context, executedFrom);//Returning ActionReport so we will get execution info on the console
        }

        private object MapActionToLiteDb(GingerCore.Actions.Act action, Context context, eExecutedFrom executedFrom)
        {
            bool isActExsits = false;
            LiteDbAction liteDbAction = new LiteDbAction();
            liteDbAction.SetReportData(GetActionReportData(action, context, executedFrom));
            liteDbAction.Seq = ++this.actionSeq;
            liteDbAction.Wait = action.Wait;
            liteDbAction.TimeOut = action.Timeout;
            if (action.LiteDbId != null && executedFrom == eExecutedFrom.Automation)
            {
                liteDbAction._id = action.LiteDbId;
            }

            //change the paths to Defect suggestion list
            var defectSuggestion = WorkSpace.Instance.RunsetExecutor.DefectSuggestionsList.FirstOrDefault(z => z.FailedActionGuid == action.Guid);
            if (defectSuggestion != null)
            {
                defectSuggestion.ScreenshotFileNames = action.ScreenShots.ToList();
            }

            liteDbAction.ScreenShots = action.ScreenShots.ToList();

            isActExsits = liteDbActionList.Any(x => x.GUID == liteDbAction.GUID);
            if (isActExsits)
            {
                liteDbActionList.RemoveAll(x => x.GUID == liteDbAction.GUID);
            }
            liteDbActionList.Add(liteDbAction);
            SaveObjToReporsitory(liteDbAction, liteDbManager.NameInDb<LiteDbAction>());
            if (executedFrom == eExecutedFrom.Automation)
            {
                action.LiteDbId = liteDbAction._id;
            }
            return liteDbAction;
        }

        public override object SetReportActivity(Activity activity, Context context, bool offlineMode = false, bool isConfEnable = false)
        {
            //return new LiteDbActivity();
            return GetActivityReportData(activity, context, offlineMode);//Returning ActivityReport so we will get execution info on the console
        }

        private object MapActivityToLiteDb(Activity activity, Context context, eExecutedFrom executedFrom)
        {
            LiteDbActivity AR = new LiteDbActivity();
            context.Runner.CalculateActivityFinalStatus(activity);
            AR.SetReportData(GetActivityReportData(activity, context, false));
            AR.ActivityGroupName = activity.ActivitiesGroupID;
            AR.Seq = ++this.activitySeq;
            actionSeq = 0;
            if (activity.LiteDbId != null && ExecutionLoggerManager.RunSetReport != null && ExecutionLoggerManager.RunSetReport.RunSetExecutionStatus == Execution.eRunStatus.Automated) // missing Executed from
            {
                AR._id = activity.LiteDbId;
                var ARToUpdate = liteDbManager.GetActivitiesLiteData().IncludeAll().Find(x => x._id == AR._id).ToList();
                if (ARToUpdate.Count > 0)
                {
                    foreach (var action in (ARToUpdate[0] as LiteDbActivity).ActionsColl)
                    {
                        if (liteDbActionList.Any(ac => ac._id == action._id))
                        {
                            liteDbActionList.RemoveAll(x => x._id == action._id);
                        }
                    }
                    liteDbActionList.AddRange((ARToUpdate[0] as LiteDbActivity).ActionsColl);
                }
            }
            activity.Acts.ToList().ForEach(action => this.MapActionToLiteDb((GingerCore.Actions.Act)action, context, executedFrom));
            AR.ActionsColl.AddRange(liteDbActionList);

            AR.ChildExecutableItemsCount = activity.Acts.Count(x => x.Active && (x.Status == eRunStatus.Passed || x.Status == eRunStatus.Failed || x.Status == eRunStatus.FailIgnored || x.Status == eRunStatus.Blocked));
            AR.ChildExecutedItemsCount = activity.Acts.Count(x => x.Status == eRunStatus.Passed || x.Status == eRunStatus.Failed || x.Status == eRunStatus.FailIgnored);
            AR.ChildPassedItemsCount = activity.Acts.Count(x => x.Status == eRunStatus.Passed);

            liteDbActivityList.Add(AR);
            SaveObjToReporsitory(AR, liteDbManager.NameInDb<LiteDbActivity>());
            liteDbActionList.Clear();
            if (ExecutionLoggerManager.RunSetReport != null && ExecutionLoggerManager.RunSetReport.RunSetExecutionStatus == Execution.eRunStatus.Automated)
            {
                activity.LiteDbId = AR._id;
            }
            return AR;
        }

        public override object SetReportActivityGroup(ActivitiesGroup activityGroup, BusinessFlow businessFlow, bool offlineMode)
        {
            //return new LiteDbActivityGroup();
            return GetAGReportData(activityGroup, businessFlow);//Returning ActivityGroupReport so we will get execution info on the console
        }

        private object MapAcgToLiteDb(ActivitiesGroup activityGroup, BusinessFlow businessFlow)
        {
            LiteDbActivityGroup AGR = new LiteDbActivityGroup();
            AGR.SetReportData(GetAGReportData(activityGroup, businessFlow));
            AGR.Seq = ++this.acgSeq;
            if (activityGroup.LiteDbId != null && ExecutionLoggerManager.RunSetReport != null && ExecutionLoggerManager.RunSetReport.RunSetExecutionStatus == Execution.eRunStatus.Automated)
            {
                AGR._id = activityGroup.LiteDbId;
            }
            AGR.ActivitiesColl = liteDbActivityList.Where(ac => ac.ActivityGroupName != null && ac.ActivityGroupName.Equals(AGR.Name)).ToList();
            SaveObjToReporsitory(AGR, liteDbManager.NameInDb<LiteDbActivityGroup>(), true);
            liteDbAGList.Add(AGR);
            if (ExecutionLoggerManager.RunSetReport != null && ExecutionLoggerManager.RunSetReport.RunSetExecutionStatus == Execution.eRunStatus.Automated)
            {
                activityGroup.LiteDbId = AGR._id;
            }
            return AGR;
        }

        public override object SetReportBusinessFlow(Context context, bool offlineMode, Amdocs.Ginger.Common.eExecutedFrom executedFrom, bool isConfEnable)
        {
            LiteDbBusinessFlow BFR = new LiteDbBusinessFlow();
            if (executedFrom == eExecutedFrom.Automation)
                ClearSeq();

            if (liteDbBFList.Count > context.Runner.BusinessFlows.Count)
            {
                liteDbBFList.RemoveRange(0, context.Runner.BusinessFlows.Count);
            }
            if (lastBfStatus == eRunStatus.Stopped)
            {
                BFR._id = lastBfObjId;
                ClearSeq();
            }
            SetBfobjects(context, executedFrom);
            context.Runner.CalculateBusinessFlowFinalStatus(context.BusinessFlow);
            BFR.SetReportData(GetBFReportData(context.BusinessFlow, context.Environment));
            BFR.Seq = ++this.bfSeq;

            int ChildExecutableItemsCountActivity = 0;
            int ChildExecutedItemsCountActivity = 0;
            int ChildPassedItemsCountActivity = 0;

            ChildExecutableItemsCountActivity = context.BusinessFlow.Activities.Count(x => x.Active && (x.Status == eRunStatus.Passed || x.Status == eRunStatus.Failed || x.Status == eRunStatus.FailIgnored || x.Status == eRunStatus.Blocked));
            ChildExecutedItemsCountActivity = context.BusinessFlow.Activities.Count(ac => ac.Status == eRunStatus.Failed || ac.Status == eRunStatus.Passed || ac.Status == eRunStatus.FailIgnored);
            ChildPassedItemsCountActivity = context.BusinessFlow.Activities.Count(ac => ac.Status == eRunStatus.Passed);

            BFR.ChildExecutableItemsCount.Add(HTMLReportConfiguration.eExecutionStatisticsCountBy.Activities.ToString(), ChildExecutableItemsCountActivity);
            BFR.ChildExecutedItemsCount.Add(HTMLReportConfiguration.eExecutionStatisticsCountBy.Activities.ToString(), ChildExecutedItemsCountActivity);
            BFR.ChildPassedItemsCount.Add(HTMLReportConfiguration.eExecutionStatisticsCountBy.Activities.ToString(), ChildPassedItemsCountActivity);

            int ChildExecutableItemsCountAction = 0;
            int ChildExecutedItemsCountAction = 0;
            int ChildPassedItemsCountAction = 0;
            foreach (LiteDbActivity activity in liteDbActivityList)
            {
                ChildExecutableItemsCountAction = ChildExecutableItemsCountAction + activity.ChildExecutableItemsCount;
                ChildExecutedItemsCountAction = ChildExecutedItemsCountAction + activity.ChildExecutedItemsCount;
                ChildPassedItemsCountAction = ChildPassedItemsCountAction + activity.ChildPassedItemsCount;
            }
            BFR.ChildExecutableItemsCount.Add(HTMLReportConfiguration.eExecutionStatisticsCountBy.Actions.ToString(), ChildExecutableItemsCountAction);
            BFR.ChildExecutedItemsCount.Add(HTMLReportConfiguration.eExecutionStatisticsCountBy.Actions.ToString(), ChildExecutedItemsCountAction);
            BFR.ChildPassedItemsCount.Add(HTMLReportConfiguration.eExecutionStatisticsCountBy.Actions.ToString(), ChildPassedItemsCountAction);

            if (context.BusinessFlow.LiteDbId != null && executedFrom == eExecutedFrom.Automation)
            {
                BFR._id = context.BusinessFlow.LiteDbId;
                var BFRToUpdate = liteDbManager.GetBfLiteData().IncludeAll().Find(x => x._id == BFR._id).ToList();
                if (BFRToUpdate.Count > 0)
                {
                    foreach (var activity in (BFRToUpdate[0] as LiteDbBusinessFlow).ActivitiesColl)
                    {
                        if (liteDbActivityList.Any(ac => ac._id == activity._id))
                        {
                            liteDbActivityList.RemoveAll(x => x._id == activity._id);
                        }
                    }
                    liteDbActivityList.AddRange((BFRToUpdate[0] as LiteDbBusinessFlow).ActivitiesColl);
                }
            }
            if (WorkSpace.Instance.Solution.LoggerConfigurations.ExecutionLoggerConfigurationIsEnabled)
            {
                if (offlineMode)
                {
                    // To check whether the execution is from Runset/Automate tab
                    if ((executedFrom == Amdocs.Ginger.Common.eExecutedFrom.Automation))
                    {
                        context.BusinessFlow.ExecutionFullLogFolder = context.BusinessFlow.ExecutionLogFolder;
                    }
                    else if ((WorkSpace.Instance.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder != null))
                    {
                        context.BusinessFlow.ExecutionFullLogFolder = context.BusinessFlow.ExecutionLogFolder;
                    }
                    BFR.ActivitiesColl.AddRange(liteDbActivityList);
                    BFR.ActivitiesGroupsColl.AddRange(liteDbAGList);
                    SaveObjToReporsitory(BFR, liteDbManager.NameInDb<LiteDbBusinessFlow>());
                    liteDbActivityList.Clear();
                    liteDbAGList.Clear();
                }
                else
                {
                    BFR.ActivitiesColl.AddRange(liteDbActivityList);
                    BFR.ActivitiesGroupsColl.AddRange(liteDbAGList);
                    SaveObjToReporsitory(BFR, liteDbManager.NameInDb<LiteDbBusinessFlow>());
                    this.lastBfObjId = BFR._id;
                    if (liteDbBFList.Exists(bf => bf._id == this.lastBfObjId))
                        liteDbBFList.RemoveAll(bf => bf._id == this.lastBfObjId);
                    liteDbBFList.Add(BFR);
                    liteDbActivityList.Clear();
                    liteDbAGList.Clear();
                    //context.BusinessFlow.ExecutionFullLogFolder = Path.Combine(ExecutionLogfolder,context.BusinessFlow.ExecutionLogFolder);
                }
                if (executedFrom == Amdocs.Ginger.Common.eExecutedFrom.Automation)
                {
                    this.ExecutionLogBusinessFlowsCounter = 0;
                    //this.BFCounter = 0;
                }
            }
            if (executedFrom == eExecutedFrom.Automation)
            {
                context.BusinessFlow.LiteDbId = BFR._id;
            }
            this.lastBfStatus = context.BusinessFlow.RunStatus;

            //return BFR;
            return GetBFReportData(context.BusinessFlow, context.Environment);//Returning BusinessFlowReport so we will get execution info on the console
        }

        private void ClearSeq()
        {
            actionSeq = 0;
            activitySeq = 0;
            bfSeq = 0;
            // runsetSeq = 0;
            acgSeq = 0;
        }

        private void SetBfobjects(Context context, eExecutedFrom executedFrom)
        {
            activitySeq = 0;
            var bf = context.BusinessFlow;
            bf.Activities.ToList().ForEach(activity => this.MapActivityToLiteDb(activity, context, executedFrom));
            bf.ActivitiesGroups.ToList().ForEach(acg => this.MapAcgToLiteDb(acg, bf));
        }

        public override void SetReportRunner(GingerExecutionEngine gingerRunner, GingerReport gingerReport, ParentGingerData gingerData, Context mContext, string filename, int runnerCount)
        {
            base.SetReportRunner(gingerRunner, gingerReport, gingerData, mContext, filename, runnerCount);
            LiteDbRunner runner = new LiteDbRunner();
            runner.BusinessFlowsColl.AddRange(liteDbBFList);
            if (lastRunnertStatus == eRunStatus.Stopped && gingerRunner.RunsetStatus != eRunStatus.Stopped && runner.BusinessFlowsColl.Count > gingerRunner.BusinessFlows.Count)
            {
                runner.BusinessFlowsColl.RemoveRange(0, gingerRunner.BusinessFlows.Count);
            }

            SetRunnerChildCounts(runner, gingerRunner.BusinessFlows);

            runner.SetReportData(gingerReport);
            SaveObjToReporsitory(runner, liteDbManager.NameInDb<LiteDbRunner>());
            if (ExecutionLoggerManager.RunSetReport == null)
            {
                ExecutionLoggerManager.RunSetReport = new RunSetReport();
                ExecutionLoggerManager.RunSetReport.GUID = Guid.NewGuid().ToString();
            }
            if (lastRunnertStatus == Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped)
            {
                var runnerItem = ExecutionLoggerManager.RunSetReport.liteDbRunnerList.Find(x => x.Name == runner.Name);
                ExecutionLoggerManager.RunSetReport.liteDbRunnerList.Remove(runnerItem);
            }
            if (runner.RunStatus != eRunStatus.Stopped.ToString())
            {
                liteDbBFList.Clear();
            }
            ExecutionLoggerManager.RunSetReport.liteDbRunnerList.Add(runner);
            lastRunnertStatus = gingerRunner.RunsetStatus;
            ClearSeq();
        }
        private void SetRunnerChildCounts(LiteDbRunner runner, ObservableList<BusinessFlow> businessFlows)
        {
            int ChildExecutableItemsCountActivity = 0;
            int ChildExecutedItemsCountActivity = 0;
            int ChildPassedItemsCountActivity = 0;
            int ChildExecutableItemsCountAction = 0;
            int ChildExecutedItemsCountAction = 0;
            int ChildPassedItemsCountAction = 0;
            foreach (LiteDbBusinessFlow businessFlow in liteDbBFList)
            {
                int count = 0;
                businessFlow.ChildExecutableItemsCount.TryGetValue(HTMLReportConfiguration.eExecutionStatisticsCountBy.Activities.ToString(), out count);
                ChildExecutableItemsCountActivity = ChildExecutableItemsCountActivity + count;

                businessFlow.ChildExecutedItemsCount.TryGetValue(HTMLReportConfiguration.eExecutionStatisticsCountBy.Activities.ToString(), out count);
                ChildExecutedItemsCountActivity = ChildExecutedItemsCountActivity + count;

                businessFlow.ChildPassedItemsCount.TryGetValue(HTMLReportConfiguration.eExecutionStatisticsCountBy.Activities.ToString(), out count);
                ChildPassedItemsCountActivity = ChildPassedItemsCountActivity + count;

                businessFlow.ChildExecutableItemsCount.TryGetValue(HTMLReportConfiguration.eExecutionStatisticsCountBy.Actions.ToString(), out count);
                ChildExecutableItemsCountAction = ChildExecutableItemsCountAction + count;

                businessFlow.ChildExecutedItemsCount.TryGetValue(HTMLReportConfiguration.eExecutionStatisticsCountBy.Actions.ToString(), out count);
                ChildExecutedItemsCountAction = ChildExecutedItemsCountAction + count;

                businessFlow.ChildPassedItemsCount.TryGetValue(HTMLReportConfiguration.eExecutionStatisticsCountBy.Actions.ToString(), out count);
                ChildPassedItemsCountAction = ChildPassedItemsCountAction + count;
            }
            foreach(BusinessFlow BF in businessFlows)
            {
                if(BF.RunStatus == eRunStatus.Blocked)
                {
                    ChildExecutableItemsCountActivity = ChildExecutableItemsCountActivity + BF.Activities.Count;
                    foreach(Activity activity in BF.Activities)
                    {
                        ChildExecutableItemsCountAction = ChildExecutableItemsCountActivity + activity.Acts.Count;
                    }
                }
            }
            runner.ChildExecutableItemsCount.Add(HTMLReportConfiguration.eExecutionStatisticsCountBy.Activities.ToString(), ChildExecutableItemsCountActivity);
            runner.ChildExecutedItemsCount.Add(HTMLReportConfiguration.eExecutionStatisticsCountBy.Activities.ToString(), ChildExecutedItemsCountActivity);
            runner.ChildPassedItemsCount.Add(HTMLReportConfiguration.eExecutionStatisticsCountBy.Activities.ToString(), ChildPassedItemsCountActivity);
            runner.ChildExecutableItemsCount.Add(HTMLReportConfiguration.eExecutionStatisticsCountBy.Actions.ToString(), ChildExecutableItemsCountAction);
            runner.ChildExecutedItemsCount.Add(HTMLReportConfiguration.eExecutionStatisticsCountBy.Actions.ToString(), ChildExecutedItemsCountAction);
            runner.ChildPassedItemsCount.Add(HTMLReportConfiguration.eExecutionStatisticsCountBy.Actions.ToString(), ChildPassedItemsCountAction);
        }
        public override void SetReportRunSet(RunSetReport runSetReport, string logFolder, eExecutedFrom executedFrom = eExecutedFrom.Run)
        {
            LiteDbRunSet runSet = new LiteDbRunSet();
            base.SetReportRunSet(runSetReport, logFolder);
            runSet.RunnersColl.AddRange(ExecutionLoggerManager.RunSetReport.liteDbRunnerList);
            if (executedFrom == eExecutedFrom.Automation)
            {
                var runners = new ObservableList<GingerRunner>();
                runners.Add(new GingerRunner());
                SetRunSetChildCounts(runSet, runners);
            }
            else
            {
                SetRunSetChildCounts(runSet, WorkSpace.Instance.RunsetExecutor.Runners);
            }
            runSet.SetReportData(runSetReport);

            ExecutionLoggerManager.RunSetReport.DataRepMethod = ExecutionLoggerConfiguration.DataRepositoryMethod.LiteDB;
            ExecutionLoggerManager.RunSetReport.RunSetExecutionStatus = (eRunStatus)Enum.Parse(typeof(eRunStatus), runSet.RunStatus);

            SaveObjToReporsitory(runSet, liteDbManager.NameInDb<LiteDbRunSet>());
            if (runSetReport.RunSetExecutionStatus != Amdocs.Ginger.CoreNET.Execution.eRunStatus.Stopped)
            {
                ExecutionLoggerManager.RunSetReport.liteDbRunnerList.Clear();
            }
            ClearSeq();
        }
        private void SetRunSetChildCounts(LiteDbRunSet runSet, ObservableList<GingerRunner> runners)
        {
            int ChildExecutableItemsCountActivity = 0;
            int ChildExecutedItemsCountActivity = 0;
            int ChildPassedItemsCountActivity = 0;
            int ChildExecutableItemsCountAction = 0;
            int ChildExecutedItemsCountAction = 0;
            int ChildPassedItemsCountAction = 0;
            foreach (LiteDbRunner runner in runSet.RunnersColl)
            {
                int count = 0;
                runner.ChildExecutableItemsCount.TryGetValue(HTMLReportConfiguration.eExecutionStatisticsCountBy.Activities.ToString(), out count);
                ChildExecutableItemsCountActivity = ChildExecutableItemsCountActivity + count;

                runner.ChildExecutedItemsCount.TryGetValue(HTMLReportConfiguration.eExecutionStatisticsCountBy.Activities.ToString(), out count);
                ChildExecutedItemsCountActivity = ChildExecutedItemsCountActivity + count;

                runner.ChildPassedItemsCount.TryGetValue(HTMLReportConfiguration.eExecutionStatisticsCountBy.Activities.ToString(), out count);
                ChildPassedItemsCountActivity = ChildPassedItemsCountActivity + count;

                runner.ChildExecutableItemsCount.TryGetValue(HTMLReportConfiguration.eExecutionStatisticsCountBy.Actions.ToString(), out count);
                ChildExecutableItemsCountAction = ChildExecutableItemsCountAction + count;

                runner.ChildExecutedItemsCount.TryGetValue(HTMLReportConfiguration.eExecutionStatisticsCountBy.Actions.ToString(), out count);
                ChildExecutedItemsCountAction = ChildExecutedItemsCountAction + count;

                runner.ChildPassedItemsCount.TryGetValue(HTMLReportConfiguration.eExecutionStatisticsCountBy.Actions.ToString(), out count);
                ChildPassedItemsCountAction = ChildPassedItemsCountAction + count;
            }
            foreach(GingerRunner runner in runners)
            {
                if (runner.Status == eRunStatus.Blocked)
                {
                    foreach(BusinessFlow BF in runner.Executor.BusinessFlows)
                    {
                        ChildExecutableItemsCountActivity = ChildExecutableItemsCountActivity + BF.Activities.Count;
                        foreach(Activity activity in BF.Activities)
                        {
                            ChildExecutableItemsCountAction = ChildExecutableItemsCountAction + activity.Acts.Count;
                        }
                    }
                }
            }
            runSet.ChildExecutableItemsCount.Add(HTMLReportConfiguration.eExecutionStatisticsCountBy.Activities.ToString(), ChildExecutableItemsCountActivity);
            runSet.ChildExecutedItemsCount.Add(HTMLReportConfiguration.eExecutionStatisticsCountBy.Activities.ToString(), ChildExecutedItemsCountActivity);
            runSet.ChildPassedItemsCount.Add(HTMLReportConfiguration.eExecutionStatisticsCountBy.Activities.ToString(), ChildPassedItemsCountActivity);

            runSet.ChildExecutableItemsCount.Add(HTMLReportConfiguration.eExecutionStatisticsCountBy.Actions.ToString(), ChildExecutableItemsCountAction);
            runSet.ChildExecutedItemsCount.Add(HTMLReportConfiguration.eExecutionStatisticsCountBy.Actions.ToString(), ChildExecutedItemsCountAction);
            runSet.ChildPassedItemsCount.Add(HTMLReportConfiguration.eExecutionStatisticsCountBy.Actions.ToString(), ChildPassedItemsCountAction);
        }
        public override void RunSetUpdate(ObjectId runSetLiteDbId, ObjectId runnerLiteDbId, GingerExecutionEngine gingerRunner)
        {
            try
            {
                LiteDbRunner runner = new LiteDbRunner();
                runner.BusinessFlowsColl.AddRange(liteDbBFList);
                runner._id = runnerLiteDbId;
                runner.Seq = 1;
                runner.Name = "Automated Runner";
                runner.ApplicationAgentsMappingList = gingerRunner.GingerRunner.ApplicationAgents.Select(a => a.AgentName + "_:_" + a.AppName).ToList();
                runner.Environment = gingerRunner.GingerRunner.ProjEnvironment != null ? gingerRunner.GingerRunner.ProjEnvironment.Name : string.Empty;
                runner.GUID = gingerRunner.GingerRunner.Guid;
                if (gingerRunner.BusinessFlows.Count > 0)
                {
                    runner.StartTimeStamp = gingerRunner.BusinessFlows[0].StartTimeStamp;
                    runner.EndTimeStamp = gingerRunner.BusinessFlows[0].EndTimeStamp;
                    runner.Elapsed = gingerRunner.BusinessFlows[0].ElapsedSecs;
                }
                runner.RunStatus = (liteDbBFList.Count > 0) ? liteDbBFList[0].RunStatus : eRunStatus.Automated.ToString();

                SetRunnerChildCounts(runner, gingerRunner.BusinessFlows);

                SaveObjToReporsitory(runner, liteDbManager.NameInDb<LiteDbRunner>());
                liteDbBFList.Clear();
                LiteDbRunSet runSet = new LiteDbRunSet();
                runSet._id = runSetLiteDbId;

                if (ExecutionLoggerManager.RunSetReport != null)
                {
                    base.SetReportRunSet(ExecutionLoggerManager.RunSetReport, "");
                    runSet.SetReportData(ExecutionLoggerManager.RunSetReport);
                }

                runSet.RunnersColl.AddRange(new List<LiteDbRunner>() { runner });

                runSet.StartTimeStamp = runner.StartTimeStamp;
                runSet.EndTimeStamp = runner.EndTimeStamp;
                runSet.Elapsed = runner.Elapsed;

                var runners = new ObservableList<GingerRunner>();
                runners.Add(gingerRunner.GingerRunner);
                SetRunSetChildCounts(runSet, runners);

                SaveObjToReporsitory(runSet, liteDbManager.NameInDb<LiteDbRunSet>());
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occured during RunSetUpdate..", ex);
            }

        }

        public override void CreateNewDirectory(string logFolder)
        {
            return;
        }

        public override void SetRunsetFolder(string execResultsFolder, long maxFolderSize, DateTime currentExecutionDateTime, bool offline)
        {
            return;
        }

        public override void StartRunSet()
        {
            ExecutionLoggerManager.RunSetReport = new RunSetReport();
            ExecutionLoggerManager.RunSetReport.Name = WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name;

            ExecutionLoggerManager.RunSetReport.Description = WorkSpace.Instance.RunsetExecutor.RunSetConfig.Description;
            ExecutionLoggerManager.RunSetReport.GUID = WorkSpace.Instance.RunsetExecutor.RunSetConfig.Guid.ToString();
            ExecutionLoggerManager.RunSetReport.StartTimeStamp = DateTime.Now.ToUniversalTime();
            ExecutionLoggerManager.RunSetReport.Watch.Start();
        }

        public override void EndRunSet()
        {
            if (ExecutionLoggerManager.RunSetReport != null)
            {
                SetReportRunSet(ExecutionLoggerManager.RunSetReport, "");

                if (WorkSpace.Instance.RunningInExecutionMode)
                {
                    WorkSpace.Instance.RunsetExecutor.RunSetExecutionStatus = ExecutionLoggerManager.RunSetReport.RunSetExecutionStatus;
                }
                if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder != null && WorkSpace.Instance.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder.Equals("-1"))
                {
                    WorkSpace.Instance.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder = ExecutionLoggerManager.RunSetReport.LogFolder;
                }
                ExecutionProgressReporterListener.AddExecutionDetailsToLog(ExecutionProgressReporterListener.eExecutionPhase.End, GingerDicser.GetTermResValue(eTermResKey.RunSet), WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name, ExecutionLoggerManager.RunSetReport);
                ExecutionLoggerManager.RunSetReport = null;
            }
        }

        public override string SetExecutionLogFolder(string executionLogfolder, bool isCleanFile)
        {
            return string.Empty;
        }

        public override string GetLogFolder(string folder)
        {
            return string.Empty;
        }

        public override async Task<bool> SendExecutionLogToCentralDBAsync(LiteDB.ObjectId runsetId, Guid executionId, eDeleteLocalDataOnPublish deleteLocalData)
        {
            //Get the latest execution details from LiteDB
            LiteDbManager dbManager = new LiteDbManager(new ExecutionLoggerHelper().GetLoggerDirectory(WorkSpace.Instance.Solution.LoggerConfigurations.CalculatedLoggerFolder));
            LiteDbRunSet liteDbRunSet = dbManager.GetLatestExecutionRunsetData(runsetId?.ToString());
            List<string> screenshotList = PopulateMissingFieldsAndGetScreenshotsList(liteDbRunSet, executionId);

            AccountReportApiHandler centralExecutionLogger = new AccountReportApiHandler(WorkSpace.Instance.Solution.LoggerConfigurations.CentralLoggerEndPointUrl);

            //Map the data to AccountReportRunset Object
            AccountReportRunSet accountReportRunSet = centralExecutionLogger.MapDataToAccountReportObject(liteDbRunSet);
            SetExecutionId(accountReportRunSet, executionId);
            accountReportRunSet.EntityId = WorkSpace.Instance.RunsetExecutor.RunSetConfig.Guid;
            accountReportRunSet.GingerSolutionGuid = WorkSpace.Instance.Solution.Guid;

            //Publish the Data and screenshots to Central DB
            await centralExecutionLogger.SendRunsetExecutionDataToCentralDBAsync(accountReportRunSet);
            await centralExecutionLogger.SendScreenShotsToCentralDBAsync(executionId, screenshotList);


            //Delete local data if configured
            if (deleteLocalData == eDeleteLocalDataOnPublish.Yes)
            {
                try
                {
                    dbManager.DeleteDocumentByLiteDbRunSet(liteDbRunSet);
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Error when deleting local LiteDB data after Publis", ex);
                }


                foreach (string screenshot in screenshotList)
                {
                    try
                    {
                        File.Delete(screenshot);
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.DEBUG, "Deleting screenshots after published to central db", ex);
                    }
                }
            }


            return true;
        }

        private void SetExecutionId(AccountReportRunSet accountReportRunSet, Guid executionId)
        {
            accountReportRunSet.ExecutionId = executionId;
            accountReportRunSet.ElapsedEndTimeStamp = accountReportRunSet.ElapsedEndTimeStamp * 1000;
            foreach (AccountReportRunner runner in accountReportRunSet.RunnersColl)
            {
                runner.ExecutionId = executionId;
                runner.ElapsedEndTimeStamp = runner.ElapsedEndTimeStamp * 1000;
                foreach (AccountReportBusinessFlow businessFlow in runner.BusinessFlowsColl)
                {
                    businessFlow.ExecutionId = executionId;
                    businessFlow.ElapsedEndTimeStamp = businessFlow.ElapsedEndTimeStamp * 1000;
                    foreach (AccountReportActivityGroup accountReportActivityGroup in businessFlow.ActivitiesGroupsColl)
                    {
                        accountReportActivityGroup.ExecutionId = executionId;
                        accountReportActivityGroup.ElapsedEndTimeStamp = accountReportActivityGroup.ElapsedEndTimeStamp * 1000;
                        foreach (AccountReportActivity accountReportActivity in accountReportActivityGroup.ActivitiesColl)
                        {
                            accountReportActivity.ExecutionId = executionId;
                            accountReportActivity.ElapsedEndTimeStamp = accountReportActivity.ElapsedEndTimeStamp * 1000;
                            foreach (AccountReportAction accountReportAction in accountReportActivity.ActionsColl)
                            {
                                accountReportAction.ElapsedEndTimeStamp = accountReportAction.ElapsedEndTimeStamp * 1000;
                                accountReportAction.ExecutionId = executionId;
                            }
                        }
                    }
                }
            }
        }

        private List<string> PopulateMissingFieldsAndGetScreenshotsList(LiteDbRunSet liteDbRunSet, Guid executionId)
        {
            List<string> allScreenshots = new List<string>();
            //select template 
            HTMLReportConfiguration _HTMLReportConfig = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<HTMLReportConfiguration>().Where(x => x.IsDefault).FirstOrDefault();

            //populate data based on level
            if (string.IsNullOrEmpty(_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()))
            {
                _HTMLReportConfig.ExecutionStatisticsCountBy = HTMLReportConfiguration.eExecutionStatisticsCountBy.Actions;
            }


            List<string> runSetEnv = new List<string>();

            liteDbRunSet.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(liteDbRunSet.ChildExecutedItemsCount[_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()], liteDbRunSet.ChildExecutableItemsCount[_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()]));

            liteDbRunSet.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(liteDbRunSet.ChildPassedItemsCount[_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()], liteDbRunSet.ChildExecutableItemsCount[_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()]));

            if (liteDbRunSet.Elapsed.HasValue)
            {
                liteDbRunSet.Elapsed = Math.Round(liteDbRunSet.Elapsed.Value, 2);
            }
            foreach (LiteDbRunner liteDbRunner in liteDbRunSet.RunnersColl)
            {
                if (!runSetEnv.Contains(liteDbRunner.Environment))
                {
                    runSetEnv.Add(liteDbRunner.Environment);
                }

                liteDbRunner.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(liteDbRunner.ChildExecutedItemsCount[_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()], liteDbRunner.ChildExecutableItemsCount[_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()]));

                liteDbRunner.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(liteDbRunner.ChildPassedItemsCount[_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()], liteDbRunner.ChildExecutableItemsCount[_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()]));

                if (liteDbRunner.Elapsed.HasValue)
                {
                    liteDbRunner.Elapsed = Math.Round(liteDbRunner.Elapsed.Value, 2);
                }
                else { liteDbRunner.Elapsed = 0; }
                foreach (LiteDbBusinessFlow liteDbBusinessFlow in liteDbRunner.BusinessFlowsColl)
                {

                    liteDbBusinessFlow.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(liteDbBusinessFlow.ChildExecutedItemsCount[_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()], liteDbBusinessFlow.ChildExecutableItemsCount[_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()]));

                    liteDbBusinessFlow.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(liteDbBusinessFlow.ChildPassedItemsCount[_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()], liteDbBusinessFlow.ChildExecutableItemsCount[_HTMLReportConfig.ExecutionStatisticsCountBy.ToString()]));

                    if (liteDbBusinessFlow.Elapsed.HasValue)
                    {
                        liteDbBusinessFlow.Elapsed = Math.Round(liteDbBusinessFlow.Elapsed.Value, 2);
                    }
                    else { liteDbBusinessFlow.Elapsed = 0; }

                    foreach (LiteDbActivityGroup liteDbActivityGroup in liteDbBusinessFlow.ActivitiesGroupsColl)
                    {
                        foreach (LiteDbActivity liteDbActivity in liteDbActivityGroup.ActivitiesColl)
                        {

                            liteDbActivity.ExecutionRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(liteDbActivity.ChildExecutedItemsCount, liteDbActivity.ChildExecutableItemsCount));

                            liteDbActivity.PassRate = string.Format("{0:F1}", CalculateExecutionOrPassRate(liteDbActivity.ChildPassedItemsCount, liteDbActivity.ChildExecutableItemsCount));


                            if (liteDbActivity.Elapsed.HasValue)
                            {
                                liteDbActivity.Elapsed = Math.Round(liteDbActivity.Elapsed.Value / 1000, 4);
                            }
                            else { liteDbActivity.Elapsed = 0; }
                            foreach (LiteDbAction liteDbAction in liteDbActivity.ActionsColl)
                            {
                                List<string> newScreenShotsList = new List<string>();
                                if (liteDbAction.Elapsed.HasValue)
                                {
                                    liteDbAction.Elapsed = Math.Round(liteDbAction.Elapsed.Value / 1000, 4);
                                }
                                else { liteDbAction.Elapsed = 0; }
                                if ((!string.IsNullOrEmpty(liteDbAction.ExInfo)) && liteDbAction.ExInfo[liteDbAction.ExInfo.Length - 1] == '-')
                                {
                                    liteDbAction.ExInfo = liteDbAction.ExInfo.Remove(liteDbAction.ExInfo.Length - 1);
                                }

                                foreach (string screenshot in liteDbAction.ScreenShots)
                                {
                                    allScreenshots.Add(screenshot);
                                    string newScreenshotPath = executionId.ToString() + "/" + Path.GetFileName(screenshot);

                                    newScreenShotsList.Add(newScreenshotPath);
                                }
                                liteDbAction.ScreenShots = newScreenShotsList;
                            }
                        }
                    }


                }
            }

            if (runSetEnv.Count > 0)
            {
                liteDbRunSet.Environment = string.Join(",", runSetEnv);
            }
            return allScreenshots;
        }

        private string CalculateExecutionOrPassRate(int firstItem, int secondItem)
        {
            if (secondItem != 0)
            {
                return (firstItem * 100 / secondItem).ToString();
            }
            else
            {
                return "0";
            }
        }

        public override string CalculateExecutionJsonData(LiteDbRunSet liteDbRunSet, HTMLReportConfiguration reportTemplate)
        {
            AccountReportApiHandler centralExecutionLogger = new AccountReportApiHandler();
            AccountReportRunSet accountReportRunSet = centralExecutionLogger.MapDataToAccountReportObject(liteDbRunSet);
            string json = JsonConvert.SerializeObject(accountReportRunSet, Formatting.Indented);

            JObject runSetObject = JObject.Parse(json);

            #region Generate JSON

            //Remove Fields from json which are not selected
            foreach (HTMLReportConfigFieldToSelect runsetFieldToRemove in reportTemplate.RunSetSourceFieldsToSelect.Where(x => !x.IsSelected))
            {
                runSetObject.Property(GetFieldToRemove(runsetFieldToRemove.FieldKey)).Remove();
            }

            //RunnersCollection
            HTMLReportConfigFieldToSelect runnerField = reportTemplate.RunSetSourceFieldsToSelect.Where(x => x.IsSelected && x.FieldKey == "RunnersColl").FirstOrDefault();
            if (runnerField != null)
            {
                if (reportTemplate.GingerRunnerSourceFieldsToSelect.Select(x => x.IsSelected).ToList().Count > 0)
                {
                    JArray runnerArray = (JArray)runSetObject[runnerField.FieldKey];
                    foreach (JObject jRunnerObject in runnerArray)
                    {
                        foreach (HTMLReportConfigFieldToSelect runnerFieldToRemove in reportTemplate.GingerRunnerSourceFieldsToSelect.Where(x => !x.IsSelected))
                        {
                            jRunnerObject.Property(GetFieldToRemove(runnerFieldToRemove.FieldKey)).Remove();
                        }
                        //BusinessFlowsCollection
                        HTMLReportConfigFieldToSelect bfField = reportTemplate.GingerRunnerSourceFieldsToSelect.Where(x => x.IsSelected && x.FieldKey == "BusinessFlowsColl").FirstOrDefault();
                        if (bfField != null)
                        {
                            if (reportTemplate.BusinessFlowSourceFieldsToSelect.Select(x => x.IsSelected).ToList().Count > 0)
                            {
                                JArray bfArray = (JArray)jRunnerObject[bfField.FieldKey];
                                foreach (JObject jBFObject in bfArray)
                                {
                                    foreach (HTMLReportConfigFieldToSelect bfFieldToRemove in reportTemplate.BusinessFlowSourceFieldsToSelect.Where(x => !x.IsSelected))
                                    {
                                        jBFObject.Property(GetFieldToRemove(bfFieldToRemove.FieldKey)).Remove();
                                    }
                                    //ActivityGroupsCollection
                                    HTMLReportConfigFieldToSelect activityGroupField = reportTemplate.BusinessFlowSourceFieldsToSelect.Where(x => x.IsSelected && x.FieldKey == "ActivitiesGroupsColl").FirstOrDefault();
                                    if (activityGroupField != null)
                                    {
                                        if (reportTemplate.ActivityGroupSourceFieldsToSelect.Select(x => x.IsSelected).ToList().Count > 0)
                                        {
                                            JArray activityGroupArray = (JArray)jBFObject[activityGroupField.FieldKey];
                                            foreach (JObject jActivityGroupObject in activityGroupArray)
                                            {
                                                foreach (HTMLReportConfigFieldToSelect activityGroupFieldToRemove in reportTemplate.ActivityGroupSourceFieldsToSelect.Where(x => !x.IsSelected))
                                                {
                                                    jActivityGroupObject.Property(GetFieldToRemove(activityGroupFieldToRemove.FieldKey)).Remove();
                                                }
                                                //ActivitiesCollection
                                                HTMLReportConfigFieldToSelect activityFieldCheck = reportTemplate.ActivityGroupSourceFieldsToSelect.Where(x => x.IsSelected && x.FieldKey == "ActivitiesColl").FirstOrDefault();
                                                if (activityFieldCheck != null)
                                                {
                                                    if (reportTemplate.ActivitySourceFieldsToSelect.Select(x => x.IsSelected).ToList().Count > 0)
                                                    {
                                                        JArray activityArray = (JArray)jActivityGroupObject[activityFieldCheck.FieldKey];
                                                        foreach (JObject jActivityObject in activityArray)
                                                        {
                                                            //Calculate ErrorDetails And add it to Activity Level
                                                            StringBuilder errorDetailsNew = new StringBuilder();
                                                            string errorDetails = null;
                                                            JArray actionsArray = (JArray)jActivityObject["ActionsColl"];
                                                            foreach (JObject jActionObject in actionsArray)
                                                            {
                                                                if (!string.IsNullOrEmpty(jActionObject.Property("Error").Value.ToString()) && !string.IsNullOrEmpty(errorDetailsNew.ToString()))
                                                                {
                                                                    errorDetailsNew.Append(",");
                                                                    errorDetailsNew.Append(jActionObject.Property("Error").Value.ToString());
                                                                }
                                                                else
                                                                {
                                                                    errorDetailsNew.Append(jActionObject.Property("Error").Value.ToString());
                                                                }
                                                            }
                                                            if (string.IsNullOrEmpty(errorDetailsNew.ToString()))
                                                            {
                                                                errorDetails = "NA";
                                                            }
                                                            else
                                                            {
                                                                errorDetails = errorDetailsNew.ToString();
                                                            }
                                                            jActivityObject.Add("ErrorDetails", errorDetails);

                                                            foreach (HTMLReportConfigFieldToSelect activityFieldToRemove in reportTemplate.ActivitySourceFieldsToSelect.Where(x => !x.IsSelected))
                                                            {
                                                                jActivityObject.Property(GetFieldToRemove(activityFieldToRemove.FieldKey)).Remove();
                                                            }
                                                            //ActionsColl
                                                            HTMLReportConfigFieldToSelect actionField = reportTemplate.ActivitySourceFieldsToSelect.Where(x => x.IsSelected && x.FieldKey == "ActionsColl").FirstOrDefault();
                                                            if (actionField != null)
                                                            {
                                                                if (reportTemplate.ActionSourceFieldsToSelect.Select(x => x.IsSelected).ToList().Count > 0)
                                                                {
                                                                    JArray actionArray = (JArray)jActivityObject[actionField.FieldKey];
                                                                    foreach (JObject jActionObject in actionArray)
                                                                    {
                                                                        foreach (HTMLReportConfigFieldToSelect actionFieldToRemove in reportTemplate.ActionSourceFieldsToSelect.Where(x => !x.IsSelected))
                                                                        {
                                                                            jActionObject.Property(GetFieldToRemove(actionFieldToRemove.FieldKey)).Remove();
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            return runSetObject.ToString(Formatting.Indented);
        }
        private string GetFieldToRemove(string fieldName)
        {
            string fieldToReturn = fieldName;
            if (fieldName.ToLower() == "executedbyuser")
            {
                fieldToReturn = "ExecutedByUser";
            }
            else if (fieldName.ToLower() == "elapsed")
            {
                fieldToReturn = "ElapsedEndTimeStamp";
            }
            else if (fieldName.ToLower() == "_id")
            {
                fieldToReturn = "ExecutionId";
            }
            return fieldToReturn;
        }

        /// <summary>
        /// Clear last run details if we are not continue from last run
        /// </summary>
        public override void ResetLastRunSetDetails()
        {
            lastBfStatus = eRunStatus.Pending;
            liteDbBFList.Clear();
        }
    }
}
