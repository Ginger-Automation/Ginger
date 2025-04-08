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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.GeneralLib;
using Amdocs.Ginger.Common.SourceControlLib;
using Amdocs.Ginger.Common.Telemetry;
using Amdocs.Ginger.Repository;
using Ginger.UserConfig;
using GingerCoreNET.ALMLib;
using GingerCoreNET.SourceControl;
using Newtonsoft.Json.Linq;

namespace Ginger
{
    public enum eGingerStatus
    {
        Closed, Active, AutomaticallyClosed
    }

    public enum eUserType
    {
        Regular,
        Business
    }

    public enum eUserRole
    {
        None,
        [EnumValueDescription("Product Owner")]
        ProductOwner,
        [EnumValueDescription("Scrum Master")]
        ScrumMaster,
        Developer,
        Tester,
        [EnumValueDescription("Technical Writer")]
        TechnicalWriter,
        [EnumValueDescription("User Experience")]
        UserExperience,
        [EnumValueDescription("Release Manager")]
        ReleaseManager,
        [EnumValueDescription("SW Architect")]
        SWArchitect,
        [EnumValueDescription("RM Scoping")]
        RMScoping,
        [EnumValueDescription("Service Partner")]
        ServicePartner,
        PMO,
        [EnumValueDescription("Dev Manager")]
        DevManager,
        [EnumValueDescription("Program Manager")]
        ProgramManager,
        Architect,
        [EnumValueDescription("Project Manager")]
        ProjectManager,
        [EnumValueDescription("Testing Manager")]
        TestingManager,
        [EnumValueDescription("Dev Expert")]
        DevExpert
    }

    public class UserProfile : RepositoryItemBase
    {
        public IUserProfileOperations UserProfileOperations;
        //Move it to UCGridLib
        public class UserProfileGrid
        {
            public string GridId { get; set; }
            public List<UserProfileGridCol> Cols = [];
        }

        public class UserProfileGridCol
        {
            public string Name { get; set; }
            public int Width { get; set; }
        }

        public List<UserProfileGrid> Grids = [];

        bool mAutoLoadLastSolution;
        [IsSerializedForLocalRepository]
        public bool AutoLoadLastSolution
        {
            get
            {
                return mAutoLoadLastSolution;
            }
            set
            {
                if (mAutoLoadLastSolution != value)
                {
                    mAutoLoadLastSolution = value;
                    OnPropertyChanged(nameof(AutoLoadLastSolution));
                }
            }
        }

        private bool mShowSourceControlStatusIcon;
        [IsSerializedForLocalRepository]
        public bool ShowSourceControlStatusIcon
        {
            get => mShowSourceControlStatusIcon;
            set
            {
                if (mShowSourceControlStatusIcon != value)
                {
                    mShowSourceControlStatusIcon = value;
                    OnPropertyChanged(nameof(ShowSourceControlStatusIcon));
                }
            }
        }

        private bool mWatchFileChanges = true;
        [IsSerializedForLocalRepository(true)]
        public bool WatchFileChanges
        {
            get => mWatchFileChanges;
            set
            {
                if (mWatchFileChanges != value)
                {
                    mWatchFileChanges = value;
                    OnPropertyChanged(nameof(mWatchFileChanges));
                }
            }
        }

        public bool IsSharedUserProfile { get { return UserProfileOperations.IsSharedUserProfile; } }

        [IsSerializedForLocalRepository]
        public eGingerStatus GingerStatus { get; set; }

        // Keep the folder names of last solutions opened
        [IsSerializedForLocalRepository]
        public List<string> RecentSolutions = [];

        [IsSerializedForLocalRepository]
        public List<string> RecentAppAgentsMapping = [];

        //[IsSerializedForLocalRepository]
        /// <summary>
        /// Not needed anymore keeping for allowing to load UserProfile xml
        /// </summary>
        //public Guid RecentBusinessFlow { get; set; }

        [IsSerializedForLocalRepository]
        public Guid RecentEnvironment { get; set; }

        [IsSerializedForLocalRepository]
        public Guid RecentRunset { get; set; }



