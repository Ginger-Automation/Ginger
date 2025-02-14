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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore.Environments;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using GingerWPF.WizardLib;
using System.Linq;

namespace Ginger.Environments.GingerOpsEnvWizardLib
{
    public class AddGingerOpsEnvWizard : WizardBase
    {
        public RepositoryFolder<ProjEnvironment> EnvsFolder;
        public ProjEnvironment NewEnvironment = new ProjEnvironment();
        public ObservableList<EnvApplication> apps = [];
        public ObservableList<ProjEnvironment> ImportedEnvs = [];
        public ObservableList<ApplicationPlatform> tempAppPlat = [];
        public override string Title { get { return "Import GingerOps Environment Wizard"; } }

        public AddGingerOpsEnvWizard(RepositoryFolder<ProjEnvironment> EnvsFolder)
        {
            this.EnvsFolder = EnvsFolder;

            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "GingerOps Introduction", Page: new GingerOpsIntroPage());

            AddPage(Name: "Environment to Import", Title: "Environment to Import", SubTitle: "Environment to Import", Page: new AddGingerOpsEnvPage());

            AddPage(Name: "Ginger Analytics Environments", Title: "Imported Environments", SubTitle: "Imported Environment Details", Page: new GingerOpsApplicationPage());
            DisableNavigationList();

        }


        public override void Finish()
        {
            bool isSolutionupdated = false;
            foreach (var item in tempAppPlat)
            {
                bool isExist = WorkSpace.Instance.Solution.ApplicationPlatforms.Any(app => app.AppName == item.AppName && app.GingerOpsAppId == item.GingerOpsAppId);
                if (!isExist)
                {
                    WorkSpace.Instance.Solution.ApplicationPlatforms.Add(item);
                    isSolutionupdated = true;
                }

            }

            if (isSolutionupdated)
            {
                WorkSpace.Instance.Solution.SolutionOperations.SaveSolution();
            }

            foreach (ProjEnvironment item in ImportedEnvs)
            {
                if (item.GOpsFlag)
                {
                    //Save the Environment 
                    EnvsFolder.AddRepositoryItem(item);
                }

            }

        }
    }
}
