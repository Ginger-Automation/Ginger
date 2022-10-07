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

using Amdocs.Ginger.Repository;
using Amdocs.Ginger.Common;
using GingerCore;
using Amdocs.Ginger.Common.InterfacesLib;

namespace Ginger.Configurations
{
    public class VRTConfiguration : RepositoryItemBase
    {
        private string mApiUrl;
        [IsSerializedForLocalRepository]
        public string ApiUrl
        {
            get
            {
                return mApiUrl;
            }
            set
            {
                mApiUrl = value;
                OnPropertyChanged(nameof(ApiUrl));
            }
        }


        private string mApiKey;
        [IsSerializedForLocalRepository]
        public string ApiKey
        {
            get
            {
                return mApiKey;
            }
            set
            {
                mApiKey = value;
                OnPropertyChanged(nameof(ApiKey));
            }
        }

        private string mProject;
        [IsSerializedForLocalRepository]
        public string Project
        {
            get
            {
                return mProject;
            }
            set
            {
                mProject = value;
                OnPropertyChanged(nameof(Project));
            }
        }

        private string mBranchName;
        [IsSerializedForLocalRepository]
        public string BranchName
        {
            get
            {
                return mBranchName;
            }
            set
            {
                mBranchName = value;
                OnPropertyChanged(nameof(BranchName));
            }
        }

        private string mDifferenceTolerance = "0.0";
        [IsSerializedForLocalRepository]
        public string DifferenceTolerance
        {
            get
            {
                return mDifferenceTolerance;
            }
            set
            {
                mDifferenceTolerance = value;
                OnPropertyChanged(nameof(DifferenceTolerance));
            }
        }

        public enum eFailActionOnCheckpointMismatch
        {
            Yes,
            No
        }
        private eFailActionOnCheckpointMismatch mFailActionOnCheckpointMismatch = eFailActionOnCheckpointMismatch.Yes;

        [IsSerializedForLocalRepository]
        public eFailActionOnCheckpointMismatch FailActionOnCheckpointMismatch
        {
            get
            {
                return mFailActionOnCheckpointMismatch;
            }
            set
            {
                mFailActionOnCheckpointMismatch = value;
            }
        }
        public override string ItemName { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        private bool? mOS;
        [IsSerializedForLocalRepository(true)]
        public bool OS
        {
            get
            {
                if (mOS.HasValue)
                {
                    return mOS.Value;
                }
                else { return true; }
            }
            set
            {
                mOS = value;
                OnPropertyChanged(nameof(OS));
            }
        }
        private bool? mAgent;
        [IsSerializedForLocalRepository(true)]
        public bool Agent
        {
            get
            {
                if (mAgent.HasValue)
                {
                    return mAgent.Value;
                }
                else { return true; }
            }
            set
            {
                mAgent = value;
                OnPropertyChanged(nameof(Agent));
            }
        }
        private bool? mEnvironment;
        [IsSerializedForLocalRepository(true)]
        public bool Environment
        {
            get
            {
                if (mEnvironment.HasValue)
                {
                    return mEnvironment.Value;
                }
                else { return true; }
            }
            set
            {
                mEnvironment = value;
                OnPropertyChanged(nameof(Environment));
            }
        }
        private bool? mViewport;
        [IsSerializedForLocalRepository(true)]
        public bool Viewport
        {
            get
            {
                if (mViewport.HasValue)
                {
                    return mViewport.Value;
                }
                else { return true; }
            }
            set
            {
                mViewport = value;
                OnPropertyChanged(nameof(Viewport));
            }
        }
        private bool? mActivityTags;
        [IsSerializedForLocalRepository(true)]
        public bool ActivityTags
        {
            get
            {
                if (mActivityTags.HasValue)
                {
                    return mActivityTags.Value;
                }
                else { return true; }
            }
            set
            {
                mActivityTags = value;
                OnPropertyChanged(nameof(ActivityTags));
            }
        }
        #region General

        #endregion
    }
}
