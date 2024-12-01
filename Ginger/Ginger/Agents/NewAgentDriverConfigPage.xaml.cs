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

using GingerCore;
using GingerCore.GeneralLib;
using System.Windows;
using System.Windows.Controls;

namespace Ginger.Agents
{
    /// <summary>
    /// Interaction logic for NewAgentDriverConfigPage.xaml
    /// </summary>
    public partial class NewAgentDriverConfigPage : Page
    {
        Agent mAgent;
        public NewAgentDriverConfigPage(Agent agent)
        {
            mAgent = agent;
            InitializeComponent();
            agent.AgentOperations.SetDriverConfiguration();
            StackPanel dynamicPanel = new StackPanel { Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center };

            foreach (DriverConfigParam DCp in mAgent.DriverConfiguration)
            {
                AgentConfigControl AGC = new AgentConfigControl(DCp)
                {
                    Margin = new Thickness(10, 0, 0, 0)
                };

                dynamicPanel.Children.Add(AGC);
            }
            dynamicPanel.UpdateLayout();
            Cntent.ClearAndSetContent(dynamicPanel);
        }
    }
}
