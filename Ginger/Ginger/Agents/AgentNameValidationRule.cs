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
using GingerCore;
using System.Linq;
using System.Windows.Controls;

namespace Ginger.Agents
{
    public class AgentNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            return IsAgentNameValid(value)
                ? new ValidationResult(false, "Agent Name cannot be empty")
                : IsAgentNameExist(value.ToString())
                ? new ValidationResult(false, "Agent with the same name already exist")
                : new ValidationResult(true, null);
        }
        private bool IsAgentNameValid(object value)
        {
            return value == null || string.IsNullOrEmpty(value.ToString()) || string.IsNullOrWhiteSpace(value.ToString());
        }
        private bool IsAgentNameExist(string value)
        {
            return (from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where x.Name == value select x).SingleOrDefault() != null;
        }
    }

}
