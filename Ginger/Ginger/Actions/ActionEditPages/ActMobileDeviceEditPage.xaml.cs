#region License
/*
Copyright © 2014-2023 European Support Limited

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
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;

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

            BindControls();
            SetControlsView();
        }

        private void BindControls()
        {
            xOperationNameComboBox.Init(mAct, nameof(mAct.MobileDeviceAction), typeof(ActMobileDevice.eMobileDeviceAction), ActionNameComboBox_SelectionChanged);

            xKeyPressComboBox.Init(mAct, nameof(mAct.MobilePressKey), typeof(ActMobileDevice.ePressKey));
           
            xX1TxtBox.Init(Context.GetAsContext(mAct.Context), mAct.X1, nameof(ActInputValue.Value));
            xY1TxtBox.Init(Context.GetAsContext(mAct.Context), mAct.Y1, nameof(ActInputValue.Value));
            xX2TxtBox.Init(Context.GetAsContext(mAct.Context), mAct.X2, nameof(ActInputValue.Value));
            xY2TxtBox.Init(Context.GetAsContext(mAct.Context), mAct.Y2, nameof(ActInputValue.Value));
        }

        private void ActionNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetControlsView();
        }

        private void SetControlsView()
        {
            xKeyPressPnl.Visibility = Visibility.Collapsed;
            xXY1Pnl.Visibility = Visibility.Collapsed;
            xXY2Pnl.Visibility = Visibility.Collapsed;

            switch (mAct.MobileDeviceAction)
            {
                case ActMobileDevice.eMobileDeviceAction.PressKey:
                case ActMobileDevice.eMobileDeviceAction.LongPressKey:
                    xKeyPressPnl.Visibility = Visibility.Visible;
                    break;

                case ActMobileDevice.eMobileDeviceAction.PressXY:
                case ActMobileDevice.eMobileDeviceAction.LongPressXY:
                case ActMobileDevice.eMobileDeviceAction.TapXY:
                    xXY1Pnl.Visibility = Visibility.Visible;
                    break;

                case ActMobileDevice.eMobileDeviceAction.DragXYXY:
                case ActMobileDevice.eMobileDeviceAction.SwipeByCoordinates:
                    xXY1Pnl.Visibility = Visibility.Visible;
                    xXY2Pnl.Visibility = Visibility.Visible;
                    break;
            }
        }      


    }
}
