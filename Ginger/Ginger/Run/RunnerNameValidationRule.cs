using System.Globalization;
using System.Windows.Controls;

namespace Ginger.Run
{
    public class RunnerNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return IsRunnerNameValid(value) ?
                new ValidationResult(false, "Runner Name cannot be empty")
                : new ValidationResult(true, null);
        }

        private bool IsRunnerNameValid(object value)
        {
            return value == null || string.IsNullOrEmpty(value.ToString()) || string.IsNullOrWhiteSpace(value.ToString());
        }
    }
}