using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Controls;

namespace Ginger.Reports.ValidationRules
{
    public class ValidateNotContainSpecificChar : ValidationRule
    {
        private string _Message = string.Empty;
        private char _CharToDetect;

        public ValidateNotContainSpecificChar(char charToDetect, string message)
        {
            _Message = message;
            _CharToDetect = charToDetect;

            //this.ValidatesOnTargetUpdated = true; // Trigger the validation on init binding (load/init form)
            //this.ValidationStep = ValidationStep.UpdatedValue; // force the rule to run after the new value is converted and written back (fix for issue: property not updated/binded on empty value)
        }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string validateValue = value.ToString();

            if (validateValue.Contains(_CharToDetect))
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
