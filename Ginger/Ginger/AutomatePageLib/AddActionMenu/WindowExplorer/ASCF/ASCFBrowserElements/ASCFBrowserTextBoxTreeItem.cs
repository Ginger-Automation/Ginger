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
using GingerCore.Actions.ASCF;
using GingerWPF.UserControlsLib.UCTreeView;
using System.Windows.Controls;

namespace Ginger.WindowExplorer.ASCF
{
    class ASCFBrowserTextBoxTreeItem : ASCFBrowserElementTreeItem, ITreeViewItem, IWindowExplorerTreeItem
    {
        StackPanel ITreeViewItem.Header()
        {
            return TreeViewUtils.CreateItemHeader(Name, ElementInfo.GetElementTypeImage(eElementType.TextBox));
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list = [];
            ActASCFBrowserElement a2 = new ActASCFBrowserElement
            {
                Description = "Set " + ASCFBrowserElementInfo.Path + " Value",
                LocateBy = eLocateBy.ByID,
                LocateValue = ASCFBrowserElementInfo.Path,
                ControlAction = ActASCFBrowserElement.eControlAction.SetValue,
                Value = "ABC"
            };
            list.Add(a2);

            ActASCFBrowserElement a1 = new ActASCFBrowserElement
            {
                Description = "Get Value",
                LocateBy = eLocateBy.ByName,
                LocateValue = ASCFBrowserElementInfo.Path
            };
            list.Add(a1);

            list.Add(new ActASCFBrowserElement()
            {
                Description = "Set Value",
                LocateBy = eLocateBy.ByXPath,
                LocateValue = ASCFBrowserElementInfo.GetProperty("XPath"),
                ControlAction = ActASCFBrowserElement.eControlAction.SetValue
            });
            return list;
        }
    }
}