using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Ginger.TwoLevelMenuLib
{
    public class TopMenuItem: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        public string Name { get; set; }        

        public bool Active { get; set; }

        public string ToolTip { get; set; }

        public ConsoleKey Key { get; set; }

        public string AutomationID { get; set; }

        public eImageType IconType { get; set; }

        public ObservableList<ListViewItem> LoadedSubItems { get; set; }        

        public TopMenuItem(eImageType iconType, string name, ConsoleKey key, string automationID, string toolTip="")
        {
            IconType = iconType;
            Key = key;
            Name = name;            
            Active = true;
            AutomationID = automationID;
            ToolTip = toolTip;
        }

        public ObservableList<SubMenuItem> SubItems = new ObservableList<SubMenuItem>();

        internal void Add(string name, Func<Page> pageFunc, ConsoleKey k, string toolTip, string AutomationID)
        {
            SubItems.Add(new SubMenuItem(name, pageFunc, k, toolTip, AutomationID));
        }
    }
}
