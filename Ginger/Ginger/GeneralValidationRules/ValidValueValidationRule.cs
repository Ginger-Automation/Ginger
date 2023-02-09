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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Amdocs.Ginger.ValidationRules
{
    public class ValidValueValidationRule : ValidationRule
    {

        string mMessage;
        string Message
        {
            get
            {
                if (string.IsNullOrEmpty(mMessage))
                {
                    return "Only '{0}' values are valid";
                }
                else
                {
                    return mMessage;
                }
            }
            set
            {
                mMessage = value;
            }
        }

        List<int> mvalidValues = new List<int>();
        public List<int> ValidValues
        {
            get
            {
                return mvalidValues;
            }
            set
            {
                mvalidValues = value;
            }
        }

        public ValidValueValidationRule(List<int> validValues, string message = "")
        {
            ValidValues.AddRange(validValues);
            Message = message;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {            
            try
            {                             
                if (value is int && !ValidValues.Contains(Convert.ToInt32(value)))
                {
                    return new ValidationResult(false, String.Format(Message, String.Join(", ", ValidValues)));
                }
            }
            catch (Exception e)
            {
                return new ValidationResult(false, String.Format(Message, String.Join(", ", ValidValues)));
            }
            return new ValidationResult(true, null);

        }
    }
}
