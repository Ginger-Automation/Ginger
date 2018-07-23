using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ginger.GeneralValidationRules
{
    public class GridValidationRule: ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if ((int)value == 0)
            {
                return new ValidationResult(false, "Grid must have data");
            }            
            else
            {
                return new ValidationResult(true, null);
            }
        }
    }
}
