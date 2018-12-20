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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using GingerCore;
using GingerWPF.WizardLib;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using static Ginger.ExtensionMethods;

namespace Ginger.Agents.AddAgentWizardLib
{
    /// <summary>
    /// Interaction logic for AddAgentDetailsPage.xaml
    /// </summary>
    public partial class AddAgentDetailsPage : Page, IWizardPage
    {
        AddAgentWizard mWizard;
        
        
        public AddAgentDetailsPage()
        {
            InitializeComponent();
        }        

        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            mWizard = (AddAgentWizard)WizardEventArgs.Wizard;
            switch (WizardEventArgs.EventType)
            {            
                case EventType.Init:                                        
                    xAgentNameTextBox.BindControl(mWizard.Agent, nameof(Agent.Name));                  
                    xAgentNameTextBox.AddValidationRule(new AgentNameValidationRule());
                    xAgentNameTextBox.Focus();

                    xAgentDescriptionTextBox.BindControl(mWizard.Agent, nameof(Agent.Notes));
                    xAgentTagsViewer.Init(mWizard.Agent.Tags);

                    xPlatformTypeComboBox.SelectionChanged += xPlatformTypeComboBox_SelectionChanged;
                    App.FillComboFromEnumVal(xPlatformTypeComboBox, mWizard.Agent.Platform);                    

                    xDriverTypeComboBox.BindControl(mWizard.Agent, nameof(Agent.DriverType));
                    xDriverTypeComboBox.SelectionChanged += xDriverTypeComboBox_SelectionChanged;
                    xDriverTypeComboBox.AddValidationRule(eValidationRule.CannotBeEmpty);                    
                    xDriverTypeStackPanel.Visibility = Visibility.Collapsed;

                    if (mWizard.Agent.AgentType == Agent.eAgentType.Service)
                    {
                        xPluginRadioButton.IsChecked = true;
                    }
                    else
                    {
                        xDriverRadioButton.IsChecked = true;
                    }
                    

                    xPlatformTypeComboBox.SelectedIndex = 0;
                    break;                
            }

        }
        

        private void xPlatformTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            xDriverTypeComboBox.SelectedItem = null;
            xDriverTypeComboBox.Items.Clear();

            List<object> driverTypeValues = mWizard.Agent.GetDriverTypesByPlatfrom(xPlatformTypeComboBox.SelectedItem.ToString());
            App.FillComboFromEnumVal(xDriverTypeComboBox, mWizard.Agent.DriverType, driverTypeValues, false);
            if (xDriverTypeComboBox.Items.Count > 0)
                xDriverTypeComboBox.SelectedItem = xDriverTypeComboBox.Items[0];

            if (xDriverTypeComboBox.Items.Count > 1)
            {
                xDriverTypeStackPanel.Visibility = Visibility.Visible;                
            }
            else
            {
                xDriverTypeStackPanel.Visibility = Visibility.Collapsed;
            }            
        }

        private void xDriverTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mWizard.Agent.InitDriverConfigs();
        }

        private void xDriverRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            mWizard.Agent.AgentType = Agent.eAgentType.Driver;            
            ShowConfig();
        }

        private void xPluginRadioButton_Checked(object sender, RoutedEventArgs e)
        {            
            mWizard.Agent.AgentType = Agent.eAgentType.Service;
            mWizard.Agent.DriverType = Agent.eDriverType.NA;
            ShowConfig();
        }

        void ShowConfig()
        {
            if (mWizard.Agent.AgentType == Agent.eAgentType.Service)
            {
                xPluginConfigStackPanel.Visibility = Visibility.Visible;
                xDriverConfigStackPanel.Visibility = Visibility.Collapsed;

                // Plugin combo
                xPluginIdComboBox.ItemsSource = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<PluginPackage>();
                xPluginIdComboBox.DisplayMemberPath = nameof(PluginPackage.PluginId);                
                xPluginIdComboBox.BindControl(mWizard.Agent, nameof(Agent.PluginId));
            }
            else
            {
                xPluginConfigStackPanel.Visibility = Visibility.Collapsed;
                xDriverConfigStackPanel.Visibility = Visibility.Visible;
            }
            
            
        }

        private void xPluginIdComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PluginPackage p = (PluginPackage)xPluginIdComboBox.SelectedItem;
            p.LoadServicesFromJSON();
            xServiceIdComboBox.ItemsSource = p.Services;
            xServiceIdComboBox.DisplayMemberPath = nameof(PluginServiceInfo.ServiceId);
            xServiceIdComboBox.SelectedValuePath = nameof(PluginServiceInfo.ServiceId);
            xServiceIdComboBox.BindControl(mWizard.Agent, nameof(Agent.ServiceId));

            // auto select if there is only one service in the plugin
            if (p.Services.Count == 1)
            {
                xServiceIdComboBox.SelectedItem = p.Services[0];
            }
        }
    }
}
