#region License
/*
Copyright © 2014-2018 European Support Limited

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
using Amdocs.Ginger.IO;
using Amdocs.Ginger.Repository;
using Ginger.BusinessFlowWindows;
using Ginger.SolutionGeneral;
using Ginger.Extensions;
using Ginger.Reports;
using Ginger.Repository;
using Ginger.Run;
using Ginger.SolutionWindows;
using Ginger.SourceControl;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Activities;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.Platforms;
using GingerCore.Repository;
using GingerCore.Repository.UpgradeLib;
using GingerCore.SourceControl;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerCoreNET.SourceControl;
using GingerWPF;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerWPF.WorkSpaceLib;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using System.Windows.Input;
using Amdocs.Ginger.Common.Repository;

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


        public static readonly string ENCRYPTION_KEY = "D3^hdfr7%ws4Kb56=Qt";

        public static FileVersionInfo ApplicationInfo
        {
            get
            {
                return FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            }
        }
        public static string AppName = ApplicationInfo.FileDescription;//"Ginger"
        public static string AppFullProductName = ApplicationInfo.ProductName;//"Amdocs Ginger Automation"

        private static string mAppVersion = String.Empty;
        public static string AppVersion
        {
            get
            {
                if (mAppVersion == string.Empty)
                {
                    if (ApplicationInfo.ProductPrivatePart != 0)//Alpha
                    {
                        mAppVersion = string.Format("{0}.{1}.{2}.{3}", ApplicationInfo.ProductMajorPart, ApplicationInfo.ProductMinorPart, ApplicationInfo.ProductBuildPart, ApplicationInfo.ProductPrivatePart);
                        mAppVersion += "(Alpha, Build Time: " + App.AppBuildTime.ToString("dd-MMM-yyyy hh:mm tt") + ")";
                    }
                    else if (ApplicationInfo.ProductBuildPart != 0)//Beta
                    {
                        mAppVersion = string.Format("{0}.{1}.{2}", ApplicationInfo.ProductMajorPart, ApplicationInfo.ProductMinorPart, ApplicationInfo.ProductBuildPart);
                        mAppVersion += "(Beta, Build Date: " + App.AppBuildTime.ToString("dd-MMM-yyyy") + ")";
                    }
                    else//Official Release
                    {
                        mAppVersion = string.Format("{0}.{1}", ApplicationInfo.ProductMajorPart, ApplicationInfo.ProductMinorPart);
                    }
                }

                return mAppVersion;
            }
        }

        private static string mAppShortVersion = String.Empty;
        public static string AppShortVersion
        {
            get
            {
                if (mAppShortVersion == string.Empty)
                {
                    if (ApplicationInfo.ProductPrivatePart != 0)//Alpha
                    {
                        mAppShortVersion = string.Format("{0}.{1}.{2}.{3}", ApplicationInfo.ProductMajorPart, ApplicationInfo.ProductMinorPart, ApplicationInfo.ProductBuildPart, ApplicationInfo.ProductPrivatePart);
                    }
                    else if (ApplicationInfo.ProductBuildPart != 0)//Beta
                    {
                        mAppShortVersion = string.Format("{0}.{1}.{2}", ApplicationInfo.ProductMajorPart, ApplicationInfo.ProductMinorPart, ApplicationInfo.ProductBuildPart);
                    }
                    else//Official Release
                    {
                        mAppShortVersion = string.Format("{0}.{1}", ApplicationInfo.ProductMajorPart, ApplicationInfo.ProductMinorPart);
                    }
                }

                return mAppShortVersion;
            }
        }

        public static Ginger.Functionalties.SolutionAutoSave AppSolutionAutoSave = new Ginger.Functionalties.SolutionAutoSave();

        public static Ginger.Functionalties.SolutionRecover AppSolutionRecover = new Ginger.Functionalties.SolutionRecover();

        static bool mIsReady = false;
        public bool IsReady { get { return mIsReady; } }

        private static bool mAppBuildTimeCalculated = false;
        private static DateTime mAppBuildTime;
        public static DateTime AppBuildTime
        {
            get
            {
                if (mAppBuildTimeCalculated == false)
                {
                    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(assembly.Location);
                    mAppBuildTime = fileInfo.LastWriteTime;
                    mAppBuildTimeCalculated = true;
                }

                return mAppBuildTime;
            }
        }

        public static TextBlock RunsetBFTextbox = null;
        public static TextBlock RunsetActivityTextbox = null;
        public static TextBlock RunsetActionTextbox = null;
        public static string TempFolder
        {
            get
            {
                return System.IO.Path.GetDirectoryName(System.IO.Path.GetTempFileName()) + "\\Ginger_Email_Reports";
            }
        }

        //Making the MainWindow Static to allow control it
        public new static MainWindow MainWindow { get; set; }
        //End
        private Dictionary<string, Int32> _exceptionsDic = new Dictionary<string, int>();

        public static Amdocs.Ginger.CoreNET.Execution.eRunStatus RunSetExecutionStatus = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Failed;
        /// <summary>
        /// Hold all Run Set execution data + execution methods
        /// </summary>        
        public static RunsetExecutor RunsetExecutor = new RunsetExecutor();

        //TODO: whenever changed check if isDirty - and ask the user if to save
        public static RepositoryItemBase CurrentRepositoryItem { get; set; }

        public static ITreeViewItem CurrentSelectedTreeItem { get; set; }

        public static string RecoverFolderPath = null;
        public static IEnumerable<object> CurrentFolderItem { get; set; }

        // Business Flow Objects        
        private static ProjEnvironment mAutomateTabEnvironment;
        public static ProjEnvironment AutomateTabEnvironment
        {
            get
            {
                return mAutomateTabEnvironment;
            }
            set
            {
                mAutomateTabEnvironment = value;
                App.AutomateTabGingerRunner.ProjEnvironment = mAutomateTabEnvironment;
                App.UserProfile.RecentEnvironment = mAutomateTabEnvironment.Guid;
            }
        }

        public static GingerRunner AutomateTabGingerRunner = new GingerRunner(GingerRunner.eExecutedFrom.Automation);


        public static AppProgressBar AppProgressBar { get; set; }

        public static UserProfile UserProfile { get; set; }

        public static event PropertyChangedEventHandler PropertyChanged;
        protected static void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                // Need to be App not biz flow, temp
                handler(mBusinessFlow, new PropertyChangedEventArgs(name));
            }
        }

        public static SplashWindow AppSplashWindow { get; set; }

        public static ucGrid ActionsGrid { get; set; }


        public static BusinessFlow LastBusinessFlow { get; set; }
        private static BusinessFlow mBusinessFlow;
        public static BusinessFlow BusinessFlow
        {
            get { return mBusinessFlow; }
            set
            {
                LastBusinessFlow = mBusinessFlow;
                mBusinessFlow = value;

                if (value != null)
                {
                    if (mBusinessFlow.Activities.Count > 0)
                    {
                        mBusinessFlow.CurrentActivity = mBusinessFlow.Activities[0];
                    }
                    UserProfile.RecentBusinessFlow = App.BusinessFlow.Guid;
                    UserProfile.Solution.LastBusinessFlowFileName = mBusinessFlow.FileName;
                    AddLastUsedBusinessFlow(mBusinessFlow);
                }

                App.AutomateTabGingerRunner.BusinessFlows.Clear();
                if (App.BusinessFlow != null)
                    App.AutomateTabGingerRunner.BusinessFlows.Add(App.BusinessFlow);
                App.AutomateTabGingerRunner.CurrentBusinessFlow = App.BusinessFlow;

                UpdateApplicationsAgentsMapping();
                OnPropertyChanged(nameof(BusinessFlow));
            }
        }

        private static void AddLastUsedBusinessFlow(BusinessFlow BF)
        {
            if (BF != null)
            {
                App.UserProfile.Solution.RecentlyUsedBusinessFlows.AddItem(BF.FileName);
                App.UserProfile.SaveUserProfile();
            }
        }


        //public static string LocalApplicationData
        //{
        //    get
        //    {
        //        //TODO: check where it goes - not roaming,.,
        //        string s = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        //        s = s + @"\amdocs\" + App.AppName;
        //        return s;
        //    }
        //}

        public static bool RunningFromConfigFile = false;

        public static bool RunningFromUnitTest = false;

        internal static void ObjFieldBinding(System.Windows.Controls.Control control, DependencyProperty dependencyProperty, object obj, string property, BindingMode BindingMode = BindingMode.TwoWay)
        {
            //TODO: add Inotify on the obj.attr - so code changes to property will be reflected
            //TODO: check perf impact + reuse existing binding on same obj.prop

            GingerCore.General.ObjFieldBinding(control, dependencyProperty, obj, property, BindingMode);
        }

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

        public static void InitApp()
        {
            AppReporter.ReportEvent += Reporter.AppReporter_ReportEvent;

            // Add event handler for handling non-UI thread exceptions.
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(StandAloneThreadExceptionHandler);

            if (Environment.GetCommandLineArgs().Count() > 1)
            {
                // When running from unit test there are args, so we set a flag in GingerAutomator to make sure Ginger will Launch
                // and will not try to process the args for RunSet auto run
                if (RunningFromUnitTest)
                {
                    // do nothing for now, but later on we might want to process and check auto run too
                }
                else
                {
                    // This Ginger is running with run set config will do the run and close Ginger
                    RunningFromConfigFile = true;
                    Reporter.CurrentAppLogLevel = eAppReporterLoggingLevel.Debug;
                    Reporter.AddAllReportingToConsole = true;//running from command line so show logs and messages also on Console (to be reviewed by Jenkins console and others)               
                }
            }

            string phase = string.Empty;

            RepositoryItemHelper.RepositoryItemFactory = new RepositoryItemFactory();


            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);

            WorkSpace.Instance.BetaFeatures = BetaFeatures.LoadUserPref();
            WorkSpace.Instance.BetaFeatures.PropertyChanged += BetaFeatureChanged;

            AutomateBusinessFlowEvent += App_AutomateBusinessFlowEvent;

            if (WorkSpace.Instance.BetaFeatures.ShowDebugConsole)
            {
                DebugConsoleWindow.Show();
                WorkSpace.Instance.BetaFeatures.DisplayStatus();
            }

            Reporter.ToLog(eAppReporterLogLevel.INFO, "######################## Application version " + App.AppVersion + " Started ! ########################");

            AppSplashWindow.LoadingInfo("Init Application");

            // We init the classed dictionary for the Repository Serialize only once
            InitClassTypesDictionary();

            // TODO: need to add a switch what we get from old ginger based on magic key

            phase = "Loading User Profile";
            Reporter.ToLog(eAppReporterLogLevel.INFO, phase);
            AppSplashWindow.LoadingInfo(phase);
            App.UserProfile = UserProfile.LoadUserProfile();

            phase = "Configuring User Type";
            Reporter.ToLogAndConsole(eAppReporterLogLevel.INFO, phase);
            AppSplashWindow.LoadingInfo(phase);
            UserProfile.LoadUserTypeHelper();


            phase = "Loading User Selected Resource Dictionaries";
            Reporter.ToLog(eAppReporterLogLevel.INFO, phase);
            AppSplashWindow.LoadingInfo(phase);
            if (App.UserProfile != null)
                LoadApplicationDictionaries(Amdocs.Ginger.Core.eSkinDicsType.Default, App.UserProfile.TerminologyDictionaryType);
            else
                LoadApplicationDictionaries(Amdocs.Ginger.Core.eSkinDicsType.Default, GingerCore.eTerminologyType.Default);

            Reporter.ToLog(eAppReporterLogLevel.INFO, "Loading user messages pool");
            UserMessagesPool.LoadUserMessgaesPool();
            GingerHelperMsgsPool.LoadGingerHelperMsgsPool();

            Reporter.ToLog(eAppReporterLogLevel.INFO, "Init the Centralized Auto Log");
            AutoLogProxy.Init(App.AppVersion);

            Reporter.ToLog(eAppReporterLogLevel.INFO, "Initializing the Source control");
            AppSplashWindow.LoadingInfo(phase);

            phase = "Loading the Main Window";
            Reporter.ToLog(eAppReporterLogLevel.INFO, phase);
            AppSplashWindow.LoadingInfo(phase);
            MainWindow = new Ginger.MainWindow();
            MainWindow.Show();
            MainWindow.Init();

            // If we have command line params process them and do not load MainWindow
            if (RunningFromConfigFile == true)
            {
                HandleAutoRunMode();
            }

            phase = "Application was loaded and ready";
            Reporter.ToLog(eAppReporterLogLevel.INFO, phase);
            AppSplashWindow.LoadingInfo("Ready!");
            App.AppSplashWindow = null;

            AutoLogProxy.LogAppOpened();

            AutomateTabGingerRunner.GiveUserFeedback = true;

            if ((App.UserProfile.Solution != null) && (App.UserProfile.Solution.ExecutionLoggerConfigurationSetList != null))
            {
                AutomateTabGingerRunner.ExecutionLogger.Configuration = App.UserProfile.Solution.ExecutionLoggerConfigurationSetList.Where(x => (x.IsSelected == true)).FirstOrDefault();
            }

            // Register our own Ginger tool tip handler
            //--Canceling customize tooltip for now due to many issues and no real added value            

            mIsReady = true;

        }

        private static void StandAloneThreadExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            if (RunningFromUnitTest)
            {
                // happen when we close Ginger from unit tests
                if (e.ExceptionObject is System.Runtime.InteropServices.InvalidComObjectException || e.ExceptionObject is System.Threading.Tasks.TaskCanceledException)
                {
                    Console.WriteLine("StandAloneThreadExceptionHandler: Running from unit test ignoring error on ginger close");
                    return;
                }
            }
            Reporter.ToLog(eAppReporterLogLevel.FATAL, ">>>>>>>>>>>>>> Error occurred on stand alone thread(non UI) - " + e.ExceptionObject);
            //Reporter.ToUser(eUserMsgKeys.ThreadError, "Error occurred on stand alone thread - " + e.ExceptionObject.ToString());

            if (App.RunningFromConfigFile == false)
            {
                App.AppSolutionAutoSave.DoAutoSave();
            }

            /// if (e.IsTerminating)...
            /// 
            //TODO: show exception
            // save work to temp folder
            // enable user to save work
            // ask if to restart/close
            // when loading check restore and restore
        }


        static bool bDone = false;
        public static void InitClassTypesDictionary()
        {
            //TODO: cleanup after all RIs moved to GingerCoreCommon

            if (bDone) return;
            bDone = true;

            // TODO: remove after we don't need old serializer to load old repo items
            NewRepositorySerializer.NewRepositorySerializerEvent += RepositorySerializer.NewRepositorySerializer_NewRepositorySerializerEvent;

            // Add all RI classes from GingerCoreCommon
            NewRepositorySerializer.AddClassesFromAssembly(typeof(RepositoryItemBase).Assembly);

            // Add all RI classes from GingerCore
            NewRepositorySerializer.AddClassesFromAssembly(typeof(Act).Assembly);

            // add  old Plugins - TODO: remove later when we change to new plugins
            NewRepositorySerializer.AddClassesFromAssembly(typeof(GingerPlugIns.ActionsLib.PlugInActionsBase).Assembly);


            // add from Ginger - items like RunSetConfig
            NewRepositorySerializer.AddClassesFromAssembly(typeof(Ginger.App).Assembly);

            // Each class which moved from GingerCore to GingerCoreCommon needed to be added here, so it will auto translate
            // For backward compatibility of loading old object name in xml
            Dictionary<string, Type> list = new Dictionary<string, Type>();
            list.Add("GingerCore.Actions.ActInputValue", typeof(ActInputValue));
            list.Add("GingerCore.Actions.ActReturnValue", typeof(ActReturnValue));
            list.Add("GingerCore.Actions.EnhancedActInputValue", typeof(EnhancedActInputValue));
            list.Add("GingerCore.Environments.GeneralParam", typeof(GeneralParam));

            // Put back for Lazy load of BF.Acitvities
            NewRepositorySerializer.AddLazyLoadAttr(nameof(BusinessFlow.Activities)); // TODO: add RI type, and use attr on field


            // Verify the old name used in XML
            //list.Add("GingerCore.Actions.RepositoryItemTag", typeof(RepositoryItemTag));
            //list.Add("GingerCore.Actions.EnhancedActInputValue", typeof(EnhancedActInputValue));

            // TODO: change to SR2  if we want the files to be loaded convert and save with the new SR2

            //if (WorkSpace.Instance.BetaFeatures.UseNewRepositorySerializer)
            //{
            //RepositorySerializer2 RS2 = new RepositorySerializer2();

            //SolutionRepository.mRepositorySerializer = RS2;
            //RepositoryFolderBase.mRepositorySerializer = RS2;
            //    ObservableListSerializer.RepositorySerializer = RS2;

            //}
            //else
            //{
            //        SolutionRepository.mRepositorySerializer = new RepositorySerializer();
            //        RepositoryFolderBase.mRepositorySerializer = new RepositorySerializer();
            //}

            NewRepositorySerializer.AddClasses(list);

        }

        private static async void HandleAutoRunMode()
        {
            string phase = "Running in Automatic Execution Mode";
            Reporter.ToLog(eAppReporterLogLevel.INFO, phase);
            Reporter.CurrentAppLogLevel = eAppReporterLoggingLevel.Debug;
            Reporter.SetRunConfigMode(true);
            AutoLogProxy.LogAppOpened();
            AppSplashWindow.LoadingInfo(phase);

            var result = await App.RunsetExecutor.RunRunSetFromCommandLine();

            Reporter.ToLog(eAppReporterLogLevel.INFO, "Closing Ginger automatically...");
            App.MainWindow.CloseWithoutAsking();
            //TODO: find a way not to open Main window at all
            Reporter.ToLog(eAppReporterLogLevel.INFO, "Ginger UI Closed.");

            //setting the exit code based on execution status
            if (result == 0)
            {
                Reporter.ToLog(eAppReporterLogLevel.INFO, ">> Run Set executed and passed, exit code: 0");
                Environment.ExitCode = 0;//success                    
            }
            else
            {
                Reporter.ToLog(eAppReporterLogLevel.INFO, ">> No indication found for successful execution, exit code: 1");
                Environment.ExitCode = 1;//failure
            }

            AutoLogProxy.LogAppClosed();
            Environment.Exit(Environment.ExitCode);
        }

        public static void FillComboFromEnumVal(ComboBox comboBox, Object EnumValue, List<object> values = null, bool sortValues = true, ListCollectionView valuesCollView = null)
        {
            GingerCore.General.FillComboFromEnumObj(comboBox, EnumValue, values, sortValues, valuesCollView);
        }

        public static void DownloadSolution(string SolutionFolder)
        {
            SourceControlBase mSourceControl;
            if (App.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.GIT)
                mSourceControl = new GITSourceControl();
            else if (App.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.SVN)
                mSourceControl = new SVNSourceControl();
            else
                mSourceControl = new SVNSourceControl();

            if (mSourceControl != null)
            {
                App.UserProfile.SourceControlType = mSourceControl.GetSourceControlType;
                mSourceControl.SourceControlURL = App.UserProfile.SourceControlURL;
                mSourceControl.SourceControlUser = App.UserProfile.SourceControlUser;
                mSourceControl.SourceControlPass = App.UserProfile.SourceControlPass;
                mSourceControl.SourceControlLocalFolder = App.UserProfile.SourceControlLocalFolder;
                mSourceControl.SolutionFolder = SolutionFolder;

                mSourceControl.SourceControlConfigureProxy = App.UserProfile.SolutionSourceControlConfigureProxy;
                mSourceControl.SourceControlProxyAddress = App.UserProfile.SolutionSourceControlProxyAddress;
                mSourceControl.SourceControlProxyPort = App.UserProfile.SolutionSourceControlProxyPort;
                mSourceControl.SourceControlTimeout = App.UserProfile.SolutionSourceControlTimeout;
                mSourceControl.supressMessage = true;
            }

            if (App.UserProfile.SourceControlLocalFolder == string.Empty)
            {
                Reporter.ToUser(eUserMsgKeys.SourceControlConnMissingLocalFolderInput);
            }
            if (SolutionFolder.EndsWith("\\"))
                SolutionFolder = SolutionFolder.Substring(0, SolutionFolder.Length - 1);
            SolutionInfo sol = new SolutionInfo();
            sol.LocalFolder = SolutionFolder;
            if (App.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.SVN && Directory.Exists(PathHelper.GetLongPath(sol.LocalFolder)))
                sol.ExistInLocaly = true;
            else if (App.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.GIT && Directory.Exists(PathHelper.GetLongPath(sol.LocalFolder + @"\.git")))
                sol.ExistInLocaly = true;
            else
                sol.ExistInLocaly = false;
            sol.SourceControlLocation = SolutionFolder.Substring(SolutionFolder.LastIndexOf("\\") + 1);

            if (sol == null)
            {
                Reporter.ToUser(eUserMsgKeys.AskToSelectSolution);
                return;
            }

            string ProjectURI = string.Empty;
            if (App.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.SVN)
            {
                ProjectURI = App.UserProfile.SourceControlURL.StartsWith("SVN", StringComparison.CurrentCultureIgnoreCase) ?
                sol.SourceControlLocation : App.UserProfile.SourceControlURL + sol.SourceControlLocation;
            }
            else
            {
                ProjectURI = App.UserProfile.SourceControlURL;
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

        static bool mLoadingSolution;
        public static bool LoadingSolution
        {
            get
            {
                return mLoadingSolution;
            }
        }

        private static void SolutionCleanup()
        {
            App.UserProfile.Solution = null;
            App.AutomateTabGingerRunner.ClearAgents();
            App.BusinessFlow = null;
            AutoLogProxy.SetAccount("");
            WorkSpace.Instance.SolutionRepository = null;
            WorkSpace.Instance.SourceControl = null;
        }

        public static bool SetSolution(string SolutionFolder)
        {
            //clear existing solution data
            try
            {
                Reporter.ToLog(eAppReporterLogLevel.INFO, string.Format("Loading the Solution '{0}'", SolutionFolder));
                mLoadingSolution = true;
                OnPropertyChanged(nameof(LoadingSolution));

                // Cleanup last loaded solution 
                WorkSpace.Instance.LocalGingerGrid.Reset();  //Clear the grid
                if (!App.RunningFromConfigFile)
                {
                    AppSolutionAutoSave.SolutionAutoSaveEnd();
                }

                //Cleanup
                SolutionCleanup();

                if (!SolutionFolder.EndsWith(@"\")) SolutionFolder += @"\";
                string SolFile = System.IO.Path.Combine(SolutionFolder, @"Ginger.Solution.xml");
                if (File.Exists(PathHelper.GetLongPath(SolFile)))
                {
                    //get Solution files
                    IEnumerable<string> solutionFiles = Solution.SolutionFiles(SolutionFolder);

                    //check if Ginger Upgrade is needed for loading this Solution
                    try
                    {
                        Reporter.ToLog(eAppReporterLogLevel.INFO, "Checking if Ginger upgrade is needed for loading the Solution");
                        ConcurrentBag<string> higherVersionFiles = SolutionUpgrade.GetSolutionFilesCreatedWithRequiredGingerVersion(solutionFiles, SolutionUpgrade.eGingerVersionComparisonResult.HigherVersion);
                        if (higherVersionFiles.Count > 0)
                        {
                            if (App.RunningFromConfigFile == false && RunningFromUnitTest == false)
                            {
                                UpgradePage gingerUpgradePage = new UpgradePage(SolutionUpgradePageViewMode.UpgradeGinger, SolutionFolder, string.Empty, higherVersionFiles.ToList());
                                gingerUpgradePage.ShowAsWindow();
                            }
                            Reporter.ToLog(eAppReporterLogLevel.WARN, "Ginger upgrade is needed for loading the Solution, aborting Solution load.");
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error occurred while checking if Solution requires Ginger Upgrade", ex);
                    }

                    Solution sol = Solution.LoadSolution(SolFile);

                    if (sol != null)
                    {
                        WorkSpace.Instance.SolutionRepository = CreateGingerSolutionRepository();
                        WorkSpace.Instance.SolutionRepository.Open(SolutionFolder);

                        HandleSolutionLoadSourceControl(sol);
                        HandleAutomateRunner(sol);

                        ValueExpression.SolutionFolder = SolutionFolder;
                        BusinessFlow.SolutionVariables = sol.Variables;

                        App.UserProfile.Solution = sol;
                        App.UserProfile.Solution.SetReportsConfigurations();
                        App.UserProfile.LoadRecentAppAgentMapping();
                        AutoLogProxy.SetAccount(sol.Account);

                        SetDefaultBusinessFlow();

                        if (!App.RunningFromConfigFile)
                        {
                            DoSolutionAutoSaveAndRecover();
                        }

                        //Offer to upgrade Solution items to current version
                        try
                        {
                            if (App.UserProfile.DoNotAskToUpgradeSolutions == false && App.RunningFromConfigFile == false && RunningFromUnitTest == false)
                            {
                                ConcurrentBag<string> lowerVersionFiles = SolutionUpgrade.GetSolutionFilesCreatedWithRequiredGingerVersion(solutionFiles, SolutionUpgrade.eGingerVersionComparisonResult.LowerVersion);
                                if (lowerVersionFiles != null && lowerVersionFiles.Count > 0)
                                {
                                    UpgradePage solutionUpgradePage = new UpgradePage(SolutionUpgradePageViewMode.UpgradeSolution, sol.Folder, sol.Name, lowerVersionFiles.ToList());
                                    solutionUpgradePage.ShowAsWindow();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error occurred while checking if Solution files should be Upgraded", ex);
                        }

                        App.UserProfile.AddSolutionToRecent(sol);
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKeys.SolutionLoadError, "Load solution from file failed.");
                        return false;
                    }
                }
                else
                {
                    Reporter.ToUser(eUserMsgKeys.BeginWithNoSelectSolution);
                    return false;
                }


                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, "Error occurred while loading the solution", ex);
                SolutionCleanup();
                throw ex;
            }
            finally
            {
                mLoadingSolution = false;
                OnPropertyChanged(nameof(LoadingSolution));
                Reporter.ToLog(eAppReporterLogLevel.INFO, string.Format("Finished Loading the Solution '{0}'", SolutionFolder));
                Mouse.OverrideCursor = null;
            }
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
                if (string.IsNullOrEmpty(App.UserProfile.SolutionSourceControlUser) || string.IsNullOrEmpty(App.UserProfile.SolutionSourceControlPass))
                {
                    if (App.UserProfile.SourceControlUser != null && App.UserProfile.SourceControlPass != null)
                    {
                        solution.SourceControl.SourceControlUser = App.UserProfile.SourceControlUser;
                        solution.SourceControl.SourceControlPass = App.UserProfile.SourceControlPass;
                        solution.SourceControl.SolutionSourceControlAuthorEmail = App.UserProfile.SolutionSourceControlAuthorEmail;
                        solution.SourceControl.SolutionSourceControlAuthorName = App.UserProfile.SolutionSourceControlAuthorName;
                    }
                }
                else
                {
                    solution.SourceControl.SourceControlUser = App.UserProfile.SolutionSourceControlUser;
                    solution.SourceControl.SourceControlPass = App.UserProfile.SolutionSourceControlPass;
                    solution.SourceControl.SolutionSourceControlAuthorEmail = App.UserProfile.SolutionSourceControlAuthorEmail;
                    solution.SourceControl.SolutionSourceControlAuthorName = App.UserProfile.SolutionSourceControlAuthorName;
                }

                string error = string.Empty;
                solution.SourceControl.SolutionFolder = solution.Folder;
                solution.SourceControl.RepositoryRootFolder = RepositoryRootFolder;
                solution.SourceControl.SourceControlURL = solution.SourceControl.GetRepositoryURL(ref error);
                solution.SourceControl.SourceControlLocalFolder = App.UserProfile.SourceControlLocalFolder;
                solution.SourceControl.SourceControlProxyAddress = App.UserProfile.SolutionSourceControlProxyAddress;
                solution.SourceControl.SourceControlProxyPort = App.UserProfile.SolutionSourceControlProxyPort;
                solution.SourceControl.SourceControlTimeout = App.UserProfile.SolutionSourceControlTimeout;

                WorkSpace.Instance.SourceControl = solution.SourceControl;
                RepositoryItemBase.SetSourceControl(solution.SourceControl);
                RepositoryFolderBase.SetSourceControl(solution.SourceControl);
            }
        }

        private static void HandleAutomateRunner(Solution solution)
        {
            App.AutomateTabGingerRunner.SolutionFolder = solution.Folder;
            App.AutomateTabGingerRunner.SolutionAgents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>();
            App.AutomateTabGingerRunner.SolutionApplications = solution.ApplicationPlatforms;
            App.AutomateTabGingerRunner.DSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            App.AutomateTabGingerRunner.CurrentSolution = solution;
        }

        private static void DoSolutionAutoSaveAndRecover()
        {
            //Init
            AppSolutionAutoSave.SolutionInit(App.UserProfile.Solution.Folder);
            AppSolutionRecover.SolutionInit(App.UserProfile.Solution.Folder);

            //start Auto Save
            AppSolutionAutoSave.SolutionAutoSaveStart();

            //check if Recover is needed
            if (!App.UserProfile.DoNotAskToRecoverSolutions)
                AppSolutionRecover.SolutionRecoverStart();
        }




        public static SolutionRepository CreateGingerSolutionRepository()
        {
            SolutionRepository SR = new SolutionRepository();

            SR.AddItemInfo<BusinessFlow>("*.Ginger.BusinessFlow.xml", @"~\BusinessFlows", true, GingerDicser.GetTermResValue(eTermResKey.BusinessFlows), PropertyNameForFileName: nameof(BusinessFlow.Name));

            SR.AddItemInfo<ApplicationAPIModel>("*.Ginger.ApplicationAPIModel.xml", @"~\Applications Models\API Models", true, "API Models", PropertyNameForFileName: nameof(ApplicationAPIModel.Name));
            SR.AddItemInfo<GlobalAppModelParameter>("*.Ginger.GlobalAppModelParameter.xml", @"~\Applications Models\Global Models Parameters", true, "Global Model Parameters", PropertyNameForFileName: nameof(GlobalAppModelParameter.PlaceHolder));
            SR.AddItemInfo<ApplicationPOMModel>("*.Ginger.ApplicationPOMModel.xml", @"~\Applications Models\POM Models", true, "POM Models", PropertyNameForFileName: nameof(ApplicationPOMModel.Name));

            SR.AddItemInfo<ProjEnvironment>("*.Ginger.Environment.xml", @"~\Environments", true, "Environments", PropertyNameForFileName: nameof(ProjEnvironment.Name));
            SR.AddItemInfo<ALMDefectProfile>("*.Ginger.ALMDefectProfile.xml", @"~\ALMDefectProfiles", true, "ALM Defect Profiles", PropertyNameForFileName: nameof(ALMDefectProfile.Name));

            SR.AddItemInfo<Agent>("*.Ginger.Agent.xml", @"~\Agents", true, "Agents", PropertyNameForFileName: nameof(Agent.Name));

            //TODO: check if below 2 reports folders are realy needed
            SR.AddItemInfo<HTMLReportConfiguration>("*.Ginger.HTMLReportConfiguration.xml", @"~\HTMLReportConfigurations", true, "HTMLReportConfigurations", PropertyNameForFileName: nameof(HTMLReportsConfiguration.Name));
            SR.AddItemInfo<HTMLReportTemplate>("*.Ginger.HTMLReportTemplate.xml", @"~\HTMLReportConfigurations\HTMLReportTemplate", true, "HTMLReportTemplate", PropertyNameForFileName: nameof(HTMLReportTemplate.Name));

            SR.AddItemInfo<ReportTemplate>("*.Ginger.ReportTemplate.xml", @"~\HTMLReportConfigurations\ReportTemplates", true, "ReportTemplates", PropertyNameForFileName: nameof(ReportTemplate.Name));

            SR.AddItemInfo<DataSourceBase>("*.Ginger.DataSource.xml", @"~\DataSources", true, "Data Sources", PropertyNameForFileName: nameof(DataSourceBase.Name));

            SR.AddItemInfo<PluginPackage>("*.Ginger.PluginPackage.xml", @"~\Plugins", true, "Plugins", PropertyNameForFileName: nameof(PluginPackage.PluginId));

            SR.AddItemInfo<ActivitiesGroup>("*.Ginger.ActivitiesGroup.xml", @"~\SharedRepository\ActivitiesGroup", true, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups, "Shared "), PropertyNameForFileName: nameof(ActivitiesGroup.Name));
            SR.AddItemInfo<Activity>("*.Ginger.Activity.xml", @"~\SharedRepository\Activities", true, GingerDicser.GetTermResValue(eTermResKey.Activities, "Shared "), PropertyNameForFileName: nameof(Activity.ActivityName));
            SR.AddItemInfo<Act>("*.Ginger.Action.xml", @"~\SharedRepository\Actions", true, "Shared Actions", PropertyNameForFileName: nameof(Act.Description));
            SR.AddItemInfo<VariableBase>("*.Ginger.Variable.xml", @"~\SharedRepository\Variables", true, GingerDicser.GetTermResValue(eTermResKey.Variables, "Shared "), PropertyNameForFileName: nameof(VariableBase.Name));

            SR.AddItemInfo<RunSetConfig>("*.Ginger.RunSetConfig.xml", @"~\RunSetConfigs", true, GingerDicser.GetTermResValue(eTermResKey.RunSets), PropertyNameForFileName: nameof(RunSetConfig.Name));

            return SR;
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
            Reporter.ToLog(eAppReporterLogLevel.ERROR, ex.ToString(), ex);

            //add to dictionary to make sure same exception won't show more than 3 times
            if (_exceptionsDic.ContainsKey(ex.Message))
                _exceptionsDic[ex.Message]++;
            else
                _exceptionsDic.Add(ex.Message, 1);

            if (_exceptionsDic[ex.Message] <= 3)
            {
                Ginger.GeneralLib.ExceptionDetailsPage.ShowError(ex);
            }

            // Clear the err so it will not crash
            e.Handled = true;
        }

        public static BusinessFlow SetDefaultBusinessFlow()
        {
            BusinessFlow defualtBF;

            ObservableList<BusinessFlow> allBizFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();
            if (allBizFlows.Count > 0)
            {
                defualtBF = allBizFlows[0];
            }
            else
            {
                defualtBF = CreateNewBizFlow(GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " 1");
                WorkSpace.Instance.SolutionRepository.AddRepositoryItem(defualtBF);
            }

            defualtBF.SaveBackup();
            App.BusinessFlow = defualtBF;
            return defualtBF;
        }

        public static BusinessFlow CreateNewBizFlow(string Name)
        {

            BusinessFlow biz = new BusinessFlow();
            biz.Name = Name;
            biz.Activities = new ObservableList<Activity>();
            biz.Variables = new ObservableList<VariableBase>();
            // Set the new BF to be same like main app
            if (App.UserProfile.Solution.MainApplication != null)
            {
                biz.TargetApplications.Add(new TargetApplication() { AppName = App.UserProfile.Solution.MainApplication });
            }

            Activity a = new Activity() { Active = true };
            a.ActivityName = GingerDicser.GetTermResValue(eTermResKey.Activity) + " 1";
            a.Acts = new ObservableList<Act>();
            if (biz.TargetApplications.Count > 0)
            {
                a.TargetApplication = biz.TargetApplications[0].Name;
            }                
            biz.Activities.Add(a);

            biz.Activities.CurrentItem = a;
            biz.CurrentActivity = a;
            return biz;
        }






        internal static void CheckIn(string Path)
        {
            CheckInPage CIW = new CheckInPage(Path);
            CIW.ShowAsWindow();
        }

        internal static string GetProjEnvironmentName()
        {
            if (AutomateTabEnvironment != null)
                return App.AutomateTabEnvironment.Name;
            else
                return null;
        }

        public static void UpdateApplicationsAgentsMapping(bool useAgentsCache = true)
        {
            if (App.UserProfile.Solution != null && App.BusinessFlow != null)
            {
                //First we check if biz flow have target apps if not add one based on solution, fast convert for old or deleted
                if (App.BusinessFlow.TargetApplications.Count() == 0)
                {
                    if (string.IsNullOrEmpty(App.UserProfile.Solution.MainApplication))
                    {
                        Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "You must have at least one Target Application configured, please set it up.");
                        return;
                    }
                    else
                    {
                        // take it from solution main platform
                        if (App.BusinessFlow.TargetApplications == null)
                            App.BusinessFlow.TargetApplications = new ObservableList<TargetBase>();

                        App.BusinessFlow.TargetApplications.Add(new TargetApplication() { AppName = App.UserProfile.Solution.MainApplication });
                    }
                }
            }

            if (App.UserProfile.Solution != null)
                App.AutomateTabGingerRunner.SolutionAgents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>();
            else
                App.AutomateTabGingerRunner.SolutionAgents = null;
            App.AutomateTabGingerRunner.UpdateApplicationAgents();
        }

        //public static void AddItemToSaveAll(RepositoryItemBase itemToSave =null)
        //{
        //    if (itemToSave == null)
        //        if (CurrentRepositoryItem != null && CurrentSelectedTreeItem != null)
        //            itemToSave = App.CurrentRepositoryItem;

        //    if (itemToSave != null)
        //        try
        //        {
        //            if (App.ItemstoSave.Where(x => x.Guid == itemToSave.Guid).FirstOrDefault() == null)
        //            {
        //                BackupAndSaveItem(itemToSave);
        //            }
        //            else
        //            {
        //                var itemToRemove = App.ItemstoSave.SingleOrDefault(x => x.Guid == itemToSave.Guid);
        //                if (itemToRemove != null)
        //                {
        //                    App.ItemstoSave.Remove(itemToRemove);
        //                    BackupAndSaveItem(itemToSave);
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Reporter.ToLog(eLogLevel.ERROR, "Failed to add the item '" + CurrentRepositoryItem.FileName + "' to Save All list after it was selected on the tree", ex);
        //        }
        //}

        //private static void BackupAndSaveItem(RepositoryItemBase itemToSave)
        //{
        //    if (itemToSave.IsDirty == false)
        //        itemToSave.SaveBackup();
        //    App.ItemstoSave.Add(itemToSave);
        //}

        private static void BetaFeatureChanged(object sender, PropertyChangedEventArgs e)
        {
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


        public static void CloseSolution()
        {
            App.UserProfile.Solution = null;
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

        private static void App_AutomateBusinessFlowEvent(AutomateEventArgs args)
        {
            if (args.EventType == AutomateEventArgs.eEventType.Automate)
            {
                App.BusinessFlow = (BusinessFlow)args.Object;
                App.BusinessFlow.SaveBackup();
            }
        }

        

    }
}
