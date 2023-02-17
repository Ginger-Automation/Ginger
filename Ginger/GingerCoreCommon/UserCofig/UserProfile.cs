#region License
/*
Copyright © 2014-2023 European Support Limited

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
using Amdocs.Ginger.Repository;
using Ginger.UserConfig;
using GingerCore;
using GingerCore.GeneralLib;
using GingerCoreNET.ALMLib;
using GingerCoreNET.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerCoreNET.SourceControl;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

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
            public List<UserProfileGridCol> Cols = new List<UserProfileGridCol>();
        }

        public class UserProfileGridCol
        {
            public string Name { get; set; }
            public int Width { get; set; }
        }

        public List<UserProfileGrid> Grids = new List<UserProfileGrid>();

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

        [IsSerializedForLocalRepository]
        public eGingerStatus GingerStatus { get; set; }

        // Keep the folder names of last solutions opened
        [IsSerializedForLocalRepository]
        public List<string> RecentSolutions = new List<string>();

        [IsSerializedForLocalRepository]
        public List<string> RecentAppAgentsMapping = new List<string>();

        //[IsSerializedForLocalRepository]
        /// <summary>
        /// Not needed anymore keeping for allowing to load UserProfile xml
        /// </summary>
        //public Guid RecentBusinessFlow { get; set; }

        [IsSerializedForLocalRepository]
        public Guid RecentEnvironment { get; set; }

        [IsSerializedForLocalRepository]
        public Guid RecentRunset { get; set; }

        [IsSerializedForLocalRepository]
        public SourceControlBase.eSourceControlType SourceControlType { get; set; }

        [IsSerializedForLocalRepository]
        public string SourceControlURL { get; set; }

        [IsSerializedForLocalRepository]
        public string SourceControlUser { get; set; }

        [IsSerializedForLocalRepository]
        public string SolutionSourceControlUser { get; set; }

        [IsSerializedForLocalRepository]
        public string SourceControlBranch { get; set; }

        public string SolutionSourceControlBranch { get; set; }

        [IsSerializedForLocalRepository]
        public bool SolutionSourceControlConfigureProxy { get; set; }

        [IsSerializedForLocalRepository]
        public string SolutionSourceControlProxyAddress { get; set; }

        [IsSerializedForLocalRepository]
        public string SolutionSourceControlAuthorName { get; set; }

        [IsSerializedForLocalRepository]
        public string SolutionSourceControlAuthorEmail { get; set; }

        [IsSerializedForLocalRepository]
        public string SolutionSourceControlProxyPort { get; set; }


        [IsSerializedForLocalRepository(80)]
        public int SolutionSourceControlTimeout { get; set; }

        //[IsSerializedForLocalRepository]
        public string EncryptedSourceControlPass { get; set; }

        //[IsSerializedForLocalRepository]
        public string EncryptedSolutionSourceControlPass { get; set; }

        [IsSerializedForLocalRepository]
        public string SourceControlLocalFolder { get; set; }

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

        public string SourceControlPass
        {
            get; set;
        }

        public string SolutionSourceControlPass
        {
            get; set;
        }

        [IsSerializedForLocalRepository]
        public ObservableList<ALMUserConfig> ALMUserConfigs { get; set; } = new ObservableList<ALMUserConfig>();

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
            UserProfile up2 = new UserProfile();
            up2.UserProfileOperations = UserProfileOperations;
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
        public bool SourceControlUseShellClient { get; internal set; }

        public bool SourceControlIgnoreCertificate { get; internal set; }

        [IsSerializedForLocalRepository]
        public List<string> ShownHelpLayoutsKeys = new List<string>();
        public override bool SerializationError(SerializationErrorType errorType, string name, string value)
        {
            if (errorType == SerializationErrorType.PropertyNotFound)
            {
                if (name.ToLower().Contains("alm"))
                {
                    ALMUserConfig AlmUserConfig = ALMUserConfigs.FirstOrDefault();
                    if (AlmUserConfig == null)
                    {
                        AlmUserConfig = new ALMUserConfig();
                        ALMUserConfigs.Add(AlmUserConfig);
                    }
                    if (name == "ALMUserName")
                    {
                        AlmUserConfig.ALMUserName = value;
                        return true;
                    }
                    if (name == "EncryptedALMPassword")
                    {
                        AlmUserConfig.EncryptedALMPassword = value;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
