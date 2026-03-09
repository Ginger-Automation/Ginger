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

namespace Amdocs.Ginger.Common.VariablesLib
{
    public class OperationValues : RepositoryItemBase
    {
        string mValue;
        [IsSerializedForLocalRepository]
        public string Value
        {
            get
            {
                return mValue;
            }
            set
            {
                if (mValue != value)
                {
                    mValue = value;
                    OnPropertyChanged(nameof(Value));
                }
            }
        }

        string mDisplayName;

        public string DisplayName
        {
            get
            {
                return mDisplayName;
            }
            set
            {
                if (mDisplayName != value)
                {
                    mDisplayName = value;
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        public override string ItemName
        {
            get;
            set;
        }
    }
}
