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


using Ginger;
using GingerWPF.WizardLib;
using System.Windows.Controls;


namespace GingerTest.WizardLib
{
    /// <summary>
    /// Interaction logic for AddAgentDetailsPage.xaml
    /// </summary>
    public partial class MyWizardDetailsPage : Page, IWizardPage
    {
        MyWizard mWizard;

        public MyWizardDetailsPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {

            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    // keep ref to wizard
                    mWizard = (MyWizard)WizardEventArgs.Wizard;
                    // Bind Name
                    xNameTextBox.BindControl(mWizard.myWizardItem, nameof(MyWizardItem.Name));
                    // Add validation rules
                    // xNameTextBox.AddValidationRule(ValidationRule ());
                    // focus to Name
                    xNameTextBox.Focus();

                    //xAgentDescriptionTextBox.BindControl(mWizard.Agent, nameof(Agent.Notes));
                    //xAgentTagsViewer.Init(mWizard.Agent.Tags);

                    //xPlatformTypeComboBox.SelectionChanged += xPlatformTypeComboBox_SelectionChanged;
                    //GingerCore.General.FillComboFromEnumObj(xPlatformTypeComboBox, mWizard.Agent.Platform);
                    //xPlatformTypeComboBox.SelectedIndex = 0;

                    //xDriverTypeComboBox.BindControl(mWizard.Agent, nameof(Agent.DriverType));
                    //xDriverTypeComboBox.SelectionChanged += xDriverTypeComboBox_SelectionChanged;
                    //xDriverTypeComboBox.AddValidationRule(eValidationRule.CannotBeEmpty);
                    //xDriverTypeStackPanel.Visibility = Visibility.Collapsed;
                    break;
            }

        }


        private void xPlatformTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //xDriverTypeComboBox.SelectedItem = null;
            //xDriverTypeComboBox.Items.Clear();

            //List<object> driverTypeValues = mWizard.Agent.GetDriverTypesByPlatfrom(xPlatformTypeComboBox.SelectedItem.ToString());
            //GingerCore.General.FillComboFromEnumObj(xDriverTypeComboBox, mWizard.Agent.DriverType, driverTypeValues, false);
            //if (xDriverTypeComboBox.Items.Count > 0)
            //    xDriverTypeComboBox.SelectedItem = xDriverTypeComboBox.Items[0];

            //if (xDriverTypeComboBox.Items.Count > 1)
            //{
            //    xDriverTypeStackPanel.Visibility = Visibility.Visible;                
            //}
            //else
            //{
            //    xDriverTypeStackPanel.Visibility = Visibility.Collapsed;
            //}            
        }

        private void xDriverTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // mWizard.Agent.InitDriverConfigs();
        }
    }
}
