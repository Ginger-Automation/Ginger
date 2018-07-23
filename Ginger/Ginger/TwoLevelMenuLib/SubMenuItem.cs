using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ginger.TwoLevelMenuLib
{
    public class SubMenuItem
    {
        public string Name { get; set; }

        // function which returen the page to show

        // item page is cached
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

        public bool IsPageLoaded { get { if (mItemPage != null) return true; else return false; } }

        internal void ResetPage()
        {
            mItemPage = null;
        }

        public Func<Page> GetMenuItemPage { get; }
        
        public bool Active { get; set; }

        // Hot key to get to this menu
        public ConsoleKey Key { get; set; }

        public string ToolTip { get; set; }

        public string AutomationID { get; set; }

        public SubMenuItem(string name, Func<Page> action, ConsoleKey key, string toolTip, string automationID)
        {
            Key = key;
            Name = name;
            GetMenuItemPage = action;
            ToolTip = toolTip;
            AutomationID = automationID;
            Active = true;
        }

        public List<TopMenuItem> SubItems = new List<TopMenuItem>();
    }
}
