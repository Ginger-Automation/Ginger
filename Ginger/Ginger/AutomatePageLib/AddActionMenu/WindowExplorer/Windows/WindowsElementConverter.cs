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

using Ginger.Drivers.Windows;
using GingerCore.Actions.UIAutomation;
using GingerCore.Drivers.Common;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore.Drivers;
using GingerCore.Drivers.WindowsLib;
using GingerCore.Platforms;
using Ginger.Drivers.WindowsAutomation;
using GingerCore;
using GingerCore.Actions;
using Amdocs.Ginger.Common.UIElement;

namespace Ginger.WindowExplorer.Windows
{
    public class WindowsElementConverter
    {
        internal static ITreeViewItem GetWindowsElementTreeItem(ElementInfo EI)
        {
          string elementControlType = ((WindowsDriver)EI.WindowExplorer).mUIAutomationHelper.GetElementControlType(EI.ElementObject);
          string elmentClass = ((WindowsDriver)EI.WindowExplorer).mUIAutomationHelper.GetControlPropertyValue(EI.ElementObject, "ClassName");

            if (elementControlType == "button")
                    {
                        WindowsButtonTreeItem BTI = new WindowsButtonTreeItem();
                        BTI.UIAElementInfo = (UIAElementInfo)EI;
                        BTI.UIAElementInfo.ElementObject = EI.ElementObject;
                        return BTI;
                    }
                    else if (elementControlType == "Edit Box")
                    {
                        WindowsTextBoxTreeItem TBTI = new WindowsTextBoxTreeItem();
                        TBTI.UIAElementInfo.ElementObject = EI.ElementObject;
                        TBTI.UIAElementInfo = (UIAElementInfo)EI;
                        return TBTI;
                    }
                    //Label
                    else if (elementControlType == "text")
                    {
                        WindowsLabelTreeItem WTBTI = new WindowsLabelTreeItem();
                        WTBTI.UIAElementInfo.ElementObject = EI.ElementObject;
                        WTBTI.UIAElementInfo = (UIAElementInfo)EI;
                        return WTBTI;
                    }
                    //Text box
                    else if (elementControlType == "edit")
                    {
                        WindowsTextBoxTreeItem WTBTI = new WindowsTextBoxTreeItem();
                        WTBTI.UIAElementInfo.ElementObject = EI.ElementObject;
                        WTBTI.UIAElementInfo = (UIAElementInfo)EI;
                        return WTBTI;
                    }

                    else if (elementControlType == "label")
                    {
                        WindowsLabelTreeItem WLTI = new WindowsLabelTreeItem();
                        WLTI.UIAElementInfo.ElementObject = EI.ElementObject;
                        WLTI.UIAElementInfo = (UIAElementInfo)EI;
                        return WLTI;
                    }
                    else if (elementControlType == "combo box")
                    {
                        WindowsComboBoxTreeItem CBTI = new WindowsComboBoxTreeItem();
                        CBTI.UIAElementInfo.ElementObject = EI.ElementObject;
                        CBTI.UIAElementInfo = (UIAElementInfo)EI;
                        return CBTI;
                    }
                    else if (elementControlType == "tab item")
                    {
                        WindowsTabItemTreeItem TITI = new WindowsTabItemTreeItem();
                        TITI.UIAElementInfo.ElementObject = EI.ElementObject;
                        TITI.UIAElementInfo = (UIAElementInfo)EI;
                        return TITI;
                    }
                    //TODO: For  Grid rows control type is item.This will work, but can be enhanced to use Grid patterns
                    else if (elementControlType == "item")
                    {
                        WindowsTabItemTreeItem ITI = new WindowsTabItemTreeItem();
                        ITI.UIAElementInfo.ElementObject = EI.ElementObject;
                        ITI.UIAElementInfo = (UIAElementInfo)EI;
                        return ITI;
                    }

                    else if (elementControlType == "menu item")
                    {
                        WindowsMenuItemTreeItem MBTI = new WindowsMenuItemTreeItem();
                        MBTI.UIAElementInfo.ElementObject = EI.ElementObject;
                        MBTI.UIAElementInfo = (UIAElementInfo)EI;
                        return MBTI;
                    }
            else if (elementControlType == "pane" && elmentClass == "SysDateTimePick32")
            {
                WindowsDatePickerTreeItem DPTI = new WindowsDatePickerTreeItem();
                DPTI.UIAElementInfo.ElementObject = EI.ElementObject;
                DPTI.UIAElementInfo = (UIAElementInfo)EI;
                return DPTI;
            }
            //// TODO: Remove Dependency on class name. Find a generic way
            else if (elementControlType == "pane" && elmentClass == "PBTabControl32_100")
            {
                WindowsTabItemTreeItem TTI = new WindowsTabItemTreeItem();
                TTI.UIAElementInfo.ElementObject = EI.ElementObject;
                TTI.UIAElementInfo = (UIAElementInfo)EI;
                return TTI;
            }
            else if (elementControlType == "pane" && elmentClass == "Internet Explorer_Server")
            {
                WindowsBrowserTreeItem TTI = new WindowsBrowserTreeItem();
                TTI.UIAElementInfo.ElementObject = EI.ElementObject;
                TTI.UIAElementInfo = (UIAElementInfo)EI;
                return TTI;
            }
            else if (elementControlType == "menu bar")
            {
                WindowsMenuBarTreeItem MBTI = new WindowsMenuBarTreeItem();
                MBTI.UIAElementInfo.ElementObject = EI.ElementObject;
                MBTI.UIAElementInfo = (UIAElementInfo)EI;
                return MBTI;
            }
            else if (elementControlType == "menu item")
            {
                WindowsMenuItemTreeItem MBTI = new WindowsMenuItemTreeItem();
                MBTI.UIAElementInfo.ElementObject = EI.ElementObject;
                MBTI.UIAElementInfo = (UIAElementInfo)EI;
                return MBTI;
            }
            else if (elementControlType == "check box")
            {
                WindowsCheckBoxTreeItem CBTI = new WindowsCheckBoxTreeItem();
                CBTI.UIAElementInfo.ElementObject = EI.ElementObject;
                CBTI.UIAElementInfo = (UIAElementInfo)EI;
                return CBTI;
            }
            else if (elementControlType == "radio button")
            {
                WindowsRadioButtonTreeItem RBTI = new WindowsRadioButtonTreeItem();
                RBTI.UIAElementInfo.ElementObject = EI.ElementObject;
                RBTI.UIAElementInfo = (UIAElementInfo)EI;
                return RBTI;
            }
            else if (elementControlType == "list")
            {
                WindowsListBoxTreeItem LTI = new WindowsListBoxTreeItem();
                LTI.UIAElementInfo.ElementObject = EI.ElementObject;
                LTI.UIAElementInfo = (UIAElementInfo)EI;
                return LTI;
            }
            else if (elementControlType == "list item")
            {
                WindowsListItemTreeItem LITI = new WindowsListItemTreeItem();
                LITI.UIAElementInfo.ElementObject = EI.ElementObject;
                LITI.UIAElementInfo = (UIAElementInfo)EI;
                return LITI;
            }
            else if (elementControlType == "Dialog")
            {
                WindowsDialogBoxTreeItem DBTI = new WindowsDialogBoxTreeItem();
                DBTI.UIAElementInfo.ElementObject = EI.ElementObject;
                DBTI.UIAElementInfo = (UIAElementInfo)EI;
                return DBTI;
            }
            //    // TODO: add all other types of controls: List, label etc...
            else
            {
                WindowsControlTreeItem TVIChild = new WindowsControlTreeItem();
                TVIChild.UIAElementInfo.ElementObject = EI.ElementObject;
                TVIChild.UIAElementInfo = (UIAElementInfo)EI;
                return TVIChild;
            }
        }
    }
}
