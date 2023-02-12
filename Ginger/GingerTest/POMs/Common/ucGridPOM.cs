#region License
/*
Copyright © 2014-2023 European Support Limited

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
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

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
                    return;
                }
            }
            
            throw new Exception("Grid item not found for: " + property + "=" + value);
        }

        public void ClickOnCheckBox(string checkboxHeaderValue, string fieldToSearchOnHeader, string fieldValueToSearch)
        {
            foreach (var item in mGrid.DataSourceList)
            {
                string actualFieldName = item.GetType().GetProperty(fieldToSearchOnHeader).GetValue(item).ToString();
                if (actualFieldName == fieldValueToSearch)
                {
                    DataGridCellsPresenter presenter = null;
                    CheckBox checkbox = null;
                 

                    Execute(() =>
                    {
                        mGrid.Grid.SelectedItem = item;
                        mGrid.ScrollToViewCurrentItem();
                    });

                    SleepWithDoEvents(1000);
                    DataGridRow row = mGrid.Grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                    Execute(() =>
                    {
                        presenter = General.GetVisualChild<DataGridCellsPresenter>(row);
                        object a = presenter.ItemContainerGenerator.Items[0].GetType().GetProperty(checkboxHeaderValue).GetValue(presenter.ItemContainerGenerator.Items[0]);
                        if (a != null)
                        {
                            DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(0);
                            checkbox = General.GetVisualChild<CheckBox>(cell);
                        }

                        checkbox.IsChecked = !checkbox.IsChecked;
                    });
                }
            }
        }

        public void ReOrderGridRows(string fieldToSearchOnHeader, string fieldValueToSearch, int newIndex)
        {
            foreach (var item in mGrid.DataSourceList)
            {
                string actualFieldName = item.GetType().GetProperty(fieldToSearchOnHeader).GetValue(item).ToString();
                if (actualFieldName == fieldValueToSearch)
                {
                    Execute(() =>
                    {
                        mGrid.Grid.SelectedItem = item;
                        mGrid.ScrollToViewCurrentItem();
                    });

                    Execute(() =>
                    {
                        int currentIndex = mGrid.Grid.Items.IndexOf(item);
                        mGrid.DataSourceList.Move(currentIndex, newIndex);
                        mGrid.ScrollToViewCurrentItem();
                    });

                    break;
                }
            }
        }
    }
}
