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

namespace Amdocs.Ginger.Repository
{
    public class APIModelBodyKeyValue : RepositoryItemBase
    {        
        [IsSerializedForLocalRepository]
        public string Param { get; set; }
        [IsSerializedForLocalRepository]
        public string Value { get; set; }

        public APIModelBodyKeyValue(string Param, string Value)
        {
            this.Param = Param;
            this.Value = Value;
        }

        public APIModelBodyKeyValue()
        {
        }


        public static partial class Fields
        {
            ///public static string Description = "Description";
            public static string ValueType = "ValueType";
        }

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
                if (mValueType != value)
                {
                    mValueType = value;
                    OnPropertyChanged(Fields.ValueType);
                }
                OnPropertyChanged(nameof(IsBrowseNeeded));
            }
        }

        //public event PropertyChangedEventHandler PropertyChanged;
        //public void OnPropertyChanged(string name)
        //{
        //    PropertyChangedEventHandler handler = PropertyChanged;
        //    if (handler != null)
        //    {
        //        handler(this, new PropertyChangedEventArgs(name));
        //    }
        //}

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

        public override string ItemName { get { return Param; } set { } }
    }
}


