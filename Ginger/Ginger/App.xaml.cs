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
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.Repository;
using Ginger.BusinessFlowWindows;
using Ginger.ReporterLib;
using Ginger.Repository;
using Ginger.SourceControl;
using GingerCore;
using GingerCore.Repository;
using GingerWPF.WorkSpaceLib;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

        private Dictionary<string, Int32> mExceptionsDic = new Dictionary<string, int>();



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
            Dictionary<string, Type> list = new Dictionary<string, Type>();
            list.Add("GingerCore.Actions.ActInputValue", typeof(ActInputValue));
            list.Add("GingerCore.Actions.ActReturnValue", typeof(ActReturnValue));
            list.Add("GingerCore.Actions.EnhancedActInputValue", typeof(EnhancedActInputValue));
            list.Add("GingerCore.Environments.GeneralParam", typeof(GeneralParam));
            


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
            Reporter.ToLog(eLogLevel.ERROR, ex.ToString(), ex);

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
                if (WorkSpace.Instance!= null && !WorkSpace.Instance.RunningInExecutionMode)
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



        // Main entry point to Ginger UI/CLI
        private void Application_Startup(object sender, StartupEventArgs e)
        {            
            Amdocs.Ginger.CoreNET.log4netLib.GingerLog.InitLog4Net();

            

            bool startGrid = e.Args.Length == 0; // no need to start grid if we have args
            WorkSpace.Init(new WorkSpaceEventHandler(), startGrid);
            if (e.Args.Length != 0)
            {
                WorkSpace.Instance.RunningInExecutionMode = true;
                Reporter.ReportAllAlsoToConsole = true;  //needed so all reporting will be added to Console      
            }
            // add additional classes from Ginger and GingerCore
            InitClassTypesDictionary();
           
            WorkSpace.Instance.InitWorkspace(new GingerWorkSpaceReporter(), new DotNetFrameworkHelper());            
            
            Amdocs.Ginger.CoreNET.log4netLib.GingerLog.PrintStartUpInfo();            

            if (!WorkSpace.Instance.RunningInExecutionMode)
            {
                if(WorkSpace.Instance.UserProfile.AppLogLevel == eAppReporterLoggingLevel.Debug)
                {
                    try
                    {
                        if (!System.Diagnostics.Trace.Listeners.Contains(CustomTraceListener.Instance))
                        {
                            System.Diagnostics.Trace.Listeners.Add(CustomTraceListener.Instance);

                            System.Diagnostics.Trace.WriteLine("Ginger Start up", "Info");
                        }
                    }
                    catch(Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Custom Trace listerner ", ex.InnerException);
                    }
                    
                }
                HideConsoleWindow();                
                StartGingerUI();// start regular Ginger UI
            }
            else
            {
                RunNewCLI(e.Args);
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

        private async void RunNewCLI(string[] args)
        {                    
            CLIProcessor cLIProcessor = new CLIProcessor();
           await cLIProcessor.ExecuteArgs(args);

            // do proper close !!!         
            System.Windows.Application.Current.Shutdown(Environment.ExitCode);
        }

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


    }
}
