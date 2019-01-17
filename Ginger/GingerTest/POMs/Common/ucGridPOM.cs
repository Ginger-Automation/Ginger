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

                        if (checkbox.IsChecked == true)
                        {
                            checkbox.IsChecked = false;
                        }
                        else
                        {
                            checkbox.IsChecked = true;
                        }
                    });
                }
            }
        }
    }
}
