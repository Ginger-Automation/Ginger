#region License
/*
Copyright © 2014-2021 European Support Limited

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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.SolutionGeneral;
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
                mAutoLoadLastSolution = value;
                OnPropertyChanged(nameof(AutoLoadLastSolution));
            }
        }

        [IsSerializedForLocalRepository]
        public eGingerStatus GingerStatus { get; set; }

        // Keep the folder names of last solutions opened
        [IsSerializedForLocalRepository]
        public List<string> RecentSolutions = new List<string>();

        private void CleanRecentSolutionsList()
        {
            try
            {
                //Clean not exist Solutions
                for (int i = 0; i < RecentSolutions.Count; i++)
                {
                    if (Directory.Exists(RecentSolutions[i]) == false)
                    {
                        RecentSolutions.RemoveAt(i);
                        i--;
                    }
                }

                //clean recent solutions list from duplications caused due to bug
                for (int i = 0; i < RecentSolutions.Count; i++)
                {
                    for (int j = i + 1; j < RecentSolutions.Count; j++)
                    {
                        if (SolutionRepository.NormalizePath(RecentSolutions[i]) == SolutionRepository.NormalizePath(RecentSolutions[j]))
                        {
                            RecentSolutions.RemoveAt(j);
                            j--;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to do Recent Solutions list clean up", ex);
            }
        }

        private ObservableList<Solution> mRecentSolutionsAsObjects = null;
        public ObservableList<Solution> RecentSolutionsAsObjects
        {
            get
            {
                if (mRecentSolutionsAsObjects == null)
                {
                    LoadRecentSolutionsAsObjects();
                }
                return mRecentSolutionsAsObjects;
            }
            set
            {
                mRecentSolutionsAsObjects = value;
            }
        }

        private void LoadRecentSolutionsAsObjects()
        {

            CleanRecentSolutionsList();

            mRecentSolutionsAsObjects = new ObservableList<Solution>();
            int counter = 0;
            foreach (string s in RecentSolutions)
            {
                string SolutionFile = Path.Combine(s, @"Ginger.Solution.xml");
                if (File.Exists(SolutionFile))
                {
                    try
                    {
                        Solution sol = Solution.LoadSolution(SolutionFile, false);
                        mRecentSolutionsAsObjects.Add(sol);
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to load the recent solution which in path '{0}'", s), ex);
                    }

                    counter++;
                    if (counter >= 10)
                    {
                        break; // only first latest 10 solutions
                    }
                }
            }

            return;
        }

        public void AddSolutionToRecent(Solution solution)
        {
            //remove existing similar folder path
            string solPath = RecentSolutions.Where(x => SolutionRepository.NormalizePath(x) == SolutionRepository.NormalizePath(solution.Folder)).FirstOrDefault();
            if (solPath != null)
            {
                RecentSolutions.Remove(solPath);
                Solution sol = mRecentSolutionsAsObjects.Where(x => SolutionRepository.NormalizePath(x.Folder) == SolutionRepository.NormalizePath(solution.Folder)).FirstOrDefault();
                if (sol != null)
                {
                    mRecentSolutionsAsObjects.Remove(sol);
                }
            }

            // Add it in first place 
            if (RecentSolutions.Count == 0)
            {
                RecentSolutions.Add(solution.Folder);
            }
            else
            {
                RecentSolutions.Insert(0, solution.Folder);
            }

            RecentSolutionsAsObjects.AddToFirstIndex(solution);

            while (RecentSolutions.Count > 10)//to keep list of 10
            {
                RecentSolutions.RemoveAt(10);
            }
        }

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
                mAutoRunAutomatePageAnalyzer = value;
                OnPropertyChanged(nameof(AutoRunAutomatePageAnalyzer));
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
                mAutoGenerateAutomatePageReport = value;
                OnPropertyChanged(nameof(AutoGenerateAutomatePageReport));
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
            set { mTerminologyType = value; OnPropertyChanged(nameof(TerminologyDictionaryType)); }
        }

        eAppReporterLoggingLevel mAppLogLevel;
        [IsSerializedForLocalRepository]
        public eAppReporterLoggingLevel AppLogLevel
        {
            get { return mAppLogLevel; }
            set { mAppLogLevel = value; Reporter.AppLoggingLevel = mAppLogLevel; OnPropertyChanged(nameof(AppLogLevel)); }
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
                mUserType = value;
                OnPropertyChanged(nameof(UserType));
            }
        }

        public UserTypeHelper UserTypeHelper { get; set; }

        static private bool mSharedUserProfileBeenUsed;
        static string mUserProfileFilePath;
        public static string UserProfileFilePath
        {
            get
            {
                if (mUserProfileFilePath == null)
                {
                    string userProfileFileName = "Ginger.UserProfile.xml";
                    string sharedUserProfilePath = Path.Combine(WorkSpace.Instance.CommonApplicationDataFolderPath, userProfileFileName);
                    string specificUserProfilePath = Path.Combine(WorkSpace.Instance.LocalUserApplicationDataFolderPath, userProfileFileName);
                    if (WorkSpace.Instance.RunningInExecutionMode && File.Exists(sharedUserProfilePath))
                    {
                        mUserProfileFilePath = sharedUserProfilePath;
                        Reporter.ToLog(eLogLevel.INFO, string.Format("Shared User Profile is been used, path:'{0}'", sharedUserProfilePath));
                        mSharedUserProfileBeenUsed = true;
                    }
                    else
                    {
                        mUserProfileFilePath = specificUserProfilePath;
                    }
                }
                return mUserProfileFilePath;
            }
        }

        public void SaveUserProfile()
        {
            if (mSharedUserProfileBeenUsed)
            {
                Reporter.ToLog(eLogLevel.INFO, string.Format("Not performing User Profile Save because Shared User Profile is been used"));
                return;
            }

            try
            {
                SaveRecentAppAgentsMapping();
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error occurred while saving Recent App-Agents Mapping for User Profile save", ex);
            }
            RepositorySerializer.SaveToFile(this, UserProfileFilePath);
            SavePasswords();
        }

        public void SaveRecentAppAgentsMapping()
        {
            if (WorkSpace.Instance.Solution != null)
            {
                //remove last saved mapping for this solution
                string existingSolMapping = RecentAppAgentsMapping.Where(x => x.Contains(WorkSpace.Instance.Solution.Name + "***") == true).FirstOrDefault();
                if (string.IsNullOrEmpty(existingSolMapping) == false)
                {
                    RecentAppAgentsMapping.Remove(existingSolMapping);
                }

                //create new save to this solution
                existingSolMapping = WorkSpace.Instance.Solution.Name + "***";
                foreach (ApplicationPlatform ap in WorkSpace.Instance.Solution.ApplicationPlatforms)
                {
                    if (string.IsNullOrEmpty(ap.LastMappedAgentName) == false)
                    {
                        existingSolMapping = string.Format("{0}{1},{2}#", existingSolMapping, ap.AppName, ap.LastMappedAgentName);
                    }
                }
                RecentAppAgentsMapping.Add(existingSolMapping);
            }
        }

        public void LoadRecentAppAgentMapping()
        {
            if (WorkSpace.Instance.Solution != null)
            {
                //unserialize the current solution mapping saving
                string existingSolMapping = RecentAppAgentsMapping.Where(x => x.Contains(WorkSpace.Instance.Solution.Name + "***") == true).FirstOrDefault();
                if (string.IsNullOrEmpty(existingSolMapping))
                {
                    return;//no saved mapping
                }
                else
                {
                    string solName = WorkSpace.Instance.Solution.Name + "***";
                    existingSolMapping = existingSolMapping.Replace(solName, string.Empty);
                    List<string> appAgentMapping = existingSolMapping.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    Dictionary<string, string> mappingDic = new Dictionary<string, string>();
                    foreach (string mapping in appAgentMapping)
                    {
                        string[] appAgent = mapping.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (appAgent.Length == 2 && !mappingDic.ContainsKey(appAgent[0])) //somehow duplicate application platform saved in solution, till we found root cause skip duplicate application platform.   
                        {
                            mappingDic.Add(appAgent[0], appAgent[1]);
                        }
                    }

                    //update the solution apps with the saved mapping
                    if (mappingDic.Count > 0)
                    {
                        foreach (ApplicationPlatform ap in WorkSpace.Instance.Solution.ApplicationPlatforms)
                        {
                            if (mappingDic.Keys.Contains(ap.AppName))
                            {
                                if (ap != null && WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>().Count > 0)
                                {
                                    List<Agent> platformAgents = (from p in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where p.Platform == ap.Platform select p).ToList();
                                    Agent matchingAgent = platformAgents.Where(x => x.Name == mappingDic[ap.AppName]).FirstOrDefault();
                                    if (matchingAgent != null)
                                        ap.LastMappedAgentName = matchingAgent.Name;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static UserProfile LoadUserProfile()
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

            if (File.Exists(UserProfileFilePath))
            {
                try
                {
                    DateTime UserProfileDT = File.GetLastWriteTime(UserProfileFilePath);
                    Reporter.ToLog(eLogLevel.INFO, string.Format("Loading existing User Profile at '{0}'", UserProfileFilePath));
                    string userProfileTxt = File.ReadAllText(UserProfileFilePath);
                    UserProfile up = (UserProfile)NewRepositorySerializer.DeserializeFromText(userProfileTxt);
                    up.FilePath = UserProfileFilePath;
                    if (UserConfigdictObj != null &&
                        DateTime.Compare(UserProfileDT, File.GetLastWriteTime(InstallationConfigurationPath)) < 0)
                    {
                        up.AddUserConfigProperties(UserConfigdictObj);
                    }
                    up = LoadPasswords(up);
                    return up;
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.ERROR, string.Format("Failed to load the existing User Profile at '{0}'", UserProfileFilePath), ex);
                    try
                    {
                        //create backup to the user profile so user won't lose all of it configs in case he went back to old Ginger version
                        //TODO- allow recover from newer User Profile version in code instead creating new user profile
                        Reporter.ToLog(eLogLevel.INFO, "Creating backup copy for the User Profile file");
                        File.Copy(UserProfileFilePath, UserProfileFilePath.Replace("Ginger.UserProfile.xml", "Ginger.UserProfile-Backup.xml"), true);
                    }
                    catch (Exception ex2)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to create backup copy for the User Profile file", ex2);
                    }
                }
            }

            Reporter.ToLog(eLogLevel.INFO, "Creating new User Profile");
            UserProfile up2 = new UserProfile();
            up2.LoadDefaults();
            if (UserConfigdictObj != null)
            {
                up2.AddUserConfigProperties(UserConfigdictObj);
            }

            return up2;
        }

        public static UserProfile LoadPasswords(UserProfile userProfile)
        {
            //Get sourcecontrol password
            if (!string.IsNullOrEmpty(userProfile.EncryptedSourceControlPass))
            {
                userProfile.SourceControlPass = EncryptionHandler.DecryptwithKey(userProfile.EncryptedSourceControlPass);
            }
            else
            {
                userProfile.SourceControlPass = WinCredentialUtil.GetCredential("Ginger_SourceControl_" + userProfile.SourceControlType);
            }

            if (!string.IsNullOrEmpty(userProfile.EncryptedSolutionSourceControlPass))
            {
                userProfile.SolutionSourceControlPass = EncryptionHandler.DecryptwithKey(userProfile.EncryptedSolutionSourceControlPass);
            }
            else
            {
                userProfile.SolutionSourceControlPass = WinCredentialUtil.GetCredential("Ginger_SolutionSourceControl");
            }


            //Get ALM passwords
            foreach (GingerCoreNET.ALMLib.ALMUserConfig almConfig in userProfile.ALMUserConfigs)
            {
                if (!string.IsNullOrEmpty(almConfig.EncryptedALMPassword))
                {
                    almConfig.ALMPassword = EncryptionHandler.DecryptwithKey(almConfig.EncryptedALMPassword);
                }
                else
                {
                    almConfig.ALMPassword = WinCredentialUtil.GetCredential("Ginger_ALM_" + almConfig.AlmType);
                }
            }

            return userProfile;
        }

        public void SavePasswords()
        {
            //Save source control password
            if (!string.IsNullOrEmpty(SourceControlPass))
            {
                WinCredentialUtil.SetCredentials("Ginger_SourceControl_" + SourceControlType, SourceControlUser, SourceControlPass);
            }
            if (!string.IsNullOrEmpty(SolutionSourceControlPass))
            {
                WinCredentialUtil.SetCredentials("Ginger_SolutionSourceControl", SolutionSourceControlUser, SolutionSourceControlPass);
            }

            //Save ALM passwords on windows credential manager
            foreach (GingerCoreNET.ALMLib.ALMUserConfig almConfig in ALMUserConfigs.Where(f => !string.IsNullOrEmpty(f.ALMPassword)))
            {
                WinCredentialUtil.SetCredentials("Ginger_ALM_" + almConfig.AlmType, almConfig.ALMUserName, almConfig.ALMPassword);
            }
        }

        public void LoadUserTypeHelper()
        {
            UserTypeHelper = new UserTypeHelper();
            UserTypeHelper.Init(UserType);
        }

        private void AddUserConfigProperties(Dictionary<string, string> dictObj)
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

        public void LoadDefaults()
        {
            AutoLoadLastSolution = true; //#Task 160            
            string defualtFolder = WorkSpace.Instance.DefualtUserLocalWorkingFolder;//calling it so it will be created
        }

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
                mDoNotAskToUpgradeSolutions = value;
                OnPropertyChanged(nameof(DoNotAskToUpgradeSolutions));
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
                mAskToSaveBusinessFlow = value;
                OnPropertyChanged(nameof(AskToSaveBusinessFlow));
            }
        }

        [IsSerializedForLocalRepository]
        public bool DoNotAskToRecoverSolutions { get; set; }

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
                mUserFirstName = value;
                OnPropertyChanged(nameof(UserFirstName));
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
                mUserMiddleName = value;
                OnPropertyChanged(nameof(UserMiddleName));
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
                mUserLastName = value;
                OnPropertyChanged(nameof(UserLastName));
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
                mUserEmail = value;
                OnPropertyChanged(nameof(UserEmail));
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
                mUserPhone = value;
                OnPropertyChanged(nameof(UserPhone));
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
                mUserRole = value;
                OnPropertyChanged(nameof(UserRole));
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
                mUserDepartment = value;
                OnPropertyChanged(nameof(UserDepartment));
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
