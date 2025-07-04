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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.External.Configurations;
using Amdocs.Ginger.CoreNET.GenAIServices;
using Ginger.UserControlsLib;
using Ginger.ValidationRules;
using GingerCore.GeneralLib;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.ExternalConfigurations
{
    /// <summary>
    /// Interaction logic for GingerPlayConfigurationpage.xaml
    /// </summary>
    public partial class GingerPlayConfigurationpage : GingerUIPage
    {
        public GingerPlayConfiguration gingerPlayConfiguration;
        public GingerPlayAPITokenManager GingerPlayAPITokenManager;
        public GingerPlayConfigurationpage()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            GingerCoreNET.GeneralLib.General.CreateGingerPlayConfiguration();
            gingerPlayConfiguration = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GingerPlayConfiguration>().Count == 0 ? new GingerPlayConfiguration() : WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<GingerPlayConfiguration>();
            gingerPlayConfiguration.StartDirtyTracking();
            SetControls();
        }

        private void SetControls()
        {
            Context mContext = new();
            xGatewayURLTextBox.Init(mContext, gingerPlayConfiguration, nameof(gingerPlayConfiguration.GingerPlayGatewayUrl));
            xClientIdTextBox.Init(mContext, gingerPlayConfiguration, nameof(gingerPlayConfiguration.GingerPlayClientId));
            xClientSecretTextBox.Init(mContext, gingerPlayConfiguration, nameof(gingerPlayConfiguration.GingerPlayClientSecret));
            BindingHandler.ObjFieldBinding(xAllowGingerPlayCheckBox, CheckBox.IsCheckedProperty, gingerPlayConfiguration, nameof(gingerPlayConfiguration.GingerPlayEnabled));
            BindingHandler.ObjFieldBinding(xGatewayURLTextBox, TextBox.TextProperty, gingerPlayConfiguration, nameof(gingerPlayConfiguration.GingerPlayGatewayUrl));
            BindingHandler.ObjFieldBinding(xClientIdTextBox, TextBox.TextProperty, gingerPlayConfiguration, nameof(gingerPlayConfiguration.GingerPlayClientId));
            BindingHandler.ObjFieldBinding(xClientSecretTextBox, TextBox.TextProperty, gingerPlayConfiguration, nameof(gingerPlayConfiguration.GingerPlayClientSecret));
            BindingHandler.ObjFieldBinding(xExecutionServiceCheckBox, CheckBox.IsCheckedProperty, gingerPlayConfiguration, nameof(gingerPlayConfiguration.GingerPlayExecutionServiceEnabled));
            BindingHandler.ObjFieldBinding(xReportServiceCheckBox, CheckBox.IsCheckedProperty, gingerPlayConfiguration, nameof(gingerPlayConfiguration.GingerPlayReportServiceEnabled));
            BindingHandler.ObjFieldBinding(xAIServiceCheckBox, CheckBox.IsCheckedProperty, gingerPlayConfiguration, nameof(gingerPlayConfiguration.GingerPlayAIServiceEnabled));
            ApplyValidationRules();
        }

        private void ApplyValidationRules()
        {
            // check if fields have been populated (font-end validation)
            xGatewayURLTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("Gateway URL cannot be empty"));
        }


        public bool AreRequiredFieldsEmpty()
        {
            return string.IsNullOrEmpty(gingerPlayConfiguration.GingerPlayGatewayUrl);
        }

        private void xAllowGingerPlayCheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            bool isChecked = xAllowGingerPlayCheckBox.IsChecked == true;
            gingerPlayConfiguration.GingerPlayEnabled = isChecked;
            xGatewayURLTextBox.IsEnabled = isChecked;
            xClientIdTextBox.IsEnabled = isChecked;
            xClientSecretTextBox.IsEnabled = isChecked;
            xReportServiceCheckBox.IsEnabled = isChecked;
            xAIServiceCheckBox.IsEnabled = isChecked;
            xExecutionServiceCheckBox.IsEnabled = isChecked;
            xTestConBtn.IsEnabled = isChecked;
        }

        private void xReportServiceCheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void xExecutionServiceCheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private void xAIServiceCheckBox_Checked(object sender, System.Windows.RoutedEventArgs e)
        {

        }

        private async void xTestConBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowLoader();
            xTestConBtn.IsEnabled = false;
            if (AreRequiredFieldsEmpty())
            {
                Reporter.ToUser(eUserMsgKey.RequiredFieldsEmpty);
                HideLoader();
                xTestConBtn.IsEnabled = true;
                return;
            }

            GingerCoreNET.GeneralLib.General.CreateGingerPlayConfiguration();

            GingerPlayAPITokenManager = new();
            bool isAuthorized = await GingerPlayAPITokenManager.InitClient();
            ShowConnectionResult(isAuthorized);
            HideLoader();
            xTestConBtn.IsEnabled = true;
        }

        private static void ShowConnectionResult(bool isAuthorized)
        {
            if (isAuthorized)
            {
                Reporter.ToUser(eUserMsgKey.GingerPlayConnectionSuccess);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.GingerPlayConnectionFail);
            }
        }
        public void HideLoader()
        {
            this.Dispatcher.Invoke(() =>
            {
                xProcessingImage.Visibility = Visibility.Hidden;
            });
        }

        public void ShowLoader()
        {
            this.Dispatcher.Invoke(() =>
            {
                xProcessingImage.Visibility = Visibility.Visible;
            });
        }

        private void xAllowGingerPlayCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {

            bool isChecked = xAllowGingerPlayCheckBox.IsChecked == true;
            gingerPlayConfiguration.GingerPlayEnabled = isChecked;
            xGatewayURLTextBox.IsEnabled = isChecked;
            xClientIdTextBox.IsEnabled = isChecked;
            xClientSecretTextBox.IsEnabled = isChecked;
            xReportServiceCheckBox.IsEnabled = isChecked;
            xAIServiceCheckBox.IsEnabled = isChecked;
            xExecutionServiceCheckBox.IsEnabled = isChecked;
            xTestConBtn.IsEnabled = isChecked;
        }
    }
}

