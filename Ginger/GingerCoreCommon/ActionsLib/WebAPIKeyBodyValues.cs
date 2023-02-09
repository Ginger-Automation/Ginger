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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;

namespace GingerCore.Actions
{
    public class WebAPIKeyBodyValues : ActInputValue
    {        

        public enum eValueType
        {
            [EnumValueDescription("Text")]
            Text,
            [EnumValueDescription("File")]
            File,
        }

        private eValueType mValueType;
        [IsSerializedForLocalRepository]
        public eValueType ValueType
        {
            get { return mValueType; }

            set
            {
                mValueType = value;
                OnPropertyChanged(nameof(ValueType));
                OnPropertyChanged(nameof(IsBrowseNeeded));
            }
        }

        public bool IsBrowseNeeded
        {
            get
            {
                if (ValueType == eValueType.File)
                    return true;
                else
                    return false;
            }
        }


        
    }
}
