using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GingerTest.WizardLib
{
    public class MyWizardItem 
    {
        // Auto set to true
        public bool Active { get; set; }

        // Mandatory, with naming rules
        public string Name { get; set; }

        // Optional
        public string Description { get; set; }

        
        public enum eColor
        {
            Red,
            Green,
            Blue
        }

        // Mandatory
        public eColor Color { get; set; }

        // Must have at least one row
        public ObservableList<MyWizardSubItem> SubItems = new ObservableList<MyWizardSubItem>();

    }
}
