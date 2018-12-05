#region License
/*
Copyright © 2014-2018 European Support Limited

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
