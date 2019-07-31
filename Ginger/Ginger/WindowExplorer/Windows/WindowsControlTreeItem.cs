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
using System;
using System.Windows.Controls;
using Ginger.Drivers.UIA;
using Ginger.WindowExplorer;
using GingerCore.Actions;
using GingerCore.Actions.Windows;
using GingerWPF.UserControlsLib.UCTreeView;
using Amdocs.Ginger.Common.Enums;

namespace Ginger.Drivers.Windows
{
    public class WindowsControlTreeItem : WindowsElementTreeItemBase, ITreeViewItem, IWindowExplorerTreeItem
    {
        Object ITreeViewItem.NodeObject()
        {
            return base.UIAElementInfo;
        }

        StackPanel ITreeViewItem.Header()
        {
            string ImageFileName = "@Agent_16x16.png";
            string Title = UIAElementInfo.ElementTitle;
            return TreeViewUtils.CreateItemHeader(Title, ImageFileName);            
        }

        Page ITreeViewItem.EditPage(Amdocs.Ginger.Common.Context mContext)
        {
            return new UIAElementPage(base.UIAElementInfo);            
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return null;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            TV.AddToolbarTool(eImageType.Refresh, "Refresh", TV.Tree.RefreshSelectedTreeNodeChildrens);
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list = new ObservableList<Act>();
          
                list.Add(new ActWindowsControl()
                {
                    Description = "Click " + UIAElementInfo.ElementTitle,
                    ControlAction = ActWindowsControl.eControlAction.Click
                });

                list.Add(new ActWindowsControl()
                {
                    Description = "Double Click " + UIAElementInfo.ElementTitle,
                    ControlAction = ActWindowsControl.eControlAction.DoubleClick
                });
          
            return list;
        }
    }
}