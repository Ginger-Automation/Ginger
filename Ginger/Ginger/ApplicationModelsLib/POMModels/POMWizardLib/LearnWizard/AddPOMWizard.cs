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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.WizardLib;
using GingerCore;
using GingerWPF.WizardLib;
using System.Drawing;
using System.IO;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    public class AddPOMWizard : WizardBase
    {
        RepositoryFolder<ApplicationPOMModel> mPomModelsFolder;
        public ApplicationPOMModel POM;

        public string POMFolder;
        public ObservableList<UIElementFilter> AutoMapElementTypesList = new ObservableList<UIElementFilter>();
        public ObservableList<ElementLocator> AutoMapElementLocatorsList = new ObservableList<ElementLocator>();
        public ObservableList<Agent> OptionalAgentsList = null;
        private Agent mAgent = null;
        private bool mManualElementConfiguration;

        public bool ManualElementConfiguration { get { return mManualElementConfiguration; } set { mManualElementConfiguration = value; } }

        public Agent Agent
        {
            get
            {
                return mAgent;
            }
            set
            {
                mAgent = value;
            }
        }

        public IWindowExplorer IWindowExplorerDriver
        {
            get
            {
                if (Agent != null)
                    return ((IWindowExplorer)(Agent.Driver));
                else
                    return null;
            }
        }

        public Bitmap ScreenShot { get; set; }
        public bool IsLearningWasDone { get; set; }

        public AddPOMWizard(RepositoryFolder<ApplicationPOMModel> pomModelsFolder = null)
        {
            mPomModelsFolder = pomModelsFolder;

            POM = new ApplicationPOMModel();

            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Page Objects Model Introduction", Page: new WizardIntroPage("/ApplicationModelsLib/POMModels/POMWizardLib/LearnWizard/AddPOMIntro.md"));

            AddPage(Name: "Learning Configurations", Title: "Learning Configurations", SubTitle: "Page Objects Learning Configurations", Page: new POMLearnConfigWizardPage());

            AddPage(Name: "Learned Objects Mapping", Title: "Learned Objects Mapping", SubTitle: "Map Learned Page Objects", Page: new POMObjectsMappingWizardPage());

            AddPage(Name: "General Details", Title: "General Details", SubTitle: "New Page Objects Model General Details", Page: new POMGeneralDetailsWizardPage());
        }

        public override string Title { get { return "Add POM Wizard"; } }

        public override void Finish()
        {

            if (ScreenShot != null)
            {
                using (var ms = new MemoryStream())
                {
                    POM.ScreenShotImage = Ginger.General.BitmapToBase64(ScreenShot);
                }
            }
            if (mPomModelsFolder != null)
                mPomModelsFolder.AddRepositoryItem(POM);
            else
                WorkSpace.Instance.SolutionRepository.AddRepositoryItem(POM);
            //close all Agents raised in Wizard
            CloseStartedAgents();
        }


        public override void Cancel()
        {
            if (mAgent != null && mAgent.Driver != null && mAgent.Driver.IsDriverBusy)
            {
                mAgent.Driver.mStopProcess = true;
            }

            //close all Agents raised in Wizard
            CloseStartedAgents();
            base.Cancel();
        }

        private void CloseStartedAgents()
        {
            if (OptionalAgentsList != null)
            {
                foreach (Agent agent in OptionalAgentsList)
                    if (agent != null && agent.Status == Agent.eStatus.Running && agent.Tag != null && agent.Tag.ToString() == "Started with Agent Control" && !agent.Driver.IsDriverBusy)
                    {
                        if (Reporter.ToUser(eUserMsgKey.AskIfToCloseAgent, agent.Name) == eUserMsgSelection.Yes)
                        {
                            agent.Close();
                        }
                    }
            }
        }

    }
}
