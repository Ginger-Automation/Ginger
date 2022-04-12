using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ginger.ValidationRules
{ 
    public class ValidateNumberInputRule : ValidationRule
    {
        private string _Message = string.Empty;

        public ValidateNumberInputRule(string message = "Value must be numeric")
        {
            _Message = message;
        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            try
            {
                if (value != null && !string.IsNullOrEmpty(value?.ToString()))
                {
                    int iValue = int.Parse(value.ToString());                    
                }

                return new ValidationResult(true, null);
            }
            catch (Exception err)
            {
                return new ValidationResult(false, _Message);
            }
        }

        private bool IsActivityNameValid(object value)
        {
            return value == null || string.IsNullOrEmpty(value.ToString()) || string.IsNullOrWhiteSpace(value.ToString());
        }
    }
}
