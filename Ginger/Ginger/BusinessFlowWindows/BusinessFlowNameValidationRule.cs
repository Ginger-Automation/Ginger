using amdocs.ginger.GingerCoreNET;
using GingerCore;
using System.Linq;
using System.Windows.Controls;

namespace Ginger.BusinessFlowWindows
{
    public class BusinessFlowNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            return IsBusinessFlowNameValid(value) ?
                new ValidationResult(false, "Business Flow Name cannot be empty")
                : new ValidationResult(true, null);
        }

        private bool IsBusinessFlowNameValid(object value)
        {
            return value == null || string.IsNullOrEmpty(value.ToString()) || string.IsNullOrWhiteSpace(value.ToString());
        }
    }
}
