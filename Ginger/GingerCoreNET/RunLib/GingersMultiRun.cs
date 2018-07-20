//#region License
///*
//Copyright © 2014-2018 European Support Limited

//Licensed under the Apache License, Version 2.0 (the "License")
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at 

//http://www.apache.org/licenses/LICENSE-2.0 

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS, 
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and 
//limitations under the License. 
//*/
//#endregion

//using Amdocs.Ginger.Common;
//using GingerCoreNET.GeneralLib;
//using GingerCoreNET.ReporterLib;
//using GingerCoreNET.ReportLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.BusinessFlowLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.EnvironmentsLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//namespace GingerCoreNET.RunLib
//{
//    public class GingersMultiRun
//    {
//        public ObservableList<GingerRunner> Gingers = new ObservableList<GingerRunner>();
//        private Stopwatch mStopwatch = new Stopwatch();
//        public TimeSpan Elapsed { get { return mStopwatch.Elapsed; } }
//        private ExecutionLoggerConfiguration _selectedExecutionLoggerConfiguration = new ExecutionLoggerConfiguration();
//        private bool mStopRun;

//        //need to make sure we fill it before run
//        public RunSetConfig RunSetConfig { get; set; }

//        public ObservableList<BusinessFlowExecutionSummary> GetAllBusinessFlowsExecutionSummary(bool GetSummaryOnlyForExecutedFlow = false)
//        {
//            ObservableList<BusinessFlowExecutionSummary> BFESs = new ObservableList<BusinessFlowExecutionSummary>();
//            foreach (GingerRunner ARC in Gingers)
//            {
//                BFESs.Append(ARC.GetAllBusinessFlowsExecutionSummary(GetSummaryOnlyForExecutedFlow,ARC.Name)); 
//            }
//            return BFESs;
//        }

//        internal void CloseAllEnvironments()
//        {
//            foreach (GingerRunner gr in Gingers)
//            {
//                if (gr.UseSpecificEnvironment)
//                {
//                    if (gr.ProjEnvironment != null)
//                        gr.ProjEnvironment.CloseEnvironment();
//                }
//            }
//        }
        
//        public void SetGingersEnv(ProjEnvironment defualtEnv, ObservableList<ProjEnvironment> allEnvs)
//        {
//            foreach (GingerRunner GR in Gingers)
//            {
//                GR.SetExecutionEnvironment(defualtEnv, allEnvs);
//            }
//        }

//        public void SetGingersExecutionResultConf()
//        {
//        }

//        public void CreateGingerExecutionReportAutomaticly()
//        {
//        }

//        public void Run()
//        {
//            mStopRun = false;
//            bool b =VerifyGingersConfig();
//            if (!b)
//            {
//                return;
//            }

//            List<Task> Tasks = new List<Task>();
//            Reset();

//            //// Process all pre run Set Actions
//            mStopwatch.Reset();
//            mStopwatch.Start();
//            if(RunSetConfig.RunModeParallel)
//            {
//                //running parallel 
//                foreach (GingerRunner GR in Gingers)
//                {
//                    Task t = new Task(() => { GR.Run(); }, TaskCreationOptions.LongRunning);
//                    Tasks.Add(t);
//                    t.Start();

//                    // Wait one second before starting another driver
//                    Thread.Sleep(1000);
//                }
//            }
//            else
//            {
//                //running sequentially 
//                Task t = new Task(() =>
//                {
//                    foreach (GingerRunner GR in Gingers)
//                    {
//                        GR.Run();
//                        // Wait one second before starting another driver
//                        Thread.Sleep(1000);
//                    }
//                }, TaskCreationOptions.LongRunning);
//                Tasks.Add(t);
//                t.Start();
//            }

//            //Wait till end of run when all tasks are completed
//            int count = 0;
//            while (count < Tasks.Count) //removing dependency on stop because all Ginger Runners needs to stop first before exit
//            {
//                Thread.Sleep(100);
//                count = (from x in Tasks where x.IsCompleted select x).Count();
//            }
//            mStopwatch.Stop();
//        }

//        private bool VerifyGingersConfig()
//        {
//            List<string> Agents = new List<string>();
//            foreach (GingerRunner GR in Gingers)
//            {
//                foreach (ApplicationAgent AA in GR.ApplicationAgents)
//                {                   
//                    if (string.IsNullOrEmpty(AA.AgentName))
//                    {
//                        if (GR.SolutionApplications.Where(x => (x.AppName == AA.AppName && x.Platform == Platform.ePlatformType.NA)).FirstOrDefault() != null)
//                            continue;                        
//                        Reporter.ToUser(eUserMsgKeys.ApplicationAgentNotMapped, AA.AppName);
//                        return false;
//                    }
//                    string a = (from x in Agents where x == AA.AgentName select x).FirstOrDefault();
//                    if(a==null)
//                    {
//                        Agents.Add(AA.AgentName);
//                    }
//                    else
//                    {
//                        if (RunSetConfig.RunModeParallel)
//                        {
//                            Reporter.ToUser(eUserMsgKeys.FoundDuplicateAgentsInRunSet, a);
//                            // We found duplicate agentname used in different Gingers so break.
//                            return false;
//                        }
//                    }                    
//                }
//            }
//            return true;
//        }
       
//        public void Reset()
//        {
//            foreach (GingerRunner GR in Gingers)
//            {
//                GR.CloseAgents();
//                foreach (BusinessFlow BF in GR.BusinessFlows)
//                {
//                    BF.Reset();
//                }
//            }
//        }

//        internal void StopRun()
//        {
//            mStopRun = true;
//            foreach (GingerRunner ginger in Gingers)
//            {
//                ginger.StopRun();
//            }
//        }
//            }
//}