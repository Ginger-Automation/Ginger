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

using System;
using System.Collections.Generic;
using System.Linq;
using Ginger.Run;
using GingerCore;
using GingerCore.Environments;
using System.IO;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Amdocs.Ginger.CoreNET.Utility;
using Amdocs.Ginger.Common.InterfacesLib;


namespace Ginger.Reports
{
    public class ReportInfo : IReportInfo
    {
        /// <summary>
        /// Should be deleted after switch will be fully done to serialized objects 
        /// </summary>
        public List<BusinessFlowReport> BusinessFlows { get { return GetBziFlowsReport(); } }

        /// <summary>
        /// Should be deleted after switch will be fully done to serialized objects
        /// </summary>                                                                                            
        public List<BusinessFlowReport> BusinessFlowReports { get; set; }

        public enum ReportInfoLevel
        {
            Unknown,
            RunSetLevel,
            GingerLevel,
            BussinesFlowLevel,
            ActivityGroupLevel,
            ActivityLevel,
            ActionLevel
        }
        public ReportInfoLevel reportInfoLevel = ReportInfoLevel.Unknown;

        private ObservableList<BusinessFlowExecutionSummary> mBFESs;
        private ProjEnvironment mProjEnvironment;
        private RunsetExecutor mGingersMultiRun;

        public string DateCreatedShort { get; set; }
        public string DateCreated { get; set; }
        public string ExecutionEnv { get; set; }
        public TimeSpan ExecutionElapsedTime { get; set; }

        /// <summary>
        /// The root item of the report which can be RunSet, Runner, BF
        /// </summary>
        public Object ReportInfoRootObject;

        public EnvironmentReport Environment { get; set; }

        public TimeSpan TotalExecutionTime { get; set; }

        /// <summary>
        /// Should be deleted after switch will be fully done to serialized objects 
        /// </summary> 
        public ReportInfo(ProjEnvironment Env, RunsetExecutor GMR, bool ReportOnlyExecuted = false)   // to remove after discussion !!!
        {
            mProjEnvironment = Env;
            mBFESs = GMR.GetAllBusinessFlowsExecutionSummary(ReportOnlyExecuted);

            mGingersMultiRun = GMR;
            // Set all General info            
            TotalExecutionTime = mGingersMultiRun.Elapsed;

            DateCreated = DateTime.Now.ToString();
            DateCreatedShort = DateTime.Now.ToString("MM/dd");
            ExecutionEnv = mProjEnvironment.Name;
            ExecutionElapsedTime = mGingersMultiRun.Elapsed;
        }

        /// <summary>
        /// Should be deleted after switch will be fully done to serialized objects 
        /// </summary> 
        public ReportInfo(ProjEnvironment Env, GingerExecutionEngine GR, bool ReportOnlyExecuted = false) // to remove after discussion !!!
        {
            mProjEnvironment = Env;
            mBFESs = GR.GetAllBusinessFlowsExecutionSummary(ReportOnlyExecuted);

            TotalExecutionTime = TimeSpan.FromMilliseconds(GR.Elapsed.GetValueOrDefault());

            DateCreated = DateTime.Now.ToString();
            DateCreatedShort = DateTime.Now.ToString("MM/dd");
            ExecutionEnv = mProjEnvironment.Name;
            ExecutionElapsedTime = TotalExecutionTime;
        }

        /// <summary>
        /// Should be deleted after switch will be fully done to serialized objects 
        /// </summary> 
        public ReportInfo(ProjEnvironment Env, BusinessFlow BF, GingerExecutionEngine GR = null) // to remove after discussion !!!
        {
            mProjEnvironment = Env;
            mBFESs = new ObservableList<BusinessFlowExecutionSummary>();
            BusinessFlowExecutionSummary BFES = new BusinessFlowExecutionSummary();

            BFES.BusinessFlowName = BF.Name;
            BFES.BusinessFlowRunDescription = BF.RunDescription;
            BFES.Status = BF.RunStatus;
            BFES.Activities = BF.Activities.Count;
            BFES.Actions = BF.GetActionsCount();
            BFES.Validations = BF.GetValidationsCount();
            BFES.ExecutionVariabeles = BF.GetBFandActivitiesVariabeles(true);
            BFES.BusinessFlow = BF;
            BFES.Selected = true;
            if (GR != null)
                BFES.BusinessFlowExecLoggerFolder = Path.Combine(GR.ExecutionLoggerManager.ExecutionLogfolder, BF.ExecutionLogFolder);

            if (mBFESs != null) mBFESs.Clear();
            mBFESs.Add(BFES);

            mGingersMultiRun = null;
            // Set all General info       
            if (BF.Elapsed != null)
                TotalExecutionTime = TimeSpan.FromSeconds((long)BF.ElapsedSecs);
            else
                TotalExecutionTime = new TimeSpan(0);

            ExecutionElapsedTime = TotalExecutionTime;
            DateCreated = DateTime.Now.ToString();
            ExecutionEnv = mProjEnvironment.Name;
            DateCreatedShort = DateTime.Now.ToString("MM/dd");
        }

