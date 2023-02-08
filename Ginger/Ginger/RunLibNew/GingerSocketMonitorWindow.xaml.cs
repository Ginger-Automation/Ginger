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
using Amdocs.Ginger.CoreNET.RunLib;
using GingerCoreNET.Drivers.CommunicationProtocol;
using GingerCoreNET.RunLib;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace Ginger.Drivers.CommunicationProtocol
{
    /// <summary>
    /// Interaction logic for GingerSocketMonitorWindow.xaml
    /// </summary>
    public partial class GingerSocketMonitorWindow : Window, IGingerNodeMonitor
    {
        GingerNodeProxy mGingerNodeProxy;
        public ObservableList<GingerSocketLog> GingerSocketLogs { get; set; }

        public GingerSocketMonitorWindow(GingerNodeProxy gingerNodeProxy)
        {
            InitializeComponent();
            MessageLabel.Visibility = Visibility.Collapsed;
            mGingerNodeProxy = gingerNodeProxy;            
        }

        private void StartRecording()
        {
            mGingerNodeProxy.StartRecordingSocketTraffic();                        
        }
      

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {             
            NewPayLoad PL = new NewPayLoad(PayLoadNameTextBox.Text);
            PL.ClosePackage();
            mGingerNodeProxy.SendRequestPayLoad(PL);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            GingerSocketLogs.Clear();
        }

        private void ResendButton_Click(object sender, RoutedEventArgs e)
        {
            GingerSocketLog GSL = (GingerSocketLog)MainListView.SelectedItem;
            mGingerNodeProxy.SendRequestPayLoad(GSL.PayLoad);
        }

        public void ShowMessage(string txt)
        {
            MessageLabel.Content = txt;
            MessageLabel.Visibility = Visibility.Visible;            
        }

        internal void DelayedClose()
        {
            Stopwatch st = new Stopwatch();
            st.Start();

            //Wait 3 sec before closing - so we can see what's going on if needed
            while (st.ElapsedMilliseconds < 3000)
            {
                ShowMessage("Window is closing in " + (int)((3000 - st.ElapsedMilliseconds) / 1000) + " Seconds");
                Thread.Sleep(100);
            }
            this.Close();
        }
        

        public void ShowMonitor(GingerNodeProxy gingerNodeProxy)
        {
            GingerSocketLogs = new ObservableList<GingerSocketLog>();
            MainListView.ItemsSource = GingerSocketLogs;            
            this.Show();
        }

        public void Add(GingerSocketLog gingerSocketLog)
        {
            Dispatcher.Invoke(() => {
                GingerSocketLogs.Add(gingerSocketLog);
            });
        }

        public void CloseMonitor()
        {
            DelayedClose();            
        }

        private void MainListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            GingerSocketLog GSL = (GingerSocketLog)MainListView.SelectedItem;
            PacketInfoTextBlock.Text = GSL.Info;
        }
    }
}
