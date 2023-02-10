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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Application_Models;
using Amdocs.Ginger.Repository;
using Ginger.WizardLib;
using GingerCore;
using GingerWPF.WizardLib;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    public class AddPOMWizard : WizardBase
    {
        public PomLearnUtils mPomLearnUtils;
        public ObservableList<Agent> OptionalAgentsList = null;
        public bool IsLearningWasDone { get; set; }

        private bool mManualElementConfiguration;
        public bool ManualElementConfiguration { get { return mManualElementConfiguration; } set { mManualElementConfiguration = value; } }
        

        public AddPOMWizard(RepositoryFolder<ApplicationPOMModel> pomModelsFolder = null)
        {
            mPomLearnUtils = new PomLearnUtils(new ApplicationPOMModel(), pomModelsFolder: pomModelsFolder);

            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Page Objects Model Introduction", Page: new WizardIntroPage("/ApplicationModelsLib/POMModels/POMWizardLib/LearnWizard/AddPOMIntro.md"));

            AddPage(Name: "Learning Configurations", Title: "Learning Configurations", SubTitle: "Page Objects Learning Configurations", Page: new POMLearnConfigWizardPage());

            AddPage(Name: "Learned Objects Mapping", Title: "Learned Objects Mapping", SubTitle: "Map Learned Page Objects", Page: new POMObjectsMappingWizardPage());

            AddPage(Name: "General Details", Title: "General Details", SubTitle: "New Page Objects Model General Details", Page: new POMGeneralDetailsWizardPage());
        }

        public override string Title { get { return "Add POM Wizard"; } }

        public override void Finish()
        {
            mPomLearnUtils.SaveLearnedPOM();

            //close all Agents raised in Wizard
            CloseStartedAgents();
        }

        public override void Cancel()
        {
            mPomLearnUtils.StopLearning();

            //close all Agents raised in Wizard
            CloseStartedAgents();
            base.Cancel();
        }

        private void CloseStartedAgents()
        {
            if (OptionalAgentsList != null)
            {
                foreach (Agent agent in OptionalAgentsList)
                {
                    if (agent.AgentOperations == null)
                    {
                        AgentOperations agentOperations = new AgentOperations(agent);
                        agent.AgentOperations = agentOperations;
                    }
                    if (agent != null && ((AgentOperations)agent.AgentOperations).Status == Agent.eStatus.Running && agent.Tag != null && agent.Tag.ToString() == "Started with Agent Control" && !((AgentOperations)agent.AgentOperations).Driver.IsDriverBusy)
                    {
                        if (Reporter.ToUser(eUserMsgKey.AskIfToCloseAgent, agent.Name) == eUserMsgSelection.Yes)
                        {
                            agent.AgentOperations.Close();
                        }
                    }
                }
            }
        }

        
    }
}
