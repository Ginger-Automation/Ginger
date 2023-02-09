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

using Amdocs.Ginger.Common;
using GingerWPF.WizardLib;
using System.Windows.Controls;

namespace Ginger.Actions.ApiActionsConversion
{
    /// <summary>
    /// Interaction logic for ApiConversionConfigurationWzardPage.xaml
    /// </summary>
    public partial class ApiConversionConfigurationWzardPage : Page, IWizardPage
    {
        IActionsConversionProcess mConversionProcess;

        /// <summary>
        /// Constructor for configuration page
        /// </summary>
        public ApiConversionConfigurationWzardPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Wizard events
        /// </summary>
        /// <param name="WizardEventArgs"></param>
        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mConversionProcess = (IActionsConversionProcess)WizardEventArgs.Wizard;
                    ((WizardWindow)((WizardBase)mConversionProcess).mWizardWindow).ShowFinishButton(false);
                    break;
                case EventType.Active:
                    Init(WizardEventArgs);
                    break;
                default:
                    mConversionProcess = (IActionsConversionProcess)WizardEventArgs.Wizard;
                    break;
            }
        }
        
        /// <summary>
        /// This method is used to init the configuration settings page
        /// </summary>
        /// <param name="WizardEventArgs"></param>
        private void Init(WizardEventArgs WizardEventArgs)
        {
            ((WizardWindow)((WizardBase)mConversionProcess).mWizardWindow).ShowFinishButton(false);
            DataContext = mConversionProcess;
        }        
    }
}
