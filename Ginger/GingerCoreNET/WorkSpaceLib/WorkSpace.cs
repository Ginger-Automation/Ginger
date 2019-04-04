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

using Amdocs.Ginger;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.CoreNET.RosLynLib.Refrences;
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.Functionalties;
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCoreNET.RunLib;
using GingerCoreNET.SourceControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;

namespace amdocs.ginger.GingerCoreNET
{
    // WorkSpace is one object per user accessible from anywhere and hold the current status of the user selection
    // For Ginger.Exe it is one per running app
    // For Web it can be one per user connected
    // DO NOT ADD STATIC FIELDS
    public class WorkSpace : RepositoryItemBase
    {
        private static WorkSpace mWorkSpace;
        public static WorkSpace Instance { get { return mWorkSpace; } }

        public static void Init(IWorkSpaceEventHandler WSEH)
        {
            mWorkSpace = new WorkSpace();
            mWorkSpace.EventHandler = WSEH;
            mWorkSpace.InitClassTypesDictionary();
        }

        public SolutionRepository SolutionRepository;

        public SourceControlBase SourceControl;

        public ApplicationInfo ApplicationInfo = new ApplicationInfo();
        
        /// <summary>
        /// Hold all Run Set execution data + execution methods
        /// </summary>    
        public RunsetExecutor RunsetExecutor = new RunsetExecutor();


        private Solution mSolution;
        public Solution Solution
        {
            get
            {
                return mSolution;
            }
            set
            {
                mSolution = value;
                OnPropertyChanged(nameof(Solution));
            }
        }

        public eRunStatus RunSetExecutionStatus = eRunStatus.Failed;
        
        public static string EmailReportTempFolder
        {
            get
            {
                return System.IO.Path.GetDirectoryName(System.IO.Path.GetTempFileName()) + "\\Ginger_Email_Reports";
            }
        }

        PluginsManager mPluginsManager = null;
        public PluginsManager PlugInsManager
        {
            get
            {
                if (mPluginsManager == null)
                {
                    mPluginsManager = new PluginsManager(SolutionRepository);
                }
                return mPluginsManager;
            }
        }

        static bool bDone = false;

        /// <summary>
        /// Init core classes type dictionary for RepositorySerializer
        /// </summary>
        void InitClassTypesDictionary()
        {
            if (bDone) return;
            bDone = true;

            // Add all RI classes from GingerCoreCommon
            NewRepositorySerializer.AddClassesFromAssembly(typeof(RepositoryItemBase).Assembly);

            // Add gingerCoreNET classes                        
            NewRepositorySerializer.AddClassesFromAssembly(typeof(RunSetConfig).Assembly);

            //Dictionary<string, Type> list = new Dictionary<string, Type>();


            // !!!!!!!!!! Cleanup
            //AddClass(list, typeof(RunSetConfig));
            //AddClass(list, typeof(RunSetActionSendEmail));
            //AddClass(list, typeof(BusinessFlowReport));
            //AddClass(list, typeof(HTMLReportConfiguration));
            //AddClass(list, typeof(HTMLReportConfigFieldToSelect));
            //AddClass(list, typeof(Agent));
            //AddClass(list, typeof(DriverConfigParam));
            //AddClass(list, typeof(GingerRunner));
            //AddClass(list, typeof(ApplicationAgent));

            //AddClass(list, typeof(RunSetActionHTMLReportSendEmail));
            //AddClass(list, typeof(EmailHtmlReportAttachment));
            //AddClass(list, typeof(RunSetActionAutomatedALMDefects));
            //AddClass(list, typeof(RunSetActionGenerateTestNGReport));
            //AddClass(list, typeof(RunSetActionHTMLReport));
            //AddClass(list, typeof(RunSetActionSaveResults));
            //AddClass(list, typeof(RunSetActionSendFreeEmail));
            //AddClass(list, typeof(RunSetActionSendSMS));
            //AddClass(list, typeof(RunSetActionPublishToQC));
            //AddClass(list, typeof(ActSetVariableValue));
            //AddClass(list, typeof(ActClearAllVariables));
            //AddClass(list, typeof(ActAgentManipulation));
            //AddClass(list, typeof(UserProfile));
            //AddClass(list, typeof(Solution));
            //AddClass(list, typeof(Email));
            //AddClass(list, typeof(EmailAttachment));
            //AddClass(list, typeof(RunSetActionScript));
        }