        public SourceControlBase.eSourceControlType Type { get; set; }

        public string URL { get; set; }

        public string Username { get; set; }


        public string Branch { get; set; }

        public bool IsProxyConfigured { get; set; }

        public string ProxyAddress { get; set; }

        public string AuthorName { get; set; }

        public string AuthorEmail { get; set; }

        public string ProxyPort { get; set; }


        public int Timeout { get; set; }


        public string LocalFolderPath { get; set; }





        public string Password
        {
            get; set;
        }


        public string EncryptedPassword { get; set; }

        public bool DoNotSaveCredentialsOnUserProfile { get; set; }

        [IsSerializedForLocalRepository]
        public string ReportTemplateName { get; set; }

        bool mAutoRunAutomatePageAnalyzer = true;
        [IsSerializedForLocalRepository(true)]
        public bool AutoRunAutomatePageAnalyzer
        {
            get
            {
                return mAutoRunAutomatePageAnalyzer;
            }
            set
            {
                if (mAutoRunAutomatePageAnalyzer || value)
                {
                    mAutoRunAutomatePageAnalyzer = value;
                    OnPropertyChanged(nameof(AutoRunAutomatePageAnalyzer));
                }
            }
        }

        bool mAutoGenerateAutomatePageReport;
        [IsSerializedForLocalRepository]
        public bool AutoGenerateAutomatePageReport
        {
            get
            {
                return mAutoGenerateAutomatePageReport;
            }
            set
            {
                if (mAutoGenerateAutomatePageReport != value)
                {
                    mAutoGenerateAutomatePageReport = value;
                    OnPropertyChanged(nameof(AutoGenerateAutomatePageReport));
                }
            }
        }


        [IsSerializedForLocalRepository]
        public ObservableList<ALMUserConfig> ALMUserConfigs { get; set; } = [];

        GingerCore.eTerminologyType mTerminologyType;
        [IsSerializedForLocalRepository]
        public GingerCore.eTerminologyType TerminologyDictionaryType
        {
            get { return mTerminologyType; }
            set { if (mTerminologyType != value) { mTerminologyType = value; OnPropertyChanged(nameof(TerminologyDictionaryType)); } }
        }

        eAppReporterLoggingLevel mAppLogLevel;
        [IsSerializedForLocalRepository]
        public eAppReporterLoggingLevel AppLogLevel
        {
            get { return mAppLogLevel; }
            set { if (mAppLogLevel != value) { mAppLogLevel = value; Reporter.AppLoggingLevel = mAppLogLevel; OnPropertyChanged(nameof(AppLogLevel)); } }
        }

        eUserType mUserType;
        [IsSerializedForLocalRepository]
        public eUserType UserType
        {
            get
            {
                return mUserType;
            }
            set
            {
                if (mUserType != value)
                {
                    mUserType = value;
                    OnPropertyChanged(nameof(UserType));
                }
            }
        }

        private bool mEnableTelemetry;
        [IsSerializedForLocalRepository(DefaultValue: false)]
        public bool EnableTelemetry
        {
            get
            {
                return mEnableTelemetry;
            }
            set
            {
                mEnableTelemetry = value;
                OnPropertyChanged(nameof(EnableTelemetry));
            }
        }

        private ITelemetryQueueManager.Config mTelemetryConfig;
        [IsSerializedForLocalRepository]
        public ITelemetryQueueManager.Config TelemetryConfig
        {
            get
            {
                if (mTelemetryConfig == null)
                {
                    mTelemetryConfig = new();
                }
                return mTelemetryConfig;
            }
            set
            {
                mTelemetryConfig = value;
                OnPropertyChanged(nameof(TelemetryConfig));
            }
        }

        public UserTypeHelper UserTypeHelper { get; set; }

