#region License
/*
Copyright © 2014-2023 European Support Limited

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

using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    public class POMTAValidationRule : ValidationRule
    {

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if(WorkSpace.Instance.Solution.ApplicationPlatforms.Where(x => ApplicationPOMModel.PomSupportedPlatforms.Contains(x.Platform)).ToList().Count() == 0)
            {
                return new ValidationResult(false, "POM supported Target Application platform is required");
            }

            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return new ValidationResult(false, "Target Application can not be empty");
            }

            return new ValidationResult(true, null);

        }

    }
}
