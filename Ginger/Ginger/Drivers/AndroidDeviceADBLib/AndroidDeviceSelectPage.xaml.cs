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
using System.Drawing;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using GingerCore;
using GingerCore.Drivers.AndroidADB;
using GingerCore.Drivers.Common;
using SharpAdbClient;

namespace Ginger.Drivers.AndroidDeviceADBLib
{
    /// <summary>
    /// Interaction logic for AndroidDeviceSelectPage.xaml
    /// </summary>
    public partial class AndroidDeviceSelectPage : Page
    {
        GenericWindow _GenericWin = null;
        Agent mAgent;
        DeviceViewPage mDeviceViewPage;

        public AndroidDeviceSelectPage(Agent Agent)
        {
            InitializeComponent();
            mAgent = Agent;
            InitDeviceView();
        }

        private void InitDeviceView()
        {
            string DeviceConfigFolder = System.IO.Path.Combine(App.UserProfile.Solution.Folder, @"Documents\Devices\nexus_4\");
            mDeviceViewPage = new DeviceViewPage(DeviceConfigFolder);
            DeviceViewFrame.Content = mDeviceViewPage;
            RefreshConnectedDevices();
        }

        private void GetConnectedDevicesButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshConnectedDevices();
        }

        private void RefreshConnectedDevices()
        {
            AdbServer srv = AndroidADBDriver.GetADBServer();
            var devices = AdbClient.Instance.GetDevices();
            DevicesGrid.ItemsSource = devices;
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            ObservableList<Button> buttons = new ObservableList<Button>();
            Button OKButton = new Button();
            OKButton.Content = "OK";
            OKButton.Click += OKButton_Click;
            buttons.Add(OKButton);
            GingerCore.General.LoadGenericWindow(ref _GenericWin, App.MainWindow, windowStyle, "Select Android Device", this, buttons, true, "Cancel");
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DeviceData DD = (DeviceData)DevicesGrid.SelectedItem;

            if (DD != null)
            {
                mAgent.GetOrCreateParam("Model").Value = DD.Model;
                mAgent.GetOrCreateParam("Serial").Value = DD.Serial;
            }
            _GenericWin.Close();
        }

        private void ConnectRemoteADBButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void ConnectOverIPButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void DevicesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeviceData DD = (DeviceData)DevicesGrid.SelectedItem;
            if (DD != null)
            {
                //TODO: start on task so user don't wait
                var img = AdbClient.Instance.GetFrameBufferAsync(DD, CancellationToken.None);
                img.Wait(5000);

                mDeviceViewPage.UpdateDeviceScreenShot((Bitmap)img.Result);
            }
        }
    }
}