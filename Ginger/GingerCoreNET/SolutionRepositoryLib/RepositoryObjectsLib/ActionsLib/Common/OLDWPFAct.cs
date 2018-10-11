//#region License
///*
//Copyright © 2014-2018 European Support Limited

//Licensed under the Apache License, Version 2.0 (the "License")
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at 

//http://www.apache.org/licenses/LICENSE-2.0 

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS, 
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and 
//limitations under the License. 
//*/
//#endregion

//using Amdocs.Ginger.Common;
//using Amdocs.Ginger.Common.Enums;
//using Amdocs.Ginger.Core;
//using Amdocs.Ginger.CoreNET.Execution;
//using Amdocs.Ginger.CoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActionsLib.Common;
//using Amdocs.Ginger.Repository;
//using GingerCoreNET.Dictionaries;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActivitiesLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.VariablesLib;
//using GingerPlugInsNET.ActionsLib;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActionsLib.Common
//{
//    // Each Act is one Activity Step     
//    public abstract partial class Act : RepositoryItem
//    {
//        // TODO: remove after we are done with old GingerCore
//        // used to keep the old class name for convereter to old action
//        public string OldClassName;
        
//        public enum eItemParts
//        {
//            All,
//            Details,
//            OutputValues,
//            ReturnValues,
//            FlowControls,
//            DSOutputConfigParams
//        }

//        public enum eColor
//        {
//            Green = 1,
//            Yellow = 2,
//            Red = 3
//        }

//        public enum eWindowsToCapture
//        {
//            [EnumValueDescription("Only Active Window")]
//            OnlyActiveWindow = 0,
//            [EnumValueDescription("All Available Windows")]
//            AllAvailableWindows = 1,
//            [EnumValueDescription("Desktop Screen")]
//            DesktopScreen = 2
//        }

//        public enum eStatusConverterOptions
//        {
//            [EnumValueDescription("None")]
//            None = 0,
//            [EnumValueDescription("Always Passed")]
//            AlwaysPass = 1,
//            [EnumValueDescription("Ignore Failed")]
//            IgnoreFail = 2,
//            [EnumValueDescription("Invert Status")]
//            InvertStatus = 3,
//        }

//        #region Serialized Attributes
//        // -----------------------------------------------------------------------------------------------------------------------------------------------
//        // All serialized Attributes - Start
//        // -----------------------------------------------------------------------------------------------------------------------------------------------

//        /// <summary>
//        /// GingerRunner will run the action if Active is true otherwise it will be marked skipped
//        /// </summary>
//        private bool mActive = true;
//        [IsSerializedForLocalRepository(DefaultValue: true)]
//        public Boolean Active { get { return mActive; } set { if (mActive != value) { mActive = value; OnPropertyChanged(nameof(Active)); } } }

//        // Each Action have uniqe identifer from which Plugin it came and the action ID

//        private string mPluginID;
//        [IsSerializedForLocalRepository]
//        public String PluginID { get { return mPluginID; } set { if (mPluginID != value) { mPluginID = value; OnPropertyChanged(nameof(PluginID)); } } }
        

//        private string mID;
//        [IsSerializedForLocalRepository]
//        public String ID { get { return mID; } set { if (mID != value) { mID = value; OnPropertyChanged(nameof(ID)); } } }


//        private string mDescription;
//        [IsSerializedForLocalRepository]
//        public String Description { get { return mDescription; } set { if (mDescription != value) { mDescription = value; OnPropertyChanged(nameof(Description)); } } }

//        private bool mSupportSimulation = false;
//        [IsSerializedForLocalRepository(DefaultValue:false)]
//        public bool SupportSimulation
//        {
//            get
//            { return mSupportSimulation; }
//            set
//            {
//                mSupportSimulation = value;
//                OnPropertyChanged(nameof(SupportSimulation));
//            }
//        }
       

//        string mOutDataSourceName;

//        public string OutDataSourceName { get { return mOutDataSourceName; } set { mOutDataSourceName = value; OnPropertyChanged(nameof(OutDataSourceName)); } }


//        string mOutDataSourceTableName;

