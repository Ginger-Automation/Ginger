using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Amdocs.Ginger.ValidationRules
{
    public class MinimumValidationRule : ValidationRule
    {
        string mMessage = "Minimum Length allowed is {0}";

        int mMinValue;

        int mCurrentValue;

        public MinimumValidationRule()
        {

        }

        public MinimumValidationRule(int minValue, string message = "")
        {
            mMinValue = minValue;
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
            catch (Exception e)
            {
                mCurrentValue = Convert.ToString(value).Length;
            }
            if (mCurrentValue < mMinValue)
            {
                return new ValidationResult(false, string.Format(mMessage, mMinValue));
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
}
