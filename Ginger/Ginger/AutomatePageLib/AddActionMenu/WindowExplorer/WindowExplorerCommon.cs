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
using Amdocs.Ginger.Common.UIElement;
using Ginger.Drivers.PowerBuilder;
using Ginger.WindowExplorer.Appium;
using Ginger.WindowExplorer.HTMLCommon;
using Ginger.WindowExplorer.Java;
using Ginger.WindowExplorer.Windows;
using GingerCore.Actions.Common;
using GingerCore.Actions.UIAutomation;
using GingerCore.Drivers.Appium;
using GingerCore.Drivers.Common;
using GingerCore.Drivers.JavaDriverLib;
using GingerCore.Drivers.PBDriver;
using GingerWPF.UserControlsLib.UCTreeView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.BusinessFlowsLibNew.AddActionMenu
{
    class WindowExplorerCommon
    {
        public static ITreeViewItem GetTreeViewItemForElementInfo(Amdocs.Ginger.Common.UIElement.ElementInfo EI)
        {
            if (EI == null) return null; // can happen when grid is filtered

            //TODO: make it OO style avoid the if else if
            ITreeViewItem TVI = null;
            if (EI is JavaElementInfo)
            {
                TVI = JavaElementInfoConverter.GetTreeViewItemFor(EI);
            }
            else if (EI is UIAElementInfo)
            {
                UIAElementInfo UEI = (UIAElementInfo)EI;
                if (UEI.WindowExplorer.GetType() == typeof(PBDriver))
                {
                    //TODO:  Below will work for now. But need to Implement element info
                    TVI = PBControlTreeItemBase.GetMatchingPBTreeItem(UEI);
                }
                else
                {
                    TVI = WindowsElementConverter.GetWindowsElementTreeItem(EI);
                }
            }
            else if (EI is AppiumElementInfo)
            {
                TVI = AppiumElementInfoConverter.GetTreeViewItemFor(EI);
            }
            else if (EI is HTMLElementInfo)
            {
                TVI = HTMLElementInfoConverter.GetHTMLElementTreeItem(((HTMLElementInfo)EI));
            }
            else
            {
                //TODO: err?
                return null;
            }

            return TVI;
        }

        public static void CreateActUISwitchWindowAction(string windowTitle, Amdocs.Ginger.Common.Context mContext)
        {
            ActUIElement actUIElement = new ActUIElement()
            {
                Description = "UI Switch Window - " + windowTitle,
                ElementLocateBy = eLocateBy.ByTitle,
                ElementLocateValue = windowTitle,
                ElementType = eElementType.Window,
                ElementAction = ActUIElement.eElementAction.Switch
            };
            actUIElement.GetOrCreateInputParam(ActUIElement.Fields.SyncTime, "30");

            if (mContext.BusinessFlow != null)
            {
                mContext.BusinessFlow.AddAct(actUIElement, true);
            }
            else
            {
                Reporter.ToUser(eUserMsgKey.RestartAgent);
            }
        }
    }
}
