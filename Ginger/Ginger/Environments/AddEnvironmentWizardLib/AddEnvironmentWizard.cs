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
using Amdocs.Ginger.Repository;
using GingerCore.Environments;
using GingerWPF.WizardLib;

namespace Ginger.Environments.AddEnvironmentWizardLib
{
    public class AddEnvironmentWizard : WizardBase
    {
        public RepositoryFolder<ProjEnvironment> EnvsFolder;
        public ProjEnvironment NewEnvironment = new ProjEnvironment();
        public ObservableList<EnvApplication> apps = new ObservableList<EnvApplication>();

        public override string Title { get { return "Add New Environment Wizard"; } }

        public AddEnvironmentWizard(RepositoryFolder<ProjEnvironment> EnvsFolder)
        {
            this.EnvsFolder = EnvsFolder;            
            
            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Environments Introduction", Page: new AddNewEnvIntroPage());

            AddPage(Name: "Environment Details", Title: "Environment Details", SubTitle: "Set New Environment Details", Page: new AddNewEnvDetailsWizardPage());

            AddPage(Name: "Environment Applications", Title: "Environment Applications", SubTitle: "Set New Environment Applications", Page: new AddNewEnvAppsPage());
            
            //AddPage(Name: "Save", Title: "Save", SubTitle: "Choose ...", Page: new AddNewEnvironmentSavePage());
        }

        public override void Finish()
        {            
            //add selected apps
            foreach (EnvApplication app in apps)
            {
                if (app.Active)
                {
                    NewEnvironment.Applications.Add(app);
                }
            }

            //Save the Environment 
            EnvsFolder.AddRepositoryItem(NewEnvironment);
            
        }
    }
}