//        public string OutDataSourceTableName { get { return mOutDataSourceTableName; } set { mOutDataSourceTableName = value; OnPropertyChanged(nameof(OutDataSourceTableName)); } }

//        private bool mEnableRetryMechanism = false;
//        [IsSerializedForLocalRepository(DefaultValue:false)]
//        public bool EnableRetryMechanism { get { return mEnableRetryMechanism; } set { mEnableRetryMechanism = value; OnPropertyChanged(nameof(EnableRetryMechanism)); } }

//        private int mRetryMechanismInterval = 5;
//        [IsSerializedForLocalRepository(DefaultValue:5)]
//        public int RetryMechanismInterval { get { return mRetryMechanismInterval; } set { mRetryMechanismInterval = value; OnPropertyChanged(nameof(RetryMechanismInterval)); } }

//        private int mMaxNumberOfRetries = 3;
//        [IsSerializedForLocalRepository(DefaultValue:3)]
//        public int MaxNumberOfRetries { get { return mMaxNumberOfRetries; } set { mMaxNumberOfRetries = value; OnPropertyChanged(nameof(MaxNumberOfRetries)); } }

//        private int mWait =0;
//        [IsSerializedForLocalRepository(DefaultValue:0)]
//        public int Wait { get { return mWait; } set { mWait = value; OnPropertyChanged(nameof(Wait)); } }

//        private int? mTimeout; //timeout in secs
//        [IsSerializedForLocalRepository]        
//        public int? Timeout { get; set; } //timeout in secs

//        private bool mConfigOutputDS = false;
//        [IsSerializedForLocalRepository(DefaultValue:false)]
//        public bool ConfigOutputDS { get { return mConfigOutputDS; } set { mConfigOutputDS = value; } }
        

//        private bool mTakeScreenShot = true;        
//        [IsSerializedForLocalRepository(DefaultValue:true)]
//        public bool TakeScreenShot { get { return mTakeScreenShot; } set { mTakeScreenShot = value; OnPropertyChanged(nameof(TakeScreenShot)); } }


//        private eWindowsToCapture mWindowToCapture;

//        [IsSerializedForLocalRepository(DefaultValue: eWindowsToCapture.OnlyActiveWindow)]
//        public eWindowsToCapture WindowsToCapture
//        {
//            get { return mWindowToCapture; }
//            set { mWindowToCapture = value; OnPropertyChanged(nameof(WindowsToCapture)); }
//        }

//        [IsSerializedForLocalRepository(DefaultValue: eStatusConverterOptions.None)]  
//        public eStatusConverterOptions StatusConverter { get; set; }

//        [IsSerializedForLocalRepository]
//        public ObservableList<FlowControl> FlowControls = new ObservableList<FlowControl>();

//        [IsSerializedForLocalRepository]
//        public ObservableList<ActInputValue> InputValues = new ObservableList<ActInputValue>();

//        [IsSerializedForLocalRepository]        
//        public ObservableList<ActReturnValue> ReturnValues = new ObservableList<ActReturnValue>();

//        [IsSerializedForLocalRepository]
//        public ObservableList<ActOutDataSourceConfig> DSOutputConfigParams = new ObservableList<ActOutDataSourceConfig>();

//        [IsSerializedForLocalRepository]
//        public ObservableList<VariableDependency> VariablesDependencies = new ObservableList<VariableDependency>();

//        // -----------------------------------------------------------------------------------------------------------------------------------------------
//        // All serialized Attributes - END
//        // -----------------------------------------------------------------------------------------------------------------------------------------------
//        #endregion Serialized Attributes

//        public string Error { get { return mError; } set { mError = value; OnPropertyChanged(nameof(Error)); } }

//        private long? mElapsed;
//        public long? Elapsed
//        {
//            get
//            {
//                return mElapsed;
//            }

//            set
//            {
//                if (mElapsed != value)
//                {
//                    mElapsed = value;
//                    OnPropertyChanged(nameof(Elapsed));
//                    OnPropertyChanged(nameof(ElapsedSecs));
//                }
//            }
//        }

//        private eRunStatus? mStatus;
//        public eRunStatus? Status { get { return mStatus; }
//            set
//            {
//                mStatus = value;
//                OnPropertyChanged(nameof(Status));
//                OnPropertyChanged(nameof(StatusIcon));
//            }
//        }
        
