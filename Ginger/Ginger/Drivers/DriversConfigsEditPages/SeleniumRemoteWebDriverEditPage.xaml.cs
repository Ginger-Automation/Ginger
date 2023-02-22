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

using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using GingerCore;
using GingerCore.Drivers;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Diagnostics;

namespace Ginger.Drivers.DriversConfigsEditPages
{
    /// <summary>
    /// Interaction logic for SeleniumDriverEditPage.xaml
    /// </summary>
    public partial class SeleniumRemoteWebDriverEditPage : Page
    {
        public SeleniumRemoteWebDriverEditPage(GingerCore.Agent mAgent)
        {
            InitializeComponent();

            if (mAgent.DriverConfiguration == null) mAgent.DriverConfiguration = new ObservableList<DriverConfigParam>();

            DriverConfigParam GridHostDCP = mAgent.GetOrCreateParam(SeleniumDriver.RemoteGridHubParam, "http://127.0.0.1:4444");            
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(GridHostTextBox, TextBox.TextProperty, GridHostDCP, "Value");

            DriverConfigParam BrowserNameDCP = mAgent.GetOrCreateParam(SeleniumDriver.RemoteBrowserNameParam , "firefox");
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(BrowserNameComboBox, ComboBox.SelectedValueProperty, BrowserNameDCP, "Value");

            DriverConfigParam PlatformDCP = mAgent.GetOrCreateParam(SeleniumDriver.RemotePlatformParam);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(PlatformComboBox, ComboBox.SelectedValueProperty, PlatformDCP, "Value");

            DriverConfigParam VersionDCP = mAgent.GetOrCreateParam(SeleniumDriver.RemoteVersionParam);
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(VersionTextBox, TextBox.TextProperty, VersionDCP, "Value");
        }

        //Loading Browser Combobox Data
        private void BrowserComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> data = new List<string>();
            data.Add("chrome");
            data.Add("firefox");
            data.Add("internet explorer");
            data.Add("safari");
            data.Add("MicrosoftEdge");

            var comboBox = sender as ComboBox;
            comboBox.ItemsSource = data;
        }

        //Loading Platform Combobox Data
        private void PlatformComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> data = new List<string>();
            data.Add("WINDOWS");
            data.Add("XP");
            data.Add("VISTA");
            data.Add("MAC");

