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
            string param;
            string value = null;
            if (args[0].StartsWith("ConfigFile="))
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
                    Console.WriteLine("Ginger Version " + "???"); //!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    break;
                case "--help":
                case "-h":
                    ShowCLIHelp();
                    break;                                    
                case "ConfigFile":
                case "--configfile":
                    mCLIHandler = new CLIConfigFile();
                    string config = ReadFile(value);
                    mCLIHandler.LoadContent(config, WorkSpace.Instance.RunsetExecutor);
                    Execute();
                    break;
                case "--scriptfile":
                    mCLIHandler = new CLIScriptFile();
                    string script = ReadFile(value);
                    mCLIHandler.LoadContent(script, null);
                    Execute();
                    break;
                case "--dynamicfile":
                    mCLIHandler = new CLIDynamicXML();
                    string dynamicXML = ReadFile(value);
                    mCLIHandler.LoadContent(dynamicXML, WorkSpace.Instance.RunsetExecutor);
                    Execute();
                    break;
                case "--args":
                    mCLIHandler = new CLIArgs();
                    mCLIHandler.LoadContent(value, WorkSpace.Instance.RunsetExecutor);
                    Execute();
                    break;
                    // TODO: Excel 
            }


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
                Reporter.ToConsole(eLogLevel.ERROR, "Execution Failed with exception: " + ex.Message);
                Reporter.ToLog(eLogLevel.DEBUG, "Exception occured during execution", ex);
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
