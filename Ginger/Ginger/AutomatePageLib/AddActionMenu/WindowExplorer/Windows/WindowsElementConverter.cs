#region License
/*
Copyright © 2014-2023 European Support Limited

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
            if (EI.ElementTypeEnum == eElementType.Button)
            {
                WindowsButtonTreeItem BTI = new WindowsButtonTreeItem();
                BTI.UIAElementInfo = (UIAElementInfo)EI;
                BTI.UIAElementInfo.ElementObject = EI.ElementObject;
                return BTI;
            }
            else if (EI.ElementTypeEnum == eElementType.TextBox)
            {
                WindowsTextBoxTreeItem TBTI = new WindowsTextBoxTreeItem();
                TBTI.UIAElementInfo.ElementObject = EI.ElementObject;
                TBTI.UIAElementInfo = (UIAElementInfo)EI;
                return TBTI;
            }
            else if (EI.ElementTypeEnum == eElementType.Label)
            {
                WindowsLabelTreeItem WTBTI = new WindowsLabelTreeItem();
                WTBTI.UIAElementInfo.ElementObject = EI.ElementObject;
                WTBTI.UIAElementInfo = (UIAElementInfo)EI;
                return WTBTI;
            }
            else if (EI.ElementTypeEnum == eElementType.ComboBox)
            {
                WindowsComboBoxTreeItem CBTI = new WindowsComboBoxTreeItem();
                CBTI.UIAElementInfo.ElementObject = EI.ElementObject;
                CBTI.UIAElementInfo = (UIAElementInfo)EI;
                return CBTI;
            }
            else if (EI.ElementTypeEnum == eElementType.Tab)
            {
                WindowsTabItemTreeItem TITI = new WindowsTabItemTreeItem();
                TITI.UIAElementInfo.ElementObject = EI.ElementObject;
                TITI.UIAElementInfo = (UIAElementInfo)EI;
                return TITI;
            }
            else if (EI.ElementTypeEnum == eElementType.MenuItem)
            {
                WindowsMenuItemTreeItem MBTI = new WindowsMenuItemTreeItem();
                MBTI.UIAElementInfo.ElementObject = EI.ElementObject;
                MBTI.UIAElementInfo = (UIAElementInfo)EI;
                return MBTI;
            }
            else if (EI.ElementTypeEnum == eElementType.DatePicker)
            {
                WindowsDatePickerTreeItem DPTI = new WindowsDatePickerTreeItem();
                DPTI.UIAElementInfo.ElementObject = EI.ElementObject;
                DPTI.UIAElementInfo = (UIAElementInfo)EI;
                return DPTI;
            }
            else if (EI.ElementTypeEnum == eElementType.Browser)
            {
                WindowsBrowserTreeItem TTI = new WindowsBrowserTreeItem();
                TTI.UIAElementInfo.ElementObject = EI.ElementObject;
                TTI.UIAElementInfo = (UIAElementInfo)EI;
                return TTI;
            }
            else if (EI.ElementTypeEnum == eElementType.MenuBar)
            {
                WindowsMenuBarTreeItem MBTI = new WindowsMenuBarTreeItem();
                MBTI.UIAElementInfo.ElementObject = EI.ElementObject;
                MBTI.UIAElementInfo = (UIAElementInfo)EI;
                return MBTI;
            }
            else if (EI.ElementTypeEnum == eElementType.CheckBox)
            {
                WindowsCheckBoxTreeItem CBTI = new WindowsCheckBoxTreeItem();
                CBTI.UIAElementInfo.ElementObject = EI.ElementObject;
                CBTI.UIAElementInfo = (UIAElementInfo)EI;
                return CBTI;
            }
            else if (EI.ElementTypeEnum == eElementType.RadioButton)
            {
                WindowsRadioButtonTreeItem RBTI = new WindowsRadioButtonTreeItem();
                RBTI.UIAElementInfo.ElementObject = EI.ElementObject;
                RBTI.UIAElementInfo = (UIAElementInfo)EI;
                return RBTI;
            }
            else if (EI.ElementTypeEnum == eElementType.List)
            {
                WindowsListBoxTreeItem LTI = new WindowsListBoxTreeItem();
                LTI.UIAElementInfo.ElementObject = EI.ElementObject;
                LTI.UIAElementInfo = (UIAElementInfo)EI;
                return LTI;
            }
            else if (EI.ElementTypeEnum == eElementType.ListItem)
            {
                WindowsListItemTreeItem LITI = new WindowsListItemTreeItem();
                LITI.UIAElementInfo.ElementObject = EI.ElementObject;
                LITI.UIAElementInfo = (UIAElementInfo)EI;
                return LITI;
            }
            else if (EI.ElementTypeEnum == eElementType.Dialog)
            {
                WindowsDialogBoxTreeItem DBTI = new WindowsDialogBoxTreeItem();
                DBTI.UIAElementInfo.ElementObject = EI.ElementObject;
                DBTI.UIAElementInfo = (UIAElementInfo)EI;
                return DBTI;
            }          
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
