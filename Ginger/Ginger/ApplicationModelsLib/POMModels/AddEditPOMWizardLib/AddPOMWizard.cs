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
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    public class AddPOMWizard : WizardBase
    {

        public IWindowExplorer WinExplorer;

        public ApplicationPOMModel POM;
        public string POMFolder;
        public ObservableList<UIElementFilter> CheckedFilteringCreteriaList = new ObservableList<UIElementFilter>();

        private Agent mAgent = null;
        public Agent Agent
        {
            get
            {
                return mAgent;
            }
            set
            {
                mAgent = value;
            }
        }


        public Bitmap ScreenShot { get; set; }

        public bool IsLearningWasDone { get; set; }

        public AddPOMWizard()
        {

            POM = new ApplicationPOMModel();

            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Add new POM page for application", Page: new AddPOMIntroWizardPage());

            AddPage(Name: "General Details", Title: "General Details", SubTitle: "Choose Target Application and Agent", Page: new SelectAppFolderWizardPage());

            AddPage(Name: "Learning Configurations", Title: "Learning Configurations", SubTitle: "Scan Config", Page: new LearnConfigWizardPage());

            AddPage(Name: "Learn", Title: "Learn", SubTitle: "Learn Page Object Model", Page: new LearnWizardPage(this.POM));

            AddPage(Name: "Screen Shot", Title: "Screen Shot", SubTitle: "Map each UI element", Page: new MapUIElementsWizardPage(this.POM));

            AddPage(Name: "Save", Title: "Save", SubTitle: "Save POM to file", Page: new SavePOMWizardPage());
            

        }

        public override string Title { get { return "Add POM Wizard"; } }

        public bool UnMapNonfunctional { get; set; }

        public override void Finish()
        {

            using (var ms = new MemoryStream())
            {
                POM.LogoBase64Image = Ginger.Reports.GingerExecutionReport.ExtensionMethods.BitmapToBase64(ScreenShot);
            }


            WorkSpace.Instance.SolutionRepository.AddRepositoryItem(POM);



            //MemoryStream ms = new MemoryStream();
            //XmlTextWriter xw = new XmlTextWriter(ms, Encoding.UTF8);
            //xw.Formatting = Formatting.Indented;
            //XmlSerializer ser = new XmlSerializer(typeof(Bitmap));
            //ser.Serialize(xw, POM.ScreenShot);
            //string s = Encoding.UTF8.GetString(ms.ToArray());

            //DataContractSerializer dcs = new DataContractSerializer(typeof(Bitmap));

            //dcs.WriteObject(File.Create("c:\\A\\test.xml"), POM.ScreenShot);
            //dcs.WriteObject(File.Create("c:\\D\\test.xml"), POM.ScreenShot);
            //object o = dcs.ReadObject(new FileStream("c:\\D\\test.xml", FileMode.Open));

            //POM.ScreenShot = (Bitmap)o;
        }


    }
}
