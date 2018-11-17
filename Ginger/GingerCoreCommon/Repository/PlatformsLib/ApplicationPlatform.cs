﻿#region License
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

using Amdocs.Ginger.Repository;
using System;

namespace GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib
{
    public class ApplicationPlatform : RepositoryItemBase
    {
        [IsSerializedForLocalRepository]
        public Guid GUID { get; set; }

        string mAppName;
        [IsSerializedForLocalRepository]
        public string AppName
        {
            get
            {
                return mAppName;
            }
            set
            {
                if (mAppName != value)
                {
                    mAppName = value;
                    OnPropertyChanged(nameof(AppName));
                }
            }
        }

        // Core is the generic name of the application like: Notepad, we will search packaged based on core app                
        public string Core { get; set; }

        string mCoreVersion;
        [IsSerializedForLocalRepository]
        public string CoreVersion
        {
            get
            {
                return mCoreVersion;
            }
            set
            {
                if (mCoreVersion != value)
                {
                    mCoreVersion = value;
                    OnPropertyChanged(nameof(CoreVersion));
                }
            }
        }

        [IsSerializedForLocalRepository]
        public Guid CoreGUID { get; set; }

        ePlatformType mPlatform;
        [IsSerializedForLocalRepository]
        public ePlatformType Platform
        {
            get
            {
                return mPlatform;
            }
            set
            {
                if (mPlatform != value)
                {
                    mPlatform = value;
                    OnPropertyChanged(nameof(Platform));
                }
            }
        }

        string mDescription;
        [IsSerializedForLocalRepository]
        public string Description
        {
            get
            {
                return mDescription;
            }
            set
            {
                if (mDescription != value)
                {
                    mDescription = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        // No need to serialzed used for temp selection only
        public bool Selected { get; set; }

        // Save the last agent who executed on this App for reloading it as the defult selection next time
        public string LastMappedAgentName { get; set; }

        public string NameBeforeEdit;


        public override string GetNameForFileName()
        {
            return AppName;
        }

        public override string ItemName
        {
            get
            {
                return this.AppName;
            }
            set
            {
                this.AppName = value;
            }
        }
    }
}
