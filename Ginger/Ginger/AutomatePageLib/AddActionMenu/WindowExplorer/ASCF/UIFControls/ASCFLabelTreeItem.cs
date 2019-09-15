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
using Ginger.WindowExplorer;
using GingerCore.Actions;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore.Actions.Common;
using Amdocs.Ginger.Common.UIElement;

namespace Ginger.Actions.Locators.ASCF
{
    class ASCFLabelTreeItem : ASCFControlTreeItem, ITreeViewItem,  IWindowExplorerTreeItem
    {
        StackPanel ITreeViewItem.Header()
        {
            //TODO: put label icon
            string ImageFileName = "@Label_16x16.png";
            return TreeViewUtils.CreateItemHeader(Name, ImageFileName);
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list = new ObservableList<Act>();

            ActASCFControl a1 = new ActASCFControl();
            a1.Description = "Get " + ASCFControlInfo.Path + " Value";
            a1.LocateBy = eLocateBy.ByName;
            a1.LocateValue = ASCFControlInfo.Path;            
            a1.ControlAction = ActASCFControl.eControlAction.GetControlProperty;
            a1.ControlProperty = ActASCFControl.eControlProperty.Value;
            list.Add(a1);

            return list;
        }
    }
}
