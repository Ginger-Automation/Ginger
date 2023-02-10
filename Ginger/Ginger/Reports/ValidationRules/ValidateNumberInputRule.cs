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
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ginger.ValidationRules
{ 
    public class ValidateNumberInputRule : ValidationRule
    {
        private string _Message = string.Empty;

        public ValidateNumberInputRule(string message = "Value must be numeric")
        {
            _Message = message;
        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            try
            {
                if (value != null && !string.IsNullOrEmpty(value?.ToString()))
                {
                    int iValue = int.Parse(value.ToString());                    
                }

                return new ValidationResult(true, null);
            }
            catch (Exception err)
            {
                return new ValidationResult(false, _Message);
            }
        }

        private bool IsActivityNameValid(object value)
        {
            return value == null || string.IsNullOrEmpty(value.ToString()) || string.IsNullOrWhiteSpace(value.ToString());
        }
    }
}
