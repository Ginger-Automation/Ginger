#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Amdocs.Ginger.Common.Telemetry;
using Amdocs.Ginger.CoreNET.log4netLib;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.CoreNET.RunLib.CLILib;
using Amdocs.Ginger.Repository;
using CommandLine;
using Ginger.BusinessFlowWindows;
using Ginger.ReporterLib;
using Ginger.SourceControl;
using GingerCore;
using GingerCore.Repository;
using GingerWPF.WorkSpaceLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;


namespace Ginger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {


        public new static MainWindow MainWindow { get; set; }

        private Dictionary<string, Int32> mExceptionsDic = [];



        public static void LoadApplicationDictionaries(Amdocs.Ginger.Core.eSkinDicsType SkinDicType = Amdocs.Ginger.Core.eSkinDicsType.Default, GingerCore.eTerminologyType TerminologyType = GingerCore.eTerminologyType.Default)
        {
            //Clear all Dictionaries
            Application.Current.Resources.MergedDictionaries.Clear();

            //Load only relevant dictionaries for the application to use
            //Skins
            switch (SkinDicType)
            {
                case Amdocs.Ginger.Core.eSkinDicsType.Default:
                    Application.Current.Resources.MergedDictionaries.Add(
                            new ResourceDictionary()
                            {
                                Source = new Uri("pack://application:,,,/Ginger;component/Dictionaries/Skins/GingerDefaultSkinDictionary.xaml")
                            });
                    break;

                default:
                    Application.Current.Resources.MergedDictionaries.Add(
                            new ResourceDictionary()
                            {
                                Source = new Uri("pack://application:,,,/Ginger;component/Dictionaries/Skins/GingerDefaultSkinDictionary.xaml")
                            });
                    break;
            }

            // set terminology type
            GingerTerminology.TERMINOLOGY_TYPE = TerminologyType;
        }

        static bool bDone = false;

        public static void InitClassTypesDictionary()
        {
            if (bDone)
            {
                return;
            }

            bDone = true;
            // TODO: remove after we don't need old serializer to load old repo items
            NewRepositorySerializer.NewRepositorySerializerEvent += RepositorySerializer.NewRepositorySerializer_NewRepositorySerializerEvent;

            // Add all RI classes from GingerCore           
            NewRepositorySerializer.AddClassesFromAssembly(NewRepositorySerializer.eAssemblyType.GingerCore);
            // add from Ginger
            NewRepositorySerializer.AddClassesFromAssembly(NewRepositorySerializer.eAssemblyType.Ginger);

            // Each class which moved from GingerCore to GingerCoreCommon needed to be added here, so it will auto translate
            // For backward compatibility of loading old object name in xml
            Dictionary<string, Type> list = new Dictionary<string, Type>
            {
                { "GingerCore.Actions.ActInputValue", typeof(ActInputValue) },
                { "GingerCore.Actions.ActReturnValue", typeof(ActReturnValue) },
                { "GingerCore.Actions.EnhancedActInputValue", typeof(EnhancedActInputValue) },
                { "GingerCore.Environments.GeneralParam", typeof(GeneralParam) }
            };



            NewRepositorySerializer.AddClasses(list);
        }

        //private static void HandleSolutionLoadSourceControl(Solution solution)
        //{
        //    string RepositoryRootFolder = string.Empty;
        //    SourceControlBase.eSourceControlType type = SourceControlIntegration.CheckForSolutionSourceControlType(solution.Folder, ref RepositoryRootFolder);
        //    if (type == SourceControlBase.eSourceControlType.GIT)
        //    {
        //        solution.SourceControl = new GITSourceControl();
        //    }
        //    else if (type == SourceControlBase.eSourceControlType.SVN)
        //    {
        //        solution.SourceControl = new SVNSourceControl();
        //    }

