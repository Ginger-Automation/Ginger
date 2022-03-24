using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ginger.Run.RunSetActions
{
    public class RunSetOperationNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return IsOperationNameValid(value) ?
                new ValidationResult(false, "Operation Name cannot be empty")
                : new ValidationResult(true, null);
        }

        private bool IsOperationNameValid(object value)
        {
            return value == null || string.IsNullOrEmpty(value.ToString()) || string.IsNullOrWhiteSpace(value.ToString());
        }
    }
}
