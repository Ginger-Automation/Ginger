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
using Ginger.Drivers.Windows;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore.Actions;
using GingerCore.Actions.Windows;
using System.Windows.Controls;

namespace Ginger.WindowExplorer.Windows
{
    public class WindowsCheckBoxTreeItem : WindowsElementTreeItemBase, ITreeViewItem, IWindowExplorerTreeItem
    {
        StackPanel ITreeViewItem.Header()
        {
            string ImageFileName = "@Checkbox_16x16.png";
            string Title = UIAElementInfo.ElementTitle;
            return TreeViewUtils.CreateItemHeader(Title, ImageFileName);
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list = new ObservableList<Act>();
            list.Add(new ActWindowsControl()
            {
                Description = "Set Checkbox to checked " + UIAElementInfo.ElementTitle,
                ControlAction = ActWindowsControl.eControlAction.SetValue,
                Value = "Checked"
            });

            list.Add(new ActWindowsControl()
            {
                Description = "Get Checkbox status " + UIAElementInfo.ElementTitle,
                ControlAction = ActWindowsControl.eControlAction.GetValue,
                Value = "Checked"
            });

            list.Add(new ActWindowsControl()
            {
                Description = "Toggle Checkbox " + UIAElementInfo.ElementTitle,
                ControlAction = ActWindowsControl.eControlAction.Toggle,
                Value = "Checked"
            });

            list.Add(new ActWindowsControl()
            {
                Description = "Checkbox " + UIAElementInfo.ElementTitle + " IsEnabled",
                ControlAction = ActWindowsControl.eControlAction.IsEnabled,
                Value = "Checked"
            });

            return list;
        }
    }
}