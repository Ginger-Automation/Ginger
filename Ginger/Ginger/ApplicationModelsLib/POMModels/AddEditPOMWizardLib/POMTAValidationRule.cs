using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    public class POMTAValidationRule : ValidationRule
    {

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            //TODO: split to 2 rules name and uniqu
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return new ValidationResult(false, "POM Target Application cannot be empty");
            }
            else if (App.UserProfile.Solution.ApplicationPlatforms.Where(x => x.Platform == ePlatformType.Web).ToList().Count() == 0)
            {
                return new ValidationResult(false, "Web Platform Target Application is mandatory to create POM");
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }

    }
}
