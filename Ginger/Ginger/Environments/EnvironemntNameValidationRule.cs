using amdocs.ginger.GingerCoreNET;
using GingerCore;
using GingerCore.Environments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ginger.Environments
{
    public class EnvironemntNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            //TODO: split to 2 rules name and unique
            if (value ==null || string.IsNullOrEmpty(value.ToString()))
            {
                return new ValidationResult(false, "Environment name cannot be empty");
            }
            else if (IsEnvNameExist((string)value))
            {
                return new ValidationResult(false, "Environment with the same name already exist");
            }
            else if (value.ToString().Trim().IndexOfAny(new char[] { '/', '\\', '*', ':', '?', '"', '<', '>', '|' }) != -1)
            {                
                return new ValidationResult(false, "Invalid chars in Environment name");
            }
            else
            {
                return new ValidationResult(true, null);
            }        
        }

        private bool IsEnvNameExist(string value)
        {
            if ((from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>() where x.Name == value select x).SingleOrDefault() != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
    
}
