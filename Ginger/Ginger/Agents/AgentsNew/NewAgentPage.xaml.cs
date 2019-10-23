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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Ginger;
using GingerCoreNET.GeneralLib;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.AgentsLib
{
    /// <summary>
    /// Interaction logic for NewAgentPage.xaml
    /// </summary>
    public partial class NewAgentPage : Page
    {
        //NewAgent mNewAgent = new NewAgent();
        GenericWindow mGenericWindow = null;        
        public NewAgentPage()
        {
            InitializeComponent();            

            //AgentNameTextBox.BindControl(mNewAgent, nameof(NewAgent.Name));
            //DriverTypeComboBox.BindControl(mNewAgent, nameof(NewAgent.PluginDriverName));
            
            //AgentNameTextBox.Focus();
        }
      
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            //WorkSpace.Instance.SolutionRepository.AddRepositoryItem(mNewAgent);
            //mGenericWindow.Close();
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {            
            Button okBtn = new Button();
            okBtn.Content = "OK";
            okBtn.Click += new RoutedEventHandler(OKButton_Click);
            ObservableList<Button> winButtons = new ObservableList<Button>();
            winButtons.Add(okBtn);

            GenericWindow.LoadGenericWindow(ref mGenericWindow, null, windowStyle, this.Title, this, winButtons, true, "Cancel");
        }
    }
}