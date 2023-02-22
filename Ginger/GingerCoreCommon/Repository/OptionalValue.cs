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

namespace Amdocs.Ginger.Repository
{
    public class OptionalValue : RepositoryItemBase
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

        bool mIsDefault;
        [IsSerializedForLocalRepository]
        public bool IsDefault
        {
            get
            {
                return mIsDefault;
            }
            set
            {
                mIsDefault = value;                
                //OnPropertyChanged(nameof(IsDefault));
            }
        }


        public OptionalValue(string value)
        {
            this.Value = value;
        }

        public OptionalValue()
        {
        }

        public override string ItemName
        {
            get
            {
                return Value;
            }
            set
            {
                Value = value;
            }
        }
    }
}
