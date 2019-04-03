using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.Repository;
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Environments;
using GingerCoreNET.RosLynLib;
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

            

            string[] param = args[0].Split('=');
            if (param[0].StartsWith("ScriptFile"))
            {
                ExecutScript(param[1]);
            }            
        }

        private static void ExecutScript(string scriptFile)
        {            
            if (!System.IO.File.Exists(scriptFile))
            {
                Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, "File not found");
                return;
            }
            string script = System.IO.File.ReadAllText(scriptFile);
            var rc = CodeProcessor.ExecuteNew(script);


          
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
