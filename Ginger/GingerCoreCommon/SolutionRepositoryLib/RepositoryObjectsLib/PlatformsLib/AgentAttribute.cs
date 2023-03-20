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

using System;

namespace GingerCore
{
    //This class is for annotation of Agent configurable props
    public class UserConfiguredAttribute : Attribute
    {
        public override string ToString()
        {
            return "Is Configured";
        }
    }

    public class UserConfiguredDefaultAttribute : Attribute
    {
        public string DefaultValue { get; set; }

        public UserConfiguredDefaultAttribute(string DefaultValue)
        {
            this.DefaultValue = DefaultValue;
        }
    }

    public class UserConfiguredDescriptionAttribute : Attribute
    {
        public string Description { get; set; }

        public UserConfiguredDescriptionAttribute(string UserConfiguredDescription)
        {
            this.Description = UserConfiguredDescription;
        }
    }

    public class UserConfiguredMultiValuesAttribute : Attribute
    {
        public override string ToString()
        {
            return "Multi Values";
        }
    }

    public class UserConfiguredEnumTypeAttribute : Attribute
    {
        public Type EnumType { get; set; }

        public UserConfiguredEnumTypeAttribute(Type UserConfiguredEnumType)
        {
            this.EnumType = UserConfiguredEnumType;
        }
    }
}
