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
//using Amdocs.Ginger.Common.Repository;
//using Amdocs.Ginger.CoreNET.WorkSpaceLib;
//using Amdocs.Ginger.Repository;
//using GingerCoreNET.Dictionaries;
//using GingerCoreNET.GeneralLib;
//using GingerCoreNET.ReporterLib;
//using GingerCoreNET.SolutionRepositoryLib;
//using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib;
//using GingerCoreNET.SourceControl;
//using System;
//using System.Collections.Generic;
//using System.IO;

//namespace amdocs.ginger.GingerCoreNET
//{
//    public enum eGingerStatus{
//        Closed,Active,AutomaticallyClosed
//    }
  
//    public class UserProfile : RepositoryItem
//    {
//        //Move it to UCGridLib
//        public class UserProfileGrid
//        {
//            public string GridId {get; set;}
//            public List<UserProfileGridCol> Cols = new List<UserProfileGridCol>();
//        }

//        public class UserProfileGridCol
//        {
//            public string Name { get; set; }
//            public int Width { get; set; }
//        }
      
//        public string LocalWorkingFolder { get; set; }
//        public List<UserProfileGrid> Grids = new List<UserProfileGrid>();

//        [IsSerializedForLocalRepository]
//        public bool AutoLoadLastSolution { get; set; }

//        [IsSerializedForLocalRepository]
//        public eGingerStatus GingerStatus { get; set; }

//        [IsSerializedForLocalRepository]
//        public Guid CurrentAgentGUID { get; set; }
       
//        public ObservableList<Solution> RecentSolutionsObjects = new ObservableList<Solution>();
//        public void SetRecentSolutionsObjects()
//        {
//            try
//            {
//                int counter = 0;
//                foreach (string s in RecentSolutions)
//                {
//                    string SolutionFile = s + @"\Ginger.Solution.xml";
//                    if (File.Exists(SolutionFile))
//                    {
//                        NewRepositorySerializer RS = new NewRepositorySerializer();
//                        Solution sol = (Solution)RS.DeserializeFromFile(SolutionFile);
//                        sol.Folder = s;
//                        RecentSolutionsObjects.Add(sol);

//                        counter++;
//                        if (counter >= 10) break; // only first latest 10 solutions
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Reporter.ToLog(eLogLevel.ERROR, "Failed to set Recent Solutions Objects", ex);
//            }
//        }

//        public void AddsolutionToRecent(Solution s)
//        {
//            RecentSolutions.Remove(s.Folder);
//            // Add it first place 
//            RecentSolutions.Insert(0, s.Folder);
//        }

//        public void AddsolutionToRecent(string solutionFolder)
//        {
//            RecentSolutions.Remove(solutionFolder);
//            // Add it first place 
//            RecentSolutions.Insert(0, solutionFolder);
//        }

//        // Keep the folder names of last solutions opened
//        [IsSerializedForLocalRepository]
//        public List<string> RecentSolutions = new List<string>();

//        [IsSerializedForLocalRepository]
//        public ObservableList<UserProfileApplicationAgentMap> RecentAppAgentsMapping = new ObservableList<UserProfileApplicationAgentMap>();

//        [IsSerializedForLocalRepository]
//        public Guid RecentBusinessFlow { get; set; }

//        [IsSerializedForLocalRepository]
//        public Guid RecentEnvironment { get; set; }

//        [IsSerializedForLocalRepository]
//        public SourceControlBase.eSourceControlType SourceControlType { get; set; }

//        [IsSerializedForLocalRepository]
//        public string SourceControlURL { get; set; }

//        [IsSerializedForLocalRepository]
//        public string SourceControlUser { get; set; }

//        [IsSerializedForLocalRepository]
//        public string SolutionSourceControlUser { get; set; }

//        [IsSerializedForLocalRepository]
//        public bool SolutionSourceControlConfigureProxy { get; set; }

//        [IsSerializedForLocalRepository]
//        public string SolutionSourceControlProxyAddress { get; set; }

//        [IsSerializedForLocalRepository]
//        public string SolutionSourceControlProxyPort { get; set; }

//        [IsSerializedForLocalRepository]
//        public string EncryptedSourceControlPass { get; set; }

//        [IsSerializedForLocalRepository]
//        public string EncryptedSolutionSourceControlPass { get; set; }

//        [IsSerializedForLocalRepository]
//        public string SourceControlLocalFolder { get; set; }

//        [IsSerializedForLocalRepository]
//        public string ReportTemplateName { get; set; }

//        [IsSerializedForLocalRepository]
//        public System.Guid ReportTemplateGUID { get; set; }
        
//        public string SourceControlPass
//        {
//            get
//            {
//                bool res = false;
//                string pass = string.Empty;
//                if (EncryptedSourceControlPass != null)
//                    pass = EncryptionHandler.DecryptString(EncryptedSourceControlPass, ref res);
//                if (res && String.IsNullOrEmpty(pass) == false)
//                    return pass;
//                else
//                    return string.Empty;
//            }
//            set
//            {
//                bool res = false;
//                if (value != null)
//                    EncryptedSourceControlPass = EncryptionHandler.EncryptString(value, ref res);
//            }
//        }

//        public string SolutionSourceControlPass
//        {
//            get
//            {
//                bool res = false;
//                string pass = string.Empty;
//                if (EncryptedSolutionSourceControlPass != null)
//                    pass = EncryptionHandler.DecryptString(EncryptedSolutionSourceControlPass, ref res);
//                if (res && String.IsNullOrEmpty(pass) == false)
//                    return pass;
//                else
//                    return string.Empty;
//            }
//            set
//            {
//                bool res = false;
//                if (value != null)
//                    EncryptedSolutionSourceControlPass = EncryptionHandler.EncryptString(value, ref res);
//            }
//        }

