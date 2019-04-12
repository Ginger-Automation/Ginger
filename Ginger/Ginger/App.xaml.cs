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
using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.RunLib;
using Amdocs.Ginger.IO;
using Amdocs.Ginger.Repository;
using Ginger.BusinessFlowWindows;
using Ginger.ReporterLib;
using Ginger.Repository;
using Ginger.SolutionGeneral;
using Ginger.SourceControl;
using GingerCore;
using GingerCore.Repository;
using GingerCore.SourceControl;
using GingerCoreNET.SourceControl;
using GingerWPF.WorkSpaceLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace Ginger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //DO NOT REMOVE- Needed for Log to work
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
                                       (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static readonly string ENCRYPTION_KEY = "D3^hdfr7%ws4Kb56=Qt";//?????

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
            if (bDone) return;
            bDone = true;                
            // TODO: remove after we don't need old serializer to load old repo items
            NewRepositorySerializer.NewRepositorySerializerEvent += RepositorySerializer.NewRepositorySerializer_NewRepositorySerializerEvent;
            
            // Add all RI classes from GingerCore
            NewRepositorySerializer.AddClassesFromAssembly(typeof(GingerCore.Actions.ActButton).Assembly); // GingerCore.dll
            
            // add from Ginger
            NewRepositorySerializer.AddClassesFromAssembly(typeof(Ginger.App).Assembly);

            // Each class which moved from GingerCore to GingerCoreCommon needed to be added here, so it will auto translate
            // For backward compatibility of loading old object name in xml
            Dictionary<string, Type> list = new Dictionary<string, Type>();
            list.Add("GingerCore.Actions.ActInputValue", typeof(ActInputValue));
            list.Add("GingerCore.Actions.ActReturnValue", typeof(ActReturnValue));
            list.Add("GingerCore.Actions.EnhancedActInputValue", typeof(EnhancedActInputValue));
            list.Add("GingerCore.Environments.GeneralParam", typeof(GeneralParam));            
           
            // Lazy load of BF.Acitvities
            NewRepositorySerializer.AddLazyLoadAttr(nameof(BusinessFlow.Activities)); // TODO: add RI type, and use attr on field
            
            NewRepositorySerializer.AddClasses(list);
        }

       

        private static async void HandleAutoRunMode()
        {
            string phase = "Running in Automatic Execution Mode";
            Reporter.ToLog(eLogLevel.INFO, phase);
            
            AutoLogProxy.LogAppOpened();
            
            // TODO: use same CLI !!!!!!!!!!!!!
            var result = await WorkSpace.Instance.RunsetExecutor.RunRunSetFromCommandLine();

            Reporter.ToLog(eLogLevel.INFO, "Closing Ginger automatically...");
            

            //setting the exit code based on execution status
            if (result == 0)
            {
                Reporter.ToLog(eLogLevel.DEBUG, ">> Run Set executed and passed, exit code: 0");
                Environment.ExitCode = 0;//success                    
            }
            else
            {
                Reporter.ToLog(eLogLevel.DEBUG, ">> No indication found for successful execution, exit code: 1");
                Environment.ExitCode = 1;//failure
            }

            AutoLogProxy.LogAppClosed();
            Environment.Exit(Environment.ExitCode);
        }

        public static void DownloadSolution(string SolutionFolder)
        {
            SourceControlBase mSourceControl;
            if ( WorkSpace.Instance.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.GIT)
                mSourceControl = new GITSourceControl();
            else if ( WorkSpace.Instance.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.SVN)
                mSourceControl = new SVNSourceControl();
            else
                mSourceControl = new SVNSourceControl();

            if (mSourceControl != null)
            {
                 WorkSpace.Instance.UserProfile.SourceControlType = mSourceControl.GetSourceControlType;
                mSourceControl.SourceControlURL =  WorkSpace.Instance.UserProfile.SourceControlURL;
                mSourceControl.SourceControlUser =  WorkSpace.Instance.UserProfile.SourceControlUser;
                mSourceControl.SourceControlPass =  WorkSpace.Instance.UserProfile.SourceControlPass;
                mSourceControl.SourceControlLocalFolder =  WorkSpace.Instance.UserProfile.SourceControlLocalFolder;
                mSourceControl.SolutionFolder = SolutionFolder;

                mSourceControl.SourceControlConfigureProxy =  WorkSpace.Instance.UserProfile.SolutionSourceControlConfigureProxy;
                mSourceControl.SourceControlProxyAddress =  WorkSpace.Instance.UserProfile.SolutionSourceControlProxyAddress;
                mSourceControl.SourceControlProxyPort =  WorkSpace.Instance.UserProfile.SolutionSourceControlProxyPort;
                mSourceControl.SourceControlTimeout =  WorkSpace.Instance.UserProfile.SolutionSourceControlTimeout;
                mSourceControl.supressMessage = true;
            }

            if ( WorkSpace.Instance.UserProfile.SourceControlLocalFolder == string.Empty)
            {
                Reporter.ToUser(eUserMsgKey.SourceControlConnMissingLocalFolderInput);
            }
            if (SolutionFolder.EndsWith("\\"))
                SolutionFolder = SolutionFolder.Substring(0, SolutionFolder.Length - 1);
            SolutionInfo sol = new SolutionInfo();
            sol.LocalFolder = SolutionFolder;
            if ( WorkSpace.Instance.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.SVN && Directory.Exists(PathHelper.GetLongPath(sol.LocalFolder)))
                sol.ExistInLocaly = true;
            else if ( WorkSpace.Instance.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.GIT && Directory.Exists(PathHelper.GetLongPath(sol.LocalFolder + @"\.git")))
                sol.ExistInLocaly = true;
            else
                sol.ExistInLocaly = false;
            sol.SourceControlLocation = SolutionFolder.Substring(SolutionFolder.LastIndexOf("\\") + 1);

            if (sol == null)
            {
                Reporter.ToUser(eUserMsgKey.AskToSelectSolution);
                return;
            }

            string ProjectURI = string.Empty;
            if ( WorkSpace.Instance.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.SVN)
            {
                ProjectURI =  WorkSpace.Instance.UserProfile.SourceControlURL.StartsWith("SVN", StringComparison.CurrentCultureIgnoreCase) ?
                sol.SourceControlLocation :  WorkSpace.Instance.UserProfile.SourceControlURL + sol.SourceControlLocation;
            }
            else
            {
                ProjectURI =  WorkSpace.Instance.UserProfile.SourceControlURL;
            }
            bool getProjectResult = true;
            getProjectResult = SourceControlIntegration.CreateConfigFile(mSourceControl);
            if (getProjectResult != true)
                return;
            if (sol.ExistInLocaly == true)
            {
                mSourceControl.RepositoryRootFolder = sol.LocalFolder;
                SourceControlIntegration.GetLatest(sol.LocalFolder, mSourceControl);
            }
            else
                getProjectResult = SourceControlIntegration.GetProject(mSourceControl, sol.LocalFolder, ProjectURI);
        }

       

        private static void HandleSolutionLoadSourceControl(Solution solution)
        {
            string RepositoryRootFolder = string.Empty;
            SourceControlBase.eSourceControlType type = SourceControlIntegration.CheckForSolutionSourceControlType(solution.Folder, ref RepositoryRootFolder);
            if (type == SourceControlBase.eSourceControlType.GIT)
            {
                solution.SourceControl = new GITSourceControl();
            }
            else if (type == SourceControlBase.eSourceControlType.SVN)
            {
                solution.SourceControl = new SVNSourceControl();
            }

            if (solution.SourceControl != null)
            {
                if (string.IsNullOrEmpty( WorkSpace.Instance.UserProfile.SolutionSourceControlUser) || string.IsNullOrEmpty( WorkSpace.Instance.UserProfile.SolutionSourceControlPass))
                {
                    if ( WorkSpace.Instance.UserProfile.SourceControlUser != null &&  WorkSpace.Instance.UserProfile.SourceControlPass != null)
                    {
                        solution.SourceControl.SourceControlUser =  WorkSpace.Instance.UserProfile.SourceControlUser;
                        solution.SourceControl.SourceControlPass =  WorkSpace.Instance.UserProfile.SourceControlPass;
                        solution.SourceControl.SolutionSourceControlAuthorEmail =  WorkSpace.Instance.UserProfile.SolutionSourceControlAuthorEmail;
                        solution.SourceControl.SolutionSourceControlAuthorName =  WorkSpace.Instance.UserProfile.SolutionSourceControlAuthorName;
                    }
                }
                else
                {
                    solution.SourceControl.SourceControlUser =  WorkSpace.Instance.UserProfile.SolutionSourceControlUser;
                    solution.SourceControl.SourceControlPass =  WorkSpace.Instance.UserProfile.SolutionSourceControlPass;
                    solution.SourceControl.SolutionSourceControlAuthorEmail =  WorkSpace.Instance.UserProfile.SolutionSourceControlAuthorEmail;
                    solution.SourceControl.SolutionSourceControlAuthorName =  WorkSpace.Instance.UserProfile.SolutionSourceControlAuthorName;
                }

                string error = string.Empty;
                solution.SourceControl.SolutionFolder = solution.Folder;
                solution.SourceControl.RepositoryRootFolder = RepositoryRootFolder;
                solution.SourceControl.SourceControlURL = solution.SourceControl.GetRepositoryURL(ref error);
                solution.SourceControl.SourceControlLocalFolder =  WorkSpace.Instance.UserProfile.SourceControlLocalFolder;
                solution.SourceControl.SourceControlProxyAddress =  WorkSpace.Instance.UserProfile.SolutionSourceControlProxyAddress;
                solution.SourceControl.SourceControlProxyPort =  WorkSpace.Instance.UserProfile.SolutionSourceControlProxyPort;
                solution.SourceControl.SourceControlTimeout =  WorkSpace.Instance.UserProfile.SolutionSourceControlTimeout;

                WorkSpace.Instance.SourceControl = solution.SourceControl;
                RepositoryItemBase.SetSourceControl(solution.SourceControl);
                RepositoryFolderBase.SetSourceControl(solution.SourceControl);
            }
        }

        

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Exception ex = e.Exception;
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
                mExceptionsDic[ex.Message]++;
            else
                mExceptionsDic.Add(ex.Message, 1);

            if (mExceptionsDic[ex.Message] <= 3)
            {
                Ginger.GeneralLib.ExceptionDetailsPage.ShowError(ex);
            }

            // Clear the err so it will not crash
            e.Handled = true;
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


        // This is the main entry point to Ginger UI/CLI
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            WorkSpace.Init(new WorkSpaceEventHandler());

            // add additional classed from Ginger and GingerCore
            InitClassTypesDictionary();

            Console.Title = "Ginger";
            Console.WriteLine("Starting Ginger");
            Console.WriteLine("Version: " + WorkSpace.Instance.ApplicationInfo.AppVersion);            

            if (e.Args.Length == 0)
            {
                HideConsoleWindow();
                // start regular Ginger UI
                StartGingerUI();                
            }
            else
            {
                // handle CLI
                if (e.Args[0].StartsWith("ConfigFile"))
                {                    
                    // This Ginger is running with run set config will do the run and close GingerInitApp();                                
                    StartGingerExecutor();
                }
                else
                {
                    RunNewCLI(e.Args);                    
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

        private void RunNewCLI(string[] args)
        {
            WorkSpace.Instance.InitWorkspace(new GingerWorkSpaceReporter(), new RepositoryItemFactory());
            WorkSpace.Instance.RunningInExecutionMode = true;
            Reporter.ReportAllAlsoToConsole = true;  //needed so all reportering will be added to Console                             
            
            CLIProcessor.ExecuteArgs(args);
            // do proper close !!!         
            System.Windows.Application.Current.Shutdown();
        }

        private void StartGingerExecutor()
        {
            WorkSpace.Instance.InitWorkspace(new GingerWorkSpaceReporter(), new RepositoryItemFactory());
            WorkSpace.Instance.RunningInExecutionMode = true;
            Reporter.ReportAllAlsoToConsole = true;  //needed so all reportering will be added to Console                             
            HandleAutoRunMode();
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

            WorkSpace.Instance.InitWorkspace(new GingerWorkSpaceReporter(), new RepositoryItemFactory());

            MainWindow.Init();
            MainWindow.HideSplash();
        }

       
    }
}
