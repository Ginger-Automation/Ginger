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

using GingerCore.Actions;
using System.Windows.Controls;
using System.Windows;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for ActMobileDeviceEditPage.xaml
    /// </summary>
    public partial class ActMobileDeviceEditPage : Page
    {

        ActMobileDevice mAct;

        public ActMobileDeviceEditPage(ActMobileDevice Act)
        {
            InitializeComponent();

            mAct = Act;


            xActionNameComboBox.Init(mAct, nameof(mAct.MobileDeviceAction), typeof(ActMobileDevice.eMobileDeviceAction));
            xKeyPressComboBox.Init(mAct, nameof(mAct.MobilePressKey), typeof(ActMobileDevice.ePressKey));
            xActionNameComboBox.ComboBox.SelectionChanged -= ActionNameComboBox_SelectionChanged;
            xActionNameComboBox.ComboBox.SelectionChanged += ActionNameComboBox_SelectionChanged;

            HideUnHideKeyPress();
        }

        private void HideUnHideKeyPress()
        {
            if (mAct != null && mAct.MobileDeviceAction == ActMobileDevice.eMobileDeviceAction.PressKey)
            {
                xKeyPressComboBox.Visibility = Visibility.Visible;
                xKeyPressLable.Visibility = Visibility.Visible;
            }
            else
            {
                xKeyPressComboBox.Visibility = Visibility.Collapsed;
                xKeyPressLable.Visibility = Visibility.Collapsed;
            }
        }

        private void ActionNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            HideUnHideKeyPress();
        }
    }
}