        public UserProfile LoadUserProfile()
        {
            if (General.isDesignMode()) return null;

            string InstallationConfigurationPath = Assembly.GetExecutingAssembly().Location.Replace(Path.GetFileName(Assembly.GetExecutingAssembly().Location), "Ginger.InstallationConfiguration.Json");

            string UserConfigJsonString = string.Empty;
            JObject UserConfigJsonObj = null;
            Dictionary<string, string> UserConfigdictObj = null;
            if (System.IO.File.Exists(InstallationConfigurationPath))
            {
                UserConfigJsonString = System.IO.File.ReadAllText(InstallationConfigurationPath);
                UserConfigJsonObj = JObject.Parse(UserConfigJsonString);
                UserConfigdictObj = UserConfigJsonObj.ToObject<Dictionary<string, string>>();
            }

            if (File.Exists(UserProfileOperations.UserProfileFilePath))
            {
                try
                {
                    DateTime UserProfileDT = File.GetLastWriteTime(UserProfileOperations.UserProfileFilePath);
                    Reporter.ToLog(eLogLevel.INFO, string.Format("Loading existing User Profile at '{0}'", UserProfileOperations.UserProfileFilePath));
                    string userProfileTxt = File.ReadAllText(UserProfileOperations.UserProfileFilePath);
                    UserProfile up = (UserProfile)NewRepositorySerializer.DeserializeFromText(userProfileTxt);
                    up.FilePath = UserProfileOperations.UserProfileFilePath;
                    if (UserConfigdictObj != null &&
                        DateTime.Compare(UserProfileDT, File.GetLastWriteTime(InstallationConfigurationPath)) < 0)
                    {
                        up.AddUserConfigProperties(UserConfigdictObj);
                    }
                    up = UserProfileOperations.LoadPasswords(up);
                    up.UserProfileOperations = UserProfileOperations;

                    return up;
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to load the existing User Profile at '{0}'", UserProfileOperations.UserProfileFilePath), ex);
                    try
                    {
                        //create backup to the user profile so user won't lose all of it configs in case he went back to old Ginger version
                        //TODO- allow recover from newer User Profile version in code instead creating new user profile
                        Reporter.ToLog(eLogLevel.INFO, "Creating backup copy for the User Profile file");
                        File.Copy(UserProfileOperations.UserProfileFilePath, UserProfileOperations.UserProfileFilePath.Replace("Ginger.UserProfile.xml", "Ginger.UserProfile-Backup.xml"), true);
                    }
                    catch (Exception ex2)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to create backup copy for the User Profile file", ex2);
                    }
                }
            }

            Reporter.ToLog(eLogLevel.INFO, "Creating new User Profile");
            UserProfile up2 = new UserProfile
            {
                UserProfileOperations = UserProfileOperations
            };
            //up2.LoadDefaults();
            UserProfileOperations.LoadDefaults();
            if (UserConfigdictObj != null)
            {
                up2.AddUserConfigProperties(UserConfigdictObj);
            }

            return up2;
        }

        public void LoadUserTypeHelper()
        {
            UserTypeHelper = new UserTypeHelper();
            UserTypeHelper.Init(UserType);
        }

        public void AddUserConfigProperties(Dictionary<string, string> dictObj)
        {
            switch (dictObj["UserType"])
            {
                case "Regular":
                    UserType = eUserType.Regular;
                    break;

                case "Business":
                    UserType = eUserType.Business;
                    break;
            }

            switch (dictObj["TerminologyDictionaryType"])
            {
                case "Default":
                    TerminologyDictionaryType = GingerCore.eTerminologyType.Default;
                    break;

                case "Testing":
                    TerminologyDictionaryType = GingerCore.eTerminologyType.Testing;
                    break;
                case "Gherkin":
                    TerminologyDictionaryType = GingerCore.eTerminologyType.Gherkin;
                    break;
            }
        }

        //public void LoadDefaults()
        //{
        //    AutoLoadLastSolution = true; //#Task 160
        //    AutoLoadLastRunSet = true; //#Task 160
        //    string defualtFolder = WorkSpace.Instance.DefualtUserLocalWorkingFolder;//calling it so it will be created
        //}

        public string GetDefaultReport()
        {
            if (!string.IsNullOrEmpty(ReportTemplateName))
            {
                return ReportTemplateName;
            }
            else
            {
                // for new user who didn't defined yet report we give the default Full rep
                return "Full Detailed Report";
            }
        }

