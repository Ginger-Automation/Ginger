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

    public class GingerOpsConfiguration : RepositoryItemBase
    {
        public delegate void GingerOpsConfigurationChangedEvent();


        private string mName = "GingerOpsConfig";
        [IsSerializedForLocalRepository]
        public string Name
        {
            get { return mName; }
            set
            {
                if (mName != value)
                {
                    mName = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private string mAccountUrl;
        [IsSerializedForLocalRepository]
        public string AccountUrl
        {
            get
            {
                return mAccountUrl;
            }
            set
            {
                if (value != null && value.EndsWith("/"))
                {
                    value = value.TrimEnd('/');
                }

                if (mAccountUrl != value)
                {
                    mAccountUrl = value;
                    OnPropertyChanged(nameof(AccountUrl));
                }
            }
        }

        private string mIdentityServiceURL;
        [IsSerializedForLocalRepository]
        public string IdentityServiceURL
        {
            get
            {
                return mIdentityServiceURL;
            }
            set
            {
                if (value != null && value.EndsWith("/"))
                {
                    value = value.TrimEnd('/');
                }

                if (mIdentityServiceURL != value)
                {
                    mIdentityServiceURL = value;
                    OnPropertyChanged(nameof(IdentityServiceURL));
                    InvalidateToken();
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
                    InvalidateToken();
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
                    InvalidateToken();
                }
            }
        }

        private string mToken = string.Empty;

        public string Token
        {
            get { return mToken; }
            set
            {
                if (mToken != value)
                {
                    mToken = value;
                }
            }
        }

        public override string ItemName
        {
            get
            {
                return this.Name;
            }
            set
            {
                this.Name = value;
            }
        }

        private void InvalidateToken()
        {
            if (!string.IsNullOrEmpty(mToken))
            {
                mToken = string.Empty;
            }
        }

        public override string GetNameForFileName() { return Name; }

    }
}
