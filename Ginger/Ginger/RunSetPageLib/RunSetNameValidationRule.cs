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
