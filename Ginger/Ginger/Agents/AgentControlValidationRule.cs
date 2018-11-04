#region License
/*
Copyright © 2014-2018 European Support Limited

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

using GingerCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ginger.Agents
{
    class AgentControlValidationRule: ValidationRule
    {
        public enum eAgentControlValidationRuleType
        {
            AgentIsMapped,
            AgentIsMappedAndRunning,
        }

        eAgentControlValidationRuleType mAgentControlValidationRuleType;

        public AgentControlValidationRule(eAgentControlValidationRuleType agentControlValidationRuleType)
        {
            mAgentControlValidationRuleType = agentControlValidationRuleType;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            switch (mAgentControlValidationRuleType)
            {
                case eAgentControlValidationRuleType.AgentIsMapped:
                    if (value == null)
                        return new ValidationResult(false, "Agent must be mapped");
                    break;

                case eAgentControlValidationRuleType.AgentIsMappedAndRunning:
                    if (value == null || ((Agent)value).Status != Agent.eStatus.Running)
                        return new ValidationResult(false, "Agent must be mapped and running");
                    break;
            }


            return new ValidationResult(true, null);
        }
    }
}