//        public bool? IsSingleAction { get; set; }
//        public DateTime StartTimeStamp { get; set; }
//        public DateTime EndTimeStamp { get; set; }
        
//        //Used only to support old actions, no need to serialize any more    
//        public bool FailIgnored
//        {
//            get
//            {
//                if (StatusConverter == Act.eStatusConverterOptions.IgnoreFail)
//                    return true;
//                else
//                    return false;
//            }
//            set
//            {
//                if (value)
//                    StatusConverter = Act.eStatusConverterOptions.IgnoreFail;
//            }
//        }

//        // Stop on this act if in debug mode
//        // No need to serialize
//        private bool mBreakPoint;
//        public bool BreakPoint { get { return mBreakPoint; } set { if (mBreakPoint != value) { mBreakPoint = value; OnPropertyChanged(nameof(BreakPoint)); } } }

//        //TODO: need to remove from here and use only ActUIElement         
//        public string LocateValueCalculated { get; set; }
        
//        // Action Type is being displayed on the actions grid and can be the action + sub action of the instance - user cannot edit
//        public abstract String ActionType { get; }

//        // Action Description is high level explanation of the action group, being displayed in add action window
//        public abstract String ActionDescription { get; }

//        /// <summary>
//        /// Define is the user will be able to select and add the Action to his flow
//        /// </summary>
//        public virtual bool IsSelectableAction { get { return true; } }

//        /// <summary>
//        /// Define if the Action will appear under the Legacy (non supported) Actions  
//        /// </summary>
//        public virtual bool IsLegacyAction { get { return false; } }

//        //Required to know if to show Locator Configuration fields in Action Edit window
//        public abstract bool ObjectLocatorConfigsNeeded { get; }
//        //Required to know if to show Value Configuration fields in Action Edit window
//        public abstract bool ValueConfigsNeeded { get; }

//        // return all supported platforms of this action, so in add action we show only the relevant
//        protected List<Platform.ePlatformType> mPlatforms = new List<Platform.ePlatformType>();
//        public abstract List<Platform.ePlatformType> Platforms { get; }
       
//        public override string ToString()
//        {
//            return mDescription;
//        }

//        // Page will be better (compile check) but since the pages are in Ginger we cannot ref them in Act
//        // So, meanhwile we can create by page string name

//        [DoNotBackup]
//        public abstract string ActionEditPage { get; }

//        //Action HL description which will be shown in the Add/Edit action window
//        [DoNotBackup]
//        public abstract String ActionUserDescription { get; }
        
//        // No need to serialze
//        public int RetryMechanismCount { get; set; }
//        public long ElapsedTicks { get; set; }

//        [DoNotBackup]
//        public Single? ElapsedSecs
//        {
//            get
//            {
//                if (Elapsed != null)
//                {
//                    return ((Single)Elapsed / 1000);
//                }
//                else
//                {
//                    return null;
//                }
//            }
//        }

//        public string SolutionFolder { set; get; }
//        private string mError;
//        private bool? mIsSingleAction;
//        private string mExInfo;

//        public string ExInfo { get { return mExInfo; } set { mExInfo = value; OnPropertyChanged(nameof(ExInfo)); } }

//        [DoNotBackup]
//        public string ActClass { get { return this.GetType().ToString(); } }


//        //Keeping screen shot in mem will eat up the memory - so we save to files and keep file name

//        public List<String> ScreenShots = new List<String>();
//        public List<String> ScreenShotsNames = new List<String>();


//        // No need to back because the list is saved to backup
//        [DoNotBackup]
//        public string Value
//        {
//            get
//            {
//                return GetInputParamValue("Value");
//            }
//            set
//            {
//                AddOrUpdateInputParamValue("Value", value);
//            }
//        }

//        [DoNotBackup]
//        public string ValueForDriver
//        {
//            get
//            {
//                return GetInputParamCalculatedValue("Value");
//            }
//            set
//            {
//                AddOrUpdateInputParamCalculatedValue("Value", value);
//            }
//        }
        