//        [IsSerializedForLocalRepository]
//        public string ALMUserName { get; set; }

//        public string ALMProject { get; set; }
//        public string ALMPassword 
//        { 
//            get
//            {
//                bool res = false;
//                string pass = EncryptionHandler.DecryptString(EncryptedALMPassword, ref res);
//                if (res && String.IsNullOrEmpty(pass) == false)
//                    return pass;
//                else
//                    return string.Empty;
//            }
//            set
//            {
//                bool res=false;
//                EncryptedALMPassword = EncryptionHandler.EncryptString(value, ref res);
//            }
//        }
//        [IsSerializedForLocalRepository]
//        public string EncryptedALMPassword { get; set; }

//        //TODO: remove not used since using RunSetAction, need XML convert
//        //
//        [IsSerializedForLocalRepository]
//            public bool DefaultSendReportByMail{ get; set; }
//            [IsSerializedForLocalRepository]
//            public bool DefaultSendDetailedReport { get; set; }
//            [IsSerializedForLocalRepository]
//            public string DefaultEmailFrom { get; set; }
//            [IsSerializedForLocalRepository]
//            public string DefaultEmailTo { get; set; }
//            [IsSerializedForLocalRepository]
//            public Email.eEmailMethod DefaultEmailSendingMethod { get; set; }
//            [IsSerializedForLocalRepository]
//            public string DefaultEmailSmtpHost { get; set; }
//            [IsSerializedForLocalRepository]
//            public int DefaultEmailSmtpPort { get; set; }
//        //

//        [IsSerializedForLocalRepository]
//        public eTerminologyDicsType TerminologyDictionaryType { get; set; }

//        eAppLogLevel mAppLogLevel;
//       [IsSerializedForLocalRepository]
//        public eAppLogLevel AppLogLevel
//        {
//            get { return mAppLogLevel; }
//            set { mAppLogLevel = value; Reporter.CurrentAppLogLevel = mAppLogLevel; }
//        }

//        [IsSerializedForLocalRepository]
//        public eUserType UserType { get; set; }

//        [IsSerializedForLocalRepository]
//        public DateTime LastNewsPopUp { get; set; }

//        public static string CreateUserProfileFileName()
//        {
//            ////Not using Properties.Settings.Default because it can refer objects in the EXE, only simple strings

//            ////So we just save and load serialized UserProfile objct
                                  
//            // so we can have several users running on same machine and each one will get his/her own user profile file

//            string FileName = Environment.UserName.ToLower() + ".Ginger.UserProfile.xml";
//            string FullPath = Path.Combine(GetUserGingerFolder() + @"\" + FileName);
//            return FullPath;
//        }

//        public static string GetUserGingerFolder()
//        {
//            string UserHomedir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
//            string s = Path.Combine(UserHomedir + @"\", @"Ginger");
//            return s;
//        }
        
//        public void Save(string FileName)   // We override the default save which goes via SolutionRepository which is not needed
//        {
//            NewRepositorySerializer RS = new NewRepositorySerializer();
//            string s = RS.SerializeToString(this);
//            if (File.Exists(FileName))
//            {
//                File.Delete(FileName);
//            }
//            File.AppendAllText(FileName, s);            
//        }
        
//        public void LoadRecentAppAgentMapping()
//        {
//        }

//        public static UserProfile LoadUserProfile(string FileName)
//        {
//            NewRepositorySerializer RS = new NewRepositorySerializer();
//            UserProfile UP = (UserProfile)RS.DeserializeFromFile(FileName);
//            return UP;
//        }

//        public void LoadUserTypeHelper()
//        {
//        }

//        private void AddUserConfigProperties(Dictionary<string, string> dictObj)
//        {
//            switch (dictObj["UserType"])
//            {
//                case "Regular":
//                    UserType = eUserType.Regular;
//                    break;

//                case "Business":
//                    UserType = eUserType.Business;
//                    break;
//            }

//            switch (dictObj["TerminologyDictionaryType"])
//            {
//                case "Default":
//                    TerminologyDictionaryType = eTerminologyDicsType.Default;
//                    break;

//                case "Testing":
//                    TerminologyDictionaryType = eTerminologyDicsType.Testing;
//                    break;
//                case "Gherkin":
//                    TerminologyDictionaryType = eTerminologyDicsType.Gherkin;
//                    break;
//            }
//        }

//        public void ValidateProfile()
//        {
//        }

//        public void LoadDefaults()
//        {
//            AutoLoadLastSolution = true;
//            SetDefaultWorkingFolder();
//            //TODO: currently we have one centralized repo, to be designed later multiple repo soultion
//        }

//        public void SetDefaultWorkingFolder()
//        {
//        }

//        internal string GetDefaultReport()
//        {
//            if(!string.IsNullOrEmpty(ReportTemplateName))
//            {
//                return ReportTemplateName;
//            }
//            else
//            {
//                // for new user who didn't defined yet report we give the default Full rep
//                return "Full Detailed Report";
//            }
//        }

//        public override string ItemName
//        {
//            get
//            {
//                return string.Empty;
//            }
//            set
//            {
//                return;
//            }
//        }

//        [IsSerializedForLocalRepository]
//        public bool DoNotAskToUpgradeSolutions { get; set; }

//        [IsSerializedForLocalRepository]
//        public bool NewHelpLibraryMessgeShown { get; set; }
//    }
//}
