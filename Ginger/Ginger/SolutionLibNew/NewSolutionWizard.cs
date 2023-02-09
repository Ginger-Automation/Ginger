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

using Amdocs.Ginger.Repository;
using Ginger.SolutionGeneral;
using GingerWPF.WizardLib;
using System.Collections.Generic;

namespace GingerWPF.SolutionLib
{
    public class NewSolutionWizard : WizardBase
    {
        Solution mSolution;
        public override string Title { get { return "Create new solution wizard"; } }

        public List<PluginPackage> SelectedPluginPackages = new List<PluginPackage>();

        public NewSolutionWizard()
        {
            mSolution = new Solution();

            AddPage(Name: "Intro", Title: "Intro", SubTitle: "Create new Solution", Page: new NewSolutionWizardPage());

            AddPage(Name: "Name", Title: "Name & Folder", SubTitle: "Select name and folder", Page: new NewSolutionNameFolderWizardPage(mSolution));

            AddPage(Name: "Plugin Packages", Title: "Plugin Packages", SubTitle: "Select Plugin Packages", Page: new NewSolutionPluginsWizardPage(mSolution));

            AddPage(Name: "Target Application", Title: "Target Applications", SubTitle: "Select Target Application and Platform", Page: new NewSolutionTargetApplicationWizardPage(mSolution));            

            AddPage(Name: "Agent", Title: "Create Agent", SubTitle: "Create Agent", Page: new NewSolutionAgentsWizardPage(mSolution));

            AddPage(Name: "Save New Solution", Title: "Save New Solution", SubTitle: "Save New Solution", Page: new SaveNewSolutionWizardPage(mSolution));
        }

        public override void Finish()
        {
            
                //Solution.SaveSolution(mSolution);
                //WorkSpace.Instance.OpenSolution(mSolution.Folder);
                //foreach (PluginPackage p in mWizard.SelectedPluginPackages)
                //{
                //    WorkSpace.Instance.SolutionRepository.AddRepositoryItem(p);
                //}
            
        }
    }
}