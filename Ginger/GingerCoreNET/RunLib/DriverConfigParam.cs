#region License
/*
Copyright © 2014-2019 European Support Limited

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
using System.Collections.Generic;

namespace GingerCore
{
    public class DriverConfigParam : RepositoryItemBase
    {        

        public static partial class Fields
        {
            public static string Parameter = "Parameter";
            public static string Value = "Value";
            public static string Description = "Description";         
        }

        string mParameter;
        [IsSerializedForLocalRepository]
        public string Parameter { get { return mParameter; } set { if (mParameter != value) { mParameter = value; OnPropertyChanged(nameof(Parameter)); } } }

        [IsSerializedForLocalRepository(false)]
        public bool IsPlatformParameter = false;

        public string mValue;
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

        string mDescription;
        public string Description { get { return mDescription; } set { if (mDescription != value) { mDescription = value; OnPropertyChanged(nameof(Description)); } } }

        public override string ItemName
        {
            get
            {
                return this.Parameter;
            }
            set
            {
                this.Parameter = value;
            }
        }

        public string Type { get; set; }
        public List<string> OptionalValues { get; set; }
    }
}
