#region License
/*
Copyright © 2014-2026 European Support Limited

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
using Ginger.UserControlsLib;
using GingerCore;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Configurations
{
    /// <summary>
    /// Interaction logic for AskLisaConfigurationsPage.xaml
    /// </summary>
    public partial class AskLisaConfigurationsPage : GingerUIPage
    {
        private AskLisaConfiguration userConfig;
        public AskLisaConfigurationsPage()
        {
            InitializeComponent();

            Init();
        }

        private void Init()
        {
            userConfig = WorkSpace.Instance.Solution.AskLisaConfiguration;

            userConfig.StartDirtyTracking();
            WorkSpace.Instance.CurrentSelectedItem = userConfig;
            SetControls();

        }

        private void SetControls()
        {
            Context mContext = new();

            xEnableChatBotRadioButton.Init(typeof(AskLisaConfiguration.eEnableChatBot), xEnableChatBotPanel, userConfig, nameof(AskLisaConfiguration.EnableChat), xEnableChatBotRadioButton_CheckedHandler);
            xHostLabelTextBox.Init(mContext, userConfig, nameof(AskLisaConfiguration.Host));
            xAuthUrlLabelTextBox.Init(mContext, userConfig, nameof(AskLisaConfiguration.AuthenticationServiceURL));
            xClientIdLabelTextBox.Init(mContext, userConfig, nameof(AskLisaConfiguration.ClientId));
            xClientSecretLabelTextBox.Init(mContext, userConfig, nameof(AskLisaConfiguration.ClientSecret));

            xStartNewChatLabelTextBox.Init(mContext, userConfig, nameof(AskLisaConfiguration.StartNewChat));
            xContinueChatLabelTextBox.Init(mContext, userConfig, nameof(AskLisaConfiguration.ContinueChat));
            xAccountLabelTextBox.Init(mContext, userConfig, nameof(AskLisaConfiguration.Account));
            xDomainTypeLabelTextBox.Init(mContext, userConfig, nameof(AskLisaConfiguration.DomainType));
            xTemperatureLevelLabelTextBox.Init(mContext, userConfig, nameof(AskLisaConfiguration.TemperatureLevel));
            xMaxTokenValueTextBox.Init(mContext, userConfig, nameof(AskLisaConfiguration.MaxTokenValue));
            xDataPathLabelTextBox.Init(mContext, userConfig, nameof(AskLisaConfiguration.DataPath));
            xGrantTypeLabelTextBox.Init(mContext, userConfig, nameof(AskLisaConfiguration.GrantType));
        }

        private void xEnableChatBotRadioButton_CheckedHandler(object sender, RoutedEventArgs e)
        {
            string value = ((RadioButton)sender).Tag?.ToString();

            Enum.TryParse(value, out AskLisaConfiguration.eEnableChatBot enableChatBot);

            if (enableChatBot == AskLisaConfiguration.eEnableChatBot.Yes)
            {
                xChatBotConfigGrid.Visibility = Visibility.Visible;
                xChabotAdvancedLabelsExpander.Visibility = Visibility.Visible;
            }
            else
            {
                xChatBotConfigGrid.Visibility = Visibility.Collapsed;
                xChabotAdvancedLabelsExpander.Visibility = Visibility.Collapsed;
            }
        }

        // Encrypt the client secret unless it's already encrypted or a value expression.
        // This ensures that sensitive data is stored securely without altering predefined expressions or duplicating encryption.
        private void xClientIdLabelTextBox_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (!EncryptionHandler.IsStringEncrypted(xClientIdLabelTextBox.ValueTextBox.Text))
            {
                xClientIdLabelTextBox.ValueTextBox.Text = ValueExpression.IsThisAValueExpression(xClientIdLabelTextBox.ValueTextBox.Text) ? xClientIdLabelTextBox.ValueTextBox.Text : EncryptionHandler.EncryptwithKey(xClientIdLabelTextBox.ValueTextBox.Text);

            }
        }

        private void xClientSecretLabelTextBox_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (!EncryptionHandler.IsStringEncrypted(xClientSecretLabelTextBox.ValueTextBox.Text))
            {
                xClientSecretLabelTextBox.ValueTextBox.Text = ValueExpression.IsThisAValueExpression(xClientSecretLabelTextBox.ValueTextBox.Text) ? xClientSecretLabelTextBox.ValueTextBox.Text : EncryptionHandler.EncryptwithKey(xClientSecretLabelTextBox.ValueTextBox.Text);

            }
        }
    }
}
