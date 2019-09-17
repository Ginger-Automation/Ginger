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
using System.Windows.Data;
using GingerCore.Actions;
using GingerCore.Actions.ScreenCapture;
namespace Ginger.Actions
{
    public partial class ActLowLevelClicksEditPage : Page
    {
        private GingerCore.Actions.ActLowLevelClicks f;

        public ActLowLevelClicksEditPage(GingerCore.Actions.ActLowLevelClicks Act)
        {
            InitializeComponent();

            this.f = Act;
            GingerCore.General.FillComboFromEnumObj(ActionNameComboBox, Act.ActLowLevelClicksAction);          
            //TODO: fix hard coded ButtonAction use Fields: Fixed
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(MainWindowTitleTextBox, TextBox.TextProperty, Act, ActLowLevelClicks.Fields.WindowTitle, BindingMode.TwoWay);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(LocatorImageTextBox, TextBox.TextProperty, Act, ActLowLevelClicks.Fields.LocatorImgFile, BindingMode.OneWay);
            
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ActionNameComboBox, ComboBox.TextProperty, Act, "ActLowLevelClicksAction");            
        }

        private void CaptureLocatorImageButton_Click(object sender, RoutedEventArgs e)
        {
            App.MainWindow.WindowState = WindowState.Minimized;
            LocatorImageCaptureWindow sc = new LocatorImageCaptureWindow(f);
            sc.Show();
            LocatorImageTextBox.Text = sc.GetPathToExpectedImage();
        }
    }
}
