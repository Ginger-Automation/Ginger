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
using GingerCore;
using GingerWPF.WizardLib;
using System;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    public class AddPOMWizard : WizardBase
    {

        public IWindowExplorer WinExplorer;

        public ApplicationPOMModel POM;
        public string POMFolder;
        public ObservableList<UIElementFilter> CheckedFilteringCreteriaList = new ObservableList<UIElementFilter>();
        internal Agent mAgent;

        public bool IsLearningWasDone { get; set; }

        public AddPOMWizard()
        {

            POM = new ApplicationPOMModel();

            AddPage(Name: "Intro", Title: "Intro", SubTitle: "Add new POM page for application", Page: new AddPOMIntroWizardPage());

            AddPage(Name: "Select App and folder", Title: "Select App and Folder", SubTitle: "Choose Target Application and Agent", Page: new SelectAppFolderWizardPage());

            AddPage(Name: "Scan Config", Title: "Scan Config", SubTitle: "Scan Config", Page: new LearnConfigWizardPage());

            AddPage(Name: "Learn", Title: "Learn", SubTitle: "Learn Page Object Model", Page: new LearnWizardPage(this.POM));

            AddPage(Name: "Map", Title: "Map", SubTitle: "Map each UI element", Page: new MapUIElementsWizardPage(this.POM));

            AddPage(Name: "Save", Title: "Save", SubTitle: "Save POM to file", Page: new SavePOMWizardPage());
            

        }

        public override string Title { get { return "Add POM Wizard"; } }

        public override void Finish()
        {
            WorkSpace.Instance.SolutionRepository.AddRepositoryItem(POM);
        }
    }
}