//        public ObservableList<ActInputValue> ActInputValues 
//        { 
//            get
//            {
//                return InputValues;
//            }
//        }

//        public ObservableList<ActReturnValue> ActReturnValues
//        {
//            get
//            {
//                return ReturnValues;
//            }
//        }

//        public ObservableList<ActOutDataSourceConfig> ActOutDSConfigParams
//        {
//            get
//            {
//                return DSOutputConfigParams;
//            }
//        }


//        public ObservableList<FlowControl> ActFlowControls
//        {
//            get
//            {
//                return FlowControls;
//            }
//        }




//        #region ActInputValues
//        public void AddInputValueParam(string ParamName)
//        {
//            // check if param already exist then update as it can be saved and loaded + keep other values
//            ActInputValue AIV = (from aiv in InputValues where aiv.Param == ParamName select aiv).FirstOrDefault();
//            if (AIV == null)
//            {
//                AIV = new ActInputValue();
//                AIV.Param = ParamName;
//                InputValues.Add(AIV);
//                AIV.Value = "";
//            }
//        }

//        public void RemoveInputParam(string ParamName)
//        {
//            InputValues.Remove((from aiv in InputValues where aiv.Param == ParamName select aiv).FirstOrDefault());
//        }
//        public void AddOrUpdateOutDataSourceParam(string DSName, string DSTable,string OutputType,string ColName="", string Active ="", List<string> mColNames=null)
//        {
//            bool isActive = true;
//            // check if param already exist then update as it can be saved and loaded + keep other values
//            ActOutDataSourceConfig ADCS = (from arc in DSOutputConfigParams where arc.DSName == DSName && arc.DSTable == DSTable && arc.OutputType == OutputType select arc).FirstOrDefault();
            
//            if (ADCS == null)
//            {
//                if (OutputType == "Parameter_Path")
//                    isActive = false;
//            }
//            else
//            {
//                if (mColNames == null && Active != "")
//                {
//                    ADCS.Active = bool.Parse(Active);
//                    return;
//                }
//                isActive = ADCS.Active;
//                ColName = ADCS.TableColumn;
//                if (mColNames != null && !mColNames.Contains(ColName))
//                    ColName = OutputType;
//                DSOutputConfigParams.Remove(ADCS);
//                ADCS = null;
//            }

//            ADCS = new ActOutDataSourceConfig();
//            ADCS.DSName = DSName;
//            ADCS.DSTable = DSTable;
//            ADCS.PossibleValues.Add(ColName);
//            DSOutputConfigParams.Add(ADCS);
//            ADCS.OutputType = OutputType;
//            if(mColNames != null)
//            {
//                foreach (string sCol in mColNames)
//                    if (sCol != ColName)
//                        ADCS.PossibleValues.Add(sCol);
//            }

//            ADCS.Active = isActive;

//            if(ColName != "")
//                ADCS.TableColumn = ColName;
//        }

//        public void RemoveAllButOneInputParam(string Param)
//        {
//            if ((from aiv in InputValues where aiv.Param == Param select aiv).Count() == 0)
//                InputValues.Clear();
//            else
//            {
//                while (InputValues.Count() > 1)
//                    InputValues.Remove((from aiv in InputValues where aiv.Param != Param select aiv).FirstOrDefault());
//            }
//        }

//        public void AddOrUpdateInputParamValue(string Param, string Value)
//        {
//            // check if param already exist then update as it can be saved and loaded + keep other values
//            ActInputValue AIV = (from aiv in InputValues where aiv.Param == Param select aiv).FirstOrDefault();
//            if (AIV == null)
//            {
//                AIV = new ActInputValue();
//                AIV.Param = Param;
//                InputValues.Add(AIV);
//            }

//            AIV.Value = Value;
//        }

//        public string GetInputParamValue(string Param)
//        {
//            // check if param already exist then update as it can be saved and loaded + keep other values
//            ActInputValue AIV = (from aiv in InputValues where aiv.Param == Param select aiv).FirstOrDefault();
//            if (AIV == null)
//            {
//                return null;
//            }

//            return AIV.Value;
//        }

