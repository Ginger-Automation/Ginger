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
                //set date format in datetime field
               var datetime = dateTimevalue.ToString(value as string, CultureInfo.InvariantCulture);

                //if dateformat is invalid it will throw exception invalid date format exception
               Convert.ToDateTime(datetime);
            }
            catch (Exception ex)
            {
                return new ValidationResult(false, $"Invalid DateFormat or {ex.Message}");
            }
            return ValidationResult.ValidResult;
        }
    }
}