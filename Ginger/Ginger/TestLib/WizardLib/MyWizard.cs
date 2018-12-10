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

using Ginger.TestLib.WizardLib;
using Ginger.WizardLib;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerTest.WizardLib
{
    public class MyWizard : WizardBase
    {        
        public MyWizardItem myWizardItem = new MyWizardItem() { Active = true };
        string mFolder;

        public override string Title { get { return "Add New MyWizardItem"; } }

        public MyWizard(string folder)
        {
            mFolder = folder;

            AddPage(Name: "Intro", Title: "MyWizardItem Intro", SubTitle: "MyWizardItem Intro", Page: new WizardIntroPage(@"\TestLib\WizardLib\Intro.md"));
            AddPage(Name: "General Details", Title: "MyWizardItem Details", SubTitle: "MyWizardItem General Details", Page: new MyWizardDetailsPage());
            AddPage(Name: "Properties", Title: "MyWizardItem Configurations", SubTitle: "MyWizardItem Configurations", Page: new MyWizardPropertiesPage());
        }
        

        public override void Finish()
        {
            // Save myWizardItem to simple text file
            string fileName = myWizardItem.Name + ".txt";
            string txt = "MyWizardItem" + Environment.NewLine;
            txt += "Name=" + myWizardItem.Name + Environment.NewLine;
            txt += "Description=" + myWizardItem.Description + Environment.NewLine;

            File.WriteAllText(fileName, txt);
        }
    }
}