        public override string ItemName
        {
            get
            {
                return string.Empty;
            }
            set
            {
                return;
            }
        }

        bool mDoNotAskToUpgradeSolutions = false;
        [IsSerializedForLocalRepository]
        public bool DoNotAskToUpgradeSolutions
        {
            get
            {
                return mDoNotAskToUpgradeSolutions;
            }
            set
            {
                if (mDoNotAskToUpgradeSolutions != value)
                {
                    mDoNotAskToUpgradeSolutions = value;
                    OnPropertyChanged(nameof(DoNotAskToUpgradeSolutions));
                }
            }
        }

        [IsSerializedForLocalRepository]
        public bool NewHelpLibraryMessgeShown { get; set; }


        bool mAskToSaveBusinessFlow = true;
        [IsSerializedForLocalRepository(true)]
        public bool AskToSaveBusinessFlow
        {
            get
            {
                return mAskToSaveBusinessFlow;
            }
            set
            {
                if (mAskToSaveBusinessFlow != value)
                {
                    mAskToSaveBusinessFlow = value;
                    OnPropertyChanged(nameof(AskToSaveBusinessFlow));
                }
            }
        }

        [IsSerializedForLocalRepository]
        public bool DoNotAskToRecoverSolutions { get; set; }

        bool mAutoLoadLastRunSet;
        [IsSerializedForLocalRepository]
        public bool AutoLoadLastRunSet
        {
            get
            {
                return mAutoLoadLastRunSet;
            }
            set
            {
                if (mAutoLoadLastRunSet != value)
                {
                    mAutoLoadLastRunSet = value;
                    OnPropertyChanged(nameof(AutoLoadLastRunSet));
                }
            }
        }

        bool mShowEnterpriseFeatures;
        [IsSerializedForLocalRepository]
        public bool ShowEnterpriseFeatures
        {
            get
            {
                return mShowEnterpriseFeatures;
            }
            set
            {
                if (mShowEnterpriseFeatures != value)
                {
                    mShowEnterpriseFeatures = value;
                    OnPropertyChanged(nameof(ShowEnterpriseFeatures));
                }
            }
        }

        string mProfileImage;
        [IsSerializedForLocalRepository]
        public string ProfileImage
        {
            get
            {
                return mProfileImage;
            }
            set
            {
                if (mProfileImage != value)
                {
                    mProfileImage = value;
                    OnPropertyChanged(nameof(ProfileImage));
                }
            }
        }


        string mActionOutputValueUserPreferences;
        [IsSerializedForLocalRepository(DefaultValue: "Path,ActualValue,ExpectedValue,StoreTo")]
        public string ActionOutputValueUserPreferences
        {
            get
            {
                return mActionOutputValueUserPreferences;
            }
            set
            {
                if (mActionOutputValueUserPreferences != value)
                {
                    mActionOutputValueUserPreferences = value;
                    OnPropertyChanged(nameof(ActionOutputValueUserPreferences));
                }
            }
        }
        public string UserName
        {
            get { return Environment.UserName; }
        }

        string mUserFirstName;
        [IsSerializedForLocalRepository]
        public string UserFirstName
        {
            get
            {
                return mUserFirstName;
            }
            set
            {
                if (mUserFirstName != value)
                {
                    mUserFirstName = value;
                    OnPropertyChanged(nameof(UserFirstName));
                }
            }
        }

        string mUserMiddleName;
        [IsSerializedForLocalRepository]
        public string UserMiddleName
        {
            get
            {
                return mUserMiddleName;
            }
            set
            {
                if (mUserMiddleName != value)
                {
                    mUserMiddleName = value;
                    OnPropertyChanged(nameof(UserMiddleName));
                }
            }
        }

        string mUserLastName;
        [IsSerializedForLocalRepository]
        public string UserLastName
        {
            get
            {
                return mUserLastName;
            }
            set
            {
                if (mUserLastName != value)
                {
                    mUserLastName = value;
                    OnPropertyChanged(nameof(UserLastName));
                }
            }
        }

