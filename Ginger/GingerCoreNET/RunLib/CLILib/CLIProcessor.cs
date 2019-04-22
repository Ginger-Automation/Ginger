using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using GingerCore;
using System;
using System.Diagnostics;
using System.IO;

namespace Amdocs.Ginger.CoreNET.RunLib
{
    public class CLIProcessor
    {
        ICLI mCLIHandler;
        public void ExecuteArgs(string[] args)
        {
            Reporter.ToLog(eLogLevel.DEBUG, string.Format("########################## Starting {0} Automatic Execution Process ##########################", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
            Reporter.ToLog(eLogLevel.DEBUG, string.Format("Loading {0} execution UI elements", GingerDicser.GetTermResValue(eTermResKey.RunSet)));

            WorkSpace.Instance.RunningInExecutionMode = true;
            Reporter.ReportAllAlsoToConsole = true;  //needed so all reportering will be added to Console                             
            ConsoleWorkspaceEventHandler consoleWorkspaceEventHandler = new ConsoleWorkspaceEventHandler();

            string[] arg1 = args[0].Split('=');
            string param = arg1[0].Trim();
            string value = arg1[1].Trim();

            SetCLIHandler(param, value);

            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                mCLIHandler.Execute(WorkSpace.Instance.RunsetExecutor);
                stopwatch.Stop();
                Reporter.ToLog(eLogLevel.DEBUG, "Execution Elapsed time: " + stopwatch.Elapsed);

                if (WorkSpace.Instance.RunsetExecutor.RunSetExecutionStatus == Execution.eRunStatus.Passed)
                {
                    Reporter.ToLog(eLogLevel.DEBUG, ">> Run Set executed and passed, exit code: 0");
                    Environment.ExitCode = 0; //success                    
                }
                else
                {
                    Reporter.ToLog(eLogLevel.DEBUG, ">> No indication found for successful execution, exit code: 1");
                    Environment.ExitCode = 1; //failure
                }
            }
            catch (Exception ex)
            {
                Reporter.ToConsole(eLogLevel.ERROR, "Execution Failed with exception: " + ex.Message);
                Reporter.ToLog(eLogLevel.DEBUG,"Exception occured during execution", ex);
                Environment.ExitCode = 1; //failure
            }

        }

        private void SetCLIHandler(string param, string value)
        {
            // TODO: get all classes impl ICLI and check Identifier then set

            switch (param)
            {
                case "--version":
                    Console.WriteLine("Ginger Version " + "???"); //!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    break;
                case "--help":
                    ShowCLIHelp();
                    break;
                case "ConfigFile":
                    mCLIHandler = new CLIConfigFile();
                    string config = ReadFile(value);
                    mCLIHandler.LoadContent(config, WorkSpace.Instance.RunsetExecutor);
                    break;
                case "ScriptFile":
                    mCLIHandler = new CLIScriptFile();
                    string script = ReadFile(value);
                    mCLIHandler.LoadContent(script, null);
                    break;
                case "DynamicFile":
                    mCLIHandler = new CLIDynamicXML();
                    string dynamicXML = ReadFile(value);
                    mCLIHandler.LoadContent(dynamicXML, WorkSpace.Instance.RunsetExecutor);
                    break;
                case "Args":
                    mCLIHandler = new CLIArgs();
                    mCLIHandler.LoadContent(value, WorkSpace.Instance.RunsetExecutor);
                    break;

            }
          
            // TODO: Excel 
        }

        private void ShowCLIHelp()
        {
            // TODO:
            Console.WriteLine("ConfigFile");
            Console.WriteLine("ScriptFile");
            // ....
        }

        //// Return true if there are analyzer issues
        //private bool RunAnalyzer()
        //{
        //    //Running Runset Analyzer to look for issues
        //    Reporter.ToLog(eLogLevel.DEBUG, string.Format("Running {0} Analyzer", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
        //    try
        //    {
        //        //run analyzer
        //        int analyzeRes = runsetExecutor.RunRunsetAnalyzerBeforeRunSync(true);
        //        if (analyzeRes == 1)
        //        {
        //            Reporter.ToLog(eLogLevel.ERROR, string.Format("{0} Analyzer found critical issues with the {0} configurations, aborting execution.", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
        //            return true;//cancel run because issues found
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed Running {0} Analyzer, still continue execution", GingerDicser.GetTermResValue(eTermResKey.RunSet)), ex);
        //        return true;
        //    }
        //    return false;
        //}

        private static string ReadFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Reporter.ToUser(eUserMsgKey.GeneralErrorOccured, "File not found: " + fileName);
                throw new FileNotFoundException("Cannot find file", fileName);
            }
            string txt = File.ReadAllText(fileName);
            return txt;
        }
    }
}
