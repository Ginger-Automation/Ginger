#region License
/*
Copyright © 2014-2025 European Support Limited

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
using GingerCore.Drivers.Common;
using GingerCore.Drivers.Common.Devices;
using GingerCore.GeneralLib;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

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
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(NameTextBox, TextBox.TextProperty, mDeviceConfig, DeviceConfig.Fields.Name);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DeviceNameTextBox, TextBox.TextProperty, mDeviceConfig, DeviceConfig.Fields.DeviceName);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DeviceImageTextBox, TextBox.TextProperty, mDeviceConfig, DeviceConfig.Fields.DeviceImage);

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DeviceScreenLeftTextBox, TextBox.TextProperty, mDeviceConfig, DeviceConfig.Fields.DeviceImageScreenLeft);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DeviceScreenTopTextBox, TextBox.TextProperty, mDeviceConfig, DeviceConfig.Fields.DeviceImageScreenTop);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DeviceScreenRightTextBox, TextBox.TextProperty, mDeviceConfig, DeviceConfig.Fields.DeviceImageScreenRight);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DeviceScreenBottomTextBox, TextBox.TextProperty, mDeviceConfig, DeviceConfig.Fields.DeviceImageScreenBottom);

        }

        private void InitDeviceList()
        {
            string DevicesFolder = System.IO.Path.Combine(WorkSpace.Instance.Solution.Folder, @"Documents\Devices\");
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
            ObservableList<Button> buttons = [];

            Button OKButton = new Button
            {
                Content = "OK"
            };
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

            mDeviceConfigFolder = System.IO.Path.Combine(WorkSpace.Instance.Solution.Folder, @"Documents\Devices", DeviceListBox.SelectedValue + @"\");

            mDeviceViewPage = new DeviceViewPage(mDeviceConfigFolder);
            mDeviceConfig = mDeviceViewPage.AndroidDeviceConfig;
            DeviceViewFrame.ClearAndSetContent(mDeviceViewPage);

            DoBindings();
        }

        private void DeviceScreenLeftTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (mDeviceViewPage == null)
            {
                return;
            }

            mDeviceViewPage.RedrawDevice();
        }

        private void AddControllerButton_Click(object sender, RoutedEventArgs e)
        {
            if (mDeviceConfig.DeviceControllers == null)
            {
                mDeviceConfig.DeviceControllers = [];
            }

            mDeviceConfig.DeviceControllers.Add(new DeviceControllerConfig());
        }

    }
}
