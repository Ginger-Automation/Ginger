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
