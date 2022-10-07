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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ginger.ValidationRules
{ 
    public class ValidateDecimalNumberInputRule : ValidationRule
    {
        private string _Message = string.Empty;

        public ValidateDecimalNumberInputRule(string message = "Value must be numeric")
        {
            _Message = message;
        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            try
            {
                if (value != null && !string.IsNullOrEmpty(value?.ToString()))
                {
                    double iValue = double.Parse(value.ToString());                    
                }

                return new ValidationResult(true, null);
            }
            catch (Exception err)
            {
                return new ValidationResult(false, _Message);
            }
        }
    }
}