        //private static void AddClass(Dictionary<string, Type> list, Type t)
        //{
        //    list.Add((t).FullName, t);
        //    list.Add((t).Name, t);
        //}

        public void InitApp(WorkSpaceReporterBase workSpaceReporterBase, IRepositoryItemFactory repositoryItemFactory)
        {
            // Add event handler for handling non-UI thread exceptions.
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(StandAloneThreadExceptionHandler);

            Reporter.WorkSpaceReporter = workSpaceReporterBase;            

            string phase = string.Empty;

            RepositoryItemHelper.RepositoryItemFactory = repositoryItemFactory;

            WorkSpace.Instance.BetaFeatures = BetaFeatures.LoadUserPref();
            WorkSpace.Instance.BetaFeatures.PropertyChanged += BetaFeatureChanged;


            // !!!!!!!!!!!
            //if (WorkSpace.Instance.BetaFeatures.ShowDebugConsole)
            //{
            //    DebugConsoleWindow debugConsole = new DebugConsoleWindow();
            //    debugConsole.ShowAsWindow();
            //    WorkSpace.Instance.BetaFeatures.DisplayStatus();
            //}

            Reporter.ToLog(eLogLevel.INFO, "######################## Application version " + ApplicationInfo.AppVersion + " Started ! ########################");

            SetLoadingInfo("Init Application");
            // AppVersion = AppShortVersion;
            // We init the classes dictionary for the Repository Serializer only once
            InitClassTypesDictionary();

            // TODO: need to add a switch what we get from old ginger based on magic key

            phase = "Loading User Profile";
            Reporter.ToLog(eLogLevel.DEBUG, phase);
            SetLoadingInfo(phase);
            WorkSpace.Instance.UserProfile = UserProfile.LoadUserProfile();

            phase = "Configuring User Type";
            Reporter.ToLog(eLogLevel.DEBUG, phase);
            SetLoadingInfo(phase);
            WorkSpace.Instance.UserProfile.LoadUserTypeHelper();


            phase = "Loading User Selected Resource Dictionaries";
            Reporter.ToLog(eLogLevel.DEBUG, phase);
            SetLoadingInfo(phase);

            // Move back to App
            //if (WorkSpace.Instance.UserProfile != null)
            //{
            //    LoadApplicationDictionaries(Amdocs.Ginger.Core.eSkinDicsType.Default, WorkSpace.Instance.UserProfile.TerminologyDictionaryType);
            //}
            //else
            //{
            //    LoadApplicationDictionaries(Amdocs.Ginger.Core.eSkinDicsType.Default, GingerCore.eTerminologyType.Default);
            //}

            Reporter.ToLog(eLogLevel.DEBUG, "Loading user messages pool");
            UserMsgsPool.LoadUserMsgsPool();
            StatusMsgsPool.LoadStatusMsgsPool();

            Reporter.ToLog(eLogLevel.DEBUG, "Init the Centralized Auto Log");
            AutoLogProxy.Init(ApplicationInfo.AppVersion);

            Reporter.ToLog(eLogLevel.DEBUG, "Initializing the Source control");
            SetLoadingInfo(phase);

            phase = "Loading the Main Window";
            Reporter.ToLog(eLogLevel.DEBUG, phase);
            SetLoadingInfo(phase);


            AutoLogProxy.LogAppOpened();


            // Register our own Ginger tool tip handler
            //--Canceling customize tooltip for now due to many issues and no real added value            

            phase = "Application was loaded and ready";
            Reporter.ToLog(eLogLevel.INFO, phase);
            // mIsReady = true;

        }

       

        private static void SetLoadingInfo(string text)
        {
            // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //if (MainWindow != null)
            //{
            //    MainWindow.LoadingInfo(text);
            //}

            // Reporter.ToStatus(eStatusMsgKey.)
            Console.WriteLine("Loading Info: " + text);
        }

        private void BetaFeatureChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        public bool RunningFromUnitTest = false;