        string mUserEmail;
        [IsSerializedForLocalRepository]
        public string UserEmail
        {
            get
            {
                return mUserEmail;
            }
            set
            {
                if (mUserEmail != value)
                {
                    mUserEmail = value;
                    OnPropertyChanged(nameof(UserEmail));
                }
            }
        }

        string mUserPhone;
        [IsSerializedForLocalRepository]
        public string UserPhone
        {
            get
            {
                return mUserPhone;
            }
            set
            {
                if (mUserPhone != value)
                {
                    mUserPhone = value;
                    OnPropertyChanged(nameof(UserPhone));
                }
            }
        }

        eUserRole mUserRole;
        [IsSerializedForLocalRepository]
        public eUserRole UserRole
        {
            get
            {
                return mUserRole;
            }
            set
            {
                if (mUserRole != value)
                {
                    mUserRole = value;
                    OnPropertyChanged(nameof(UserRole));
                }
            }
        }

        string mUserDepartment;
        [IsSerializedForLocalRepository]
        public string UserDepartment
        {
            get
            {
                return mUserDepartment;
            }
            set
            {
                if (mUserDepartment != value)
                {
                    mUserDepartment = value;
                    OnPropertyChanged(nameof(UserDepartment));
                }
            }
        }

        [IsSerializedForLocalRepository]
        public List<string> ShownHelpLayoutsKeys = [];



        private ObservableList<GingerSolution> mGingerSolutions = new ObservableList<GingerSolution>();
        [IsSerializedForLocalRepository]
        public ObservableList<GingerSolution> GingerSolutions
        {
            get
            {
                return mGingerSolutions;
            }
            set
            {
                if (mGingerSolutions != value)
                {
                    mGingerSolutions = value;
                    OnPropertyChanged(nameof(GingerSolutions));
                }
            }
        }

        private Guid mRecentDownloadedSolutionGuid;
        [IsSerializedForLocalRepository]
        public Guid RecentDownloadedSolutionGuid
        {
            get
            {
                return mRecentDownloadedSolutionGuid;
            }
            set
            {
                if (mRecentDownloadedSolutionGuid != value)
                {
                    mRecentDownloadedSolutionGuid = value;
                    OnPropertyChanged(nameof(RecentDownloadedSolutionGuid));
                }
            }
        }

        public GingerSolution GetSolutionSourceControlInfo(Guid solutionGuid)
        {
            foreach (GingerSolution item in GingerSolutions)
            {
                if (item.SolutionGuid == solutionGuid)
                {
                    return item;
                }
            }
            GingerSolution solutionSourceControlInfo = new Amdocs.Ginger.Common.SourceControlLib.GingerSolution()
            {
                SolutionGuid = solutionGuid,
                SourceControlInfo = new Amdocs.Ginger.Common.SourceControlLib.SourceControlInfo()
            };
            GingerSolutions.Add(solutionSourceControlInfo);

            return solutionSourceControlInfo;
        }

