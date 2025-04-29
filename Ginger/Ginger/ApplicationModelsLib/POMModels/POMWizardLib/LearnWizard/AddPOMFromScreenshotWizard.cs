using Amdocs.Ginger.Common;
using Amdocs.Ginger.CoreNET.Application_Models;
using Amdocs.Ginger.Repository;
using Ginger.WizardLib;
using GingerCore;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    public class AddPOMFromScreenshotWizard : WizardBase
    {
        public PomLearnUtils mPomLearnUtils;
        public ObservableList<Agent> OptionalAgentsList = null;
        public bool IsLearningWasDone { get; set; }

        private bool mManualElementConfiguration;
        public bool ManualElementConfiguration { get { return mManualElementConfiguration; } set { mManualElementConfiguration = value; } }

        public string ScreenShotImage { get; set; }


        public string ScreenShotImagePath { get; set; }
        public string HtmlFilePath { get; set; }

        public AddPOMFromScreenshotWizard(RepositoryFolder<ApplicationPOMModel> pomModelsFolder = null)
        {
            mPomLearnUtils = new PomLearnUtils(new ApplicationPOMModel(), pomModelsFolder: pomModelsFolder);

            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Page Objects Model Introduction", Page: new WizardIntroPage("/ApplicationModelsLib/POMModels/POMWizardLib/LearnWizard/AddPOMIntro.md"));

            AddPage(Name: "Upload MockUp",Title: "Upload MockUp", SubTitle: "Upload MockUp", Page: new UploadMockUpWizardPage());

            AddPage(Name: "AI Generated Preview", Title: "AI Generated Preview", SubTitle: "AI Generated Preview", Page: new AIGeneratedPreviewWizardPage());

            AddPage(Name: "Learning Configurations", Title: "Learning Configurations", SubTitle: "Page Objects Learning Configurations", Page: new POMLearnConfigWizardPage(true));

            AddPage(Name: "Learned Objects Mapping", Title: "Learned Objects Mapping", SubTitle: "Map Learned Page Objects", Page: new POMObjectsMappingWizardPage(true));

            AddPage(Name: "General Details", Title: "General Details", SubTitle: "New Page Objects Model General Details", Page: new POMGeneralDetailsWizardPage(true));
        }

        public override string Title { get { return "Add POM From Screen-shot Wizard"; } }

        public PomLearnUtils PomLearnUtils {get;}

        public override void Finish()
        {
            mPomLearnUtils.SaveLearnedPOM(true);

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
