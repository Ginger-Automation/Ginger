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

using Amdocs.Ginger.Common;
using GingerCore;
using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Ginger
{
    /// <summary>
    /// Interaction logic for BotDialog.xaml 
    /// </summary>
    public partial class GingerHelperWindow : Window
    {
        public DateTime LoadStartTime {get;set;}

        Timer t = null;
    
        public GingerHelperWindow()
        {
            t = new Timer();
            InitializeComponent();
            BoardBtn.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Left+ 5;
            this.Top = desktopWorkingArea.Bottom - this.Height - 5;

            //start timer
            LoadStartTime = DateTime.Now;
        }

        private void BtnClose_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void MsgActionBtn_Click(object sender, RoutedEventArgs e)
        {
            //will be provided as part of the message call
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            t.Close();
        }

        public void ProcessAnimationControl(bool runAnimation)
        {
            try
            {
                Storyboard SB = (Storyboard)FindResource("ProcessStoryboard");
                if (runAnimation == true)
                {
                    SB.Begin();
                }
                else
                {
                    SB.Stop();
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Failed to control the Ginger Helper animation", ex);
            }
        }

        internal async Task showHelper(StatusMsg messageToShow, RoutedEventHandler btnHandler = null)
        {
            t.Interval = 30000;
            t.AutoReset = true;
            t.Elapsed += T_Elapsed;
            t.Start();
            BoardHeader.Text = messageToShow.MsgHeader;
            BoardContent.Text = messageToShow.MsgContent;

            if (messageToShow.ShowBtn) //show button only if needed
            {
                BoardBtn.Visibility = Visibility.Visible;
                BoardBtn.Content = messageToShow.BtnContent;
                BoardBtn.Click += btnHandler;
            }
            if (messageToShow.MessageType == eStatusMsgType.PROCESS)//show animation
            {
                ProcessAnimationControl(true);
            }

            Show();
        }

        private void T_Elapsed(object sender, ElapsedEventArgs e)
        {
            t.Close();
            this.Dispatcher.Invoke(() =>
            {
                this.Close();
            });
        }
    }
}
