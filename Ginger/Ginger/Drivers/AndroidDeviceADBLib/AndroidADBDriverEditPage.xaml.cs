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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Ginger.Drivers.AndroidDeviceADBLib;
using Ginger.Drivers.Common;
using GingerCore;
using GingerCore.Drivers.AndroidADB;
using GingerCore.Drivers.Common;
using SharpAdbClient;

namespace Ginger.Drivers
{
    /// <summary>
    /// Interaction logic for SeleniumDriverEditPage.xaml
    /// </summary>
    public partial class AndroidADBDriverEditPage : Page
    {
        private GingerCore.Agent mAgent;

        public AndroidADBDriverEditPage(GingerCore.Agent mAgent)
        {
            InitializeComponent();
            
            this.mAgent = mAgent;


            if (mAgent.DriverConfiguration == null) mAgent.DriverConfiguration = new ObservableList<DriverConfigParam>();

            DriverConfigParam ModelDCP = mAgent.GetOrCreateParam("Model" , "");
            App.ObjFieldBinding(DeviceModelTextBox, TextBox.TextProperty, ModelDCP, "Value");

            DriverConfigParam SerialDCP = mAgent.GetOrCreateParam("Serial" , "");
            App.ObjFieldBinding(DeviceSerialTextBox, TextBox.TextProperty, SerialDCP, "Value");

            DriverConfigParam LaunchEmulatorCommand = mAgent.GetOrCreateParam("LaunchEmulatorCommand", "");
            App.ObjFieldBinding(LaunchEmulatorCommandTextBox, TextBox.TextProperty, LaunchEmulatorCommand, "Value");

            DriverConfigParam DeviceConfigFolder = mAgent.GetOrCreateParam("DeviceConfigFolder", "");
            App.ObjFieldBinding(DeviceConfigFolderTextBox, TextBox.TextProperty, DeviceConfigFolder, "Value");

            UpdateDeviceViewPage();
        }

        private void SelectDeviceButton_Click(object sender, RoutedEventArgs e)
        {
            AndroidDeviceSelectPage p = new AndroidDeviceSelectPage(mAgent);
            p.ShowAsWindow();

            //???
            //TODO: fix me , from some reason the bind above didn't work so after the window is closed we bind again.. temp solution
            DriverConfigParam ModelDCP = mAgent.GetOrCreateParam("Model", "");
            App.ObjFieldBinding(DeviceModelTextBox, TextBox.TextProperty, ModelDCP, "Value");

            DriverConfigParam SerialDCP = mAgent.GetOrCreateParam("Serial", "");
            App.ObjFieldBinding(DeviceSerialTextBox, TextBox.TextProperty, SerialDCP, "Value");
        }

        private void StartEmulatorButton_Click(object sender, RoutedEventArgs e)
        {
            string args = "-avd " + EmulatorsComboBox.SelectedValue.ToString() + " " + LaunchEmulatorCommandTextBox.Text;
            string EmulatorEXE = GetAndroidHome() + @"\tools\emulator.exe";
            Process prcsCMD = new Process();
            prcsCMD.StartInfo.FileName = EmulatorEXE;   
            prcsCMD.StartInfo.UseShellExecute = true;
            prcsCMD.StartInfo.Arguments = args;
            prcsCMD.StartInfo.RedirectStandardOutput = false;
            prcsCMD.Start();
        }

        string GetAndroidHome()
        {
            return Environment.GetEnvironmentVariable("ANDROID_HOME");
        }

        public string ExecuteCommandSync(string command)
        {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo =
                new System.Diagnostics.ProcessStartInfo("cmd", "/c \"" + command +"\"");

                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                // Get the output into a string
                string result = proc.StandardOutput.ReadToEnd();

                string ExInfo = result;
                return ExInfo;
        }

        private void AVDManagerButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: run the command without wait for out put just shell

