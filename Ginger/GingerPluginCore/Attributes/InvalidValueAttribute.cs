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
using System.Text;

namespace Amdocs.Ginger.Plugin.Core
{
    // Enable to define Valid values for int param
    // [InvalidValue(5)]   // 5 is not valid value for this param
    // [InvalidValues(new int[] { 1, 5, 10})]   // 1,5,10 are not valid values for this param
    

    // Can set multiple times per param
    [System.AttributeUsage(System.AttributeTargets.Parameter, AllowMultiple = true)]
    public class InvalidValueAttribute : Attribute, IParamProperty
    {
        // when saved to services json the attr property name will be:
        public string PropertyName => "InvalidValue";
        
        public List<int> InvalidValue { get; set; }

        public InvalidValueAttribute(int invalidValue)
        {
            if (InvalidValue == null)
            {
                InvalidValue = new List<int>();
            }
            InvalidValue.Add(invalidValue);
        }

        public InvalidValueAttribute(int[] invalidValues)
        {
            if (InvalidValue == null)
            {
                InvalidValue = new List<int>();
            }
            InvalidValue.AddRange(invalidValues);            
        }

        public InvalidValueAttribute()
        {
        }
    }
}
