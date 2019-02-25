using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.CoreNET.Application_Models;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerCoreNET.Application_Models;
using GingerWPF.WizardLib;
using System.Collections.Generic;
using System.Linq;

namespace Ginger.ApplicationModelsLib.POMModels.POMWizardLib
{
    public class PomDeltaWizard : WizardBase
    {
        public PomDeltaUtils mPomDeltaUtils = null;
               
        public override string Title { get { return "POM Elements Update Wizard"; } }

        public PomDeltaWizard(ApplicationPOMModel pom, Agent agent)
        {            
            mPomDeltaUtils = new PomDeltaUtils(pom, agent);            

            AddPage(Name: "Elements Update Settings", Title: "Elements Update Settings", SubTitle: "Elements Update Settings", Page: new PomDeltaSettingsWizardPage());
            AddPage(Name: "Elements Compare", Title: "Elements Compare", SubTitle: "Elements Comparison with Latest Status", Page: new PomDeltaElementCompareWizardPage());
        }

        public override void Finish()
        {
            mPomDeltaUtils.UpdateOriginalPom();
        }

    }
}
