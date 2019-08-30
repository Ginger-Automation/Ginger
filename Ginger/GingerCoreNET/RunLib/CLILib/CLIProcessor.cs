#region License
/*
Copyright © 2014-2019 European Support Limited

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
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using CommandLine;
using GingerCore;
using GingerCoreNET.RunLib;
using System;
using System.Collections.Generic;
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
            WorkSpace.Instance.RunningInExecutionMode = true;
            Reporter.ReportAllAlsoToConsole = true;

            Reporter.ToLog(eLogLevel.DEBUG, string.Format("########################## Starting Automatic {0} Execution Process ##########################", GingerDicser.GetTermResValue(eTermResKey.RunSet)));

            Reporter.ToLog(eLogLevel.DEBUG, string.Format("Parsing {0} execution arguments...", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
            ConsoleWorkspaceEventHandler consoleWorkspaceEventHandler = new ConsoleWorkspaceEventHandler();

            // This is the new style using verb and CommandLine Nuget to parse
            if (args[0] == "run" || args[0] == "grid")
            {
                ParseArgs(args);
            }
            else
            {
                // Support old style
                string param;
                string value = null;                
                if (args[0].Contains("="))
                {
                    string[] arg1 = args[0].Split(new[] { '=' }, 2);
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

        }

        private void ParseArgs(string[] args)
        {            
            Parser.Default.ParseArguments<RunOptions, GridOptions>(args)
                   .WithParsed<RunOptions>(runOptions =>
                   {
                       // mCLIHelper.CLIType = eCLIType.Arguments;                       
                       mCLIHandler = new CLIArgs();
                       ExecRunOptions(runOptions);
                       // Console.WriteLine($"Solution: {runOptions.Solution}");
                       // Console.WriteLine($"runset: {runOptions.Runset}");
                       // opt = runOptions;
                   }).WithParsed<GridOptions>(gridOptions =>
                   {
                       StartGrid(gridOptions);                                
                   })
                   .WithNotParsed(errs =>
                   {
                       HandleCLIParseError(errs);
                   });
        }

        private void StartGrid(GridOptions gridOptions)
        {
            Console.WriteLine("Starting Ginger Grid at port: " + gridOptions.Port);
            GingerGrid gingerGrid = new GingerGrid(gridOptions.Port);   
            gingerGrid.Start();
        }

        private void ExecRunOptions(RunOptions runOptions)
        {
            Console.WriteLine($"Solution: {runOptions.Solution}");
            Console.WriteLine($"Runset: {runOptions.Runset}");

            // Dup Code!!!!!!!!!!!!!!
            Reporter.ToLog(eLogLevel.DEBUG, "Loading Configurations...");
            // mCLIHandler.LoadContent(configurations, mCLIHelper, WorkSpace.Instance.RunsetExecutor);

            mCLIHelper.Solution = runOptions.Solution;
            mCLIHelper.Runset = runOptions.Runset;
            mCLIHelper.Env = runOptions.Environment;


                if (!mCLIHelper.LoadSolution())
                {
                    return;//failed to load Solution;
                }

                if (!mCLIHelper.LoadRunset(WorkSpace.Instance.RunsetExecutor))
                {
                    return;//failed to load Run set
                }

                if (!mCLIHelper.PrepareRunsetForExecution())
                {
                    return; //Failed to perform execution perpetrations
                }

                mCLIHelper.SetTestArtifactsFolder();
            

            Reporter.ToLog(eLogLevel.DEBUG, string.Format("Executing..."));
            Execute();

            Reporter.ToLog(eLogLevel.DEBUG, "Closing Solution and doing Cleanup...");
            mCLIHelper.CloseSolution();
        }

        private void HandleCLIParseError(IEnumerable<Error> errs)
        {
            //TODO: clean print errs !!!!!!!!!!!!!
            foreach (Error error in errs)
            {         
                //if (error is typeof(BadVerbSelectedError))
                //Console.WriteLine($"arg error: {error.Token} {error.Tag}");
            }
            Console.WriteLine("Please fix arguments and try again");
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
                    mCLIHelper.CLIType = eCLIType.Config;
                    Reporter.ToLog(eLogLevel.DEBUG, string.Format("Running with ConfigFile= '{0}'", value));
                    mCLIHandler = new CLIConfigFile();                   
                    PerformLoadAndExecution(ReadFile(value));
                    break;
                case "Script":
                case "--scriptfile":
                    mCLIHelper.CLIType = eCLIType.Script;
                    Reporter.ToLog(eLogLevel.DEBUG, string.Format("Running with ScriptFile= '{0}'", value));
                    mCLIHandler = new CLIScriptFile();
                    PerformLoadAndExecution(ReadFile(value));
                    break;
                case "--dynamicfile":
                case "Dynamic":
                    mCLIHelper.CLIType = eCLIType.Dynamic;
                    Reporter.ToLog(eLogLevel.DEBUG, string.Format("Running with DynamicXML= '{0}'", value));
                    mCLIHandler = new CLIDynamicXML();
                    PerformLoadAndExecution(ReadFile(value));
                    break;                                
                case "--args":
                    mCLIHelper.CLIType = eCLIType.Arguments;
                    Reporter.ToLog(eLogLevel.DEBUG, string.Format("Running with Command Args= '{0}'", value));
                    mCLIHandler = new CLIArgs();
                    PerformLoadAndExecution(value);
                    break;

                    // Removing unpublished options 
                //case "--excel":
                //    Reporter.ToLog(eLogLevel.DEBUG, string.Format("Running with CLI Excel= '{0}'", value));
                //    mCLIHandler = new CLIExcel();
                //    PerformLoadAndExecution(value);
                //    break;                
                //case "--servicegrid":
                //    int port = 15555;
                //    Console.WriteLine("Starting Ginger Grid at port: " + port);
                //    GingerGrid gingerGrid = new GingerGrid(15555);   // TODO: get port from CLI arg
                //    gingerGrid.Start();

                //    ServiceGridTracker serviceGridTracker = new ServiceGridTracker(gingerGrid);

                //    Console.ReadKey();
                //    break;                
                default:
                    Console.WriteLine("Error - Unknown Command Line Argument(s): " + param);
                    break;
            }
        }

        private void PerformLoadAndExecution(string configurations)
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Loading Configurations...");
            mCLIHandler.LoadContent(configurations, mCLIHelper, WorkSpace.Instance.RunsetExecutor);

            if (mCLIHelper.CLIType != eCLIType.Script)
            {
                if (!mCLIHelper.LoadSolution())
                {
                    return;//failed to load Solution;
                }

                if (!mCLIHelper.LoadRunset(WorkSpace.Instance.RunsetExecutor))
                {
                    return;//failed to load Run set
                }

                if (!mCLIHelper.PrepareRunsetForExecution())
                {
                    return; //Failed to perform execution perperations
                }

                mCLIHelper.SetTestArtifactsFolder();                
            }

            Reporter.ToLog(eLogLevel.DEBUG, string.Format("Executing..."));
            Execute();

            Reporter.ToLog(eLogLevel.DEBUG, "Closing Solution and doing Cleanup...");
            mCLIHelper.CloseSolution();
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
                    Reporter.ToLog(eLogLevel.INFO, string.Format(">> {0} executed and passed, exit code: 0", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                    Environment.ExitCode = 0; //success                    
                }
                else
                {
                    Reporter.ToLog(eLogLevel.WARN, string.Format(">> No indication found for successful {0} execution, exit code: 1", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
                    Environment.ExitCode = 1; //failure
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, string.Format("Unexpected exception occured during {0} execution, exit code 1", GingerDicser.GetTermResValue(eTermResKey.RunSet)), ex);
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
