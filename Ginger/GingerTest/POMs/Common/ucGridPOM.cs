using Amdocs.Ginger.UserControls;
using Ginger;
using GingerWPFUnitTest.POMs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace GingerTest.POMs.Common
{
    public class ucGridPOM : GingerPOMBase
    {
        ucGrid mGrid;
        public ucGridPOM(ucGrid grid)
        {
            mGrid = grid;
        }

        // return the new rounded Add button at the top of the grid
        public UCButtonPOM EnhancedHeaderAddButton
        {
            get
            {
                ucButton button = (ucButton)FindElementByAutomationID<ucButton>(mGrid, "AddButton");
                UCButtonPOM uCButtonPOM = new UCButtonPOM(button);
                return uCButtonPOM;
            }
        }


        /// <summary>
        /// Select a row in the grid
        /// </summary>
        /// <param name="property">item property as defined in the class</param>
        /// <param name="value">text with of this property</param>
        internal void GotoRow(string property, string value)
        {            
            foreach (var item in mGrid.DataSourceList)
            {
                if (item.GetType().GetProperty(property).GetValue(item).ToString() == value)
                {
                    mGrid.DataSourceList.CurrentItem = item;                             
                    SleepWithDoEvents(100);         
                    return;
                }
            }
            
            throw new Exception("Grid item not found for: " + property + "=" + value);
        }
    }
}
