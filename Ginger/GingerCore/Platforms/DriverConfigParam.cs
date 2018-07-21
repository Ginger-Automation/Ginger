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

using Amdocs.Ginger.Repository;
namespace GingerCore
{
    public class DriverConfigParam : RepositoryItemBase
    {
        public override bool UseNewRepositorySerializer { get { return true; } }

        public static partial class Fields
        {
            public static string Parameter = "Parameter";
            public static string Value = "Value";
            public static string Description = "Description";         
        }

        [IsSerializedForLocalRepository]
        public string Parameter { get; set; }

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
                mValue = value;
                OnPropertyChanged(Fields.Value);
            }
        }

        [IsSerializedForLocalRepository]
        public string Description { get; set; }

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
    }
}