        //    if (solution.SourceControl != null)
        //    {
        //        if (string.IsNullOrEmpty(WorkSpace.Instance.UserProfile.SolutionSourceControlUser) || string.IsNullOrEmpty(WorkSpace.Instance.UserProfile.SolutionSourceControlPass))
        //        {
        //            if (WorkSpace.Instance.UserProfile.SourceControlUser != null && WorkSpace.Instance.UserProfile.SourceControlPass != null)
        //            {
        //                solution.SourceControl.SourceControlUser = WorkSpace.Instance.UserProfile.SourceControlUser;
        //                solution.SourceControl.SourceControlPass = WorkSpace.Instance.UserProfile.SourceControlPass;
        //                solution.SourceControl.SolutionSourceControlAuthorEmail = WorkSpace.Instance.UserProfile.SolutionSourceControlAuthorEmail;
        //                solution.SourceControl.SolutionSourceControlAuthorName = WorkSpace.Instance.UserProfile.SolutionSourceControlAuthorName;
        //            }
        //        }
        //        else
        //        {
        //            solution.SourceControl.SourceControlUser = WorkSpace.Instance.UserProfile.SolutionSourceControlUser;
        //            solution.SourceControl.SourceControlPass = WorkSpace.Instance.UserProfile.SolutionSourceControlPass;
        //            solution.SourceControl.SolutionSourceControlAuthorEmail = WorkSpace.Instance.UserProfile.SolutionSourceControlAuthorEmail;
        //            solution.SourceControl.SolutionSourceControlAuthorName = WorkSpace.Instance.UserProfile.SolutionSourceControlAuthorName;
        //        }

        //        string error = string.Empty;
        //        solution.SourceControl.SolutionFolder = solution.Folder;
        //        solution.SourceControl.RepositoryRootFolder = RepositoryRootFolder;
        //        solution.SourceControl.SourceControlURL = solution.SourceControl.GetRepositoryURL(ref error);
        //        solution.SourceControl.SourceControlLocalFolder = WorkSpace.Instance.UserProfile.SourceControlLocalFolder;
        //        solution.SourceControl.SourceControlProxyAddress = WorkSpace.Instance.UserProfile.SolutionSourceControlProxyAddress;
        //        solution.SourceControl.SourceControlProxyPort = WorkSpace.Instance.UserProfile.SolutionSourceControlProxyPort;
        //        solution.SourceControl.SourceControlTimeout = WorkSpace.Instance.UserProfile.SolutionSourceControlTimeout;

        //        WorkSpace.Instance.SourceControl = solution.SourceControl;
        //        RepositoryItemBase.SetSourceControl(solution.SourceControl);
        //        RepositoryFolderBase.SetSourceControl(solution.SourceControl);
        //    }
        //}

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            //WorkSpace.Instance.Telemetry.AddException(ex);
            //Exceptions to avoid because it source is in some .NET issue
            if (ex.Message == "Value cannot be null.\r\nParameter name: element" && ex.Source == "PresentationCore")//Seems like WPF Bug 
            {
                e.Handled = true;
                return;
            }

            //log it
            Reporter.ToLog(eLogLevel.ERROR, ex.ToString(), ex, TelemetryMetadata.WithTags("ApplicationUnhandledException"));

            //add to dictionary to make sure same exception won't show more than 3 times
            if (mExceptionsDic.ContainsKey(ex.Message))
            {
                mExceptionsDic[ex.Message]++;
            }
            else
            {
                mExceptionsDic.Add(ex.Message, 1);
            }

            if (mExceptionsDic[ex.Message] <= 3)
            {
                if (WorkSpace.Instance != null && !WorkSpace.Instance.RunningInExecutionMode)
                {
                    Ginger.GeneralLib.ExceptionDetailsPage.ShowError(ex);
                }
            }

            Environment.ExitCode = -1;

