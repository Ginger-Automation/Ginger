#region License
/*
Copyright © 2014-2024 European Support Limited

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
    class ASCFTextBoxTreeItem : ASCFControlTreeItem, ITreeViewItem, IWindowExplorerTreeItem
    {
        StackPanel ITreeViewItem.Header()
        {
            string ImageFileName = "@Label_16x16.png";
            return TreeViewUtils.CreateItemHeader(Name, ImageFileName);
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list = [];

            ActASCFControl a1 = new ActASCFControl
            {
                Description = "Set " + ASCFControlInfo.Path + " Value",
                LocateBy = eLocateBy.ByName,
                LocateValue = ASCFControlInfo.Path,
                Value = "ABC",
                ControlAction = ActASCFControl.eControlAction.SetValue
            };
            list.Add(a1);

            ActASCFControl a2 = new ActASCFControl
            {
                Description = "Get " + ASCFControlInfo.Path + " Value",
                LocateBy = eLocateBy.ByName,
                LocateValue = ASCFControlInfo.Path,
                ControlAction = ActASCFControl.eControlAction.GetControlProperty,
                ControlProperty = ActASCFControl.eControlProperty.Value
            };
            list.Add(a2);

            ActASCFControl a3 = new ActASCFControl
            {
                Description = "Verify EditBox " + ASCFControlInfo.Path + " is Visible",
                LocateBy = eLocateBy.ByName,
                LocateValue = ASCFControlInfo.Path,
                ControlAction = ActASCFControl.eControlAction.IsVisible,
                ControlProperty = ActASCFControl.eControlProperty.Value
            };
            list.Add(a3);

            ActASCFControl a4 = new ActASCFControl
            {
                Description = "Verify EditBox " + ASCFControlInfo.Path + " is Enabled",
                LocateBy = eLocateBy.ByName,
                LocateValue = ASCFControlInfo.Path,
                ControlAction = ActASCFControl.eControlAction.IsEnabled,
                ControlProperty = ActASCFControl.eControlProperty.Value
            };
            a4.AddOrUpdateReturnParamExpected("Value", "True");
            list.Add(a4);

            ActASCFControl a5 = new ActASCFControl
            {
                Description = "Verify EditBox " + ASCFControlInfo.Path + " is Disabled",
                LocateBy = eLocateBy.ByName,
                LocateValue = ASCFControlInfo.Path,
                ControlAction = ActASCFControl.eControlAction.IsEnabled,
                ControlProperty = ActASCFControl.eControlProperty.Value
            };
            a5.AddOrUpdateReturnParamExpected("Value", "False");
            list.Add(a5);

            ActASCFControl a6 = new ActASCFControl
            {
                Description = "Set Focus - " + ASCFControlInfo.Path,
                LocateBy = eLocateBy.ByName,
                LocateValue = ASCFControlInfo.Path,
                ControlAction = ActASCFControl.eControlAction.SetFocus
            };
            list.Add(a6);

            return list;
        }
    }
}
