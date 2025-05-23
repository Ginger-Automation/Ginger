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
using GingerCore.Actions.Common;
using GingerWPF.UserControlsLib.UCTreeView;
using System.Windows.Controls;

namespace Ginger.WindowExplorer.Android
{
    public class AndroidWidgetEditTextTreeItem : AndroidElementTreeItemBase, ITreeViewItem, IWindowExplorerTreeItem
    {
        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.CreateItemHeader(Name, ElementInfo.GetElementTypeImage(eElementType.TextBox));
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list =
            [
                new ActUIElement()
                {
                    Description = "Set Text " + Name,
                    ElementAction = ActUIElement.eElementAction.SetValue
                },
                new ActUIElement()
                {
                    Description = "Get " + Name + " Text",
                    ElementAction = ActUIElement.eElementAction.GetValue
                },
                new ActUIElement()
                {
                    Description = "Validate " + Name + " Is Enabled  ",
                    ElementAction = ActUIElement.eElementAction.IsEnabled
                },
            ];
            return list;
        }
    }
}
