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

using Ginger.Actions._Common.ActUIElementLib;
using GingerCore.Actions.Common;
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
            }
        }
        
        private void Init(WizardEventArgs WizardEventArgs)
        {
            mWizard.DefaultTargetAppChecked = Convert.ToBoolean(xChkDefaultTargetApp.IsChecked);
            
            LocateByPOMElementPage locateByPOMElementPage = new LocateByPOMElementPage(null, nameof(ActUIElement.ElementType), mWizard, nameof(mWizard.SelectedPOMObjectName), true);
            xLocateValueEditFrame.Content = locateByPOMElementPage;
            DataContext = mWizard;
            BindTargetApplication();
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            BindTargetApplication();
        }

        private void BindTargetApplication()
        {
            xCmbTargetApp.BindControl(mWizard.BusinessFlow.TargetApplications.Select(x => x.Name).ToList());
            if ((xCmbTargetApp != null) && (xCmbTargetApp.Items.Count > 0))
            {
                xCmbTargetApp.SelectedIndex = 0;
            }
        }
    }
}
