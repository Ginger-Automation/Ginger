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

using GingerWPFUnitTest.POMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace GingerTest.POMs.Common
{    
    public class TreeViewItemPOM : GingerPOMBase
    {
        TreeViewItem mTreeViewItem;

        public TreeViewItemPOM(TreeViewItem treeViewItem)
        {
            mTreeViewItem = treeViewItem;
        }


        public ContextMenuPOM ContextMenu
        {
            get
            {
                ContextMenuPOM CM = null;
                Execute(() => {
                    // Trigger event so menu will be created for the MI
                    MouseButtonEventArgs e = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Right)
                                                                        { RoutedEvent = Mouse.PreviewMouseDownEvent };
                    mTreeViewItem.RaiseEvent(e);

                    ContextMenu contextMenu = mTreeViewItem.ContextMenu;
                    //contextMenu.Placement = PlacementMode.Relative;
                    //contextMenu.PlacementTarget = mTreeViewItem;
                    //contextMenu.IsOpen = true;
                    //SleepWithDoEvents(100);
                    CM = new ContextMenuPOM(mTreeViewItem, contextMenu);
                });
                return CM;
            }
        }
    }
}
