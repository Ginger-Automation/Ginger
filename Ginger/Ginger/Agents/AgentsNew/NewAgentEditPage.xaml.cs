#region License
/*
Copyright © 2014-2019 European Support Limited

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

using System.Windows;
using System.Windows.Controls;

namespace Ginger.Agents
{
    /// <summary>
    /// Interaction logic for AgentEditPage.xaml
    /// </summary>
    public partial class NewAgentEditPage : Page
    {
        //NewAgent mAgent;

        // public NewAgentEditPage(NewAgent agent)
        public NewAgentEditPage()
        {
            InitializeComponent();

            //mAgent = agent;
            //AgentNameTextBox.BindControl(mAgent, nameof(NewAgent.Name));

            //List<DriverInfo> drivers = WorkSpace.Instance.PlugInsManager.GetAllDrivers();
            //DriverComboBox.ItemsSource = drivers;
            //DriverComboBox.DisplayMemberPath = nameof(DriverInfo.Name);
            //DriverComboBox.BindControl(mAgent, nameof(NewAgent.PluginDriverName));

            //FillGingerGridComboBox();
            //GingerGridComboBox.BindControl(mAgent, nameof(NewAgent.GingerGridName));

            //PlatformComboBox.BindControl(mAgent, nameof(NewAgent.Platform));

            //ScriptTextBox.Text = mAgent.StartAgentScript();
        }

        private void FillGingerGridComboBox()
        {
            //TODO: temp fix me add RI of GingerGrid
            GingerGridComboBox.Items.Add("Local Ginger Grid");
            GingerGridComboBox.Items.Add("Ginger Grid 1");
            GingerGridComboBox.Items.Add("Ginger Grid 2");
            GingerGridComboBox.Items.Add("Ginger Grid 3");
            GingerGridComboBox.Items.Add("Ginger Grid 4");
        }

        private void driverTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void ParamsGridVEButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void InitConfigGrid()
        {
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            //// TODO: run it async
            //TestButton.IsEnabled = false;
            //mAgent.StartDriver();

            ////TODO: add test which each agent will impl

            //mAgent.CloseDriver();
            //TestButton.IsEnabled = true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
