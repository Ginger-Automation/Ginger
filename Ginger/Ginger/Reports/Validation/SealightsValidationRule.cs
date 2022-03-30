#region License
/*
Copyright © 2014-2022 European Support Limited

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

using amdocs.ginger.GingerCoreNET;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ginger.Sealights
{
    public class SealightsValidationRule : ValidationRule
    {
        string mFieldName;
        string FieldName
        {
            get
            {
                if (string.IsNullOrEmpty(mFieldName))
                {
                    return "Field";
                }
                else
                {
                    return mFieldName;
                }
            }
            set
            {
                mFieldName = value;
            }
        }

        public SealightsValidationRule(string fieldName = "")
        {
            FieldName = fieldName;
        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                if (FieldName == "Lab ID" || FieldName == "Build Session ID")
                {
                    return new ValidationResult(false, "Lab ID or Build Session ID must be provided");
                }
                else
                {
                    return new ValidationResult(false, FieldName + " cannot be empty");
                }
            }
            else
            {
                return new ValidationResult(true, null);
            }        
        }

    }
    
}
