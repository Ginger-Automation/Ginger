using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Environments;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Amdocs.Ginger.CoreNET.RunLib
{
    public class CLIProcessor
    {
        public static void ExecuteArgs(string[] args)
        {
            WorkSpace.Instance.RunningInExecutionMode = true;
            Reporter.ReportAllAlsoToConsole = true;  //needed so all reportering will be added to Console                             

            ConsoleWorkspaceEventHandler consoleWorkspaceEventHandler = new ConsoleWorkspaceEventHandler();
            
            // OpenSolution(@"C:\yaron\GingerSolution\Plugins\Plugins");

            // Temp !!!!!
            //string solutionFolder = @"C:\temp\CLI\CLI";
            string solutionFolder = @"C:\Yaron\GingerSolutions\Ginger Demo";
            OpenSolution(solutionFolder);
            string SolFile = System.IO.Path.Combine(solutionFolder, @"Ginger.Solution.xml");            
            WorkSpace.Instance.Solution = WorkSpace.Instance.Solution = Solution.LoadSolution(SolFile);
            WorkSpace.Instance.Solution.SetReportsConfigurations();
            RunRunSet();
        }

        private static void RunRunSet()
        {
            // !!!!!!!! cleanup
            //var zz = SR.GetAllRepositoryItems<RunSetConfig>();
            var envs = SR.GetAllRepositoryItems<ProjEnvironment>();

            // ProjEnvironment projEnvironment = (from x in SR.GetAllRepositoryItems<ProjEnvironment>() where x.Name == "Default" select x).SingleOrDefault();
            ProjEnvironment projEnvironment = (from x in SR.GetAllRepositoryItems<ProjEnvironment>() where x.Name == "CMI IIS test server - DEV" select x).SingleOrDefault();
            // 
            RunSetConfig runSetConfig = (from x in SR.GetAllRepositoryItems<RunSetConfig>() where x.Name == "Default Run Set" select x).SingleOrDefault();
            RunsetExecutor runsetExecutor = new RunsetExecutor();
            WorkSpace.Instance.RunsetExecutor = runsetExecutor;
            runsetExecutor.RunSetConfig = runSetConfig;
            runsetExecutor.RunsetExecutionEnvironment = projEnvironment;
            //runsetExecutor.ConfigureAllRunnersForExecution();
            // runsetExecutor.LoadRunSetConfig(runSetConfig);
            // runsetExecutor.SetRunnersExecutionLoggerConfigs();

            runsetExecutor.InitRunners();
            BusinessFlow bf  = runsetExecutor.Runners[0].BusinessFlows[0];
            runsetExecutor.RunRunset();
            
            string json = runsetExecutor.CreateSummary(runsetExecutor);
            // temp !!!!!!!!!!!!!!!!!!!!
            System.IO.File.WriteAllText(@"c:\temp\ExecutionSummary.json", json, System.Text.Encoding.Default);
        }

        public static SolutionRepository SR;
        private static void RunBF()
        {

            
            var x1 = SR.GetAllRepositoryItems<BusinessFlow>();
            BusinessFlow BF = (from x in SR.GetAllRepositoryItems<BusinessFlow>() where x.Name == "SCM - Create Customer 1" select x).SingleOrDefault();
            if (BF == null)
            {
                // !!! Err
            }

            RunFlow(BF);            
        }


        // Combine to one in core !!!!!!!!!!!!!!!!!!!!!

        private static void OpenSolution(string sFolder)
        {
            if (Directory.Exists(sFolder))
            {
                Console.WriteLine("Opening Solution at folder: " + sFolder);
                SR = GingerSolutionRepository.CreateGingerSolutionRepository();
                WorkSpace.Instance.SolutionRepository = SR;
                SR.Open(sFolder);
            }
            else
            {
                Console.WriteLine("Directory not found: " + sFolder);
            }
        }


        static void RunFlow(BusinessFlow businessFlow)
        {
            GingerRunner gingerRunner = new GingerRunner();
            ExecutionLogger ex = (ExecutionLogger)gingerRunner.RunListeners[0];  // temp until we remove it from GR constructor and add manually
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
            gingerRunner.BusinessFlows.Clear();
            gingerRunner.BusinessFlows.Add(businessFlow);
            gingerRunner.CurrentBusinessFlow = businessFlow;
            gingerRunner.RunRunner();

            Console.WriteLine("Execution Completed");
            Console.WriteLine("----------------------------");
            Console.WriteLine("Elapsed: " + businessFlow.Elapsed);
            Console.WriteLine("Business Flow: " + businessFlow.Name);
            Console.WriteLine("Business Flow Description: " + businessFlow.Description);
            Console.WriteLine("Business Flow Status: " + businessFlow.RunStatus);
            Console.WriteLine("Activities Count: " + businessFlow.Activities.Count);
            Console.WriteLine("----------------------------");
        }
    }
}
