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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.CoreNET.Execution;
using Amdocs.Ginger.CoreNET.RosLynLib.Refrences;
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.Run;
using Ginger.SolutionGeneral;
using GingerCoreNET.RunLib;
using GingerCoreNET.SourceControl;
using System;
using System.Collections.Generic;
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
        }

        public SolutionRepository SolutionRepository;

        public SourceControlBase SourceControl;

        /// <summary>
        /// Hold all Run Set execution data + execution methods
        /// </summary>    
        public RunsetExecutor RunsetExecutor = new RunsetExecutor();

        public static string AppVersion="0.0.0.0.0";

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

        public void OpenSolution(string SolutionFolder)
        {
            mPluginsManager = null;
            //TODO: remove later since below init only RS2
            SolutionRepository.Open(SolutionFolder);
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
    }
}
