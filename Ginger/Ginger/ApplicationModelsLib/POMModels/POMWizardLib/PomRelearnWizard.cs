using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib;
using GingerCore;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.ApplicationModelsLib.POMModels.POMWizardLib
{
    public class PomRelearnWizard : WizardBase
    {
        public ApplicationPOMModel mPOM;

        public override string Title { get { return "POM Delta Check Wizard"; } }

        private Agent mAgent = null;

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

        public bool IsLearningWasDone { get; set; }
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

        public PomRelearnWizard(ApplicationPOMModel POM, Agent agent)
        {
            mPOM = POM.CreateCopy(false) as ApplicationPOMModel;
            mAgent = agent;

            AddPage(Name: "Delta Status", Title: "Delta Status", SubTitle: "Get latest changes from page", Page: new POMDeltaWizardPage());

        }

        public override void Finish()
        {
           
        }
    }
}
