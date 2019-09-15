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
using System.Collections.Generic;
using System.Windows.Controls;
using Ginger.WindowExplorer;
using GingerCore.Actions;
using GingerCore.Actions.Windows;
using GingerCore.Drivers.Common;
using GingerWPF.UserControlsLib.UCTreeView;

namespace Ginger.Drivers.Windows
{
    public class WindowsComboBoxTreeItem : WindowsElementTreeItemBase, ITreeViewItem, IWindowExplorerTreeItem
    {
        StackPanel ITreeViewItem.Header()
        {
            string ImageFileName = "@DropDownList_16x16.png";
            string Title = UIAElementInfo.ElementTitle;
            return TreeViewUtils.CreateItemHeader(Title, ImageFileName);            
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list = new ObservableList<Act>();

                list.Add(new ActWindowsControl()
                {
                    Description = "Set " + UIAElementInfo.ElementTitle + " Value",
                    Value = "Sample Value",
                    ControlAction = ActWindowsControl.eControlAction.SetValue
                });

                list.Add(new ActWindowsControl()
                {
                    Description = "Get " + UIAElementInfo.ElementTitle + " Value",
                    ControlAction = ActWindowsControl.eControlAction.SetValue
                });

                //Add option to select the valid values
                List<ComboBoxElementItem> ComboValues = (List<ComboBoxElementItem>)UIAElementInfo.GetElementData();
                foreach (ComboBoxElementItem CBEI in ComboValues)
                {
                    list.Add(new ActWindowsControl()
                    {
                        Description = "Set ComboBox Text of '" + this.UIAElementInfo.ElementTitle + "' to '" + CBEI.Text + "'",
                        ControlAction = ActWindowsControl.eControlAction.SetValue,
                        Value = CBEI.Text
                    });
                }
            return list;
        }
    }
}