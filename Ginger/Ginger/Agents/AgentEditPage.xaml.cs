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
using Amdocs.Ginger.Common.Drivers.CoreDrivers.Web;
using Amdocs.Ginger.CoreNET.Drivers.CoreDrivers.Web;
using Amdocs.Ginger.Repository;
using Ginger.UserControlsLib;
using GingerCore;
using GingerCore.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static GingerCore.Agent;

namespace Ginger.Agents
{
    /// <summary>
    /// Interaction logic for AgentEditPage.xaml
    /// </summary>
    public partial class AgentEditPage : GingerUIPage
    {
        private Agent mAgent;
        ePlatformType mOriginalPlatformType;
        string mOriginalDriverType;
        bool IsReadOnly, IsEnabledCheckBox;
        private readonly bool _ignoreValidationRules;
        private readonly General.eRIPageViewMode _viewMode;

        public AgentEditPage(Agent agent, bool isReadOnly = false, bool ignoreValidationRules = false, General.eRIPageViewMode viewMode = General.eRIPageViewMode.Standalone)
        {
            InitializeComponent();
            //xAgentNameTextBox.IsReadOnly

            this.IsReadOnly = isReadOnly;
            _ignoreValidationRules = ignoreValidationRules;
            _viewMode = viewMode;
            ChangeContorlsReadOnly(IsReadOnly);

            if (agent != null)
            {
                mAgent = agent;
                PropertyChangedEventManager.AddHandler(mAgent, Agent_PropertyChanged, propertyName: string.Empty);
                CurrentItemToSave = mAgent;
                xShowIDUC.Init(mAgent);
                BindingHandler.ObjFieldBinding(xAgentNameTextBox, TextBox.TextProperty, mAgent, nameof(Agent.Name));
                if (!_ignoreValidationRules)
                {
                    xAgentNameTextBox.AddValidationRule(new AgentNameValidationRule());
                }
                BindingHandler.ObjFieldBinding(xDescriptionTextBox, TextBox.TextProperty, mAgent, nameof(Agent.Notes));
                BindingHandler.ObjFieldBinding(xAgentTypelbl, Label.ContentProperty, mAgent, nameof(Agent.AgentType));
                BindingHandler.ObjFieldBinding(xPublishcheckbox, CheckBox.IsCheckedProperty, mAgent, nameof(RepositoryItemBase.Publish));

                if (WorkSpace.Instance.BetaFeatures.ShowHealenium)
                {
                    UpdateHealeniumUI();
                }
                else
                {
                    xHealeniumURLPnl.Visibility = Visibility.Collapsed;
                    xHealeniumcheckbox.Visibility = Visibility.Collapsed;
                }
                string allProperties = string.Empty;
                PropertyChangedEventManager.AddHandler(source: WorkSpace.Instance.BetaFeatures, handler: BetaFeatures_PropertyChanged, propertyName: allProperties);
                TagsViewer.Init(mAgent.Tags);

                if (mAgent.AgentType == eAgentType.Driver)
                {
                    mOriginalPlatformType = mAgent.Platform;
                    mOriginalDriverType = mAgent.DriverType.ToString();

                    xPlatformTxtBox.Text = mOriginalPlatformType.ToString();
                    SetDriverInformation();
                    BindingHandler.ObjFieldBinding(xDriverTypeComboBox, ComboBox.TextProperty, mAgent, nameof(Agent.DriverType));
                    xDriverTypeComboBox.SelectionChanged += driverTypeComboBox_SelectionChanged;
                }
                else//Plugin
                {
                    xDriverConfigPnl.Visibility = Visibility.Collapsed;
                    xPluginConfigPnl.Visibility = Visibility.Visible;

                    // Plugin combo
                    xPluginIdComboBox.ItemsSource = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<PluginPackage>();
                    xPluginIdComboBox.DisplayMemberPath = nameof(PluginPackage.PluginId);
                    xPluginIdComboBox.BindControl(mAgent, nameof(Agent.PluginId));
                }
                if (mAgent.AgentType == eAgentType.Driver)
                {
                    xAgentConfigFrame.SetContent(new AgentDriverConfigPage(mAgent, _viewMode));
                }
                else
                {
                    // xAgentConfigFrame.SetContent(new NewAgentDriverConfigPage(mAgent));
                    xAgentConfigFrame.SetContent(new AgentDriverConfigPage(mAgent, _viewMode));
                }
            }
        }

