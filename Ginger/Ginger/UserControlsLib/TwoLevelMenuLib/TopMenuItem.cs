using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using System;
using System.Windows.Controls;

namespace Ginger.TwoLevelMenuLib
{
    public class TopMenuItem: MenuItemBase
    {
        public ObservableList<SubMenuItem> SubItems = new ObservableList<SubMenuItem>();

        public SubMenuItem LastSubMenuItem;

        public TopMenuItem(eImageType iconType, string name, ConsoleKey key, string automationID, string toolTip="")
        {
            IconType = iconType;
            Key = key;
            Name = name;            
            Active = true;
            AutomationID = automationID;
            ToolTip = toolTip;
        }
        
        internal void Add(eImageType iconType, string name, Func<Page> pageFunc, ConsoleKey k, string toolTip, string AutomationID)
        {
            SubItems.Add(new SubMenuItem(iconType, name, pageFunc, k, toolTip, AutomationID));
        }
    }
}
