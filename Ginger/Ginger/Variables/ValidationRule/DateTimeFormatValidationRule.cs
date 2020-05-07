using GingerCore.Variables;
using System;
using System.Globalization;
using System.Windows.Controls;

namespace Ginger.Variables
{
    public class DateTimeFormatValidationRule : ValidationRule
    {
        private VariableDateTime dateTimeVar;
        public DateTimeFormatValidationRule(VariableDateTime value)
        {
            dateTimeVar = value;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            try
            {
                //set date format in datetime field
               var datetime = dateTimeVar.ConvertDateTimeToSpecificFormat(value as string);

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