#region License
/*
Copyright © 2014-2026 European Support Limited
 
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
    class ASCFBrowserLabelTreeItem : ASCFBrowserElementTreeItem, ITreeViewItem, IWindowExplorerTreeItem
    {
        StackPanel ITreeViewItem.Header()
        {
            //TODO: put text icon
            return TreeViewUtils.CreateItemHeader(Name, ElementInfo.GetElementTypeImage(eElementType.Label));
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list = [];

            ActASCFBrowserElement a2 = new ActASCFBrowserElement
            {
                Description = "Get Label Text " + ASCFBrowserElementInfo.Path,
                LocateBy = eLocateBy.ByName,
                LocateValue = ASCFBrowserElementInfo.Path,
                ControlAction = ActASCFBrowserElement.eControlAction.GetValue
            };
            list.Add(a2);


            ActASCFBrowserElement a3 = new ActASCFBrowserElement
            {
                Description = "Get Label Text " + ASCFBrowserElementInfo.Path,
                LocateBy = eLocateBy.ByID,
                LocateValue = ASCFBrowserElementInfo.Path,
                ControlAction = ActASCFBrowserElement.eControlAction.GetValue
            };
            list.Add(a3);
            return list;
        }
    }
}
