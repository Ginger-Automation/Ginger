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


using Amdocs.Ginger.CoreNET.Drivers.CommunicationProtocol;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPFDriverWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        RemoteObjectsServer mRemoteObjectsServer;        
        
        public MainWindow()
        {
            InitializeComponent();
            StartServer();            
        }

        public void Adddisplay(Page driverPage)
        {
            MainFrame.Content = driverPage;
        }
        
        private void StartServer()
        {
            mRemoteObjectsServer = new RemoteObjectsServer();
            mRemoteObjectsServer.Start(15111);   // TODO: get free port
            mRemoteObjectsServer.GetObjectHandler = GetObjectHandler;
            this.Title = mRemoteObjectsServer.Info;
        }

        private object GetObjectHandler(string id)
        {
            Page driverPage = null;
            this.Dispatcher.Invoke(() => {
                //TODO: temp fix me, based on id create the obj
                Assembly a = Assembly.LoadFrom(@"C:\Yaron\TFS\Ginger\Devs\GingerNextVer_Dev\GingerWebServicesPluginWPF\bin\Debug\GingerWebServicesPluginWPF.dll"); 
                driverPage = (Page)a.CreateInstance("GingerWebServicesPluginWPF.WebServicesDriverPage");
                Adddisplay(driverPage);
                
            });
            return driverPage;
        }

        private void RecordButton_Click(object sender, RoutedEventArgs e)
        {
        }
    }
}
