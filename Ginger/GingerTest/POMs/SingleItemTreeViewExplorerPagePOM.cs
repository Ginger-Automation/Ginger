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

using Amdocs.Ginger.UserControls;
using GingerTest.POMs.Common;
using GingerWPF.UserControlsLib;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPFUnitTest.POMs
{
    public class SingleItemTreeViewExplorerPagePOM : GingerPOMBase
    {
        private SingleItemTreeViewExplorerPage mItemTreeViewExplorer;

        public SingleItemTreeViewExplorerPagePOM(SingleItemTreeViewExplorerPage ps)
        {
            this.mItemTreeViewExplorer = ps;
        }

        TreeView1 SingleItemTreeViewControl
        {
            get
            {
                TreeView1 tv = null;
                mItemTreeViewExplorer.Dispatcher.Invoke(() => 
                {                
                    tv = (TreeView1)LogicalTreeHelper.FindLogicalNode(mItemTreeViewExplorer, "xTreeView");
                });
                return tv;
            }
        }

        UCTreeView TreeViewControl
        {
            get
            {                 
                UCTreeView tvu = (UCTreeView)FindElementByName(SingleItemTreeViewControl,"xTreeViewTree");                
                return tvu;
            }
        }

        public TreeViewItem GetRootItem()
        {
            TreeViewItem tv = null;
            Execute(() => { 
                var items = TreeViewControl.TreeItemsCollection;
                tv = (TreeViewItem)items[0];
            });
            return tv;
        }

        public UCButtonPOM AddButton { get
            {
                //AddButton                    
                ucButton b = (ucButton)FindElementByName(SingleItemTreeViewControl, "xAddButton");
                UCButtonPOM p = new UCButtonPOM(b);
                return p;
            }
        }

        public TreeViewItemPOM SelectedItem
        {
            get
            {
                TreeViewItemPOM TV = null;
                Execute(() => { 
                    TreeView tv = (TreeView)TreeViewControl.FindName("Tree");
                    TreeViewItem tvi = (TreeViewItem)tv.SelectedItem;
                    TV = new TreeViewItemPOM(tvi);
                });
                return TV;
            }
        }

        internal void SelectRootItem()
        {                        
            Execute(() => {
                GetRootItem().IsSelected = true;                
            });         
        }


        internal void SelectItem(string title)
        {
            TreeViewItem tvi = FindItem(title);
            if (tvi != null)
            {
                Execute(() => { 
                    tvi.IsSelected = true;
                    SleepWithDoEvents(100);
                });
            }
            else
            {
                throw new Exception("Item not found in treeview: " + title);
            }            
        }

        TreeViewItem FindItem(string title)
        {
            TreeViewItem tvi = null;
            Execute(() => 
            {
                // retry up to 10 seconds
                int i = 0;
                while (tvi == null & i<100)
                {
                    tvi = FindItemInList(GetRootItems(), title);                                        
                    if (tvi == null)
                    {
                        SleepWithDoEvents(100);
                    }
                    i++;                    
                }                                
            });
            return tvi;
        }

        TreeViewItem FindItemInList(IEnumerable<TreeViewItem> list, string title)
        {            
            foreach (TreeViewItem tvi1 in list)
            {
                if (tvi1.Header.ToString() == "DUMMY") continue;
                StackPanel sp = (StackPanel)tvi1.Header;

                //ImageMakerControl imc1 = (ImageMakerControl)sp.Children[0];
                // imc1.ImageType
                //ImageMakerControl imc2 = (ImageMakerControl)sp.Children[1];

                // find the label - not all the time in same index
                Label lbl = null;
                foreach (var uie in sp.Children)
                {
                    if (uie is Label)
                    {
                        lbl = (Label)uie;
                        break;
                    }
                }

                if ((string)lbl.Content == title)
                {
                    return tvi1;                    
                }

                //tvi1.IsExpanded = true;

                //Drill down recurs
                if (tvi1.Items.Count > 0)
                {
                    TreeViewItem tvisub = FindItemInList(tvi1.Items.Cast<TreeViewItem>(), title);
                    if (tvisub != null)
                    {
                        return tvisub;
                    }
                }
            }
            return null;
        }


        internal object GetSelectedItemNodeObject()
        {
            object selectedItem = null;
            Execute(() => { 
                selectedItem = (TreeViewControl.CurrentSelectedTreeViewItem).NodeObject();
            });
            return selectedItem;
        }

        public Page GetSelectedItemEditPage()
        {
            Page p = null;
            Execute(() => {
                Frame frame = (Frame)FindElementByName(mItemTreeViewExplorer, "DetailsFrame");               
                p = (Page)frame.Content;                
            });
            return p;
        }

        public IEnumerable<TreeViewItem> GetRootItems()
        {            
            TreeViewItem RootTVI = GetRootItem();
            IEnumerable<TreeViewItem> items = RootTVI.Items.Cast<TreeViewItem>();
            return items;            
        }

       
        /// <summary>
        ///  Check if item exist in the tree, include retry mechanism of 10 sec
        ///  Use it when you expect the item to be fiund
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        internal bool IsItemExist(string title)
        {            
            TreeViewItem tvi = FindItem(title);            
            if (tvi != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Check if item not in the tree include retry mechanism if item exist to wait if it will be removed
        /// Use it when you expect the item not to be found
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        internal bool IsItemNotExist(string title)
        {
            bool bFound = true;
            TreeViewItem tvi = null;
            Execute(() =>
            {
                // retry up to 10 seconds
                int i = 0;
                while (bFound == true && i < 100) 
                {
                    tvi = FindItemInList(GetRootItems(), title);
                    if (tvi == null)
                    {
                        bFound = false;
                    }
                    else
                    {
                        Thread.Sleep(100);
                        i++;
                    }
                    
                } 
            });
            return !bFound;
        }


        internal void Copy()
        {
            SelectedItem.ContextMenu["Copy"].Click();            
        }

        internal void Duplicate(string newName)
        {
            SelectedItem.ContextMenu["Duplicate"].Click();
            InputBoxWindowPOM p = new InputBoxWindowPOM();
            p.SetText(newName);
            p.ClickOK();
        }

        internal void Paste(string NewName = null)
        {
            SelectedItem.ContextMenu["Paste"].Click();
            if (NewName != null)
            {
                CurrentInputBoxWindow.SetText(NewName);
                CurrentInputBoxWindow.ClickOK();
            }            
        }

        internal void Cut()
        {
            SelectedItem.ContextMenu["Cut"].Click();
        }

        //internal void RightClickSelectItem(string header)
        //{          
        //    Execute(() => {
        //        TreeView TV = (TreeView)FindElementByName(TreeViewControl, "Tree");               
        //         MouseButtonEventArgs e2 = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Right)
        //                                                                { RoutedEvent = Mouse.MouseDownEvent };
        //        SleepWithDoEvents(1000);
        //        //  MouseButtonEventArgs e3 = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Right)
        //        //  { RoutedEvent = Mouse.MouseUpEvent };




        //         TV.RaiseEvent(e2);
        //        // SleepWithDoEvents(100);
        //        // TV.RaiseEvent(e3);
        //        SleepWithDoEvents(10000);

        //        ContextMenu contextMenu = TreeViewControl.CurrentSelectedTreeViewItem.Menu();
        //        contextMenu.IsOpen = true;
        //        contextMenu.Placement = PlacementMode.Center;
        //        contextMenu.PlacementTarget = TV;
        //        SleepWithDoEvents(100);
        //        //contextMenu.RaiseEvent(new ContextMenuEventArgs(ContextMenu.ContextMenuOpeningEvent));
        //        foreach (MenuItem MI in contextMenu.Items)
        //        {
        //            if (MI.Header == header)
        //            {
        //                //MenuItem MI = (MenuItem)contextMenu.Items[0];
        //                MI.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = Mouse.MouseEnterEvent });
        //                SleepWithDoEvents(100);
        //                // TODO: run on thread as modal dialog might pop up
        //                Task.Factory.StartNew(() => { 
        //                    MI.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
        //                    SleepWithDoEvents(100);
        //                });
        //            }
        //        }

        //    });

        //}
    }
}
