using amdocs.ginger.GingerCoreNET;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;

namespace Ginger.Run
{
    public class RunSetNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return IsRunSetNameValid(value)
                ? new ValidationResult(false, "Run Set Name cannot be empty")
                : IsRunSetNameExist(value.ToString())
                ? new ValidationResult(false, "Run Set with the same name already exist")
                : new ValidationResult(true, null);
        }
        private bool IsRunSetNameValid(object value)
        {
            return value == null || string.IsNullOrEmpty(value.ToString()) || string.IsNullOrWhiteSpace(value.ToString());
        }
        private bool IsRunSetNameExist(string value)
        {
            return (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<RunSetConfig>() where x.Name == value select x).SingleOrDefault() != null;
        }
    }
}