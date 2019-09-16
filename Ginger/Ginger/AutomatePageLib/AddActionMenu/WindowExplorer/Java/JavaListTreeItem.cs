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
using GingerCore;
using GingerCore.Actions;
using GingerCore.Actions.Java;
using System.Windows.Controls;
using GingerWPF.UserControlsLib.UCTreeView;

namespace Ginger.WindowExplorer.Java
{
    class JavaListTreeItem:JavaElementTreeItem, ITreeViewItem, IWindowExplorerTreeItem
    {
        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list = new ObservableList<Act>();

            list.Add(new ActJavaElement()
            {
                Description = "Set Selected Value " + Name,
                ControlAction = ActJavaElement.eControlAction.SetValue
            });

            list.Add(new ActJavaElement()
            {
                Description = "Get " + Name + " Value",
                ControlAction = ActJavaElement.eControlAction.GetValue
            });
            return list;
        }

        StackPanel ITreeViewItem.Header()
        {
            string ImageFileName = "@List_16x16.png";
            return TreeViewUtils.CreateItemHeader(Name, ImageFileName);
        }
    }
}
