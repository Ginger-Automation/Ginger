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
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace GingerTest.POMs.Common
{    
    public class ContextMenuPOM : GingerPOMBase
    {
        ContextMenu mContextMenu;
        UIElement mUIElement;

        public ContextMenuPOM(UIElement UIElement, ContextMenu contextMenu)
        {
            mContextMenu = contextMenu;
            mUIElement = UIElement;
            mContextMenu.Placement = PlacementMode.Relative;
            contextMenu.PlacementTarget = mUIElement;
        }

        public ContextMenuItemPOM this[string header]
        {
            get
            {
                ContextMenuItemPOM c = null;
                Execute(() => { 
                    foreach (MenuItem MI in mContextMenu.Items)
                    {
                        if (MI.Header.ToString() == header)
                        {
                            c = new ContextMenuItemPOM(MI);
                            break;
                        }
                    }                    
                });
                if (c != null)
                {
                    return c;
                }
                else
                {
                    throw new Exception("ContextMenuPOM: Sub menu not found - " + header);
                }

            }
        }

        

        // TODO: get items - to compare menu as joined string

        public void Click(string header)
        {
            foreach (MenuItem MI in mContextMenu.Items)
            {
                if (MI.Header.ToString() == header)
                {
                    //MenuItem MI = (MenuItem)contextMenu.Items[0];
                    MI.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, 0) { RoutedEvent = Mouse.MouseEnterEvent });
                    SleepWithDoEvents(100);
                    // TODO: run on thread as modal dialog might pop up
                    Task.Factory.StartNew(() => {
                        MI.RaiseEvent(new RoutedEventArgs(MenuItem.ClickEvent));
                        SleepWithDoEvents(100);
                    });
                }
            }
        }
    }
}
