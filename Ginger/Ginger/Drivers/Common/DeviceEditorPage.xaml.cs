#region License
/*
Copyright © 2014-2018 European Support Limited

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
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using GingerCore.Drivers.Common;
using GingerCore.Drivers.Common.Devices;

namespace Ginger.Drivers.Common
{
    /// <summary>
    /// Interaction logic for AndroidDeviceConfigPage.xaml
    /// </summary>
    public partial class DeviceEditorPage : Page
    {
        GenericWindow _GenericWin = null;

        DeviceConfig mDeviceConfig;

        DeviceViewPage mDeviceViewPage;

        string mDeviceConfigFolder;

        public DeviceEditorPage()
        {
            InitializeComponent();

            DoBindings();

            InitDeviceList();
        }

        private void DoBindings()
        {
            App.ObjFieldBinding(NameTextBox, TextBox.TextProperty, mDeviceConfig, DeviceConfig.Fields.Name);
            App.ObjFieldBinding(DeviceNameTextBox, TextBox.TextProperty, mDeviceConfig, DeviceConfig.Fields.DeviceName);
            App.ObjFieldBinding(DeviceImageTextBox, TextBox.TextProperty, mDeviceConfig, DeviceConfig.Fields.DeviceImage);

            App.ObjFieldBinding(DeviceScreenLeftTextBox, TextBox.TextProperty, mDeviceConfig, DeviceConfig.Fields.DeviceImageScreenLeft );
            App.ObjFieldBinding(DeviceScreenTopTextBox, TextBox.TextProperty, mDeviceConfig, DeviceConfig.Fields.DeviceImageScreenTop);
            App.ObjFieldBinding(DeviceScreenRightTextBox, TextBox.TextProperty, mDeviceConfig, DeviceConfig.Fields.DeviceImageScreenRight);
            App.ObjFieldBinding(DeviceScreenBottomTextBox, TextBox.TextProperty, mDeviceConfig, DeviceConfig.Fields.DeviceImageScreenBottom);
            
        }

        private void InitDeviceList()
        {
            string DevicesFolder = System.IO.Path.Combine(App.UserProfile.Solution.Folder, @"Documents\Devices\");
            string[] devices = Directory.GetDirectories(DevicesFolder);
            foreach (string s in devices)
            {
                string FolderName = System.IO.Path.GetFileName(s);
                DeviceListBox.Items.Add(FolderName);
            }
        }


        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            mDeviceConfig.Save(mDeviceConfigFolder);
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            ObservableList<Button> buttons = new ObservableList<Button>();

            Button OKButton = new Button();
            OKButton.Content = "OK";
            OKButton.Click += OKButton_Click;

            buttons.Add(OKButton);

            GingerCore.General.LoadGenericWindow(ref _GenericWin, App.MainWindow, windowStyle, "Android Device Config", this, buttons, true, "Cancel");
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void DeviceListBox_SelectionChanged(object sender, RoutedEventArgs e)
        {

            mDeviceConfigFolder = System.IO.Path.Combine(App.UserProfile.Solution.Folder, @"Documents\Devices", DeviceListBox.SelectedValue + @"\");

            mDeviceViewPage = new DeviceViewPage(mDeviceConfigFolder);
            mDeviceConfig = mDeviceViewPage.AndroidDeviceConfig;
            DeviceViewFrame.Content = mDeviceViewPage;

            DoBindings();
        }

        private void DeviceScreenLeftTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (mDeviceViewPage == null) return;
            mDeviceViewPage.RedrawDevice();
        }

        private void AddControllerButton_Click(object sender, RoutedEventArgs e)
        {
            if (mDeviceConfig.DeviceControllers == null) mDeviceConfig.DeviceControllers = new List<DeviceControllerConfig>();
            mDeviceConfig.DeviceControllers.Add(new DeviceControllerConfig());
        }

    }
}
