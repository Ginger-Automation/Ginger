using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ginger.GeneralValidationRules
{
    public class FileValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return new ValidationResult(false, "Value cannot be null");
            }
            else if(!System.IO.File.Exists((string)value))
            {
                return new ValidationResult(false, "File not found");
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
}
