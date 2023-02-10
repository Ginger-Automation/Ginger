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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.Reports;
using Ginger.Reports.GingerExecutionReport;
using Ginger.Run;
using GingerCore;
using GingerCore.Environments;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Amdocs.Ginger.CoreNET.RosLynLib
{
    // This is Roslyn global file which is passed to the script code
    // DO NOT put here general code unless it is intended to be used from script
    // DO NOT change functions signature once published as we might have script which use them, or make sure it is backward compatible

    public class GingerScriptGlobals
    {
        //TODO: we can put global vars too
        // TODO: Just example delete me later
        public int X = 1;
        public int Y;

        PluginPackage P;

        public void LoadPluginPackage(string folder)
        {
            Console.WriteLine("* Loading Plugin - " + folder);
            P = new PluginPackage(folder);
            P.PluginPackageOperations = new PluginPackageOperations(P);
            P.PluginPackageOperations.LoadPluginPackage(folder);
            Console.WriteLine("* Plugin Loaded");
        }


        public void Sleep(int millisecondsTimeout)
        {
            Thread.Sleep(millisecondsTimeout);
        }



        public void OpenSolution(string solutionFolder)
        {
            WorkSpace.Instance.OpenSolution(solutionFolder);
        }

        public async void OpenRunSet(string runSetName, string envName)
        {
            SolutionRepository SR = WorkSpace.Instance.SolutionRepository;
            var envs = SR.GetAllRepositoryItems<ProjEnvironment>();
            ProjEnvironment projEnvironment = (from x in SR.GetAllRepositoryItems<ProjEnvironment>() where x.Name == envName select x).SingleOrDefault();
            RunSetConfig runSetConfig = (from x in SR.GetAllRepositoryItems<RunSetConfig>() where x.Name == runSetName select x).SingleOrDefault();
            RunsetExecutor runsetExecutor = new RunsetExecutor();
            WorkSpace.Instance.RunsetExecutor = runsetExecutor;
            runsetExecutor.RunSetConfig = runSetConfig;
            runsetExecutor.RunsetExecutionEnvironment = projEnvironment;
            runsetExecutor.InitRunners();            
            await runsetExecutor.RunRunset();            
        }

        /// <summary>
        /// Create summary json of the execution
        /// </summary>
        /// <param name="fileName"></param>
        public void CreateExecutionSummaryJSON(string fileName)
        {            
            string s = WorkSpace.Instance.RunsetExecutor.CreateSummary();
            System.IO.File.WriteAllText(fileName, s);
        }


        /// <summary>
        /// Create summary json of the execution
        /// </summary>
        /// <param name="fileName"></param>
        public void CreateExecutionHTMLReport(string fileName)
        {
            WorkSpace.Instance.RunsetExecutor.CreateGingerExecutionReportAutomaticly();


            HTMLReportsConfiguration currentConf = WorkSpace.Instance.Solution.HTMLReportsConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            HTMLReportConfiguration htmlRep = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems< HTMLReportConfiguration>().Where(x => (x.IsSelected == true)).FirstOrDefault();
            //if (grdExecutionsHistory.CurrentItem == null)
            //{
            //    Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
            //    return;
            //}

            // string runSetFolder = ExecutionLogger.GetLoggerDirectory(((RunSetReport)grdExecutionsHistory.CurrentItem).LogFolder);

            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!! temp 
            string runSetFolder = @"C:\Users\yaronwe\source\repos\Ginger\Ginger\GingerCoreNETUnitTest\bin\Debug\netcoreapp2.2\TestResources\Solutions\CLI\ExecutionResults\Default Run Set_04082019_115742";

            string reportsResultFolder = ExtensionMethods.CreateGingerExecutionReport(new ReportInfo(runSetFolder), false, htmlRep, null, false, currentConf.HTMLReportConfigurationMaximalFolderSize);

            if (reportsResultFolder == string.Empty)
            {
                Reporter.ToUser(eUserMsgKey.NoItemWasSelected);
                return;
            }
            else
            {
                Process.Start(new ProcessStartInfo() { FileName = reportsResultFolder, UseShellExecute = true });
                Process.Start(new ProcessStartInfo() { FileName = reportsResultFolder + "\\" + "GingerExecutionReport.html", UseShellExecute = true });
            }


            // System.IO.File.WriteAllText(fileName, s);

            // ExtensionMethods.CreateGingerExecutionReport()
        }



        public void RunBusinessFlow(string name)
        {                        
            BusinessFlow BF = (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>() where x.Name == name select x).SingleOrDefault();
            if (BF == null)
            {
                // !!! Err
            }

            RunFlow(BF);
        }


        private void RunFlow(BusinessFlow businessFlow)
        {
            GingerRunner gingerRunner = new GingerRunner();
            ExecutionLoggerManager ex = (ExecutionLoggerManager)((GingerExecutionEngine)gingerRunner.Executor).RunListeners[0];  // temp until we remove it from GR constructor and add manually


            //!!!!!!!!!!!

            ex.ExecutionLogfolder = @"c:\temp\jj";   // !!!!!!!!!!!!!!!!!
            ex.Configuration.ExecutionLoggerConfigurationIsEnabled = true;
            //ex.exec
            // ex.Configuration.exe
            // TODO: add dumper

            ProjEnvironment projEnvironment = new ProjEnvironment();
            //ExecutionDumperListener executionDumperListener = new ExecutionDumperListener(@"c:\temp\dumper");   // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!! temp
            //gingerRunner.RunListeners.Add(executionDumperListener);


            // executionLogger = new ExecutionLogger(projEnvironment, eExecutedFrom.Automation);
            // executionLogger.Configuration.ExecutionLoggerConfigurationIsEnabled = true;
            // gingerRunner.RunListeners.Add(executionLogger);
            gingerRunner.Executor.BusinessFlows.Clear();
            gingerRunner.Executor.BusinessFlows.Add(businessFlow);
            gingerRunner.Executor.CurrentBusinessFlow = businessFlow;
            gingerRunner.Executor.RunRunner();

            Console.WriteLine("Execution Completed");
            Console.WriteLine("----------------------------");
            Console.WriteLine("Elapsed: " + businessFlow.Elapsed);
            Console.WriteLine(GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + ": " + businessFlow.Name);
            Console.WriteLine(GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Description: " + businessFlow.Description);
            Console.WriteLine(GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Status: " + businessFlow.RunStatus);
            Console.WriteLine(GingerDicser.GetTermResValue(eTermResKey.Activities) + " Count: " + businessFlow.Activities.Count);
            Console.WriteLine("----------------------------");
        }


    }
}
