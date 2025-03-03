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

using Amdocs.Ginger.Repository;

namespace Ginger.Configurations
{

    public class AskLisaConfiguration : RepositoryItemBase
    {
        public delegate void AskLisaConfigurationChangedEvent();

        public enum eEnableChatBot
        {
            Yes,
            No
        }

        private eEnableChatBot mEnableChat = eEnableChatBot.No;
        [IsSerializedForLocalRepository]
        public eEnableChatBot EnableChat
        {
            get
            {
                return mEnableChat;
            }
            set
            {
                if (mEnableChat != value)
                {
                    mEnableChat = value;
                    OnPropertyChanged(nameof(EnableChat));
                }
            }
        }


        private string mHost;
        [IsSerializedForLocalRepository]
        public string Host
        {
            get
            {
                return mHost;
            }
            set
            {
                if (mHost != value)
                {
                    mHost = value;
                    OnPropertyChanged(nameof(Host));
                }
            }
        }

        private string mAuthenticationServiceURL;
        [IsSerializedForLocalRepository]
        public string AuthenticationServiceURL
        {
            get
            {
                return mAuthenticationServiceURL;
            }
            set
            {
                if (mAuthenticationServiceURL != value)
                {
                    mAuthenticationServiceURL = value;
                    OnPropertyChanged(nameof(AuthenticationServiceURL));
                }
            }
        }

        private string mClientId;
        [IsSerializedForLocalRepository]
        public string ClientId
        {
            get
            {
                return mClientId;
            }
            set
            {
                if (mClientId != value)
                {
                    mClientId = value;
                    OnPropertyChanged(nameof(ClientId));
                }
            }
        }

        private string mClientSecret;
        [IsSerializedForLocalRepository]
        public string ClientSecret
        {
            get
            {
                return mClientSecret;
            }
            set
            {
                if (mClientSecret != value)
                {
                    mClientSecret = value;
                    OnPropertyChanged(nameof(ClientSecret));
                }
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
                if (mSealightsSessionTimeout != value)
                {
                    mSealightsSessionTimeout = value;
                    OnPropertyChanged(nameof(SealightsSessionTimeout));
                }
            }
        }

        private string mStartNewChat = "AQEQABot/Lisa/StartNewChat";
        [IsSerializedForLocalRepository]
        public string StartNewChat
        {
            get
            {
                return mStartNewChat;
            }
            set
            {
                if (mStartNewChat != value)
                {
                    mStartNewChat = value;
                    OnPropertyChanged(nameof(StartNewChat));
                }
            }
        }

        private string mContinueChat = "AQEQABot/Lisa/ContinueChat";
        [IsSerializedForLocalRepository]
        public string ContinueChat
        {
            get
            {
                return mContinueChat;
            }
            set
            {
                if (mContinueChat != value)
                {
                    mContinueChat = value;
                    OnPropertyChanged(nameof(ContinueChat));
                }
            }
        }

        private string mAccount = "Ginger";
        [IsSerializedForLocalRepository]
        public string Account
        {
            get
            {
                return mAccount;
            }
            set
            {
                if (mAccount != value)
                {
                    mAccount = value;
                    OnPropertyChanged(nameof(Account));
                }
            }
        }

        private string mDomainType = "Knowledge Management";
        [IsSerializedForLocalRepository]
        public string DomainType
        {
            get
            {
                return mDomainType;
            }
            set
            {
                if (mDomainType != value)
                {
                    mDomainType = value;
                    OnPropertyChanged(nameof(DomainType));
                }
            }
        }

        private string mTemperatureLevel = "0.1";
        [IsSerializedForLocalRepository]
        public string TemperatureLevel
        {
            get
            {
                return mTemperatureLevel;
            }
            set
            {
                if (mTemperatureLevel != value)
                {
                    mTemperatureLevel = value;
                    OnPropertyChanged(nameof(TemperatureLevel));
                }
            }
        }

        private string mMaxTokenValue = "2000";
        [IsSerializedForLocalRepository]
        public string MaxTokenValue
        {
            get
            {
                return mMaxTokenValue;
            }
            set
            {
                if (mMaxTokenValue != value)
                {
                    mMaxTokenValue = value;
                    OnPropertyChanged(nameof(MaxTokenValue));
                }
            }
        }

        private string mDataPath = "./Data/Ginger";
        [IsSerializedForLocalRepository]
        public string DataPath
        {
            get
            {
                return mDataPath;
            }
            set
            {
                if (mDataPath != value)
                {
                    mDataPath = value;
                    OnPropertyChanged(nameof(DataPath));
                }
            }
        }

        private string mGrantType = "client_credentials";
        [IsSerializedForLocalRepository]
        public string GrantType
        {
            get
            {
                return mGrantType;
            }
            set
            {
                if (mGrantType != value)
                {
                    mGrantType = value;
                    OnPropertyChanged(nameof(GrantType));
                }
            }
        }

        public string Token = "token";


        public override string ItemName { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

    }
}
