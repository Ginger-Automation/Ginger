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
using System.Windows.Controls;
using GingerCore.Actions;
using GingerCore.Actions.ASCF;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore.Actions.Common;
using Amdocs.Ginger.Common.UIElement;

namespace Ginger.WindowExplorer.ASCF
{
    class ASCFBrowserTextBoxTreeItem : ASCFBrowserElementTreeItem , ITreeViewItem, IWindowExplorerTreeItem
    {
        StackPanel ITreeViewItem.Header()
        {            
            string ImageFileName = "@TextBox_16x16.png";
            return TreeViewUtils.CreateItemHeader(Name, ImageFileName);
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list = new ObservableList<Act>();
            ActASCFBrowserElement a2 = new ActASCFBrowserElement();
            a2.Description = "Set " + ASCFBrowserElementInfo.Path + " Value";
            a2.LocateBy = eLocateBy.ByID;
            a2.LocateValue = ASCFBrowserElementInfo.Path;
            a2.ControlAction = ActASCFBrowserElement.eControlAction.SetValue;
            a2.Value = "ABC";
            list.Add(a2);

            ActASCFBrowserElement a1 = new ActASCFBrowserElement();
            a1.Description = "Get Value";
            a1.LocateBy = eLocateBy.ByName;
            a1.LocateValue = ASCFBrowserElementInfo.Path;
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