using Ginger.Drivers.PowerBuilder;
using Ginger.WindowExplorer.Appium;
using Ginger.WindowExplorer.HTMLCommon;
using Ginger.WindowExplorer.Java;
using Ginger.WindowExplorer.Windows;
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

    }
}
