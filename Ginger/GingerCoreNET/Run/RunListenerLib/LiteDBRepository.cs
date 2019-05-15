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
        public List<LiteDbRunner> liteDbRunnerList = new List<LiteDbRunner>();
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
                            System.IO.File.Copy(action.ScreenShots[s], imagesFolderName + @"\ScreenShot_" + liteDbAction.GUID + "_" + screenShotCountPerAction.ToString() + ".png", true);
                        }
                        else
                        {
                            System.IO.File.Move(action.ScreenShots[s], imagesFolderName + @"\ScreenShot_" + liteDbAction.GUID + "_" + screenShotCountPerAction.ToString() + ".png");
                            action.ScreenShots[s] = imagesFolderName + @"\ScreenShot_" + liteDbAction.GUID + "_" + screenShotCountPerAction.ToString() + ".png";
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
            SaveObjToReporsitory(liteDbAction, "Actions");
            return liteDbAction;
        }

        public override object SetReportActivity(Activity activity,Context context, bool offlineMode = false)
        {
            LiteDbActivity AR = new LiteDbActivity();
            AR.SetReportData(GetActivityReportData(activity,context, offlineMode));
            AR.ActivityGroupName = activity.ActivitiesGroupID;
            AR.actionsColl.AddRange(liteDbActionList);
            liteDbActivityList.Add(AR);
            SaveObjToReporsitory(AR, "Activities");
            liteDbActionList.Clear();
            return AR;
        }
        public override object SetReportActivityGroup(ActivitiesGroup activityGroup, BusinessFlow businessFlow, bool offlineMode)
        {
            LiteDbActivityGroup AGR = new LiteDbActivityGroup();
            AGR.SetReportData(GetAGReportData(activityGroup, businessFlow));
            AGR.ActivitiesColl = liteDbActivityList.Where(ac => ac.ActivityGroupName != null && ac.ActivityGroupName.Equals(AGR.Name)).ToList();
            SaveObjToReporsitory(AGR, "ActivityGroups", true);
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
                    BFR.ActivitiesGroupColl.AddRange(liteDbAGList);
                    SaveObjToReporsitory(BFR, "BusinessFlows");
                    liteDbActivityList.Clear();
                    liteDbAGList.Clear();
                }
                else
                {
                    BFR.ActivitiesColl.AddRange(liteDbActivityList);
                    BFR.ActivitiesGroupColl.AddRange(liteDbAGList);
                    SaveObjToReporsitory(BFR, "BusinessFlows");
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
            runner.SetReportData(gingerReport);
            runner.BusinessFlowColl.AddRange(liteDbBFList);
            SaveObjToReporsitory(runner, "Runners");
            liteDbRunnerList.Add(runner);
            liteDbBFList.Clear();
        }

        internal override void SetReportRunSet(RunSetReport runSetReport, string logFolder)
        {
            LiteDbRunSet runSet = new LiteDbRunSet();
            base.SetReportRunSet(runSetReport, logFolder);
            runSet.SetReportData(runSetReport);
            runSet.RunnerColl.AddRange(liteDbRunnerList);
            SaveObjToReporsitory(runSet, "RunSet");
            liteDbRunnerList.Clear();
        }
    }
}
