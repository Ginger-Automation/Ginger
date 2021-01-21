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
using System.Collections.Generic;
using System.Text;

namespace GingerCoreNET.ALMLib
{
    public class ALMUserConfig : RepositoryItemBase
    {
        private string mALMConfigPackageFolderPath;
        public string ALMConfigPackageFolderPath
        {
            get { return mALMConfigPackageFolderPath; }
            set
            {
                if (mALMConfigPackageFolderPath != value)
                {
                    mALMConfigPackageFolderPath = value;
                    OnPropertyChanged(nameof(ALMConfigPackageFolderPath));
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

        private string mALMUserName;
        [IsSerializedForLocalRepository]
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

        [IsSerializedForLocalRepository]
        public string EncryptedALMPassword { get; set; }

        public string ALMPassword
        {
            get
            {
                bool res = false;
                string pass = EncryptionHandler.DecryptString(EncryptedALMPassword, ref res);
                if (res && !String.IsNullOrEmpty(pass))
                {
                    return pass;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                bool res = false;
                EncryptedALMPassword = EncryptionHandler.EncryptString(value, ref res);
            }
        }
        private string mALMToken;
        [IsSerializedForLocalRepository]
        public string ALMToken
        {
            get { return mALMToken; }
            set
            {
                if (mALMToken != value)
                {
                    mALMToken = value;
                    OnPropertyChanged(nameof(ALMToken));
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
