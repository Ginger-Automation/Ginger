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
using Amdocs.Ginger.CoreNET.External.WireMock;
using Ginger.GeneralWindows;
using Ginger.UserControlsLib;
using Ginger.ValidationRules;
using System;
using System.Windows;

namespace Ginger.ExternalConfigurations
{
    /// <summary>
    /// Interaction logic for WireMockConfigurationPage.xaml
    /// </summary>
    public partial class WireMockConfigurationPage : GingerUIPage
    {
        public WireMockConfiguration wireMockConfiguration;
        public WireMockAPI mockAPI;
        public WireMockConfigurationPage()
        {
            InitializeComponent();
            Init();
        }
        private void Init()
        {
            wireMockConfiguration = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<WireMockConfiguration>().Count == 0 ? new WireMockConfiguration() : WorkSpace.Instance.SolutionRepository.GetFirstRepositoryItem<WireMockConfiguration>();
            wireMockConfiguration.StartDirtyTracking();
            mockAPI = new WireMockAPI();
            SetControls();
        }

        private void SetControls()
        {
            Context mContext = new();
            xWMURLTextBox.Init(mContext, wireMockConfiguration, nameof(wireMockConfiguration.WireMockUrl));
            ApplyValidationRules();

        }

        private void ApplyValidationRules()
        {
            // check if fields have been populated (font-end validation)
            xWMURLTextBox.ValueTextBox.AddValidationRule(new ValidateEmptyValue("WireMock URL cannot be empty"));
        }

        private async void xTestConBtn_Click(object sender, RoutedEventArgs e)
        {
            if (xViewMappingBtn.Visibility == Visibility.Visible)
            {
                xViewMappingBtn.Visibility = Visibility.Collapsed;
            }

            GingerCoreNET.GeneralLib.General.CreateWireMockConfiguration();
            ShowLoader();
            xTestConBtn.IsEnabled = false;
            if (AreRequiredFieldsEmpty())
            {
                Reporter.ToUser(eUserMsgKey.RequiredFieldsEmpty);
                HideLoader();
                xTestConBtn.IsEnabled = true;
                return;
            }

            bool isAuthorized = await mockAPI.TestWireMockConnectionAsync(wireMockConfiguration.WireMockUrl);
            ShowConnectionResult(isAuthorized);
            EnableMappingBtn(isAuthorized);
            xTestConBtn.IsEnabled = true;
        }

        public void EnableMappingBtn(bool isAuthorized)
        {
            if (isAuthorized)
            {
                xViewMappingBtn.Visibility = Visibility.Visible;
            }
            else
            {
                xViewMappingBtn.Visibility = Visibility.Collapsed;
            }
        }

        public bool AreRequiredFieldsEmpty()
        {
            return string.IsNullOrEmpty(wireMockConfiguration.WireMockUrl);
        }


        public void ShowConnectionResult(bool isAuthorized)
        {
            if (isAuthorized)
            {
                HideLoader();
                Reporter.ToUser(eUserMsgKey.WireMockConnectionSuccess);
            }
            else
            {
                HideLoader();
                Reporter.ToUser(eUserMsgKey.WireMockConnectionFail);
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

        private void xViewMappingBtn_Click(object sender, RoutedEventArgs e)
        {
            WireMockMappingPage wmp = new();
            wmp.ShowAsWindow();
        }
    }
}