            string cmd = GetAndroidHome() +  @"\AVD Manager.exe";
            Process prcsCMD = new Process();
            prcsCMD.StartInfo.FileName = cmd;
            prcsCMD.StartInfo.UseShellExecute = true;
            prcsCMD.StartInfo.RedirectStandardOutput = false;
            prcsCMD.Start();
        }

        private void EmulatorsComboBox_DropDownOpened(object sender, EventArgs e)
        {
            //do it only once
            if (EmulatorsComboBox.ItemsSource != null) return;
            
            string cmd = GetAndroidHome() + @"\tools\emulator.exe -list-avds";
            List<string> list = new List<string>();

            string lst = ExecuteCommandSync(cmd);
            string[] sep = new string[] { "\r\n" };
            string[] a = lst.Split(sep, StringSplitOptions.None);
            foreach (string s in a)
            {
                list.Add(s);
            }
            EmulatorsComboBox.ItemsSource = list;
        }

        private void RefreshAVDList_Click(object sender, RoutedEventArgs e)
        {
            EmulatorsComboBox.ItemsSource = null;
        }

        private void DeviceEditorButton_Click(object sender, RoutedEventArgs e)
        {
            DeviceEditorPage ADCP = new DeviceEditorPage();
            ADCP.ShowAsWindow();
        }

        private void DeviceConfigFileCombo_DropDownOpened(object sender, EventArgs e)
        {
            string DevicesFolder = System.IO.Path.Combine(App.UserProfile.Solution.Folder, @"\Documents\Devices\");

            DeviceConfigFolderComboBox.Items.Clear();            


            //TODO: get only Android Devices
            string[] devices =  Directory.GetDirectories(DevicesFolder);
            foreach (string s in devices)
            {
                string FolderName = System.IO.Path.GetFileName(s);
                DeviceConfigFolderComboBox.Items.Add(FolderName);
            }
        }
        
        private void DeviceConfigFileComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DeviceConfigFolderTextBox.Text = DeviceConfigFolderComboBox.SelectedValue + "";
            UpdateDeviceViewPage();
        }

        private void UpdateDeviceViewPage()
        {
            string DeviceFolder = DeviceConfigFolderTextBox.Text;
            if (!string.IsNullOrEmpty(DeviceFolder))
            {
                DeviceViewPage DVP = new DeviceViewPage(System.IO.Path.Combine(App.UserProfile.Solution.Folder, @"Documents\Devices", DeviceFolder + @"\"));
                DeviceFrame.Content = DVP;
            }
            else
            {
                DeviceFrame.Content = null;
            }
        }

        void InstallGingerAPK()
        {
            List<DeviceData> list = AdbClient.Instance.GetDevices();           
            DeviceData AndroidDevice = (from x in list where x.Serial == DeviceSerialTextBox.Text select x).FirstOrDefault();

            // push the apk

            //TODO: removed hard coded path, use ginger folder
            //  getting permsission denided - fix me

            string GingerAPKPath = GingerCore.General.GetGingerEXEPath() + @"Drivers\AndroidADB\Java\GingerService\app\build\outputs\apk\";

            //TODO: add in the driver config if to reinstall app
            // Below is if we want to install test sample app - for unit test or play around

            string APKFile2 = GingerAPKPath + "com.amdocs.ginger.android-debug-androidTest.apk";
            if (!File.Exists(APKFile2))
            {
                throw new Exception("Ginger apk file not found at: " + APKFile2);
            }
            InstallApplication(AndroidDevice.Serial, APKFile2, true);  // reinstall = true
        }

        void InstallApplication(string DeviceSerial, string APKFileName, bool reinstall = false)
        {
            //FIXME not working
            string p = "-s " + DeviceSerial + " install -r " + APKFileName;
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = AndroidADBDriver.GetADBFileName(),
                    Arguments = p
                }
            };
            process.Start();
            process.WaitForExit();
        }

        private void InstallGingerAPKButton_Click(object sender, RoutedEventArgs e)
        {
            InstallGingerAPK();
        }
    }
}
