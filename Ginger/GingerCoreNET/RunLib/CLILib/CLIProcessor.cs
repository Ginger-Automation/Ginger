using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
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
        CLIHelper mCLIHelper = new CLIHelper();

        public void ExecuteArgs(string[] args)
        {
            Reporter.ToLog(eLogLevel.DEBUG, string.Format("########################## Starting {0} Automatic Execution Process ##########################", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
            Reporter.ToLog(eLogLevel.DEBUG, string.Format("Loading {0} execution UI elements", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                          
            ConsoleWorkspaceEventHandler consoleWorkspaceEventHandler = new ConsoleWorkspaceEventHandler();
            string param;
            string value = null;
            if (args[0].StartsWith("ConfigFile=") || args[0].StartsWith("DynamicXML="))  // special case to support backword compatibility of old style ConfigFile=%filename%
            {
                string[] arg1 = args[0].Split('=');
                param = arg1[0].Trim();
                value = arg1[1].Trim();                
            }
            else
            {                
                param = args[0].Trim();
                if (args.Length > 1)
                {
                    value = args[1].Trim();
                }                
            }
            HandleArg(param, value);           
        }

        private void HandleArg(string param, string value)
        {
            // TODO: get all classes impl ICLI and check Identifier then set

            switch (param)
            {
                case "--version":
                    Console.WriteLine(string.Format("{0} Version: {1}", ApplicationInfo.ApplicationName, ApplicationInfo.ApplicationVersionWithInfo));
                    break;
                case "--help":
                case "-h":
                    ShowCLIHelp();
                    break;
                case "ConfigFile":
                case "--configfile":
                    Reporter.ToLog(eLogLevel.DEBUG, string.Format("Running with ConfigFile= '{0}'", value));
                    mCLIHandler = new CLIConfigFile();                   
                    PerformLoadAndExecution(ReadFile(value));
                    break;
                case "--scriptfile":
                    Reporter.ToLog(eLogLevel.DEBUG, string.Format("Running with ScriptFile= '{0}'", value));
                    mCLIHandler = new CLIScriptFile();
                    PerformLoadAndExecution(ReadFile(value));
                    break;
                case "--dynamicfile":
                case "DynamicXML":
                    Reporter.ToLog(eLogLevel.DEBUG, string.Format("Running with DynamicXML= '{0}'", value));
                    mCLIHandler = new CLIDynamicXML();
                    PerformLoadAndExecution(ReadFile(value));
                    break;
                case "--args":
                    Reporter.ToLog(eLogLevel.DEBUG, string.Format("Running with Command Args= '{0}'", value));
                    mCLIHandler = new CLIArgs();
                    PerformLoadAndExecution(value);
                    break;
                case "--excel":
                    Reporter.ToLog(eLogLevel.DEBUG, string.Format("Running with CLI Excel= '{0}'", value));
                    mCLIHandler = new CLIExcel();
                    PerformLoadAndExecution(value);
                    break;
            }
        }

        private void PerformLoadAndExecution(string configurations)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Loading Configurations...");
            mCLIHandler.LoadContent(configurations, mCLIHelper, WorkSpace.Instance.RunsetExecutor);
            Reporter.ToLog(eLogLevel.DEBUG, "Loading Solution...");
            mCLIHelper.ProcessArgs(WorkSpace.Instance.RunsetExecutor);
            Reporter.ToLog(eLogLevel.DEBUG, "Executing based on Configurations...");
            Execute();
        }

        void Execute()
        {
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
                Reporter.ToLog(eLogLevel.ERROR, "Exception occured during execution", ex);
                Environment.ExitCode = 1; //failure
            }
        }

        private void ShowCLIHelp()
        {
            // TODO:
            Console.WriteLine("Ginger CLI options");
            Console.WriteLine("Use -- for full argument name or - for short name");
            Console.WriteLine(@"Use space to seperate arguments and value for example Ginger.exe --solution c:\ginger\solution1 --environment UAT");
            Console.WriteLine(@"List of available arguments");
            Console.WriteLine("--version Display ginger version");
            Console.WriteLine("--help -h Display Ginger help");
            Console.WriteLine("--configfile %filename% or ConfigFile=%filename% auto run as per detailed in %filename%");
            Console.WriteLine("--scriptfile %scriptfilename% run GingerScript file");
            Console.WriteLine("--args GingerScript with args");            

            // TODO: add more details....
        }

        

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
