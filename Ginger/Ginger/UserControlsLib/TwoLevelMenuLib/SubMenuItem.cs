using Amdocs.Ginger.Common.Enums;
using System;
using System.Windows.Controls;

namespace Ginger.TwoLevelMenuLib
{
    public class SubMenuItem: MenuItemBase
    {       
        Page mItemPage = null;
        public Page ItemPage
        {
            get
            {
                if (mItemPage==null)
                {
                    mItemPage = GetMenuItemPage();
                }
                return mItemPage;
            }
        }

        public bool IsPageLoaded
        {
            get
            {
                if (mItemPage != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        internal void ResetPage()
        {
            mItemPage = null;
        }

        public Func<Page> GetMenuItemPage { get; }               

        public SubMenuItem(eImageType iconType, string name, Func<Page> action, ConsoleKey key, string toolTip, string automationID)
        {
            IconType = iconType;
            Key = key;
            Name = name;
            GetMenuItemPage = action;
            ToolTip = toolTip;
            AutomationID = automationID;
            Active = true;
        }       
    }
}
