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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.ValidationRules;
using GingerCore.Actions.ActionConversion;
using GingerWPF.WizardLib;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Actions.ActionConversion
{
    /// <summary>
    /// Interaction logic for ConversionConfigurationWzardPage.xaml
    /// </summary>
    public partial class ConversionConfigurationWzardPage : Page, IWizardPage
    {
        ActionsConversionWizard mWizard;
        public bool DefaultTaretApp { get; set; }

        public ConversionConfigurationWzardPage()
        {
            InitializeComponent();
        }

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (ActionsConversionWizard)WizardEventArgs.Wizard;
                    break;
                case EventType.Active:
                    Init(WizardEventArgs);            
                    break;
                case EventType.Finish:
                    break;
            }
        }
        
        private void Init(WizardEventArgs WizardEventArgs)
        {
            cmbTargetApp.BindControl(mWizard.BusinessFlow.TargetApplications.Select(x => x.Name).ToList());
            if ((cmbTargetApp != null) && (cmbTargetApp.Items.Count > 0))
            {
                cmbTargetApp.SelectedIndex = 0;
            }
            mWizard.ChkDefaultTargetApp = Convert.ToBoolean(chkDefaultTargetApp.IsChecked);
        }

        private void chkDefaultTargetApp_Checked(object sender, RoutedEventArgs e)
        {
            if ((cmbTargetApp != null) && (cmbTargetApp.Items.Count > 0))
            {
                mWizard.ChkDefaultTargetApp = Convert.ToBoolean(chkDefaultTargetApp.IsChecked);
                cmbTargetApp.IsEnabled = true;
                btnRefresh.IsEnabled = true;
            }
        }

        private void chkDefaultTargetApp_Unchecked(object sender, RoutedEventArgs e)
        {
            if ((cmbTargetApp != null) && (cmbTargetApp.Items.Count > 0))
            {
                mWizard.ChkDefaultTargetApp = Convert.ToBoolean(chkDefaultTargetApp.IsChecked);
                cmbTargetApp.IsEnabled = false;
                btnRefresh.IsEnabled = false;
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            cmbTargetApp.BindControl(mWizard.BusinessFlow.TargetApplications.Select(x => x.Name).ToList());
            if ((cmbTargetApp != null) && (cmbTargetApp.Items.Count > 0))
            {
                cmbTargetApp.SelectedIndex = 0;
            }
        }

        private void CmbTargetApp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cmbTargetApp.SelectedIndex > -1)
            {
                mWizard.CmbTargetApp = Convert.ToString(cmbTargetApp.SelectedValue);
            }
        }

        private void RadNewActivity_Checked(object sender, RoutedEventArgs e)
        {
            if (mWizard != null)
            {
                mWizard.RadNewActivity = Convert.ToBoolean(radNewActivity.IsChecked); 
            }
        }

        private void RadSameActivity_Checked(object sender, RoutedEventArgs e)
        {
            if (mWizard != null)
            {
                mWizard.RadNewActivity = !Convert.ToBoolean(radSameActivity.IsChecked);
            }
        }
    }
}
