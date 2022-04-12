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

namespace Ginger.ValidationRules
{
    public class TextBoxEmptyAndDependentValidationRule : ValidationRule
    {
        private string _Message = string.Empty;
        private object _DependentObj = null;
        private string _DependentObjField = string.Empty;

        public TextBoxEmptyAndDependentValidationRule(string message = "Value must be provided")
        {
            _Message = message;
        }

        public TextBoxEmptyAndDependentValidationRule(object dependentObj, string dependentObjField, string message = "Value must be provided")
        {
            _Message = message;
            _DependentObj = dependentObj;
            _DependentObjField = dependentObjField;
        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (_DependentObj != null && !string.IsNullOrEmpty(_DependentObjField))
            {
                if (String.IsNullOrEmpty(value.ToString()) && String.IsNullOrEmpty(_DependentObj.GetType().GetProperty(_DependentObjField)?.GetValue(_DependentObj)?.ToString()))
                {
                    return new ValidationResult(false, _Message);
                }
                else
                {
                    return new ValidationResult(true, null);
                }
            }
            else if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return new ValidationResult(false, _Message);
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
    
}
