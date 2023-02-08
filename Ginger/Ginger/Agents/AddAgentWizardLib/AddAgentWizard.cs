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

using Amdocs.Ginger.Repository;
using Ginger.WizardLib;
using GingerCore;
using GingerWPF.WizardLib;

namespace Ginger.Agents.AddAgentWizardLib
{
    public class AddAgentWizard : WizardBase
    {
        public RepositoryFolder<Agent> AgentsFolder;
        public Agent Agent = new Agent() { Active = true};

        public override string Title { get { return "Add New Agent Wizard"; } }
        

        public AddAgentWizard(RepositoryFolder<Agent> AgentsFolder)
        {
            AgentOperations agentOperations = new AgentOperations(Agent);
            Agent.AgentOperations = agentOperations;

            this.AgentsFolder = AgentsFolder;            

            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Agents Introduction", Page: new WizardIntroPage("/Agents/AddAgentWizardLib/AddAgentIntro.md"));

            AddPage(Name: "General Details", Title: "Agent Details", SubTitle: "Set New Agent General Details", Page: new AddAgentDetailsPage());

            AddPage(Name: "Driver Configurations", Title: "Driver Configurations", SubTitle: "Set New Agent Driver Configurations", Page: new AddAgentDriverConfigPage());            
        }

        public override void Finish()
        {
            // TODO: do it in the page where user select the type
            if(((AgentOperations)Agent.AgentOperations).DriverInfo.isDriverPlugin)
            {
                Agent.AgentType = Agent.eAgentType.Service;
            }
            else
            {
                Agent.AgentType = Agent.eAgentType.Driver;
            }

            AgentsFolder.AddRepositoryItem(Agent);
        }


    }
}
