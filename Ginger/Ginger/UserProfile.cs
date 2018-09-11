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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.SolutionGeneral;
using Ginger.UserConfig;
using GingerCore;
using GingerCore.Platforms;
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
    public enum eGingerStatus{
        Closed,Active,AutomaticallyClosed
    }
  
    public class UserProfile : RepositoryItem
    {
        //Move it to UCGridLib
        public class UserProfileGrid
        {
            public string GridId {get; set;}
            public List<UserProfileGridCol> Cols = new List<UserProfileGridCol>();
        }

        public class UserProfileGridCol
        {
            public string Name { get; set; }
            public int Width { get; set; }
        }

        public new static class Fields
        {
            public static string AutoLoadLastSolution = "AutoLoadLastSolution";
            public static string ReportTemplate ="ReportTemplate";
            public static string DoNotAskToUpgradeSolutions = "DoNotAskToUpgradeSolutions";
            public static string SourceControlURL = "SourceControlURL"; //represent the source control SERVER url
            public static string SourceControlType = "SourceControlType";   //represent the last used source control type
            public static string SourceControlUser = "SourceControlUser"; //user profile  
            public static string SolutionSourceControlUser = "SolutionSourceControlUser"; //Solution
            public static string UserType = "UserType";
            public static string DoNotAskToRecoverSolutions = "DoNotAskToRecoverSolutions";
        }

        public string LocalWorkingFolder { get; set; }

        public Solution mSolution { get; set; }

        public Solution Solution
        {
            get { return mSolution; }
            set
            {
                mSolution = value;
                OnPropertyChanged(nameof(Solution));
            }
        }
        
        public List<UserProfileGrid> Grids = new List<UserProfileGrid>();

        [IsSerializedForLocalRepository]
        public bool AutoLoadLastSolution { get; set; }

        [IsSerializedForLocalRepository]
        public eGingerStatus GingerStatus { get; set; }
        
        public ObservableList<Solution> RecentSolutionsObjects = new ObservableList<Solution>();
        public void SetRecentSolutionsObjects()
        {
            try
            {
                int counter = 0;
                foreach (string s in RecentSolutions)
                {
                    string SolutionFile = s + @"\Ginger.Solution.xml";
                    if (File.Exists(SolutionFile))
                    {
                        Solution sol = Solution.LoadSolution(SolutionFile, false);
                        sol.Folder = s;
                        RecentSolutionsObjects.Add(sol);

                        counter++;
                        if (counter >= 10) break; // only first latest 10 solutions
                    }
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to set Recent Solutions Objects", ex);
            }
        }

        public void AddsolutionToRecent(Solution s)
        {
            App.UserProfile.RecentSolutions.Remove(s.Folder);
            // Add it first place 
            App.UserProfile.RecentSolutions.Insert(0, s.Folder);
        }

        public void AddsolutionToRecent(string solutionFolder)
        {
            App.UserProfile.RecentSolutions.Remove(solutionFolder);
            // Add it first place 
            App.UserProfile.RecentSolutions.Insert(0, solutionFolder);
        }

        // Keep the folder names of last solutions opened
        [IsSerializedForLocalRepository]
        public List<string> RecentSolutions = new List<string>();

        [IsSerializedForLocalRepository]
        public List<string> RecentAppAgentsMapping = new List<string>();

        [IsSerializedForLocalRepository]
        public Guid RecentBusinessFlow { get; set; }

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
        public bool SolutionSourceControlConfigureProxy { get; set; }

        [IsSerializedForLocalRepository]
        public string SolutionSourceControlProxyAddress { get; set; }

        [IsSerializedForLocalRepository]
        public string SolutionSourceControlAuthorName { get; set; }

        [IsSerializedForLocalRepository]
        public string SolutionSourceControlAuthorEmail { get; set; }

        [IsSerializedForLocalRepository]
        public string SolutionSourceControlProxyPort { get; set; }

        [IsSerializedForLocalRepository]
        public string EncryptedSourceControlPass { get; set; }

        [IsSerializedForLocalRepository]
        public string EncryptedSolutionSourceControlPass { get; set; }

        [IsSerializedForLocalRepository]
        public string SourceControlLocalFolder { get; set; }

        [IsSerializedForLocalRepository]
        public string ReportTemplateName { get; set; }
        
        public string SourceControlPass
        {
            get
            {
                bool res = false;
                string pass = string.Empty;
                if (EncryptedSourceControlPass != null)
                    pass = EncryptionHandler.DecryptString(EncryptedSourceControlPass, ref res);
                if (res && String.IsNullOrEmpty(pass) == false)
                    return pass;
                else
                    return string.Empty;
            }
            set
            {
                bool res = false;
                if (value != null)
                    EncryptedSourceControlPass = EncryptionHandler.EncryptString(value, ref res);
            }
        }

        public string SolutionSourceControlPass
        {
            get
            {
                bool res = false;
                string pass = string.Empty;
                if (EncryptedSolutionSourceControlPass != null)
                    pass = EncryptionHandler.DecryptString(EncryptedSolutionSourceControlPass, ref res);
                if (res && String.IsNullOrEmpty(pass) == false)
                    return pass;
                else
                    return string.Empty;
            }
            set
            {
                bool res = false;
                if (value != null)
                    EncryptedSolutionSourceControlPass = EncryptionHandler.EncryptString(value, ref res);
            }
        }

        [IsSerializedForLocalRepository]
        public string ALMUserName { get; set; }
        
        public string ALMPassword 
        { 
            get
            {
                bool res = false;
                string pass = EncryptionHandler.DecryptString(EncryptedALMPassword, ref res);
                if (res && String.IsNullOrEmpty(pass) == false)
                    return pass;
                else
                    return string.Empty;
            }
            set
            {
                bool res=false;
                EncryptedALMPassword = EncryptionHandler.EncryptString(value, ref res);
            }
        }
        [IsSerializedForLocalRepository]
        public string EncryptedALMPassword { get; set; }
        
        [IsSerializedForLocalRepository]
        public Amdocs.Ginger.Core.eTerminologyDicsType TerminologyDictionaryType { get; set; }

        eAppLogLevel mAppLogLevel;
       [IsSerializedForLocalRepository]
        public eAppLogLevel AppLogLevel
        {
            get { return mAppLogLevel; }
            set { mAppLogLevel = value; Reporter.CurrentAppLogLevel = mAppLogLevel; }
        }

        [IsSerializedForLocalRepository]
        public eUserType UserType { get; set; }

        public UserTypeHelper UserTypeHelper { get; set; }
        
        public static string getUserProfileFileName()
        {
            //we just save and load serialized UserProfile objct
            string s = App.LocalApplicationData + @"\Ginger.UserProfile.xml";
            return s;
        }
        
        public void SaveUserProfile()
        {
            try
            {
                SaveRecentAppAgentsMapping();
            }
            catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}"); }

            string UserProfileFileName = getUserProfileFileName();
            this.SaveToFile(UserProfileFileName);
        }

        public void SaveRecentAppAgentsMapping()
        {
            if (mSolution != null)
            {
                //remove last saved mapping for this solution
                string existingSolMapping= RecentAppAgentsMapping.Where(x=> x.Contains(mSolution.Name + "***")==true).FirstOrDefault();
                if(string.IsNullOrEmpty(existingSolMapping) ==false)                
                    RecentAppAgentsMapping.Remove(existingSolMapping);

                //create new save to this solution
                existingSolMapping= mSolution.Name + "***";                
                foreach (ApplicationPlatform ap in mSolution.ApplicationPlatforms)
                {
                    if (string.IsNullOrEmpty(ap.LastMappedAgentName) == false)
                        existingSolMapping+= ap.AppName + "," + ap.LastMappedAgentName + "#";
                }
                RecentAppAgentsMapping.Add(existingSolMapping);
            }
        }

        public void LoadRecentAppAgentMapping()
        {
            if (mSolution != null)
            {
                //unserialize the current solution mapping saving
                string existingSolMapping = RecentAppAgentsMapping.Where(x => x.Contains(mSolution.Name + "***") == true).FirstOrDefault();
                if (string.IsNullOrEmpty(existingSolMapping))
                    return;//no saved mapping
                else
                {
                    string solName = mSolution.Name + "***";
                    existingSolMapping = existingSolMapping.Replace(solName,string.Empty);
                    List<string> appAgentMapping = existingSolMapping.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    Dictionary<string, string> mappingDic = new Dictionary<string, string>();
                    foreach (string mapping in appAgentMapping)
                    {
                        string[] appAgent = mapping.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        if (appAgent.Length == 2)
                        {
                            mappingDic.Add(appAgent[0], appAgent[1]);
                        }
                    }

                    //update the solution apps with the saved mapping
                    if (mappingDic.Count > 0)
                    {
                        foreach (ApplicationPlatform ap in mSolution.ApplicationPlatforms)
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

            string UserProfilePath = getUserProfileFileName();
            string InstallationConfigurationPath = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace("Ginger.exe", "Ginger.InstallationConfiguration.Json");
            DateTime InstallationDT = File.GetLastWriteTime(InstallationConfigurationPath);
            DateTime UserProfileDT = File.GetLastWriteTime(UserProfilePath);

            string UserConfigJsonString = string.Empty;
            JObject UserConfigJsonObj = null;
            Dictionary<string, string> UserConfigdictObj = null;
            if (System.IO.File.Exists(InstallationConfigurationPath))
            {
                UserConfigJsonString = System.IO.File.ReadAllText(InstallationConfigurationPath);
                UserConfigJsonObj = JObject.Parse(UserConfigJsonString);
                UserConfigdictObj = UserConfigJsonObj.ToObject<Dictionary<string, string>>();
            }

            if (File.Exists(UserProfilePath))
            {
                try
                {
                    UserProfile up = (UserProfile)RepositoryItem.LoadFromFile(typeof(UserProfile), UserProfilePath);
                    if (DateTime.Compare(UserProfileDT, InstallationDT) < 0)
                    {
                        if (UserConfigdictObj != null)
                            up.AddUserConfigProperties(UserConfigdictObj);
                    }
                    return up;
                }
                catch (Exception e)
                {
                    Reporter.ToUser(eUserMsgKeys.UserProfileLoadError, e.Message);
                }
            }
            
            UserProfile up2 = new UserProfile();
            up2.LoadDefaults();
            if (UserConfigdictObj != null)
                up2.AddUserConfigProperties(UserConfigdictObj);
            up2.ValidateProfile();
            return up2;
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
                    TerminologyDictionaryType = Amdocs.Ginger.Core.eTerminologyDicsType.Default;
                    break;

                case "Testing":
                    TerminologyDictionaryType = Amdocs.Ginger.Core.eTerminologyDicsType.Testing;
                    break;
                case "Gherkin":
                    TerminologyDictionaryType = Amdocs.Ginger.Core.eTerminologyDicsType.Gherkin;
                    break;
            }
        }

        public void ValidateProfile()
        {
        }

        public void LoadDefaults()
        {
            AutoLoadLastSolution = true; //#Task 160
            SetDefaultWorkingFolder();
        }
        
        public void SetDefaultWorkingFolder()
        {
            LocalWorkingFolder = App.LocalApplicationData + @"\WorkingFolder";
            Directory.CreateDirectory(LocalWorkingFolder);
        }
        
        internal string GetDefaultReport()
        {
            if(!string.IsNullOrEmpty(ReportTemplateName))
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

        [IsSerializedForLocalRepository]
        public bool DoNotAskToUpgradeSolutions { get; set; }

        [IsSerializedForLocalRepository]
        public bool NewHelpLibraryMessgeShown { get; set; }

        [IsSerializedForLocalRepository]
        public bool DoNotAskToRecoverSolutions { get; set; }
    }
}
