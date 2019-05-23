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
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using Ginger.Reports;
using Ginger.Run;
using GingerCore;
using GingerCore.Activities;
using GingerCore.Environments;

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
            string executionLogFolder = executionLoggerHelper.GetLoggerDirectory(WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.ExecutionLoggerConfigurationExecResultsFolder);
            LiteDbAction liteDbAction = new LiteDbAction();
            liteDbAction.SetReportData(GetActionReportData(action, context, executedFrom));
            if (System.IO.Directory.Exists(executionLogFolder))
            {
                liteDbAction.Wait = action.Wait;
                liteDbAction.TimeOut = action.Timeout;
                
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
                liteDbActionList.Add(liteDbAction);
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to create ExecutionLogger JSON file for the Action :" + action.Description + " because directory not exists :" + executionLogFolder + action.ExecutionLogFolder);
            }
            SaveObjToReporsitory(liteDbAction, liteDbManager.NameInDb<LiteDbAction>());
            return liteDbAction;
        }

        public override object SetReportActivity(Activity activity,Context context, bool offlineMode = false)
        {
            LiteDbActivity AR = new LiteDbActivity();
            AR.SetReportData(GetActivityReportData(activity,context, offlineMode));
            AR.ActivityGroupName = activity.ActivitiesGroupID;
            AR.ActionsColl.AddRange(liteDbActionList);
            liteDbActivityList.Add(AR);
            SaveObjToReporsitory(AR, liteDbManager.NameInDb<LiteDbActivity>());
            liteDbActionList.Clear();
            return AR;
        }
        public override object SetReportActivityGroup(ActivitiesGroup activityGroup, BusinessFlow businessFlow, bool offlineMode)
        {
            LiteDbActivityGroup AGR = new LiteDbActivityGroup();
            AGR.SetReportData(GetAGReportData(activityGroup, businessFlow));
            AGR.ActivitiesColl = liteDbActivityList.Where(ac => ac.ActivityGroupName != null && ac.ActivityGroupName.Equals(AGR.Name)).ToList();
            SaveObjToReporsitory(AGR, liteDbManager.NameInDb<LiteDbActivityGroup>(), true);
            liteDbAGList.Add(AGR);
            return AGR;
        }

        public override object SetReportBusinessFlow(BusinessFlow businessFlow, ProjEnvironment environment, bool offlineMode, Amdocs.Ginger.Common.eExecutedFrom executedFrom)
        {
            LiteDbBusinessFlow BFR = new LiteDbBusinessFlow();
            BFR.SetReportData(GetBFReportData(businessFlow, environment));
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
            return BFR;
        }

        public override void SetReportRunner(GingerRunner gingerRunner, GingerReport gingerReport, ExecutionLoggerManager.ParentGingerData gingerData, Context mContext, string filename, int runnerCount)
        {
            base.SetReportRunner(gingerRunner, gingerReport, gingerData, mContext, filename, runnerCount);
            LiteDbRunner runner = new LiteDbRunner();
            runner.BusinessFlowsColl.AddRange(liteDbBFList);
            runner.SetReportData(gingerReport);
            SaveObjToReporsitory(runner, liteDbManager.NameInDb<LiteDbRunner>());
            ExecutionLoggerManager.RunSetReport.liteDbRunnerList.Add(runner);
            liteDbBFList.Clear();
        }

        internal override void SetReportRunSet(RunSetReport runSetReport, string logFolder)
        {
            LiteDbRunSet runSet = new LiteDbRunSet();
            base.SetReportRunSet(runSetReport, logFolder);
            runSet.RunnersColl.AddRange(ExecutionLoggerManager.RunSetReport.liteDbRunnerList);
            runSet.SetReportData(runSetReport);
            SaveObjToReporsitory(runSet, liteDbManager.NameInDb<LiteDbRunSet>());
            ExecutionLoggerManager.RunSetReport.liteDbRunnerList.Clear();
        }
    }
}
