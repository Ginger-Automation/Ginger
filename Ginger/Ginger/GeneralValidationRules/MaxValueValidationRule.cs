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
