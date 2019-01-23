using Amdocs.Ginger.Run;
using Ginger.Reports;
using Ginger.Run;
using GingerCore;
using GingerCore.Actions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Amdocs.Ginger.CoreNET.Run.RunListenerLib
{
    // TODO: !!!!!!!!!!!!!!!!!!!!!
    // Add handlign for ActivityGroup
    // Add dump of screen shots
    // Compare with Execution Logger


    public class ExecutionDumperListener : RunListenerBase
    {
        static JsonSerializer mJsonSerializer;

        const string ActivityFileName = "Activity.txt";

        int mBusinessFlowCounter = 0;
        int mActivitiesCounter = 0;
        int mActionsCounter = 0;
        string mDumpFolder;
        string CurrentBusinessFlowFolder;        
        string CurrentActivityFolder;
        string CurrentActionFolder;

        public ExecutionDumperListener(string folder)
        {
            mDumpFolder = folder;
            if (mJsonSerializer == null)
            {
                mJsonSerializer = new JsonSerializer();
            }
            Reset();            
        }

        private void Reset()
        {
            mBusinessFlowCounter = 0;
            CleanDumpFolder();            
        }

        private void CleanDumpFolder()
        {
            DirectoryInfo di = new DirectoryInfo(mDumpFolder);
            foreach (FileInfo file in di.EnumerateFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.EnumerateDirectories())
            {
                dir.Delete(true);
            }
        }

        public override void RunnerRunStart(uint eventTime, GingerRunner gingerRunner)
        {
            Reset();
        }

        public override void BusinessFlowStart(uint eventTime, BusinessFlow businessFlow, bool ContinueRun = false)
        {
            mBusinessFlowCounter++;
            mActivitiesCounter = 0;
            CurrentBusinessFlowFolder = Path.Combine(mDumpFolder, mBusinessFlowCounter + " " + businessFlow.GetNameForFileName());
            Directory.CreateDirectory(CurrentBusinessFlowFolder);
        }

        public override void BusinessFlowEnd(uint eventTime, BusinessFlow businessFlow, bool offlineMode = false)
        {
            BusinessFlowReport businessFlowReport = new BusinessFlowReport(businessFlow);
            SaveObjToJSonFile(businessFlowReport, Path.Combine(mDumpFolder, CurrentBusinessFlowFolder, "BusinessFlowReport.txt"));
        }

        public override void ActivityStart(uint eventTime, Activity activity, bool continuerun= false)
        {
            mActivitiesCounter++;
            mActionsCounter = 0;
            CurrentActivityFolder = Path.Combine(CurrentBusinessFlowFolder, mActivitiesCounter + " " + activity.GetNameForFileName());
            Directory.CreateDirectory(CurrentActivityFolder);
        }

        public override void ActivityEnd(uint eventTime, Activity activity, bool offlineMode = false)
        {
            ActivityReport activityReport = new ActivityReport(activity);
            SaveObjToJSonFile(activityReport, Path.Combine(mDumpFolder, CurrentActivityFolder, "ActivityReport.txt"));
        }

        public override void ActionStart(uint eventTime, Act action)
        {
            mActionsCounter++;
            CurrentActionFolder = Path.Combine(CurrentActivityFolder, mActionsCounter + " " + action.FileName);
            Directory.CreateDirectory(CurrentActionFolder);
        }

        public override void ActionEnd(uint eventTime, Act action, bool offlineMode = false)
        {            
            ActionReport actionReport = new ActionReport(action);
            SaveObjToJSonFile(actionReport, Path.Combine(CurrentActivityFolder, CurrentActionFolder, "ActionReport.txt"));
        }



        private static void SaveObjToJSonFile(object obj, string FileName, bool toAppend = false)
        {
            //TODO: for speed we can do it async on another thread...
            using (StreamWriter SW = new StreamWriter(FileName, toAppend))
            using (JsonWriter writer = new JsonTextWriter(SW))
            {
                mJsonSerializer.Serialize(writer, obj);
            }
        }

    }
}
