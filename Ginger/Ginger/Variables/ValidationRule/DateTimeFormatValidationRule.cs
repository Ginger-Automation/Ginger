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
using System;
using System.Globalization;
using System.Windows.Controls;

namespace Ginger.Variables
{
    public class DateTimeFormatValidationRule : ValidationRule
    {
        private VariableDateTime dateTimeVar;
        public DateTimeFormatValidationRule(VariableDateTime value)
        {
            dateTimeVar = value;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                //set date format in datetime field
               var datetime = dateTimeVar.ConvertDateTimeToSpecificFormat(value as string);

                //if dateformat is invalid it will throw exception invalid date format exception
               Convert.ToDateTime(datetime);
            }
            catch (Exception ex)
            {
                return new ValidationResult(false, $"Invalid DateFormat or {ex.Message}");
            }
            return ValidationResult.ValidResult;
        }
    }
}