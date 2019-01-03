using Amdocs.Ginger.Common.UIElement;
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
                }
            }
            
            throw new Exception("Grid item not found for: " + property + "=" + value);
        }

        public void ClickOnCheckBox(string checkboxHeaderValue, string fieldToSearchOnHeader, string fieldValueToSearch)
        {
            try
            {
                int i = 0;


                foreach (var item in mGrid.DataSourceList)
                {

                    string actualFieldName = item.GetType().GetProperty(fieldToSearchOnHeader).GetValue(item).ToString();
                    if (actualFieldName == fieldValueToSearch)
                    {
                        DataGridCellsPresenter presenter = null;
                        CheckBox checkbox = null;
                        DataGridRow row = mGrid.Grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;

                        //object a = row.GetType().GetProperty(checkboxHeaderValue).GetValue(row);

                        Execute(() =>
                        {
                            presenter = General.GetVisualChild<DataGridCellsPresenter>(row);

                            object a = presenter.ItemContainerGenerator.Items[0].GetType().GetProperty(checkboxHeaderValue).GetValue(presenter.ItemContainerGenerator.Items[0]);



                            if (a != null)
                            {
                                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(0);
                                checkbox = General.GetVisualChild<CheckBox>(cell);
                            }

                            //((DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(1)).TabIndex;


 

                        });

                        if (checkbox.IsChecked == true)
                        {
                            checkbox.IsChecked = false;
                        }
                        else
                        {
                            checkbox.IsChecked = true;
                        }
                    }
                }


            }
            catch (Exception ex)
            {

            }
           
        }


                //DataGridCellsPresenter presenter = null;
                //DataGridRow row = mGrid.Grid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                //Execute(() =>
                //{
                //    presenter = General.GetVisualChild<DataGridCellsPresenter>(row);
                //});

                //try
                //{

                //    CheckBox o = null;

                //    Execute(() =>
                //    {
                //        DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(0);

                //        o = General.GetVisualChild<CheckBox>(cell);
                //        ((CheckBox)o).IsChecked = false;
                //    });
                   
                //}
                //catch (Exception ex)
                //{

                //}


                //foreach (UIElementFilter cell in presenter.ItemContainerGenerator.Items)
                //{
                //    //Object o1 = null;
                //    //Execute(() =>
                //    //{
                //    //    o1 = General.GetVisualChild<CheckBox>(cell);
                //    //});

                //    //object o = cell.GetType().GetProperty(fieldToSearchOnHeader);

                //}
           



            //foreach (DataGridRow row in mGrid.GetDataGridRows(mGrid.Grid))
            //{
            //    try
            //    {

            //        CheckBox cb = (CheckBox)mGrid.GetDataTemplateCellControl<CheckBox>(row, 0);
            //    }
            //    catch
            //    {

            //    }



            //    DataGridCell cell = null;
                
            //    Execute(() => {
            //        cell = mGrid.GetDataGridCell(row, i);

            //    });
                
                
            //    i++;
            //}

            //foreach (var item in mGrid.DataSourceList)
            //{
            //    //if (item.GetType().GetProperty(property).GetValue(item).ToString() == value)
            //    //{
            //    //    mGrid.GetDataGridRows(mGrid.Grid);
            //    //}
            //}
        
    }
}
