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
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using CommandLine;
using GingerCore;
using GingerCoreNET.RunLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

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
            ConsoleWorkspaceEventHandler consoleWorkspaceEventHandler = new ConsoleWorkspaceEventHandler();

            // auto convert old args if detected to new args then run unified processing
            // Support backward compatibility 
            if (args[0].StartsWith("ConfigFile=")  || args[0] == "Dynamic=" || args[0] == "Script=")
            {                                
                string[] newArgs = ConvertOldArgs(args);
                ShowOLDCLIArgsWarning(args, newArgs);
                args = newArgs;
            }
            
            ParseArgs(args);                            
        }

        public void ExecuteArgs(string commandLine)
        {
            string[] args = ParseArguments(commandLine).ToArray();
            ExecuteArgs(args);
        }


        // Parse a command line with multiple switches to string list
        public static IEnumerable<string> ParseArguments(string commandLine)
        {
            if (string.IsNullOrWhiteSpace(commandLine))
                yield break;

            var sb = new StringBuilder();
            bool inQuote = false;
            foreach (char c in commandLine)
            {
                if (c == '"' && !inQuote)
                {
                    inQuote = true;
                    continue;
                }

                if (c != '"' && !(char.IsWhiteSpace(c) && !inQuote))
                {
                    sb.Append(c);
                    continue;
                }

                if (sb.Length > 0)
                {
                    var result = sb.ToString();
                    sb.Clear();
                    inQuote = false;
                    yield return result;
                }
            }

            if (sb.Length > 0)
                yield return sb.ToString();
        }


        private string[] ConvertOldArgs(string[] oldArgs)
        {                        
            string param;
            string value = null;            
            if (oldArgs[0].Contains("="))
            {
                string[] arg1 = oldArgs[0].Split(new[] { '=' }, 2);
                param = arg1[0].Trim();
                value = arg1[1].Trim();
            }
            else
            {
                param = oldArgs[0].Trim();
                if (oldArgs.Length > 1)
                {
                    value = oldArgs[1].Trim();
                }
            }            
            
            VerbAttribute verb;  // Get the correct verb using reflection
            switch (param)
            {                
                case "ConfigFile":
                    verb = typeof(ConfigFileOptions).GetCustomAttribute<VerbAttribute>();                    
                    break;
                case "Script":
                    verb = typeof(ScriptOptions).GetCustomAttribute<VerbAttribute>();                    
                    break;
                case "Dynamic":
                    verb = typeof(DynamicOptions).GetCustomAttribute<VerbAttribute>();
                    break;
                default:
                    Reporter.ToConsole(eLogLevel.ERROR, "Error - Unknown Command Line Argument(s): " + param);
                    return null;
            }
            
            return new string[] { verb.Name, "-f", value};            
        }



        private void ParseArgs(string[] args)
        {
            //if (args[0].StartsWith("-"))
            //{
            //    Parser.Default.ParseArguments<BasicOptions>(args)
            //            .WithParsed<BasicOptions>(basicOptions =>
            //            {
            //                HandleBasicOptions(basicOptions);
            //            })                       
            //           .WithNotParsed(errs =>
            //           {
            //               // handle args parsing errors
            //               HandleCLIParseError(errs);
            //           });
            //}
            //else
            //{
                Parser.Default.ParseArguments<RunOptions, GridOptions, ConfigFileOptions, DynamicOptions, ScriptOptions>(args)                        
                       .WithParsed<RunOptions>(runOptions =>
                       {
                           mCLIHandler = new CLIArgs();
                           HandleRunOptions(runOptions);
                       })
                       .WithParsed<GridOptions>(gridOptions =>
                       {
                           HanldeGridOption(gridOptions);
                       })
                       .WithParsed<ConfigFileOptions>(configFileOptions =>
                       {
                           HandleConfigFileOptions(configFileOptions);
                       })
                       .WithParsed<DynamicOptions>(dynamicOptions =>
                       {
                           HandleDynamicOptions(dynamicOptions);
                       })
                       .WithParsed<ScriptOptions>(scriptOptions =>
                       {
                           HandleScriptOptions(scriptOptions);
                       })
                       

                       .WithNotParsed(errs =>
                       {
                            // handle args parsing errors
                            HandleCLIParseError(errs);
                       });
            //}
        }

        private void HandleScriptOptions(ScriptOptions scriptOptions)
        {            
            Reporter.ToLog(eLogLevel.DEBUG, string.Format("Running with ScriptFile= '{0}'", scriptOptions.FileName));
            mCLIHandler = new CLIScriptFile();
            mCLIHandler.LoadContent(ReadFile(scriptOptions.FileName), mCLIHelper, WorkSpace.Instance.RunsetExecutor);            
            CLILoadAndPrepare();
            ExecuteRunSet();
        }

        private void HandleBasicOptions(BasicOptions basicOptions)
        {
            Console.WriteLine("Basic options !!!!!!!!!!!!");
            // basicOptions.ShowVersion
        }

        private void HandleDynamicOptions(DynamicOptions dynamicOptions)
        {            
            Reporter.ToLog(eLogLevel.DEBUG, string.Format("Running with DynamicXML= '{0}'", dynamicOptions.FileName));
            mCLIHandler = new CLIDynamicXML();
            mCLIHandler.LoadContent(ReadFile(dynamicOptions.FileName), mCLIHelper, WorkSpace.Instance.RunsetExecutor);
            CLILoadAndPrepare();
            ExecuteRunSet();
        }

        private void HanldeCLIOption(BasicOptions cliOptions)
        {
            throw new NotImplementedException();
        }

        private void HandleConfigFileOptions(ConfigFileOptions configFileOptions)
        {            
            Reporter.ToLog(eLogLevel.DEBUG, string.Format("Running with ConfigFile= '{0}'", configFileOptions.FileName));
            mCLIHandler = new CLIConfigFile();
            mCLIHandler.LoadContent(ReadFile(configFileOptions.FileName), mCLIHelper, WorkSpace.Instance.RunsetExecutor);            
            CLILoadAndPrepare();
            ExecuteRunSet();
        }

        private void HanldeGridOption(GridOptions gridOptions)
        {
            Reporter.ToConsole(eLogLevel.INFO, "Starting Ginger Grid at port: " + gridOptions.Port);            
            GingerGrid gingerGrid = new GingerGrid(gridOptions.Port);   
            gingerGrid.Start();
        }

        private void HandleRunOptions(RunOptions runOptions)
        {
            Reporter.ToLog(eLogLevel.DEBUG, string.Format("########################## Starting Automatic {0} Execution Process ##########################", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
            Reporter.ToLog(eLogLevel.DEBUG, string.Format("Parsing {0} execution arguments...", GingerDicser.GetTermResValue(eTermResKey.RunSet)));

            Reporter.ToLog(eLogLevel.INFO, $"Solution: {runOptions.Solution}");
            Reporter.ToLog(eLogLevel.INFO, $"Runset: {runOptions.Runset}");
                        
            Reporter.ToLog(eLogLevel.DEBUG, "Loading Configurations...");
            // mCLIHandler.LoadContent(configurations, mCLIHelper, WorkSpace.Instance.RunsetExecutor);

            mCLIHelper.Solution = runOptions.Solution;
            mCLIHelper.Runset = runOptions.Runset;
            mCLIHelper.Env = runOptions.Environment;

            CLILoadAndPrepare();
            ExecuteRunSet();
        }

        private void CLILoadAndPrepare()
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
                return; //Failed to perform execution perpetrations
            }

            mCLIHelper.SetTestArtifactsFolder();
        }

        private void HandleCLIParseError(IEnumerable<Error> errs)
        {                        
            Reporter.ToConsole(eLogLevel.ERROR, "Please fix the arguments and try again");
        }

        //private void HandleArg(string param, string value)
        //{
        //    // TODO: get all classes impl ICLI and check Identifier then set

        //    switch (param)
        //    {
        //        case "--version":
        //            Console.WriteLine(string.Format("{0} Version: {1}", ApplicationInfo.ApplicationName, ApplicationInfo.ApplicationVersionWithInfo));
        //            break;
        //        case "--help":
        //        case "-h":
        //            ShowCLIHelp();
        //            break;
        //        case "ConfigFile":
        //        case "--configfile":
        //            mCLIHelper.CLIType = eCLIType.Config;
        //            Reporter.ToLog(eLogLevel.DEBUG, string.Format("Running with ConfigFile= '{0}'", value));
        //            mCLIHandler = new CLIConfigFile();                   
        //            PerformLoadAndExecution(ReadFile(value));
        //            break;
        //        case "Script":
        //        case "--scriptfile":
        //            mCLIHelper.CLIType = eCLIType.Script;
        //            Reporter.ToLog(eLogLevel.DEBUG, string.Format("Running with ScriptFile= '{0}'", value));
        //            mCLIHandler = new CLIScriptFile();
        //            PerformLoadAndExecution(ReadFile(value));
        //            break;
        //        case "--dynamicfile":
        //        case "Dynamic":
        //            mCLIHelper.CLIType = eCLIType.Dynamic;
        //            Reporter.ToLog(eLogLevel.DEBUG, string.Format("Running with DynamicXML= '{0}'", value));
        //            mCLIHandler = new CLIDynamicXML();
        //            PerformLoadAndExecution(ReadFile(value));
        //            break;                                
        //        //case "--args":
        //        //    mCLIHelper.CLIType = eCLIType.Arguments;
        //        //    Reporter.ToLog(eLogLevel.DEBUG, string.Format("Running with Command Args= '{0}'", value));
        //        //    mCLIHandler = new CLIArgs();
        //        //    PerformLoadAndExecution(value);
        //        //    break;

        //            // Removing unpublished options 
        //        //case "--excel":
        //        //    Reporter.ToLog(eLogLevel.DEBUG, string.Format("Running with CLI Excel= '{0}'", value));
        //        //    mCLIHandler = new CLIExcel();
        //        //    PerformLoadAndExecution(value);
        //        //    break;                
        //        //case "--servicegrid":
        //        //    int port = 15555;
        //        //    Console.WriteLine("Starting Ginger Grid at port: " + port);
        //        //    GingerGrid gingerGrid = new GingerGrid(15555);   // TODO: get port from CLI arg
        //        //    gingerGrid.Start();

        //        //    ServiceGridTracker serviceGridTracker = new ServiceGridTracker(gingerGrid);

        //        //    Console.ReadKey();
        //        //    break;                
        //        default:
        //            Console.WriteLine("Error - Unknown Command Line Argument(s): " + param);
        //            break;
        //    }
        //}

        //private void PerformLoadAndExecution(string configurations)
        //{
        //    Reporter.ToLog(eLogLevel.DEBUG, "Loading Configurations...");
        //    mCLIHandler.LoadContent(configurations, mCLIHelper, WorkSpace.Instance.RunsetExecutor);

        //    if (mCLIHelper.CLIType != eCLIType.Script)
        //    {
        //        if (!mCLIHelper.LoadSolution())
        //        {
        //            return;//failed to load Solution;
        //        }

        //        if (!mCLIHelper.LoadRunset(WorkSpace.Instance.RunsetExecutor))
        //        {
        //            return;//failed to load Run set
        //        }

        //        if (!mCLIHelper.PrepareRunsetForExecution())
        //        {
        //            return; //Failed to perform execution preparations
        //        }

        //        mCLIHelper.SetTestArtifactsFolder();                
        //    }

        //    Reporter.ToLog(eLogLevel.DEBUG, string.Format("Executing..."));
        //    Execute();

        //    Reporter.ToLog(eLogLevel.DEBUG, "Closing Solution and doing Cleanup...");
        //    mCLIHelper.CloseSolution();
        //}

        void ExecuteRunSet()
        {
            Reporter.ToLog(eLogLevel.DEBUG, string.Format("Executing..."));            
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
                Reporter.ToLog(eLogLevel.ERROR, string.Format("Unexpected exception occurred during {0} execution, exit code 1", GingerDicser.GetTermResValue(eTermResKey.RunSet)), ex);
                Environment.ExitCode = 1; //failure
            }

            Reporter.ToLog(eLogLevel.DEBUG, "Closing Solution and doing Cleanup...");
            mCLIHelper.CloseSolution();
        }

        
        private void ShowOLDCLIArgsWarning(string[] oldArgs, string[] newArgs)
        {
            // TODO:            
            Reporter.ToConsole(eLogLevel.WARN, "You are using old style command line arguments which are obsolete!");

            Reporter.ToConsole(eLogLevel.WARN, "Instead of using: " + oldArgs);
            Reporter.ToConsole(eLogLevel.WARN, "You can use: " + newArgs);
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
