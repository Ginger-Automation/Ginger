#region License
/*
Copyright © 2014-2025 European Support Limited

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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Utility;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using GingerCore;
using GingerCore.Environments;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;


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
            mBFESs = [];
            BusinessFlowExecutionSummary BFES = new BusinessFlowExecutionSummary
            {
                BusinessFlowName = BF.Name,
                BusinessFlowRunDescription = BF.RunDescription,
                Status = BF.RunStatus,
                Activities = BF.Activities.Count,
                Actions = BF.GetActionsCount(),
                Validations = BF.GetValidationsCount(),
                ExecutionVariabeles = BF.GetBFandActivitiesVariabeles(true),
                BusinessFlow = BF,
                Selected = true
            };
            if (GR != null)
            {
                BFES.BusinessFlowExecLoggerFolder = Path.Combine(GR.ExecutionLoggerManager.ExecutionLogfolder, BF.ExecutionLogFolder);
            }

            if (mBFESs != null)
            {
                mBFESs.Clear();
            }

            mBFESs.Add(BFES);

            mGingersMultiRun = null;
            // Set all General info       
            if (BF.Elapsed != null)
            {
                TotalExecutionTime = TimeSpan.FromSeconds((long)BF.ElapsedSecs);
            }
            else
            {
                TotalExecutionTime = new TimeSpan(0);
            }

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
            if (txtFilesInDirectoryCount is 0 or > 1)
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
                return BusinessFlows.Count;
            }
        }

        public int TotalBusinessFlowsPassed
        {
            get
            {
                return BusinessFlows.Count(x => x.IsPassed);
            }
        }

        public int TotalBusinessFlowsFailed
        {
            get
            {
                return BusinessFlows.Count(x => x.IsFailed);
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
                return AllActivitiesForReport.Count(activity => activity.Status.Equals(Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed.ToString()));

            }
        }
        public int TotalActivitesFailedFromAllFlows
        {
            get
            {
                return AllActivitiesForReport.Count(activity => activity.Status.Equals(Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed.ToString()));

            }
        }
        public int TotalActivitesBlockedFromAllFlows
        {
            get
            {
                return AllActivitiesForReport.Count(activity => activity.Status.Equals(Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked.ToString()));
            }
        }
        public int TotalActivitesSkippedFromAllFlows
        {
            get
            {
                return AllActivitiesForReport.Count(activity => activity.Status.Equals(Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped.ToString()));
            }
        }


        public int TotalActionsPassedFromAllFlows
        {
            get
            {
                return AllActionsForReport.Count(act => act.Status.Equals(Amdocs.Ginger.CoreNET.Execution.eRunStatus.Passed.ToString()));
            }
        }
        public int TotalActionsFailedFromAllFlows
        {
            get
            {
                return AllActionsForReport.Count(act => act.Status.Equals(Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed.ToString()));
            }
        }
        public int TotalActionsBlockedFromAllFlows
        {
            get
            {
                return AllActionsForReport.Count(act => act.Status.Equals(Amdocs.Ginger.CoreNET.Execution.eRunStatus.Blocked.ToString()));
            }
        }
        public int TotalActionsSkippedFromAllFlows
        {
            get
            {
                return AllActionsForReport.Count(act => act.Status.Equals(Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped.ToString()));
            }
        }


        public int TotalBusinessFlowsStopped
        {
            get
            {
                return BusinessFlows.Count(f1 => f1.IsStopped);
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
                    mAllValidationsForReport = AllActionsForReport.SelectMany(act => act.ReturnValueReport).Where(arv => arv.Expected is not null and not "");
                }

                return mAllValidationsForReport;
            }
        }

        //Activities Info
        public int TotalActivitiesCount
        {
            get
            {
                return AllActivitiesForReport.Count();
            }
        }

        public int TotalActivitiesByRunStatus(Amdocs.Ginger.CoreNET.Execution.eRunStatus RunStatus)
        {
            //TODO: fix me to use the same enum on activity add GetActivity on ActivityReport
            return AllActivitiesForReport.Count(activity => activity.Status.Equals(RunStatus.ToString()));
        }

        public int TotalActionsCount()
        {
            return AllActionsForReport.Count();
        }

        public int TotalActionsCountByStatus(Amdocs.Ginger.CoreNET.Execution.eRunStatus Status)
        {
            //TODO: fix me to use the same enum on Act add GetAct on ActivityReport
            return AllActionsForReport.Count(act => act.Status.Equals(Status.ToString()));
        }

        public int TotalValidationsCount()
        {
            int count = AllValidationsForReport.Count();
            return count;
        }

        public int TotalValidationsCountByStatus(ActReturnValue.eStatus Status)
        {
            //TODO: fix me to use the same enum on Act add GetAct on ActivityReport            
            return AllValidationsForReport.Count(arv => arv.Status.Equals(Status.ToString()));
        }

        private List<BusinessFlowReport> GetBziFlowsReport()
        {
            List<BusinessFlowReport> list = [];
            int BizFlowNumber = 0;
            foreach (BusinessFlowExecutionSummary BFES in mBFESs)
            {
                BizFlowNumber++;
                BusinessFlowReport BFR = new(BFES)
                {
                    Seq = BizFlowNumber
                };
                list.Add(BFR);
            }
            return list;
        }
    }
}
