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

using System;
using System.Windows;

namespace Ginger
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        private bool InitDone = false;
        private int counter = 0;

        System.Windows.Threading.DispatcherTimer dispatcherTimer;

        public SplashWindow()
        {
            //load defualt dics to be used for splash screen till customized dics will be loaded from user profile 
            App.LoadApplicationDictionaries();

            InitializeComponent();

            //updating splash window
            //lblAppName.Content = "Amdocs Ginger Automation";// Ginger.App.AppName; TODO: pull name from App once ready
            //lblAppMoto.Content = Ginger.App.AppMoto;
            lblAppVersion.Content = "Version " + Ginger.App.AppVersion;
            //End           
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
            App.AppSplashWindow = this;
            App.InitApp();         
            InitDone = true;

            //CheckShowNews();            
        }

        private void CheckShowNews()
        {
            //Stopping the news until we do have news + instead of popup just show home page with news
            //Check show once a day only
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {           
            //switch (lblLoading.Content.ToString())
            //{
            //    case "Loading...":
            //        lblLoading.Content = "Loading   ";
            //        break;
            //    case "Loading   ":
            //        lblLoading.Content = "Loading.  ";
            //        break;
            //    case "Loading.  ":
            //        lblLoading.Content = "Loading.. ";
            //        break;
            //    case "Loading.. ":
            //        lblLoading.Content = "Loading...";
            //        break;
            //}
            counter++;


            if (InitDone && System.Diagnostics.Debugger.IsAttached)
            {
                // You are debugging so close splash and don't waste debuggig time!
                this.Close();
            }


            // Wait at lease 3 sec before closing even if app init completed
            if (InitDone && counter > 2)
            {
                dispatcherTimer.Stop();
                dispatcherTimer = null;
                this.Close();
            }
            
            //close after 5 sec - since there might be error hiding below
            //no need to close, the msg box will be shown on top of the splash screen
            //Simple mesgbox didn't close, so putting this code back...
            if (counter >= 5)
            {
                if (dispatcherTimer!=null)
                    dispatcherTimer.Stop();
                dispatcherTimer = null;
                this.Close();
            }
        }

        public void LoadingInfo(string txt)
        {
            lblLoadingInfo.Content = txt;            
        }
    }
}
