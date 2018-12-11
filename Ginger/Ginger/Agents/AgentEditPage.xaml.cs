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
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using static GingerCore.Agent;

namespace Ginger.Agents
{
    /// <summary>
    /// Interaction logic for AgentEditPage.xaml
    /// </summary>
    public partial class AgentEditPage : Page
    {        
        Agent mAgent;
        ePlatformType mOriginalPlatformType;
        string mOriginalDriverType;
        
        public AgentEditPage(Agent agent)
        {
            InitializeComponent();

            if (agent != null)
            {
                mAgent = agent;

                App.ObjFieldBinding(xAgentNameTextBox, TextBox.TextProperty, mAgent, nameof(Agent.Name));
                App.ObjFieldBinding(xDescriptionTextBox, TextBox.TextProperty, mAgent, nameof(Agent.Notes));
                App.ObjFieldBinding(xAgentTypelbl, Label.ContentProperty, mAgent, nameof(Agent.AgentType));
                TagsViewer.Init(mAgent.Tags);

                if (mAgent.AgentType == eAgentType.Driver)
                {
                    mOriginalPlatformType = mAgent.Platform;
                    mOriginalDriverType = mAgent.DriverType.ToString();

                    xPlatformTxtBox.Text = mOriginalPlatformType.ToString();
                    SetDriverTypeCombo();
                    App.ObjFieldBinding(xDriverTypeComboBox, ComboBox.TextProperty, mAgent, nameof(Agent.DriverType));
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
                         
                xAgentConfigFrame.SetContent(new AgentDriverConfigPage(mAgent));                
            }
        }

        private void xPluginIdComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PluginPackage p = (PluginPackage)xPluginIdComboBox.SelectedItem;
            p.LoadServicesFromJSON();
            xServiceIdComboBox.ItemsSource = p.Services;
            xServiceIdComboBox.DisplayMemberPath = nameof(PluginServiceInfo.ServiceId);
            xServiceIdComboBox.SelectedValuePath = nameof(PluginServiceInfo.ServiceId);
            xServiceIdComboBox.BindControl(mAgent, nameof(Agent.ServiceId));

            // auto select if there is only one service in the plugin
            if (p.Services.Count == 1)
            {
                xServiceIdComboBox.SelectedItem = p.Services[0];
            }
        }

        private void SetDriverTypeCombo()
        {            
            List<object> lst = new List<object>();
            foreach (eDriverType item in Enum.GetValues(typeof(eDriverType)))
            {
                var platform = Agent.GetDriverPlatformType(item);
                if (platform == mOriginalPlatformType)
                {
                    lst.Add(item);
                }
            }
            
            App.FillComboFromEnumVal(xDriverTypeComboBox, mAgent.DriverType, lst);           
        }


        private void driverTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xDriverTypeComboBox.SelectedItem == null) return;

            if ((Agent.eDriverType)xDriverTypeComboBox.SelectedValue == mAgent.DriverType) return;

            //notify user that all driver configurations will be reset
            if (xDriverTypeComboBox.SelectedItem.ToString() != mOriginalDriverType)
            {
                if (Reporter.ToUser(eUserMsgKeys.ChangingAgentDriverAlert) == MessageBoxResult.No)
                {
                    foreach (object item in xDriverTypeComboBox.Items)
                        if (item.ToString() == mOriginalDriverType)
                        {
                            xDriverTypeComboBox.SelectedItem = item;
                            break;
                        }                    
                }
                else
                {
                    mOriginalDriverType = xDriverTypeComboBox.SelectedItem.ToString();                    
                    mAgent.InitDriverConfigs(); 
                }                
            }
        }
                
        private void xTestBtn_Click(object sender, RoutedEventArgs e)
        {
            xTestBtn.IsEnabled = false;                        
            try
            {
                mAgent.Test();
            }
            finally
            {
                xTestBtn.IsEnabled = true;
            }
        }
      
    }
}
