using System.Linq;
using GingerCore.DataSource;
using amdocs.ginger.GingerCoreNET;
using System.Windows.Controls;

namespace Ginger.DataSource
{
    class DataSourceNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            return IsDataSourceNameValid(value)
                ? new ValidationResult(false, "DataSource Name cannot be empty")
                : IsDataSourceNameExist(value.ToString())
                ? new ValidationResult(false, "DataSource with the same name already exist")
                : new ValidationResult(true, null);
        }
        private bool IsDataSourceNameValid(object value)
        {
            return value == null || string.IsNullOrEmpty(value.ToString()) || string.IsNullOrWhiteSpace(value.ToString());
        }
        private bool IsDataSourceNameExist(string value)
        {
            return WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>().Any(x => x.Name == value);
        }
    }
}
