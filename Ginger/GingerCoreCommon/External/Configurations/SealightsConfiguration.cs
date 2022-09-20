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
    
    public class SealightsConfiguration : RepositoryItemBase
    {
        public enum eSealightsEntityLevel
        {
            [EnumValueDescription("Business Flow")]
            BusinessFlow,
            [EnumValueDescription("Activities Group")]
            ActivitiesGroup,
            [EnumValueDescription("Activity")]
            Activity
        }

        public enum eSealightsLog
        {
            Yes,
            No
        }

        private eSealightsLog mSealightsLog = eSealightsLog.No;

        [IsSerializedForLocalRepository]
        public eSealightsLog SealightsLog
        {
            get
            {
                return mSealightsLog;
            }
            set
            {
                mSealightsLog = value;
            }
        }


        private string mSealightsURL;
        [IsSerializedForLocalRepository]
        public string SealightsURL
        {
            get
            {
                return mSealightsURL;
            }
            set
            {
                mSealightsURL = value;
                OnPropertyChanged(nameof(SealightsURL));
            }
        }


        private string mSealightsAgentToken;
        [IsSerializedForLocalRepository]
        public string SealightsAgentToken
        {
            get
            {
                return mSealightsAgentToken;
            }
            set
            {
                mSealightsAgentToken = value;
                OnPropertyChanged(nameof(SealightsAgentToken));
            }
        }

        private string mSealightsLabId;
        [IsSerializedForLocalRepository]
        public string SealightsLabId
        {
            get
            {
                return mSealightsLabId;
            }
            set
            {
                mSealightsLabId = value;
                OnPropertyChanged(nameof(SealightsLabId));
            }
        }

        private string mSealightsTestStage;
        [IsSerializedForLocalRepository]
        public string SealightsTestStage
        {
            get
            {
                return mSealightsTestStage;
            }
            set
            {
                mSealightsTestStage = value;
                OnPropertyChanged(nameof(SealightsTestStage));
            }
        }

        private string mSealightsBuildSessionID;
        [IsSerializedForLocalRepository]
        public string SealightsBuildSessionID
        {
            get
            {
                return mSealightsBuildSessionID;
            }
            set
            {
                mSealightsBuildSessionID = value;
                OnPropertyChanged(nameof(SealightsBuildSessionID));
            }
        }

        private string mSealightsSessionTimeout;
        [IsSerializedForLocalRepository]
        public string SealightsSessionTimeout
        {
            get
            {
                return mSealightsSessionTimeout;
            }
            set
            {
                mSealightsSessionTimeout = value;
                OnPropertyChanged(nameof(SealightsSessionTimeout));
            }
        }

        private eSealightsEntityLevel mSealightsReportedEntityLevel;
        [IsSerializedForLocalRepository]
        public eSealightsEntityLevel SealightsReportedEntityLevel
        {
            get
            {
                return mSealightsReportedEntityLevel;
            }
            set
            {
                mSealightsReportedEntityLevel = value;
                OnPropertyChanged(nameof(SealightsReportedEntityLevel));
            }
        }
        public enum eSealightsTestRecommendations
        {
            Yes,
            No
        }

        private eSealightsTestRecommendations mSealightsTestRecommendations = eSealightsTestRecommendations.No;

        [IsSerializedForLocalRepository]
        public eSealightsTestRecommendations SealightsTestRecommendations
        {
            get
            {
                return mSealightsTestRecommendations;
            }
            set
            {
                mSealightsTestRecommendations = value;
            }
        }

        public override string ItemName { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }



        #region General

        #endregion
    }
}
