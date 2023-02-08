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
            AddPage(Name: "Elements Mapping", Title: "Elements Mapping", SubTitle: "Map deleted element with new element", Page: new PomDeltaDeletedElementMappingWizardPage());

        }

        public override void Finish()
        {
            mPomDeltaUtils.UpdateOriginalPom();
        }

    }
}
