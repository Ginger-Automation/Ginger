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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    [System.AttributeUsage(System.AttributeTargets.All, AllowMultiple = true)]

    public class ValidValueAttribute : Attribute, IParamProperty
    {
        public string PropertyName => "ValidValue";

        public List<object> ValidValue { get; set; } = new List<object>();

        public ValidValueAttribute()
        {

        }

        public ValidValueAttribute(int validValue)
        {
            
            ValidValue.Add(validValue);
        }

        public ValidValueAttribute(int[] validValues)
        { 
            ValidValue.AddRange(validValues.Cast<object>());
        }

        public ValidValueAttribute(string[] validValues)
        {
            ValidValue.AddRange(validValues.Cast<object>());
        }
        public ValidValueAttribute(bool[] validValues)
        {
            ValidValue.AddRange(validValues.Cast<object>());
        }        
    }
}
