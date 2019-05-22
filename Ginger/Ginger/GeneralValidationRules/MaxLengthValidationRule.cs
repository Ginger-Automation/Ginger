using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Amdocs.Ginger.ValidationRules
{
    public class MaxLengthValidationRule : ValidationRule
    {       
        string mMessage;
        string Message
        {
            get
            {
                if (string.IsNullOrEmpty(mMessage))
                {
                    return "Maximum Length allowed is {0}";
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

        int mMaxLength;
       
        public MaxLengthValidationRule(int maxLength, string message = "")
        {
            mMaxLength = maxLength;            
            Message = message;            
        }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is string && ((string)value).Length > mMaxLength)
            {
                return new ValidationResult(false, string.Format(Message, mMaxLength));
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
}
