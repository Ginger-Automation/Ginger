#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Ginger.ValidationRules;
using GingerCoreNET;
using System;
using System.Linq;
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
            userConfig = WorkSpace.Instance.UserProfile.AskLisaConfiguration;

            userConfig.StartDirtyTracking();
            SetControls();
            
        }

        private void SetControls()
        {
            Context mContext = new Context();
            xHostLabelTextBox.Init(mContext, userConfig, nameof(AskLisaConfiguration.Host));
            xAccountLabelTextBox.Init(mContext, userConfig, nameof(AskLisaConfiguration.Account));

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xEnableChatBotCheckBox, CheckBox.IsCheckedProperty, userConfig, nameof(AskLisaConfiguration.EnableChat));
            //GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xHostLabelTextBox,TextBox.TextProperty, userConfig, nameof(AskLisaConfiguration.Host));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xAccountLabelTextBox, TextBox.TextProperty, userConfig, nameof(AskLisaConfiguration.Account));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xDomainTypeLabelTextBox, TextBox.TextProperty, userConfig, nameof(AskLisaConfiguration.DomainType));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xStartNewChatLabelTextBox, CheckBox.IsCheckedProperty, userConfig, nameof(AskLisaConfiguration.StartNewChat));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xContinueChatLabelTextBox, TextBox.TextProperty, userConfig, nameof(AskLisaConfiguration.ContinueChat));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xTemperatureLevelLabelTextBox, TextBox.TextProperty, userConfig, nameof(AskLisaConfiguration.TemperatureLevel));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xMaxTokenValueTextBox, TextBox.TextProperty, userConfig, nameof(AskLisaConfiguration.MaxTokenValue));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xDataPathLabelTextBox, TextBox.TextProperty, userConfig, nameof(AskLisaConfiguration.DataPath));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xAuthUrlLabelTextBox, CheckBox.IsCheckedProperty, userConfig, nameof(AskLisaConfiguration.AuthenticationServiceURL));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xGrantTypeLabelTextBox, TextBox.TextProperty, userConfig, nameof(AskLisaConfiguration.GrantType));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xClientIdLabelTextBox, TextBox.TextProperty, userConfig, nameof(AskLisaConfiguration.ClientId));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xClientSecretLabelTextBox, TextBox.TextProperty, userConfig, nameof(AskLisaConfiguration.ClientSecret));
        }

        private void LoadConfigurations()
        {
            xClientIdLabelTextBox.Content = "BrAIn";
            xClientSecretLabelTextBox.Content = "AQEBrAIn_secret";
            xStartNewChatLabelTextBox.Content = "AQEQABot/Lisa/StartNewChat";
            xContinueChatLabelTextBox.Content = "AQEQABot/Lisa/ContinueNewChat";
            xAccountLabelTextBox.Content = "Ginger";
            xDomainTypeLabelTextBox.Content = "Knowledge Management";
            xTemperatureLevelLabelTextBox.Content = "0.1";
            xMaxTokenValueTextBox.Content = "2000";
            xDataPathLabelTextBox.Content = "./Data/Ginger";
            xGrantTypeLabelTextBox.Content = "client_credentials";
        }

        private void xEnableChatBotCheckBox_Checked(object sender, RoutedEventArgs e)
        {
           if(xChatBotConfigGrid.Visibility == Visibility.Collapsed)
            {
                
                xChatBotConfigGrid.Visibility = Visibility.Visible;
                xChabotAdvancedLabelsExpander.Visibility = Visibility.Visible;
            }
            else
            {
                xChatBotConfigGrid.Visibility= Visibility.Collapsed;
                xChabotAdvancedLabelsExpander.Visibility = Visibility.Collapsed;

            }
        }
    }
}
