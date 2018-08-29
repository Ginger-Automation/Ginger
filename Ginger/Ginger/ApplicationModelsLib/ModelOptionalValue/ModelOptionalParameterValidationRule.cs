using System;
using System.Windows.Controls;

namespace Ginger.ApplicationModelsLib.ModelOptionalValue
{
    public class ModelOptionalParameterValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (string.IsNullOrEmpty(Convert.ToString(value)))
            {
                return new ValidationResult(false, "Please provide the value"); 
            }
            else
            {
                return new ValidationResult(true, string.Empty);
            }
        }
    }
}
