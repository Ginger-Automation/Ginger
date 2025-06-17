#region License
/*
Copyright © 2014-2025 European Support Limited

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

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    public class AddPOMFromScreenshotWizard : BasePOMWizard
    {

        public AddPOMFromScreenshotWizard(RepositoryFolder<ApplicationPOMModel> pomModelsFolder = null)
        {
            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Page Objects Model Introduction", Page: new WizardIntroPage("/ApplicationModelsLib/POMModels/POMWizardLib/LearnWizard/AddPOMScreenshotIntro.md"));

            AddPage(Name: "Upload MockUp", Title: "Upload MockUp", SubTitle: "Upload MockUp Screen-shot for POM Generation", Page: new UploadMockUpWizardPage());

            AddPage(Name: "AI Generated Preview", Title: "AI Generated Preview", SubTitle: "AI Generated Preview", Page: new AIGeneratedPreviewWizardPage());

            AddPage(Name: "Learning Configurations", Title: "Learning Configurations", SubTitle: "Page Objects Learning Configurations", Page: new POMLearnConfigWizardPage());

            AddPage(Name: "Learned Objects Mapping", Title: "Learned Objects Mapping", SubTitle: "Map Learned Page Objects", Page: new POMObjectsMappingWizardPage());

            AddPage(Name: "General Details", Title: "General Details", SubTitle: "New Page Objects Model General Details", Page: new POMGeneralDetailsWizardPage());
        }

        public override string Title { get { return "Add POM From Screen-shot Wizard"; } }
    }
}
