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
using GingerCore.Variables;
using System.Windows.Controls;

namespace Ginger.Variables
{
    public class NumberValidationRule : ValidationRule
    {
        private VariableNumber variableNumber;

        public NumberValidationRule()
        {
        }

        public NumberValidationRule(VariableNumber variableNumber)
        {
            this.variableNumber = variableNumber;
        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            float result = 0.0f;
            bool canConvert = float.TryParse(value as string, out result);
            if(!canConvert)
            {
                return new ValidationResult(canConvert, "Not a valid Number");
            }
            if(variableNumber != null)
            {
                try
                {
                    if(!variableNumber.CheckNumberInRange(result) )
                    {
                        return new ValidationResult(false, $"Please enter number in the range: MinValue[{variableNumber.MinValue}], MaxValue[{variableNumber.MaxValue}].");
                    }
                }
                catch (System.Exception ex)
                {
                    return new ValidationResult(false, $"Illegal characters or {ex.Message}");
                }
            }
            return ValidationResult.ValidResult;
        }
    }
}
