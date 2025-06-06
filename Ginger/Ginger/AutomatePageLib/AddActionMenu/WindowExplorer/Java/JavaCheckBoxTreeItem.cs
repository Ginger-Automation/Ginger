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
using Amdocs.Ginger.Common.UIElement;
using GingerCore.Actions;
using GingerCore.Actions.Java;
using GingerWPF.UserControlsLib.UCTreeView;
using System.Windows.Controls;

namespace Ginger.WindowExplorer.Java
{
    class JavaCheckBoxTreeItem : JavaElementTreeItem, ITreeViewItem, IWindowExplorerTreeItem
    {
        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.CreateItemHeader(Name, ElementInfo.GetElementTypeImage(eElementType.CheckBox));
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list =
            [
                new ActJavaElement()
                {
                    Description = "Set " + Name + " ON",
                    ControlAction = ActJavaElement.eControlAction.SetValue,
                    Value = "true"
                },
                new ActJavaElement()
                {
                    Description = "Set " + Name + " OFF",
                    ControlAction = ActJavaElement.eControlAction.SetValue,
                    Value = "false"
                },
                new ActJavaElement()
                {
                    Description = "Toggle Checkbox " + Name,
                    ControlAction = ActJavaElement.eControlAction.Toggle
                },
                new ActJavaElement()
                {
                    Description = "Get " + Name + " Value",
                    ControlAction = ActJavaElement.eControlAction.GetValue
                },
                new ActJavaElement()
                {
                    Description = "Get IsEnabled Property " + Name,
                    ControlAction = ActJavaElement.eControlAction.IsEnabled
                },
                new ActJavaElement()
                {
                    Description = "Is Checked " + Name,
                    ControlAction = ActJavaElement.eControlAction.IsChecked
                },
            ];
            return list;
        }
    }
}
