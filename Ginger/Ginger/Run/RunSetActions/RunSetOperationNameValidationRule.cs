#region License
/*
Copyright © 2014-2025 European Support Limited

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

using System.Globalization;
using System.Windows.Controls;

namespace Ginger.Run.RunSetActions
{
    public class RunSetOperationNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return IsOperationNameValid(value) ?
                new ValidationResult(false, "Operation Name cannot be empty")
                : new ValidationResult(true, null);
        }

        private bool IsOperationNameValid(object value)
        {
            return value == null || string.IsNullOrEmpty(value.ToString()) || string.IsNullOrWhiteSpace(value.ToString());
        }
    }
}
