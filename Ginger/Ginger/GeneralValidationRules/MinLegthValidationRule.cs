using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public MinLegthValidationRule()
        {

        }

        public MinLegthValidationRule(int minLength, string message = "")
        {
            mMinLength = minLength;           
            Message = message;            
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {

            if (((string)value).Length < mMinLength)
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
