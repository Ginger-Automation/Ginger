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
using Ginger.WindowExplorer;
using GingerCore.Actions;
using GingerWPF.UserControlsLib.UCTreeView;
using System.Windows.Controls;

namespace Ginger.Actions.Locators.ASCF
{
    class ASCFLabelTreeItem : ASCFControlTreeItem, ITreeViewItem, IWindowExplorerTreeItem
    {
        StackPanel ITreeViewItem.Header()
        {
            //TODO: put label icon
            string ImageFileName = "@Label_16x16.png";
            return TreeViewUtils.CreateItemHeader(Name, ImageFileName);
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list = [];

            ActASCFControl a1 = new ActASCFControl
            {
                Description = "Get " + ASCFControlInfo.Path + " Value",
                LocateBy = eLocateBy.ByName,
                LocateValue = ASCFControlInfo.Path,
                ControlAction = ActASCFControl.eControlAction.GetControlProperty,
                ControlProperty = ActASCFControl.eControlProperty.Value
            };
            list.Add(a1);

            return list;
        }
    }
}
