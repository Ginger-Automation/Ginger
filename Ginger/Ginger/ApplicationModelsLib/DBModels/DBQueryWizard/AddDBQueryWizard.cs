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
using GingerWPF.WizardLib;

namespace GingerWPF.ApplicationModels.DBModels.DBQueryWizard
{
    public class AddDBQueryWizard : WizardBase
    {
        public string DBMFolder;

        ApplicationDBQueryModel mApplicationDBQueryModel;

        public AddDBQueryWizard(string Folder)
        {
            this.DBMFolder = Folder;
            mApplicationDBQueryModel = new ApplicationDBQueryModel();
            mApplicationDBQueryModel.ContainingFolder = Folder;
            AddPage(Name: "Add Query Intro", Title: "Add query Intro", SubTitle: "Choose ...", Page: new AddDBQueryIntroWizardPage());
        }

        public override string Title { get { return "Add DB Query Wizard"; } }

        public override void Finish()
        {
            throw new System.NotImplementedException();
        }
    }
}
