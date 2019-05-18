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

using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    public class GingerServiceConfigurationAttribute : Attribute
    {
        public readonly string Name;
        public readonly string Description;
        public readonly  Type Type;
        public readonly object DefaultValue;
        public readonly object[] OptionalValues;

        public GingerServiceConfigurationAttribute(string mName, string mDescription,Type mtype,object mDefaultValue,object[] mOptionalValues)
        {
            Name = mName;
            Description = mDescription;
            Type = mtype;
            DefaultValue = mDefaultValue;
            OptionalValues = mOptionalValues;
        }


        public GingerServiceConfigurationAttribute(string mName, string mDescription, Type mtype, object mDefaultValue)
        {
            Name = mName;
            Description = mDescription;
            Type = mtype;
            DefaultValue = mDefaultValue;
        }
    }
}