        // Use it when we have data from disk which ws saved by ExecutionLogger
        public ReportInfo(string folder)    // this is only that should stayed after discussion !!!
        {
            // in received folder looking for json file with specific name (file should be single txt file in folder - if no - not proceed with deserialization)
            int txtFilesInDirectoryCount = 0;
            string txtFileName = string.Empty;
            string fileName = string.Empty;
            foreach (string txt_file in System.IO.Directory.GetFiles(folder))
            {
                fileName = Path.GetFileName(txt_file);
                if (fileName.Contains(".txt") && (fileName != "ActivityGroups.txt"))     // !!!!!!!!!!!!!!!!!!!!!!!!
                {
                    txtFilesInDirectoryCount++;
                    txtFileName = fileName;
                }
            }
            if ((txtFilesInDirectoryCount == 0) || (txtFilesInDirectoryCount > 1))
            {
                return;
            }

            // setting ReportInfoRootObject and ReportInfoLevel according to jason that folder pointing on
            string curFileWithPath = String.Empty;
            try
            {
                switch (txtFileName)
                {
                    case "RunSet.txt":
                        curFileWithPath = Path.Combine(folder, "RunSet.txt");
                        ReportInfoRootObject = (RunSetReport)JsonLib.LoadObjFromJSonFile(curFileWithPath, typeof(RunSetReport));
                        ((RunSetReport)ReportInfoRootObject).LogFolder = folder;
                        reportInfoLevel = ReportInfo.ReportInfoLevel.RunSetLevel;
                        break;
                    case "Ginger.txt":
                        curFileWithPath = folder + @"\Ginger.txt";
                        ReportInfoRootObject = (GingerReport)JsonLib.LoadObjFromJSonFile(curFileWithPath, typeof(GingerReport));
                        ((GingerReport)ReportInfoRootObject).LogFolder = folder;
                        reportInfoLevel = ReportInfo.ReportInfoLevel.GingerLevel;
                        break;
                    case "BusinessFlow.txt":
                        curFileWithPath = folder + @"\BusinessFlow.txt";
                        ReportInfoRootObject = (BusinessFlowReport)JsonLib.LoadObjFromJSonFile(curFileWithPath, typeof(BusinessFlowReport));
                        ((BusinessFlowReport)ReportInfoRootObject).LogFolder = folder;
                        reportInfoLevel = ReportInfo.ReportInfoLevel.BussinesFlowLevel;
                        break;
                    case "Activity.txt":
                        curFileWithPath = folder + @"\Activity.txt";
                        ReportInfoRootObject = (ActivityReport)JsonLib.LoadObjFromJSonFile(curFileWithPath, typeof(ActivityReport));
                        ((ActivityReport)ReportInfoRootObject).LogFolder = folder;
                        reportInfoLevel = ReportInfo.ReportInfoLevel.ActivityLevel;
                        break;
                    case "Action.txt":
                        curFileWithPath = folder + @"\Action.txt";
                        ReportInfoRootObject = (ActionReport)JsonLib.LoadObjFromJSonFile(curFileWithPath, typeof(ActionReport));
                        ((ActionReport)ReportInfoRootObject).LogFolder = folder;
                        reportInfoLevel = ReportInfo.ReportInfoLevel.ActionLevel;
                        break;
                    default:
                        ReportInfoRootObject = null;
                        return;
                }
            }
            catch (Exception EC)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to Deserialize report Json file type", EC);
            }
        }

        public int TotalBusinessFlows
        {
            get
            {
                return BusinessFlows.Count();
            }
        }

        public int TotalBusinessFlowsPassed
        {
            get
            {
                int count = (from x in BusinessFlows where x.IsPassed == true select x).Count();
                return count;
            }
        }

        public int TotalBusinessFlowsFailed
        {
            get
            {
                int count = (from x in BusinessFlows where x.IsFailed == true select x).Count();
                return count;
            }
        }

        public int TotalBusinessFlowsBlocked
        {
            get
            {
                return (TotalBusinessFlows - (TotalBusinessFlowsFailed + TotalBusinessFlowsPassed));
            }
        }
        public int TotalActivitesPassedFromAllFlows
        {
            get
            {
                string sStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed.ToString();
                int count = AllActivitiesForReport.Where(activity => activity.Status == sStatus).Count();
                return count;

            }
        }
        public int TotalActivitesFailedFromAllFlows
        {
            get
            {
                string sStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed.ToString();
                int count = AllActivitiesForReport.Where(activity => activity.Status == sStatus).Count();
                return count;

            }
        }
        public int TotalActivitesBlockedFromAllFlows
        {
            get
            {
                string sStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked.ToString();
                int count = AllActivitiesForReport.Where(activity => activity.Status == sStatus).Count();
                return count;

            }
        }
        public int TotalActivitesSkippedFromAllFlows
        {
            get
            {
                string sStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped.ToString();
                int count = AllActivitiesForReport.Where(activity => activity.Status == sStatus).Count();
                return count;

            }
        }


