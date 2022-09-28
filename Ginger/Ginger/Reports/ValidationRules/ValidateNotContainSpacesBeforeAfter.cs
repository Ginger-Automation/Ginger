using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Globalization;

namespace Ginger.Reports.ValidationRules
{
    public class ValidateNotContainSpacesBeforeAfter: ValidationRule
    {
        private string _Message = string.Empty;

        public ValidateNotContainSpacesBeforeAfter(string message = "Name can't contains spaces before and\\or after")
        {
            _Message = message;

            //this.ValidatesOnTargetUpdated = true; // Trigger the validation on init binding (load/init form)
            //this.ValidationStep = ValidationStep.UpdatedValue; // force the rule to run after the new value is converted and written back (fix for issue: property not updated/binded on empty value)
        }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string validateValue = value.ToString();

            if (validateValue.StartsWith(' ') || validateValue.EndsWith(' '))
            {
                return new ValidationResult(false, _Message);
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
}
