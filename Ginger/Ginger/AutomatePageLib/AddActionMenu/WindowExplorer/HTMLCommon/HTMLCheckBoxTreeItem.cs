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
using GingerWPF.UserControlsLib.UCTreeView;
using System.Windows.Controls;

namespace Ginger.WindowExplorer.HTMLCommon
{
    public class HTMLCheckBoxTreeItem : HTMLElementTreeItemBase, ITreeViewItem, IWindowExplorerTreeItem
    {
        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.CreateItemHeader(ElementInfo.ElementTitle, ElementInfo.GetElementTypeImage(eElementType.CheckBox));
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list =
            [
                new ActGenElement()
                {
                    Description = "Set Check Box ON " + this.ElementInfo.ElementTitle,
                    GenElementAction = ActGenElement.eGenElementAction.SetValue,
                    Value = "true"
                },
                new ActGenElement()
                {
                    Description = "Set Check Box OFF " + this.ElementInfo.ElementTitle,
                    GenElementAction = ActGenElement.eGenElementAction.SetValue,
                    Value = "false"
                },
                new ActGenElement()
                {
                    Description = "Validate CheckBox Text " + this.ElementInfo.ElementTitle,
                    GenElementAction = ActGenElement.eGenElementAction.GetValue,
                    Value = ""
                },
                new ActGenElement()
                {
                    Description = "Validate CheckBox is Enabled " + this.ElementInfo.ElementTitle,
                    GenElementAction = ActGenElement.eGenElementAction.Enabled,
                    Value = ""
                    //TODO: add REturn value Actual = Enabled=true
                },
            ];

            AddGeneralHTMLActions(list);
            return list;
        }
    }
}
