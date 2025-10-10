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

namespace Amdocs.Ginger.Common.External.Configurations
{
    public class GingerPlayConfiguration : RepositoryItemBase
    {
        public delegate void GingerPlayConfigurationChangedEvent();


        private string mName= "GingerPlay";
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

        private string mGingerPlayGatewayUrl;
        [IsSerializedForLocalRepository]
        public string GingerPlayGatewayUrl
        {
            get { return mGingerPlayGatewayUrl; }
            set
            {
                if (!string.IsNullOrEmpty(value) && !value.EndsWith('/'))
                {
                    value += "/";
                }
                if (mGingerPlayGatewayUrl != value)
                {
                    mGingerPlayGatewayUrl = value;
                    OnPropertyChanged(nameof(GingerPlayGatewayUrl));
                }
            }
        }

        private string mGingerPlayClientId;
        [IsSerializedForLocalRepository]
        public string GingerPlayClientId
        {
            get { return mGingerPlayClientId; }
            set
            {
                if (mGingerPlayClientId != value)
                {
                    mGingerPlayClientId = value;
                    OnPropertyChanged(nameof(GingerPlayClientId));
                }
            }
        }

        private string mGingerPlayClientSecret;
        [IsSerializedForLocalRepository]
        public string GingerPlayClientSecret
        {
            get { return mGingerPlayClientSecret; }
            set
            {
                if (mGingerPlayClientSecret != value)
                {
                    mGingerPlayClientSecret = value;
                    OnPropertyChanged(nameof(GingerPlayClientSecret));
                }
            }
        }

        private bool mGingerPlayEnabled;
        [IsSerializedForLocalRepository]
        public bool GingerPlayEnabled
        {
            get { return mGingerPlayEnabled; }
            set
            {
                if (mGingerPlayEnabled != value)
                {
                    mGingerPlayEnabled = value;
                    OnPropertyChanged(nameof(GingerPlayEnabled));
                }
            }
        }

        private bool mGingerPlayReportServiceEnabled;
        [IsSerializedForLocalRepository]
        public bool GingerPlayReportServiceEnabled
        {
            get { return mGingerPlayReportServiceEnabled; }
            set
            {
                if (mGingerPlayReportServiceEnabled != value)
                {
                    mGingerPlayReportServiceEnabled = value;
                    OnPropertyChanged(nameof(GingerPlayReportServiceEnabled));
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

        private bool mGingerPlayExecutionServiceEnabled;
        [IsSerializedForLocalRepository]
        public bool GingerPlayExecutionServiceEnabled
        {
            get { return mGingerPlayExecutionServiceEnabled; }
            set
            {
                if (mGingerPlayExecutionServiceEnabled != value)
                {
                    mGingerPlayExecutionServiceEnabled = value;
                    OnPropertyChanged(nameof(GingerPlayExecutionServiceEnabled));
                }
            }
        }

        private bool mGingerPlayAIServiceEnabled;
        [IsSerializedForLocalRepository]
        public bool GingerPlayAIServiceEnabled
        {
            get { return mGingerPlayAIServiceEnabled; }
            set
            {
                if (mGingerPlayAIServiceEnabled != value)
                {
                    mGingerPlayAIServiceEnabled = value;
                    OnPropertyChanged(nameof(GingerPlayAIServiceEnabled));
                }
            }
        }

        public string Token = "token";

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

        //For Backward compatibility
        private string mCentralizedAccountReportURL = string.Empty;
        [IsSerializedForLocalRepository]
        public string CentralizedAccountReportURL
        {
            get { return mCentralizedAccountReportURL; }
            set
            {
                if (mCentralizedAccountReportURL != value)
                {
                    mCentralizedAccountReportURL = value;
                    OnPropertyChanged(nameof(CentralizedAccountReportURL));
                }
            }
        }

        private string mCentralizedHTMLReportServiceURL = string.Empty;
        [IsSerializedForLocalRepository]
        public string CentralizedHTMLReportServiceURL
        {
            get { return mCentralizedHTMLReportServiceURL; }
            set
            {
                if (mCentralizedHTMLReportServiceURL != value)
                {
                    mCentralizedHTMLReportServiceURL = value;
                    OnPropertyChanged(nameof(CentralizedHTMLReportServiceURL));
                }
            }
        }

        private string mCentralizedExecutionHandlerURL = string.Empty;
        [IsSerializedForLocalRepository]
        public string CentralizedExecutionHandlerURL
        {
            get { return mCentralizedExecutionHandlerURL; }
            set
            {
                if (mCentralizedExecutionHandlerURL != value)
                {
                    mCentralizedExecutionHandlerURL = value;
                    OnPropertyChanged(nameof(CentralizedExecutionHandlerURL));
                }
            }
        }

        public override string GetNameForFileName() { return Name; }

        public bool IsGingerPlayGateWayConfigured()
        {
            return GingerPlayEnabled &&
                   !string.IsNullOrEmpty(GingerPlayGatewayUrl);
        }

        public bool IsGingerPlayConfigured()
        {
            return GingerPlayEnabled &&
                   !string.IsNullOrEmpty(GingerPlayGatewayUrl) &&
                   !string.IsNullOrEmpty(GingerPlayClientId) &&
                   !string.IsNullOrEmpty(GingerPlayClientSecret);
        }

    }
}
