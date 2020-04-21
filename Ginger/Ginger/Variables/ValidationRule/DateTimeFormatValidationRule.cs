using System;
using System.Globalization;
using System.Windows.Controls;

namespace Ginger.Variables
{
    public class DateTimeFormatValidationRule : ValidationRule
    {
        private DateTime dateTimevalue;
        public DateTimeFormatValidationRule(DateTime value)
        {
            dateTimevalue = value;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
               var dateTtime = dateTimevalue.ToString(value as string, CultureInfo.InvariantCulture);
               Convert.ToDateTime(dateTtime);
            }
            catch (Exception ex)
            {
                return new ValidationResult(false, $"Invalid DateFormat or {ex.Message}");
            }
            return ValidationResult.ValidResult;
        }
    }
}