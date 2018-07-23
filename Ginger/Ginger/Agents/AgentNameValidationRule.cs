using amdocs.ginger.GingerCoreNET;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ginger.Agents
{
    public class AgentNameValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            //TODO: split to 2 rules name and uniqu
            if (value ==null || string.IsNullOrEmpty(value.ToString()))
            {
                return new ValidationResult(false, "Agent Name cannot be empty");
            }
            else if (IsAgentNameExist(value.ToString()))
            {
                return new ValidationResult(false, "Agent with the same name already exist");
            }
            else if (value.ToString().Trim().IndexOfAny(new char[] { '/', '\\', '*', ':', '?', '"', '<', '>', '|' }) != -1)
            {                
                return new ValidationResult(false, "Invalid chars in Agent name");
            }
            else
            {
                return new ValidationResult(true, null);
            }        
        }

        private bool IsAgentNameExist(string value)
        {
            if ((from x in WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<Agent>() where x.Name == value select x).SingleOrDefault() != null)
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
