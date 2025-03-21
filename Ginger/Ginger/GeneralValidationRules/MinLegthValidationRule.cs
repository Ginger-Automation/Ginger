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

namespace Amdocs.Ginger.ValidationRules
{
    public class MinLegthValidationRule : ValidationRule
    {
        string mMessage;
        string Message
        {
            get
            {
                if (string.IsNullOrEmpty(mMessage))
                {
                    return "Minimum Length allowed is {0}";
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

        int mMinLength;

        public MinLegthValidationRule(int minLength, string message = "")
        {
            mMinLength = minLength;
            Message = message;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {

            if (value is string && ((string)value).Length < mMinLength)
            {
                return new ValidationResult(false, string.Format(Message, mMinLength));
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
}
