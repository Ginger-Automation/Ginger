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

using GingerWPF.WizardLib;

namespace Ginger.Imports.CDL
{
    public class ImportCDLWizard : WizardBase
    {
        public ImportCDL ImportCDL { get; set;}

        public override string Title { get { return "Import CDL Wizard"; } }

        public ImportCDLWizard()
        {
            ImportCDL = new ImportCDL();

            AddPage(Name: "Intro", Title: "Intro", SubTitle: "Import CDL", Page: new ImportCDLPage());

            AddPage(Name: "Process", Title: "Process", SubTitle: "Process CDL", Page: new ImportCDLWizardProcessPage());
        }

        public override void Finish()
        {
            // throw new System.NotImplementedException();
        }
    }
}