            if (WorkSpace.Instance != null && !WorkSpace.Instance.RunningInExecutionMode)
            {
                // Clear the err so it will not crash
                e.Handled = true;
            }
        }


        internal static void CheckIn(string Path)
        {
            CheckInPage CIW = new CheckInPage(Path);
            CIW.ShowAsWindow();
        }

        public Dispatcher GetMainWindowDispatcher()
        {
            return MainWindow.Dispatcher;
        }

        internal static Style GetStyle(string key)
        {
            foreach (ResourceDictionary RD in Application.Current.Resources.MergedDictionaries)
            {
                var s = (Style)RD[key];
                if (s != null)
                {
                    return s;
                }
            }
            return null;
        }


        public static event AutomateBusinessFlowEventHandler AutomateBusinessFlowEvent;
        public delegate void AutomateBusinessFlowEventHandler(AutomateEventArgs args);
        public static void OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType eventType, object obj)
        {
            AutomateBusinessFlowEventHandler handler = AutomateBusinessFlowEvent;
            if (handler != null)
            {
                handler(new AutomateEventArgs(eventType, obj));
            }
        }

        CLIProcessor cliProcessor;

        // Main entry point to Ginger UI/CLI
        /// <summary>
        /// Handles the startup sequence of the application. Initializes logging, workspace, 
        /// processes command-line arguments, and determines the running mode (UI or execution).
        /// </summary>
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                InitLogging();

                bool startGrid = ShouldStartGrid(e.Args);
                WorkSpace.Init(new WorkSpaceEventHandler(), startGrid);
                var parserResult = ParseCommandLineArguments(e.Args);

                DoOptions doOptions = ExtractDoOptions(parserResult);

                if (IsExecutionMode(e.Args, doOptions))
                {
                    WorkSpace.Instance.RunningInExecutionMode = true;
                    Reporter.ReportAllAlsoToConsole = true;
                }
                InitializeGingerCore();
                if (!WorkSpace.Instance.RunningInExecutionMode)
                {
                    ProcessGingerUIStartup(doOptions);
                }
                else
                {
                    await RunNewCLI(parserResult);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Unhandled exception in Application_Startup", ex);
            }
       }


        /// <summary>
        /// Initializes the logging mechanism for the application using log4net.
        /// </summary>
        private void InitLogging()
        {
            Amdocs.Ginger.CoreNET.log4netLib.GingerLog.InitLog4Net();
        }

        /// <summary>
        /// Determines whether the application should start with the grid view.
        /// Returns true if there are no command-line arguments.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        /// <returns>True if no arguments are provided, otherwise false.</returns>
        private bool ShouldStartGrid(string[] args)
        {
            return args.Length == 0;
        }

        /// <summary>
        /// Parses the command line arguments and returns the parsed result.
        /// </summary>
        /// <param name="args">The command line arguments.</param>
        /// <returns>The parsed result of the command line arguments.</returns>
        private ParserResult<object> ParseCommandLineArguments(string[] args)
        {
            string[] arguments;
            //Added this codition if only user want to launch Ginger without any solution from browser.
            if (args.Length == 1 && System.Web.HttpUtility.UrlDecode(args[0]).Equals("ginger:///", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }
            if (args.Length == 1)
            {
                string input = args[0];
                input = input.Replace("\n", "").Replace("\r", "");
                input = System.Web.HttpUtility.UrlDecode(input);
                if (input.StartsWith("ginger://"))
                {
                    input = input.Substring("ginger://".Length);
                }
                if (input.EndsWith("/"))
                {
                    input = input.Substring(0, input.Length - 1);
                }
                List<string> resultList = General.SplitWithPaths(input).Select(s => s.Trim('\"', '\'')).ToList();
                arguments = resultList.ToArray();
            }
            else
            {
                arguments = args;
            }

            cliProcessor = new CLIProcessor();
            return arguments.Length != 0 ? cliProcessor.ParseArguments(arguments) : null;
        }
        /// <summary>
        /// Extracts the DoOptions object from the parser result if available and the operation is 'open'.
        /// Otherwise, returns null.
        /// </summary>
        /// <param name="parserResult">Parsed command-line arguments.</param>
        /// <returns>DoOptions object or null.</returns>
        private DoOptions ExtractDoOptions(ParserResult<object> parserResult)
        {
            if (parserResult?.Value is DoOptions tempOptions && tempOptions.Operation == DoOptions.DoOperation.open)
            {
                return tempOptions;
            }
            return null;
        }

        /// <summary>
        /// Determines if the application is in execution mode based on command-line arguments and DoOptions.
        /// Returns true if there are arguments and DoOptions is null.
        /// </summary>
        /// <param name="args">Command-line arguments.</param>
        /// <param name="doOptions">DoOptions extracted from the parsed arguments.</param>
        /// <returns>True if the application is in execution mode, otherwise false.</returns>
        private bool IsExecutionMode(string[] args, DoOptions doOptions)
        {
            if (args.Length == 1 && System.Web.HttpUtility.UrlDecode(args[0]).Equals("ginger:///", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            return args.Length != 0 && doOptions == null;
        }

        /// <summary>
        /// Initializes various core components of Ginger such as class types, workspace, and telemetry.
        /// Logs the startup information.
        /// </summary>
        private void InitializeGingerCore()
        {
            InitClassTypesDictionary();
            WorkSpace.Instance.InitWorkspace(new GingerWorkSpaceReporter(), new DotNetFrameworkHelper());
            WorkSpace.Instance.InitTelemetry();
            Amdocs.Ginger.CoreNET.log4netLib.GingerLog.PrintStartUpInfo();
        }

        /// <summary>
        /// Processes the startup for the Ginger UI. Initializes logging, checks the user profile settings, 
        /// hides the console window, and loads the last solution if applicable.
        /// </summary>
        /// <param name="doOptions">DoOptions object containing user-specific startup options.</param>
        private void ProcessGingerUIStartup(DoOptions doOptions)
        {
            if (WorkSpace.Instance.UserProfile != null && WorkSpace.Instance.UserProfile.AppLogLevel == eAppReporterLoggingLevel.Debug)
            {
                GingerLog.StartCustomTraceListeners();
            }

            HideConsoleWindow();
            bool CheckAutoLoadSolution = false;

            try
            {
                if (WorkSpace.Instance.UserProfile != null)
                {
                    CheckAutoLoadSolution = WorkSpace.Instance.UserProfile.AutoLoadLastSolution;
                }

                if (doOptions != null)
                {
                    WorkSpace.Instance.UserProfile.AutoLoadLastSolution = false;
                }

                StartGingerUI();

                if (doOptions != null && !string.IsNullOrWhiteSpace(doOptions.Solution))
                {
                    if(Directory.Exists(doOptions.Solution))
                    {
                        DoOptionsHandler.Run(doOptions);
                    }
                    else
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "The specified solution folder path does not exist. Please check the path and try again.");
                    }
                   
                }
            }
            finally
            {
                if (doOptions != null)
                {
                    WorkSpace.Instance.UserProfile.AutoLoadLastSolution = CheckAutoLoadSolution;
                }
            }
        }




        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        public void HideConsoleWindow()
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
        }

        public static void ShowConsoleWindow()
        {
            var handle = GetConsoleWindow();
            ShowWindow(handle, SW_SHOW);
        }

        /// <summary>
        /// Runs the new CLI process with the provided parsed arguments.
        /// </summary>
        /// <param name="parserResult">The parsed result of the command line arguments.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task RunNewCLI(ParserResult<object> parserResult)
        {
            try
            {
                if (parserResult != null)
                {
                    await cliProcessor.ProcessParsedArguments(parserResult);
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while processing command-line arguments", ex);
            }
            finally
            {
                System.Windows.Application.Current.Shutdown(Environment.ExitCode);
            }
        }

        /// <summary>
        /// Starts the Ginger UI. Initializes dictionaries and the main window.
        /// </summary>
        public void StartGingerUI()
        {
            if (WorkSpace.Instance.RunningFromUnitTest)
            {
                LoadApplicationDictionaries();
                InitClassTypesDictionary();
            }

            MainWindow = new MainWindow();
            MainWindow.Show();
            GingerCore.General.DoEvents();

            if (WorkSpace.Instance.UserProfile != null)
            {
                LoadApplicationDictionaries(Amdocs.Ginger.Core.eSkinDicsType.Default, WorkSpace.Instance.UserProfile.TerminologyDictionaryType);
            }
            else
            {
                LoadApplicationDictionaries(Amdocs.Ginger.Core.eSkinDicsType.Default, GingerCore.eTerminologyType.Default);
            }

            MainWindow.Init();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            base.OnStartup(e);
        }
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            // log the exception or do other error handling
            if (ex != null)
            {
                Reporter.ToLog(eLogLevel.ERROR, "An unhandled exception occurred: ", ex.InnerException);
            }
        }
    }
}
