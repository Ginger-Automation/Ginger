using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ginger.Actions.Validation
{
    public class UCValueExpressionValidationRule : ValidationRule
    {
        private string _Message;
        public UCValueExpressionValidationRule(string message)
        {
            _Message = message;
        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return new ValidationResult(false, _Message);
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }

        private bool IsActivityNameValid(object value)
        {
            return value == null || string.IsNullOrEmpty(value.ToString()) || string.IsNullOrWhiteSpace(value.ToString());
        }
    }
}
