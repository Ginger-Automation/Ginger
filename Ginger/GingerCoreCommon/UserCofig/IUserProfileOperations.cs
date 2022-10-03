﻿#region License
/*
Copyright © 2014-2022 European Support Limited

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
//using Ginger.SolutionGeneral;

namespace Ginger
{
    public interface IUserProfileOperations
    {
        bool SourceControlIgnoreCertificate { get; set; }
        bool SourceControlUseShellClient { get; set; }
        string UserProfileFilePath { get; }

        void LoadDefaults();
        UserProfile LoadPasswords(UserProfile userProfile);
        void LoadRecentAppAgentMapping();
        void SavePasswords();
        void SaveRecentAppAgentsMapping();
        void SaveUserProfile();
        public void RefreshSourceControlCredentials(GingerCoreNET.SourceControl.SourceControlBase.eSourceControlType argsSourceControlType);
    }
}
