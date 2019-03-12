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

using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace GingerCore.Drivers.CommunicationProtocol
{
    /// <summary>
    /// Interaction logic for GingerSocketMonitorWindow.xaml
    /// </summary>
    public partial class GingerSocketMonitorWindow : Window
    {
        GingerSocketClient mGingerSocketClient;
        public GingerSocketMonitorWindow(GingerSocketClient GS)
        {
            InitializeComponent();

            MessageLabel.Visibility = Visibility.Collapsed;

            mGingerSocketClient = GS;
            MainGrid.ItemsSource = GS.GingerSocketLogs;
            GS.GingerSocketLogs.CollectionChanged += GingerSocketLogs_CollectionChanged;
        }

        private void GingerSocketLogs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //Enable the screen to refresh
            General.DoEvents();            
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            PayLoad PL = new PayLoad(PayLoadNameTextBox.Text);
            PL.ClosePackage();
            mGingerSocketClient.SendPayLoad(PL);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            mGingerSocketClient.GingerSocketLogs.Clear();
        }

        private void ResendButton_Click(object sender, RoutedEventArgs e)
        {
            GingerSocketLog GSL = (GingerSocketLog)MainGrid.SelectedItem;            
                        
            mGingerSocketClient.SendPayLoad(GSL.PayLoad);
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
                General.DoEvents();
                ShowMessage("Window is closing in " + (int)((3000 - st.ElapsedMilliseconds) / 1000) + " Seconds");
                Thread.Sleep(100);
            }
            this.Close();
        }
    }
}