        public void SetSourceControlPropertyOnUserProfile(SourceControlBase mSourceControl, Guid solutionGuid)
        {
            try
            {
                if (mSourceControl == null || solutionGuid == Guid.Empty || DoNotSaveCredentialsOnUserProfile)
                {
                    return;
                }
                var GingerSolutionSourceControl = GetSolutionSourceControlInfo(solutionGuid);


                GingerSolutionSourceControl.SourceControlInfo.Type = mSourceControl.GetSourceControlType;

                if (!string.IsNullOrEmpty(mSourceControl.URL))
                {
                    GingerSolutionSourceControl.SourceControlInfo.Url = mSourceControl.URL;
                }
                if (!string.IsNullOrEmpty(mSourceControl.Username))
                {
                    GingerSolutionSourceControl.SourceControlInfo.Username = mSourceControl.Username;
                }
                if (!string.IsNullOrEmpty(mSourceControl.Password))
                {
                    GingerSolutionSourceControl.SourceControlInfo.Password = mSourceControl.Password;
                }
                if (!string.IsNullOrEmpty(mSourceControl.LocalFolder))
                {
                    if (mSourceControl.LocalFolder.EndsWith(".git"))
                    {
                        int slashIndex = mSourceControl.LocalFolder.LastIndexOf('\\');
                        if (slashIndex >= 0)
                        {
                            GingerSolutionSourceControl.SourceControlInfo.LocalFolderPath = mSourceControl.LocalFolder.Substring(0, slashIndex);
                        }
                        else
                        {
                            GingerSolutionSourceControl.SourceControlInfo.LocalFolderPath = mSourceControl.LocalFolder;
                        }
                    }
                    else
                    {
                        GingerSolutionSourceControl.SourceControlInfo.LocalFolderPath = mSourceControl.LocalFolder;
                    }
                }
                if (!string.IsNullOrEmpty(mSourceControl.Branch))
                {
                    GingerSolutionSourceControl.SourceControlInfo.Branch = mSourceControl.Branch;
                }
                if (!string.IsNullOrEmpty(mSourceControl.AuthorName))
                {
                    GingerSolutionSourceControl.SourceControlInfo.AuthorName = mSourceControl.AuthorName;
                }
                if (!string.IsNullOrEmpty(mSourceControl.AuthorEmail))
                {
                    GingerSolutionSourceControl.SourceControlInfo.AuthorEmail = mSourceControl.AuthorEmail;
                }
                if (mSourceControl.Timeout != 0)
                {
                    GingerSolutionSourceControl.SourceControlInfo.Timeout = mSourceControl.Timeout;
                }
                if (mSourceControl.IsProxyConfigured)
                {
                    GingerSolutionSourceControl.SourceControlInfo.IsProxyConfigured = mSourceControl.IsProxyConfigured;
                }
                if (!string.IsNullOrEmpty(mSourceControl.ProxyAddress))
                {
                    GingerSolutionSourceControl.SourceControlInfo.ProxyAddress = mSourceControl.ProxyAddress;
                }
                if (mSourceControl.ProxyPort != null)
                {
                    GingerSolutionSourceControl.SourceControlInfo.ProxyPort = mSourceControl.ProxyPort;
                }

                UserProfileOperations.SaveUserProfile();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.ToString());
            }
        }



        public void GetSourceControlPropertyFromUserProfile(SourceControlBase mSourceControl, Guid solutionGuid)
        {
            try
            {
                if (mSourceControl == null || solutionGuid == Guid.Empty || DoNotSaveCredentialsOnUserProfile)
                {
                    return;
                }
                UserProfileOperations.RefreshSourceControlCredentials(solutionGuid);
                var GingerSolutionSourceControl = GetSolutionSourceControlInfo(solutionGuid);
                mSourceControl.URL = GingerSolutionSourceControl.SourceControlInfo.Url;
                mSourceControl.Username = GingerSolutionSourceControl.SourceControlInfo.Username;
                mSourceControl.Password = GingerSolutionSourceControl.SourceControlInfo.Password;
                mSourceControl.LocalFolder = GingerSolutionSourceControl.SourceControlInfo.LocalFolderPath;
                mSourceControl.Branch = GingerSolutionSourceControl.SourceControlInfo.Branch;
                mSourceControl.AuthorName = GingerSolutionSourceControl.SourceControlInfo.AuthorName;
                mSourceControl.AuthorEmail = GingerSolutionSourceControl.SourceControlInfo.AuthorEmail;
                mSourceControl.IsProxyConfigured = GingerSolutionSourceControl.SourceControlInfo.IsProxyConfigured;
                mSourceControl.ProxyAddress = GingerSolutionSourceControl.SourceControlInfo.ProxyAddress;
                mSourceControl.ProxyPort = GingerSolutionSourceControl.SourceControlInfo.ProxyPort;

                if (GingerSolutionSourceControl.SourceControlInfo.Timeout == 0)
                {
                    GingerSolutionSourceControl.SourceControlInfo.Timeout = 80;
                }
                mSourceControl.Timeout = GingerSolutionSourceControl.SourceControlInfo.Timeout;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.ToString());
            }
        }

        /// <summary>
        /// Retrieves source control properties from the user profile using the provided URL.
        /// </summary>
        /// <param name="mSourceControl">The source control object to populate with properties.</param>
        /// <param name="solutionGuid">The GUID of the solution.</param>
        public void GetSourceControlPropertyFromUserProfileUsingURL(SourceControlBase mSourceControl, Guid solutionGuid)
        {
            try
            {
                //old git info stored on 0 node 
                GingerSolution item = GingerSolutions.Count >= 1 ? GingerSolutions[0] : null;
                if (item == null)
                {
                    return;
                }
                if (item.SourceControlInfo.Url == mSourceControl.URL && item.SolutionGuid == Guid.Empty)
                {
                    UserProfileOperations.ReadOldSourceControlCredentials(mSourceControl);
                    mSourceControl.AuthorName = item.SourceControlInfo.AuthorName;
                    mSourceControl.AuthorEmail = item.SourceControlInfo.AuthorEmail;
                    mSourceControl.Branch = item.SourceControlInfo.Branch;
                    GingerSolutions.RemoveAt(0);
                    RecentDownloadedSolutionGuid = solutionGuid;
                    SetSourceControlPropertyOnUserProfile(mSourceControl, solutionGuid);                   
                }

            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to load the old source control configuration", ex);
            }
        }



        public override bool SerializationError(SerializationErrorType errorType, string name, string value)
        {
            if (errorType != SerializationErrorType.PropertyNotFound)
            {
                return false;
            }

            name = name.ToLower();
            if (name.Contains("alm"))
            {
                var almUserConfig = ALMUserConfigs.FirstOrDefault() ?? new ALMUserConfig();
                if (!ALMUserConfigs.Contains(almUserConfig))
                {
                    ALMUserConfigs.Add(almUserConfig);
                }

                if (name == "almusername")
                {
                    almUserConfig.ALMUserName = value;
                    return true;
                }
                else if (name == "encryptedalmpassword")
                {
                    almUserConfig.EncryptedALMPassword = value;
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else if (name.Contains("sourcecontrol"))
            {
                var gingerSolution = GingerSolutions.FirstOrDefault() ?? new GingerSolution { SourceControlInfo = new SourceControlInfo() };
                if (!GingerSolutions.Contains(gingerSolution))
                {
                    GingerSolutions.Add(gingerSolution);
                }

                switch (name)
                {
                    case "sourcecontroltype":
                        if (Enum.TryParse(value, out SourceControlBase.eSourceControlType sourceControlType))
                        {
                            gingerSolution.SourceControlInfo.Type = sourceControlType;
                            return true;
                        }
                        break;
                    case "sourcecontrolurl":
                        gingerSolution.SourceControlInfo.Url = value;
                        return true;
                    case "solutionsourcecontroluser":
                        gingerSolution.SourceControlInfo.Username = value;
                        return true;
                    case "sourcecontrolbranch":
                        gingerSolution.SourceControlInfo.Branch = value;
                        return true;
                    case "solutionsourcecontrolconfigureproxy":
                        if (bool.TryParse(value, out bool isProxyConfigure))
                        {
                            gingerSolution.SourceControlInfo.IsProxyConfigured = isProxyConfigure;
                            return true;
                        }
                        break;
                    case "solutionsourcecontrolproxyaddress":
                        gingerSolution.SourceControlInfo.ProxyAddress = value;
                        return true;
                    case "solutionsourcecontrolauthorname":
                        gingerSolution.SourceControlInfo.AuthorName = value;
                        return true;
                    case "solutionsourcecontrolauthoremail":
                        gingerSolution.SourceControlInfo.AuthorEmail = value;
                        return true;
                    case "solutionsourcecontrolproxyport":
                        gingerSolution.SourceControlInfo.ProxyPort = value;
                        return true;
                    case "sourcecontrollocalfolder":
                        gingerSolution.SourceControlInfo.LocalFolderPath = value;
                        return true;
                    case "solutionsourcecontroltimeout":
                        if (int.TryParse(value, out int timeout))
                        {
                            gingerSolution.SourceControlInfo.Timeout = timeout;
                            return true;
                        }
                        break;
                    default:
                        return false;
                }
            }

            return false;
        }
    }
}