        private void Agent_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (string.Equals(e.PropertyName, nameof(Agent.DriverType)))
            {
                mAgent.AgentOperations.InitDriverConfigs();
                if (mAgent.Platform == ePlatformType.Web && DriverSupportMultipleBrowsers(mAgent.DriverType))
                {
                    PopulateBrowserTypeComboBox();
                    BrowserTypePanel.Visibility = Visibility.Visible;
                }
                else
                {
                    BrowserTypeComboBox.Items.Clear();
                    BrowserTypePanel.Visibility = Visibility.Collapsed;
                }
            }
        }

        private bool DriverSupportMultipleBrowsers(eDriverType driverType)
        {
            return driverType is eDriverType.Selenium or eDriverType.Playwright;
        }

        private void BetaFeatures_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(BetaFeatures.ShowHealenium))
            {
                if (WorkSpace.Instance.BetaFeatures.ShowHealenium)
                {
                    UpdateHealeniumUI();
                }
                else
                {
                    xHealeniumURLPnl.Visibility = Visibility.Collapsed;
                    xHealeniumcheckbox.Visibility = Visibility.Collapsed;
                }

            }

        }

        private void UpdateHealeniumUI()
        {
            bool isSeleniumDriver = mAgent.DriverType == eDriverType.Selenium;
            WebBrowserType? browserType = null;
            if (mAgent.DriverConfiguration != null)
            {
                DriverConfigParam? browserTypeParam = mAgent.DriverConfiguration.FirstOrDefault(p => string.Equals(p.Parameter, nameof(GingerWebDriver.BrowserType)));
                if (browserTypeParam != null && Enum.TryParse(browserTypeParam.Value, out WebBrowserType result))
                {
                    browserType = result;
                }
            }
            bool isRemoteBrowser = browserType.HasValue && browserType.Value == WebBrowserType.RemoteWebDriver;

            if (isSeleniumDriver && isRemoteBrowser)
            {
                xHealeniumcheckbox.IsEnabled = true;
                BindingHandler.ObjFieldBinding(xHealeniumcheckbox, CheckBox.IsCheckedProperty, mAgent, nameof(Agent.Healenium));
                xHealeniumcheckbox.Visibility = Visibility.Visible;
                if (mAgent.Healenium)
                {
                    xHealeniumURLPnl.Visibility = Visibility.Visible;
                    BindingHandler.ObjFieldBinding(xHealeniumURLTextBox, TextBox.TextProperty, mAgent, nameof(Agent.HealeniumURL));
                }
                else
                {
                    xHealeniumURLPnl.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                xHealeniumcheckbox.IsEnabled = false;
                xHealeniumcheckbox.Visibility = Visibility.Collapsed;
                xHealeniumURLPnl.Visibility = Visibility.Collapsed;
            }
        }

        private void ChangeContorlsReadOnly(bool isReadOnly)
        {
            xAgentNameTextBox.IsReadOnly = isReadOnly;
            xDescriptionTextBox.IsReadOnly = isReadOnly;
            xDriverTypeComboBox.IsReadOnly = isReadOnly;
            xPublishcheckbox.IsEnabled = !isReadOnly;
            xTestBtn.IsEnabled = !IsReadOnly;
        }

        private void xPluginIdComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PluginPackage p = (PluginPackage)xPluginIdComboBox.SelectedItem;
            if (p.PluginPackageOperations == null)
            {
                p.PluginPackageOperations = new PluginPackageOperations(p);
            }
            p.PluginPackageOperations.LoadServicesFromJSON();
            xServiceIdComboBox.ItemsSource = ((PluginPackageOperations)p.PluginPackageOperations).Services;
            xServiceIdComboBox.DisplayMemberPath = nameof(PluginServiceInfo.ServiceId);
            xServiceIdComboBox.SelectedValuePath = nameof(PluginServiceInfo.ServiceId);
            xServiceIdComboBox.BindControl(mAgent, nameof(Agent.ServiceId));

            // auto select if there is only one service in the plugin
            if (((PluginPackageOperations)p.PluginPackageOperations).Services.Count == 1)
            {
                xServiceIdComboBox.SelectedItem = ((PluginPackageOperations)p.PluginPackageOperations).Services[0];
            }
        }

        private void SetDriverInformation()
        {
            if (mAgent.Platform == ePlatformType.Web && DriverSupportMultipleBrowsers(mAgent.DriverType))
            {
                BrowserTypePanel.Visibility = Visibility.Visible;
                PopulateBrowserTypeComboBox();
            }
            else
            {
                BrowserTypeComboBox.Items.Clear();
                BrowserTypePanel.Visibility = Visibility.Collapsed;
            }
            List<object> lst = [];
            foreach (eDriverType item in Enum.GetValues(typeof(eDriverType)))
            {
                var platform = Agent.GetDriverPlatformType(item);
                if (platform == mOriginalPlatformType)
                {
                    lst.Add(item);
                }
            }

            GingerCore.General.FillComboFromEnumObj(xDriverTypeComboBox, mAgent.DriverType, lst);
            if (mAgent.SupportVirtualAgent())
            {
                xVirtualAgentsPanel.Visibility = Visibility.Visible;
                xAgentVirtualSupported.Content = "Yes";

                //VirtualAgentCount.Content = mAgent.VirtualAgentsStarted().Count;
            }
            else
            {
                xAgentVirtualSupported.Content = "No";
            }
        }

        private void PopulateBrowserTypeComboBox()
        {
            BrowserTypeComboBox.Items.Clear();

            foreach (WebBrowserType browser in GingerWebDriver.GetSupportedBrowserTypes(mAgent.DriverType))
            {
                BrowserTypeComboBox.Items.Add(new ComboEnumItem()
                {
                    text = browser.ToString(),
                    Value = browser
                });
            }
            BrowserTypeComboBox.SelectedValuePath = nameof(ComboEnumItem.Value);
            BrowserTypeComboBox.SelectedValue = Enum.Parse<WebBrowserType>(mAgent.GetParamValue(nameof(GingerWebDriver.BrowserType)));
        }

        private void BrowserTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }

            DriverConfigParam browserTypeParam = mAgent.GetParam(nameof(GingerWebDriver.BrowserType));
            browserTypeParam.Value = ((WebBrowserType)BrowserTypeComboBox.SelectedValue).ToString();
            if (xAgentConfigFrame.Content is AgentDriverConfigPage driverConfigPage)
            {
                driverConfigPage.SetDriverConfigsPageContent();
            }

            if (WorkSpace.Instance.BetaFeatures.ShowHealenium)
            {
                UpdateHealeniumUI();
            }
        }


        private void driverTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xDriverTypeComboBox.SelectedItem == null)
            {
                return;
            }

            if ((Agent.eDriverType)xDriverTypeComboBox.SelectedValue == mAgent.DriverType)
            {
                return;
            }

            //notify user that all driver configurations will be reset
            if (xDriverTypeComboBox.SelectedItem.ToString() != mOriginalDriverType)
            {
                if (Reporter.ToUser(eUserMsgKey.ChangingAgentDriverAlert) == Amdocs.Ginger.Common.eUserMsgSelection.No)
                {
                    foreach (object item in xDriverTypeComboBox.Items)
                    {
                        if (item.ToString() == mOriginalDriverType)
                        {
                            xDriverTypeComboBox.SelectedItem = item;
                            break;
                        }
                    }
                }
                else
                {
                    mOriginalDriverType = xDriverTypeComboBox.SelectedItem.ToString();
                }
            }
            if (WorkSpace.Instance.BetaFeatures.ShowHealenium)
            {
                UpdateHealeniumUI();
            }
            else
            {
                xHealeniumURLPnl.Visibility = Visibility.Collapsed;
                xHealeniumcheckbox.Visibility = Visibility.Collapsed;
            }
        }

        private async void xTestBtn_Click(object sender, RoutedEventArgs e)
        {
            xTestBtn.IsEnabled = false;
            Reporter.ToStatus(eStatusMsgKey.StaticStatusProcess, null, string.Format("Testing '{0}' Agent start...", mAgent.Name));
            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        mAgent.AgentOperations.Test();
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToLog(eLogLevel.ERROR, "Failed to test the agent", ex);
                    }
                });
            }
            finally
            {
                Reporter.HideStatusMessage();
                xTestBtn.IsEnabled = true;
            }
        }

        //private void RefreshVirtualAgentCount_Click(object sender, RoutedEventArgs e)
        //{
        //    VirtualAgentCount.Content = mAgent.VirtualAgentsStarted().Count;
        //}

        private void Healeniumcheckbox_Checked(object sender, RoutedEventArgs e)
        {
            mAgent.Healenium = true;
            xHealeniumURLPnl.Visibility = Visibility.Visible;

        }

        private void Healeniumcheckbox_UnChecked(object sender, RoutedEventArgs e)
        {
            mAgent.Healenium = false;
            xHealeniumURLPnl.Visibility = Visibility.Collapsed;

        }
    }
}
