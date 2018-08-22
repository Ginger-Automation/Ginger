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
using Ginger.Environments;
using Ginger.Extensions;
using Ginger.Reports;
using Ginger.Repository;
using Ginger.Run;
using Ginger.SolutionWindows;
using Ginger.SourceControl;
using GingerCore;
using GingerCore.DataSource;
using GingerCore.Environments;
using GingerCore.Platforms;
using GingerCore.Repository;
using GingerCore.Repository.UpgradeLib;
using GingerCore.SourceControl;
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

        public  static Ginger.Functionalties.SolutionAutoSave AppSolutionAutoSave = new Ginger.Functionalties.SolutionAutoSave();

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
        public new static MainWindow MainWindow {get; set;}
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

        public static ObservableList<RepositoryItemBase> ItemstoSave = new ObservableList<RepositoryItemBase>();

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

        public static LocalRepository LocalRepository { get; set; } 
        
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
                    if (App.UserProfile.UserTypeHelper.IsSupportAutomate)
                        App.MainWindow.AutomateRibbon.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    App.MainWindow.AutomateRibbon.Visibility = System.Windows.Visibility.Collapsed;
                }

                App.AutomateTabGingerRunner.BusinessFlows.Clear();
                if (App.BusinessFlow != null)                
                    App.AutomateTabGingerRunner.BusinessFlows.Add(App.BusinessFlow);
                App.AutomateTabGingerRunner.CurrentBusinessFlow = App.BusinessFlow;
                
                UpdateApplicationsAgentsMapping();

                OnPropertyChanged("BusinessFlow");
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
        
        public static bool RunningFromConfigFile= false;
        
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
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(StanndAloneThreadsExceptionHandler);

            if (Environment.GetCommandLineArgs().Count() > 1)
            {
                RunningFromConfigFile = true;
                Reporter.CurrentAppLogLevel = eAppLogLevel.Debug;
                Reporter.AddAllReportingToConsole = true;//running from command line so show logs and messages also on Console (to be reviewd by Jenkins console and others)               
            }

            string phase = string.Empty;
            
            WorkSpaceEventHandler WSEH = new WorkSpaceEventHandler();
            WorkSpace.Init(WSEH);

            WorkSpace.Instance.BetaFeatures = BetaFeatures.LoadUserPref();
            WorkSpace.Instance.BetaFeatures.PropertyChanged += BetaFeatureChanged;


            if (WorkSpace.Instance.BetaFeatures.ShowDebugConsole)
            {
                DebugConsoleWindow.Show();
                WorkSpace.Instance.BetaFeatures.DisplayStatus();
            }

            Reporter.ToLog(eLogLevel.INFO, "######################## Application version " + App.AppVersion + " Started ! ########################");

            Reporter.ToLog(eLogLevel.INFO, "Loading the application progress bar");
            AppSplashWindow.LoadingInfo("InitApp");
            App.AppProgressBar = new AppProgressBar();

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
            UserProfile.SetRecentSolutionsObjects();

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

            Reporter.ToLog(eLogLevel.INFO, "Creating the Local Repository object");
            App.LocalRepository = new LocalRepository();
            
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

            App.MainWindow.lblStatus.Content = "Ready";
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

        private static void StanndAloneThreadsExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
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
            if (bDone) return;
            bDone = true;
            NewRepositorySerializer.NewRepositorySerializerEvent += RepositorySerializer.NewRepositorySerializer_NewRepositorySerializerEvent;
            RepositoryItemBase.InitSerializers(new RepositorySerializer());

            // Add all RI classes from GingerCoreCommon
            NewRepositorySerializer.AddClassesFromAssembly(typeof(RepositoryItemBase).Assembly);

            // Add all RI classes from GingerCore
            NewRepositorySerializer.AddClassesFromAssembly(typeof(GingerCore.RepositoryItem).Assembly);

            // add  old Plugins - TODO: remove later when we change to new plugins
            NewRepositorySerializer.AddClassesFromAssembly(typeof(GingerPlugIns.ActionsLib.PlugInActionsBase).Assembly);


            // Each class which moved from GingerCore to GingerCoreCommon needed to be added here, so it will auto translate
            Dictionary<string, Type> list = new Dictionary<string, Type>();
            list.Add("GingerCore.Actions.ActInputValue", typeof(ActInputValue));
            list.Add("GingerCore.Actions.ActReturnValue", typeof(ActReturnValue));
            list.Add("GingerCore.Actions.EnhancedActInputValue", typeof(EnhancedActInputValue));
            list.Add("GingerCore.Environments.GeneralParam", typeof(GeneralParam));


            // Verify the old name used in XML
            //list.Add("GingerCore.Actions.RepositoryItemTag", typeof(RepositoryItemTag));
            //list.Add("GingerCore.Actions.EnhancedActInputValue", typeof(EnhancedActInputValue));

            // TODO: chage to SR2  if we want the files to be loaded convert and save with the new SR2

            //if (WorkSpace.Instance.BetaFeatures.UseNewRepositorySerializer)
            //{
            //RepositorySerializer2 RS2 = new RepositorySerializer2();
            //    //RS2.AddLazyLoadAttr("Activities"); // TODO: add RI type
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
            App.LocalRepository.UpdateAppProgressBar = false;//because going to be executed by diffrent thread

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

        public static void FillComboFromEnumVal(ComboBox comboBox, Object EnumValue, List<object> values = null, bool sortValues=true, ListCollectionView valuesCollView = null)
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
            else if(App.UserProfile.SourceControlType == SourceControlBase.eSourceControlType.GIT && Directory.Exists(PathHelper.GetLongPath(sol.LocalFolder + @"\.git")))
                sol.ExistInLocaly = true;
            else
                sol.ExistInLocaly = false;
            sol.SourceControlLocation = SolutionFolder.Substring(SolutionFolder.LastIndexOf("\\")+1);

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

        public static bool SetSolution(string SolutionFolder)
        {
            //clear existing solution data
            try
            {
                AppSolutionAutoSave.SolutionAutoSaveEnd();
                App.UserProfile.Solution = null;
                //clear exsiting solution data- TODO: catch the solution value change event and if null then clear all below in relevant class/places
                App.LocalRepository.ClearAllCache();
                App.AutomateTabGingerRunner.ClearAgents();
                App.BusinessFlow = null;
                App.MainWindow.ResetSolutionDependedUIElements(false);
                AutoLogProxy.SetAccount("");
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
                        if (App.RunningFromConfigFile == false)
                        {
                            UpgradePage gingerUpgradePage = new UpgradePage(SolutionUpgradePageViewMode.UpgradeGinger, SolutionFolder, string.Empty, higherVersionFiles.ToList());
                            gingerUpgradePage.ShowAsWindow();
                        }
                        Reporter.ToLog(eLogLevel.WARN, "Ginger upgrade is needed for loading the Solution, aborting Solution load.");
                        return false;
                    }

                    Solution sol = (Solution)RepositoryItem.LoadFromFile(typeof(Solution), SolFile);

                    if (sol != null)
                    {
                        sol.Folder = SolutionFolder;
                        ValueExpression.SolutionFolder = SolutionFolder;

                        //Offer to upgrade Solution items to current version
                        if (App.UserProfile.DoNotAskToUpgradeSolutions == false && App.RunningFromConfigFile == false)
                        {
                            //TODO: think if it safe to use Async upgrade offer while already started to load the solution

                            ConcurrentBag<string> lowerVersionFiles = SolutionUpgrade.GetSolutionFilesCreatedWithRequiredGingerVersion(solutionFiles, SolutionUpgrade.eGingerVersionComparisonResult.LowerVersion);
                            if (lowerVersionFiles != null && lowerVersionFiles.Count > 0)
                            {
                                UpgradePage solutionUpgradePage = new UpgradePage(SolutionUpgradePageViewMode.UpgradeSolution, sol.Folder, sol.Name, lowerVersionFiles.ToList());
                                solutionUpgradePage.ShowAsWindow();
                            }
                        }

                        string RepositoryRootFolder = string.Empty;
                        switch (SourceControlIntegration.CheckForSolutionSourceControlType(SolutionFolder, ref RepositoryRootFolder))
                        {
                            case SourceControlBase.eSourceControlType.GIT:
                                {
                                    sol.SourceControl = new GITSourceControl();
                                }
                                break;
                            case SourceControlBase.eSourceControlType.SVN:
                                {
                                    sol.SourceControl = new SVNSourceControl();
                                }
                                break;
                        }
                        if (sol.SourceControl != null)
                        {
                            if (string.IsNullOrEmpty(App.UserProfile.SolutionSourceControlUser) || string.IsNullOrEmpty(App.UserProfile.SolutionSourceControlPass))
                            {
                                if (App.UserProfile.SourceControlUser != null && App.UserProfile.SourceControlPass != null)
                                {
                                    sol.SourceControl.SourceControlUser = App.UserProfile.SourceControlUser;
                                    sol.SourceControl.SourceControlPass = App.UserProfile.SourceControlPass;

                                    sol.SourceControl.SolutionSourceControlAuthorEmail = App.UserProfile.SolutionSourceControlAuthorEmail;
                                    sol.SourceControl.SolutionSourceControlAuthorName = App.UserProfile.SolutionSourceControlAuthorName;
                                }
                            }
                            else
                            {
                                sol.SourceControl.SourceControlUser = App.UserProfile.SolutionSourceControlUser;
                                sol.SourceControl.SourceControlPass = App.UserProfile.SolutionSourceControlPass;

                                sol.SourceControl.SolutionSourceControlAuthorEmail = App.UserProfile.SolutionSourceControlAuthorEmail;
                                sol.SourceControl.SolutionSourceControlAuthorName = App.UserProfile.SolutionSourceControlAuthorName;
                            }

                            string error = string.Empty;
                            sol.SourceControl.SolutionFolder = SolutionFolder;
                            sol.SourceControl.RepositoryRootFolder = RepositoryRootFolder;
                            sol.SourceControl.SourceControlURL = sol.SourceControl.GetRepositoryURL(ref error);
                            sol.SourceControl.SourceControlLocalFolder = App.UserProfile.SourceControlLocalFolder;

                            sol.SourceControl.SourceControlProxyAddress = App.UserProfile.SolutionSourceControlProxyAddress;
                            sol.SourceControl.SourceControlProxyPort = App.UserProfile.SolutionSourceControlProxyPort;

                            MainWindow.CheckInSolutionBtn.Visibility = Visibility.Visible;
                            MainWindow.GetLatestSolutionBtn.Visibility = Visibility.Visible;
                            MainWindow.ResolveConflictsBtn.Visibility = Visibility.Visible;
                            MainWindow.ConnectionDetailsBtn.Visibility = Visibility.Visible;
                            MainWindow.RepositoryDetailsBtn.Visibility = Visibility.Visible;
                            MainWindow.SourceControlSolutioRibbonGroup.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            MainWindow.CheckInSolutionBtn.Visibility = Visibility.Collapsed;
                            MainWindow.GetLatestSolutionBtn.Visibility = Visibility.Collapsed;
                            MainWindow.ResolveConflictsBtn.Visibility = Visibility.Collapsed;
                            MainWindow.ConnectionDetailsBtn.Visibility = Visibility.Collapsed;
                            MainWindow.SourceControlSolutioRibbonGroup.Visibility = Visibility.Collapsed;
                        }

                        


                        //adding GingerCoreNET SolutionRepository                        
                        WorkSpace.Instance.SolutionRepository = CreateGingerSolutionRepository();
                        WorkSpace.Instance.SolutionRepository.Open(SolutionFolder);

                        WorkSpace.Instance.SourceControl = sol.SourceControl;
                        RepositoryItemBase.SourceControl = sol.SourceControl;

                        App.UserProfile.Solution = sol;
                        

                        SetUserTypeButtons();
                        App.UserProfile.LoadRecentAppAgentMapping();
                        App.AutomateTabGingerRunner.SolutionFolder = SolutionFolder;
                        App.AutomateTabGingerRunner.SolutionAgents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>();
                        // App.AutomateTabGingerRunner.PlugInsList = App.LocalRepository.GetSolutionPlugIns();
                        App.UserProfile.Solution.SetReportsConfigurations();
                        App.AutomateTabGingerRunner.SolutionApplications = App.UserProfile.Solution.ApplicationPlatforms;
                        App.AutomateTabGingerRunner.DSList = App.LocalRepository.GetSolutionDataSources();


                        BindEnvsCombo();

                        if (App.MainWindow.MainRibbonSelectedTab != "Solution")
                            App.MainWindow.MainRibbonSelectedTab = "Solution";
                        App.MainWindow.RefreshSolutionTabRibbon();
                        App.MainWindow.ResetSolutionDependedUIElements(true);

                        AutoLogProxy.SetAccount(sol.Account);
                        BusinessFlow.SolutionVariables = sol.Variables;
                        App.AutomateTabGingerRunner.CurrentSolution = sol;
                        LoadRecentBusinessFlow();
                        if (!App.RunningFromConfigFile)
                            DoSolutionAutoSaveAndRecover();
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

        private static void BindEnvsCombo()
        {
            ComboBox envsCombo = App.MainWindow.lstEnvs;

            envsCombo.ItemsSource = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().AsCollectionViewOrderBy(nameof(ProjEnvironment.Name));
            envsCombo.DisplayMemberPath = nameof(ProjEnvironment.Name);
            envsCombo.SelectedValuePath = nameof(ProjEnvironment.Guid);


            if (UserProfile.Solution != null)
            {
                //select last used environment
                if (envsCombo.Items != null && envsCombo.Items.Count > 0)
                {
                    if (envsCombo.Items.Count > 1 && App.UserProfile.RecentEnvironment != null && App.UserProfile.RecentEnvironment != Guid.Empty)
                    {
                        foreach (object env in envsCombo.Items)
                        {
                            if (((ProjEnvironment)env).Guid == App.UserProfile.RecentEnvironment)
                            {
                                envsCombo.SelectedIndex = envsCombo.Items.IndexOf(env);
                                return;
                            }
                        }
                    }

                    //defualt selection
                    envsCombo.SelectedIndex = 0;
                }
            }

            //move to top after bind
            if (envsCombo.Items.Count == 0)
            {
                CreateDefaultEnvironment();
                envsCombo.SelectedItem = envsCombo.Items[0];
            }
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

            ////// Note the | which enable to define multiple pattern for same folder
            ////// Shared repository can contains Activities and Actions            
            //SR.AddItemInfo<Activity>("*.Ginger.Activity.xml", @"~\SharedRepository\Activities", true, @"Shared Repository\Activites", addToRootFolders: false);
            //SR.AddItemInfo<Act>("*.Ginger.Action.xml", @"~\SharedRepository\Actions", true, @"Shared Repository\Actions", addToRootFolders: false);
            //mSolutionRootFolders.Add(new RepositoryFolder<object>(SR, null, "*.Ginger.Activity.xml|*.Ginger.Action.xml", @"~\SharedRepository", true, "Shared Repository"));

            ////SR.AddItemInfo<RunSetConfig>("*.Ginger.RunSetConfig.xml", @"~\RunSetConfigs", true, "Run Set Configs", addToRootFolders: true);
            ////SR.AddItemInfo<HTMLReportConfiguration>("*.Ginger.HTMLReportConfiguration.xml", @"~\HTMLReportConfigurations", true, "HTML Report Configurations", addToRootFolders: true);

            //mSolutionRootFolders.Add(new RepositoryFolder<object>(SR, null, "*.Ginger.PluginPackage.xml|*.Ginger.ApplicationAPIModel.xml", @"~\Resources", true, "Resources"));
            //SR.AddItemInfo<PluginPackage>("*.Ginger.PluginPackage.xml", @"~\PluginPackages", true, "Plugin Packages", addToRootFolders: false);

            ////// Docs do not contains repo items
            //mRootFolders.Add(new RepositoryFolder(this, "*", @"~\Documents";, false, "Documents"));

            return SR;
        }






        private static void SetUserTypeButtons()
        {
            if (App.UserProfile.UserType == eUserType.Business && App.UserProfile.Solution != null)
            {
                MainWindow.SolutionGherkin.Visibility = Visibility.Visible;
                MainWindow.ImportFeatureFile.Visibility = Visibility.Visible;
                MainWindow.CreateFeatureFile.Visibility = Visibility.Visible;
            }
        }
        
        private static void LoadRecentBusinessFlow()
        {
            try
            {
                if (App.UserProfile.Solution == null) return;
                ObservableList<BusinessFlow> allBizFlows = App.LocalRepository.GetSolutionBusinessFlows();

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
               App.BusinessFlow= LoadDefaultBusinessFlow();
                
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
            BusinessFlow biz = LocalRepository.CreateNewBizFlow( GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) +" 1");            
            WorkSpace.Instance.SolutionRepository.AddRepositoryItem(biz);            
            return biz;
        }

        public static void CreateDefaultEnvironment()
        {
            ProjEnvironment newEnv = new ProjEnvironment() { Name = "Default" };
            WorkSpace.Instance.SolutionRepository.AddRepositoryItem(newEnv);

            // Add all solution target app
            foreach (ApplicationPlatform AP in App.UserProfile.Solution.ApplicationPlatforms)
            {
                EnvApplication EA = new EnvApplication();
                EA.Name = AP.AppName;
                EA.CoreProductName = AP.Core;
                EA.CoreVersion = AP.CoreVersion;
                EA.Active = true;
                newEnv.Applications.Add(EA);
            }
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

        public static void UpdateApplicationsAgentsMapping(bool useAgentsCache=true)
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

        public static void AddItemToSaveAll(RepositoryItemBase itemToSave =null)
        {
            if (itemToSave == null)
                if (CurrentRepositoryItem != null && CurrentSelectedTreeItem != null)
                    itemToSave = App.CurrentRepositoryItem;

            if (itemToSave != null)
                try
                {
                    //if (itemToSave is DataSourceTable)
                    //{
                    //    DataSourceBase DS = ((DataSourceTableTreeItem)CurrentSelectedTreeItem).DSDetails;
                    //    if (DS.DSC == null)
                    //    {
                    //        if (DS.FilePath.StartsWith("~"))
                    //        {
                    //            DS.FileFullPath = DS.FilePath.Replace("~", "");
                    //            DS.FileFullPath = App.UserProfile.Solution.Folder + DS.FileFullPath;
                    //        }
                    //        DS.Init(DS.FileFullPath);
                    //    }
                    //    itemToSave = DS;                                 
                    //}
                    //else if (itemToSave is EnvApplication)
                    //{
                    //    itemToSave = ((EnvApplicationTreeItem)CurrentSelectedTreeItem).ProjEnvironment; //adding the Env parent if not in list
                    //}

                    if (App.ItemstoSave.Where(x=>x.Guid == itemToSave.Guid).FirstOrDefault() == null)
                    {
                        if (itemToSave.IsDirty == false)
                            itemToSave.SaveBackup();
                        App.ItemstoSave.Add(itemToSave);
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, "Failed to add the item '" + CurrentRepositoryItem.FileName + "' to Save All list after it was selected on the tree", ex);
                }
        }

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

    }
}
