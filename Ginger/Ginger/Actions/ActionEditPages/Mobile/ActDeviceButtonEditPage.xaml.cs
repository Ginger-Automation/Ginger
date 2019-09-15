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

using System.Windows.Controls;
using GingerCore.Actions.Common;

namespace Ginger.Actions._Common
{
    /// <summary>
    /// Interaction logic for ActDeviceButtonEditPage.xaml
    /// </summary>
    public partial class ActDeviceButtonEditPage : Page
    {
        ActDeviceButton mAct;

        public ActDeviceButtonEditPage(ActDeviceButton act)
        {
            InitializeComponent();

            mAct = act;

            ButtonActionComboBox.BindControl(mAct, ActDeviceButton.Fields.DeviceButtonAction);           
        }

        private void ButtonActionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {            

            if (ButtonActionComboBox.SelectedValue == null)
            {
                PressKeyComboBox.Visibility = System.Windows.Visibility.Visible;
                PressKeyCodeStackPanel.Visibility = System.Windows.Visibility.Collapsed;
                return;
            }

            if ((ActDeviceButton.eDeviceButtonAction)ButtonActionComboBox.SelectedValue == ActDeviceButton.eDeviceButtonAction.Press)
            {
                PressKeyComboBox.Visibility = System.Windows.Visibility.Visible;
                PressKeyCodeStackPanel.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                PressKeyComboBox.Visibility = System.Windows.Visibility.Collapsed;
                PressKeyCodeStackPanel.Visibility = System.Windows.Visibility.Visible;
            }
        }
    }
}
