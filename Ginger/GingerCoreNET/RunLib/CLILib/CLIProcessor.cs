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
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using CommandLine;
using Ginger;
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
using System.Threading.Tasks;
using static Amdocs.Ginger.CoreNET.RunLib.CLILib.OptionsBase;

namespace Amdocs.Ginger.CoreNET.RunLib
{
    public class CLIProcessor
    {
        ICLI mCLIHandler;
        CLIHelper mCLIHelper = new CLIHelper();

        public async Task ExecuteArgs(string[] args)
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

            await ParseArgs(args);
        }



        private async Task ParseArgs(string[] args)
        {            
            // FIXME: failing with exc of obj state
            // Do not show default version
            // Parser.Default.Settings.AutoVersion = false;
            var parser = new Parser(settings =>
            {
                settings.IgnoreUnknownArguments = true;
            });

            int result = await parser.ParseArguments<RunOptions, GridOptions, ConfigFileOptions, DynamicOptions, ScriptOptions, SCMOptions, VersionOptions, ExampleOptions, DoOptions>(args).MapResult(
                    async (RunOptions opts) => await HandleRunOptions(opts),
                    async (GridOptions opts) => await HanldeGridOption(opts),
                    async (ConfigFileOptions opts) => await HandleFileOptions("config", opts.FileName, opts.VerboseLevel),
                    async (DynamicOptions opts) => await HandleFileOptions("dynamic", opts.FileName, opts.VerboseLevel),
                    async (ScriptOptions opts) => await HandleFileOptions("script", opts.FileName, opts.VerboseLevel),
                    async (VersionOptions opts) => await HandleVersionOptions(opts),
                    async (ExampleOptions opts) => await HandleExampleOptions(opts),
                    async (DoOptions opts) => await HandleDoOptions(opts),
                        

                    async errs => await HandleCLIParseError(errs)
            );

            if (result != 0)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error(s) occurred process exit code (" + result + ")");
                Environment.ExitCode = 1; // error
            }            
        }

        private async Task<int> HandleDoOptions(DoOptions opts)
        {
            return await Task.Run(() =>
             {
                 DoOptionsHanlder.Run(opts);
                 return 0;
             });

        }

        private async Task<int> HandleVersionOptions(VersionOptions opts)
        {
            return await Task.Run(() =>
             {
                 PrintGingerVersionInfo();
                 return 0;
             });

        }

        private async Task<int> HandleExampleOptions(ExampleOptions exampleOptions)
        {
            return await Task.Run(() =>
             {
                 Reporter.ToLog(eLogLevel.DEBUG, "Running example options");

                 switch (exampleOptions.verb)
                 {
                     case "all":
                         ShowExamples();
                         break;
                     case "run":
                         ShowExamples();
                         break;
                     default:
                         Reporter.ToLog(eLogLevel.ERROR, "Unknown verb '" + exampleOptions.verb + "' ");
                         return 1;
                 }

                 return 0;
             });

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

            Reporter.ToLog(eLogLevel.INFO, sb.ToString());
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

        private async Task<int> HandleSCMOptions(SCMOptions sCmOptions)
        {
            return await Task.Run(() =>
             {
                 SetVerboseLevel(sCmOptions.VerboseLevel);
                 Reporter.ToLog(eLogLevel.INFO, "Running SCM options");
                 CLISCM cliSCM = new CLISCM();
                 cliSCM.Download(sCmOptions);
                 return Environment.ExitCode;
             });

        }

        private async Task<int> HandleFileOptions(string fileType, string fileName, eVerboseLevel verboseLevel)
        {
            WorkSpace.Instance.GingerCLIMode = eGingerCLIMode.script;
            try
            {
                SetVerboseLevel(verboseLevel);
                Reporter.ToLog(eLogLevel.INFO, string.Format("Running with " + fileType + " file = '{0}'", fileName));
                switch (fileType)
                {
                    case "config":
                        WorkSpace.Instance.GingerCLIMode = eGingerCLIMode.config;
                        mCLIHandler = new CLIConfigFile();
                        break;
                    case "dynamic":
                        WorkSpace.Instance.GingerCLIMode = eGingerCLIMode.dynamic;
                        if (Path.GetExtension(fileName).ToLower() == ".xml")
                        {
                            mCLIHandler = new CLIDynamicFile(CLIDynamicFile.eFileType.XML);
                        }
                        else if (Path.GetExtension(fileName).ToLower() == ".json")
                        {
                            mCLIHandler = new CLIDynamicFile(CLIDynamicFile.eFileType.JSON);
                        }
                        else
                        {
                            Reporter.ToLog(eLogLevel.ERROR, string.Format("Dynamic file type is not supported, file path: '{0}'", fileName));
                            Environment.ExitCode = 1; //failure
                            return Environment.ExitCode;
                        }
                        break;

                    case "script":
                        mCLIHandler = new CLIScriptFile();
                        break;
                }

                string fileContent = ReadFile(fileName);
                mCLIHandler.LoadGeneralConfigurations(fileContent, mCLIHelper);

                if (fileType == "config" || fileType == "dynamic")  // not needed for script
                {
                    if (!CLILoadAndPrepare(runsetConfigs: fileContent))
                    {
                        Reporter.ToLog(eLogLevel.WARN, "Issue occured while doing CLI Load and Prepare so aborting execution");
                        Environment.ExitCode = 1;
                        return Environment.ExitCode;
                    }
                }

                await ExecuteRunSet();

                return Environment.ExitCode;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Exception occured while Handling File Option", ex);
                Environment.ExitCode = 1;
                return 1;//error
            }
        }


        private async Task<int> HanldeGridOption(GridOptions gridOptions)
        {
            WorkSpace.Instance.GingerCLIMode = eGingerCLIMode.grid;
            return await Task.Run(() =>
             {
                 SetVerboseLevel(gridOptions.VerboseLevel);

                 Reporter.ToLog(eLogLevel.INFO, "Starting Ginger Grid at port: " + gridOptions.Port);
                 GingerGrid gingerGrid = new GingerGrid(gridOptions.Port);
                 gingerGrid.Start();

                 if (gridOptions.Tracker)
                 {
                     ServiceGridTracker serviceGridTracker = new ServiceGridTracker(gingerGrid);
                 }

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
             });

        }
               

        private async Task<int> HandleRunOptions(RunOptions runOptions)
        {
            WorkSpace.Instance.GingerCLIMode = eGingerCLIMode.run;
            SetVerboseLevel(runOptions.VerboseLevel);

            Reporter.ToLog(eLogLevel.INFO, string.Format("######## Starting Automatic {0} Execution Process ########", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
            Reporter.ToLog(eLogLevel.INFO, string.Format("Parsing {0} execution arguments...", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
            Reporter.ToLog(eLogLevel.INFO, $"Solution: '{runOptions.Solution}'");
            Reporter.ToLog(eLogLevel.INFO, $"Runset: '{runOptions.Runset}'");
            Reporter.ToLog(eLogLevel.INFO, $"Environment: '{runOptions.Environment}'");
            Reporter.ToLog(eLogLevel.INFO, "Loading Configurations...");

            mCLIHandler = new CLIArgs();
            mCLIHelper.Solution = runOptions.Solution;
            mCLIHelper.SetEncryptionKey(runOptions.EncryptionKey);
            mCLIHelper.Runset = runOptions.Runset;
            mCLIHelper.Env = runOptions.Environment;
            mCLIHelper.RunAnalyzer = !runOptions.DoNotAnalyze;
            mCLIHelper.ShowAutoRunWindow = runOptions.ShowUI;
            mCLIHelper.TestArtifactsFolder = runOptions.TestArtifactsPath;

            mCLIHelper.SourceControlURL = runOptions.URL;
            mCLIHelper.SourcecontrolUser = runOptions.User;
            mCLIHelper.sourceControlType = runOptions.SCMType;
            
            mCLIHelper.SetSourceControlBranch(runOptions.Branch);

            mCLIHelper.sourceControlPass = runOptions.Pass;
            mCLIHelper.sourceControlPassEncrypted = runOptions.PasswordEncrypted;
            mCLIHelper.SourceControlProxyServer(runOptions.SourceControlProxyServer);
            mCLIHelper.SourceControlProxyPort(runOptions.SourceControlProxyPort);
            mCLIHelper.SelfHealingCheckInConfigured = runOptions.SelfHealingCheckInConfigured;

            mCLIHelper.SealightsEnable = runOptions.SealightsEnable;
            mCLIHelper.SealightsUrl = runOptions.SealightsUrl;
            mCLIHelper.SealightsAgentToken = runOptions.SealightsAgentToken;
            mCLIHelper.SealightsLabID = runOptions.SealightsLabID;
            mCLIHelper.SealightsSessionID = runOptions.SealightsSessionID;
            mCLIHelper.SealightsSessionTimeOut = runOptions.SealightsSessionTimeOut;
            mCLIHelper.SealightsTestStage = runOptions.SealightsTestStage;
            mCLIHelper.SealightsEntityLevel = runOptions.SealightsEntityLevel?.ToString() == "None" ? null : runOptions.SealightsEntityLevel?.ToString();
            mCLIHelper.SealightsTestRecommendations = runOptions.SealightsTestRecommendations;

            if (!string.IsNullOrEmpty(runOptions.RunSetExecutionId))
            {
                if (!Guid.TryParse(runOptions.RunSetExecutionId, out Guid temp))
                {
                    Reporter.ToLog(eLogLevel.ERROR, string.Format("The provided ExecutionID '{0}' is not valid.", runOptions.RunSetExecutionId));
                    Environment.ExitCode = 1;
                    return Environment.ExitCode;
                }
                else
                {
                    mCLIHelper.ExecutionId = runOptions.RunSetExecutionId;
                    Reporter.ToLog(eLogLevel.INFO, string.Format("Using provided ExecutionID '{0}'.", mCLIHelper.ExecutionId.ToString()));
                }
            }

            if (WorkSpace.Instance.UserProfile == null)
            {
                WorkSpace.Instance.UserProfile = new UserProfile();
                UserProfileOperations userProfileOperations = new UserProfileOperations(WorkSpace.Instance.UserProfile);
                WorkSpace.Instance.UserProfile.UserProfileOperations = userProfileOperations;
            }
            WorkSpace.Instance.UserProfile.SourceControlURL = runOptions.URL;
            WorkSpace.Instance.UserProfile.SourceControlUser = runOptions.User;
            WorkSpace.Instance.UserProfile.SourceControlType = runOptions.SCMType;
            WorkSpace.Instance.UserProfile.UserProfileOperations.SourceControlIgnoreCertificate = runOptions.ignoreCertificate;
            WorkSpace.Instance.UserProfile.UserProfileOperations.SourceControlUseShellClient = runOptions.useScmShell;



            WorkSpace.Instance.UserProfile.EncryptedSourceControlPass = runOptions.Pass;
            WorkSpace.Instance.UserProfile.SourceControlPass = runOptions.Pass;
            if (runOptions.PasswordEncrypted)
            {
                mCLIHandler.LoadGeneralConfigurations("", mCLIHelper);
            }


            WorkSpace.Instance.RunningInExecutionMode = true;
            if (!CLILoadAndPrepare())
            {
                Reporter.ToLog(eLogLevel.WARN, "Issue occured while doing CLI Load and Prepare so aborting execution");
                Environment.ExitCode = 1;
                return Environment.ExitCode;
            }

            await ExecuteRunSet();

            mCLIHelper.PostExecution();

            return Environment.ExitCode;
        }



        public static void SetVerboseLevel(OptionsBase.eVerboseLevel verboseLevel)
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

        private async Task<int> HandleCLIParseError(IEnumerable<Error> errs)
        {
            return await Task.Run(() =>
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
                     Reporter.ToLog(eLogLevel.ERROR, "Please fix the arguments and try again");
                     return 1;
                 }
             });



        }

        private void PrintGingerVersionInfo()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("Ginger Executor: ").Append(Assembly.GetEntryAssembly().Location).Append(Environment.NewLine);
            stringBuilder.Append(Environment.NewLine);
            Reporter.ToLog(eLogLevel.INFO, stringBuilder.ToString());
        }

        private void PrintGingerCLIHelp()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(Environment.NewLine);
            stringBuilder.Append("Ginger support many command line arguments and verbs").Append(Environment.NewLine);
            stringBuilder.Append("'help' for verb list").Append(Environment.NewLine);
            stringBuilder.Append("'help {verb}' for help on specific verb options, for example: 'help run'").Append(Environment.NewLine);
            stringBuilder.Append(Environment.NewLine);
            Reporter.ToLog(eLogLevel.INFO, stringBuilder.ToString());
        }


        async Task ExecuteRunSet()
        {
            Reporter.ToLog(eLogLevel.INFO, string.Format("Executing {0}... ", GingerDicser.GetTermResValue(eTermResKey.RunSet)));
            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();

                await mCLIHandler.Execute(WorkSpace.Instance.RunsetExecutor);

                stopwatch.Stop();
                Reporter.ToLog(eLogLevel.INFO, "Execution Elapsed time: " + stopwatch.Elapsed);

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

            //self healing changes check-in in source control
            if (WorkSpace.Instance.RunsetExecutor.RunSetConfig.SelfHealingConfiguration.SaveChangesInSourceControl || mCLIHelper.SelfHealingCheckInConfigured)
            {
                mCLIHelper.SaveAndCommitSelfHealingChanges();
            }

            Reporter.ToLog(eLogLevel.INFO, "Closing Solution and doing Cleanup...");
            mCLIHelper.CloseSolution();
        }

        private bool CLILoadAndPrepare(string runsetConfigs="")
        {
            try
            {
                if (!mCLIHelper.LoadSolution())
                {
                    return false;//failed to load Solution;
                }

                if (!string.IsNullOrEmpty(runsetConfigs))
                {
                    mCLIHandler.LoadRunsetConfigurations(runsetConfigs, mCLIHelper, WorkSpace.Instance.RunsetExecutor);
                }

                if (!mCLIHelper.LoadRunset(WorkSpace.Instance.RunsetExecutor))
                {
                    return false;//failed to load Run set
                }

                if (!mCLIHelper.PrepareRunsetForExecution())
                {
                    return false; //Failed to perform execution preparations
                }

                // Check for any Sealights Settings
                if (!mCLIHelper.SetSealights())
                {
                    return false;
                }    


                mCLIHelper.SetTestArtifactsFolder();
                WorkSpace.Instance.StartLocalGrid();

                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occured while doing CLI Load And Prepare", ex);
                return false;
            }
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
                    Reporter.ToLog(eLogLevel.ERROR, "Error - Unknown Command Line Argument(s): " + param);
                    return null;
            }

            return new string[] { verb, "--" + CLIOptionClassHelper.FILENAME, value };
        }


        private void ShowOLDCLIArgsWarning(string[] oldArgs, string[] newArgs)
        {
            // TODO:            
            Reporter.ToLog(eLogLevel.WARN, "You are using old style command line arguments which are obsolete!");

            Reporter.ToLog(eLogLevel.WARN, "Instead of using: " + oldArgs);
            Reporter.ToLog(eLogLevel.WARN, "You can use: " + newArgs);
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
