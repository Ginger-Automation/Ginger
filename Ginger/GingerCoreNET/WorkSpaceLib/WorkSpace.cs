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
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.Repository;
using Amdocs.Ginger.CoreNET.RosLynLib.Refrences;
using Amdocs.Ginger.CoreNET.WorkSpaceLib;
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.Functionalties;
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.Platforms;
using GingerCore.Variables;
using GingerCoreNET.RunLib;
using GingerCoreNET.SolutionRepositoryLib.UpgradeLib;
using GingerCoreNET.SourceControl;
using System;
using System.Collections.Concurrent;
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
    public class WorkSpace 
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
        }


        public void InitWorkspace(WorkSpaceReporterBase workSpaceReporterBase, IRepositoryItemFactory repositoryItemFactory)
        {
            // Add event handler for handling non-UI thread exceptions.
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(StandAloneThreadExceptionHandler);

            Reporter.WorkSpaceReporter = workSpaceReporterBase;            

            string phase = string.Empty;

            RepositoryItemHelper.RepositoryItemFactory = repositoryItemFactory;

            BetaFeatures = BetaFeatures.LoadUserPref();
            BetaFeatures.PropertyChanged += BetaFeatureChanged;
            
            if (BetaFeatures.ShowDebugConsole)
            {
                EventHandler.ShowDebugConsole(true);                
                BetaFeatures.DisplayStatus();
            }

            Reporter.ToLog(eLogLevel.INFO, "######################## Application version " + ApplicationInfo.ApplicationVersionWithInfo + " Started ! ########################");

            Reporter.ToLog(eLogLevel.DEBUG, "Loading user messages pool");
            UserMsgsPool.LoadUserMsgsPool();
            StatusMsgsPool.LoadStatusMsgsPool();

            SetLoadingInfo("Init Application");
            // AppVersion = AppShortVersion;
            // We init the classes dictionary for the Repository Serializer only once
            InitClassTypesDictionary();

            // TODO: need to add a switch what we get from old ginger based on magic key

            phase = "Loading User Profile";
            Reporter.ToLog(eLogLevel.DEBUG, phase);
            SetLoadingInfo(phase);
            UserProfile = UserProfile.LoadUserProfile();
            
            phase = "Configuring User Type";
            Reporter.ToLog(eLogLevel.DEBUG, phase);
            SetLoadingInfo(phase);
            UserProfile.LoadUserTypeHelper();            

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

            

            Reporter.ToLog(eLogLevel.DEBUG, "Init the Centralized Auto Log");
            AutoLogProxy.Init(ApplicationInfo.ApplicationVersionWithInfo);

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
           // FIX Message not shown !!!!!!!!!!!

            Reporter.ToStatus(eStatusMsgKey.GingerLoadingInfo, text);
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
            mPluginsManager = null;            
            try
            {
                Reporter.ToLog(eLogLevel.INFO, string.Format("Loading the Solution '{0}'", solutionFolder));
                LoadingSolution = true;

                //Cleanup previous Solution load
                Reporter.ToLog(eLogLevel.DEBUG, "Loading Solution- Cleaning previous Solution items");
                SolutionCleanup();

                //Load Solution file
                Reporter.ToLog(eLogLevel.DEBUG, "Loading Solution- Opening Solution located at: " + solutionFolder);
                string solutionFile = System.IO.Path.Combine(solutionFolder, @"Ginger.Solution.xml");
                Reporter.ToLog(eLogLevel.DEBUG, "Loading Solution- Loading Solution File: " + solutionFile);
                if (System.IO.File.Exists(solutionFile))
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Loading Solution- Solution File exist");
                }
                else
                {
                    Reporter.ToLog(eLogLevel.DEBUG, "Loading Solution- Solution File Not Found");
                }
                if (!File.Exists(Amdocs.Ginger.IO.PathHelper.GetLongPath(solutionFile)))
                {
                    Reporter.ToUser(eUserMsgKey.BeginWithNoSelectSolution);
                    return false;
                }

                //Checking if Ginger upgrade is needed
                Reporter.ToLog(eLogLevel.DEBUG, "Loading Solution- Checking if Ginger upgrade is needed");
                IEnumerable<string> solutionFiles = Solution.SolutionFiles(solutionFolder);
                SolutionUpgrade.ClearPreviousScans();
                SolutionUpgrade.CheckGingerUpgrade(solutionFolder, solutionFiles);

                Reporter.ToLog(eLogLevel.DEBUG, "Loading Solution- Loading Solution xml into object");
                Solution solution = Solution.LoadSolution(solutionFile);
                if (solution == null)
                {
                    Reporter.ToUser(eUserMsgKey.SolutionLoadError, "Load solution from file failed.");
                    return false;
                }

                Reporter.ToLog(eLogLevel.DEBUG, "Loading Solution- Creating Items Repository");
                SolutionRepository = GingerSolutionRepository.CreateGingerSolutionRepository();
                SolutionRepository.Open(solutionFolder);

                Reporter.ToLog(eLogLevel.DEBUG, "Loading Solution- Loading needed Plugins");
                PlugInsManager.SolutionChanged(SolutionRepository);

                Reporter.ToLog(eLogLevel.DEBUG, "Loading Solution- Doing Source Control Configurations");
                HandleSolutionLoadSourceControl(solution);

                Reporter.ToLog(eLogLevel.DEBUG, "Loading Solution- Updating Application Functionalities to Work with Loaded Solution");
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
                LoadingSolution = false;                  
                Reporter.ToLog(eLogLevel.INFO, string.Format("Finished Loading the Solution '{0}'", solutionFolder));                
            }
        }

        

        private static void HandleSolutionLoadSourceControl(Solution solution)
        {
            string RepositoryRootFolder = string.Empty;

            WorkSpace.Instance.EventHandler.SetSolutionSourceControl(solution);
            

            if (solution.SourceControl != null)
            {
                if (string.IsNullOrEmpty(WorkSpace.Instance.UserProfile.SolutionSourceControlUser) || string.IsNullOrEmpty(WorkSpace.Instance.UserProfile.SolutionSourceControlPass))
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
                solution.SourceControl.RepositoryRootFolder = RepositoryRootFolder;
                solution.SourceControl.SourceControlURL = solution.SourceControl.GetRepositoryURL(ref error);
                solution.SourceControl.SourceControlLocalFolder = WorkSpace.Instance.UserProfile.SourceControlLocalFolder;
                solution.SourceControl.SourceControlProxyAddress = WorkSpace.Instance.UserProfile.SolutionSourceControlProxyAddress;
                solution.SourceControl.SourceControlProxyPort = WorkSpace.Instance.UserProfile.SolutionSourceControlProxyPort;
                solution.SourceControl.SourceControlTimeout = WorkSpace.Instance.UserProfile.SolutionSourceControlTimeout;

                WorkSpace.Instance.SourceControl = solution.SourceControl;
                RepositoryItemBase.SetSourceControl(solution.SourceControl);
                RepositoryFolderBase.SetSourceControl(solution.SourceControl);
            }
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


        public void CloseSolution()
        {
            SolutionRepository = null;
            SourceControl = null;
            EventHandler.SolutionClosed();
            Solution = null;
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
            BusinessFlow biz = new BusinessFlow();
            biz.Name = Name;
            biz.Activities = new ObservableList<Activity>();
            biz.Variables = new ObservableList<VariableBase>();
            Activity a = new Activity() { Active = true };
            a.ActivityName = GingerDicser.GetTermResValue(eTermResKey.Activity) + " 1";
            a.Acts = new ObservableList<IAct>();
            biz.Activities.Add(a);
            biz.Activities.CurrentItem = a;
            biz.CurrentActivity = a;

            if (setTargetApp == true && WorkSpace.Instance.Solution.ApplicationPlatforms.Count > 0)
            {
                biz.TargetApplications.Add(new TargetApplication() { AppName = WorkSpace.Instance.Solution.MainApplication });
                biz.CurrentActivity.TargetApplication = biz.TargetApplications[0].Name;
            }

            return biz;
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


    }
}
