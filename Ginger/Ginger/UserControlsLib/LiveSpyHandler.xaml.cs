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
using Amdocs.Ginger.Common.UIElement;
using GingerCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Ginger.UserControlsLib
{
    /// <summary>
    /// Interaction logic for LiveSpyHandler.xaml
    /// </summary>
    public partial class LiveSpyHandler : UserControl
    {
        public Agent DriverAgent { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private ElementInfo mSpySelectedElement = null;

        public ElementInfo SpySelectedElement
        {
            get
            {
                return mSpySelectedElement;
            }
            set
            {
                mSpySelectedElement = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(SpySelectedElement)));
                }
            }
        }


        System.Windows.Threading.DispatcherTimer mDispatcherTimer = null;
        public IWindowExplorer mWinExplorer
        {
            get
            {
                if (DriverAgent != null && ((AgentOperations)DriverAgent.AgentOperations).Status == Agent.eStatus.Running)
                {
                    return ((AgentOperations)DriverAgent.AgentOperations).Driver as IWindowExplorer;
                }
                else
                {
                    if (DriverAgent != null)
                    {
                        DriverAgent.AgentOperations.Close();
                    }
                    return null;
                }
            }
        }

        public LiveSpyHandler()
        {
            InitializeComponent();
        }

        private void LiveSpyButton_Click(object sender, RoutedEventArgs e)
        {
            if (mWinExplorer == null)
            {
                Reporter.ToUser(eUserMsgKey.POMAgentIsNotRunning);
                xLiveSpyButton.IsChecked = false;
                return;
            }

            if (((AgentOperations)DriverAgent.AgentOperations).Driver.IsDriverBusy)
            {
                Reporter.ToUser(eUserMsgKey.POMDriverIsBusy);
                xLiveSpyButton.IsChecked = false;
                return;
            }

            if (xLiveSpyButton.IsChecked == true)
            {
                mWinExplorer.StartSpying();
                xStatusLable.Content = "Spying is On";
                if (mDispatcherTimer == null)
                {
                    mDispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                    mDispatcherTimer.Tick += GetElementInfoOnKeyPress;
                    mDispatcherTimer.Interval = new TimeSpan(0, 0, 1);
                }

                mDispatcherTimer.IsEnabled = true;
            }
            else
            {
                xStatusLable.Content = "Spying is Off";
                mDispatcherTimer.IsEnabled = false;
            }
        }

        private void GetElementInfoOnKeyPress(object sender, EventArgs e)
        {
            // Get control info only if control key is pressed
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                SetLableStatusText("Spying element, please wait...");

                GingerCore.General.DoEvents();
                SpySelectedElement = mWinExplorer.GetControlFromMousePosition();
                if (SpySelectedElement != null)
                {

                    SpySelectedElement.WindowExplorer = mWinExplorer;
                    SpySelectedElement.IsAutoLearned = true;
                    mWinExplorer.HighLightElement(SpySelectedElement);
                }
                else
                {
                    SetLableStatusText("Failed to spy element.");
                    GingerCore.General.DoEvents();
                }
            }
        }


        public void SetLableStatusText(string labelText)
        {
            xStatusLable.Content = labelText;
            GingerCore.General.DoEvents();
        }
    }
}
