#region License
/*
Copyright © 2014-2025 European Support Limited

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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Actions;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.Common.Repository;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore.Actions.Common;
using GingerCore.Actions.WebServices.WebAPI;
using GingerCore.FlowControlLib;
using GingerCore.GeneralLib;
using GingerCore.Variables;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
namespace GingerCore.Actions
{
    // Each Act is one Activity Step     
    public abstract partial class Act : RepositoryItemBase, IAct
    {
        public enum eItemParts
        {
            All,
            Details,
            InputValues,
            ReturnValues,
            FlowControls,
            DSOutputConfigParams
        }

        public enum eColor
        {
            Green = 1,
            Yellow = 2,
            Red = 3
        }

        public enum eWindowsToCapture
        {
            [EnumValueDescription("Only Active Window")]
            OnlyActiveWindow = 0,
            [EnumValueDescription("All Available Windows")]
            AllAvailableWindows = 1,
            [EnumValueDescription("Desktop Screen")]
            DesktopScreen = 2,
            [EnumValueDescription("Full Page Screenshot")]
            FullPage = 3,
            [EnumValueDescription("Full Page With URL and Timestamp")]
            FullPageWithUrlAndTimestamp = 4
        }



        private static string mScreenshotTempFolder;
        public static string ScreenshotTempFolder
        {
            get
            {
                if (string.IsNullOrEmpty(mScreenshotTempFolder))
                {
                    mScreenshotTempFolder = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Ginger_Screenshots");
                }
                return mScreenshotTempFolder;
            }
        }


        public enum eOutputDSParamMapType
        {
            [EnumValueDescription("Param to Row")]
            ParamToRow,
            [EnumValueDescription("Param To Col")]
            ParamToCol,
        }
        public static partial class Fields
        {
            public static string Active = "Active";
            public static string ActionDescription = "ActionDescription";
            public static string ActionUserDescription = "ActionUserDescription";
            public static string BreakPoint = "BreakPoint";
            public static string Image = "Image";
            public static string Description = "Description";
            public static string RunDescription = "RunDescription";
            public static string ActionType = "ActionType";
            public static string LocateBy = "LocateBy";
            public static string LocateValue = "LocateValue";
            public static string Status = "Status";
            public static string ElapsedSecs = "ElapsedSecs";
            public static string Elapsed = "Elapsed";
            public static string ElapsedTicks = "ElapsedTicks";
            public static string Error = "Error";
            public static string ExInfo = "ExInfo";
            public static string ActClass = "ActClass";
            public static string TakeScreenShot = "TakeScreenShot";
            public static string WindowsToCapture = "WindowsToCapture";
            public static string ScreenShot = "ScreenShot";
            public static string Varb = "Varb";
            public static string Wait = "Wait";
            public static string Timeout = "Timeout";
            public static string Platform = "Platform";
            public static string SupportedPlatforms = "SupportedPlatforms";
            public static string InputValues = "InputValues";
            public static string ReturnValues = "ReturnValues";
            public static string FlowControls = "FlowControls";
            public static string EnableRetryMechanism = "EnableRetryMechanism";
            public static string RetryMechanismInterval = "RetryMechanismInterval";
            public static string MaxNumberOfRetries = "MaxNumberOfRetries";
            public static string FailIgnored = "FailIgnored";
            public static string AddNewReturnParams = "AddNewReturnParams";
            public static string StatusConverter = "StatusConverter";
            public static string Details = "Details";
            public static string ConfigOutputDS = "ConfigOutputDS";
            public static string DSOutputConfigParams = "DSOutputConfigParams";
            public static string OutDataSourceName = "OutDataSourceName";
            public static string OutDataSourceTableName = "OutDataSourceTableName";
            public static string OutDSParamMapType = "OutDSParamMapType";
            public static string SupportSimulation = "SupportSimulation";
            public static string AutoScreenShotOnFailure = "AutoScreenShotOnFailure";
        }

        // Being set by GingerRunner in PrepAction
        public IValueExpression ValueExpression { get; set; }

        #region Serialized Attributes
        // -----------------------------------------------------------------------------------------------------------------------------------------------
        // All serialized Attributes - Start
        // -----------------------------------------------------------------------------------------------------------------------------------------------

        [IsSerializedForLocalRepository]
        public ActionLogConfig ActionLogConfig { get; set; }

        private bool mEnableActionLogConfig;
        [IsSerializedForLocalRepository]
        public bool EnableActionLogConfig { get { return mEnableActionLogConfig; } set { if (mEnableActionLogConfig != value) { mEnableActionLogConfig = value; OnPropertyChanged(nameof(EnableActionLogConfig)); } } }


        private bool mActive;
        [IsSerializedForLocalRepository]
        [AllowUserToEdit("Active")]
        public Boolean Active { get { return mActive; } set { if (mActive != value) { mActive = value; OnPropertyChanged(Fields.Active); } } }


        private string mDescription;
        [IsSerializedForLocalRepository]
        [AllowUserToEdit("Description")]
        public String Description { get { return mDescription; } set { if (mDescription != value) { mDescription = value; OnPropertyChanged(Fields.Description); } } }

        private string mRunDescription;
        [IsSerializedForLocalRepository]
        [AllowUserToEdit("Run Description")]
        public String RunDescription { get { return mRunDescription; } set { if (mRunDescription != value) { mRunDescription = value; OnPropertyChanged(Fields.RunDescription); } } }

        private bool mSupportSimulation;
        [IsSerializedForLocalRepository]
        public bool SupportSimulation
        {
            get
            {
                return mSupportSimulation;
            }
            set
            {
                if (mSupportSimulation != value)
                {
                    mSupportSimulation = value;
                    OnPropertyChanged(Fields.SupportSimulation);
                }
            }
        }

        ///<summary>
        /// Gets a value indicating whether automatic screenshot capture is enabled on failure.
        /// </summary>
        private bool mAutoScreenShotOnFailure = true;
        [IsSerializedForLocalRepository(DefaultValue: true)]
        public bool AutoScreenShotOnFailure
        {
            get
            {
                return mAutoScreenShotOnFailure;
            }
            set
            {

                if (mAutoScreenShotOnFailure != value)
                {
                    mAutoScreenShotOnFailure = value;
                    OnPropertyChanged(Fields.AutoScreenShotOnFailure);
                }
            }
        }
        public virtual eLocateBy LocateBy
        {
            get
            {
                // Avoid creating new LcoateBy if this action doesn't need it
                if (this.ObjectLocatorConfigsNeeded)
                {
                    return GetOrCreateInputParam<eLocateBy>(Fields.LocateBy, eLocateBy.NA);
                }
                else
                {
                    return eLocateBy.NA;
                }
            }
            set
            {
                if (this.ObjectLocatorConfigsNeeded)
                {
                    AddOrUpdateInputParamValue(Act.Fields.LocateBy, value.ToString());
                    OnPropertyChanged(Fields.LocateBy);
                    OnPropertyChanged(Fields.Details);
                }
            }
        }


        public virtual string LocateValue
        {
            get
            {
                // Avoid creating new LcoateBy if this action doesn't need it
                if (this.ObjectLocatorConfigsNeeded)
                {
                    return GetOrCreateInputParam(Fields.LocateValue).Value;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (this.ObjectLocatorConfigsNeeded)
                {
                    AddOrUpdateInputParamValue(Act.Fields.LocateValue, value);
                    OnPropertyChanged(Fields.LocateValue);
                    OnPropertyChanged(Fields.Details);
                }
            }
        }

        string mOutDataSourceName;

        public string OutDataSourceName { get { return mOutDataSourceName; } set { mOutDataSourceName = value; OnPropertyChanged(Fields.OutDataSourceName); } }


        string mOutDataSourceTableName;

        public string OutDataSourceTableName { get { return mOutDataSourceTableName; } set { mOutDataSourceTableName = value; OnPropertyChanged(Fields.OutDataSourceTableName); } }

        string mOutDSParamMapType;

        public string OutDSParamMapType { get { return mOutDSParamMapType; } set { mOutDSParamMapType = value; OnPropertyChanged(Fields.OutDSParamMapType); } }

        string mRawResponseValues;

        public string RawResponseValues
        {
            get
            {
                return mRawResponseValues;
            }
            set
            {
                mRawResponseValues = value;
                OnPropertyChanged(nameof(RawResponseValues));
            }
        }




        private bool mEnableRetryMechanism;
        [IsSerializedForLocalRepository]
        public bool EnableRetryMechanism
        {
            get
            {
                return mEnableRetryMechanism;
            }
            set
            {
                if (mEnableRetryMechanism != value)
                {
                    mEnableRetryMechanism = value;
                    OnPropertyChanged(Fields.EnableRetryMechanism);
                }

                if (!value)
                {
                    MaxNumberOfRetries = 2;
                }
            }
        }

        private int mRetryMechanismInterval = 5;
        [IsSerializedForLocalRepository]
        public int RetryMechanismInterval { get { return mRetryMechanismInterval; } set { if (mRetryMechanismInterval != value) { mRetryMechanismInterval = value; OnPropertyChanged(Fields.RetryMechanismInterval); } } }

        private int mMaxNumberOfRetries = 2;
        [IsSerializedForLocalRepository]
        public int MaxNumberOfRetries
        {
            get
            {
                if (EnableRetryMechanism)
                {
                    return mMaxNumberOfRetries;
                }
                return 0;

                }
            set
            {
                if (mMaxNumberOfRetries != value)
                {
                    mMaxNumberOfRetries = value;
                    OnPropertyChanged(Fields.MaxNumberOfRetries);
                }
            }
        }


        private int mWait;
        public int Wait
        {
            get
            {
                return mWait;
            }
            set
            {
                if (mWait != value)
                {
                    mWait = value;
                    if (WaitVE == null)
                    {
                        WaitVE = value.ToString();
                    }
                }
            }
        }

        private string mWaitVE;
        [IsSerializedForLocalRepository]
        public string WaitVE
        {
            get
            {
                return mWaitVE;
            }
            set
            {
                if (mWaitVE != value)
                {
                    mWaitVE = value;
                    OnPropertyChanged(nameof(WaitVE));
                }
            }
        }


        private int? mTimeout;
        [IsSerializedForLocalRepository]
        public int? Timeout
        {
            get
            {
                return mTimeout;
            }
            set
            {
                if (mTimeout != value)
                {
                    mTimeout = value;
                    OnPropertyChanged(nameof(Timeout));
                }
            }
        } //timeout in secs

        private bool mConfigOutputDS;
        [IsSerializedForLocalRepository]
        public bool ConfigOutputDS { get { return mConfigOutputDS; } set { mConfigOutputDS = value; } }

        [IsSerializedForLocalRepository]
        public bool ConfigOutDSParamAutoCheck { get; set; }


        private ePlatformType mPlatform;
        [IsSerializedForLocalRepository]
        public ePlatformType Platform { get { return mPlatform; } set { if (mPlatform != value) { mPlatform = value; OnPropertyChanged(Fields.Platform); } } }
        // -------------------------------

        private bool mTakeScreenShot { get; set; }
        [IsSerializedForLocalRepository]
        public bool TakeScreenShot { get { return mTakeScreenShot; } set { if (mTakeScreenShot != value) { mTakeScreenShot = value; OnPropertyChanged(Fields.TakeScreenShot); } } }


        private eWindowsToCapture mWindowToCapture;

        [IsSerializedForLocalRepository]
        public eWindowsToCapture WindowsToCapture
        {
            get { return mWindowToCapture; }
            set { if (mWindowToCapture != value) { mWindowToCapture = value; OnPropertyChanged(Fields.WindowsToCapture); } }
        }

        private eStatusConverterOptions mStatusConverter;
        [IsSerializedForLocalRepository]
        public eStatusConverterOptions StatusConverter
        {
            get
            {
                return mStatusConverter;
            }
            set
            {
                if (mStatusConverter != value)
                {
                    mStatusConverter = value;
                    OnPropertyChanged(nameof(StatusConverter));
                }
            }
        }

        [IsSerializedForLocalRepository]
        public ObservableList<FlowControl> FlowControls { get; set; } = [];

        [IsSerializedForLocalRepository]
        public ObservableList<ActInputValue> InputValues { get; set; } = [];

        [IsSerializedForLocalRepository]
        public ObservableList<ActReturnValue> ReturnValues { get; set; } = [];

        [IsSerializedForLocalRepository]
        public ObservableList<ActOutDataSourceConfig> DSOutputConfigParams = [];

        [IsSerializedForLocalRepository]
        public ObservableList<VariableDependency> VariablesDependencies { get; set; } = [];

        // -----------------------------------------------------------------------------------------------------------------------------------------------
        // All serialized Attributes - END
        // -----------------------------------------------------------------------------------------------------------------------------------------------
        #endregion Serialized Attributes

        public string Error { get { return mError; } set { mError = value; OnPropertyChanged(Fields.Error); } }

        private long? mElapsed;
        public long? Elapsed { get { return mElapsed; } set { mElapsed = value; OnPropertyChanged(Fields.Elapsed); OnPropertyChanged(Fields.ElapsedSecs); } }

        private Amdocs.Ginger.CoreNET.Execution.eRunStatus? mStatus;
        public Amdocs.Ginger.CoreNET.Execution.eRunStatus? Status { get { return mStatus; } set { mStatus = value; OnPropertyChanged(Fields.Status); } }


        //TODO: Move to ActUIElem... where is it used? commented out since look like it is not in use
        //private int mSmartWait;
        //public int SmartWait { get; set; }  

        // public bool? IsSingleAction { get; set; }

        public DateTime StartTimeStamp { get; set; }
        public DateTime EndTimeStamp { get; set; }


        //Used only to support old actions, no need to serialize any more     
        // [IsSerializedForLocalRepository]
        public bool FailIgnored
        {
            get
            {
                return StatusConverter == eStatusConverterOptions.IgnoreFail;
            }
            set
            {
                if (value)
                {
                    StatusConverter = eStatusConverterOptions.IgnoreFail;
                }
            }
        }



        // Stop on this act if in debug mode
        // No need to serialize
        private bool mBreakPoint;
        public bool BreakPoint
        {
            get
            {
                return mBreakPoint;
            }
            set
            {
                if (mBreakPoint != value)
                {
                    mBreakPoint = value;
                    OnPropertyChanged(Fields.BreakPoint);
                }
            }
        }

        //TODO: need to remove from here and use only ActUIElement         
        public string LocateValueCalculated { get; set; }


        // show image base on Act type near the line number
        //public virtual System.Drawing.Image Image { get { return null; } } //TODO: to be replaced with ItemImageType for all Actions types
        public virtual eImageType Image { get { return eImageType.Action; } } //TODO: to be replaced with ItemImageType for all Actions types


        // [IsSerializedForLocalRepository]
        // public string Description { get { return mDescription; } set { mDescription = value; OnPropertyChanged(Fields.Description); } }


        // Action Type is being displayed on the actions grid and can be the action + sub action of the instance - user cannot edit
        public abstract String ActionType { get; }

        // Action Description is high level explanation of the action group, being displayed in add action window
        public abstract String ActionDescription { get; }

        /// <summary>
        /// Define is the user will be able to select and add the Action to his flow
        /// </summary>
        public virtual bool IsSelectableAction { get { return true; } }

        public virtual List<ePlatformType> LegacyActionPlatformsList { get { return []; } }

        //Required to know if to show Locator Configuration fields in Action Edit window
        public abstract bool ObjectLocatorConfigsNeeded { get; }
        //Required to know if to show Value Configuration fields in Action Edit window
        public abstract bool ValueConfigsNeeded { get; }

        public virtual string AddActionWizardPage { get { return null; } }

        // return all supported platforms of this action, so in add action we show only the relevant
        protected List<ePlatformType> mPlatforms = [];
        public abstract List<ePlatformType> Platforms { get; }

        // return all supported LocateBy of this action, so in edit action page we show only the relevant
        // by default we return all, each action can override
        // TODO: use abstract to force all actions to impl
        public virtual List<eLocateBy> AvailableLocateBy()
        {
            return getAllLocateBy();
        }


        public override string ToString()
        {
            return mDescription;
        }

        private List<eLocateBy> getAllLocateBy()
        {
            List<eLocateBy> l = [];
            foreach (var v in Enum.GetValues(typeof(eLocateBy)))
            {
                l.Add((eLocateBy)v);
            }
            return l;

        }


        // Page will be better (compile check) but since the pages are in Ginger we cannot ref them in Act
        // So, meanwhile we can create by page string name

        [DoNotBackup]
        public abstract string ActionEditPage { get; }

        //Action HL description which will be shown in the Add/Edit action window
        [DoNotBackup]
        public abstract String ActionUserDescription { get; }

        //Action HL description which will be shown in the Add/Edit action window to help the user understand what he can do with the action
        // TBH enable to format the text, headers images etc.
        [DoNotBackup]
        public abstract void ActionUserRecommendedUseCase(ITextBoxFormatter TBH);

        // No need to serialize
        public int RetryMechanismCount { get; set; }

        public long ElapsedTicks { get; set; }

        [DoNotBackup]
        public Single? ElapsedSecs
        {
            get
            {
                if (Elapsed != null)
                {
                    return ((Single)Elapsed / 1000);
                }
                else
                {
                    return null;
                }
            }
        }

        public string SolutionFolder { set; get; }

        private string mError;


        //private bool? mIsSingleAction;

        private string mExInfo;

        public string ExInfo
        {
            get
            {
                return mExInfo;
            }
            set
            {
                if (!string.IsNullOrEmpty(mExInfo) && value != null && value.Contains(mExInfo) && value.IndexOf(mExInfo) == 0)//meaning act.ExInfo += was used
                {
                    //add line break
                    mExInfo = string.Format("{0}{1}{2}", value[..mExInfo.Length], Environment.NewLine, value[mExInfo.Length..]);
                }
                else
                {
                    mExInfo = value;
                }
                OnPropertyChanged(Fields.ExInfo);
            }
        }

        [DoNotBackup]
        public string ActClass { get { return this.GetType().ToString(); } }


        //Keeping screen shot in memory will eat up the memory - so we save to files and keep file name

        public ObservableList<String> ScreenShots { get; set; } = [];
        public ObservableList<String> ScreenShotsNames = [];

        public ObservableList<ArtifactDetails> Artifacts { get; set; } = [];

        // No need to back because the list is saved to backup
        [DoNotBackup]
        public string Value
        {
            get
            {
                return GetInputParamValue("Value");
            }
            set
            {
                AddOrUpdateInputParamValue("Value", value);
            }
        }

        [DoNotBackup]
        public string ValueForDriver
        {
            get
            {
                return GetInputParamCalculatedValue("Value");
            }
            set
            {
                AddOrUpdateInputParamCalculatedValue("Value", value);
            }
        }


        public ObservableList<ActInputValue> ActInputValues
        {
            get
            {
                return InputValues;
            }
        }

        public ObservableList<ActReturnValue> ActReturnValues
        {
            get
            {
                return ReturnValues;
            }
        }

        public ObservableList<ActOutDataSourceConfig> ActOutDSConfigParams
        {
            get
            {
                return DSOutputConfigParams;
            }
        }


        public ObservableList<FlowControl> ActFlowControls
        {
            get
            {
                return FlowControls;
            }
        }


        #region ActInputValues
        public void AddInputValueParam(string ParamName)
        {
            // check if param already exist then update as it can be saved and loaded + keep other values            
            ActInputValue AIV = InputValues.FirstOrDefault(aiv => aiv.Param == ParamName);
            if (AIV == null)
            {
                AIV = new ActInputValue
                {
                    Param = ParamName
                };
                InputValues.Add(AIV);
                AIV.Value = "";
            }
        }

        public void RemoveInputParam(string ParamName)
        {
            InputValues.Remove(InputValues.FirstOrDefault(aiv => aiv.Param == ParamName));
        }
        public void AddOrUpdateOutDataSourceParam(string DSName, string DSTable, string OutputType, string ColName = "", string Active = "", List<string> mColNames = null, string OutDSParamType = "ParamToRow")
        {
            bool isActive = true;
            // check if param already exist then update as it can be saved and loaded + keep other values
            ActOutDataSourceConfig ADCS = DSOutputConfigParams
                .FirstOrDefault(param =>
                    string.Equals(param.DSName, DSName) &&
                    string.Equals(param.DSTable, DSTable) &&
                    string.Equals(param.OutputType, OutputType));

            if (ADCS == null)
            {
                if (OutputType == "Parameter_Path" || OutDSParamType != "ParamToRow")
                {
                    isActive = false;
                }
            }
            else
            {
                if (mColNames == null && Active != "")
                {
                    ADCS.Active = bool.Parse(Active);
                    return;
                }
                isActive = ADCS.Active;
                ColName = ADCS.TableColumn;
                if (mColNames != null && !mColNames.Contains(ColName))
                {
                    ColName = OutputType;
                }
                DSOutputConfigParams.Remove(ADCS);
            }

            ADCS = new ActOutDataSourceConfig
            {
                DSName = DSName,
                DSTable = DSTable
            };
            ADCS.PossibleValues.Add(ColName);
            ADCS.StartDirtyTracking();
            ADCS.OnDirtyStatusChanged += this.RaiseDirtyChanged;
            DSOutputConfigParams.Add(ADCS);
            ADCS.OutputType = OutputType;
            ADCS.OutParamMap = OutDSParamType;
            if (mColNames != null)
            {
                foreach (string sCol in mColNames)
                {
                    if (sCol != ColName)
                    {
                        ADCS.PossibleValues.Add(sCol);
                    }
                }
            }

            ADCS.Active = isActive;

            if (ColName != "")
            {
                ADCS.TableColumn = ColName;
            }
        }

        public void RemoveAllButOneInputParam(string Param)
        {
            if (!InputValues.Any(aiv => aiv.Param == Param))
            {
                InputValues.Clear();
            }
            else
            {
                while (InputValues.Count > 1)
                {
                    InputValues.Remove(InputValues.FirstOrDefault(aiv => aiv.Param != Param));
                }
            }
        }

        public void AddOrUpdateInputParamValue(string Param, string Value)
        {
            // check if param already exist then update as it can be saved and loaded + keep other values
            ActInputValue AIV = InputValues.FirstOrDefault(aiv => aiv.Param == Param);
            if (AIV == null)
            {
                AIV = new ActInputValue
                {
                    Param = Param
                };
                InputValues.Add(AIV);
                AIV.StartDirtyTracking();
                AIV.OnDirtyStatusChanged += this.RaiseDirtyChanged;
            }
            else
            {
                //Remove duplicate ActInputValues from the InputValues                
                while (InputValues.Count(aiv => aiv.Param == Param) > 1)
                {
                    InputValues.Remove(InputValues.LastOrDefault(aiv => aiv.Param == Param));
                }
            }
            AIV.Value = Value;
        }

        public string GetInputParamValue(string Param)
        {
            // check if param already exist then update as it can be saved and loaded + keep other values
            ActInputValue AIV = InputValues.FirstOrDefault(aiv => aiv != null && aiv.Param == Param);
            if (AIV == null)
            {
                return null;
            }

            return AIV.Value;
        }



        public ActInputValue GetOrCreateInputParam(string Param, string DefaultValue = null)
        {
            ActInputValue AIV = InputValues.FirstOrDefault(aiv => aiv != null && aiv.Param == Param);
            if (AIV == null)
            {
                AIV = new ActInputValue() { Param = Param, Value = DefaultValue };
                InputValues.Add(AIV);
                AIV.StartDirtyTracking();
                AIV.OnDirtyStatusChanged += this.RaiseDirtyChanged;
            }
            return AIV;
        }

        public TEnum GetOrCreateInputParam<TEnum>(string Param, TEnum DefaultValue) where TEnum : struct
        {

            ActInputValue AIV = GetOrCreateInputParam(Param, DefaultValue.ToString());

            TEnum result;
            _ = Enum.TryParse<TEnum>(AIV.Value, out result);

            return result;

        }

        //YW - removed as it was causing problem - need to rethink better.
        //public ActInputValueEnum GetOrCreateEnumInputParam(string Param, object DefaultValue = null)
        //{
        //    ActInputValueEnum AIV = (ActInputValueEnum)InputValues.FirstOrDefault(aiv=> aiv.Param == Param);
        //    if (AIV == null)
        //    {
        //        AIV = new ActInputValueEnum() { Param = Param, EnumValue = DefaultValue };                
        //        InputValues.Add(AIV);
        //    }
        //    return (ActInputValueEnum)AIV;
        //}


        public void AddOrUpdateInputParamCalculatedValue(string Param, string calculatedValue)
        {
            // check if param already exist then update as it can be saved and loaded + keep other values
            ActInputValue AIV = InputValues.FirstOrDefault(aiv => aiv.Param == Param);
            if (AIV == null)
            {
                AIV = new ActInputValue
                {
                    Param = Param
                };
                InputValues.Add(AIV);
            }

            AIV.ValueForDriver = calculatedValue;
        }

        public void AddOrUpdateInputParamValueAndCalculatedValue(string Param, string calculatedValue)
        {
            // check if param already exist then update as it can be saved and loaded + keep other values
            ActInputValue AIV = InputValues.FirstOrDefault(aiv => aiv.Param == Param);
            if (AIV == null)
            {
                AIV = new ActInputValue
                {
                    Param = Param
                };
                InputValues.Add(AIV);
            }

            AIV.Value = calculatedValue;
            AIV.ValueForDriver = calculatedValue;
        }

        public string GetInputParamCalculatedValue(string Param, bool decryptValue = true)
        {
            // check if param already exist then update as it can be saved and loaded + keep other values
            ActInputValue AIV = InputValues.FirstOrDefault(aiv => aiv != null && aiv.Param == Param);
            if (AIV == null)
            {
                return null;
            }

            /*
            if (decryptValue == true)
            {
                String strValuetoPass = EncryptionHandler.DecryptString(AIV.ValueForDriver, ref res);

                if (res == true) return strValuetoPass;

            } */

            return AIV.ValueForDriver;
        }


        /// <summary>
        /// Pulling the params corresponding calculated value from the ReturnParams list and parsing the Caluculatedvalue into the specified type. 
        /// </summary>
        /// <param name="Param">The name of the Parameter from the Return Values List</param>
        /// <param name="T">Is the required type of return parameter value (int Enum string etc.)</param>
        /// <returns>Object which contains object of the requested type</returns>
        public object GetInputParamCalculatedValue<T>(string Param)
        {
            string calculatedValue = GetInputParamCalculatedValue(Param);

            return GetParsedValue<T>(calculatedValue);


        }   // end of GetInputParamCalculatedValue


        public object GetInputParamValue<T>(string Param)
        {
            string paramValue = GetInputParamValue(Param);

            return GetParsedValue<T>(paramValue);
        }


        private object GetParsedValue<T>(string paramValue)
        {
            if (typeof(T) == typeof(int))
            {
                int value = 0;
                return int.TryParse(paramValue, out value) ? value : 0;
            }
            if (typeof(T) == typeof(bool))
            {
                bool value = false;
                if (Boolean.TryParse(paramValue, out value))
                {
                    return value;
                }
                else
                {
                    return false; // default value
                }
            }
            if (typeof(T) == typeof(string))
            {
                return string.IsNullOrEmpty(paramValue) ? string.Empty : paramValue;
            }
            if (typeof(T).BaseType == typeof(Enum))
            {
                try
                {
                    return (T)Enum.Parse(typeof(T), paramValue, true);
                }
                catch (Exception ex)
                {
                    Array a = Enum.GetValues(typeof(T));
                    Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
                    return a.GetValue(0).ToString();
                }
            }

            return null; // Type is not supported
        }


        //TODO: make Name a mandatory and enforce providing where used, meanwhile making it optional
        public void AddScreenShot(Bitmap bmp, string Name = "")
        {
            try
            {
                using (bmp)
                {
                    ScreenShots.Add(SaveScreenshotToTempFile(bmp));
                    ScreenShotsNames.Add(Name);
                }
            }
            catch (Exception ex)
            {
                Error = "Failed to save the screenshot bitmap to temp file. Error= " + ex.Message;
                Reporter.ToLog(eLogLevel.ERROR, Error, ex);
            }
        }

        public void AddScreenShot(string Base64String, string Name = "")
        {
            byte[] bytes = Convert.FromBase64String(Base64String);
            try
            {

                string filePath = GetScreenShotRandomFileName();
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    fs.Write(bytes, 0, bytes.Length);
                }
                ScreenShots.Add(filePath);
                ScreenShotsNames.Add(Name);

            }
            catch (Exception ex)
            {
                Error = "Failed to save the screenshot bitmap to temp file. Error= " + ex.Message;
                Reporter.ToLog(eLogLevel.ERROR, Error, ex);
            }
            finally
            {
                if (bytes != null)
                {
                    Array.Clear(bytes);
                }

            }
        }

        public void AddScreenShot(byte[] bytes, string Name)
        {
            try
            {
                string filePath = GetScreenShotRandomFileName();
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite))
                {
                    fs.Write(bytes, 0, bytes.Length);
                }
                ScreenShots.Add(filePath);
                ScreenShotsNames.Add(Name);
            }
            catch (Exception ex)
            {
                Error += "Unable to add Screen shot " + ex.Message;
            }
            finally
            {
                if (bytes != null)
                {
                    Array.Clear(bytes);
                }
            }
        }



        // TODO: move to Utils
        public static string SaveScreenshotToTempFile(Bitmap screenshot)
        {
            using (screenshot)
            {
                string filePath = GetScreenShotRandomFileName();
                screenshot.Save(filePath);
                return filePath;
            }
        }

        public static string GetScreenShotRandomFileName()
        {
            string filename = Path.GetRandomFileName().Split('.')[0] + ".png";
            string filePath = Path.Combine(ScreenshotTempFolder, filename);
            if (!Directory.Exists(ScreenshotTempFolder))
            {
                Directory.CreateDirectory(ScreenshotTempFolder);
            }
            return filePath;
        }

        public static void AddArtifactToAction(string artifactName, Act action, string artifactPath)
        {
            string extension = Path.GetExtension(artifactPath);
            string artifactOriginalName = artifactName.Length <= 15 ? artifactName : artifactName[..15];
            var randomFileName = string.Concat(artifactOriginalName, "_", action.Guid, "_", DateTime.UtcNow.ToString("hhmmss.fff"), "_", extension);

            ArtifactDetails artifact = new ArtifactDetails
            {
                ArtifactOriginalName = artifactName,
                ArtifactOriginalPath = Path.GetFullPath(artifactPath),
                ArtifactReportStoragePath = Path.GetFullPath(artifactPath),
                ArtifactReportStorageName = randomFileName
            };
            action.Artifacts.Add(artifact);
        }

        public override string GetNameForFileName()
        {
            //TODO: replace name with a unique ID?
            //TODO: To add Action.Name to the file name
            string fn = Description;// + ActionName + ": " + mLocateBy + "=" + LocateValue + " value=" + InputValues.FirstOrDefault(); ; 
            if (fn != null)
            {
                if (fn.Length > 100)
                {
                    return fn[..99];
                }
                else
                {
                    return fn;
                }
            }
            return ItemName;
        }

        #endregion ActInputValues
        public void AddOrUpdateReturnParsedParamValue(List<KeyValuePair<string, string>> list)
        {

            foreach (KeyValuePair<string, string> outputValuePair in list)
            {
                AddOrUpdateReturnParamActual(outputValuePair.Key, outputValuePair.Value);
            }
        }

        public void ParseJSONToOutputValues(string ResponseMessage, int i)// added i especially for cassandra, for retrieving path , other cases give i=1
        {
            Dictionary<string, object> outputValues = General.DeserializeJson(ResponseMessage);
            foreach (KeyValuePair<string, object> entry in outputValues)
            {
                AddJsonKeyValueToOutputValue(entry.Value, entry.Key, i);
            }
        }

        public void AddToOutputValues(Dictionary<string, object> keyValuePairs)
        {
            foreach (KeyValuePair<string, object> keyValuePair in keyValuePairs)
            {
                AddJsonKeyValueToOutputValue(keyValuePair.Value, keyValuePair.Key, 1);
            }
        }

        private void AddJsonKeyValueToOutputValue(object Value, string Key, int Path)
        {
            try
            {
                if (Value is Dictionary<string, object>)
                {
                    foreach (KeyValuePair<string, object> entry in (Dictionary<string, object>)Value)
                    {
                        AddJsonKeyValueToOutputValue(entry.Value, Key + "." + entry.Key, Path);
                    }
                }
                else
                 if (Value is ICollection)
                {
                    // Checks whether the current Table-Value in Table is Sub-Dictionary|Collection|Flat Object
                    if (Value is ArrayList)
                    {
                        int k = ((System.Collections.ArrayList)Value).Count;
                        AddOrUpdateReturnParamActualWithPath(Key, k.ToString(), Path.ToString());
                        int j = 1;
                        // If the table base value is a collection of items then loop through them
                        foreach (var item in (ICollection)Value)
                        {
                            AddJsonKeyValueToOutputValue(item, Key + "[" + j + "]", Path);
                            j++;
                        }
                    }
                    else
                    {
                        if (Value is IList)
                        {
                            try
                            {
                                if (Value is Newtonsoft.Json.Linq.JObject)
                                {
                                    int k = ((Newtonsoft.Json.Linq.JContainer)Value).Count;
                                    AddOrUpdateReturnParamActualWithPath(Key.ToString(), k.ToString(), Path.ToString());
                                    int j = 1;

                                    foreach (IList item in (IList)Value)
                                    {
                                        AddJsonKeyValueToOutputValue(((Newtonsoft.Json.Linq.JProperty)item).Value, Key + "." + ((Newtonsoft.Json.Linq.JProperty)item).Name, Path);
                                        j++;
                                    }
                                    k++;
                                }
                                else if (Value is Newtonsoft.Json.Linq.JArray)
                                {
                                    int k = ((Newtonsoft.Json.Linq.JContainer)Value).Count;
                                    AddOrUpdateReturnParamActualWithPath(Key.ToString(), k.ToString(), Path.ToString());
                                    int j = 1;
                                    foreach (var a in (Newtonsoft.Json.Linq.JArray)Value)
                                    {
                                        AddJsonKeyValueToOutputValue(a, Key + "[" + j + "]", Path);
                                        j++;
                                    }
                                    k++;
                                }
                            }
                            catch (Exception ex)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.StackTrace}", ex);
                            }
                        }
                    }
                }
                else
                {
                    try
                    {
                        if (Value != null)
                        {
                            AddOrUpdateReturnParamActualWithPath(Key, Value.ToString(), Path.ToString());
                        }
                        else
                        {
                            AddOrUpdateReturnParamActualWithPath(Key, null, Path.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.StackTrace}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.StackTrace}", ex);
            }
        }   // end of AddJsonKeyValueToOutputValue



        public bool IsInputParamExist(string Param)
        {
            ActInputValue AIV = InputValues.FirstOrDefault(aiv => aiv.Param == Param);
            if (AIV != null)
            {
                return true;
            }
            return false;
        }

        [IsSerializedForLocalRepository]
        public ObservableList<Guid> Tags = [];

        public override bool FilterBy(eFilterBy filterType, object obj)
        {
            switch (filterType)
            {
                case eFilterBy.Tags:
                    foreach (Guid tagGuid in Tags)
                    {
                        Guid guid = ((List<Guid>)obj).FirstOrDefault(x => tagGuid.Equals(x));
                        if (!guid.Equals(Guid.Empty))
                        {
                            return true;
                        }
                    }
                    break;
            }
            return false;
        }

        public void AddOrUpdateReturnParamActual(string ParamName, string ActualValue, string ExpectedValue = "dummy")
        {
            // check if param already exist then update as it can be saved and loaded + keep other values
            ActReturnValue ARC = ReturnValues.FirstOrDefault(arc => arc.ParamCalculated == ParamName);
            if (ARC == null && (AddNewReturnParams == true || ConfigOutputDS == true))
            {
                ARC = new ActReturnValue
                {
                    Operator = Amdocs.Ginger.Common.Expressions.eOperator.Equals,
                    Active = true
                };
                ReturnValues.Add(ARC);
                ARC.Param = ParamName;
            }

            if (ARC != null)
            {
                ARC.Actual = ActualValue;
                if (string.IsNullOrEmpty(ExpectedValue))
                {
                    ARC.Operator = Amdocs.Ginger.Common.Expressions.eOperator.Equals;
                }
                if (!ExpectedValue.Equals("dummy"))
                {
                    ARC.Expected = ExpectedValue;
                }
            }
        } // end of AddOrUpdateReturnParamActual


        public void AddOrUpdateReturnParamExpected(string ParamName, string ExpectedValue)
        {
            // check if param already exist then update as it can be saved and loaded + keep other values
            ActReturnValue ARC = ReturnValues.FirstOrDefault(arc => arc.Param == ParamName);
            if (ARC == null && (AddNewReturnParams == true || ConfigOutputDS == true))
            {
                ARC = new ActReturnValue
                {
                    Active = true
                };
                ReturnValues.Add(ARC);
                ARC.Param = ParamName;
            }

            if (ARC != null)
            {
                ARC.Expected = ExpectedValue;
            }
        } // end of AddOrUpdateReturnParamExpected


        public void AddOrUpdateReturnParamActualWithPath(string ParamName, string ActualValue, string sPath)
        {
            // check if param already exist then update as it can be saved and loaded + keep other values
            if (sPath == null)
            {
                sPath = string.Empty;
            }
            ActReturnValue ARC = ReturnValues.FirstOrDefault(arc => arc.ParamCalculated == ParamName && arc.PathCalculated == sPath);
            if (ARC == null && (AddNewReturnParams == true || ConfigOutputDS == true))
            {
                ARC = new ActReturnValue
                {
                    Operator = Amdocs.Ginger.Common.Expressions.eOperator.Equals,
                    Active = true
                };
                ReturnValues.Add(ARC);
                ARC.Param = ParamName;
                ARC.Path = sPath;
            }

            if (ARC != null)
            {
                if (string.IsNullOrEmpty(ARC.Expected))
                {
                    ARC.Operator = Amdocs.Ginger.Common.Expressions.eOperator.Equals;
                }
                ARC.Actual = ActualValue;
            }
        }


        public string GetReturnParam(string Param)
        {
            // check if param already exist then update as it can be saved and loaded + keep other values
            ActReturnValue ARC = ReturnValues.FirstOrDefault(arc => arc.Param == Param);
            if (ARC == null)
            {
                return null;
            }

            return ARC.Actual;
        }

        public int GetReturnParamCount(string Param)
        {
            // check if param already exist and return the amount of instances of that param in the ReturnValues list
            int returnParamCount = ReturnValues.Count(arc => arc.Param.Equals(Param));

            return returnParamCount;
        }

        public string GetDataSourceConfigParam(string OutputParam)
        {
            // check if param already exist then update as it can be saved and loaded + keep other values
            ActOutDataSourceConfig ADSC = DSOutputConfigParams.FirstOrDefault(arc => arc.OutputType == OutputParam);
            if (ADSC == null)
            {
                return null;
            }

            return ADSC.TableColumn;
        }

        public string GetCalculatedExpectedParam(string Param)
        {
            // check if param already exist then update as it can be saved and loaded + keep other values
            ActReturnValue ARC = ReturnValues.FirstOrDefault(arc => arc.Param == Param);
            if (ARC == null)
            {
                return null;
            }

            return ARC.ExpectedCalculated;
        }
        //public string GetStoredVariableParam(string Param)
        //{
        //    // check if param already exist then update as it can be saved and loaded + keep other values
        //    ActReturnValue ARC = (from arc in ReturnValues where arc.Param == Param select arc).FirstOrDefault();
        //    if (ARC == null)
        //    {
        //        return null;
        //    }

        //    return ARC.StoreToVariable;
        //}

        //public string GetStoredDataSourceParam(string Param)
        //{
        //    // check if param already exist then update as it can be saved and loaded + keep other values
        //    ActReturnValue ARC = (from arc in ReturnValues where arc.Param == Param select arc).FirstOrDefault();
        //    if (ARC == null)
        //    {
        //        return null;
        //    }

        //    return ARC.StoreToDataSource;
        //}

        public string GetStoreToValueParam(string Param)
        {
            // check if param already exist then update as it can be saved and loaded + keep other values
            ActReturnValue ARC = ReturnValues.FirstOrDefault(arc => arc.Param == Param);
            if (ARC == null)
            {
                return null;
            }

            return ARC.StoreToValue;
        }

        protected void AddAllPlatforms()
        {
            lock (mPlatforms)   // Handle reentry 
            {
                if (mPlatforms.Count != 0)
                {
                    return;
                }

                foreach (object v in Enum.GetValues(typeof(ePlatformType)))
                {
                    mPlatforms.Add((ePlatformType)v);
                }
            }
        }

        /// <summary>
        ///  this function is called after the action was executed by the driver
        ///  Derived classed can override to handle special treatment like: ACTSceenShot, save to file
        /// </summary>
        public virtual void PostExecute()
        {
            // Base - do nothing
        }

        private string mSupportedPlatforms = string.Empty;
        public string SupportedPlatforms
        {
            get
            {
                if (mSupportedPlatforms == string.Empty)
                {
                    mSupportedPlatforms = ConvertSupportedPlatformsToString(this);
                    return mSupportedPlatforms;
                }
                else
                {
                    return mSupportedPlatforms;
                }
            }
            set
            {
                mSupportedPlatforms = value;
            }
        }

        private string ConvertSupportedPlatformsToString(Act act)
        {
            string supportedPlatforms = string.Empty;
            foreach (ePlatformType actPlatform in act.Platforms)
            {
                supportedPlatforms += actPlatform.ToString() + ",";
            }

            if (supportedPlatforms.Contains("NA"))
            {
                supportedPlatforms = "All";//assumption is that if 'NA' is in the platforms list then all platforms are supported
            }
            else
            {
                supportedPlatforms = supportedPlatforms.TrimEnd(',');
            }
            return supportedPlatforms;
        }


        public void ActionDescriptionTextBlock(object ActionRecUseCaseTextBlock)
        {

            ITextBoxFormatter TBH = TargetFrameworkHelper.Helper.CreateTextBoxFormatter(ActionRecUseCaseTextBlock);

            TBH.AddHeader1("Description:");
            TBH.AddLineBreak();
            TBH.AddText(ActionUserDescription);
            TBH.AddLineBreak();
            TBH.AddLineBreak();

            TBH.AddHeader1("Supported Platforms:");
            TBH.AddLineBreak();
            TBH.AddText(SupportedPlatforms);
            TBH.AddLineBreak();
            TBH.AddLineBreak();

            TBH.AddHeader1("Recommended Use Case/s and Guidelines:");
            TBH.AddLineBreak();
            // Let the action return the full help info which can be formatted using TBH
            // Free text from here 
            ActionUserRecommendedUseCase(TBH);
        }



        /// <summary>
        /// Check if the action supposed to be executed according to it variables dependencies configurations
        /// </summary>
        /// <param name="parentActivity">The Action parent Activity</param>  
        /// <param name="setActStatus">Define of to set the Action Status value in case the check fails</param>   
        /// <returns></returns>
        public bool? CheckIfVaribalesDependenciesAllowsToRun(Activity parentActivity, bool setActStatus = false)
        {
            bool? checkStatus = null;
            try
            {
                //check objects are valid
                if (parentActivity != null)
                {
                    //check if the Action-variables dependencies mechanism is enabled
                    if (parentActivity.EnableActionsVariablesDependenciesControl)
                    {
                        //check if the action configured to run with all activity selection list variables selected value
                        List<VariableBase> activityListVars = parentActivity.Variables.Where(v => v.GetType() == typeof(VariableSelectionList) && v.Value != null).ToList();
                        if (activityListVars != null && activityListVars.Count > 0)
                        {
                            foreach (VariableBase listVar in activityListVars)
                            {
                                VariableDependency varDep = null;
                                if (this.VariablesDependencies != null)
                                {
                                    varDep = VariablesDependencies.FirstOrDefault(avd => avd.VariableName == listVar.Name && avd.VariableGuid == listVar.Guid);
                                }
                                if (varDep == null)
                                {
                                    varDep = VariablesDependencies.FirstOrDefault(avd => avd.VariableGuid == listVar.Guid);
                                }
                                if (varDep != null)
                                {
                                    if (!varDep.VariableValues.Contains(listVar.Value))
                                    {
                                        checkStatus = false;//the Selection List variable selected Value was not configured on the action
                                        break;
                                    }
                                }
                                else
                                {
                                    checkStatus = false;//the Selection List variable was not configured on the action
                                    break;
                                }
                            }
                            if (checkStatus == null)
                            {
                                checkStatus = true;//All Selection List variable selected values were configured on the action
                            }
                        }
                        else
                        {
                            checkStatus = true;//the Activity don't has Selection List variables
                        }
                    }
                    else
                    {
                        checkStatus = true;//the mechanism is disabled
                    }
                }
                else
                {
                    checkStatus = false; //Activity object is null
                }

                //Check failed
                if (checkStatus == false && setActStatus)
                {
                    this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                    this.ExInfo = "Action was not configured to run with current " + GingerDicser.GetTermResValue(eTermResKey.Activity) + " " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " values.";
                }

                return checkStatus;
            }
            catch (Exception ex)
            {
                //Check failed
                if (setActStatus)
                {
                    this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Skipped;
                    this.Error = "Error occurred while checking the Action " + GingerDicser.GetTermResValue(eTermResKey.Variables) + " dependencies. Details: " + ex.Message;
                }
                return false;
            }
        } // end of CheckIfVaribalesDependenciesAllowsToRun



        public void InvokPropertyChanngedForAllFields()
        {
            foreach (var field in typeof(Fields).GetFields())
            {
                OnPropertyChanged(field.Name);
            }
        }

        public virtual List<ObservableList<ActInputValue>> GetInputValueListForVEProcessing()
        {
            //Ginger Runner call this func in prep time, each action can impl if needed

            // will be override in derived class if needed otherwise return null
            // use in Tuxedo to process the UD file params vals
            return null;
        }

        public ActReturnValue GetReturnValue(string ParamName)
        {
            ActReturnValue RC = ReturnValues.FirstOrDefault(x => x.Param == ParamName);
            return RC;
        }

        public void ClearUnUsedReturnParams()
        {
            List<ActReturnValue> arvsToRemove = ReturnValues.Where(x => string.IsNullOrEmpty(x.Expected) && string.IsNullOrEmpty(x.SimulatedActual) && string.IsNullOrEmpty(x.StoreToValue)).ToList();
            foreach (ActReturnValue arv in arvsToRemove)
            {
                ReturnValues.Remove(arv);
            }
        }


        public bool? AddNewReturnParams { get; set; }

        public override string ItemName
        {
            get
            {
                return this.Description;
            }
            set
            {
                this.Description = value;
            }
        }

        public override void UpdateInstance(RepositoryItemBase instance, string partToUpdate, RepositoryItemBase hostItem = null, object extraDetails = null)
        {
            Act actInstance = (Act)instance;

            //Create new instance of source
            Act newInstance = (Act)this.CreateInstance();
            newInstance.IsSharedRepositoryInstance = true;

            //update required part
            Act.eItemParts ePartToUpdate = (Act.eItemParts)Enum.Parse(typeof(Act.eItemParts), partToUpdate);
            switch (ePartToUpdate)
            {
                case eItemParts.All:
                case eItemParts.Details:
                    newInstance.Guid = actInstance.Guid;
                    newInstance.ParentGuid = actInstance.ParentGuid;
                    newInstance.ExternalID = actInstance.ExternalID;
                    newInstance.Active = actInstance.Active;
                    newInstance.VariablesDependencies = actInstance.VariablesDependencies;
                    if (ePartToUpdate == eItemParts.Details)
                    {
                        //keep other parts
                        newInstance.FlowControls = actInstance.FlowControls;
                        newInstance.InputValues = actInstance.InputValues;
                        newInstance.ReturnValues = actInstance.ReturnValues;
                    }
                    if (hostItem != null)
                    {
                        //replace old instance object with new
                        int originalIndex = ((Activity)hostItem).Acts.IndexOf(actInstance);
                        ((Activity)hostItem).Acts.Remove(actInstance);
                        ((Activity)hostItem).Acts.Insert(originalIndex, newInstance);
                    }
                    break;
                case eItemParts.FlowControls:
                    actInstance.FlowControls = newInstance.FlowControls;
                    break;
                case eItemParts.InputValues:
                    actInstance.InputValues = newInstance.InputValues;
                    break;
                case eItemParts.ReturnValues:
                    actInstance.ReturnValues = newInstance.ReturnValues;
                    break;
                case eItemParts.DSOutputConfigParams:
                    actInstance.DSOutputConfigParams = newInstance.DSOutputConfigParams;
                    break;
            }
        }


        public override RepositoryItemBase GetUpdatedRepoItem(RepositoryItemBase itemToUpload, RepositoryItemBase exisstingRepoItem, string itemPartToUpdate)
        {
            Act updatedAct = null;

            eItemParts ePartToUpdate = (eItemParts)Enum.Parse(typeof(eItemParts), itemPartToUpdate);
            switch (ePartToUpdate)
            {
                case eItemParts.All:
                case eItemParts.Details:
                    updatedAct = (Act)itemToUpload.CreateCopy(false);
                    if (ePartToUpdate == eItemParts.Details)
                    {
                        //keep other parts
                        updatedAct.FlowControls = ((Act)exisstingRepoItem).FlowControls;
                        updatedAct.InputValues = ((Act)exisstingRepoItem).InputValues;
                        updatedAct.ReturnValues = ((Act)exisstingRepoItem).ReturnValues;
                    }
                    break;
                case eItemParts.FlowControls:
                    updatedAct = (Act)exisstingRepoItem.CreateCopy(false);

                    updatedAct.FlowControls = ((Act)itemToUpload).FlowControls;
                    break;
                case eItemParts.InputValues:
                    updatedAct = (Act)exisstingRepoItem.CreateCopy(false);
                    updatedAct.InputValues = ((Act)itemToUpload).InputValues;
                    break;
                case eItemParts.ReturnValues:
                    updatedAct = (Act)exisstingRepoItem.CreateCopy(false);
                    updatedAct.ReturnValues = ((Act)itemToUpload).ReturnValues;
                    break;
                case eItemParts.DSOutputConfigParams:
                    updatedAct = (Act)exisstingRepoItem.CreateCopy(false);
                    updatedAct.DSOutputConfigParams = ((Act)itemToUpload).DSOutputConfigParams;
                    break;
            }

            return updatedAct;
        }

        public void Reset(bool reSetActionErrorHandlerExecutionStatus = false, bool isActionDirtyTrackingPaused = false)
        {
            if (this != null)
            {
                /* added the flag in order to avoid resuming the dirty tracking when this method is being called after RunAction method from GingerExecutionEngine. commented this as it need further testing
                if (!isActionDirtyTrackingPaused)
                {
                    PauseDirtyTracking();
                }*/
                if (reSetActionErrorHandlerExecutionStatus)
                {
                    this.ErrorHandlerExecuted = false;
                }

                this.ExInfo = string.Empty;
                this.Error = null;
                this.Elapsed = null;
                foreach (string ss in this.ScreenShots)
                {
                    if (ss.ToUpper().Contains(ScreenshotTempFolder.ToUpper()))
                    {
                        if (System.IO.File.Exists(ss))
                        {
                            try
                            {
                                System.IO.File.Delete(ss);
                            }
                            catch (System.IO.IOException ex)
                            {
                                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}", ex);
                            }
                        }
                    }
                }
                this.ScreenShots.Clear();
                this.ScreenShotsNames.Clear();
                this.Artifacts.Clear();
                // remove return vals which don't have expected or store to var
                // it is not needed since it will return back after we get results
                // if i.e the SQL changed we want to reflect the latest changes and output what we got

                //for (int indx = 0; indx < this.ReturnValues.Count; indx++)
                //{
                //    ActReturnValue value = this.ReturnValues[indx];
                //    if ((String.IsNullOrEmpty(value.Expected) == false || String.IsNullOrEmpty(value.StoreToValue) == false || String.IsNullOrEmpty(value.SimulatedActual) == false))
                //    {
                //        //If outputvalue is configured then reset it
                //        value.Actual = null;
                //        value.ExpectedCalculated = null;
                //        value.Status = ActReturnValue.eStatus.Pending;
                //    }
                //    else
                //    {
                //        this.ReturnValues.RemoveAt(indx);
                //        indx--;
                //    }
                //}
                List<ActReturnValue> configuredReturnParamsList = this.ReturnValues.Where(x => !String.IsNullOrEmpty(x.Expected) || !String.IsNullOrEmpty(x.StoreToValue) || !String.IsNullOrEmpty(x.SimulatedActual)).ToList();
                if (this.ReturnValues.Count != 0)
                {
                    this.ReturnValues.Clear();
                }
                //Added in order to remove the "Raw Response Button" when reseting the excution details
                this.RawResponseValues = String.Empty;
                foreach (ActReturnValue returnValue in configuredReturnParamsList)
                {
                    returnValue.Actual = null;
                    returnValue.ExpectedCalculated = null;
                    returnValue.Status = ActReturnValue.eStatus.Pending;
                    this.ReturnValues.Add(returnValue);
                }

                //foreach (ActDataSourceConfig ADSC in this.DSOutputConfigParams)
                //{
                //    ARV.Actual = null;
                //    ARV.ExpectedCalculated = null;
                //    ARV.Status = ActReturnValue.eStatus.Pending;
                //}

                foreach (FlowControl FC in this.FlowControls)
                {
                    FC.Status = eStatus.Pending;
                }
                this.Status = Amdocs.Ginger.CoreNET.Execution.eRunStatus.Pending;
                /*  commented this as it need further testing
                 * if (!isActionDirtyTrackingPaused)
                {
                    ResumeDirtyTracking();
                }*/
            }
        }    // end of Reset


        public string ExecutionLogFolder { get; set; }

        internal void AddScreenShot(string FileName)
        {
            ScreenShots.Add(FileName);
        }

        internal string GetFileNameForScreenShot()
        {
            return ExecutionLogFolder + @"ScreenShot_" + ScreenShots.Count + 1;
        }


        // This function is used to copy basic Act data from one type of action to another 
        // Used for conversion fro example from ActGenElement --> ActUIElement
        public void CopyInfoFrom(Act act)
        {
            //TODO: go over all SerailzedFields and copy

            this.Description = act.Description;
            this.BreakPoint = act.BreakPoint;
            this.Active = act.Active;

            //this.LocateBy = act.LocateBy;     // The new action need to take it from old and decide to which attr - this attr will be removed from Act.cs in the future
            //this.LocateValue = act.LocateValue;  // The new action need to take it from old and decide to which attr - this attr will be removed from Act.cs in the future

            this.Wait = act.Wait;

            //zz
            //TODO: copy all other in/out

            // this.FlowControls =  // TODO: create copy 
            // this.InputValues
            // this.out
        }


        // Return details of the action for Actions grid and report 
        // below is default impl but each action can customize
        public virtual ActionDetails Details
        {
            get
            {
                // Make sure that each item displayed in the details have Propertychanged trigger for sync with UI


                // Show old LocateBy, LocateValue
                // TODO: remove when locate by removed from here
                ActionDetails AD = new ActionDetails();
                if (this.ObjectLocatorConfigsNeeded)
                {
                    AD.Info = this.LocateBy + "=" + this.LocateValue;
                }



                // TODO: we can also create Params for Driver list
                ObservableList<ActionParamInfo> l = [];
                foreach (ActInputValue AIV in this.InputValues)
                {
                    if (AIV != null && !string.IsNullOrEmpty(AIV.Value))
                    {
                        l.Add(new ActionParamInfo() { Param = AIV.Param, Value = AIV.Value, CalculatedValue = AIV.ValueForDriver });
                    }
                }
                AD.Params = l;

                return AD;
            }
        }


        public void ParseRC(string sRC)
        {
            string GingerRCStart = "~~~GINGER_RC_START~~~";
            string GingerRCEnd = "~~~GINGER_RC_END~~~";

            int i = sRC.IndexOf(GingerRCStart);
            int i2 = -1;
            if (i >= 0)
            {
                i2 = sRC.IndexOf(GingerRCEnd, i);
                if (i2 > 0)
                {
                    sRC = sRC.Substring(i + GingerRCStart.Length, i2 - i - GingerRCEnd.Length - 2);
                }
            }

            if (i >= 0 && i2 > 0)
            {
                string[] RCValues = sRC.Split('\n');
                foreach (string RCValue in RCValues)
                {
                    if (RCValue.Length > 0) // Ignore empty lines
                    {
                        string param;
                        string value;
                        i = RCValue.IndexOf('=');
                        if (i > 0)
                        {
                            param = RCValue[..i];
                            //the rest is the value
                            value = RCValue[(param.Length + 1)..];
                        }
                        else
                        {
                            // in case of bad RC not per Ginger style we show it as "?" with value
                            param = "???";
                            value = RCValue;
                        }
                        AddOrUpdateReturnParamActual(param, value);
                    }
                }
            }
            else
            {
                //No params found so return the full output
                AddOrUpdateReturnParamActual("???", sRC);
            }
        } // end of ParseRC



        public override eImageType ItemImageType
        {
            get
            {
                return eImageType.Action;
            }
        }

        public override string ItemNameField
        {
            get
            {
                return nameof(this.Description);
            }
        }

        public int ReturnValuesCount
        {
            get
            {
                if (ReturnValues != null)
                {
                    return ReturnValues.Count;
                }
                else
                {
                    return 0;
                }
            }
        }

        public string FlowControlsInfo
        {
            get
            {
                if (FlowControls != null && FlowControls.Count > 0)
                {
                    return string.Format("{0} Flow Controls Rules", FlowControls.Count);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public virtual void CalculateModelParameterExpectedValue(ActReturnValue aRC)
        {
            // do nothing, will be override in action which needs it like ActWebAPIModel
        }

        /// <summary>
        /// should be object from type 'Context' which should include in context objects
        /// </summary>
        public object Context { get; set; }

        /// <summary>
        /// ID which been provided for each execution instance on the Action
        /// </summary>
        public Guid ExecutionId { get; set; }

        public Guid ParentExecutionId { get; set; }
        public bool ErrorHandlerExecuted { get; set; }

        public virtual void DoNewActionSetup()
        {
            // Base - do nothing
        }

        public override void PrepareItemToBeCopied()
        {
            this.IsSharedRepositoryInstance = TargetFrameworkHelper.Helper?.IsSharedRepositoryItem(this) ?? false;
        }

        public override string GetItemType()
        {
            return "Action";
        }


        /// <summary>
        /// Compares the current Act instance with another Act instance to determine equality.
        /// </summary>
        /// <param name="other">The other Act instance to compare with.</param>
        /// <returns>True if the instances are equal; otherwise, false.</returns>
        public bool AreEqual(Act other)
        {
            if (other == null || this.ActInputValues.Count != other.ActInputValues.Count)
            {
                return false;
            }

            if (this.Description != other.Description ||
                   this.Platform != other.Platform)
            {
                return false;
            }

            for (int i = 0; i < this.ActInputValues.Count; i++)
            {
                if (!this.ActInputValues[i].AreEqual(other.ActInputValues[i]))
                {
                    return false;
                }
            }

            if (this is ActWebAPIModel thisAction && other is ActWebAPIModel otherAction)
            {
                if (thisAction.APIModelParamsValue.Count != otherAction.APIModelParamsValue.Count)
                {
                    return false;
                }
                for (int i = 0; i < thisAction.APIModelParamsValue.Count; i++)
                {
                    if (!thisAction.APIModelParamsValue[i].AreEqual(otherAction.APIModelParamsValue[i]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Compares the current Act instance with another object to determine equality.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns>True if the object is an Act instance and is equal to the current instance; otherwise, false.</returns>
        public bool AreEqual(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType())
            {
                return false;
            }

            return AreEqual(obj as Act);
        }

    }
}
