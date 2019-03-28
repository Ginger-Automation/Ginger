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

using Amdocs.Ginger.Common;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using GingerCore;

namespace Ginger.Agents
{
    /// <summary>
    /// Interaction logic for NewAgentPage.xaml
    /// </summary>
    public partial class NewAgentPage : Page
    {
        Agent mNewAgent = new Agent();
        GenericWindow _pageGenericWin = null;
        bool okClicked = false;

        public NewAgentPage()
        {
            //DELETE ME!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

            InitializeComponent();

            mNewAgent.Active = true;
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(AgentNameTextBox, TextBox.TextProperty, mNewAgent, Agent.Fields.Name);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DriverTypeComboBox, ComboBox.TextProperty, mNewAgent, Agent.Fields.DriverType);

            PlatformTypeComboBox.SelectionChanged += PlatformTypeComboBox_SelectionChanged;
            GingerCore.General.FillComboFromEnumObj(PlatformTypeComboBox, mNewAgent.Platform); 
            AgentNameTextBox.Focus();
        }

        private void PlatformTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DriverTypeComboBox.SelectedItem = null;
            DriverTypeComboBox.Items.Clear();

            List<object> driverTypeValues = mNewAgent.GetDriverTypesByPlatfrom(PlatformTypeComboBox.SelectedItem.ToString());
            GingerCore.General.FillComboFromEnumObj(DriverTypeComboBox, mNewAgent.DriverType, driverTypeValues, false);
            if (DriverTypeComboBox.Items.Count > 0)
                DriverTypeComboBox.SelectedItem = DriverTypeComboBox.Items[0];
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            //validate details
            if (AgentNameTextBox.Text.Trim() == string.Empty) { Reporter.ToUser(eUserMsgKey.MissingNewAgentDetails, "name"); return; }
            else if (AgentNameTextBox.Text.Trim().IndexOfAny(new char[] { '/', '\\', '*', ':', '?', '"', '<', '>', '|' }) != -1) { Reporter.ToUser(eUserMsgKey.InvalidCharactersWarning, string.Empty); return; }
            else if (PlatformTypeComboBox.SelectedItem == null) { Reporter.ToUser(eUserMsgKey.MissingNewAgentDetails, "Platform type"); return; }
            else if (DriverTypeComboBox.SelectedItem == null) { Reporter.ToUser(eUserMsgKey.MissingNewAgentDetails, "Driver type"); return; }

            okClicked = true;
            _pageGenericWin.Close();
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {            
            Button okBtn = new Button();
            okBtn.Content = "OK";
            okBtn.Click += new RoutedEventHandler(OKButton_Click);
            ObservableList<Button> winButtons = new ObservableList<Button>();
            winButtons.Add(okBtn);
            
            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, winButtons, true, "Cancel");
        }

        public Agent NewAgent
        {
            get
            {
                if (okClicked)
                {
                    return mNewAgent;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
