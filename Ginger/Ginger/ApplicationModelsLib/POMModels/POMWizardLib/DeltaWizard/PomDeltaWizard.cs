using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.UIElement;
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
        public Agent mAgent = null;
        public override string Title { get { return "POM Elements Update Wizard"; } }

        public PomDeltaWizard(ApplicationPOMModel pom, Agent agent)
        {
            mAgent = agent;
            mPomDeltaUtils = new PomDeltaUtils(pom, agent);           
            AddPage(Name: "Elements Compare", Title: "Elements Compare", SubTitle: "Comparison Status of Elements with Latest", Page: new PomDeltaElementComparePage());
        }

        public override void Finish()
        {
            mPomDeltaUtils.UpdateOriginalPom();
        }

    }
}
