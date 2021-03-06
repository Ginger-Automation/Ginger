#region License
/*
Copyright © 2014-2021 European Support Limited

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
using System.Threading.Tasks;
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

                xShowIDUC.Init(mAgent);
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xAgentNameTextBox, TextBox.TextProperty, mAgent, nameof(Agent.Name));
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xDescriptionTextBox, TextBox.TextProperty, mAgent, nameof(Agent.Notes));
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xAgentTypelbl, Label.ContentProperty, mAgent, nameof(Agent.AgentType));
                TagsViewer.Init(mAgent.Tags);

                if (mAgent.AgentType == eAgentType.Driver)
                {
                    mOriginalPlatformType = mAgent.Platform;
                    mOriginalDriverType = mAgent.DriverType.ToString();

                    xPlatformTxtBox.Text = mOriginalPlatformType.ToString();
                    SetDriverInformation();
                    GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xDriverTypeComboBox, ComboBox.TextProperty, mAgent, nameof(Agent.DriverType));
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
                    xAgentConfigFrame.SetContent(new AgentDriverConfigPage(mAgent));
                }
                else
                {
                   // xAgentConfigFrame.SetContent(new NewAgentDriverConfigPage(mAgent));
                   xAgentConfigFrame.SetContent(new AgentDriverConfigPage(mAgent));
                }
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

        private void SetDriverInformation()
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
            
            GingerCore.General.FillComboFromEnumObj(xDriverTypeComboBox, mAgent.DriverType, lst);   
            if(mAgent.SupportVirtualAgent())
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


        private void driverTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xDriverTypeComboBox.SelectedItem == null) return;

            if ((Agent.eDriverType)xDriverTypeComboBox.SelectedValue == mAgent.DriverType) return;

            //notify user that all driver configurations will be reset
            if (xDriverTypeComboBox.SelectedItem.ToString() != mOriginalDriverType)
            {
                if (Reporter.ToUser(eUserMsgKey.ChangingAgentDriverAlert) == Amdocs.Ginger.Common.eUserMsgSelection.No)
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
                
        private async void xTestBtn_Click(object sender, RoutedEventArgs e)
        {
            xTestBtn.IsEnabled = false;
            Reporter.ToStatus(eStatusMsgKey.StaticStatusProcess, null, string.Format("Testing '{0}' Agent start...", mAgent.Name));
            try
            {
                await Task.Run(() =>
                {
                    mAgent.Test();
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
    }
}
