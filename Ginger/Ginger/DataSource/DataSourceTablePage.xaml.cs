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

using Amdocs.Ginger.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Ginger.UserControls;
using GingerCore;
using GingerCore.DataSource;
using GingerCore.GeneralLib;
using System;

namespace Ginger.DataSource
{
    /// <summary>
    /// Interaction logic for AgentEditPage.xaml
    /// </summary>
    public partial class DataSourceTablePage : Page
    {
        DataSourceTable mDSTableDetails;
        Setter mCellSetter=null;
        List<string> mColumnNames = new List<string>();
        Setter SetterModified = new Setter(Border.BorderBrushProperty, Brushes.Orange);
        Setter SetterError = new Setter(Border.BorderBrushProperty, Brushes.Red);
        Setter SetterBorderBold = new Setter(Border.BorderThicknessProperty, new Thickness(2, 2, 2, 2));
        Setter SetterBorder = new Setter(Border.BorderThicknessProperty, new Thickness(1, 1, 1, 1));
        int i = 0;
        
        public DataSourceTablePage(DataSourceTable dsTableDetails)
        {
            InitializeComponent();

            if (dsTableDetails != null)    
            {
                mDSTableDetails = dsTableDetails;
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DataSourceTableNameTextBox, TextBox.TextProperty, dsTableDetails, DataSourceTable.Fields.Name);
                GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(DataSourceTableType, TextBox.TextProperty, dsTableDetails, DataSourceTable.Fields.DSTableType, BindingMode.OneWay);
                
                SetGridView();
                SetGridData();
                grdTableData.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddRow));
                grdTableData.btnDelete.AddHandler(Button.ClickEvent, new RoutedEventHandler(DeleteRow));               
                grdTableData.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshTable));

                //Handle Table Actions
                if (mColumnNames.Contains("GINGER_USED"))
                {
                    grdTableData.btnMarkAll.Visibility = Visibility.Visible;
                    grdTableData.MarkUnMarkAllActive += MarkUnMarkAllUsed;
                }

                grdTableData.AddToolbarTool("@Undo_16x16.png", "Undo Local Changes", new RoutedEventHandler(UndoTableChanges));
                grdTableData.AddToolbarTool("@Copy_16x16.png", "Duplicate Row", new RoutedEventHandler(DuplicateRow));
                grdTableData.AddToolbarTool("@Trash_16x16.png", "Delete All Rows", new RoutedEventHandler(DeleteAll));


                if (dsTableDetails.DSTableType == DataSourceTable.eDSTableType.Customized)
                {
                    grdTableData.AddToolbarTool("@AddTableColumn_16x16.png", "Add Table Column", new RoutedEventHandler(AddColumn));
                    grdTableData.AddToolbarTool("@DeleteTableColumn_16x16.png", "Remove Table Column", new RoutedEventHandler(RemoveColumn));
                }               
                grdTableData.AddToolbarTool("@Commit_16x16.png", "Commit", new RoutedEventHandler(SaveTable));
                grdTableData.Grid.LostFocus += Grid_LostFocus;                         
            }            
        }
        
        private void Grid_LostFocus(object sender, RoutedEventArgs e)
        {   
            DataGridCell cell=null;
            if (e.OriginalSource.GetType() == typeof(CheckBox))
            { 
                cell = (DataGridCell)((CheckBox)e.OriginalSource).Parent;
            }
            else if (e.OriginalSource.GetType() == typeof(TextBox))
            { 
                cell = (DataGridCell)((TextBox)e.OriginalSource).Parent;
            }
            else if (e.OriginalSource.GetType() == typeof(DataGridCell))
            { 
                cell = (DataGridCell)e.OriginalSource;
            }

            if (cell == null)
            {
                return;
            }

            Style cellstyle = new Style(typeof(DataGridCell));
            if (mDSTableDetails.DSTableType == DataSourceTable.eDSTableType.GingerKeyValue)
            {      
                string keyName = ((DataRowView)cell.DataContext).Row["GINGER_KEY_NAME"].ToString();
                if (cell.Column.Header.ToString() == "GINGER__KEY__NAME" && (keyName == "" || getKeyRows(keyName).Count() > 1))
                {
                    if (keyName == "")                    
                        cellstyle.Setters.Add(new Setter(DataGridRow.ToolTipProperty, "Empty Key Name"));                      
                    else                   
                        cellstyle.Setters.Add(new Setter(DataGridRow.ToolTipProperty, "Duplicate Key Name"));                       
                    
                    cellstyle.Setters.Remove(SetterModified);
                    cellstyle.Setters.Add(SetterError);
                    cellstyle.Setters.Add(SetterBorderBold);
                    cell.Style = cellstyle;                    
                    return;
                }
                else if(((DataRowView)cell.DataContext).Row.RowError == "Duplicate Key Name")
                {
                    DataRow[] foundRows;
                    foundRows = mDSTableDetails.DataTable.Select("GINGER_KEY_NAME='" + keyName + "'");
                    if (foundRows.Count() < 2)
                    {
                        foreach (DataRow dr1 in foundRows)
                            dr1.RowError = "";
                    }
                }
            }
            string colName = "";
            if (cell.Column.Header != null)                
                colName = cell.Column.Header.ToString().Replace("__","_");
            if (((DataRowView)cell.DataContext).Row.RowState == DataRowState.Added || ((DataRowView)cell.DataContext).Row[colName].ToString() != ((DataRowView)cell.DataContext).Row[colName, DataRowVersion.Original].ToString())
            {       
                ((DataRowView)cell.DataContext).Row.RowError = "";
                cellstyle.Setters.Add(new Setter(DataGridRow.ToolTipProperty, ""));
                cellstyle.Setters.Add(SetterModified);
                cellstyle.Setters.Add(SetterBorderBold);                
            }
            else
            {
                ((DataRowView)cell.DataContext).Row.RowError = "";
                cellstyle.Setters.Add(new Setter(DataGridRow.ToolTipProperty, ""));
                cellstyle.Setters.Remove(SetterModified);
                cellstyle.Setters.Remove(SetterBorderBold);               
            }
            
            cell.Style = cellstyle;
        }    
        
        private ObservableList<DataRow> getKeyRows(string keyName)
        {            
            ObservableList<DataRow> mDataRow = new ObservableList<DataRow>();
            foreach (DataGridRow row in grdTableData.GetDataGridRows(grdTableData.Grid))
            {
                if (((DataRowView)(row.DataContext)).Row["GINGER_KEY_NAME"].ToString() == keyName)
                {
                    mDataRow.Add(((DataRowView)(row.DataContext)).Row);
                }
            }
            return mDataRow;
        }

        #region Functions
        private void SetGridView(bool bUpdate=false)
        {
            //Set the grid name
            grdTableData.Title = "'" + mDSTableDetails.Name + "' Table Data";
            grdTableData.ShowViewCombo = Visibility.Collapsed;
            mColumnNames = mDSTableDetails.DSC.GetColumnList(mDSTableDetails.Name);
            
            int iColIndex = mColumnNames.Count - 1;
            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            foreach (string colName in mColumnNames)
            {                
                string colHeader= colName.Replace("_","__");
                if (colName == "GINGER_ID")
                {
                    view.GridColsView.Add(new GridColView() { Field = colName, Header= colHeader,Order = 0, WidthWeight = 10, BindingMode = BindingMode.OneWay });
                }
                else if (colName == "GINGER_LAST_UPDATE_DATETIME" || colName == "GINGER_LAST_UPDATED_BY")
                {
                    view.GridColsView.Add(new GridColView() { Field = colName, Header = colHeader, WidthWeight = 20, BindingMode = BindingMode.OneWay });                        
                } 
                else if (colName == "GINGER_USED")
                {
                    view.GridColsView.Add(new GridColView() { Field = colName, Header = colHeader, WidthWeight = 20, StyleType = GridColView.eGridColStyleType.CheckBox });
                }
                else
                {
                    view.GridColsView.Add(new GridColView() { Field = colName, Header = colHeader, WidthWeight = 30 });
                }
            }

            if (bUpdate == false)
            {
                grdTableData.SetAllColumnsDefaultView(view);
                grdTableData.InitViewItems();
            }
            else
            {
                grdTableData.updateAndSelectCustomView(view);
            }            
           
            foreach (DataGridColumn sCol in grdTableData.grdMain.Columns)
            {
                if (sCol.IsReadOnly == true)
                {
                    sCol.CellStyle = new Style(typeof(DataGridCell));
                    mCellSetter = new Setter(DataGridCell.BackgroundProperty, new SolidColorBrush(Colors.LightGray));
                    sCol.CellStyle.Setters.Add(mCellSetter);
                }
                if (sCol.Header.ToString() == "GINGER__USED")
                {
                        sCol.DisplayIndex = 1;
                }
                if (sCol.Header.ToString() == "GINGER__LAST__UPDATED__BY" || sCol.Header.ToString() == "GINGER__LAST__UPDATE__DATETIME")
                {
                    if (sCol.DisplayIndex != -1)
                    {
                        sCol.DisplayIndex = iColIndex;
                        iColIndex--;
                    }
                    else
                    {
                        sCol.DisplayIndex = iColIndex;
                        iColIndex--;
                    }
                }                    
            }          
        }

        private void SetGridData()
        {
                mDSTableDetails.DataTable = mDSTableDetails.DSC.GetTable(mDSTableDetails.Name);

                grdTableData.Grid.SetBinding(ItemsControl.ItemsSourceProperty, new Binding
                {
                    Source = mDSTableDetails.DataTable
                });
                grdTableData.UseGridWithDataTableAsSource(mDSTableDetails.DataTable, false);
           
        }
        public void RefreshGrid()
        {
            SetGridView(true);
            SetGridData();
        }
        private void MarkUnMarkAllUsed(bool usedStatus)
        {
            foreach (object oRow in grdTableData.Grid.Items)
                ((DataRowView)oRow).Row["GINGER_USED"] = usedStatus;                          
        }

        private void AddRow(object sender, RoutedEventArgs e)
        {
            mDSTableDetails.DSC.AddRow(mColumnNames, mDSTableDetails);
                        
        }

        private void DeleteRow(object sender, RoutedEventArgs e)
        {
            if (grdTableData.Grid.SelectedItems.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.SelectItemToDelete);
                return;
            }
            List<object> SelectedItemsList = grdTableData.Grid.SelectedItems.Cast<object>().ToList();
            foreach (object o in SelectedItemsList)
            {
                ((DataRowView)o).Delete();               
            }                 
        }

        private void DeleteAll(object sender, RoutedEventArgs e)
        {
            if (grdTableData.Grid.Items.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.NoItemToDelete);
                return;
            }
            if ((Reporter.ToUser(eUserMsgKey.SureWantToDeleteAll)) == Amdocs.Ginger.Common.eUserMsgSelection.Yes)
            {
                List<object> AllItemsList = grdTableData.Grid.Items.Cast<object>().ToList();
                mDSTableDetails.DSC.DeleteAll(AllItemsList, mDSTableDetails.Name);
                Reporter.ToUser(eUserMsgKey.StaticInfoMessage, "Click 'Commit' to reflect the changes in the Database Or 'Refresh' to get the deleted rows back in the view.");
                //RefreshGrid();
            }            
        }
        private void DuplicateRow(object sender, RoutedEventArgs e)
        {
            if (grdTableData.Grid.SelectedItems.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.AskToSelectItem);
                return;
            }
            List<object> SelectedItemsList = grdTableData.Grid.SelectedItems.Cast<object>().ToList();
            mDSTableDetails.DSC.DuplicateRow(mColumnNames, SelectedItemsList, mDSTableDetails);
                             
        }
        
        private void AddColumn(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.SaveLocalChanges) == Amdocs.Ginger.Common.eUserMsgSelection.No)
            {
                return;
            }
            AddNewTableColumnPage ANTCP = new AddNewTableColumnPage();
            ANTCP.ShowAsWindow();

            DataSourceTableColumn dsTableColumn = ANTCP.DSTableCol;
            if (dsTableColumn != null)
            {
                SaveTable();
                mDSTableDetails.DSC.AddColumn(mDSTableDetails.Name, dsTableColumn.Name, "Text");

                RefreshGrid();
                mColumnNames.Add(dsTableColumn.Name);
                if(dsTableColumn.Name == "GINGER_USED")
                {
                    grdTableData.btnMarkAll.Visibility = Visibility.Visible;
                    grdTableData.MarkUnMarkAllActive += MarkUnMarkAllUsed;
                }                
            }
        }

        private void RemoveColumn(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.SaveLocalChanges) == Amdocs.Ginger.Common.eUserMsgSelection.No)
            {
                return;
            }
            RemoveTableColumnPage RTCP = new RemoveTableColumnPage(mDSTableDetails.DSC.GetColumnList(mDSTableDetails.Name));
            RTCP.ShowAsWindow();

            DataSourceTableColumn dsTableColumn = RTCP.DSTableCol;
            if (dsTableColumn != null)
            {
                SaveTable();
                mDSTableDetails.DSC.RemoveColumn(mDSTableDetails.Name, dsTableColumn.Name);
                SetGridView(true);
                mColumnNames.Remove(dsTableColumn.Name);
                if (dsTableColumn.Name == "GINGER_USED")
                {
                    grdTableData.btnMarkAll.Visibility = Visibility.Collapsed;
                }
            }            
        }

        private void UndoTableChanges(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.SaveLocalChanges) == Amdocs.Ginger.Common.eUserMsgSelection.No)
            {
                return;
            }
            mDSTableDetails.DataTable.RejectChanges();
            mDSTableDetails.DirtyStatus = Amdocs.Ginger.Common.Enums.eDirtyStatus.NoChange;
        }

        private void RefreshTable(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.SaveLocalChanges) == Amdocs.Ginger.Common.eUserMsgSelection.No)
            {
                return;
            }
            RefreshGrid();
            mDSTableDetails.DirtyStatus = Amdocs.Ginger.Common.Enums.eDirtyStatus.NoChange;
        }

        private void SaveTable(object sender, RoutedEventArgs e)
        {
            grdTableData.Grid.SelectedItems.Clear();
            SaveTable();            
        }

        public void SaveTable()
        {
            grdTableData.Grid.SelectedItems.Clear();
            if (TableValidation() == false)
                return;
            if (grdTableData.Grid.CurrentItem != null)
                ((DataRowView)grdTableData.Grid.CurrentItem).EndEdit();
            grdTableData.Grid.CommitEdit();
            mDSTableDetails.DSC.SaveTable(mDSTableDetails.DataTable);            
            Reporter.ToStatus(eStatusMsgKey.SaveItem,null, mDSTableDetails.Name, "Data Source Table");
            SetGridData();
            mDSTableDetails.DirtyStatus = Amdocs.Ginger.Common.Enums.eDirtyStatus.NoChange;
            Reporter.HideStatusMessage();
        }

        private bool TableValidation()
        {           
            bool status = true;
            if(mDSTableDetails.DSTableType == DataSourceTable.eDSTableType.GingerKeyValue)
            {
                foreach (DataGridRow row in grdTableData.GetDataGridRows(grdTableData.Grid))
                {
                    string keyName = ((DataRowView)row.DataContext).Row["GINGER_KEY_NAME"].ToString();
                    string rowError = "";
                    Style rowstyle = new Style(typeof(DataGridRow));
                    if (((DataRowView)row.DataContext).Row.RowState != DataRowState.Deleted)
                    {
                        if (keyName == "")
                            rowError = "Empty Key Name";
                        else if (getKeyRows(keyName).Count() > 1)
                            rowError = "Duplicate Key Name";
                        if (rowError != "")
                        {
                            rowstyle.Setters.Add(SetterError);
                            rowstyle.Setters.Add(SetterBorder);
                            rowstyle.Setters.Add(new Setter(DataGridRow.ToolTipProperty, rowError));
                            status = false;
                        }
                        else
                        {
                            rowstyle.Setters.Remove(SetterError);
                            rowstyle.Setters.Remove(SetterBorder);
                            rowstyle.Setters.Add(new Setter(DataGridRow.ToolTipProperty, ""));
                        }
                        row.Style = rowstyle;
                    }
                }
                if (status == false)
                    Reporter.ToUser(eUserMsgKey.GingerKeyNameError,mDSTableDetails.Name);
            }            
            return status;               
        }
        #endregion Functions

        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            if (Reporter.ToUser(eUserMsgKey.SaveLocalChanges) == Amdocs.Ginger.Common.eUserMsgSelection.No)
            {
                return;
            }

            string newName = mDSTableDetails.Name;
            InputBoxWindow.OpenDialog("Rename", "Table Name:", ref newName);

            ValidateAndUpdateDBTableName(newName);

            mDSTableDetails.DirtyStatus = Amdocs.Ginger.Common.Enums.eDirtyStatus.NoChange;
        }

        void ValidateAndUpdateDBTableName(string newName)
        {
            if (mDSTableDetails.DSC.DSTableList.Any(t => t.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
            {
                Reporter.ToUser(eUserMsgKey.DbTableNameError, newName);
            }
            else
            {
                mDSTableDetails.DSC.RenameTable(mDSTableDetails.Name, newName);
                mDSTableDetails.Name = newName;
                RefreshGrid();
            }
        }
    }
}
