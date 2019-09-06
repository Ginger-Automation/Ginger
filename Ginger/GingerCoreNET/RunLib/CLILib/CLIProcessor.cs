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
using System.Runtime.InteropServices;
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
            if (args[0].StartsWith("ConfigFile=") || args[0].StartsWith("Dynamic=") || args[0].StartsWith("Script="))
            {                                
                string[] newArgs = ConvertOldArgs(args);
                ShowOLDCLIArgsWarning(args, newArgs);
                args = newArgs;
            }
            
            ParseArgs(args);                                        
        }



        private void ParseArgs(string[] args)
        {
            // FIXME: failing with exc of obj state
            // Do not show default version
            // Parser.Default.Settings.AutoVersion = false;            

            int result = Parser.Default.ParseArguments<RunOptions, GridOptions, ConfigFileOptions, DynamicOptions, ScriptOptions, SCMOptions, VersionOptions, ExampleOptions>(args).MapResult(
                    (RunOptions opts) => HandleRunOptions(opts),
                    (GridOptions opts) => HanldeGridOption(opts),
                    (ConfigFileOptions opts) => HandleConfigFileOptions(opts),
                    (DynamicOptions opts) => HandleDynamicOptions(opts),
                    (ScriptOptions opts) => HandleScriptOptions(opts),
                    (SCMOptions opts) => HandleSCMOptions(opts),
                    (VersionOptions opts) => HandleVersionOptions(opts),                        
                    (ExampleOptions opts) => HandleExampleOptions(opts),

                    errs => HandleCLIParseError(errs)
            );          

            if (result != 0)
            {
                Reporter.ToConsole(eLogLevel.ERROR, "Error(s) occurred process exit code (" + result + ")");
            }            
        }

        private int HandleVersionOptions(VersionOptions opts)
        {
            PrintGingerVersionInfo();
            return 0;
        }

        private int HandleExampleOptions(ExampleOptions exampleOptions)
        {
            Reporter.ToConsole(eLogLevel.DEBUG, "Running example options");

            switch (exampleOptions.verb)
            {
                case "all":
                    ShowExamples();
                    break;
                case "run":
                    ShowExamples();
                    break;
                default:
                    Reporter.ToConsole(eLogLevel.ERROR, "Unknown verb '" + exampleOptions.verb + "' ");
                    return 1;                    
            }
            
            return 0;
        }

      

        private void ShowExamples()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append("* Note when argument value contain space(s) wrap it with '\"' before and after, see examples below").Append(Environment.NewLine);
            sb.Append("* Note when running on Linux it is recommended to avoid using spaces in directories names, however it is supported").Append(Environment.NewLine);
            sb.Append(Environment.NewLine);

            string solution;
            string solutionWithSpaces;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                solution = @"c:\GingerSolutions\Sol1";
                solutionWithSpaces = @"c:\GingerSolutions\Ginger Sol 1";
            }
            else
            {
                // for Linux show env.sep 
                solution = @"/user/GingerSolutions/Sol1";
                solutionWithSpaces = @"/user/GingerSolutions/Ginger Sol 1";
            }

            sb.Append("Run set examples:").Append(Environment.NewLine);
            sb.Append(Environment.NewLine);

            sb.Append("Example #1: ");
            sb.Append(CreateExample(solution, "Runset1"));

            sb.Append("Example #2: ");
            sb.Append(CreateExample(solution, "Runset1", "UAT"));

            sb.Append("Example #3: ");
            sb.Append(CreateExample(solutionWithSpaces, "Default Run Set", "UAT"));

            Reporter.ToConsole(eLogLevel.INFO, sb.ToString());
        }

        private string CreateExample(string solution, string runset, string env = null)
        {
            StringBuilder sb = new StringBuilder();
            RunOptions runOptions = new RunOptions();
            runOptions.Solution = solution;
            if (env != null)
            {
                runOptions.Environment = env;
            }

            if (runset != null)
            {
                runOptions.Runset = runset;
            }

            var arguments = Parser.Default.FormatCommandLine<RunOptions>(runOptions);

            string exe = "dotnet GingerConsole.dll ";

            sb.Append(@"Open solution at '" + runOptions.Solution + "'");
            if (runOptions.Environment == null)
            {
                sb.Append(" auto select environment (assuming only one exist)");
            }
            else
            {
                sb.Append(" select environment '" + runOptions.Environment + "'");
            }
                            
            sb.Append(" and execute run set '").Append(runOptions.Runset).Append("'");
            sb.Append(Environment.NewLine);
            sb.Append("Long form : ").Append("'" + exe + arguments + "'").Append(Environment.NewLine);
            sb.Append("Short form: ").Append("'" + exe + "run");
            sb.Append(" -").Append(CLIOptionClassHelper.GetAttrShorName<RunOptions>(nameof(RunOptions.Solution))).Append(" ").Append(WrapTextIfNeeded(runOptions.Solution));
            if (env != null)
            {
                sb.Append(" -").Append(CLIOptionClassHelper.GetAttrShorName<RunOptions>(nameof(RunOptions.Environment))).Append(" ").Append(WrapTextIfNeeded(runOptions.Environment));
            }

            if (runset != null)
            {
                sb.Append(" -").Append(CLIOptionClassHelper.GetAttrShorName<RunOptions>(nameof(RunOptions.Runset))).Append(" ").Append(WrapTextIfNeeded(runOptions.Runset));
            }

            sb.Append("'");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            return sb.ToString();
        }

        string WrapTextIfNeeded(string val)
        {
            if (val.IndexOf(' ') > 0)
            {
                return "\"" + val + "\"";
            }
            else
            {
                return val;
            }
        }

        private int HandleSCMOptions(SCMOptions sCmOptions)
        {
            SetVerboseLevel(sCmOptions.VerboseLevel);
            Reporter.ToLog(eLogLevel.INFO, "Running SCM options");

            mCLIHandler = new CLISCM();
            mCLIHelper.SourceControlURL = sCmOptions.URL;
            
            // do the rest !!!
            
            ExecuteRunSet();

            return Environment.ExitCode;
        }

        private int HandleScriptOptions(ScriptOptions scriptOptions)
        {
            SetVerboseLevel(scriptOptions.VerboseLevel);

            Reporter.ToLog(eLogLevel.DEBUG, string.Format("Running with ScriptFile= '{0}'", scriptOptions.FileName));
            mCLIHandler = new CLIScriptFile();
            mCLIHandler.LoadContent(ReadFile(scriptOptions.FileName), mCLIHelper, WorkSpace.Instance.RunsetExecutor);                        
            ExecuteRunSet();
            
            return Environment.ExitCode;
        }

        

        private int HandleDynamicOptions(DynamicOptions dynamicOptions)
        {
            SetVerboseLevel(dynamicOptions.VerboseLevel);

            Reporter.ToLog(eLogLevel.DEBUG, string.Format("Running with DynamicXML= '{0}'", dynamicOptions.FileName));
            mCLIHandler = new CLIDynamicXML();
            mCLIHandler.LoadContent(ReadFile(dynamicOptions.FileName), mCLIHelper, WorkSpace.Instance.RunsetExecutor);            
            ExecuteRunSet();

            return Environment.ExitCode;
        }

       
        private int HandleConfigFileOptions(ConfigFileOptions configFileOptions)
        {
            SetVerboseLevel(configFileOptions.VerboseLevel);

            Reporter.ToLog(eLogLevel.DEBUG, string.Format("Running with ConfigFile= '{0}'", configFileOptions.FileName));
            mCLIHandler = new CLIConfigFile();
            mCLIHandler.LoadContent(ReadFile(configFileOptions.FileName), mCLIHelper, WorkSpace.Instance.RunsetExecutor);                        
            ExecuteRunSet();

            return Environment.ExitCode;
        }

        private int HanldeGridOption(GridOptions gridOptions)
        {
            SetVerboseLevel(gridOptions.VerboseLevel);

            Reporter.ToConsole(eLogLevel.INFO, "Starting Ginger Grid at port: " + gridOptions.Port);            
            GingerGrid gingerGrid = new GingerGrid(gridOptions.Port);   
            gingerGrid.Start();

            // if flag !!!!!!!!!!!!
            ServiceGridTracker serviceGridTracker = new ServiceGridTracker(gingerGrid);

            Console.WriteLine();
            Console.WriteLine("---------------------------------------------------");
            Console.WriteLine("-               Press 'q' exit                    -");
            Console.WriteLine("---------------------------------------------------");

            if (!Console.IsInputRedirected && !WorkSpace.Instance.RunningFromUnitTest)  // for example unit test redirect input, or we can run without input like from Jenkins
            {
                ConsoleKey consoleKey = ConsoleKey.A;
                while (consoleKey != ConsoleKey.Q)
                {
                    ConsoleKeyInfo consoleKeyInfo = Console.ReadKey();
                    consoleKey = consoleKeyInfo.Key;
                }
            }

            return 0; 
        }

        private int HandleRunOptions(RunOptions runOptions)
        {
            SetVerboseLevel(runOptions.VerboseLevel);

            Reporter.ToLog(eLogLevel.DEBUG, string.Format("########################## Starting Automatic {0} Execution Process ##########################", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
            Reporter.ToLog(eLogLevel.DEBUG, string.Format("Parsing {0} execution arguments...", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
            Reporter.ToLog(eLogLevel.INFO, $"Solution: {runOptions.Solution}");
            Reporter.ToLog(eLogLevel.INFO, $"Runset: {runOptions.Runset}");
            Reporter.ToLog(eLogLevel.INFO, $"Environment: {runOptions.Environment}");
            Reporter.ToLog(eLogLevel.DEBUG, "Loading Configurations...");
            
            mCLIHandler = new CLIArgs();
            mCLIHelper.Solution = runOptions.Solution;
            mCLIHelper.Runset = runOptions.Runset;
            mCLIHelper.Env = runOptions.Environment;
            mCLIHelper.RunAnalyzer = runOptions.RunAnalyzer;
            mCLIHelper.ShowAutoRunWindow = runOptions.ShowAutoRunWindow;
            mCLIHelper.TestArtifactsFolder = runOptions.TestArtifactsPath;

            ExecuteRunSet();

            return Environment.ExitCode;
        }

        private void SetVerboseLevel(OptionsBase.eVerboseLevel verboseLevel)
        {            
            if (verboseLevel == OptionsBase.eVerboseLevel.debug)
            {
                Reporter.AppLoggingLevel = eAppReporterLoggingLevel.Debug;
            }
            else
            {
                Reporter.AppLoggingLevel = eAppReporterLoggingLevel.Normal;
            }
            
        }

        private int HandleCLIParseError(IEnumerable<Error> errs)
        {
            if (errs.Count() == 1 && errs.First() is HelpVerbRequestedError)
            {
                // User requested help 
                PrintGingerCLIHelp();
                return 0;
            }
            else if (errs.Count() == 1 && errs.First() is VersionRequestedError)
            {
                // User requested version
                PrintGingerVersionInfo();
                return 0;
            }
            else
            {
                Reporter.ToConsole(eLogLevel.ERROR, "Please fix the arguments and try again");
                return 1;
            }
            
        }

        private void PrintGingerVersionInfo()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("Ginger Executor: ").Append(Assembly.GetEntryAssembly().Location).Append(Environment.NewLine);            
            stringBuilder.Append(Environment.NewLine);
            Reporter.ToConsole(eLogLevel.INFO, stringBuilder.ToString());
        }

        private void PrintGingerCLIHelp()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("Ginger support many command line arguments and verbs").Append(Environment.NewLine);
            stringBuilder.Append("'-h' for basic arguments list").Append(Environment.NewLine);
            stringBuilder.Append("'help' for verb list").Append(Environment.NewLine);
            stringBuilder.Append("'help {verb}' for help on specific verb options, for example: 'help run'").Append(Environment.NewLine);
            stringBuilder.Append("'-e' for list of examples").Append(Environment.NewLine);
            stringBuilder.Append(Environment.NewLine);
            Reporter.ToConsole(eLogLevel.INFO, stringBuilder.ToString());
        }
        

        void ExecuteRunSet()
        {
            CLILoadAndPrepare();

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
                return; //Failed to perform execution preparations
            }

            mCLIHelper.SetTestArtifactsFolder();
            WorkSpace.Instance.StartLocalGrid();
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

            string verb;
            switch (param)
            {
                case "ConfigFile":
                    verb = ConfigFileOptions.Verb;
                    break;
                case "Script":
                    verb = ScriptOptions.Verb;
                    break;
                case "Dynamic":
                    verb = DynamicOptions.Verb;
                    break;
                default:
                    Reporter.ToConsole(eLogLevel.ERROR, "Error - Unknown Command Line Argument(s): " + param);
                    return null;
            }

            return new string[] { verb, "--" + CLIOptionClassHelper.FILENAME, value };
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
