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
using Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib;
using GingerWPF.WizardLib;
using System.Windows;
using System.Windows.Controls;


namespace Ginger.ApplicationModelsLib.POMModels.AddEditPOMWizardLib
{
    /// <summary>
    /// Interaction logic for SavePOMWizardPage.xaml
    /// </summary>
    public partial class SavePOMWizardPage : Page, IWizardPage
    {
        AddPOMWizard mWizard;

        public SavePOMWizardPage()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: get name from POM
            if (string.IsNullOrEmpty(mWizard.POM.Name))
            {
                MessageBox.Show("Please select a name for this POM");
            }
            else
            {                
                //if (!Directory.Exists(mWizard.POMFolder))
                //{
                //    Directory.CreateDirectory(mWizard.POMFolder);
                //}

                //string POMFolder = Path.Combine(mWizard.POMFolder, mWizard.POM.Name);
                //if (!Directory.Exists (POMFolder))
                //{
                //    Directory.CreateDirectory(POMFolder);
                //}
                
                //string ext = RepositoryItem.FileExt(typeof(ApplicationPOM));
                //string filename = Path.Combine(POMFolder, mWizard.POM.Name + "." + ext);
                //mWizard.POM.FilePath = filename + ".XML";
                //mWizard.POM.Save();

                // mWizard.POM.ScreenShot.Save(filename + ".Screenshot.bmp");  // save the screen shot
                
            }
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            if (WizardEventArgs.EventType == EventType.Init)
            {
                mWizard = (AddPOMWizard)WizardEventArgs.Wizard;
            }

            // TODO: if Finish and not save then do save
            
        }
    }
}
