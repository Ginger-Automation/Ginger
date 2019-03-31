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
using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Run.RunListenerLib;
using Amdocs.Ginger.CoreNET.Utility;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Run;
using Ginger.Reports;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.Environments;
using GingerCore.FlowControlLib;
using GingerCore.Variables;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Ginger.Run
{
    // Each ExecutionLogger instance should be added to GingerRunner Listeneres
    // Create new ExecutionLogger for each run 

    public class ExecutionLogger : RunListenerBase
    {
        static JsonSerializer mJsonSerializer;
        public string ExecutionLogfolder { get; set; }
        private eExecutedFrom ExecutedFrom;
        public BusinessFlow mCurrentBusinessFlow;
        public Activity mCurrentActivity;
        uint meventtime;
        IValueExpression mVE;

        ProjEnvironment mExecutionEnvironment = null;

        int mBusinessFlowCounter { get; set; }

        public ProjEnvironment ExecutionEnvironment
        {
            get
            {
                // !!!!!!!!!!!!! called many time ??
                if (mExecutionEnvironment == null)//not supposed to be null but in case it is
                {
                    // !!!!!!!!!!!!!!!!! remove logger should get the env from GR
                    if (this.ExecutedFrom == eExecutedFrom.Automation)
                    {
                        mExecutionEnvironment = WorkSpace.AutomateTabEnvironment;
                    }
                    else
                    {
                        mExecutionEnvironment = WorkSpace.RunsetExecutor.RunsetExecutionEnvironment;
                    }
                }
                return mExecutionEnvironment;
            }
            set
            {
                mExecutionEnvironment = value;
            }
        }

        private GingerReport gingerReport = new GingerReport();
        public static Ginger.Reports.RunSetReport RunSetReport;

        public int ExecutionLogBusinessFlowsCounter = 0;

        GingerRunnerLogger mGingerRunnerLogger;
        private ExecutionLoggerConfiguration mConfiguration = new ExecutionLoggerConfiguration();

        public class ParentGingerData
        {
            public int Seq;
            public string GingerName;
            public string GingerEnv;
            public List<string> GingerAggentMapping;
            public Guid Ginger_GUID;
        };
        public ParentGingerData GingerData = new ParentGingerData();

        // TODO: remove the need for env - get it from notify event !!!!!!
        public ExecutionLogger(ProjEnvironment environment, eExecutedFrom executedFrom = eExecutedFrom.Run)
        {
            mJsonSerializer = new JsonSerializer();
            mJsonSerializer.NullValueHandling = NullValueHandling.Ignore;
            ExecutedFrom = executedFrom;
            ExecutionEnvironment = environment;//needed for supporting diffrent env config per Runner
        }
        // check if needed after remove to manager
        private static void CleanDirectory(string folderName, bool isCleanFile= true)
        {
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(folderName);
            if (isCleanFile)
                foreach (System.IO.FileInfo file in di.GetFiles())
                {
                    file.Delete();
                }
            foreach (System.IO.DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }
        private static void CreateTempDirectory()
        {
            try
            {
                if (!Directory.Exists(WorkSpace.TempFolder))
                {
                    System.IO.Directory.CreateDirectory(WorkSpace.TempFolder);
                }
                else
                {
                    CleanDirectory(WorkSpace.TempFolder);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while creating temporary folder", ex);
            }

        }
        public static string GetLoggerDirectory(string logsFolder)
        {
            logsFolder = logsFolder.Replace(@"~", WorkSpace.Instance.Solution.Folder);
            try
            {
                if(CheckOrCreateDirectory(logsFolder))
                {
                    return logsFolder;
                }
                else
                {
                    //If the path configured by user in the logger is not accessible, we set the logger path to default path
                    logsFolder = System.IO.Path.Combine(WorkSpace.Instance.Solution.Folder, @"ExecutionResults\");
                    System.IO.Directory.CreateDirectory(logsFolder);

                    WorkSpace.Instance.Solution.ExecutionLoggerConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault().ExecutionLoggerConfigurationExecResultsFolder = @"~\ExecutionResults\";
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }

            return logsFolder;
        }

        private static Boolean CheckOrCreateDirectory(string directoryPath)
        {
            try
            {
                if (System.IO.Directory.Exists(directoryPath))
                {
                    return true;
                }
                else
                {
                    System.IO.Directory.CreateDirectory(directoryPath);
                    return true;
                }
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public void SaveObjToJSonFile(object obj, string FileName, bool toAppend = false)
        {
            //TODO: for speed we can do it async on another thread...
            using (StreamWriter SW = new StreamWriter(FileName, toAppend))
            using (JsonWriter writer = new JsonTextWriter(SW))
            {
                mJsonSerializer.Serialize(writer, obj);

            }
        }

        public static object LoadObjFromJSonFile(string FileName, Type t)
        {
            return JsonLib.LoadObjFromJSonFile(FileName, t, mJsonSerializer);
        }

        public static object LoadObjFromJSonString(string str, Type t)
        {
            return JsonLib.LoadObjFromJSonString(str, t, mJsonSerializer);
        }


        public BusinessFlowReport LoadBusinessFlow(string FileName)
        {
            try
            {
                BusinessFlowReport BFR = (BusinessFlowReport)LoadObjFromJSonFile(FileName, typeof(BusinessFlowReport));
                BFR.GetBusinessFlow().ExecutionLogFolder = System.IO.Path.GetDirectoryName(FileName);
                return BFR;
            }
            catch
            {
                return new BusinessFlowReport();
            }
        }
        // same function in extention
        private static string folderNameNormalazing(string folderName)
        {
            foreach (char invalidChar in System.IO.Path.GetInvalidFileNameChars())
            {
                folderName = folderName.Replace(invalidChar.ToString(), "");
            }
            folderName = folderName.Replace(@".", "");
            folderName = folderName.TrimEnd().TrimEnd('-').TrimEnd();
            if (folderName.Length > 30)
            {
                folderName = folderName.Substring(0, 30);
            }
            folderName = folderName.TrimEnd().TrimEnd('-').TrimEnd();
            return folderName;
        }
        
        
    }
}