//        public ActInputValue GetOrCreateInputParam(string Param, string DefaultValue = null)
//        {
//            ActInputValue AIV = (from aiv in InputValues where aiv.Param == Param select aiv).FirstOrDefault();
//            if (AIV == null)
//            {
//                AIV = new ActInputValue() { Param = Param, Value = DefaultValue };
//                InputValues.Add(AIV);
//            }
//            return AIV;
//        }

//        public void AddOrUpdateInputParamCalculatedValue(string Param, string calculatedValue)
//        {
//            // check if param already exist then update as it can be saved and loaded + keep other values
//            ActInputValue AIV = (from aiv in InputValues where aiv.Param == Param select aiv).FirstOrDefault();
//            if (AIV == null)
//            {
//                AIV = new ActInputValue();
//                AIV.Param = Param;
//                InputValues.Add(AIV);
//            }

//            AIV.ValueForDriver = calculatedValue;
//        }

//        public string GetInputParamCalculatedValue(string Param, bool decryptValue=true)
//        {
//            // check if param already exist then update as it can be saved and loaded + keep other values
//            ActInputValue AIV = (from aiv in InputValues where aiv.Param == Param select aiv).FirstOrDefault();
//            if (AIV == null)
//            {
//                return null;
//            }
//            return AIV.ValueForDriver;
//        }

//        public object GetInputParamCalculatedValueWithCustomType(string Param, Type typeOfReturn)
//        {
//            // check if param already exist then update as it can be saved and loaded + keep other values
//            ActInputValue AIV = (from aiv in InputValues where aiv.Param == Param select aiv).FirstOrDefault();
//            if (AIV == null)
//            {
//                return null;
//            }

//            if (typeOfReturn == typeof(Boolean))
//            {
//                bool value = false;
//                if (Boolean.TryParse(AIV.ValueForDriver, out value))
//                    return value;
//                else
//                    return null;
//            }
//            else if (typeOfReturn == typeof(int))
//            {
//                int value = 0;
//                if (int.TryParse(AIV.ValueForDriver, out value))
//                    return value;
//                else
//                    return null;
//            }
//            return null;
//        }

//        //TODO: make Name a mandatory and enforce providing where used, meanhwile making it optional
        
//        public override string GetNameForFileName()
//        {
//            //TODO: replace name with a unique ID?
//            //TODO: To add Action.Name to the file name
//            string fn = Description;
//            if (fn.Length > 100)
//            {
//                return fn.Substring(0, 99);
//            }
//            else
//            {
//                return fn;
//            }
//        }

//        #endregion ActInputValues
//        public void AddOrUpdateReturnParsedParamValue(List<object> list)
//        {
//            for (int i = 0; i< list.Count; i++)
//            {
//                AddOrUpdateReturnParamActual("Actual" + i, list[i].ToString());

//            }
//            this.ExInfo += list.FirstOrDefault();
//        }
//        public void AddOrUpdateReturnParamActual(string Param, string Value)
//        {
//            // check if param already exist then update as it can be saved and loaded + keep other values
//            ActReturnValue ARC = (from arc in ReturnValues where arc.ParamCalculated == Param select arc).FirstOrDefault();
//            if (ARC == null && (AddNewReturnParams==true || ConfigOutputDS==true))
//            {
//                ARC = new ActReturnValue();
//                ARC.Active = true;                
//                ReturnValues.Add(ARC);
//                ARC.Param = Param;
//            }

//            if (ARC != null)
//                ARC.Actual = Value;
//        }
//        private void updateOutDSColumn(string sColName,string sValue)
//        {
//            List<ActOutDataSourceConfig> mADCS = (from arc in DSOutputConfigParams where arc.DSName == OutDataSourceName && arc.DSTable == OutDataSourceTableName && arc.Active==true select arc).ToList();
//            foreach(ActOutDataSourceConfig ADSC in mADCS)
//            {

//            }
//        }