        public int TotalActionsPassedFromAllFlows
        {
            get
            {
                string sStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed.ToString();
                int count = AllActionsForReport.Where(act => act.Status == sStatus).Count();
                return count;

            }
        }
        public int TotalActionsFailedFromAllFlows
        {
            get
            {
                string sStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed.ToString();
                int count = AllActionsForReport.Where(act => act.Status == sStatus).Count();
                return count;

            }
        }
        public int TotalActionsBlockedFromAllFlows
        {
            get
            {
                string sStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked.ToString();
                int count = AllActionsForReport.Where(act => act.Status == sStatus).Count();
                return count;

            }
        }
        public int TotalActionsSkippedFromAllFlows
        {
            get
            {
                string sStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped.ToString();
                int count = AllActionsForReport.Where(act => act.Status == sStatus).Count();
                return count;

            }
        }


        public int TotalBusinessFlowsStopped
        {
            get
            {
                int count = (from f1 in BusinessFlows where f1.IsStopped == true select f1).Count();
                return count;
            }
        }


        IEnumerable<ActivityReport> mAllActivitiesForReport = null;
        public IEnumerable<ActivityReport> AllActivitiesForReport
        {
            // We get all Active Activities which are not error handler
            get
            {
                // We cache it for next time, so gen only one time
                if (mAllActivitiesForReport == null)
                {
                    mAllActivitiesForReport = BusinessFlows.SelectMany(b => b.Activities).Where(z => z.Active == true && z.GetType() != typeof(IErrorHandler));
                }

                return mAllActivitiesForReport;
            }
        }

        IEnumerable<ActionReport> mAllActionsForReport = null;
        public IEnumerable<ActionReport> AllActionsForReport
        {
            // We get all Active Actions
            get
            {
                // We cache it for next time, so gen only one time
                if (mAllActionsForReport == null)
                {
                    mAllActionsForReport = AllActivitiesForReport.SelectMany(activity => activity.Actions).Where(act => act.Active == true);
                }

                return mAllActionsForReport;
            }
        }

        IEnumerable<ReturnValueReport> mAllValidationsForReport = null;
        public IEnumerable<ReturnValueReport> AllValidationsForReport
        {
            // We get all Validation for ARV which are active and have Expected value
            get
            {
                // We cache it for next time, so gen only one time
                if (mAllValidationsForReport == null)
                {
                    mAllValidationsForReport = AllActionsForReport.SelectMany(act => act.ReturnValueReport).Where(arv => arv.Expected != null && arv.Expected != "");
                }

                return mAllValidationsForReport;
            }
        }

        //Activities Info
        public int TotalActivitiesCount
        {
            get
            {
                int count = AllActivitiesForReport.Count();
                return count;
            }
        }

        public int TotalActivitiesByRunStatus(Amdocs.Ginger.CoreNET.Execution.eRunStatus RunStatus)
        {
            //TODO: fix me to use the same enum on activity add GetActivity on ActivityReport
            string sStatus = RunStatus.ToString();
            int count = AllActivitiesForReport.Where(activity => activity.Status == sStatus).Count();
            return count;
        }

        public int TotalActionsCount()
        {
            int count = AllActionsForReport.Count();
            return count;
        }

        public int TotalActionsCountByStatus(Amdocs.Ginger.CoreNET.Execution.eRunStatus Status)
        {
            //TODO: fix me to use the same enum on Act add GetAct on ActivityReport
            string sStatus = Status.ToString();
            int count = AllActionsForReport.Where(act => act.Status == sStatus).Count();
            return count;
        }

        public int TotalValidationsCount()
        {
            int count = AllValidationsForReport.Count();
            return count;
        }

        public int TotalValidationsCountByStatus(ActReturnValue.eStatus Status)
        {
            //TODO: fix me to use the same enum on Act add GetAct on ActivityReport
            string sStatus = Status.ToString();
            int count = AllValidationsForReport.Where(arv => arv.Status == sStatus).Count();
            return count;
        }

        private List<BusinessFlowReport> GetBziFlowsReport()
        {
            List<BusinessFlowReport> list = new List<BusinessFlowReport>();
            int BizFlowNumber = 0;
            foreach (BusinessFlowExecutionSummary BFES in mBFESs)
            {
                BizFlowNumber++;
                BusinessFlowReport BFR = new BusinessFlowReport(BFES);
                BFR.Seq = BizFlowNumber;
                list.Add(BFR);
            }
            return list;
        }
    }
}
