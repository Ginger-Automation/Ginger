using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Amdocs.Ginger.ValidationRules
{
    public class MaximumValidationRule : ValidationRule
    {
        string mMessage = "Maximum Length allowed is {0}";

        int mMaxValue;

        int mCurrentValue;

        public MaximumValidationRule()
        {

        }

        public MaximumValidationRule(int maxValue, string message = "")
        {           
            mMaxValue = maxValue;
            if(!string.IsNullOrEmpty(message))
            {
                mMessage = message;
            }                
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {        
            try
            {
                mCurrentValue = Convert.ToInt32(value);
            }
            catch(Exception e)
            {
                mCurrentValue = Convert.ToString(value).Length;
            }
            if (mCurrentValue > mMaxValue)
            {
                return new ValidationResult(false, string.Format(mMessage, mMaxValue));
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
}
