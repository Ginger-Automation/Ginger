using Amdocs.Ginger.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ginger.TwoLevelMenuLib
{
    public class TwoLevelMenu
    {
        public ObservableList<TopMenuItem> MenuList = new ObservableList<TopMenuItem>();

        internal void Add(TopMenuItem topMenuItem)
        {            
            MenuList.Add(topMenuItem);
        }

        internal void Reset()
        {
            foreach (TopMenuItem topMenuItem in MenuList)
            {
                foreach(SubMenuItem subMenuItem in topMenuItem.SubItems)
                {
                    subMenuItem.ResetPage();
                }
            }
        }
    }
}
