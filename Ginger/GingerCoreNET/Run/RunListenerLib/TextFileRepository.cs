using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.LiteDBFolder;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Environments;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Run.RunListenerLib
{
    class TextFileRepository: ExecutionLogger
    {
        static JsonSerializer mJsonSerializer;
        public TextFileRepository()
        {
            mJsonSerializer = new JsonSerializer();
            mJsonSerializer.NullValueHandling = NullValueHandling.Ignore;
        }
        public override void SaveObjToReporsitory(object obj, string FileName = "", bool toAppend = false)
        {
            //TODO: for speed we can do it async on another thread...
            using (StreamWriter SW = new StreamWriter(FileName, toAppend))
            using (JsonWriter writer = new JsonTextWriter(SW))
            {
                mJsonSerializer.Serialize(writer, obj);
            }
        }

        public override object SetReportAction(Act action, Context context, Amdocs.Ginger.Common.eExecutedFrom executedFrom, bool offlineMode = false)
        {
            string executionLogFolder = string.Empty;
            if (!offlineMode)
                executionLogFolder = ExecutionLogfolder;
            ActionReport AR = GetActionReportData(action, context, executedFrom);
            if (System.IO.Directory.Exists(executionLogFolder + action.ExecutionLogFolder))
            {
                this.SaveObjToReporsitory(AR, executionLogFolder + action.ExecutionLogFolder + @"\Action.txt");

                // Save screenShots
                int screenShotCountPerAction = 0;
                for (var s = 0; s < action.ScreenShots.Count; s++)
                {
                    try
                    {
                        screenShotCountPerAction++;
                        if (executedFrom == Amdocs.Ginger.Common.eExecutedFrom.Automation)
                        {
                            System.IO.File.Copy(action.ScreenShots[s], executionLogFolder + action.ExecutionLogFolder + @"\ScreenShot_" + AR.Seq + "_" + screenShotCountPerAction.ToString() + ".png", true);
                        }
                        else
                        {
                            System.IO.File.Move(action.ScreenShots[s], executionLogFolder + action.ExecutionLogFolder + @"\ScreenShot_" + AR.Seq + "_" + screenShotCountPerAction.ToString() + ".png");
                            action.ScreenShots[s] = executionLogFolder + action.ExecutionLogFolder + @"\ScreenShot_" + AR.Seq + "_" + screenShotCountPerAction.ToString() + ".png";
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to move screen shot of the action:'" + action.Description + "' to the Execution Logger folder", ex);
                        screenShotCountPerAction--;
                    }
                }

            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to create ExecutionLogger JSON file for the Action :" + action.Description + " because directory not exists :" + executionLogFolder + action.ExecutionLogFolder);
            }
            return AR;
        }

        public override object SetReportActivity(Activity activity, Context context, bool offlineMode)
        {
            ActivityReport AR = GetActivityReportData(activity, context, offlineMode);
            if (WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.ExecutionLoggerConfigurationIsEnabled)
            {
                if (offlineMode)
                    // use Path.combine !!!!
                    SaveObjToReporsitory(AR, activity.ExecutionLogFolder + @"\Activity.txt");
                else
                    // use Path.combine !!!!
                    SaveObjToReporsitory(AR, ExecutionLogfolder + activity.ExecutionLogFolder + @"\Activity.txt");
            }
            return AR; 
        }

        public override object SetReportActivityGroup(ActivitiesGroup activityGroup, BusinessFlow businessFlow, bool offlineMode)
        {
            ActivityGroupReport AGR = GetAGReportData(activityGroup, businessFlow);
            //AGR.ReportMapper(activityGroup, businessFlow, ExecutionLogfolder);
            if (offlineMode && activityGroup.ExecutionLogFolder != null)
            {
                SaveObjToReporsitory(AGR, activityGroup.ExecutionLogFolder + @"\ActivityGroups.txt", true);
                File.AppendAllText(activityGroup.ExecutionLogFolder + @"\ActivityGroups.txt", Environment.NewLine);
            }
            else
            {
                SaveObjToReporsitory(AGR, ExecutionLogfolder + businessFlow.ExecutionLogFolder + @"\ActivityGroups.txt", true);
                File.AppendAllText(ExecutionLogfolder + businessFlow.ExecutionLogFolder + @"\ActivityGroups.txt", Environment.NewLine);
            }
            return AGR;
        }

        public override object SetReportBusinessFlow(BusinessFlow businessFlow, ProjEnvironment environment, bool offlineMode, Amdocs.Ginger.Common.eExecutedFrom executedFrom)
        {
            BusinessFlowReport BFR = GetBFReportData(businessFlow, environment);
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
                    SaveObjToReporsitory(BFR, businessFlow.ExecutionFullLogFolder + @"\BusinessFlow.txt");

                }
                else
                {
                    // use Path.cOmbine
                    SaveObjToReporsitory(BFR, ExecutionLogfolder + businessFlow.ExecutionLogFolder + @"\BusinessFlow.txt");
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
            base.SetReportRunner(gingerRunner, gingerReport,  gingerData,  mContext, filename, runnerCount);
            SaveObjToReporsitory(gingerReport, gingerReport.LogFolder + @"\Ginger.txt");
        }

        internal override void SetReportRunSet(RunSetReport runSetReport, string logFolder)
        {
            base.SetReportRunSet(runSetReport, logFolder);
            if (logFolder == null)
            {
                SaveObjToReporsitory(runSetReport, runSetReport.LogFolder + @"\RunSet.txt");
            }
            else
            {
                SaveObjToReporsitory(runSetReport, logFolder + @"\RunSet.txt");
            }
        }
    }
}
