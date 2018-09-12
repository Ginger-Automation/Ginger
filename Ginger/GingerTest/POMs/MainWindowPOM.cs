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

using Ginger.GeneralWindows;
using Ginger.MoveToGingerWPF;
using Ginger.TwoLevelMenuLib;
using GingerTest.POMs;
using GingerWPF.UserControlsLib;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;

namespace GingerWPFUnitTest.POMs
{
    public class MainWindowPOM : GingerPOMBase
    {
        // WorkSpaceEventHandler w = (WorkSpaceEventHandler)WorkSpace.Instance.EventHandler;
        Ginger.MainWindow mMainWindow;
        public EnvironmentsPOM Environments;
        public AgentsPOM Agents;

        public MainWindowPOM(Ginger.MainWindow mainWin)
        {
            mMainWindow = mainWin;
        }

        public void Close()
        {
            Dispatcher.Invoke(() =>
            {                
                mMainWindow.CloseWithoutAsking();
                Dispatcher.BeginInvokeShutdown(System.Windows.Threading.DispatcherPriority.Background);                
            });
            
        }

        public Button TestButton()
        {
            Button b = (Button)mMainWindow.FindName("TestButton");
            return b;
        }

        public void ClickTestButton()
        {
            mMainWindow.Dispatcher.Invoke(() => {
                Button b = TestButton();
                b.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
            });
        }

        public void ClickRunTab()
        {
            SelectRibbonTab("RunRibbon");
        }
        public void ClickAutomateTab()
        {
            SelectRibbonTab("AutomateRibbon");
        }

        public void ClickSolutionTab()
        {
            SelectRibbonTab("Solution AID");
        }

        internal void ClickResourcesRibbon()
        {
            SelectRibbonTab("Resources");
            WaitForPage(typeof(TwoLevelMenu));
        }

        internal void ClickConfigurationsRibbon()
        {
            SelectRibbonTab("Configurations");
            WaitForPage(typeof(TwoLevelMenuPage));
        }

        private void WaitForPage(Type p1)
        {
            SleepWithDoEvents(100);
            Frame f = (Frame)mMainWindow.FindName("MainFrame");
            int i = 0;
            while (true && i <100)
            {                
                Page p = (Page)f.Content;
                if (p.GetType().FullName == p1.FullName && p.IsVisible)
                {                    
                    return;
                }
                SleepWithDoEvents(100);
                i++;
            }
        }

        internal void ChangeSize(int width, int height)
        {
            Execute(() => {                
                mMainWindow.Width = width;
                mMainWindow.Height = height;
                mMainWindow.WindowState = WindowState.Minimized;
                SleepWithDoEvents(100);
                mMainWindow.WindowState = WindowState.Normal;
                //mMainWindow.UpdateLayout();
                SleepWithDoEvents(100);         
            });
            SleepWithDoEvents(500);

        }
        
        private void SelectRibbonTab(string automationID)
        {
            Execute(() => {
                Ribbon rc = (Ribbon)mMainWindow.FindName("MainRibbon");
                foreach (RibbonTab RT in rc.Items)
                {
                    if (AutomationProperties.GetAutomationId(RT) == automationID)
                    {
                        //mimic user click
                        //MouseEventArgs - not working...
                        //MouseDevice mouse = InputManager.Current.PrimaryMouseDevice;                        
                        //MouseButtonEventArgs arg = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left);
                        //arg.RoutedEvent = RibbonTab.PreviewMouseLeftButtonDownEvent;                        
                        //RT.RaiseEvent(arg);

                        // for now use direct change of the tab
                        rc.SelectedItem = RT;

                        SleepWithDoEvents(100);                        

                        //while (!rc.IsVisible)
                        //{
                        //    Thread.Sleep(100);
                        //}
                        return;
                    }
                }
                throw new Exception("SelectRibbonTab element not found by AutomationID: " + automationID);
            });
        }

        

        public EnvironmentsPOM GotoEnvironments()
        {
            Environments = null;
            Execute(() => {
                ClickResourcesRibbon();
                Frame f = (Frame)mMainWindow.FindName("MainFrame");
                TwoLevelMenuPage resourcesPage = (TwoLevelMenuPage)f.Content;

                ListView lv = (ListView)resourcesPage.FindName("xMainNavigationListView");                
                foreach (TopMenuItem topMenuItem in lv.Items)
                {
                    if (topMenuItem.AutomationID == "Environemnts_AID")
                    {
                        lv.SelectedItem = topMenuItem;
                        SleepWithDoEvents(100);
                        Frame f1 = (Frame)FindElementByName(resourcesPage, "xSelectedItemFrame");
                        SingleItemTreeViewExplorerPage itemExplorerPage = (SingleItemTreeViewExplorerPage)f1.Content;
                        while (!itemExplorerPage.IsVisible)
                        {
                            SleepWithDoEvents(100);
                        }
                        Environments = new EnvironmentsPOM(itemExplorerPage);
                        break;
                    }
                }
            });
            return Environments;
        }


        public List<string> GetVisibleRibbonTabs()
        {
            List<string> list = new List<string>();
            Execute(() => {
                Ribbon rc = (Ribbon)mMainWindow.FindName("MainRibbon");
                foreach (RibbonTab RT in rc.Items)
                {
                    if (RT.Visibility == Visibility.Visible)
                    {
                        list.Add(RT.Name);
                    }
                }                
            });
            return list;
        }


        internal AgentsPOM GotoAgents()
        {
            Agents = null;
            Execute(() => {
                ClickConfigurationsRibbon();
                Frame f = (Frame)mMainWindow.FindName("MainFrame");
                TwoLevelMenuPage configurationsPage = (TwoLevelMenuPage)f.Content;

                ListView lv = (ListView)configurationsPage.FindName("xMainNavigationListView");
                foreach (ListViewItem lvi in lv.Items)
                {
                    string AID = AutomationProperties.GetAutomationId(lvi);
                    if (AID == "Agents AID")
                    {
                        lv.SelectedItem = lvi;
                        SleepWithDoEvents(100);
                        Frame f1 = (Frame)FindElementByName(configurationsPage, "xSelectedItemFrame");
                        SingleItemTreeViewExplorerPage itemExplorerPage = (SingleItemTreeViewExplorerPage)f1.Content;
                        while (!itemExplorerPage.IsVisible)
                        {
                            SleepWithDoEvents(100);
                        }
                        Agents = new AgentsPOM(itemExplorerPage);                        
                    }
                }
            });

            if (Agents == null) throw new Exception("Cannot goto Agents");

            return Agents;

        }

    }
        
}