        private void StandAloneThreadExceptionHandler(object sender, UnhandledExceptionEventArgs e)
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
            Reporter.ToLog(eLogLevel.FATAL, ">>>>>>>>>>>>>> Error occurred on stand alone thread(non UI) - " + e.ExceptionObject);
            //Reporter.ToUser(eUserMsgKey.ThreadError, "Error occurred on stand alone thread - " + e.ExceptionObject.ToString());

            if (WorkSpace.Instance.RunningInExecutionMode == false)
            {                
                WorkSpace.Instance.AppSolutionAutoSave.DoAutoSave();
            }

            /// if (e.IsTerminating)...
            /// 
            //TODO: show exception
            // save work to temp folder
            // enable user to save work
            // ask if to restart/close
            // when loading check restore and restore
        }

        public bool OpenSolution(string SolutionFolder)
        {
            mPluginsManager = null;
            //TODO: remove later since below init only RS2
            // SolutionRepository.Open(SolutionFolder);
            try
            {
                Reporter.ToLog(eLogLevel.INFO, string.Format("Loading the Solution '{0}'", SolutionFolder));
                // mLoadingSolution = true;   // !!!!!!
                // OnPropertyChanged(nameof(LoadingSolution));   // !!!!!!

                //Cleanup
                SolutionCleanup();

                //Load new Solution
                string SolFile = System.IO.Path.Combine(SolutionFolder, @"Ginger.Solution.xml");
                if (File.Exists(Amdocs.Ginger.IO.PathHelper.GetLongPath(SolFile)))
                {

                    // !!!!!!!!!!!!!!!!!!!!!!!!

                    //get Solution files
                    //IEnumerable<string> solutionFiles = Solution.SolutionFiles(SolutionFolder);
                    //ConcurrentBag<Tuple<SolutionUpgrade.eGingerVersionComparisonResult, string>> solutionFilesWithVersion = null;


                    
                    //check if Ginger Upgrade is needed for loading this Solution
                    //try
                    //{
                    //    Reporter.ToLog(eLogLevel.DEBUG, "Checking if Ginger upgrade is needed for loading the Solution");
                    //    if (solutionFilesWithVersion == null)
                    //    {
                    //        solutionFilesWithVersion = SolutionUpgrade.GetSolutionFilesWithVersion(solutionFiles);
                    //    }
                    //    ConcurrentBag<string> higherVersionFiles = SolutionUpgrade.GetSolutionFilesCreatedWithRequiredGingerVersion(solutionFilesWithVersion, SolutionUpgrade.eGingerVersionComparisonResult.HigherVersion);
                    //    if (higherVersionFiles.Count > 0)
                    //    {
                    //        if (WorkSpace.Instance.RunningInExecutionMode == false && RunningFromUnitTest == false)
                    //        {
                    //            // MainWindow.HideSplash(); !!!!
                    //            UpgradePage gingerUpgradePage = new UpgradePage(SolutionUpgradePageViewMode.UpgradeGinger, SolutionFolder, string.Empty, higherVersionFiles.ToList());
                    //            gingerUpgradePage.ShowAsWindow();
                    //        }
                    //        Reporter.ToLog(eLogLevel.WARN, "Ginger upgrade is needed for loading the Solution, aborting Solution load.");
                    //        return false;
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    Reporter.ToLog(eLogLevel.ERROR, "Error occurred while checking if Solution requires Ginger Upgrade", ex);
                    //}

                    Solution sol = Solution.LoadSolution(SolFile);

                    if (sol != null)
                    {
                        SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
                        SolutionRepository.Open(SolutionFolder);

                        PlugInsManager.SolutionChanged(WorkSpace.Instance.SolutionRepository);

                        HandleSolutionLoadSourceControl(sol);

                        //ValueExpression.SolutionFolder = SolutionFolder;   //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        //BusinessFlow.SolutionVariables = sol.Variables;      //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        sol.SetReportsConfigurations();

                        Solution = sol;

                        UserProfile.LoadRecentAppAgentMapping();
                        AutoLogProxy.SetAccount(sol.Account);

                        //SetDefaultBusinessFlow();

                        if (!RunningInExecutionMode)
                        {
                            WorkSpace.Instance.AppSolutionRecover.DoSolutionAutoSaveAndRecover();   //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        }

                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        //Offer to upgrade Solution items to current version
                        //try
                        //{
                        //    if (WorkSpace.Instance.UserProfile.DoNotAskToUpgradeSolutions == false && WorkSpace.Instance.RunningInExecutionMode == false && RunningFromUnitTest == false)
                        //    {
                        //        if (solutionFilesWithVersion == null)
                        //        {
                        //            solutionFilesWithVersion = SolutionUpgrade.GetSolutionFilesWithVersion(solutionFiles);
                        //        }
                        //        ConcurrentBag<string> lowerVersionFiles = SolutionUpgrade.GetSolutionFilesCreatedWithRequiredGingerVersion(solutionFilesWithVersion, SolutionUpgrade.eGingerVersionComparisonResult.LowerVersion);
                        //        if (lowerVersionFiles != null && lowerVersionFiles.Count > 0)
                        //        {
                        //            UpgradePage solutionUpgradePage = new UpgradePage(SolutionUpgradePageViewMode.UpgradeSolution, sol.Folder, sol.Name, lowerVersionFiles.ToList());
                        //            solutionUpgradePage.ShowAsWindow();
                        //        }
                        //    }
                        //}
                        //catch (Exception ex)
                        //{
                        //    Reporter.ToLog(eLogLevel.ERROR, "Error occurred while checking if Solution files should be Upgraded", ex);
                        //}

                        // No need to add solution to recent if running from CLI
                        if (!RunningInExecutionMode)   // && !RunningFromUnitTest)     //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                        {
                            UserProfile.AddSolutionToRecent(sol);
                        }
                    }
                    else
                    {
                        Reporter.ToUser(eUserMsgKey.SolutionLoadError, "Load solution from file failed.");
                        return false;
                    }
                }
                else
                {
                    Reporter.ToUser(eUserMsgKey.BeginWithNoSelectSolution);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while loading the solution", ex);
                SolutionCleanup();
                throw ex;
            }
            finally
            {
                // mLoadingSolution = false;    // !!!!!!!!!!!!!!!!!!!!!!!!!
                // OnPropertyChanged(nameof(LoadingSolution));   // !!!!!!!!!!!!!!!!!!!!!!!!!
                Reporter.ToLog(eLogLevel.INFO, string.Format("Finished Loading the Solution '{0}'", SolutionFolder));
                // Mouse.OverrideCursor = null;   // !!!!!!!!!!!!!!!!!!!!!!!!!
            }
        }

        private static void HandleSolutionLoadSourceControl(Solution solution)
        {
            //string RepositoryRootFolder = string.Empty;
            //SourceControlBase.eSourceControlType type = SourceControlIntegration.CheckForSolutionSourceControlType(solution.Folder, ref RepositoryRootFolder);
            //if (type == SourceControlBase.eSourceControlType.GIT)
            //{
            //    solution.SourceControl = new GITSourceControl();
            //}
            //else if (type == SourceControlBase.eSourceControlType.SVN)
            //{
            //    solution.SourceControl = new SVNSourceControl();
            //}

            //if (solution.SourceControl != null)
            //{
            //    if (string.IsNullOrEmpty(WorkSpace.Instance.UserProfile.SolutionSourceControlUser) || string.IsNullOrEmpty(WorkSpace.Instance.UserProfile.SolutionSourceControlPass))
            //    {
            //        if (WorkSpace.Instance.UserProfile.SourceControlUser != null && WorkSpace.Instance.UserProfile.SourceControlPass != null)
            //        {
            //            solution.SourceControl.SourceControlUser = WorkSpace.Instance.UserProfile.SourceControlUser;
            //            solution.SourceControl.SourceControlPass = WorkSpace.Instance.UserProfile.SourceControlPass;
            //            solution.SourceControl.SolutionSourceControlAuthorEmail = WorkSpace.Instance.UserProfile.SolutionSourceControlAuthorEmail;
            //            solution.SourceControl.SolutionSourceControlAuthorName = WorkSpace.Instance.UserProfile.SolutionSourceControlAuthorName;
            //        }
            //    }
            //    else
            //    {
            //        solution.SourceControl.SourceControlUser = WorkSpace.Instance.UserProfile.SolutionSourceControlUser;
            //        solution.SourceControl.SourceControlPass = WorkSpace.Instance.UserProfile.SolutionSourceControlPass;
            //        solution.SourceControl.SolutionSourceControlAuthorEmail = WorkSpace.Instance.UserProfile.SolutionSourceControlAuthorEmail;
            //        solution.SourceControl.SolutionSourceControlAuthorName = WorkSpace.Instance.UserProfile.SolutionSourceControlAuthorName;
            //    }

            //    string error = string.Empty;
            //    solution.SourceControl.SolutionFolder = solution.Folder;
            //    solution.SourceControl.RepositoryRootFolder = RepositoryRootFolder;
            //    solution.SourceControl.SourceControlURL = solution.SourceControl.GetRepositoryURL(ref error);
            //    solution.SourceControl.SourceControlLocalFolder = WorkSpace.Instance.UserProfile.SourceControlLocalFolder;
            //    solution.SourceControl.SourceControlProxyAddress = WorkSpace.Instance.UserProfile.SolutionSourceControlProxyAddress;
            //    solution.SourceControl.SourceControlProxyPort = WorkSpace.Instance.UserProfile.SolutionSourceControlProxyPort;
            //    solution.SourceControl.SourceControlTimeout = WorkSpace.Instance.UserProfile.SolutionSourceControlTimeout;

            //    WorkSpace.Instance.SourceControl = solution.SourceControl;
            //    RepositoryItemBase.SetSourceControl(solution.SourceControl);
            //    RepositoryFolderBase.SetSourceControl(solution.SourceControl);
            //}
        }

        private static void SolutionCleanup()
        {
            //if (WorkSpace.Instance.SolutionRepository != null)
            //{
            //    WorkSpace.Instance.PlugInsManager.CloseAllRunningPluginProcesses();
            //}

            //if (!WorkSpace.Instance.RunningInExecutionMode)
            //{
            //    AppSolutionAutoSave.SolutionAutoSaveEnd();
            //}

            //WorkSpace.Instance.Solution = null;

            //CloseAllRunningAgents();
            //App.OnAutomateBusinessFlowEvent(AutomateEventArgs.eEventType.ClearAutomate, null);
            //AutoLogProxy.SetAccount("");
            //WorkSpace.Instance.SolutionRepository = null;
            //WorkSpace.Instance.SourceControl = null;
        }

        public void CloseAllRunningAgents()
        {
            if (WorkSpace.Instance.SolutionRepository != null)
            {
                List<Agent> runningAgents = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>().Where(x => x.Status == Agent.eStatus.Running).ToList();
                if (runningAgents != null && runningAgents.Count > 0)
                {
                    foreach (Agent agent in runningAgents)
                    {
                        try
                        {
                            agent.Close();
                        }
                        catch (Exception ex)
                        {
                            if (agent.Name != null)
                                Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to Close the '{0}' Agent", agent.Name), ex);
                            else
                                Reporter.ToLog(eLogLevel.ERROR, "Failed to Close the Agent", ex);
                        }
                        agent.IsFailedToStart = false;
                    }
                }
            }
        }


        public void CloseSolution()
        {
            SolutionRepository = null;
            SourceControl = null;
            EventHandler.SolutionClosed();
        }        

        public UserProfile UserProfile { get; set; }
       

        public IWorkSpaceEventHandler EventHandler { get; set; }


        // This is the local one 
        GingerGrid mLocalGingerGrid;
        public GingerGrid LocalGingerGrid
        {
            get
            {
                if (mLocalGingerGrid == null)
                {                    
                    mLocalGingerGrid = new GingerGrid();   
                    mLocalGingerGrid.Start();
                }
                return mLocalGingerGrid;
            }
        }

        public string LocalUserApplicationDataFolderPath
        {
            get
            {
                return General.LocalUserApplicationDataFolderPath;
            }
        }

        public string DefualtUserLocalWorkingFolder
        {
            get
            {
                return General.DefualtUserLocalWorkingFolder;
            }
        }

        public BetaFeatures mBetaFeatures;
        public BetaFeatures BetaFeatures
        {
            get
            {
                // in case we come from unit test which are not creating Beta feature flags and no user pref we create a clean new BetaFeatures obj
                if (mBetaFeatures == null)
                {
                    mBetaFeatures = new BetaFeatures();
                }
                return mBetaFeatures;
            }
            set
            {
                mBetaFeatures = value;
            }
        }

        private VEReferenceList mVERefrences;
        public VEReferenceList VERefrences
        {
            get
            {
                if (mVERefrences == null)
                {

                 mVERefrences=   VEReferenceList.LoadFromJson(Path.Combine(new string[] { Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "RosLynLib", "ValueExpressionRefrences.json" }));
                }

                return mVERefrences;
            }
            set
            {

                mVERefrences = value;
            }
        }        

        // Running from CLI
        public bool RunningInExecutionMode = false;
        
        public override string ItemName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void RefreshGlobalAppModelParams(ApplicationModelBase AppModel)
        {
            ObservableList<GlobalAppModelParameter> allGlobalParams = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GlobalAppModelParameter>();
            for (int i = 0; i < AppModel.GlobalAppModelParameters.Count; i++)
            {
                GlobalAppModelParameter apiGlobalParamInstance = AppModel.GlobalAppModelParameters[i];
                GlobalAppModelParameter globalParamInstance = allGlobalParams.Where(x => x.Guid == apiGlobalParamInstance.Guid).FirstOrDefault();
                //If the param still exist in the global list
                if (globalParamInstance != null)
                {
                    //Check for change and update in Configurations tab
                    if (!globalParamInstance.PlaceHolder.Equals(apiGlobalParamInstance.PlaceHolder))
                    {
                        AppModel.UpdateParamsPlaceholder(this, new List<string> { apiGlobalParamInstance.PlaceHolder }, globalParamInstance.PlaceHolder);
                        apiGlobalParamInstance.PlaceHolder = globalParamInstance.PlaceHolder;
                    }
                    apiGlobalParamInstance.CurrentValue = globalParamInstance.CurrentValue;

                    if (globalParamInstance.OptionalValuesList.Count > 0)
                    {
                        bool recoverSavedOV = false;
                        Guid currentDefaultOVGuid = new Guid();
                        //Save current default
                        if (apiGlobalParamInstance.OptionalValuesList.Count > 0)
                        {
                            currentDefaultOVGuid = apiGlobalParamInstance.OptionalValuesList.Where(x => x.IsDefault == true).FirstOrDefault().Guid;
                            recoverSavedOV = true;
                        }

                        string newDefaultOV = null;
                        apiGlobalParamInstance.OptionalValuesList.ClearAll();
                        foreach (OptionalValue ov in globalParamInstance.OptionalValuesList)
                        {
                            OptionalValue newOV = new OptionalValue();
                            newOV.Guid = ov.Guid;
                            newOV.Value = ov.Value;
                            if (ov.IsDefault)
                                newDefaultOV = ov.Guid.ToString();
                            apiGlobalParamInstance.OptionalValuesList.Add(newOV);
                        }

                        if (recoverSavedOV)
                        {
                            OptionalValue savedOV = apiGlobalParamInstance.OptionalValuesList.Where(x => x.Guid == currentDefaultOVGuid).FirstOrDefault();
                            if (savedOV != null)
                                savedOV.IsDefault = true;
                            else
                                apiGlobalParamInstance.OptionalValuesList.Where(x => x.Guid.ToString() == newDefaultOV).FirstOrDefault().IsDefault = true;
                        }
                        else
                            apiGlobalParamInstance.OptionalValuesList.Where(x => x.Guid.ToString() == newDefaultOV).FirstOrDefault().IsDefault = true;


                        apiGlobalParamInstance.OnPropertyChanged(nameof(AppModelParameter.OptionalValuesString));
                    }
                    else
                    {
                        apiGlobalParamInstance.OptionalValuesList.ClearAll();
                    }

                    apiGlobalParamInstance.OnPropertyChanged(nameof(AppModelParameter.PlaceHolder));
                }
                else
                {
                    AppModel.GlobalAppModelParameters.Remove(apiGlobalParamInstance);
                    i--;
                }
            }
        }

        
        public SolutionAutoSave AppSolutionAutoSave = new SolutionAutoSave();
        public SolutionRecover AppSolutionRecover = new SolutionRecover();
        public string RecoverFolderPath = null; //???  move to above ? !!!!!!!!!!!


    }
}
