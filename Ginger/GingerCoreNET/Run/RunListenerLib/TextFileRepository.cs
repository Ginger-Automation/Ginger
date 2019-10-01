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
using Amdocs.Ginger.Common.GeneralLib;
using System.Linq;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Run.RunListenerLib
{
    class TextFileRepository: ExecutionLogger
    {
        static Newtonsoft.Json.JsonSerializer mJsonSerializer;
        public TextFileRepository()
        {
            mJsonSerializer = new Newtonsoft.Json.JsonSerializer();
            mJsonSerializer.NullValueHandling = NullValueHandling.Ignore;
        }

        public override void RunSetUpdate(LiteDB.ObjectId runSetLiteDbId, LiteDB.ObjectId runnerLiteDbId, GingerRunner gingerRunner)
        {
            return;
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

        public override string SetExecutionLogFolder(string executionLogfolder, bool isCleanFile)
        {
            executionLogfolder = executionLoggerHelper.GetLoggerDirectory(executionLogfolder);
            executionLoggerHelper.CleanDirectory(executionLogfolder, isCleanFile);
            return executionLogfolder;
        }

        public override object SetReportAction(Act action, Context context, Amdocs.Ginger.Common.eExecutedFrom executedFrom, bool offlineMode = false)
        {
            string executionLogFolder = string.Empty;
            if (!offlineMode)
                executionLogFolder = ExecutionLogfolder;
            ActionReport AR = GetActionReportData(action, context, executedFrom);
            if (System.IO.Directory.Exists(Path.Combine(executionLogFolder,action.ExecutionLogFolder)))
            {
                this.SaveObjToReporsitory(AR, Path.Combine(executionLogFolder,action.ExecutionLogFolder,"Action.txt"));

                // Save screenShots
                int screenShotCountPerAction = 0;
                for (var s = 0; s < action.ScreenShots.Count; s++)
                {
                    try
                    {
                        screenShotCountPerAction++;
                        if (executedFrom == Amdocs.Ginger.Common.eExecutedFrom.Automation)
                        {
                            System.IO.File.Copy(action.ScreenShots[s], Path.Combine(executionLogFolder,action.ExecutionLogFolder,"ScreenShot_" + AR.Seq + "_" + screenShotCountPerAction.ToString() + ".png"), true);
                        }
                        else
                        {
                            System.IO.File.Move(action.ScreenShots[s], Path.Combine(executionLogFolder,action.ExecutionLogFolder,"ScreenShot_" + AR.Seq + "_" + screenShotCountPerAction.ToString() + ".png"));
                            action.ScreenShots[s] = Path.Combine(executionLogFolder,action.ExecutionLogFolder,"ScreenShot_" + AR.Seq + "_" + screenShotCountPerAction.ToString() + ".png");
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
                Reporter.ToLog(eLogLevel.ERROR, "Failed to create ExecutionLogger JSON file for the Action :" + action.Description + " because directory not exists :" + Path.Combine(executionLogFolder,action.ExecutionLogFolder));
            }
            return AR;
        }

        public override object SetReportActivity(Activity activity, Context context, bool offlineMode, bool isConfEnable)
        {
            ActivityReport AR = GetActivityReportData(activity, context, offlineMode);
            if (isConfEnable)
            {
                if (offlineMode)
                    // use Path.combine !!!!
                    SaveObjToReporsitory(AR, Path.Combine(activity.ExecutionLogFolder,"Activity.txt"));
                else
                    // use Path.combine !!!!
                    SaveObjToReporsitory(AR, Path.Combine(ExecutionLogfolder,activity.ExecutionLogFolder,"Activity.txt"));
            }
            return AR; 
        }

        public override object SetReportActivityGroup(ActivitiesGroup activityGroup, BusinessFlow businessFlow, bool offlineMode)
        {
            ActivityGroupReport AGR = GetAGReportData(activityGroup, businessFlow);
            //AGR.ReportMapper(activityGroup, businessFlow, ExecutionLogfolder);
            if (offlineMode && activityGroup.ExecutionLogFolder != null)
            {
                SaveObjToReporsitory(AGR, Path.Combine(activityGroup.ExecutionLogFolder,"ActivityGroups.txt"), true);
                File.AppendAllText(Path.Combine(activityGroup.ExecutionLogFolder,"ActivityGroups.txt"), Environment.NewLine);
            }
            else if (ExecutionLogfolder != null && businessFlow.ExecutionLogFolder != null)
            {
                SaveObjToReporsitory(AGR, Path.Combine(ExecutionLogfolder,businessFlow.ExecutionLogFolder,"ActivityGroups.txt"), true);
                File.AppendAllText(Path.Combine(ExecutionLogfolder,businessFlow.ExecutionLogFolder,"ActivityGroups.txt"), Environment.NewLine);
            }
            return AGR;
        }

        public override object SetReportBusinessFlow(Context context, bool offlineMode, Amdocs.Ginger.Common.eExecutedFrom executedFrom, bool isConfEnable)
        {
            BusinessFlowReport BFR = GetBFReportData(context.BusinessFlow, context.Environment);
            if (isConfEnable)
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
                    SaveObjToReporsitory(BFR, Path.Combine(context.BusinessFlow.ExecutionFullLogFolder,"BusinessFlow.txt"));

                }
                else
                {
                    // use Path.cOmbine
                    SaveObjToReporsitory(BFR, Path.Combine(ExecutionLogfolder,context.BusinessFlow.ExecutionLogFolder,"BusinessFlow.txt"));
                    context.BusinessFlow.ExecutionFullLogFolder = Path.Combine(ExecutionLogfolder,context.BusinessFlow.ExecutionLogFolder);
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
            if (gingerRunner.ExecutionLoggerManager.Configuration.ExecutionLoggerConfigurationIsEnabled)
            {
                base.SetReportRunner(gingerRunner, gingerReport, gingerData, mContext, filename, runnerCount);
                SaveObjToReporsitory(gingerReport, Path.Combine(gingerReport.LogFolder, "Ginger.txt"));
            }
        }

        public override void SetReportRunSet(RunSetReport runSetReport, string logFolder)
        {
            base.SetReportRunSet(runSetReport, logFolder);
            if (logFolder == null)
            {
                SaveObjToReporsitory(runSetReport, Path.Combine(runSetReport.LogFolder,"RunSet.txt"));
            }
            else
            {
                SaveObjToReporsitory(runSetReport, Path.Combine(logFolder,"RunSet.txt"));
            }
        }

        internal override void CreateNewDirectory(string logFolder)
        {
            System.IO.Directory.CreateDirectory(logFolder);
        }

        internal override void EndRunSet()
        {
            return;
        }

        public override string GetLogFolder(string folder)
        {
            return folder;
        }

        internal override void SetRunsetFolder(string execResultsFolder, long maxFolderSize, DateTime currentExecutionDateTime, bool offline)
        {
            if (!offline)
                ExecutionLoggerManager.RunSetReport.LogFolder = executionLoggerHelper.GetLoggerDirectory(Path.Combine(execResultsFolder,executionLoggerHelper.folderNameNormalazing(ExecutionLoggerManager.RunSetReport.Name.ToString()) + "_" + currentExecutionDateTime.ToString("MMddyyyy_HHmmss")));
            else
                ExecutionLoggerManager.RunSetReport.LogFolder = executionLoggerHelper.GetLoggerDirectory(execResultsFolder);
            DeleteFolderContentBySizeLimit DeleteFolderContentBySizeLimit = new DeleteFolderContentBySizeLimit(ExecutionLoggerManager.RunSetReport.LogFolder, maxFolderSize);

            executionLoggerHelper.CreateTempDirectory();
        }

        internal override void StartRunSet()
        {
            return;
        }
    }
}
