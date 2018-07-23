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
                mOriginalPlatformType = mAgent.Platform;
                mOriginalDriverType = mAgent.DriverType.ToString();

                App.ObjFieldBinding(AgentNameTextBox, TextBox.TextProperty, mAgent, Agent.Fields.Name);
                txtPlatformType.Text = mOriginalPlatformType.ToString();
                
                App.ObjFieldBinding(NotesTextBox, TextBox.TextProperty, mAgent, Agent.Fields.Notes);

                // Remote Agent config
                //App.ObjFieldBinding(HostTextBox, TextBox.TextProperty, mAgent, Agent.Fields.Host);
                //App.ObjFieldBinding(PortTextBox, TextBox.TextProperty, mAgent, Agent.Fields.Port);                
                //App.ObjFieldBinding(RemoteCheckBox, CheckBox.IsCheckedProperty, mAgent, Agent.Fields.Remote);

                TagsViewer.Init(mAgent.Tags);

                SetDriverTypeCombo();                
                App.ObjFieldBinding(driverTypeComboBox, ComboBox.TextProperty, mAgent, Agent.Fields.DriverType);

                driverTypeComboBox.SelectionChanged += driverTypeComboBox_SelectionChanged;
                                    
                DriverConfigFrmae.SetContent(new AgentDriverConfigPage(mAgent));                
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
            
            App.FillComboFromEnumVal(driverTypeComboBox, mAgent.DriverType, lst);           
        }


        private void driverTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (driverTypeComboBox.SelectedItem == null) return;

            if ((Agent.eDriverType)driverTypeComboBox.SelectedValue == mAgent.DriverType) return;

            //notify user that all driver configurations will be reset
            if (driverTypeComboBox.SelectedItem.ToString() != mOriginalDriverType)
            {
                if (Reporter.ToUser(eUserMsgKeys.ChangingAgentDriverAlert) == MessageBoxResult.No)
                {
                    foreach (object item in driverTypeComboBox.Items)
                        if (item.ToString() == mOriginalDriverType)
                        {
                            driverTypeComboBox.SelectedItem = item;
                            break;
                        }                    
                }
                else
                {
                    mOriginalDriverType = driverTypeComboBox.SelectedItem.ToString();                    
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