//        [IsSerializedForLocalRepository]
//        public ObservableList<Guid> Tags = new ObservableList<Guid>();      
//        public void AddOrUpdateReturnParamActualWithPath(string Param, string Value, string sPath)
//        {
//            // check if param already exist then update as it can be saved and loaded + keep other values
//            ActReturnValue ARC = (from arc in ReturnValues where arc.Param == Param && arc.PathCalculated == sPath select arc).FirstOrDefault();
//            if (ARC == null && (AddNewReturnParams==true || ConfigOutputDS == true))
//            {
//                ARC = new ActReturnValue();
//                ARC.Active = true;
//                ReturnValues.Add(ARC);
//                ARC.Param = Param;
//                ARC.Path = sPath;
//            }

//            if (ARC != null)
//                ARC.Actual = Value;
//        }
//        public void AddOrUpdateReturnParamExpected(string Param, string Value)
//        {
//            // check if param already exist then update as it can be saved and loaded + keep other values
//            ActReturnValue ARC = (from arc in ReturnValues where arc.Param == Param select arc).FirstOrDefault();
//            if (ARC == null && (AddNewReturnParams==true || ConfigOutputDS == true))
//            {
//                ARC = new ActReturnValue();
//                ARC.Active = true;
//                ReturnValues.Add(ARC);
//                ARC.Param = Param;
//            }

//            if (ARC != null)
//                ARC.Expected = Value;
//        }


//        public string GetReturnParam(string Param)
//        {
//            // check if param already exist then update as it can be saved and loaded + keep other values
//            ActReturnValue ARC = (from arc in ReturnValues where arc.Param == Param select arc).FirstOrDefault();
//            if (ARC == null)
//            {
//                return null;
//            }

//            return ARC.Actual;
//        }

//        public string GetDataSourceConfigParam(string OutputParam)
//        {
//            // check if param already exist then update as it can be saved and loaded + keep other values
//            ActOutDataSourceConfig ADSC = (from arc in DSOutputConfigParams where arc.OutputType == OutputParam select arc).FirstOrDefault();
//            if (ADSC == null)
//            {
//                return null;
//            }

//            return ADSC.TableColumn;
//        }

//        public string GetCalculatedExpectedParam(string Param)
//        {
//            // check if param already exist then update as it can be saved and loaded + keep other values
//            ActReturnValue ARC = (from arc in ReturnValues where arc.Param == Param select arc).FirstOrDefault();
//            if (ARC == null)
//            {
//                return null;
//            }

//            return ARC.ExpectedCalculated;
//        }

//        public string GetStoreToValueParam(string Param)
//        {
//            // check if param already exist then update as it can be saved and loaded + keep other values
//            ActReturnValue ARC = (from arc in ReturnValues where arc.Param == Param select arc).FirstOrDefault();
//            if (ARC == null)
//            {
//                return null;
//            }

//            return ARC.StoreToValue;
//        }
//        internal void AddAllPlatforms()
//        {
//            foreach (object v in Enum.GetValues(typeof(Platform.ePlatformType)))
//            {
//                mPlatforms.Add((Platform.ePlatformType)v);
//            }
//        }


//        /// <summary>
//        ///  this function is called after the action was executed by the driver
//        ///  Derived classed can override to handle special treatment like: ACTSceenShot, save to file
//        /// </summary>
//        public virtual void PostExecute()
//        {
//            // Base - do nothing
//        }

//        private string mSupportedPlatforms = string.Empty;
//        public string SupportedPlatforms
//        {
//            get
//            {
//                if (mSupportedPlatforms == string.Empty)
//                {
//                    mSupportedPlatforms = ConvertSupportedPlatformsToString(this);
//                    return mSupportedPlatforms;
//                }
//                else
//                {
//                    return mSupportedPlatforms;
//                }
//            }
//            set
//            {
//                mSupportedPlatforms = value;
//            }
//        }

//        private string ConvertSupportedPlatformsToString(Act act)
//        {
//            string supportedPlatforms = string.Empty;
//            foreach (Platform.ePlatformType actPlatform in act.Platforms)
//                supportedPlatforms += actPlatform.ToString() + ",";

//            if (supportedPlatforms.Contains("NA"))
//                supportedPlatforms = "All";//assumption is that if 'NA' is in the platforms list then all platforms are supported
//            else
//                supportedPlatforms = supportedPlatforms.TrimEnd(',');
//            return supportedPlatforms;
//        }

