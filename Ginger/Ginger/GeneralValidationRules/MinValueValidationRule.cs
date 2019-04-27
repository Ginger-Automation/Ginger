using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Amdocs.Ginger.ValidationRules
{
    public class MinValueValidationRule : ValidationRule
    {        
        string mMessage;
        string Message
        {
            get
            {
                if (string.IsNullOrEmpty(mMessage))
                {
                    return "Minimum Value allowed is {0}";
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

        int mMinValue;        

        public MinValueValidationRule()
        {

        }

        public MinValueValidationRule(int minValue, string message = "")
        {
            mMinValue = minValue;            
            Message = message;           
        }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                if (Convert.ToInt32(value) < mMinValue)
                {
                    return new ValidationResult(false, string.Format(Message, mMinValue));
                }
                else
                {
                    return new ValidationResult(true, null);
                }
            }
            catch (Exception e)
            {
                return new ValidationResult(false, "Enter valid data");
            }
        }
    }
}
