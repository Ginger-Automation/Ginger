using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ginger.ApplicationModelsLib.POMModels.POMWizardLib
{
    class POMScreenShotValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            //if (App.UserProfile.Solution.ApplicationPlatforms.Where(x => x.Platform == ePlatformType.Web).ToList().Count() == 0)
            //{
            //    return new ValidationResult(false, "Web Platform Target Application is required");
            //}

            //if (value == null || string.IsNullOrEmpty(value.ToString()))
            //{
            //    return new ValidationResult(false, "Target Application can not be empty");
            //}

            return new ValidationResult(true, null);

        }
    }
}
