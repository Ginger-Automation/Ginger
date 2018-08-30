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
