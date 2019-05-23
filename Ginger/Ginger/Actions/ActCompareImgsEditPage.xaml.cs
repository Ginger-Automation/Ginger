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
using Amdocs.Ginger.Common;
using GingerCore.Actions;
using GingerCore.Actions.ScreenCapture;
namespace Ginger.Actions
{
    public partial class ActCompareImgsEditPage : Page
    {
        private GingerCore.Actions.ActCompareImgs f;

        public ActCompareImgsEditPage(GingerCore.Actions.ActCompareImgs Act)
        {
            InitializeComponent();

            this.f = Act;
           
            //TODO: fix hard coded ButtonAction use Fields - changed 
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ScreenAreaCoordinatesTextBox, TextBox.TextProperty, Act, ActCompareImgs.Fields.Coordinates, BindingMode.OneWay);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(ExpectedImageTextBox, TextBox.TextProperty, Act, ActCompareImgs.Fields.ExpectedImgFile, BindingMode.OneWay);
            WindowNameTextBox.Init(Context.GetAsContext(Act.Context), Act.GetOrCreateInputParam(ActCompareImgs.Fields.WindowName), true, false, UCValueExpression.eBrowserType.Folder);
        }

        private void CaptureExpectedImageButton_Click(object sender, RoutedEventArgs e)
        {
            App.MainWindow.WindowState = WindowState.Minimized;
            ScreenCaptureWindow sc = new ScreenCaptureWindow(f);
            sc.Show();
            ExpectedImageTextBox.Text = sc.GetPathToExpectedImage();
            //ScreenAreaCoordinatesTextBox.Text = sc.GetCordinates(); TODO: Need to check to get the coordinates 
        }
    }
}
