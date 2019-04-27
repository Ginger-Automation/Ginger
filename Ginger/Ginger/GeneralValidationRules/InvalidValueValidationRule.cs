using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Amdocs.Ginger.ValidationRules
{
    public class InvalidValueValidationRule : ValidationRule
    {
        string mMessage;
        string Message
        {
            get
            {
                if(string.IsNullOrEmpty(mMessage))
                {
                    return "Invalid Value";
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

        List<int> mInvalidValue = new List<int>();
        public List<int> InvalidValue
        {
            get
            {
                return mInvalidValue;
            }
            set
            {
                mInvalidValue = value;
            }
        }

        public InvalidValueValidationRule()
        {

        }

        public InvalidValueValidationRule(List<int> invalidValue, string message = "")
        {
            InvalidValue.AddRange(invalidValue);            
            Message = message;            
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                foreach (int val in InvalidValue)
                {
                    if (val == Convert.ToInt32(value))
                    {
                        return new ValidationResult(false, Message);
                    }
                }
            }
            catch(Exception e)
            {
                return new ValidationResult(false, Message);
            }                      
            return new ValidationResult(true, null);            

        }
    }
}
