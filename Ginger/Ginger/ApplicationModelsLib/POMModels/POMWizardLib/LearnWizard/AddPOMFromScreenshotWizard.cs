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
    public class AddPOMFromScreenshotWizard : BasePOMWizard
    {

        public AddPOMFromScreenshotWizard(RepositoryFolder<ApplicationPOMModel> pomModelsFolder = null)
        {

            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Page Objects Model Introduction", Page: new WizardIntroPage("/ApplicationModelsLib/POMModels/POMWizardLib/LearnWizard/AddPOMIntro.md"));

            AddPage(Name: "Upload MockUp",Title: "Upload MockUp", SubTitle: "Upload MockUp Screen-shot for POM Generation", Page: new UploadMockUpWizardPage());

            AddPage(Name: "AI Generated Preview", Title: "AI Generated Preview", SubTitle: "AI Generated Preview", Page: new AIGeneratedPreviewWizardPage());

            AddPage(Name: "Learning Configurations", Title: "Learning Configurations", SubTitle: "Page Objects Learning Configurations", Page: new POMLearnConfigWizardPage());

            AddPage(Name: "Learned Objects Mapping", Title: "Learned Objects Mapping", SubTitle: "Map Learned Page Objects", Page: new POMObjectsMappingWizardPage());

            AddPage(Name: "General Details", Title: "General Details", SubTitle: "New Page Objects Model General Details", Page: new POMGeneralDetailsWizardPage());
        }

        public override string Title { get { return "Add POM From Screen-shot Wizard"; } }

    }
}