//        /// <summary>
//        /// Check if the action supposed to be executed according to it variables dependencies configurations
//        /// </summary>
//        /// <param name="parentActivity">The Action parent Activity</param>  
//        /// <param name="setActStatus">Define of to set the Action Status value in case the check fails</param>   
//        /// <returns></returns>
//        public bool? CheckIfVaribalesDependenciesAllowsToRun(Activity parentActivity, bool setActStatus=false)
//        {
//            bool? checkStatus = null;
//            try
//            {
//                //check objects are valid
//                if (parentActivity != null)
//                {
//                    //check if the Action-variables dependencies mechanisem is enabled
//                    if (parentActivity.EnableActionsVariablesDependenciesControl)
//                    {
//                        //check if the action configured to run with all activity selection list variables selected value
//                        List<VariableBase> activityListVars = parentActivity.Variables.Where(v => v.GetType() == typeof(VariableSelectionList) && v.Value != null).ToList();
//                        if (activityListVars != null && activityListVars.Count > 0)
//                        {
//                            foreach (VariableBase listVar in activityListVars)
//                            {
//                                VariableDependency varDep = null;
//                                if (this.VariablesDependencies != null)
//                                    varDep = this.VariablesDependencies.Where(avd => avd.VariableName == listVar.Name && avd.VariableGuid == listVar.Guid).FirstOrDefault();
//                                if (varDep == null)
//                                    varDep = this.VariablesDependencies.Where(avd => avd.VariableGuid == listVar.Guid).FirstOrDefault();
//                                if (varDep != null)
//                                {
//                                    if (!varDep.VariableValues.Contains(listVar.Value))
//                                    {
//                                        checkStatus = false;//the Selection List variable selected Value was not configured on the action
//                                        break;
//                                    }
//                                }
//                                else
//                                {
//                                    checkStatus = false;//the Selection List variable was not configured on the action
//                                    break;
//                                }
//                            }
//                            if (checkStatus == null)
//                                checkStatus = true;//All Selection List variable selected values were configured on the action
//                        }
//                        else
//                            checkStatus = true;//the Activity dont has Selection List variables
//                    }
//                    else
//                        checkStatus = true;//the mechanisem is disabled                    
//                }
//                else
//                    checkStatus = false; //Activity object is null

//                //Check failed
//                if (checkStatus == false && setActStatus == true)
//                {
//                    this.Status = eRunStatus.Skipped;
//                    this.ExInfo = "Action was not configured to run with current " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " values.";
//                }

//                return checkStatus;
//            }
//            catch (Exception ex)
//            {
//                //Check failed
//                if (setActStatus)
//                {
//                    this.Status = eRunStatus.Skipped;
//                    this.Error = "Error occurred while checking the Action " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " dependencies. Details: " + ex.Message;
//                }
//                return false;
//            }
//        }

//        public virtual List<ObservableList<ActInputValue>> GetInputValueListForVEProcessing()
//        {
//            //Ginger Runner call this func in prep time, each action can impl if needed

//            // will be override in derived class if needed othwerwise return null
//            // use in Tuxedo to process the UD file params vals
//            return null;
//        }

//        public ActReturnValue GetReturnValue(string ParamName)
//        {
//            ActReturnValue RC = (from x in ReturnValues where x.Param == ParamName select x).FirstOrDefault();
//            return RC;
//        }
//        public void ClearUnUsedReturnParams()
//        {
//            List<ActReturnValue> arvsToRemove = ReturnValues.Where(x => string.IsNullOrEmpty(x.Expected) && string.IsNullOrEmpty(x.SimulatedActual) && string.IsNullOrEmpty(x.StoreToValue)).ToList();
//            foreach (ActReturnValue arv in arvsToRemove)
//                ReturnValues.Remove(arv);
//        }


//        public bool? AddNewReturnParams { get; set; }

//        public override string ItemName
//        {
//            get
//            {
//                return this.Description;
//            }
//            set
//            {
//                this.Description = value;
//            }
//        }

//        public void Reset()
//        {
//            if (this != null)
//            {
//                this.ExInfo = string.Empty;
//                this.Error = null;
//                this.Elapsed = null;
//                this.ScreenShots.Clear();
//                this.ScreenShotsNames.Clear();

