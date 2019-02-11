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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.RosLynLib.Refrences;
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.Run;
using GingerCore;
using GingerCore.Environments;
using GingerCoreNET.RunLib;
using GingerCoreNET.SourceControl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;

namespace amdocs.ginger.GingerCoreNET
{
    // WorkSpace is one object per user accessible from anywhere and hold the current status of the user selection
    // For GingerWPF it is one per running app
    // For Web it can be one per user connected
    public class WorkSpace : RepositoryItemBase
    {
        private static WorkSpace mWorkSpace;

        // public UserProfile UserProfile;

        public SolutionRepository SolutionRepository;        

        // Will be back when we moved GR to GingerCoreNET
        // public GingerRunner GingerRunner;

        // public ProjEnvironment CurrentEnvironment;
  
        public SourceControlBase SourceControl;
        public static RunsetExecutor RunsetExecutor = new RunsetExecutor();
        public static string AppVersion="0.0.0.0.0";

        // move from App to here
        //public static GingerRunner AutomateTabGingerRunner = new GingerRunner(Amdocs.Ginger.Common.eExecutedFrom.Automation);
        public  ISolution mSolution { get; set; }
        public  ISolution Solution
        {
            get { return mSolution; }
            set
            {
                mSolution = value;
                OnPropertyChanged(nameof(Solution));
            }
        }

        public static eRunStatus RunSetExecutionStatus = eRunStatus.Failed;
        
        public static string TempFolder
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

        // Here we will have knwon GingerGrids - !!!!!!!!!!!!!!!!!!! Design, think..........
        // public IObservable<GingerGrid> GingerGrids;
        public static GingerRunner AutomateTabGingerRunner { get; set; }
        public void OpenSolution(string SolutionFolder)
        {
            mPluginsManager = null;
            //TODO: remove later since below init only RS2
            SolutionRepository.Open(SolutionFolder);
            
            // AutoLogProxy.Init("Ginger Test");  //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            // ValueExpression.Solutionfolder = SolutionFolder;

            //if (UserProfile != null)
            //{
            //    UserProfile.AddsolutionToRecent(SolutionFolder);
            //    string UserProfileFileName = UserProfile.CreateUserProfileFileName();
            //    UserProfile.Save(UserProfileFileName);
            //}
        }

        //public void CleanupBeforeAppClosing()
        //{
        //    if (mLocalGingerGrid != null)
        //    {
        //        mLocalGingerGrid.Stop();
        //    }
        //}

        public void InitPluginsManager()
        {
            //WorkSpace.Instance.PlugInsManager = new PluginsManager();

            ////TODO: load plugin on demand, meanwhile we load all
            //ObservableList<PluginPackage> list = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<PluginPackage>();
            //foreach (PluginPackage p in list)
            //{
            //    PlugInsManager.AddPluginPackage(p.Folder);
            //}
        }

        public void CloseSolution()
        {
            SolutionRepository = null;
            // PlugInsManager = null;
            // GingerRunner = null;
            // CurrentEnvironment = null;
            SourceControl = null;
            EventHandler.SolutionClosed();
        }

        public static void Init(IWorkSpaceEventHandler WSEH)
        {
            mWorkSpace = new WorkSpace();
            mWorkSpace.EventHandler = WSEH;
        }
        public static UserProfile UserProfile { get; set; }
        public static WorkSpace Instance { get { return mWorkSpace; } }

        public IWorkSpaceEventHandler EventHandler { get; set; }

        //public void SetCurrentBusinessFlow(BusinessFlow BF)
        //{
        //}

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
        private static VERefrenceList mVERefrences;
        public static VERefrenceList VERefrences
        {
            get
            {
                if (mVERefrences == null)
                {

                 mVERefrences=   VERefrenceList.LoadFromJson(Path.Combine(new string[] { Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "RosLynLib", "ValueExpressionRefrences.json" }));
                }

                return mVERefrences;
            }
            set
            {

                mVERefrences = value;
            }
        }
        public static BusinessFlow Businessflow { get;  set; }

        public static bool RunningInExecutionMode = false;

        public static ProjEnvironment AutomateTabEnvironment;
        public override string ItemName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }


        //TODO: move to GingerRunner - pass the obj needed
        private void HookAgents()
        {
            // this.GingerRunner.ApplicationAgents.CollectionChanged += ApplicationAgents_CollectionChanged;            
        }

        //private void AA_PropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == nameof(ApplicationAgent.Agent))
        //    {
        //        ((ApplicationAgent)sender).Agent.PlugInsManager = PlugInsManager;s
        //        ((ApplicationAgent)sender).Agent.LocalGingerGrid = LocalGingerGrid;
        //    }
        //}

        //private void SaveUserProfile()
        //{
        //    string filename = UserProfile.CreateUserProfileFileName();
        //    UserProfile.Save(filename);
        //}

        private void ApplicationAgents_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //foreach (ApplicationAgent AA in this.GingerRunner.ApplicationAgents)
            //{
            //    CheckAssignAgent(AA);
            //    if (AA.Agent != null)
            //    {
            //        AA.Agent.PlugInsManager = PlugInsManager;
            //        AA.Agent.LocalGingerGrid = LocalGingerGrid;
            //    }
            //    AA.PropertyChanged += AA_PropertyChanged;
            //}
        }

        

        public void LoadUserProfile()
        {
            //string UserProfileFileName = UserProfile.CreateUserProfileFileName();
            //if (File.Exists(UserProfileFileName))
            //{
            //    UserProfile = UserProfile.LoadUserProfile(UserProfileFileName);
            //}
            //else
            //{
            //    string gingerFolder = UserProfile.GetUserGingerFolder();
            //    if (!Directory.Exists(gingerFolder))
            //    {
            //        Directory.CreateDirectory(gingerFolder);
            //    }
            //    UserProfile = new UserProfile();
            //    UserProfile.Save(UserProfileFileName);
            //}
        }

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


    }
    public interface IUserprofile
    {

    }
}
