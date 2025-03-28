#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Ginger.Drivers.PowerBuilder;
using GingerCore.Actions;
using GingerWPF.UserControlsLib.UCTreeView;
using System.Windows.Controls;

namespace Ginger.WindowExplorer.PowerBuilder
{
    class PBScrollBarTreeItem : PBControlTreeItemBase, ITreeViewItem, IWindowExplorerTreeItem
    {
        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.CreateItemHeader(UIAElementInfo.ElementTitle, Amdocs.Ginger.Common.Enums.eImageType.Agent);
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list =
            [
                //Window element with name "Simple Page" existe so it is not working
                new ActPBControl()
                {
                    Description = "ScrollUp the scrollbar - " + UIAElementInfo.ElementTitle,
                    ControlAction = ActPBControl.eControlAction.ScrollUp
                },
                new ActPBControl()
                {
                    Description = "ScrollDown the scrollbar - " + UIAElementInfo.ElementTitle,
                    ControlAction = ActPBControl.eControlAction.Scrolldown
                },
            ];

            return list;
        }
    }
}
