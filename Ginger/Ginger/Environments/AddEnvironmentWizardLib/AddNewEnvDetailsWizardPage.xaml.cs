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
using GingerCore.Environments;
using GingerWPF.WizardLib;
using System;
using System.Linq;
using System.Windows.Controls;
using static Ginger.ExtensionMethods;

namespace Ginger.Environments.AddEnvironmentWizardLib
{
    /// <summary>
    /// Interaction logic for AddNewEnvDetailsWizardPage.xaml
    /// </summary>
    public partial class AddNewEnvDetailsWizardPage : Page, IWizardPage
    {
        AddEnvironmentWizard mWizard;
        public AddNewEnvDetailsWizardPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (AddEnvironmentWizard)WizardEventArgs.Wizard;
                    xNameTextBox.BindControl(mWizard.NewEnvironment, nameof(ProjEnvironment.Name));                    
                    xNameTextBox.AddValidationRule(new EnvironemntNameValidationRule());
                    xEnvTagsViewer.Init(mWizard.NewEnvironment.Tags);                    
                    xNameTextBox.Focus();
                    break;
                //case EventType.Active:
                //    // SetNextBtn();
                //    // WizardEventArgs.IgnoreDefaultNextButtonSettings = true;
                //    //mWizard.FinishEnabled = false;
                //    break;
                //case EventType.Finish:
                //    //avoid keeping the form in memory ?? FIXME
                //    // mWizard.NewEnvironment.PropertyChanged -= NewEnvironment_PropertyChanged;
                //    break;
            }
        }

        //private void NewEnvironment_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == nameof(ProjEnvironment.Name))
        //    {
        //        SetNextBtn();
        //    }
        //}

        //private void SetNextBtn()
        //{
        //    //if (string.IsNullOrEmpty(mWizard.NewEnvironment.Name))
        //    //{
        //    //    mWizard.NextEnabled = false;
        //    //}
        //    //else
        //    //{
        //    //    mWizard.NextEnabled = true;
        //    //}
        //}


        //private void XNameTextBox_LostFocus(object sender, System.Windows.RoutedEventArgs e)
        //{            
        //    //check if name is unique or empty
        //    ProjEnvironment env = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<ProjEnvironment>().Where(x => x.Name == mWizard.NewEnvironment.Name).FirstOrDefault();
        //    if (env != null || string.IsNullOrEmpty(mWizard.NewEnvironment.Name))
        //    {
        //        xNameNotUniqueLbl.Visibility = System.Windows.Visibility.Visible;
        //        mWizard.NextEnabled = false;
        //    }
        //    else
        //    {
        //        xNameNotUniqueLbl.Visibility = System.Windows.Visibility.Collapsed;
        //    }
        //}
        
    }
}
