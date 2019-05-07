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

        public ValidValueValidationRule()
        {

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
                if (!ValidValues.Contains(Convert.ToInt32(value)))
                {
                    return new ValidationResult(false, String.Format(Message, String.Join(",", ValidValues)));
                }
            }
            catch (Exception e)
            {
                return new ValidationResult(false, String.Format(Message, String.Join(",", ValidValues)));
            }
            return new ValidationResult(true, null);

        }
    }
}
