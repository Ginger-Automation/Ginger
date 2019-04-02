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
using System.Collections.Generic;
using System.Windows;

namespace GingerWPF.GeneralLib
{
    /// <summary>
    /// Interaction logic for ActionSelectorWindow.xaml
    /// </summary>
    public partial class ActionSelectorWindow : Window
    {

        public ActionSelectorWindow(string Title, List<ActionSelectorItem> actions)
        {
            InitializeComponent();
            this.Title = Title;
           
            MainList.ItemsSource = actions;
            MainList.DisplayMemberPath = nameof(ActionSelectorItem.Name);
            MainList.SelectedIndex = 0;
            MainList.Focus();            
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            RunSelectedAction();            
        }

        private void RunSelectedAction()
        {
            ActionSelectorItem action = (ActionSelectorItem)MainList.SelectedItem;
            if (action != null)
            {
                this.Hide();
                action.Action.Invoke();
                this.Close();
            }
        }

        private void MainList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RunSelectedAction();
        }

        private void MainList_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                RunSelectedAction();
            }
        }
    }
}
