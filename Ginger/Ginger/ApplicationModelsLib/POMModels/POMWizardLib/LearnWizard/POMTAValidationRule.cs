#region License
/*
Copyright © 2014-2026 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;
using GingerCore;
using System.Linq;
using System.Windows.Controls;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    public class POMTAValidationRule : ValidationRule
    {

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (!WorkSpace.Instance.Solution.ApplicationPlatforms.Any(x => ApplicationPOMModel.PomSupportedPlatforms.Contains(x.Platform)))
            {
                return new ValidationResult(false, $"POM supported {GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} platform is required");
            }

            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return new ValidationResult(false, $"{GingerDicser.GetTermResValue(eTermResKey.TargetApplication)} can not be empty");
            }

            return new ValidationResult(true, null);

        }

    }
}
