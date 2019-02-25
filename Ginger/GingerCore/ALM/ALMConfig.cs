#region License
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

using System.ComponentModel;

namespace GingerCore.ALM
{
    public class ALMConfig : INotifyPropertyChanged
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
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
