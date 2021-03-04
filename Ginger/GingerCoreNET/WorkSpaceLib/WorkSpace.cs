#region License
/*
Copyright © 2014-2020 European Support Limited

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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.CoreNET.RosLynLib.Refrences;
using Amdocs.Ginger.CoreNET.TelemetryLib;
using Amdocs.Ginger.CoreNET.Utility;
using Amdocs.Ginger.CoreNET.WorkSpaceLib;
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.Functionalties;
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Environments;
using GingerCore.Platforms;

using GingerCoreNET.RunLib;
using GingerCoreNET.SolutionRepositoryLib.UpgradeLib;
using GingerCoreNET.SourceControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Amdocs.Ginger.Common.OS;

namespace amdocs.ginger.GingerCoreNET
{
    // WorkSpace is one object per user accessible from anywhere and hold the current status of the user selection
    // For Ginger.Exe it is one per running app
    // For Web it can be one per user connected
    // DO NOT ADD STATIC FIELDS
    public class WorkSpace 
    {        
        private static WorkSpace mWorkSpace;
        public static WorkSpace Instance
        {
            get
            {                
                return mWorkSpace;                
            }
        }

        public static OperatingSystemBase OS = OperatingSystemBase.GetOperatingSystem();
        static bool lockit;

        public static void LockWS()
        {
            Reporter.ToLog(eLogLevel.DEBUG, "Lock Workspace");
            
            Task.Factory.StartNew(() =>
            {
                lock (WorkSpace.Instance)
                {                    
                    lockit = true;
                    while (lockit)  // TODO: add timeout max 60 seconds
                    {
                        Thread.Sleep(100);
                    }
                }
            });
        }

        public static void RelWS()
        {
            lockit = false;
            Reporter.ToLog(eLogLevel.DEBUG, "Workspace released");
        }


        public static void Init(IWorkSpaceEventHandler WSEH, bool startLocalGrid = true)
        {
         
            mWorkSpace = new WorkSpace();
            mWorkSpace.EventHandler = WSEH;
            mWorkSpace.InitClassTypesDictionary();

            if (startLocalGrid)
            {
                mWorkSpace.InitLocalGrid();
            }
            Telemetry.Init();
            mWorkSpace.Telemetry.SessionStarted();
        }   

        public void StartLocalGrid()
        {
            if (LocalGingerGrid == null)
            {
                InitLocalGrid();
            }
            else
            {
                Reporter.ToLog(eLogLevel.ERROR, "StartLocalGrid requested but grid is already running");
            }
        }

        public void CloseWorkspace()
        {
            try
            {
                if (Solution != null)
                {
                    CloseSolution();
                }
                if (LocalGingerGrid != null)
                {
                    LocalGingerGrid.Stop();
                }
                Close();             
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.DEBUG, "ReleaseWorkspace error - " + ex.Message, ex);
            }            
        }


        public void Close()
        {
            try
            {
                AppSolutionAutoSave.StopSolutionAutoSave();
                if (SolutionRepository != null)
                {
                    CloseAllRunningAgents();
                    PlugInsManager.CloseAllRunningPluginProcesses();
                    SolutionRepository.StopAllRepositoryFolderWatchers();
                }

                if (!RunningInExecutionMode)
                {
                    UserProfile.GingerStatus = eGingerStatus.Closed;
                    UserProfile.SaveUserProfile();
                    AppSolutionAutoSave.CleanAutoSaveFolders();
                }

                if (WorkSpace.Instance.LocalGingerGrid != null)
                {
                    WorkSpace.Instance.LocalGingerGrid.Stop();
                }
                WorkSpace.Instance.Telemetry.SessionEnd();
                mWorkSpace = null;
            }
            catch (Exception ex)            
            {
                Reporter.ToLog(eLogLevel.DEBUG, "Exception during close workspace",ex);

            }       
        }

        private void InitLocalGrid()
        {            
            mLocalGingerGrid = new GingerGrid();
            mLocalGingerGrid.Start();
        }

        public SolutionRepository SolutionRepository;

        public SourceControlBase SourceControl;

        public ReportsInfo ReportsInfo = new ReportsInfo();
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


        PluginsManager mPluginsManager = new PluginsManager();
        public PluginsManager PlugInsManager { get { return mPluginsManager; }  }        

        static bool bDone = false;

        /// <summary>
        /// Init core classes type dictionary for RepositorySerializer
        /// </summary>
        void InitClassTypesDictionary()
        {
            if (bDone) return;
            bDone = true;

            // Add all RI classes from GingerCoreCommon
            NewRepositorySerializer.AddClassesFromAssembly(NewRepositorySerializer.eAssemblyType.GingerCoreCommon);

            // Add gingerCoreNET classes                        
            NewRepositorySerializer.AddClassesFromAssembly(NewRepositorySerializer.eAssemblyType.GingerCoreNET);
            //NewRepositorySerializer.AddClassesFromAssembly(typeof(ALMConfig).Assembly);
        }


        public void InitWorkspace(WorkSpaceReporterBase workSpaceReporterBase, ITargetFrameworkHelper repositoryItemFactory)
        {
            // Add event handler for handling non-UI thread exceptions.
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(StandAloneThreadExceptionHandler);

            Reporter.WorkSpaceReporter = workSpaceReporterBase;            

            string phase = string.Empty;

            TargetFrameworkHelper.Helper = repositoryItemFactory;

            BetaFeatures = BetaFeatures.LoadUserPref();
            BetaFeatures.PropertyChanged += BetaFeatureChanged;
            
            if (BetaFeatures.ShowDebugConsole && !WorkSpace.Instance.RunningFromUnitTest)
            {
                EventHandler.ShowDebugConsole(true);                
                BetaFeatures.DisplayStatus();
            }

            Reporter.ToLog(eLogLevel.INFO, "######################## Application version " + ApplicationInfo.ApplicationVersionWithInfo + " Started ! ########################");

            Reporter.ToLog(eLogLevel.DEBUG, "Loading user messages pool");
            UserMsgsPool.LoadUserMsgsPool();
            StatusMsgsPool.LoadStatusMsgsPool();

            // AppVersion = AppShortVersion;
            // We init the classes dictionary for the Repository Serializer only once
            InitClassTypesDictionary();

            // TODO: need to add a switch what we get from old ginger based on magic key

            Reporter.ToLog(eLogLevel.INFO, "Loading User Profile");            
            UserProfile = UserProfile.LoadUserProfile();
            
            Reporter.ToLog(eLogLevel.INFO, "Configuring User Type");
            UserProfile.LoadUserTypeHelper();            
                        
            CheckWebReportFolder();

            if (WorkSpace.Instance.LocalGingerGrid != null)
            {
                Reporter.ToLog(eLogLevel.INFO,"Ginger Grid Started at Port:" + WorkSpace.Instance.LocalGingerGrid.Port);                
            }
        }

        private void CheckWebReportFolder()
        {
            try
            {
                string clientAppFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports","Ginger-Web-Client");
                Reporter.ToLog(eLogLevel.INFO, "Copying from web report from: "+ clientAppFolderPath);
                string userAppFolder = Path.Combine(WorkSpace.Instance.LocalUserApplicationDataFolderPath, "Reports","Ginger-Web-Client");
                Reporter.ToLog(eLogLevel.INFO, "Copying to web report from: " + userAppFolder);
                if (Directory.Exists(clientAppFolderPath))
                {
                    string rootUserFolder = Path.Combine(WorkSpace.Instance.LocalUserApplicationDataFolderPath, "Reports");
                    if (Directory.Exists(rootUserFolder))
                        TryFolderDelete(rootUserFolder);
                    IoHandler.Instance.CopyFolderRec(clientAppFolderPath, userAppFolder, true);
                }
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Check WebReportFolder Error: " + ex.Message, ex);
            }
        }

        private void TryFolderDelete(string rootUserFolder)
        {
            try
            {
                Directory.Delete(rootUserFolder,true);
            }
            catch (Exception ex)
            {
               Reporter.ToLog(eLogLevel.DEBUG, "TryFolderDelete error - " + ex.Message, ex);
            }
        }

        private static void SetLoadingInfo(string text)
        {
           // FIX Message not shown !!!!!!!!!!!

            Reporter.ToStatus(eStatusMsgKey.GingerLoadingInfo, text);
            Reporter.ToLog(eLogLevel.DEBUG, "Loading Info: " + text);
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
                    Reporter.ToLog(eLogLevel.DEBUG, "StandAloneThreadExceptionHandler: Running from unit test ignoring error on ginger close");
                    return;
                }
            }
            Reporter.ToLog(eLogLevel.FATAL, ">>>>>>>>>>>>>> Error occurred on stand alone thread(non UI) - " + e.ExceptionObject);
            //Reporter.ToUser(eUserMsgKey.ThreadError, "Error occurred on stand alone thread - " + e.ExceptionObject.ToString());

            if (RunningInExecutionMode == false)
            {                
                AppSolutionAutoSave.DoAutoSave();
            }

            /// if (e.IsTerminating)...
            /// 
            //TODO: show exception
            // save work to temp folder
            // enable user to save work
            // ask if to restart/close
            // when loading check restore and restore
        }

        public bool OpenSolution(string solutionFolder)
        {                      
            try
            {
                Reporter.ToLog(eLogLevel.INFO, string.Format("Loading the Solution '{0}'", solutionFolder));
                LoadingSolution = true;

                //Cleaning previous Solution load
                Reporter.ToLog(eLogLevel.INFO, "Loading Solution- Cleaning previous Solution items");
                CloseSolution();

                //Load Solution file
                Reporter.ToLog(eLogLevel.INFO, "Loading Solution- Opening Solution located at: " + solutionFolder);
                string solutionFile = System.IO.Path.Combine(solutionFolder, @"Ginger.Solution.xml");
                Reporter.ToLog(eLogLevel.INFO, "Loading Solution- Loading Solution File: " + solutionFile);
                if (System.IO.File.Exists(solutionFile))
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Loading Solution- Solution File exist");
                }
                else
                {
                    if (!File.Exists(Amdocs.Ginger.IO.PathHelper.GetLongPath(solutionFile)))
                    {
                        //Reporter.ToUser(eUserMsgKey.BeginWithNoSelectSolution);
                        Reporter.ToLog(eLogLevel.ERROR, "Loading Solution- Error: Solution File Not Found");
                        return false;
                    }                   
                }                

                //Checking if Ginger upgrade is needed
                Reporter.ToLog(eLogLevel.INFO, "Loading Solution- Checking if Ginger Solution items upgrade is needed");
                IEnumerable<string> solutionFiles = Solution.SolutionFiles(solutionFolder);
                SolutionUpgrade.ClearPreviousScans();
                if (SolutionUpgrade.IsGingerUpgradeNeeded(solutionFolder, solutionFiles))
                {
                    Reporter.ToLog(eLogLevel.WARN, "Loading Solution- Error: Current Ginger version can't load the Solution because it includes items from higher Ginger version");
                    return false;
                }

                Reporter.ToLog(eLogLevel.DEBUG, "Loading Solution- Loading Solution xml into object");
                Solution solution = Solution.LoadSolution(solutionFile);
                if (solution == null)
                {
                    Reporter.ToUser(eUserMsgKey.SolutionLoadError, "Failed to load the Solution file");
                    Reporter.ToLog(eLogLevel.ERROR, "Loading Solution- Error: Failed to load the Solution file");
                    return false;
                }

                Reporter.ToLog(eLogLevel.INFO, "Loading Solution- Creating Items Repository");
                SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
                SolutionRepository.Open(solutionFolder);

                Reporter.ToLog(eLogLevel.INFO, "Loading Solution- Loading needed Plugins");
                mPluginsManager = new PluginsManager();
                mPluginsManager.SolutionChanged(SolutionRepository);

                Reporter.ToLog(eLogLevel.INFO, "Loading Solution- Doing Source Control Configurations");
                HandleSolutionLoadSourceControl(solution);

                Reporter.ToLog(eLogLevel.INFO, "Loading Solution- Updating Application Functionalities to Work with Loaded Solution");
                ValueExpression.SolutionFolder = solutionFolder;
                BusinessFlow.SolutionVariables = solution.Variables; 
                solution.SetReportsConfigurations();
                Solution = solution;
                UserProfile.LoadRecentAppAgentMapping();

                if (!RunningInExecutionMode)
                {
                    AppSolutionRecover.DoSolutionAutoSaveAndRecover();   
                }

                //Solution items upgrade
                SolutionUpgrade.CheckSolutionItemsUpgrade(solutionFolder, solution.Name, solutionFiles.ToList());

                // No need to add solution to recent if running from CLI
                if (!RunningInExecutionMode)  
                {
                    UserProfile.AddSolutionToRecent(solution);
                }
                // PlugInsManager = new PluginsManager();
                // mPluginsManager.Init(SolutionRepository);

                Reporter.ToLog(eLogLevel.INFO, string.Format("Finished Loading successfully the Solution '{0}'", solutionFolder));
                return true;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Loading Solution- Unexpected Error occurred while loading the solution", ex);
                CloseSolution();
                throw ex;
            }
            finally
            {
                LoadingSolution = false;                                               
            }
        }

        

        private static void HandleSolutionLoadSourceControl(Solution solution)
        {
            string repositoryRootFolder = string.Empty;
            WorkSpace.Instance.EventHandler.SetSolutionSourceControl(solution, ref repositoryRootFolder);
            
            if (solution.SourceControl != null && WorkSpace.Instance.UserProfile != null)
            {
                if ( string.IsNullOrEmpty(WorkSpace.Instance.UserProfile.SolutionSourceControlUser) || string.IsNullOrEmpty(WorkSpace.Instance.UserProfile.SolutionSourceControlPass))
                {
                    if (WorkSpace.Instance.UserProfile.SourceControlUser != null && WorkSpace.Instance.UserProfile.SourceControlPass != null)
                    {
                        solution.SourceControl.SourceControlUser = WorkSpace.Instance.UserProfile.SourceControlUser;
                        solution.SourceControl.SourceControlPass = WorkSpace.Instance.UserProfile.SourceControlPass;
                        solution.SourceControl.SolutionSourceControlAuthorEmail = WorkSpace.Instance.UserProfile.SolutionSourceControlAuthorEmail;
                        solution.SourceControl.SolutionSourceControlAuthorName = WorkSpace.Instance.UserProfile.SolutionSourceControlAuthorName;
                    }
                }
                else
                {
                    solution.SourceControl.SourceControlUser = WorkSpace.Instance.UserProfile.SolutionSourceControlUser;
                    solution.SourceControl.SourceControlPass = WorkSpace.Instance.UserProfile.SolutionSourceControlPass;
                    solution.SourceControl.SolutionSourceControlAuthorEmail = WorkSpace.Instance.UserProfile.SolutionSourceControlAuthorEmail;
                    solution.SourceControl.SolutionSourceControlAuthorName = WorkSpace.Instance.UserProfile.SolutionSourceControlAuthorName;
                }

                string error = string.Empty;
                solution.SourceControl.SolutionFolder = solution.Folder;
                solution.SourceControl.RepositoryRootFolder = repositoryRootFolder;
                solution.SourceControl.SourceControlURL = solution.SourceControl.GetRepositoryURL(ref error);
                solution.SourceControl.SourceControlLocalFolder = WorkSpace.Instance.UserProfile.SourceControlLocalFolder;
                solution.SourceControl.SourceControlProxyAddress = WorkSpace.Instance.UserProfile.SolutionSourceControlProxyAddress;
                solution.SourceControl.SourceControlProxyPort = WorkSpace.Instance.UserProfile.SolutionSourceControlProxyPort;
                solution.SourceControl.SourceControlTimeout = WorkSpace.Instance.UserProfile.SolutionSourceControlTimeout;

                if (solution.SourceControl.GetSourceControlType == SourceControlBase.eSourceControlType.GIT)
                {
                    solution.SourceControl.SourceControlBranch = Ginger.SourceControl.SourceControlIntegration.GetCurrentBranchForSolution(solution.SourceControl);
                }

                WorkSpace.Instance.SourceControl = solution.SourceControl;
                RepositoryItemBase.SetSourceControl(solution.SourceControl);
                RepositoryFolderBase.SetSourceControl(solution.SourceControl);
            }
        }

        public void CloseAllRunningAgents()
        {
            if (SolutionRepository != null)
            {
                List<Agent> runningAgents = SolutionRepository.GetAllRepositoryItems<Agent>().Where(x => x.Status == Agent.eStatus.Running).ToList();
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

        public void CloseAllEnvironments()
        {
            if (SolutionRepository != null)
            {
                foreach(ProjEnvironment env in SolutionRepository.GetAllRepositoryItems<ProjEnvironment>())
                {
                    env.CloseEnvironment();
                }
            }
        }

        public bool DoNotResetWorkspaceArgsOnClose { get; set; }
        public void CloseSolution()
        {
            //Do cleanup
            if (SolutionRepository != null)
            {
                PlugInsManager.CloseAllRunningPluginProcesses();
                CloseAllRunningAgents();
                CloseAllEnvironments();
                SolutionRepository.StopAllRepositoryFolderWatchers();
                if (!RunningInExecutionMode)
                {
                    AppSolutionAutoSave.SolutionAutoSaveEnd();
                }                
            }

            //Reset values
            if (!DoNotResetWorkspaceArgsOnClose)
            {
                mPluginsManager = new PluginsManager();
                SolutionRepository = null;
                SourceControl = null;
                Solution = null;
            }

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
                return mLocalGingerGrid;
            }
        }

        public string CommonApplicationDataFolderPath
        {
            get
            {
                return General.CommonApplicationDataFolderPath;
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

        public BusinessFlow GetNewBusinessFlow(string Name, bool setTargetApp = false)
        {
            BusinessFlow newBF = new BusinessFlow();
            newBF.Name = Name;
            
            Activity defActivity = new Activity() { Active = true };
            defActivity.ActivityName = GingerDicser.GetTermResValue(eTermResKey.Activity) + " 1";
            newBF.AddActivity(defActivity, newBF.AddActivitiesGroup());
            newBF.Activities.CurrentItem = defActivity;
            newBF.CurrentActivity = defActivity;

            if (setTargetApp == true && WorkSpace.Instance.Solution.ApplicationPlatforms.Count > 0)
            {
                newBF.TargetApplications.Add(new TargetApplication() { AppName = WorkSpace.Instance.Solution.MainApplication });
                newBF.CurrentActivity.TargetApplication = newBF.TargetApplications[0].Name;
            }

            return newBF;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(null, new PropertyChangedEventArgs(name));
            }
        }

        
        bool mLoadingSolution;
        public bool LoadingSolution
        {
            get
            {
                return mLoadingSolution;
            }
            set
            {
                if (mLoadingSolution != value)
                {
                    mLoadingSolution = value;
                    OnPropertyChanged(nameof(LoadingSolution));
                }
            }
        }

        public Telemetry Telemetry { get; internal set; }

        // Unified ;location to get the ExecutionResults Folder
        // Enable to redirect all test artifacts to another folder used in CLI, include json summary, report, execution results
        private string mTestArtifactsFolder;

        /// <summary>
        /// Return full path for folder to save execution results and any test artifacts like json summary, if folder do not exist it will be created
        /// </summary>
        public string TestArtifactsFolder
        {
            get
            {
                string folder;

                if (string.IsNullOrEmpty(mTestArtifactsFolder))
                {
                    return null;
                }
                else
                {
                    folder = mTestArtifactsFolder;
                }

                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                return folder;
            }
            set
            {
                mTestArtifactsFolder = value;
            }
        }

        

    }
}
