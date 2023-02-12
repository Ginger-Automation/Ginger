#region License
/*
Copyright © 2014-2023 European Support Limited

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
    public class ApplitoolsConfiguration : RepositoryItemBase
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
                if (mApiUrl != value)
                {
                    mApiUrl = value;
                    OnPropertyChanged(nameof(ApiUrl));
                }
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
                if (mApiKey != value)
                {
                    mApiKey = value;
                    OnPropertyChanged(nameof(ApiKey));
                }
            }
        }

        public override string ItemName { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }



        #region General

        #endregion
    }
}
