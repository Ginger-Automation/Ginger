using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    class POMNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            //TODO: split to 2 rules name and uniqu
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return new ValidationResult(false, "POM Name cannot be empty");
            }
            else if (IsPOMNameExist(value.ToString()))
            {
                return new ValidationResult(false, "POM with the same name already exist");
            }
            else if (value.ToString().Trim().IndexOfAny(new char[] { '/', '\\', '*', ':', '?', '"', '<', '>', '|' }) != -1)
            {
                return new ValidationResult(false, "Invalid chars in POM name");
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }

        private bool IsPOMNameExist(string value)
        {
            if ((from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ApplicationPOMModel>() where x.Name == value select x).SingleOrDefault() != null)
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
