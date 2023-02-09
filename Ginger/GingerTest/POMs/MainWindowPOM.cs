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

using Ginger.BusinessFlowPages;
using Ginger.BusinessFlowWindows;
using Ginger.GeneralWindows;
using Ginger.TwoLevelMenuLib;
using Ginger.Variables;
using GingerCore;
using GingerTest.POMs;
using GingerWPF.BusinessFlowsLib;
using GingerWPF.UserControlsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GingerWPFUnitTest.POMs
{
    public class MainWindowPOM : GingerPOMBase
    {
        // WorkSpaceEventHandler w = (WorkSpaceEventHandler)WorkSpace.Instance.EventHandler;
        Ginger.MainWindow mMainWindow;
       
        public EnvironmentsPOM Environments;
        public AgentsPOM Agents;
        public POMsPOM POMs;
        public GlobalVariablesPOM GlobalVariables;
        public BusinessFlowPOM businessFlow;
        public int ActivityCount;

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
        }
        public void ClickAutomateTab()
        {
        }

        public void ClickSolutionTab()
        {         
        }

        internal void ClickResourcesRibbon()
        {
            mMainWindow.Dispatcher.Invoke(() => {                
                ListView a = (ListView)mMainWindow.FindName("xSolutionTabsListView");
                a.SelectedItem = null; 
                ListViewItem b = (ListViewItem)mMainWindow.FindName("xResourcesListItem");
                b.RaiseEvent(new RoutedEventArgs(ListViewItem.SelectedEvent));
                WaitForPage(typeof(TwoLevelMenuPage));
            });

            
        }

        internal void ClickConfigurationsRibbon()
        {

            mMainWindow.Dispatcher.Invoke(() => {
                SleepWithDoEvents(1);                
                ListView lv = (ListView)mMainWindow.FindName("xSolutionTabsListView");
                ListViewItem b = (ListViewItem)mMainWindow.FindName("xConfigurationsListItem");
                lv.SelectedItem = b;
                WaitForPage(typeof(TwoLevelMenuPage));
            });
        }

        internal void ClickBusinessFlowRibbon()
        {
            mMainWindow.Dispatcher.Invoke(() => {
                SleepWithDoEvents(1);
                ListView lv = (ListView)mMainWindow.FindName("xSolutionTabsListView");
                ListViewItem b = (ListViewItem)mMainWindow.FindName("xBusinessFlowsListItem");
                lv.SelectedItem = b;
                WaitForPage(typeof(TwoLevelMenuPage));
            });
        }

        private void WaitForPage(Type p1)
        {
            SleepWithDoEvents(1);
            Frame f = (Frame)mMainWindow.FindName("xMainWindowFrame");
            int i = 0;
            while (true && i <100)
            {                
                Page p = (Page)f.Content;
                if (p.GetType().FullName == p1.FullName && p.IsVisible)
                {
                    SleepWithDoEvents(1);
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

        //private void SelectMenu(string automationID)
        //{
        //    Execute(() => {
        //        Menu rc = (Menu)mMainWindow.FindName("MainRibbon");
        //        foreach (RibbonTab RT in rc.Items)
        //        {
        //            if (AutomationProperties.GetAutomationId(RT) == automationID)
        //            {
        //                //mimic user click
        //                //MouseEventArgs - not working...
        //                //MouseDevice mouse = InputManager.Current.PrimaryMouseDevice;                        
        //                //MouseButtonEventArgs arg = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left);
        //                //arg.RoutedEvent = RibbonTab.PreviewMouseLeftButtonDownEvent;                        
        //                //RT.RaiseEvent(arg);

        //                // for now use direct change of the tab
        //                rc.SelectedItem = RT;

        //                SleepWithDoEvents(100);

        //                //while (!rc.IsVisible)
        //                //{
        //                //    Thread.Sleep(100);
        //                //}
        //                return;
        //            }
        //        }
        //        throw new Exception("SelectRibbonTab element not found by AutomationID: " + automationID);
        //    });
        //}


        //private void SelectRibbonTab(string automationID)
        //{
        //    Execute(() => {
        //        Ribbon rc = (Ribbon)mMainWindow.FindName("MainRibbon");
        //        foreach (RibbonTab RT in rc.Items)
        //        {
        //            if (AutomationProperties.GetAutomationId(RT) == automationID)
        //            {
        //                //mimic user click
        //                //MouseEventArgs - not working...
        //                //MouseDevice mouse = InputManager.Current.PrimaryMouseDevice;                        
        //                //MouseButtonEventArgs arg = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left);
        //                //arg.RoutedEvent = RibbonTab.PreviewMouseLeftButtonDownEvent;                        
        //                //RT.RaiseEvent(arg);

        //                // for now use direct change of the tab
        //                rc.SelectedItem = RT;

        //                SleepWithDoEvents(100);                        

        //                //while (!rc.IsVisible)
        //                //{
        //                //    Thread.Sleep(100);
        //                //}
        //                return;
        //            }
        //        }
        //        throw new Exception("SelectRibbonTab element not found by AutomationID: " + automationID);
        //    });
        //}

        public BusinessFlowPOM SelectBusinessFlow()
        {
            businessFlow = null;
            Execute(() =>
            {
                ClickBusinessFlowRibbon();
                Frame f = (Frame)mMainWindow.FindName("xMainWindowFrame");
                BusinessFlowsAutomatePage resourcesPage = (BusinessFlowsAutomatePage)f.Content;
                Frame f2 = (Frame)resourcesPage.FindName("xContentFrame");
                SingleItemTreeViewExplorerPage singleItemTreePage = (SingleItemTreeViewExplorerPage)f2.Content;

                TreeView1 lv = (TreeView1)singleItemTreePage.FindName("xTreeView");

                while (!singleItemTreePage.IsVisible)
                {
                    SleepWithDoEvents(100);
                }

                businessFlow = new BusinessFlowPOM(singleItemTreePage);
               
            });

            return businessFlow;
        }

        public void AddActivityToLIstView()
        {
            Execute(() =>
            {
                Frame f1 = (Frame)mMainWindow.FindName("xMainWindowFrame");
                BusinessFlowsAutomatePage page1 = (BusinessFlowsAutomatePage)f1.Content;
                Frame f2 = (Frame)page1.FindName("xContentFrame");
                NewAutomatePage page2 = (NewAutomatePage)f2.Content;
                Frame f3 = (Frame)page2.FindName("xActivitiesListFrame");
                ActivitiesListViewPage activitiesListPage = (ActivitiesListViewPage)f3.Content;
                Activity activity2 = new Activity();
                activity2.ActivityName = "Test Activity";
                Dispatcher.Invoke(() =>
                {
                    activitiesListPage.ListView.DataSourceList.Add(activity2);
                    SleepWithDoEvents(100);
                });

                ClickOnBackToBFTreeBtn();

            });
        }


        public int ClickOnUndoBtn()
        {
            Execute(() =>
            {
                Frame f1 = (Frame)mMainWindow.FindName("xMainWindowFrame");
                BusinessFlowsAutomatePage page1 = (BusinessFlowsAutomatePage)f1.Content;
                Frame f2 = (Frame)page1.FindName("xContentFrame");
                NewAutomatePage page2 = (NewAutomatePage)f2.Content;

                var elByName = FindElementByName(page2, "xUndoChangesBtn");

                if (elByName != null)
                {
                    if (elByName is Amdocs.Ginger.UserControls.ucButton)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            (elByName as Amdocs.Ginger.UserControls.ucButton).DoClick();
                            SleepWithDoEvents(100);

                        });
                    }
                }

                Frame f3 = (Frame)page2.FindName("xActivitiesListFrame");
                ActivitiesListViewPage activitiesListPage = (ActivitiesListViewPage)f3.Content;

              ActivityCount =activitiesListPage.ListView.DataSourceList.Count;

            });

            return ActivityCount;

            }


        public void ClickOnBackToBFTreeBtn()
        {
            Execute(() =>
            {
                Frame f1 = (Frame)mMainWindow.FindName("xMainWindowFrame");
                BusinessFlowsAutomatePage page1 = (BusinessFlowsAutomatePage)f1.Content;
                Frame f2 = (Frame)page1.FindName("xContentFrame");
                NewAutomatePage page2 = (NewAutomatePage)f2.Content;

                var elByName = FindElementByName(page2, "xGoToBFsTreeBtn");

                if (elByName != null)
                {
                    if (elByName is Amdocs.Ginger.UserControls.ucButton)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            (elByName as Amdocs.Ginger.UserControls.ucButton).DoClick();
                            SleepWithDoEvents(100);
                        });
                    }
                }

               

                

            });
        }

        public EnvironmentsPOM GotoEnvironments()
        {
            Environments = null;
            Execute(() => {
                ClickResourcesRibbon();
                Frame f = (Frame)mMainWindow.FindName("xMainWindowFrame");
                TwoLevelMenuPage resourcesPage = (TwoLevelMenuPage)f.Content;

                ListView lv = (ListView)resourcesPage.FindName("xMainNavigationListView");
                lv.SelectedItem = null;
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


        public List<string> GetMenus()
        {
            List<string> list = new List<string>();
            Execute(() => {
                foreach (Menu menu in FindVisualChildren<Menu>(mMainWindow))
                {                    
                    list.Add(menu.Name);                    
                }                
            });
            return list;
        }


        // TODO: move to more generic place
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }


        internal AgentsPOM GotoAgents()
        {
            Agents = null;
            Execute(() => {
                ClickConfigurationsRibbon();
                Frame f = (Frame)mMainWindow .FindName("xMainWindowFrame");
                TwoLevelMenuPage p = (TwoLevelMenuPage)f.Content;
                
                TwoLevelMenuPage configurationsPage = (TwoLevelMenuPage)f.Content;

                ListView lv = (ListView)configurationsPage.FindName("xMainNavigationListView");
                foreach (TopMenuItem topMenuItem in lv.Items)
                {                    
                    if (topMenuItem.AutomationID == "Agents AID")
                    {
                        lv.SelectedItem = topMenuItem;
                        SleepWithDoEvents(100);
                        Frame f1 = (Frame)FindElementByName(configurationsPage, "xSelectedItemFrame");
                        SingleItemTreeViewExplorerPage itemExplorerPage = (SingleItemTreeViewExplorerPage)f1.Content;
                        while (!itemExplorerPage.IsVisible)
                        {
                            SleepWithDoEvents(100);
                        }
                        Agents = new AgentsPOM(itemExplorerPage);
                        break;
                    }
                }
            });

            if (Agents == null) throw new Exception("Cannot goto Agents");

            return Agents;

        }

        internal POMsPOM GotoPOMs()
        {
            Agents = null;
            Execute(() =>
            {
                ClickResourcesRibbon();
                Frame f = (Frame)mMainWindow.FindName("xMainWindowFrame");

                TwoLevelMenuPage resourcesPage = (TwoLevelMenuPage)f.Content;

                ListView lv = (ListView)resourcesPage.FindName("xMainNavigationListView");

                foreach (TopMenuItem topMenuItem in lv.Items)
                {
                    if (topMenuItem.AutomationID == "Application Models AID")
                    {
                        lv.SelectedItem = topMenuItem;
                        ListView lvi = (ListView)resourcesPage.FindName("xSubNavigationListView");
                        foreach (SubMenuItem subMenuItem in lvi.Items)
                        {
                            if (subMenuItem.AutomationID == "POM Menu AID")
                            {
                                lvi.SelectedItem = subMenuItem;
                            }

                        }
                        SleepWithDoEvents(100);
                        Frame f1 = (Frame)FindElementByName(resourcesPage, "xSelectedItemFrame");
                        SingleItemTreeViewExplorerPage itemExplorerPage = (SingleItemTreeViewExplorerPage)f1.Content;
                        while (!itemExplorerPage.IsVisible)
                        {
                            SleepWithDoEvents(100);
                        }
                        POMs = new POMsPOM(itemExplorerPage);
                        break;
                    }
                }
            });

            return POMs;

        }


        internal GlobalVariablesPOM GotoGlobalVariables()
        {
            GlobalVariables = null;
            Execute(() => {
                                
                ClickResourcesRibbon();
                Frame f = (Frame)mMainWindow.FindName("xMainWindowFrame");
                TwoLevelMenuPage resourcesPage = (TwoLevelMenuPage)f.Content;

                ListView lv = (ListView)resourcesPage.FindName("xMainNavigationListView");
                lv.SelectedItem = null;
                foreach (TopMenuItem topMenuItem in lv.Items)
                {
                    if (topMenuItem.AutomationID == "Global Variables AID")
                    {
                        lv.SelectedItem = topMenuItem;
                        SleepWithDoEvents(100);
                        Frame f1 = (Frame)FindElementByName(resourcesPage, "xSelectedItemFrame");
                        //VariablesPage variablesPage = (VariablesPage)f1.Content;
                        
                       // GlobalVariables = new GlobalVariablesPOM(variablesPage);
                        break;
                    }
                }
            });

            if (GlobalVariables == null) throw new Exception("Cannot goto Global Variables");

            return GlobalVariables;

        }

        // TODO: move to generic UI utils
        internal void TakeScreenShot(string fileName)
        {
            Execute(() =>
            {
                int width = 1200;
                int height = 800;
                RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                renderTargetBitmap.Render(mMainWindow);
                PngBitmapEncoder pngImage = new PngBitmapEncoder();
                pngImage.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
                using (Stream fileStream = File.Create(fileName))
                {
                    pngImage.Save(fileStream);
                }
            });
        }

    }
        
}
