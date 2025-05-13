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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.SolutionGeneral;
using GingerCore;
using GingerCore.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace Ginger
{
    public class UserProfileOperations : IUserProfileOperations
    {
        public UserProfile UserProfile;
        public UserProfileOperations(UserProfile UserProfile)
        {
            this.UserProfile = UserProfile;
            this.UserProfile.UserProfileOperations = this;
        }
        private void CleanRecentSolutionsList()
        {
            try
            {
                //Clean not exist Solutions
                for (int i = 0; i < UserProfile.RecentSolutions.Count; i++)
                {
                    if (Directory.Exists(UserProfile.RecentSolutions[i]) == false)
                    {
                        UserProfile.RecentSolutions.RemoveAt(i);
                        i--;
                    }
                }

                //clean recent solutions list from duplications caused due to bug
                for (int i = 0; i < UserProfile.RecentSolutions.Count; i++)
                {
                    for (int j = i + 1; j < UserProfile.RecentSolutions.Count; j++)
                    {
                        if (SolutionRepository.NormalizePath(UserProfile.RecentSolutions[i]) == SolutionRepository.NormalizePath(UserProfile.RecentSolutions[j]))
                        {
                            UserProfile.RecentSolutions.RemoveAt(j);
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

            mRecentSolutionsAsObjects = [];
            int counter = 0;
            foreach (string s in UserProfile.RecentSolutions)
            {
                string SolutionFile = Path.Combine(s, @"Ginger.Solution.xml");
                if (File.Exists(SolutionFile))
                {
                    try
                    {
                        Solution sol = SolutionOperations.LoadSolution(SolutionFile, false);
                        SolutionOperations solutionOperations = new SolutionOperations(sol);
                        sol.SolutionOperations = solutionOperations;

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
            string solPath = UserProfile.RecentSolutions.FirstOrDefault(x => SolutionRepository.NormalizePath(x) == SolutionRepository.NormalizePath(solution.Folder));
            if (solPath != null)
            {
                UserProfile.RecentSolutions.Remove(solPath);
                Solution sol = mRecentSolutionsAsObjects.FirstOrDefault(x => SolutionRepository.NormalizePath(x.Folder) == SolutionRepository.NormalizePath(solution.Folder));
                if (sol != null)
                {
                    mRecentSolutionsAsObjects.Remove(sol);
                }
            }

            // Add it in first place 
            if (UserProfile.RecentSolutions.Count == 0)
            {
                UserProfile.RecentSolutions.Add(solution.Folder);
            }
            else
            {
                UserProfile.RecentSolutions.Insert(0, solution.Folder);
            }

            RecentSolutionsAsObjects.AddToFirstIndex(solution);

            while (UserProfile.RecentSolutions.Count > 10)//to keep list of 10
            {
                UserProfile.RecentSolutions.RemoveAt(10);
            }
        }

        private bool mSharedUserProfileBeenUsed;
        string mUserProfileFilePath;
        public string UserProfileFilePath
        {
            get
            {
                if (mUserProfileFilePath == null)
                {
                    string specificUserProfilePath = Path.Combine(WorkSpace.Instance.LocalUserApplicationDataFolderPath, "Ginger.UserProfile.xml");
                    string sharedUserProfilePath = Path.Combine(WorkSpace.Instance.CommonApplicationDataFolderPath, "Ginger.UserProfile.xml");
                    string sharedUserProfileForUIPath = Path.Combine(WorkSpace.Instance.CommonApplicationDataFolderPath, "GingerUI.UserProfile.xml");
                    if (File.Exists(sharedUserProfileForUIPath))
                    {
                        mUserProfileFilePath = sharedUserProfileForUIPath;
                        Reporter.ToLog(eLogLevel.INFO, string.Format("Shared User Profile for UI is been used, path:'{0}'", sharedUserProfileForUIPath));
                        mSharedUserProfileBeenUsed = true;
                    }
                    else if (WorkSpace.Instance.RunningInExecutionMode && File.Exists(sharedUserProfilePath))
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

        public bool IsSharedUserProfile { get { return mSharedUserProfileBeenUsed; } }

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
            UserProfile.RepositorySerializer.SaveToFile(UserProfile, UserProfileFilePath);
            SavePasswords();
        }

        public void SaveRecentAppAgentsMapping()
        {
            if (WorkSpace.Instance.Solution != null)
            {
                //remove last saved mapping for this solution
                string existingSolMapping = UserProfile.RecentAppAgentsMapping.FirstOrDefault(x => x.Contains(WorkSpace.Instance.Solution.Name + "***") == true);
                if (string.IsNullOrEmpty(existingSolMapping) == false)
                {
                    UserProfile.RecentAppAgentsMapping.Remove(existingSolMapping);
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
                UserProfile.RecentAppAgentsMapping.Add(existingSolMapping);
            }
        }

        public void LoadRecentAppAgentMapping()
        {
            if (WorkSpace.Instance.Solution != null)
            {
                //unserialize the current solution mapping saving
                string existingSolMapping = UserProfile.RecentAppAgentsMapping.FirstOrDefault(x => x.Contains(WorkSpace.Instance.Solution.Name + "***") == true);
                if (string.IsNullOrEmpty(existingSolMapping))
                {
                    return;//no saved mapping
                }
                else
                {
                    string solName = WorkSpace.Instance.Solution.Name + "***";
                    existingSolMapping = existingSolMapping.Replace(solName, string.Empty);
                    List<string> appAgentMapping = existingSolMapping.Split(new char[] { '#' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    Dictionary<string, string> mappingDic = [];
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
                                if (ap != null && WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>().Any())
                                {
                                    List<Agent> platformAgents = (from p in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where p.Platform == ap.Platform select p).ToList();
                                    Agent matchingAgent = platformAgents.FirstOrDefault(x => x.Name == mappingDic[ap.AppName]);
                                    if (matchingAgent != null)
                                    {
                                        ap.LastMappedAgentName = matchingAgent.Name;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public UserProfile LoadPasswords(UserProfile userProfile)
        {
            if (!ValidateWindowOS())
            {
                return userProfile;
            }
            if (WorkSpace.Instance.Solution != null)
            {
                var GingerSolutionSourceControl = WorkSpace.Instance.UserProfile.GetSolutionSourceControlInfo(WorkSpace.Instance.Solution.Guid);

                if (!string.IsNullOrEmpty(GingerSolutionSourceControl.SourceControlInfo.EncryptedPassword))
                {
                    GingerSolutionSourceControl.SourceControlInfo.Password = EncryptionHandler.DecryptwithKey(GingerSolutionSourceControl.SourceControlInfo.EncryptedPassword);
                }
                else
                {
                    GingerSolutionSourceControl.SourceControlInfo.Password = WinCredentialUtil.GetCredential($"Ginger_Sol_Git_{WorkSpace.Instance.Solution.Guid}");
                }
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
            if (!ValidateWindowOS())
            {
                return;
            }
            if (WorkSpace.Instance.Solution != null)
            {
                var GingerSolutionSourceControl = WorkSpace.Instance.UserProfile.GetSolutionSourceControlInfo(WorkSpace.Instance.Solution.Guid);

                if (!string.IsNullOrEmpty(GingerSolutionSourceControl.SourceControlInfo.Password) && !string.IsNullOrEmpty(GingerSolutionSourceControl.SourceControlInfo.Username))
                {
                    WinCredentialUtil.SetCredentials($"Ginger_Sol_Git_{WorkSpace.Instance.Solution.Guid}", GingerSolutionSourceControl.SourceControlInfo.Username, GingerSolutionSourceControl.SourceControlInfo.Password);
                }
            }
            //Save ALM passwords on windows credential manager
            foreach (GingerCoreNET.ALMLib.ALMUserConfig almConfig in UserProfile.ALMUserConfigs.Where(f => !string.IsNullOrEmpty(f.ALMPassword)))
            {
                WinCredentialUtil.SetCredentials("Ginger_ALM_" + almConfig.AlmType, almConfig.ALMUserName, almConfig.ALMPassword);
            }
        }
        private bool ValidateWindowOS()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return true;
            }
            return false;
        
        }
        public void RefreshSourceControlCredentials(Guid solutionGuid)
        {
            if (!ValidateWindowOS())
            {
                return;
            }
            var GingerSolutionSourceControl = WorkSpace.Instance.UserProfile.GetSolutionSourceControlInfo(solutionGuid);
            UserPass userPassObj = WinCredentialUtil.ReadCredential($"Ginger_Sol_Git_{solutionGuid}");
            GingerSolutionSourceControl.SourceControlInfo.Password = userPassObj.Password;
            GingerSolutionSourceControl.SourceControlInfo.Username = userPassObj.Username;
        }

        /// <summary>
        /// Reads old source control credentials from the Windows Credential Manager.
        /// </summary>
        /// <param name="mSourceControl">The source control object to populate with credentials.</param>
        public void ReadOldSourceControlCredentials(GingerCoreNET.SourceControl.SourceControlBase mSourceControl)
        {
            if (!ValidateWindowOS())
            {
                return;
            }
            try
            {
                UserPass userPassObj = WinCredentialUtil.ReadCredential($"Ginger_SolutionSourceControl");
                mSourceControl.Username = userPassObj.Username;
                mSourceControl.Password = userPassObj.Password;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.ToString());
            }
        }


        public void LoadDefaults()
        {
            UserProfile.AutoLoadLastSolution = true; //#Task 160
            UserProfile.AutoLoadLastRunSet = true; //#Task 160
            string defualtFolder = WorkSpace.Instance.DefualtUserLocalWorkingFolder;//calling it so it will be created
        }

        public bool SourceControlUseShellClient { get; set; }

        public bool SourceControlIgnoreCertificate { get; set; }



    }
}
