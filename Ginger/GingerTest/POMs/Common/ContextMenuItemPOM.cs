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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GingerTest.POMs.Common
{
    public class ContextMenuItemPOM : GingerPOMBase
    {
        MenuItem mMenuItem;

        public ContextMenuItemPOM(MenuItem menuItem)
        {
            mMenuItem = menuItem;
        }

        public void Click()
        {
            // TODO: run on thread as modal dialog might pop up
            Task.Factory.StartNew(() => {
                Execute(() => {
                    //open the menu
                    ((ContextMenu)mMenuItem.Parent).IsOpen = true;
                    SleepWithDoEvents(100);
                    // Highlight the element
                    mMenuItem.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = Mouse.MouseEnterEvent });
                    SleepWithDoEvents(100);                    
                    mMenuItem.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                    SleepWithDoEvents(100);
                });
            });
            SleepWithDoEvents(1000);
        }
    }
}
