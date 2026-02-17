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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Application_Models;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerWPF.WizardLib;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    /// <summary>
    /// Base class for POM Wizard implementations that provides common functionality for
    /// configuring and managing POM learning.
    /// </summary>
    public abstract class BasePOMWizard : WizardBase
    {
        /// <summary>
        /// Utility class that handles the POM learning operations.
        /// </summary>
        public PomLearnUtils mPomLearnUtils;
        /// <summary>
        /// List of optional agents that can be used for POM learning.
        /// </summary>
        public ObservableList<Agent> OptionalAgentsList = null;
        /// <summary>
        /// Indicates whether the learning process has been completed.
        /// </summary>
        public bool IsLearningWasDone { get; set; }
        /// <summary>
        /// Indicates whether elements are configured manually.
        /// </summary>
        private bool mManualElementConfiguration;
        public bool ManualElementConfiguration { get { return mManualElementConfiguration; } set { mManualElementConfiguration = value; } }
        /// <summary>
        /// Base64 encoded image of the screen-shot
        /// </summary>
        public string ScreenShotImage { get; set; }
        /// <summary>
        /// Path to the screen-shot image file
        /// </summary>
        public string ScreenShotImagePath { get; set; }
        /// <summary>
        /// Path to the generated HTML file
        /// </summary>
        public string HtmlFilePath { get; set; }

        public string userTempDataFolderPath { get; set; }

        protected BasePOMWizard(RepositoryFolder<ApplicationPOMModel> pomModelsFolder = null)
        {
            mPomLearnUtils = new PomLearnUtils(new ApplicationPOMModel(), pomModelsFolder: pomModelsFolder);
        }

        public override void Finish()
        {
            // Ensure any pending operations are complete
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
                    if (ShouldCloseAgent(agent))
                    {
                        if (Reporter.ToUser(eUserMsgKey.AskIfToCloseAgent, agent.Name) == eUserMsgSelection.Yes)
                        {
                            agent.AgentOperations.Close();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determines if an agent should be closed based on its status and properties
        /// </summary>
        private bool ShouldCloseAgent(Agent agent)
        {
            return agent != null &&
                   ((AgentOperations)agent.AgentOperations).Status == Agent.eStatus.Running &&
                   agent.Tag != null &&
                   agent.Tag.ToString() == "Started with Agent Control" &&
                   !((AgentOperations)agent.AgentOperations).Driver.IsDriverBusy;
        }

    }
}
