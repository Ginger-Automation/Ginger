#region License
/*
Copyright © 2014-2020 European Support Limited

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

using Amdocs.Ginger.Repository;
using GingerCore;
using System;
using System.ComponentModel;

namespace GingerCoreNET.ALMLib
{
    public class ALMConfig : RepositoryItemBase
    {
        private string mALMUserName;
        public string ALMUserName
        {
            get { return mALMUserName; }
            set
            {
                if (mALMUserName != value)
                {
                    mALMUserName = value;
                    OnPropertyChanged(nameof(ALMUserName));
                }
            }
        }

        private string mALMDomain;
        [IsSerializedForLocalRepository]
        public string ALMDomain
        {
            get { return mALMDomain; }
            set
            {
                if (mALMDomain != value)
                {
                    mALMDomain = value;
                    OnPropertyChanged(nameof(ALMDomain));
                }
            }
        }

        private bool mUseRest;
        [IsSerializedForLocalRepository]
        public bool UseRest
        {
            get { return mUseRest; }
            set
            {
                if (mUseRest != value)
                {
                    mUseRest = value;
                    OnPropertyChanged(nameof(UseRest));
                }
            }
        }

        private string mALMProjectName;
        [IsSerializedForLocalRepository]
        public string ALMProjectName
        {
            get { return mALMProjectName; }
            set
            {
                if (mALMProjectName != value)
                {
                    mALMProjectName = value;
                    OnPropertyChanged(nameof(ALMProjectName));
                }
            }
        }

        private ALMIntegration.eTestingALMType mJiraTestingALM = ALMIntegration.eTestingALMType.None;
        [IsSerializedForLocalRepository]
        public ALMIntegration.eTestingALMType JiraTestingALM
        {
            get
            {
                return mJiraTestingALM;
            }
            set
            {
                mJiraTestingALM = value;
            }
        }


        private string mALMPassword;
        public string ALMPassword
        {
            get { return mALMPassword; }
            set
            {
                if (mALMPassword != value)
                {
                    mALMPassword = value;
                    OnPropertyChanged(nameof(ALMPassword));
                }
            }
        }

        private string mALMServerURL;
        [IsSerializedForLocalRepository]
        public string ALMServerURL
        {
            get { return mALMServerURL; }
            set
            {
                if (mALMServerURL != value)
                {
                    mALMServerURL = value;
                    OnPropertyChanged(nameof(ALMServerURL));
                }
            }
        }

        private string mALMProjectKey;
        [IsSerializedForLocalRepository]
        public string ALMProjectKey
        {
            get { return mALMProjectKey; }
            set
            {
                if (mALMProjectKey != value)
                {
                    mALMProjectKey = value;
                    OnPropertyChanged(nameof(mALMProjectKey));
                }
            }
        }

        private string mALMConfigPackageFolderPath;
        [IsSerializedForLocalRepository]
        public string ALMConfigPackageFolderPath
        {
            get { return mALMConfigPackageFolderPath; }
            set
            {
                if (mALMConfigPackageFolderPath != value)
                {
                    mALMConfigPackageFolderPath = value;
                    OnPropertyChanged(nameof(mALMConfigPackageFolderPath));
                }
            }
        }

        private bool mDefaultAlm;
        [IsSerializedForLocalRepository]
        public bool DefaultAlm
        {
            get { return mDefaultAlm; }
            set
            {
                if (mDefaultAlm != value)
                {
                    mDefaultAlm = value;
                    OnPropertyChanged(nameof(DefaultAlm));
                }
            }
        }

        private ALMIntegration.eALMType mAlmType = ALMIntegration.eALMType.QC;
        [IsSerializedForLocalRepository]
        public ALMIntegration.eALMType AlmType
        {
            get
            {
                return mAlmType;
            }
            set
            {
                mAlmType = value;
            }
        }

        public override string ItemName { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    }
}

