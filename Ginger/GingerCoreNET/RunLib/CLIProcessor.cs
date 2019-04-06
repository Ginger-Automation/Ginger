using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Ginger.Run;
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
            //if (Environment.GetCommandLineArgs().Count() > 1)
            //{
            //    // When running from unit test there are args, so we set a flag in GingerAutomator to make sure Ginger will Launch
            //    // and will not try to process the args for RunSet auto run
            //    if (RunningFromUnitTest)
            //    {
            //        // do nothing for now, but later on we might want to process and check auto run too
            //    }
            //    else
            //    {
            //        // This Ginger is running with run set config will do the run and close Ginger
            //        WorkSpace.RunningInExecutionMode = true;
            //        Reporter.ReportAllAlsoToConsole = true; //needed so all reportering will be added to Consol
            //        //Reporter.AppLogLevel = eAppReporterLoggingLevel.Debug;//needed so all reportering will be added to Log file
            //        //RunRunSet();
            //        CLI.ExecuteArgs(Environment.GetCommandLineArgs());
            //    }
            //}


            RunRunSet();
        }


        public static SolutionRepository SR;
        private static void RunRunSet()
        {

            ConsoleWorkspaceEventHandler consoleWorkspaceEventHandler = new ConsoleWorkspaceEventHandler();

            WorkSpace.Init(consoleWorkspaceEventHandler);   // is needed !!!
            OpenSolution(@"C:\yaron\GingerSolution\Plugins\Plugins");
            var x1 = SR.GetAllRepositoryItems<BusinessFlow>();
            BusinessFlow BF = (from x in SR.GetAllRepositoryItems<BusinessFlow>() where x.Name == "Dummy" select x).SingleOrDefault();
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