//                // remove return vals which don't have expected or store to var
//                // it is not needed since it will return back after we get results
//                // if i.e the SQL changed we want to reflect the latest changes and output what we got
//                for (int i = this.ReturnValues.Count; i > 0; i--)
//                {
//                    ActReturnValue ARV = this.ReturnValues[i - 1];
//                    if (String.IsNullOrEmpty(ARV.Expected) && String.IsNullOrEmpty(ARV.StoreToValue) && String.IsNullOrEmpty(ARV.SimulatedActual))
//                    {
//                        this.ReturnValues.Remove(ARV);
//                    }          
//                }

//                // reset output
//                foreach (ActReturnValue ARV in this.ReturnValues)
//                {
//                    ARV.Actual = null;
//                    ARV.ExpectedCalculated = null;
//                    ARV.Status = ActReturnValue.eStatus.Pending;
//                }
                
//                foreach (FlowControl FC in this.FlowControls)
//                {
//                    FC.Status = FlowControl.eStatus.Pending;
//                }

//                this.Status = eRunStatus.Pending;
//            }
//        }

//        public string ExecutionLogFolder { get; set; }

//        internal void AddScreenShot(string FileName)
//        {
//            ScreenShots.Add(FileName);
//        }

//        internal string GetFileNameForScreenShot()
//        {
//            return ExecutionLogFolder + @"ScreenShot_" + ScreenShots.Count + 1;
//        }
        
//        // This function is used to copy basic Act data from one type of action to another 
//        // Used for conversion fro example from ActGenElement --> ActUIElement
//        public void CopyInfoFrom(Act act)
//        {
//            //TODO: go over all SerailzedFields and copy

//            this.Description = act.Description;
//            this.BreakPoint = act.BreakPoint;
//            this.Active = act.Active;
//            this.Wait = act.Wait;
//        }

//        // Return details of the action for Actions grid and report 
//        // below is default impl but each action can customize
//        public virtual ActionDetails Details
//        {
//            get
//            {
//                // Make sure that each item displayed in the details have Propertychanged trigger for sync with UI
//                // Show old LocateBy, LocateValue
//                // TODO: remove when locate by removed from here
                
//                ActionDetails AD = new ActionDetails();

//                // TODO: we can also create Params for Driver list
//                ObservableList<ActionParamInfo> l = new ObservableList<ActionParamInfo>();
//                foreach (ActInputValue AIV in this.InputValues)
//                {
//                    if (!string.IsNullOrEmpty(AIV.Value))
//                    {
//                        l.Add(new ActionParamInfo() { Param = AIV.Param, Value = AIV.Value, CalculatedValue = AIV.ValueForDriver });
//                    }
//                }
//                AD.Params = l;

//                return AD;
//            }
//        }

//        public eImageType StatusIcon
//        {
//            get
//            {
//                switch (mStatus)
//                {
//                    case eRunStatus.Passed:
//                        return eImageType.Passed;                        
//                    case eRunStatus.Failed:
//                        return eImageType.Failed;
//                    case eRunStatus.Pending:
//                        return eImageType.Pending;
//                    case eRunStatus.Running:
//                        return eImageType.Processing;
//                    //TODO: all the rest
//                    default:
//                        return eImageType.Empty;
//                }
//            }
//        }

//        public object AsDriverAction()
//        {
//            //TODO: make me work
//            GingerAction gingerAction = new GingerAction(this.ID);
            
//            // TODO: pass input params

//            return gingerAction;
//        }

//        // Will happen when user edit action params since we don't keep it in XML we get the data on demand from the Plugin/Driver
//        public void UpdateInputParamsType()
//        {
//            Act act = ActionFactory.GetActionByID(this.ID);
//            if (act != null)
//            {
//                //TODO: we can auto fix new input values and remove unrelevant ones
//                foreach (ActInputValue AIV in act.InputValues)
//                {
//                    ActInputValue AIV1 = this.GetOrCreateInputParam(AIV.Param);
//                    AIV1.ParamType = AIV.ParamType;
//                }
//            }
//            else
//            {
//                throw new Exception("Cannot find action with ID: " + ID);
//            }
//        }
//    }
//}
