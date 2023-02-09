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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Repository;
using GingerWPF.ApplicationModels.DBModels.DBModelWizard;
using GingerWPF.WizardLib;
using System.Linq;

namespace Ginger.ApplicationModels.DBModels.DBModelWizard
{
    public class AddDBModelWizard : WizardBase
    {
        public string DBMFolder;
        ApplicationDBModel mApplicationDBModel;

        public AddDBModelWizard(string DBMFolder)
        {            
            this.DBMFolder = DBMFolder;
            mApplicationDBModel = new ApplicationDBModel();
            mApplicationDBModel.ContainingFolder = DBMFolder;
            AddPage(Name: "Add DB Model", Title: "Select DB", SubTitle: "Choose ...", Page: new AddDBModelIntroWizardPage());
            AddPage(Name: "Learn DB", Title: "Learn DB", SubTitle: "Choose ...", Page: new ScanDBPage(mApplicationDBModel));
            AddPage(Name: "Save", Title: "Save", SubTitle: "Choose ...", Page: new SaveDBModelPageWizardPage(mApplicationDBModel));
        }

        public override string Title { get { return "ADB Model Wizard"; } }

        public override void Finish()
        {
            WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(mApplicationDBModel);

            // Save tables each one in separate file - better handling, easy compare and more, less merges
            foreach (ApplicationDBTableModel table in mApplicationDBModel.Tables.Where(x => x.Selected))
            {
                WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(table);
            }
        }
    }
}
