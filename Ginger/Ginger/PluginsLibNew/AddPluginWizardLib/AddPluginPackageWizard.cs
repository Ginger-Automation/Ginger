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
using Amdocs.Ginger.Repository;
using Ginger.WizardLib;
using GingerWPF.WizardLib;

namespace GingerWPF.PluginsLib.AddPluginWizardLib
{
    public class AddPluginPackageWizard : WizardBase
    {
        public PluginPackage PluginPackage;

        public string Folder { get; set; }
        public AddPluginPackageWizard()
        {
            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Plugin Introduction", Page: new WizardIntroPage("/PluginsLibNew/AddPluginWizardLib/AddPluginIntro.md"));

            AddPage(Name: "Select Plugin Type", Title: "Select Plugin Type", SubTitle: "Choose ...", Page: new SelectPluginPackageTypePage());

            AddPage(Name: "Select Plugin Folder Page", Title: "Select Plugin Folder Page", SubTitle: "Select Plugin Folder Page ...", Page: new SelectPlugPackageinFolderPage());

            AddPage(Name: "Plugin Info Page", Title: "Plugin Info Page", SubTitle: "Plugin Info Page ...", Page: new PlugPackageinInfoPage());            

            AddPage(Name: "Add Plugin to Solution Page", Title: "Add Plugin to Solution Page", SubTitle: "Add Plugin to Solution Page ...", Page: new AddPluginPackageToSolutionPage());
        }

        public override string Title { get { return "Add Plugin Package Wizard"; } }

        public override void Finish()
        {
            WorkSpace.Instance.SolutionRepository.AddRepositoryItem(PluginPackage);
        }
    }
}
