#region License
/*
Copyright © 2014-2024 European Support Limited

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
using Ginger.Drivers.PowerBuilder;
using GingerCore.Actions;
using GingerWPF.UserControlsLib.UCTreeView;
using System.Windows.Controls;

namespace Ginger.WindowExplorer.PowerBuilder
{
    class PBCheckBoxTreeItem : PBControlTreeItemBase, ITreeViewItem, IWindowExplorerTreeItem
    {
        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.CreateItemHeader(UIAElementInfo.ElementTitle, ElementInfo.GetElementTypeImage(eElementType.CheckBox));
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list =
            [
                new ActPBControl()
                {
                    Description = "Set Checkbox to checked " + UIAElementInfo.ElementTitle,
                    ControlAction = ActPBControl.eControlAction.SetValue,
                    Value = "Checked"
                },
                new ActPBControl()
                {
                    Description = "Get Checkbox status " + UIAElementInfo.ElementTitle,
                    ControlAction = ActPBControl.eControlAction.GetValue,
                    Value = "Checked"
                },
                new ActPBControl()
                {
                    Description = "Toggle Checkbox " + UIAElementInfo.ElementTitle,
                    ControlAction = ActPBControl.eControlAction.Toggle,
                    Value = "Checked"
                },
                new ActPBControl()
                {
                    Description = "Checkbox " + UIAElementInfo.ElementTitle + " IsEnabled",
                    ControlAction = ActPBControl.eControlAction.IsEnabled,
                    Value = "Checked"
                },
            ];
            return list;
        }
    }
}
