#region License
/*
Copyright © 2014-2026 European Support Limited

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
    public class ZAPConfiguration : RepositoryItemBase
    {
        private string mZAPApiKey;
        [IsSerializedForLocalRepository]
        public string ZAPApiKey
        {
            get { return mZAPApiKey; }
            set
            {
                if (mZAPApiKey != value)
                {
                    mZAPApiKey = value;
                    OnPropertyChanged(nameof(ZAPApiKey));
                }
            }
        }

        private string mName= "ZAPConfiguration";
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

        private string mZAPUrl;
        [IsSerializedForLocalRepository]
        public string ZAPUrl
        {
            get
            {
                return mZAPUrl;
            }
            set
            {
                if (value != null && value.EndsWith("/"))
                {
                    value = value.TrimEnd('/');
                }

                if (mZAPUrl != value)
                {
                    mZAPUrl = value;
                    OnPropertyChanged(nameof(ZAPUrl));
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
        public override string GetNameForFileName() { return Name; }
    }
}