            var comboBox = sender as ComboBox;
            comboBox.ItemsSource = data;
        }

        private void ShowconsoleButton_Click(object sender, RoutedEventArgs e)
        {
            browser.Navigate(GridHostTextBox.Text + "/grid/console");
        }

        private void GetHubFilesButton_Click(object sender, RoutedEventArgs e)
        {
            //Getting the Ginger execution path
            string? assemblyLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string sourcePath = Path.Combine(assemblyLocation, "StaticDrivers");
            string targetPath = userFolder + "\\Temp\\GingerTemp";
            
            string jarFileName = "selenium-server-standalone.jar";

            string sourceFile = System.IO.Path.Combine(sourcePath, jarFileName);
            string destFile = System.IO.Path.Combine(targetPath, jarFileName);
            //Create the folder
            if (!System.IO.Directory.Exists(targetPath))
            {
                System.IO.Directory.CreateDirectory(targetPath);               
            }
            //Copy the jar file
            if (!System.IO.File.Exists(destFile))
            {
             System.IO.File.Copy(sourceFile, destFile);
            }

            string batHubFileName = targetPath + "\\starthub.bat";

            string hostPort = GridHostTextBox.Text;
            int hostPortLeanth = hostPort.Length;
            string port = hostPort.Substring(hostPortLeanth - 4);

            //Creating the bat file
            if (!File.Exists(batHubFileName))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(batHubFileName))
                {
                    sw.WriteLine($"java -jar {jarFileName} -role hub -port " + port);
                }
            }

            //Creating the Readme file
            string txtFileName = targetPath.Replace("GingerTemp", "GingerTemp\\ReadMe.txt");

            if (!File.Exists(txtFileName))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(txtFileName))
                {
                    sw.WriteLine("Welcome to Ginger remote agent. \r\nplease follow the below steps: \r\n1.Extract the starthub.zip to your local machine. \r\n2.Run starthub.bat and leave the window open for the run. \r\n3.Get the node zip from the Ginger and refer to the startnode.zip Readme for Node configuration. \r\nGood Luck! :)");
                }
            }
            
            //Creating the Zip file
            string targetZipPath = targetPath.Replace("GingerTemp","Ginger");
            string zipFileName = targetZipPath + "\\Hub Installation Package.zip";

            if (!System.IO.Directory.Exists(targetZipPath))
            {
                System.IO.Directory.CreateDirectory(targetZipPath);
            }


            if (!System.IO.File.Exists(zipFileName))
            {
                ZipFile.CreateFromDirectory(targetPath, zipFileName);
            }
            else
            {
                System.IO.File.Delete(zipFileName);
                ZipFile.CreateFromDirectory(targetPath, zipFileName);
            }

            System.IO.Directory.Delete(targetPath,true);

            //Open Dialog folder
            Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = targetZipPath, UseShellExecute = true });           
        }

        private void GetNodeFilesButton_Click(object sender, RoutedEventArgs e)
        {
            //Getting the Ginger execution path
            string? assemblyLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string userFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string sourcePath = Path.Combine(assemblyLocation, "StaticDrivers");
            string targetPath = userFolder + "\\Temp\\GingerTemp";
            string jarFileName = "selenium-server-standalone.jar";
            string sourceFile = System.IO.Path.Combine(sourcePath, jarFileName);
            string destFile = System.IO.Path.Combine(targetPath, jarFileName);

            //Create the folder
            if (!System.IO.Directory.Exists(targetPath))
            {
                System.IO.Directory.CreateDirectory(targetPath);
            }

            //Copy the jar file
            if (!System.IO.File.Exists(destFile))
            {
                System.IO.File.Copy(sourceFile, destFile);
            }

            //taking the drivers to the temp folder
            string driversSourcePath = assemblyLocation;
            string chromeDriverFile = "chromedriver.exe";
            string fireFoxDriverFile = "geckodriver.exe";
            string IEDriverFile = "IEDriverServer.exe";
            string EdgeDriverFile = "msedgedriver.exe";

            string chromeSourceFile = System.IO.Path.Combine(assemblyLocation, chromeDriverFile);
            string chromeDestFile = System.IO.Path.Combine(targetPath, chromeDriverFile);

            if (!System.IO.File.Exists(chromeDestFile))
            {
                System.IO.File.Copy(chromeSourceFile, chromeDestFile);
            }

            string fireFoxSourceFile = System.IO.Path.Combine(driversSourcePath, fireFoxDriverFile);
            string fireFoxDestFile = System.IO.Path.Combine(targetPath, fireFoxDriverFile);

            if (!System.IO.File.Exists(fireFoxDestFile))
            {
                System.IO.File.Copy(fireFoxSourceFile, fireFoxDestFile);
            }

            string IESourceFile = System.IO.Path.Combine(driversSourcePath, IEDriverFile);
            string IEDestFile = System.IO.Path.Combine(targetPath, IEDriverFile);

            if (!System.IO.File.Exists(IEDestFile))
            {
                System.IO.File.Copy(IESourceFile, IEDestFile);
            }

            string edgeSourceFile = System.IO.Path.Combine(driversSourcePath, EdgeDriverFile);
            string edgeDestFile = System.IO.Path.Combine(targetPath, EdgeDriverFile);

            if (!System.IO.File.Exists(edgeDestFile))
            {
                System.IO.File.Copy(EdgeDriverFile, edgeDestFile);
            }

            //creating startnode.bat file
            string batNodeFileName = targetPath + "\\startnode.bat";

            string hostIpAndPort = GridHostTextBox.Text;
            int hostPortLeanth = hostIpAndPort.Length;
            string port = hostIpAndPort.Substring(hostPortLeanth - 4);

            //Getting the IP from the field
            string localp = GetIP();

            //Generatin Random Port
            string randomPort = GeneratePortNo();

            string PlatformName = PlatformComboBox.Text;
            string browsername = BrowserNameComboBox.Text;

            //Creating the bat file
            if (!File.Exists(batNodeFileName))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(batNodeFileName))
                {

                    if ((bool)AllBroesers.IsChecked)
                        if (PlatformName.Equals("MAC"))
                        {
                            sw.WriteLine($"java -Dwebdriver.edge.driver=msedgedriver.exe -Dwebdriver.ie.driver=IEDriverServer.exe -Dwebdriver.chrome.driver=chromedriver.exe -Dwebdriver.gecko.driver=geckodriver.exe -jar {jarFileName} -port " + randomPort + " -role node  -hub " + "http://" + localp + ":" + port + "/grid/register -browser browserName=safari -browser browserName=safari -browser browserName=safari -browser browserName=safari -browser browserName=safari");
                        }
                        else
                        {
                            sw.WriteLine($"java -Dwebdriver.edge.driver=msedgedriver.exe -Dwebdriver.ie.driver=IEDriverServer.exe -Dwebdriver.chrome.driver=chromedriver.exe -Dwebdriver.gecko.driver=geckodriver.exe -jar {jarFileName} -port " + randomPort + " -role node  -hub " + "http://" + localp + ":" + port + "/grid/register -browser browserName=chrome -browser browserName=chrome -browser browserName=chrome -browser browserName=chrome -browser browserName=chrome -browser browserName=firefox -browser browserName=firefox -browser browserName=firefox -browser browserName=firefox -browser browserName=firefox -browser " + ((char)34) + "browserName=internet explorer" + ((char)34) + " -browser " + ((char)34) + "browserName=internet explorer" + ((char)34) + " -browser " + ((char)34) + "browserName=internet explorer" + ((char)34) + " -browser " + ((char)34) + "browserName=internet explorer" + ((char)34) + " -browser " + ((char)34) + "browserName=internet explorer" + ((char)34) + " -browser browserName=MicrosoftEdge -browser browserName=MicrosoftEdge -browser browserName=MicrosoftEdge -browser browserName=MicrosoftEdge -browser browserName=MicrosoftEdge");
                        }
                    else
                        sw.WriteLine($"java -Dwebdriver.edge.driver=msedgedriver.exe -Dwebdriver.ie.driver=IEDriverServer.exe -Dwebdriver.chrome.driver=chromedriver.exe -Dwebdriver.gecko.driver=geckodriver.exe -jar {jarFileName} -port " + randomPort + " -role node  -hub " + "http://" + localp + ":" + port + "/grid/register -browser " + ((char)34) + "browserName=" + browsername + ((char)34) + " -browser " + ((char)34) + "browserName=" + browsername + ((char)34) + " -browser " + ((char)34) + "browserName=" + browsername + ((char)34) + " -browser " + ((char)34) + "browserName=" + browsername + ((char)34) + " -browser " + ((char)34) + "browserName=" + browsername + ((char)34));
                }
            }

            string txtFileName = targetPath.Replace("GingerTemp", "GingerTemp\\ReadMe.txt");
            //Creating the Readme fole
            if (!File.Exists(txtFileName))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(txtFileName))
                {
                    sw.WriteLine("Welcome to Ginger remote agent. \r\nplease follow the below steps: \r\n1.Extract the startnode.zip to your machine. \r\n2.Edit startnode.bat file and make sure your node port and hub IP and Port are correct and not conflicted with another existing port \r\n3.Run startnode.bat and leave the window open for the run.\r\n4.for Safari: \r\n  a.Download Safari Driver from http://www.seleniumhq.org/download/ and open it with your Safari \r\n  b.Go to Safari-->Preferences-->check the Show Develop menu checkbox \r\n  c.Go to Develop and click on Allow Remote Automation. \r\nGood Luck! :)");
                }
            }

            string targetZipPath = targetPath.Replace("GingerTemp", "Ginger");
            string zipFileName = targetZipPath + "\\Node Installation Package.zip";

            if (!System.IO.Directory.Exists(targetZipPath))
            {
                System.IO.Directory.CreateDirectory(targetZipPath);
            }


            if (!System.IO.File.Exists(zipFileName))
            {
                ZipFile.CreateFromDirectory(targetPath, zipFileName);
            }
            else
            {
                System.IO.File.Delete(zipFileName);
                ZipFile.CreateFromDirectory(targetPath, zipFileName);
            }
           
            System.IO.Directory.Delete(targetPath, true);

            //Open Dialog folder
            Process.Start(new ProcessStartInfo() { FileName = targetZipPath, UseShellExecute = true });
        }

        //If LocalHost selected then return the IPv4 local IP else return the selected IP
        private string GetIP()
        {

            if (GridHostTextBox.Text.Contains("127.0.0.1"))
            {
                string strHostName = "";
                strHostName = System.Net.Dns.GetHostName();

                IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);

                IPAddress[] addr = ipEntry.AddressList;

                return addr[addr.Length - 1].ToString();
            }
            else
            {

                int len = GridHostTextBox.Text.Length;
                return GridHostTextBox.Text.Substring(7, len - 12);
            }

        }

        //Generate a random port number for each node.
        public string GeneratePortNo()
        {
            int _min = 1000;
            int _max = 9999;
            Random _rdm = new Random();
            return _rdm.Next(_min, _max).ToString();
        }

        private void ActionTab_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ActionTab.SelectedItem == ConsoleTab)
            {
                browser.Navigate(GridHostTextBox.Text + "/grid/console");
            }
        }
    }
}
