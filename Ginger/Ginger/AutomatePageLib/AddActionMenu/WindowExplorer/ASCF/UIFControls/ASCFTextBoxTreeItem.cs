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
    class ASCFTextBoxTreeItem : ASCFControlTreeItem, ITreeViewItem,  IWindowExplorerTreeItem
    {
        StackPanel ITreeViewItem.Header()
        {
            string ImageFileName = "@Label_16x16.png";
            return TreeViewUtils.CreateItemHeader(Name, ImageFileName);
        }

        ObservableList<Act> IWindowExplorerTreeItem.GetElementActions()
        {
            ObservableList<Act> list = new ObservableList<Act>();

            ActASCFControl a1 = new ActASCFControl();
            a1.Description = "Set " + ASCFControlInfo.Path + " Value";
            a1.LocateBy = eLocateBy.ByName;
            a1.LocateValue = ASCFControlInfo.Path;
            a1.Value = "ABC";
            a1.ControlAction = ActASCFControl.eControlAction.SetValue;
            list.Add(a1);

            ActASCFControl a2 = new ActASCFControl();
            a2.Description = "Get " + ASCFControlInfo.Path + " Value";
            a2.LocateBy = eLocateBy.ByName;
            a2.LocateValue = ASCFControlInfo.Path;
            a2.ControlAction = ActASCFControl.eControlAction.GetControlProperty;
            a2.ControlProperty = ActASCFControl.eControlProperty.Value;
            list.Add(a2);

            ActASCFControl a3 = new ActASCFControl();
            a3.Description = "Verify EditBox " + ASCFControlInfo.Path + " is Visible";
            a3.LocateBy = eLocateBy.ByName;
            a3.LocateValue = ASCFControlInfo.Path;
            a3.ControlAction = ActASCFControl.eControlAction.IsVisible;
            a3.ControlProperty = ActASCFControl.eControlProperty.Value;
            list.Add(a3);

            ActASCFControl a4 = new ActASCFControl();
            a4.Description = "Verify EditBox " + ASCFControlInfo.Path + " is Enabled";
            a4.LocateBy = eLocateBy.ByName;
            a4.LocateValue = ASCFControlInfo.Path;
            a4.ControlAction = ActASCFControl.eControlAction.IsEnabled;
            a4.ControlProperty = ActASCFControl.eControlProperty.Value;
            a4.AddOrUpdateReturnParamExpected("Value", "True");
            list.Add(a4);

            ActASCFControl a5 = new ActASCFControl();
            a5.Description = "Verify EditBox " + ASCFControlInfo.Path + " is Disabled";
            a5.LocateBy = eLocateBy.ByName;
            a5.LocateValue = ASCFControlInfo.Path;
            a5.ControlAction = ActASCFControl.eControlAction.IsEnabled;
            a5.ControlProperty = ActASCFControl.eControlProperty.Value;
            a5.AddOrUpdateReturnParamExpected("Value", "False");
            list.Add(a5);

            ActASCFControl a6 = new ActASCFControl();
            a6.Description = "Set Focus - " + ASCFControlInfo.Path;
            a6.LocateBy = eLocateBy.ByName;
            a6.LocateValue = ASCFControlInfo.Path;
            a6.ControlAction = ActASCFControl.eControlAction.SetFocus;    
            list.Add(a6);

            return list;
        }
    }
}
