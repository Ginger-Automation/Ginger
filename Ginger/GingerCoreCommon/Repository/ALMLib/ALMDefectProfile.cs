#region License
/*
Copyright © 2014-2021 European Support Limited

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

using System;
using Amdocs.Ginger.Common;
using GingerCoreNET.ALMLib;

namespace Amdocs.Ginger.Repository
{
    public class ALMDefectProfile : RepositoryItemBase
    {
        [IsSerializedForLocalRepository]
        public bool IsDefault { get; set; }

        [IsSerializedForLocalRepository]
        public bool ShowAllIterationsElements { get; set; }

        [IsSerializedForLocalRepository]
        public bool UseLocalStoredStyling { get; set; }

        [IsSerializedForLocalRepository]
        public string LogoBase64Image { get; set; }

        [IsSerializedForLocalRepository]
        public int ID { get; set; }

        [IsSerializedForLocalRepository]
        public bool ToUpdate { get; set; }

        [IsSerializedForLocalRepository]
        public string Name { get; set; }

        [IsSerializedForLocalRepository]
        public string Description { get; set; }

        [IsSerializedForLocalRepository]
        public ALMIntegration.eALMType AlmType { get; set; }


        [IsSerializedForLocalRepository]
        public string ReportLowerLevelToShow { get; set; }

        [IsSerializedForLocalRepository]
        public int ALMDefectProfilesSeq { get; set; }

        [IsSerializedForLocalRepository]
        public ObservableList<ExternalItemFieldBase> ALMDefectProfileFields = new ObservableList<ExternalItemFieldBase>();

        public RepositoryFolder<ALMDefectProfile> ALMDefectProfileFolder;

        public override string GetNameForFileName()
        {
            return Name;
        }

        private string _ALMDefectProfile = string.Empty;
        public override string ItemName
        {
            get
            {
                return _ALMDefectProfile;
            }
            set
            {
                _ALMDefectProfile = value;
            }
        }

        public override string GetItemType()
        {
            return nameof(ALMDefectProfile);
        }

        public override void PostDeserialization()
        {
            if (this.AlmType == 0)
            {
                string almType = TargetFrameworkHelper.Helper.GetALMConfig();
                Enum.TryParse(almType, out ALMIntegration.eALMType AlmType);
                this.AlmType = AlmType;
            }
        }
        
    }
}
