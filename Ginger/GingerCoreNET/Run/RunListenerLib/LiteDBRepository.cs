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
using System.IO;
using System.Linq;
using System.Text;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using Ginger.Reports;
using Ginger.Run;
using GingerCore;
using GingerCore.Activities;
using GingerCore.Environments;
using LiteDB;

namespace Amdocs.Ginger.CoreNET.Run.RunListenerLib
{
    class LiteDBRepository: ExecutionLogger
    {
        public LiteDbManager liteDbManager; 

        public List<LiteDbRunSet> liteDbRunSetList = new List<LiteDbRunSet>();
        
        public List<LiteDbBusinessFlow> liteDbBFList = new List<LiteDbBusinessFlow>();
        public List<LiteDbActivityGroup> liteDbAGList = new List<LiteDbActivityGroup>();
        public List<LiteDbActivity> liteDbActivityList = new List<LiteDbActivity>();
        public List<LiteDbAction> liteDbActionList = new List<LiteDbAction>();
        public LiteDBRepository()
        {
            liteDbManager = new LiteDbManager(executionLoggerHelper.GetLoggerDirectory(WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.ExecutionLoggerConfigurationExecResultsFolder));
        }

        public override void SaveObjToReporsitory(object obj, string FileName = "", bool toAppend = false)
        {
            liteDbManager.WriteToLiteDb(FileName, new List<LiteDbReportBase>() { (LiteDbReportBase)obj });
        }
        public override object SetReportAction(GingerCore.Actions.Act action, Context context, Amdocs.Ginger.Common.eExecutedFrom executedFrom, bool offlineMode = false)
        {
            bool isActExsits = false;
            string executionLogFolder = executionLoggerHelper.GetLoggerDirectory(WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.ExecutionLoggerConfigurationExecResultsFolder);
            LiteDbAction liteDbAction = new LiteDbAction();
            liteDbAction.SetReportData(GetActionReportData(action, context, executedFrom));
            //if (System.IO.Directory.Exists(executionLogFolder))
            //{
                liteDbAction.Wait = action.Wait;
                liteDbAction.TimeOut = action.Timeout;
                if (action.LiteDbId != null && executedFrom == eExecutedFrom.Automation)
                {
                    liteDbAction._id = action.LiteDbId;
                }
                // Save screenShots
                int screenShotCountPerAction = 0;
                for (var s = 0; s < action.ScreenShots.Count; s++)
                {
                    try
                    {
                        screenShotCountPerAction++;
                        string imagesFolderName = executionLogFolder + "LiteDBImages";
                        if (!System.IO.Directory.Exists(imagesFolderName))
                        {
                            System.IO.Directory.CreateDirectory(imagesFolderName);
                        }
                        if (executedFrom == Amdocs.Ginger.Common.eExecutedFrom.Automation)
                        {
                            System.IO.File.Copy(action.ScreenShots[s], imagesFolderName + @"\ScreenShot_" + liteDbAction.GUID + "_" + liteDbAction.StartTimeStamp.ToString("hhmmss") + "_" + screenShotCountPerAction.ToString() + ".png", true);
                        }
                        else
                        {
                            System.IO.File.Move(action.ScreenShots[s], imagesFolderName + @"\ScreenShot_" + liteDbAction.GUID + "_" + liteDbAction.StartTimeStamp.ToString("hhmmss") + "_" + screenShotCountPerAction.ToString() + ".png");
                            action.ScreenShots[s] = imagesFolderName + @"\ScreenShot_" + liteDbAction.GUID + "_" + liteDbAction.StartTimeStamp.ToString("hhmmss") + "_" + screenShotCountPerAction.ToString() + ".png";
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to move screen shot of the action:'" + action.Description + "' to the Execution Logger folder", ex);
                        screenShotCountPerAction--;
                    }
                }
                liteDbAction.ScreenShots = action.ScreenShots;

            isActExsits = liteDbActionList.Any(x => x.GUID == liteDbAction.GUID);
            if (isActExsits)
            {
                liteDbActionList.RemoveAll(x => x.GUID == liteDbAction.GUID);
            }
            liteDbActionList.Add(liteDbAction);
            //}
            //else
            //{
            //    Reporter.ToLog(eLogLevel.ERROR, "Failed to create ExecutionLogger JSON file for the Action :" + action.Description + " because directory not exists :" + executionLogFolder + action.ExecutionLogFolder);
            //}
            SaveObjToReporsitory(liteDbAction, liteDbManager.NameInDb<LiteDbAction>());
            if (executedFrom == eExecutedFrom.Automation)
            {
                action.LiteDbId = liteDbAction._id;
            }
            return liteDbAction;
        }

        public override object SetReportActivity(Activity activity,Context context, bool offlineMode = false)
        {
            LiteDbActivity AR = new LiteDbActivity();
            AR.SetReportData(GetActivityReportData(activity,context, offlineMode));
            AR.ActivityGroupName = activity.ActivitiesGroupID;
            if(activity.LiteDbId != null && ExecutionLoggerManager.RunSetReport.RunSetExecutionStatus == Execution.eRunStatus.Automated) // missing Executed from
            {
                AR._id = activity.LiteDbId;
            }
            AR.ActionsColl.AddRange(liteDbActionList);
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
            LiteDbActivityGroup AGR = new LiteDbActivityGroup();
            AGR.SetReportData(GetAGReportData(activityGroup, businessFlow));
            if (activityGroup.LiteDbId != null && ExecutionLoggerManager.RunSetReport.RunSetExecutionStatus == Execution.eRunStatus.Automated)
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

        public override object SetReportBusinessFlow(BusinessFlow businessFlow, ProjEnvironment environment, bool offlineMode, Amdocs.Ginger.Common.eExecutedFrom executedFrom)
        {
            LiteDbBusinessFlow BFR = new LiteDbBusinessFlow();
            BFR.SetReportData(GetBFReportData(businessFlow, environment));
            if(businessFlow.LiteDbId != null && executedFrom == eExecutedFrom.Automation)
            {
                BFR._id = businessFlow.LiteDbId;
            }
            if (WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.ExecutionLoggerConfigurationIsEnabled)
            {
                if (offlineMode)
                {
                    // To check whether the execution is from Runset/Automate tab
                    if ((executedFrom == Amdocs.Ginger.Common.eExecutedFrom.Automation))
                    {
                        businessFlow.ExecutionFullLogFolder = businessFlow.ExecutionLogFolder;
                    }
                    else if ((WorkSpace.Instance.RunsetExecutor.RunSetConfig.LastRunsetLoggerFolder != null))
                    {
                        businessFlow.ExecutionFullLogFolder = businessFlow.ExecutionLogFolder;
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
                    liteDbBFList.Add(BFR);
                    liteDbActivityList.Clear();
                    liteDbAGList.Clear();
                    businessFlow.ExecutionFullLogFolder = ExecutionLogfolder + businessFlow.ExecutionLogFolder;
                }
                if (executedFrom == Amdocs.Ginger.Common.eExecutedFrom.Automation)
                {
                    this.ExecutionLogBusinessFlowsCounter = 0;
                    //this.BFCounter = 0;
                }
            }
            if (executedFrom == eExecutedFrom.Automation)
            {
                businessFlow.LiteDbId = BFR._id;
            }
            return BFR;
        }

        public override void SetReportRunner(GingerRunner gingerRunner, GingerReport gingerReport, ExecutionLoggerManager.ParentGingerData gingerData, Context mContext, string filename, int runnerCount)
        {
            base.SetReportRunner(gingerRunner, gingerReport, gingerData, mContext, filename, runnerCount);
            LiteDbRunner runner = new LiteDbRunner();
            runner.BusinessFlowsColl.AddRange(liteDbBFList);
            runner.SetReportData(gingerReport);
            SaveObjToReporsitory(runner, liteDbManager.NameInDb<LiteDbRunner>());
            if (ExecutionLoggerManager.RunSetReport == null)
            {
                ExecutionLoggerManager.RunSetReport = new RunSetReport();
                ExecutionLoggerManager.RunSetReport.GUID = Guid.NewGuid().ToString();
            }
            ExecutionLoggerManager.RunSetReport.liteDbRunnerList.Add(runner);
            liteDbBFList.Clear();
        }

        public override void SetReportRunSet(RunSetReport runSetReport, string logFolder)
        {
            LiteDbRunSet runSet = new LiteDbRunSet();
            base.SetReportRunSet(runSetReport, logFolder);
            runSet.RunnersColl.AddRange(ExecutionLoggerManager.RunSetReport.liteDbRunnerList);
            runSet.SetReportData(runSetReport);
            SaveObjToReporsitory(runSet, liteDbManager.NameInDb<LiteDbRunSet>());
            ExecutionLoggerManager.RunSetReport.liteDbRunnerList.Clear();
            if(runSetReport.LogFolder != null && System.IO.Directory.Exists(runSetReport.LogFolder))
            {
                System.IO.Directory.Delete(runSetReport.LogFolder,true);
            }
        }
        public override void RunSetUpdate(ObjectId runSetLiteDbId, ObjectId runnerLiteDbId, GingerRunner gingerRunner)
        {
            LiteDbRunner runner = new LiteDbRunner();
            runner.BusinessFlowsColl.AddRange(liteDbBFList);
            runner._id = runnerLiteDbId;
            runner.Seq = 1;
            runner.Name = "Automated Runner";
            runner.ApplicationAgentsMappingList = gingerRunner.ApplicationAgents.Select(a => a.AgentName + "_:_" + a.AppName).ToList();
            runner.Environment = gingerRunner.ProjEnvironment != null ? gingerRunner.ProjEnvironment.Name : string.Empty;
            runner.GUID = gingerRunner.Guid;
            if(gingerRunner.BusinessFlows.Count > 0)
            {
                runner.StartTimeStamp = gingerRunner.BusinessFlows[0].StartTimeStamp;
                runner.EndTimeStamp = gingerRunner.BusinessFlows[0].EndTimeStamp;
                runner.Elapsed = gingerRunner.BusinessFlows[0].Elapsed;
            }
            runner.RunStatus = (liteDbBFList.Count > 0) ? liteDbBFList[0].RunStatus : eRunStatus.Automated.ToString();
            SaveObjToReporsitory(runner, liteDbManager.NameInDb<LiteDbRunner>());
            liteDbBFList.Clear();
            LiteDbRunSet runSet = new LiteDbRunSet();
            runSet._id = runSetLiteDbId;
            base.SetReportRunSet(ExecutionLoggerManager.RunSetReport, "");
            runSet.SetReportData(ExecutionLoggerManager.RunSetReport);
            runSet.RunnersColl.AddRange(new List<LiteDbRunner>() { runner });
            SaveObjToReporsitory(runSet, liteDbManager.NameInDb<LiteDbRunSet>());
        }

        internal override void CreateNewDirectory(string logFolder)
        {
            return;
        }

        internal override void SetRunsetFolder(string execResultsFolder, long maxFolderSize, DateTime currentExecutionDateTime, bool offline)
        {
            return;
        }

        internal override void StartRunSet()
        {
            if (ExecutionLoggerManager.RunSetReport == null)
            {
                ExecutionLoggerManager.RunSetReport = new RunSetReport();
                ExecutionLoggerManager.RunSetReport.Name = WorkSpace.Instance.RunsetExecutor.RunSetConfig.Name;

                ExecutionLoggerManager.RunSetReport.Description = WorkSpace.Instance.RunsetExecutor.RunSetConfig.Description;
                ExecutionLoggerManager.RunSetReport.GUID = WorkSpace.Instance.RunsetExecutor.RunSetConfig.Guid.ToString();
                ExecutionLoggerManager.RunSetReport.StartTimeStamp = DateTime.Now.ToUniversalTime();
                ExecutionLoggerManager.RunSetReport.Watch.Start();
            }
        }

        internal override void EndRunSet()
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
    }
}
