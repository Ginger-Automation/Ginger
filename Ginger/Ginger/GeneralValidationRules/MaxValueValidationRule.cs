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
    public class MaxValueValidationRule : ValidationRule
    {        
        string mMessage;
        string Message
        {
            get
            {
                if (string.IsNullOrEmpty(mMessage))
                {
                    return "Maximum Value allowed is {0}";
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

        int mMaxValue;        
       
        public MaxValueValidationRule(int maxValue, string message = "")
        {           
            mMaxValue = maxValue;
            Message = message;                            
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                if (value is int && Convert.ToInt32(value) > mMaxValue)
                {
                    return new ValidationResult(false, string.Format(Message, mMaxValue));
                }
                else
                {
                    return new ValidationResult(true, null);
                }
            }    
            catch(Exception e)
            {
                return new ValidationResult(false, "Enter valid data");
            }
        }
    }
}
