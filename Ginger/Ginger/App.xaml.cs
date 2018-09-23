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
                    else//Oficial Release
                    {
                        mAppVersion = string.Format("{0}.{1}", ApplicationInfo.ProductMajorPart, ApplicationInfo.ProductMinorPart);
                    }
                }

                return mAppVersion;
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

        //public static ObservableList<RepositoryItemBase> ItemstoSave = new ObservableList<RepositoryItemBase>();

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

        public static List<Page> PageList { get; set; }

        public static Page GetPage(Type PageType)
        {
            Page p = (from p1 in PageList where p1.GetType() == PageType select p1).FirstOrDefault();
            return p;
        }

        public static string LocalApplicationData
        {
            get
            {
                //TODO: check where it goes - not roaming,.,
                string s = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                s = s + @"\amdocs\" + App.AppName;
                return s;
            }
        }

        public static bool RunningFromConfigFile = false;

        public static bool RunningFromUnitTest = false;

        internal static void ObjFieldBinding(System.Windows.Controls.Control control, DependencyProperty dependencyProperty, object obj, string property, BindingMode BindingMode = BindingMode.TwoWay)
        {
            //TODO: add Inotify on the obj.attr - so code changes to property will be reflected
            //TODO: check perf impact + reuse exisitng binding on same obj.prop

            GingerCore.General.ObjFieldBinding(control, dependencyProperty, obj, property, BindingMode);
        }

        public static void LoadApplicationDictionaries(Amdocs.Ginger.Core.eSkinDicsType SkinDicType = Amdocs.Ginger.Core.eSkinDicsType.Default, Amdocs.Ginger.Core.eTerminologyDicsType TerminologyDicType = Amdocs.Ginger.Core.eTerminologyDicsType.Default)
        {
            //Clear all Dictionaries
            Application.Current.Resources.MergedDictionaries.Clear();

            //Load only relevant dictionaries for the application to use
            //Skins
            switch (SkinDicType)
            {
                case Amdocs.Ginger.Core.eSkinDicsType.Default:
                    Application.Current.Resources.MergedDictionaries.Add(
               new ResourceDictionary() { Source = new Uri("pack://application:,,,/Ginger;component/Dictionaries/Skins/GingerDefualtSkinDictionary.xaml") });
                    break;


                default:
                    Application.Current.Resources.MergedDictionaries.Add(
               new ResourceDictionary() { Source = new Uri("pack://application:,,,/Ginger;component/Dictionaries/Skins/GingerDefualtSkinDictionary.xaml") });
                    break;
            }


            //Terminologies
            switch (TerminologyDicType)
            {
                case Amdocs.Ginger.Core.eTerminologyDicsType.Testing:
                    Application.Current.Resources.MergedDictionaries.Add(
                                                new ResourceDictionary() { Source = new Uri("pack://application:,,,/Ginger;component/Dictionaries/Terminologies/GingerTestingTerminologyDictionary.xaml") });
                    break;

                case Amdocs.Ginger.Core.eTerminologyDicsType.Gherkin:
                    Application.Current.Resources.MergedDictionaries.Add(
                                                new ResourceDictionary() { Source = new Uri("pack://application:,,,/Ginger;component/Dictionaries/Terminologies/GherkinTerminologyDictionary.xaml") });
                    break;

                default:
                    Application.Current.Resources.MergedDictionaries.Add(
                                                new ResourceDictionary() { Source = new Uri("pack://application:,,,/Ginger;component/Dictionaries/Terminologies/GingerDefualtTerminologyDictionary.xaml") });
                    break;
            }
        }

        public static void InitApp()
        {
            // Add event handler for handling non-UI thread exceptions.
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(StandAloneThreadExceptionHandler);

            if (Environment.GetCommandLineArgs().Count() > 1)
            {
                // When running from unit test there are args, so we set a flag in GingerAutomator to make sure Ginger will Launh
                // and will not try to process the args for RunSet auto run
                if (RunningFromUnitTest)
                {
                    // do nothing for now, but later on we might want to process and check auto run too
                }
                else
                {
                    // This Ginger is running with run set config will do the run and close Ginger
                    RunningFromConfigFile = true;
                    Reporter.CurrentAppLogLevel = eAppLogLevel.Debug;
                    Reporter.AddAllReportingToConsole = true;//running from command line so show logs and messages also on Console (to be reviewd by Jenkins console and others)               
                }
            }

            string phase = string.Empty;

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

            Reporter.ToLog(eLogLevel.INFO, "######################## Application version " + App.AppVersion + " Started ! ########################");
           
            AppSplashWindow.LoadingInfo("Init Application");

            // We init the classed dictionary for the Repository Serialzier only once
            InitClassTypesDictionary();

            // TODO: need to add a switch what we get from old ginger based on magic key

            phase = "Loading User Profile";
            Reporter.ToLog(eLogLevel.INFO, phase);
            AppSplashWindow.LoadingInfo(phase);
            App.UserProfile = UserProfile.LoadUserProfile();

            phase = "Configuring User Type";
            Reporter.ToLogAndConsole(eLogLevel.INFO, phase);
            AppSplashWindow.LoadingInfo(phase);
            UserProfile.LoadUserTypeHelper();

            phase = "Loading User Selected Resource Dictionaries";
            Reporter.ToLog(eLogLevel.INFO, phase);
            AppSplashWindow.LoadingInfo(phase);
            if (App.UserProfile != null)
                LoadApplicationDictionaries(Amdocs.Ginger.Core.eSkinDicsType.Default, App.UserProfile.TerminologyDictionaryType);
            else
                LoadApplicationDictionaries(Amdocs.Ginger.Core.eSkinDicsType.Default, Amdocs.Ginger.Core.eTerminologyDicsType.Default);

            Reporter.ToLog(eLogLevel.INFO, "Loading user messages pool");
            UserMessagesPool.LoadUserMessgaesPool();
            GingerHelperMsgsPool.LoadGingerHelperMsgsPool();

            Reporter.ToLog(eLogLevel.INFO, "Init the Centralized Auto Log");
            AutoLogProxy.Init(App.AppVersion);

            Reporter.ToLog(eLogLevel.INFO, "Initializing the Source control");
            AppSplashWindow.LoadingInfo(phase);

            phase = "Loading the Main Window";
            Reporter.ToLog(eLogLevel.INFO, phase);
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
            Reporter.ToLog(eLogLevel.INFO, phase);
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
            Reporter.ToLog(eLogLevel.FATAL, ">>>>>>>>>>>>>> Error occured on stand alone thread(non UI) - " + e.ExceptionObject.ToString());
            MessageBox.Show("Error occurred on stand alone thread - " + e.ExceptionObject.ToString());
            App.AppSolutionAutoSave.DoAutoSave();

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

            // TODO: remove after we don't need old serialzier to load old repo items
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
            // For backword compatibility of loading old object name in xml
            Dictionary<string, Type> list = new Dictionary<string, Type>();
            list.Add("GingerCore.Actions.ActInputValue", typeof(ActInputValue));
            list.Add("GingerCore.Actions.ActReturnValue", typeof(ActReturnValue));
            list.Add("GingerCore.Actions.EnhancedActInputValue", typeof(EnhancedActInputValue));
            list.Add("GingerCore.Environments.GeneralParam", typeof(GeneralParam));
            //list.Add("GingerCore.BusinessFlow", typeof(BusinessFlow));

            // Put back for Lazy load of BF.Acitvities
            NewRepositorySerializer.AddLazyLoadAttr(nameof(BusinessFlow.Activities)); // TODO: add RI type, and use attr on field


            // Verify the old name used in XML
            //list.Add("GingerCore.Actions.RepositoryItemTag", typeof(RepositoryItemTag));
            //list.Add("GingerCore.Actions.EnhancedActInputValue", typeof(EnhancedActInputValue));

            // TODO: chage to SR2  if we want the files to be loaded convert and save with the new SR2

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
            Reporter.ToLog(eLogLevel.INFO, phase);
            Reporter.CurrentAppLogLevel = eAppLogLevel.Debug;
            AutoLogProxy.LogAppOpened();
            AppSplashWindow.LoadingInfo(phase);

            var result = await App.RunsetExecutor.RunRunSetFromCommandLine();

            Reporter.ToLog(eLogLevel.INFO, "Closing Ginger automatically...");
            App.MainWindow.CloseWithoutAsking();
            //TODO: find a way not to open Main window at all
            Reporter.ToLog(eLogLevel.INFO, "Ginger UI Closed.");

            //setting the exit code based on execution status
            if (result == 0)
            {
                Reporter.ToLog(eLogLevel.INFO, ">> Run Set executed and passed, exit code: 0");
                Environment.ExitCode = 0;//success                    
            }
            else
            {
                Reporter.ToLog(eLogLevel.INFO, ">> No indication found for successfull execution, exit code: 1");
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


        public static bool LoadingSolution;

        public static bool SetSolution(string SolutionFolder)
        {
            //clear existing solution data
            try
            {
                LoadingSolution = true;
                OnPropertyChanged(nameof(LoadingSolution));

                // Cleanup last loaded solution 
                //WorkSpace.Instance.LocalGingerGrid.Reset();  //Temp
                AppSolutionAutoSave.SolutionAutoSaveEnd();

                //Cleanup
                App.UserProfile.Solution = null;
                App.AutomateTabGingerRunner.ClearAgents();
                App.BusinessFlow = null;
                AutoLogProxy.SetAccount("");

                WorkSpace.Instance.SolutionRepository = null;
                WorkSpace.Instance.SourceControl = null;
                RepositoryItemBase.SourceControl = null;

                if (!SolutionFolder.EndsWith(@"\")) SolutionFolder += @"\";
                string SolFile = SolutionFolder + @"Ginger.Solution.xml";
                if (File.Exists(PathHelper.GetLongPath(SolFile)))
                {
                    //get Solution files
                    IEnumerable<string> solutionFiles = Solution.SolutionFiles(SolutionFolder);

                    //check if Ginger Upgrade is needed for loading this Solution
                    Reporter.ToLog(eLogLevel.INFO, "Checking if Ginger upgrade is needed for loading the Solution");
                    ConcurrentBag<string> higherVersionFiles = SolutionUpgrade.GetSolutionFilesCreatedWithRequiredGingerVersion(solutionFiles, SolutionUpgrade.eGingerVersionComparisonResult.HigherVersion);
                    if (higherVersionFiles.Count > 0)
                    {
                        if (App.RunningFromConfigFile == false && RunningFromUnitTest == false)
                        {
                            UpgradePage gingerUpgradePage = new UpgradePage(SolutionUpgradePageViewMode.UpgradeGinger, SolutionFolder, string.Empty, higherVersionFiles.ToList());
                            gingerUpgradePage.ShowAsWindow();
                        }
                        Reporter.ToLog(eLogLevel.WARN, "Ginger upgrade is needed for loading the Solution, aborting Solution load.");
                        return false;
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
                        
                        LoadRecentBusinessFlow();

                        if (!App.RunningFromConfigFile)
                            DoSolutionAutoSaveAndRecover();

                        //Offer to upgrade Solution items to current version
                        if (App.UserProfile.DoNotAskToUpgradeSolutions == false && App.RunningFromConfigFile == false && RunningFromUnitTest == false)
                        {                            
                            ConcurrentBag<string> lowerVersionFiles = SolutionUpgrade.GetSolutionFilesCreatedWithRequiredGingerVersion(solutionFiles, SolutionUpgrade.eGingerVersionComparisonResult.LowerVersion);
                            if (lowerVersionFiles != null && lowerVersionFiles.Count > 0)
                            {
                                UpgradePage solutionUpgradePage = new UpgradePage(SolutionUpgradePageViewMode.UpgradeSolution, sol.Folder, sol.Name, lowerVersionFiles.ToList());
                                solutionUpgradePage.ShowAsWindow();
                            }
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
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while loading the solution", ex);
                throw ex;
            }
            finally
            {
                LoadingSolution = false;
                OnPropertyChanged(nameof(LoadingSolution));
            }
        }

        private static void HandleSolutionLoadSourceControl(Solution solution)
        {
            string RepositoryRootFolder = string.Empty;
            switch (SourceControlIntegration.CheckForSolutionSourceControlType(solution.Folder, ref RepositoryRootFolder))
            {
                case SourceControlBase.eSourceControlType.GIT:
                    {
                        solution.SourceControl = new GITSourceControl();
                    }
                    break;
                case SourceControlBase.eSourceControlType.SVN:
                    {
                        solution.SourceControl = new SVNSourceControl();
                    }
                    break;
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

                WorkSpace.Instance.SourceControl = solution.SourceControl;
                RepositoryItemBase.SourceControl = solution.SourceControl;
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
            //SR.AddItemInfo<ApplicationPlatform>("*.Ginger.TargetApplication.xml", @"~\TargetApplications", true, "Target Applications", addToRootFolders: true, PropertyNameForFileName: nameof(ApplicationPlatform.AppName));
            SR.AddItemInfo<BusinessFlow>("*.Ginger.BusinessFlow.xml", @"~\BusinessFlows", true, "Business Flows", addToRootFolders: true, PropertyNameForFileName: nameof(BusinessFlow.Name));

            SR.AddItemInfo<ApplicationAPIModel>("*.Ginger.ApplicationAPIModel.xml", @"~\Applications Models\API Models", true, "API Models", addToRootFolders: true, PropertyNameForFileName: nameof(ApplicationAPIModel.Name));
            SR.AddItemInfo<GlobalAppModelParameter>("*.Ginger.GlobalAppModelParameter.xml", @"~\Applications Models\Global Models Parameters", true, "Global Model Parameters", addToRootFolders: true, PropertyNameForFileName: nameof(GlobalAppModelParameter.PlaceHolder));
            SR.AddItemInfo<ApplicationPOMModel>("*.Ginger.ApplicationPOMModel.xml", @"~\Applications Models\POM Models", true, "POM Models", addToRootFolders: false, PropertyNameForFileName: nameof(ApplicationPOMModel.Name));


            SR.AddItemInfo<ProjEnvironment>("*.Ginger.Environment.xml", @"~\Environments", true, "Environments", addToRootFolders: true, PropertyNameForFileName: nameof(ProjEnvironment.Name));
            SR.AddItemInfo<ALMDefectProfile>("*.Ginger.ALMDefectProfile.xml", @"~\ALMDefectProfiles", true, "ALM Defect Profiles", addToRootFolders: true, PropertyNameForFileName: nameof(ALMDefectProfile.Name));

            SR.AddItemInfo<Agent>("*.Ginger.Agent.xml", @"~\Agents", true, "Agents", addToRootFolders: true, PropertyNameForFileName: nameof(Agent.Name));


            SR.AddItemInfo<HTMLReportConfiguration>("*.Ginger.HTMLReportConfiguration.xml", @"~\HTMLReportConfigurations", true, "HTMLReportConfigurations", addToRootFolders: true, PropertyNameForFileName: nameof(HTMLReportsConfiguration.Name));
            SR.AddItemInfo<HTMLReportTemplate>("*.Ginger.HTMLReportTemplate.xml", @"~\HTMLReportConfigurations\HTMLReportTemplate", true, "HTMLReportTemplate", addToRootFolders: true, PropertyNameForFileName: nameof(HTMLReportTemplate.Name));
            SR.AddItemInfo<ReportTemplate>("*.Ginger.ReportTemplate.xml", @"~\HTMLReportConfigurations\ReportTemplates", true, "ReportTemplates", addToRootFolders: true, PropertyNameForFileName: nameof(ReportTemplate.Name));

            SR.AddItemInfo<DataSourceBase>("*.Ginger.DataSource.xml", @"~\DataSources", true, "DataSources", addToRootFolders: true, PropertyNameForFileName: nameof(DataSourceBase.Name));

            SR.AddItemInfo<PluginPackage>("*.Ginger.PluginPackage.xml", @"~\Plugins", true, "Plugins", addToRootFolders: true, PropertyNameForFileName: nameof(PluginPackage.PluginID));

            SR.AddItemInfo<Activity>("*.Ginger.Activity.xml", @"~\SharedRepository\Activities", true, "Shared Activities", addToRootFolders: false, PropertyNameForFileName: nameof(Activity.ActivityName));
            SR.AddItemInfo<Act>("*.Ginger.Action.xml", @"~\SharedRepository\Actions", true, "Shared Actions", addToRootFolders: false, PropertyNameForFileName: nameof(Act.Description));

            SR.AddItemInfo<VariableBase>("*.Ginger.Variable.xml", @"~\SharedRepository\Variables", true, "Shared Variables", addToRootFolders: false, PropertyNameForFileName: nameof(VariableBase.Name));

            ////SR.AddItemInfo<RunSetConfig>("*.Ginger.RunSetConfig.xml", @"~\RunSetConfigs", true, "Run Set Configs", addToRootFolders: true);, PropertyNameForFileName: nameof(VariableBase.Name));

            SR.AddItemInfo<RunSetConfig>("*.Ginger.RunSetConfig.xml", @"~\RunSetConfigs", true, "Run Set Configs", addToRootFolders: true, PropertyNameForFileName: nameof(RunSetConfig.Name));
            SR.AddItemInfo<ActivitiesGroup>("*.Ginger.ActivitiesGroup.xml", @"~\SharedRepository\ActivitiesGroup", true, "Shared Activities Group", addToRootFolders: false, PropertyNameForFileName: nameof(ActivitiesGroup.Name));
            ////// Note the | which enable to define multiple pattern for same folder
            ////// Shared repository can contains Activities and Actions            
            //SR.AddItemInfo<Activity>("*.Ginger.Activity.xml", @"~\SharedRepository\Activities", true, @"Shared Repository\Activites", addToRootFolders: false);
            //SR.AddItemInfo<Act>("*.Ginger.Action.xml", @"~\SharedRepository\Actions", true, @"Shared Repository\Actions", addToRootFolders: false);
            //mSolutionRootFolders.Add(new RepositoryFolder<object>(SR, null, "*.Ginger.Activity.xml|*.Ginger.Action.xml", @"~\SharedRepository", true, "Shared Repository"));

            ////SR.AddItemInfo<RunSetConfig>("*.Ginger.RunSetConfig.xml", @"~\RunSetConfigs", true, "Run Set Configs", addToRootFolders: true);g

            //mSolutionRootFolders.Add(new RepositoryFolder<object>(SR, null, "*.Ginger.PluginPackage.xml|*.Ginger.ApplicationAPIModel.xml", @"~\Resources", true, "Resources"));
            //SR.AddItemInfo<PluginPackage>("*.Ginger.PluginPackage.xml", @"~\PluginPackages", true, "Plugin Packages", addToRootFolders: false);

            ////// Docs do not contains repo items
            //mRootFolders.Add(new RepositoryFolder(this, "*", @"~\Documents";, false, "Documents"));

            return SR;
        }
        
        private static void LoadRecentBusinessFlow()
        {
            try
            {
                if (App.UserProfile.Solution == null) return;
                ObservableList<BusinessFlow> allBizFlows = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<BusinessFlow>();

                if (App.UserProfile.RecentBusinessFlow != null &&
                            App.UserProfile.RecentBusinessFlow != Guid.Empty)
                {
                    BusinessFlow recentBizFlow = allBizFlows.Where(biz => biz.Guid == App.UserProfile.RecentBusinessFlow).FirstOrDefault();
                    if (recentBizFlow != null)
                    {
                        recentBizFlow.SaveBackup();
                        App.BusinessFlow = recentBizFlow;
                        return;
                    }
                }

                if (allBizFlows.Count > 0)
                {
                    allBizFlows[0].SaveBackup();
                    App.BusinessFlow = allBizFlows[0];
                    return;
                }

                //load new Business Flow as default
                App.BusinessFlow = LoadDefaultBusinessFlow();

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to load the recent " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " used", ex);
                LoadDefaultBusinessFlow();
            }
        }

        //public static void UpdateEnvironmentsCombo(ComboBox envsCombo)
        //{
        //    envsCombo.ItemsSource = null;

        //    if (UserProfile.Solution != null)
        //    {
        //        envsCombo.ItemsSource = App.LocalRepository.GetSolutionEnvironments();
        //        envsCombo.DisplayMemberPath = ProjEnvironment.Fields.Name;
        //        envsCombo.SelectedValuePath = RepositoryItem.Fields.Guid;

        //        //select last used environment
        //        if (envsCombo.Items != null && envsCombo.Items.Count > 0)
        //        {
        //            if (envsCombo.Items.Count > 1 && App.UserProfile.RecentEnvironment != null && App.UserProfile.RecentEnvironment != Guid.Empty)
        //            {
        //                foreach (object env in envsCombo.Items)
        //                {
        //                    if (((ProjEnvironment)env).Guid == App.UserProfile.RecentEnvironment)
        //                    {
        //                        envsCombo.SelectedIndex = envsCombo.Items.IndexOf(env);
        //                        return;
        //                    }
        //                }
        //            }

        //            //defualt selection
        //            envsCombo.SelectedIndex = 0;
        //        }
        //    }
        //    if (envsCombo.Items.Count == 0)
        //    {
        //        LoadDefaultENV();
        //        UpdateEnvironmentsCombo(envsCombo);
        //    }
        //}

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

            //add to dictenery to make sure same excption won't show more than 3 times
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

        public static BusinessFlow LoadDefaultBusinessFlow()
        {
            BusinessFlow biz = CreateNewBizFlow(GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " 1");
            WorkSpace.Instance.SolutionRepository.AddRepositoryItem(biz);
            return biz;
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
                a.TargetApplication = biz.TargetApplications[0].AppName;
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
                        Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "You must have at least one Targent Application configured, please set it up.");
                        return;
                    }
                    else
                    {
                        // take it from solution main platform
                        if (App.BusinessFlow.TargetApplications == null)
                            App.BusinessFlow.TargetApplications = new ObservableList<TargetApplication>();

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
            //if (e.PropertyName == nameof(BetaFeatures.BFUseSolutionRepositry))
            //{
            //    // is this the only item to refresh?
            //    MainWindow.RefreshSolutionPage();
            //}

        }

        internal static void ErrorLogged(int errorsCounter)
        {
            MainWindow.ErrorsLabel.Visibility = Visibility.Visible;
            MainWindow.ErrorsLabel.Content = "Errors (" + errorsCounter + ")";
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
