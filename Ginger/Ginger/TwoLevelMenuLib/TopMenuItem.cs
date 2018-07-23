using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ginger.TwoLevelMenuLib
{
    public class TopMenuItem
    {
        public string Name { get; set; }        

        public bool Active { get; set; }

        public ConsoleKey Key { get; set; }

        public string AutomationID { get; set; }

        public TopMenuItem(string name, ConsoleKey key, string automationID)
        {
            Key = key;
            Name = name;            
            Active = true;
            AutomationID = automationID;
        }

        public ObservableList<SubMenuItem> SubItems = new ObservableList<SubMenuItem>();

        internal void Add(string name, Func<Page> pageFunc, ConsoleKey k, string toolTip, string AutomationID)
        {
            SubItems.Add(new SubMenuItem(name, pageFunc, k, toolTip, AutomationID));
        }
    }
}
