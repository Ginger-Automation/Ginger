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
using System.Windows;
using System.Windows.Controls;
using GingerCore;
using Ginger.Actions;
using System.Collections.ObjectModel;
using System;
using GingerCore.DataSource;
using System.Data;
using GingerCore.GeneralLib;
using Ginger.UserControls;
using System.Collections.Generic;
using Amdocs.Ginger.Repository;
using GingerCore.Actions;
using Amdocs.Ginger.CoreNET.DataSource;
using amdocs.ginger.GingerCoreNET;
using GingerCore.Helpers;
using System.Linq;

namespace Ginger.DataSource
{
    /// <summary>
    /// Interaction logic for NewAgentPage.xaml
    /// </summary>
    public partial class DataSourceExportToExcel : Page
    {
        GenericWindow _pageGenericWin = null;
        bool okClicked = false;

        public ObservableList<ColumnCheckListItem> mColumnList = null;

        public ExportToExcelConfig mExcelConfig = new ExportToExcelConfig();

        public ObservableList<ActDSConditon> mWhereConditionList = new ObservableList<ActDSConditon>();

        private DataTable mDataTable = new DataTable();
        private ActDSTableElement mActDSTableElement = null;

        private DataSourceTable mDataSourceTable = null;

        public DataSourceExportToExcel()
        {
            InitializeComponent();
            InitPageData();
        }
        public DataSourceExportToExcel(DataSourceTable dataSourceTable)
        {
            InitializeComponent();
            mDataSourceTable = dataSourceTable;

            InitPageData();
            
            mDataTable = dataSourceTable.DataTable;
            InitColumnListGrid(mDataTable.Columns);
            xExportSheetName.ValueTextBox.Text = mDataTable.TableName;
        }

        private void InitPageData()
        {
            ExcelFilePath.Init(null, null, false, true, UCValueExpression.eBrowserType.File, "xlsx");
            xExcelExportQuery.Init(null, mExcelConfig,nameof(ExportToExcelConfig.ExportQueryValue), true);

            xExportSheetName.Init(null, mExcelConfig, nameof(ExportToExcelConfig.ExcelSheetName), true);



            xRdoByCustomExport.IsChecked = true;

            if (mWhereConditionList == null)
            {
                mWhereConditionList = new ObservableList<ActDSConditon>();
            }
             
            xGrdExportCondition.DataSourceList = mWhereConditionList;

            ExcelFilePath.ValueTextBox.TextChanged += ExcelFilePathTextBox_TextChanged;
            xExportSheetName.ValueTextBox.TextChanged += ExcelSheetNameTextBox_TextChanged;
            xExcelExportQuery.ValueTextBox.TextChanged += ExcelExportQuery_ValueTextBox_TextChanged;

           
            SetConditionGridView();
        }

        public DataSourceExportToExcel(ActDSTableElement actDSTableElement)
        {
            InitializeComponent();
            mActDSTableElement = actDSTableElement;

            if (mActDSTableElement.ExcelConfig == null)
            {
                mActDSTableElement.ExcelConfig = new ExportToExcelConfig();
            }
            SetFilePath();
            SetDataTable();
            SetSheetName();
            ExcelFilePath.Init(Context.GetAsContext(mActDSTableElement.Context), mActDSTableElement.ExcelConfig, nameof(ExportToExcelConfig.ExcelPath), true, true, UCValueExpression.eBrowserType.File, "xlsx");

            xExcelExportQuery.Init(Context.GetAsContext(mActDSTableElement.Context), mActDSTableElement.ExcelConfig, nameof(ExportToExcelConfig.ExportQueryValue),true);
            xExportSheetName.Init(Context.GetAsContext(mActDSTableElement.Context), mActDSTableElement.ExcelConfig, nameof(ExportToExcelConfig.ExcelSheetName),true);

            BindingHandler.ObjFieldBinding(xRdoByCustomExport, RadioButton.IsCheckedProperty, mActDSTableElement.ExcelConfig, nameof(ExportToExcelConfig.IsCustomExport));
           
            BindingHandler.ObjFieldBinding(xRdoByQueryExport, RadioButton.IsCheckedProperty, mActDSTableElement.ExcelConfig, nameof(ExportToExcelConfig.IsExportByQuery));
        
            BindingHandler.ObjFieldBinding(xExportWhereChkBox, CheckBox.IsCheckedProperty, mActDSTableElement.ExcelConfig, nameof(ExportToExcelConfig.ExportByWhere));
          
            ExcelFilePath.ValueTextBox.TextChanged += ExcelFilePathTextBox_TextChanged;
            xExportSheetName.ValueTextBox.TextChanged += ExcelSheetNameTextBox_TextChanged;
            xExcelExportQuery.ValueTextBox.TextChanged += ExcelExportQuery_ValueTextBox_TextChanged;

            if (mDataTable != null)
            {
                InitColumnListGrid(mDataTable.Columns);
            }

            SetConditionGridView();
            UpdateQueryValue();
        }

        private void SetFilePath()
        {
            if (!string.IsNullOrEmpty(mActDSTableElement.ExcelPath))
            {
                mActDSTableElement.ExcelConfig.ExcelPath = mActDSTableElement.ExcelPath;
                mActDSTableElement.ExcelPath = string.Empty;
            }
        }

        private void SetSheetName()
        {
            if (!string.IsNullOrEmpty(mActDSTableElement.ExcelSheetName))
            {
                mActDSTableElement.ExcelConfig.ExcelSheetName = mActDSTableElement.ExcelSheetName;
                mActDSTableElement.ExcelSheetName = string.Empty;
            }
            else if (mDataTable != null && string.IsNullOrEmpty(mActDSTableElement.ExcelConfig.ExcelSheetName))
            {
                mActDSTableElement.ExcelConfig.ExcelSheetName = mDataTable.TableName;
            }
        }

        private void ExcelSheetNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (mActDSTableElement != null)
            {
                mActDSTableElement.ExcelConfig.ExcelSheetName = xExportSheetName.ValueTextBox.Text;
            }
            else
            {
                mExcelConfig.ExcelSheetName = xExportSheetName.ValueTextBox.Text;
            }
            
        }

        private void ExcelFilePathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (mActDSTableElement != null)
            {
                mActDSTableElement.ExcelConfig.ExcelPath = ExcelFilePath.ValueTextBox.Text;
            }
            else
            {
                mExcelConfig.ExcelPath = ExcelFilePath.ValueTextBox.Text;
            }
            
        }

        private void SetDataTable()
        {
            if (mActDSTableElement.DSList == null)
            {
                mActDSTableElement.DSList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<DataSourceBase>();
            }

            DataSourceBase dataSource = null;
            var tableName = mActDSTableElement.DSTableName;

            if (string.IsNullOrEmpty(tableName))
            {
                return;
            }

            foreach (var ds in mActDSTableElement.DSList)
            {
                if (ds.Name == mActDSTableElement.DSName)
                {
                    dataSource = ds;
                }
            }
     
            mDataTable = dataSource.GetTable(tableName);

            foreach (DataSourceTable dst in dataSource.GetTablesList())
            {
                if (dst.Name == mActDSTableElement.DSTableName)
                {
                    mDataSourceTable = dst;
                    break;
                }
            }
        }

        private void ExcelExportQuery_ValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (mActDSTableElement != null)
            {
                mActDSTableElement.ExcelConfig.ExportQueryValue = xExcelExportQuery.ValueTextBox.Text;
            }
        }

        private void InitColumnListGrid(DataColumnCollection columns)
        {
            SetTableColumnListGridView();
            mColumnList = new ObservableList<ColumnCheckListItem>();


            foreach (DataColumn column in columns)
            {
                mColumnList.Add(new ColumnCheckListItem { IsSelected = true, ColumnText = column.ColumnName });
            }

            if (mActDSTableElement != null && mActDSTableElement.ExcelConfig.ColumnList != null && mActDSTableElement.ExcelConfig.ColumnList.Count > 0)
            {
                if (mColumnList.Count.Equals(mActDSTableElement.ExcelConfig.ColumnList.Count) && mColumnList.Select(p => p.ColumnText).ToList().All(mActDSTableElement.ExcelConfig.ColumnList.Select(f => f.ColumnText).ToList().Contains))
                {
                    mColumnList = mActDSTableElement.ExcelConfig.ColumnList;
                }
            }
            else if (mActDSTableElement != null && mActDSTableElement.ExcelConfig.ColumnList == null)
            {
                mActDSTableElement.ExcelConfig.ColumnList = mColumnList;
            }
            else if (mExcelConfig.ColumnList == null)
            {
                mExcelConfig.ColumnList = mColumnList;
            }

            xColumnListGrid.DataSourceList = mColumnList;
            xColumnListGrid.RowChangedEvent += XColumnListGrid_RowChangedEvent;
            xColumnListGrid.LostFocus += XColumnListGrid_LostFocus;
        }

        private void XColumnListGrid_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateQueryValue();
        }

        private void XColumnListGrid_RowChangedEvent(object sender, EventArgs e)
        {
            UpdateQueryValue();
        }

        private void UpdateQueryValue()
        {
            CreateQueryBasedWhereCondition();
            if (mActDSTableElement != null)
            {
                mActDSTableElement.ExcelConfig.ColumnList = mColumnList;
            }
            else
            {
                mExcelConfig.ColumnList = mColumnList;
            }
            
        }

        private void SetTableColumnListGridView()
        {
            //tool bar
            xColumnListGrid.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Check/Uncheck All Columns", new RoutedEventHandler(CheckUnCheckTableColumn));
            
            //Set the Data Grid columns            
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = nameof(ColumnCheckListItem.IsSelected), Header = "Select", WidthWeight = 20, StyleType = GridColView.eGridColStyleType.CheckBox,BindingMode=System.Windows.Data.BindingMode.TwoWay });
            view.GridColsView.Add(new GridColView() { Field = nameof(ColumnCheckListItem.ColumnText), Header = "Table Column", WidthWeight = 100, ReadOnly = true });

            xColumnListGrid.SetAllColumnsDefaultView(view);
            xColumnListGrid.InitViewItems();
            
        }

        private void CheckUnCheckTableColumn(object sender, RoutedEventArgs e)
        {
            if (mColumnList.Count > 0)
            {
                bool valueToSet = !mColumnList[0].IsSelected;
                foreach (ColumnCheckListItem elem in mColumnList)
                {
                    elem.IsSelected = valueToSet;
                }
                xColumnListGrid.DataSourceList = mColumnList;
                CreateQueryBasedWhereCondition();
            }

        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            //validate details
            if (ExcelFilePath.ValueTextBox.Text.Trim() == string.Empty) { Reporter.ToUser(eUserMsgKey.MissingExcelDetails); return; }
            if (!ExcelFilePath.ValueTextBox.Text.ToLower().EndsWith(".xlsx")) { Reporter.ToUser(eUserMsgKey.InvalidExcelDetails); return; }            

            okClicked = true;

            mExcelConfig.ExcelPath = ExcelFilePath.ValueTextBox.Text;
            mExcelConfig.ExcelSheetName = xExportSheetName.ValueTextBox.Text;

            if (xRdoByCustomExport.IsChecked==true)
            {
                CreateQueryBasedWhereCondition();
                if (mActDSTableElement != null)
                {
                    mExcelConfig.ExportQueryValue = mExcelConfig.CreateQueryWithWhereList(mActDSTableElement.ExcelConfig.ColumnList.ToList().FindAll(x => x.IsSelected), mExcelConfig.WhereConditionStringList, mDataTable.TableName, mDataSourceTable.DSC.DSType); 
                }
                else if(mDataSourceTable != null)
                {
                    mExcelConfig.ExportQueryValue = mExcelConfig.CreateQueryWithWhereList(mColumnList.ToList().FindAll(x => x.IsSelected), mExcelConfig.WhereConditionStringList, mDataTable.TableName, mDataSourceTable.DSC.DSType);
                }
            }
            else
            {
                mExcelConfig.ExportQueryValue = xExcelExportQuery.ValueTextBox.Text;
            }
            

            _pageGenericWin.Close();
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button okBtn = new Button();
            okBtn.Content = "OK";
            okBtn.Click += new RoutedEventHandler(OKButton_Click);
            ObservableList<Button> winButtons = new ObservableList<Button>();
            winButtons.Add(okBtn);

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, winButtons, true, "Cancel");
        }

       


        private void SetConditionGridView()
        {
            xGrdExportCondition.SetTitleLightStyle = true;
            xGrdExportCondition.ShowViewCombo = Visibility.Collapsed;

            List<ComboEnumItem> lstCond = GingerCore.General.GetEnumValuesForCombo(typeof(ActDSConditon.eCondition));
            List<ComboEnumItem> lstOper = GingerCore.General.GetEnumValuesForCombo(typeof(ActDSConditon.eOperator));

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            view.GridColsView.Add(new GridColView() { Field = ActDSConditon.Fields.wCondition, Header = "And/Or", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(ActDSConditon.Fields.PossibleCondValues, ActDSConditon.Fields.wCondition) });
            view.GridColsView.Add(new GridColView() { Field = ActDSConditon.Fields.wTableColumn, Header = "Column", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = ucGrid.GetGridComboBoxTemplate(ActDSConditon.Fields.PossibleColumnValues, ActDSConditon.Fields.wTableColumn) });
            view.GridColsView.Add(new GridColView() { Field = ActDSConditon.Fields.wOperator, Header = "Operator", WidthWeight = 10, StyleType = GridColView.eGridColStyleType.ComboBox, CellValuesList = lstOper });
            view.GridColsView.Add(new GridColView() { Field = ActDSConditon.Fields.wValue, Header = "Value", WidthWeight = 30 });
            view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 5, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.WhereGrid.Resources["ValueExpressionButton"] });

            xGrdExportCondition.SetAllColumnsDefaultView(view);
            xGrdExportCondition.InitViewItems();

            if (mActDSTableElement != null)
            {
                if (mActDSTableElement.ExcelConfig.WhereConditionStringList == null)
                {
                    mActDSTableElement.ExcelConfig.WhereConditionStringList = mActDSTableElement.ExcelConfig.CreateConditionStringList(mWhereConditionList);
                }
                
                mWhereConditionList = mActDSTableElement.ExcelConfig.GetConditons(mActDSTableElement.ExcelConfig.WhereConditionStringList,mDataTable);

            }

            xGrdExportCondition.DataSourceList = mWhereConditionList;
            xGrdExportCondition.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddWhereCondition));
            xGrdExportCondition.btnDelete.AddHandler(Button.ClickEvent, new RoutedEventHandler(DeleteWhereCondition));

            xGrdExportCondition.LostFocus += XGrdExportCondition_LostFocus;
            if (Convert.ToBoolean(xExportWhereChkBox.IsChecked))
            {
                xGrdExportCondition.Visibility = Visibility.Visible;
            }
        }

        private void XGrdExportCondition_LostFocus(object sender, RoutedEventArgs e)
        {
            CreateQueryBasedWhereCondition();
        }

        private void xRdoByQueryExport_Checked(object sender, RoutedEventArgs e)
        {
            xExcelExportCustomPanel.Visibility = Visibility.Collapsed;
            xByQueryPanel.Visibility = Visibility.Visible;
            
            if (mActDSTableElement != null)
            {
                mActDSTableElement.ExcelConfig.IsCustomExport = Convert.ToBoolean(xRdoByCustomExport.IsChecked);
                xExcelExportQuery.ValueTextBox.Text = mExcelConfig.CreateQueryWithWhereList(mActDSTableElement.ExcelConfig.ColumnList.ToList().FindAll(x => x.IsSelected), mActDSTableElement.ExcelConfig.WhereConditionStringList, mDataTable.TableName, mDataSourceTable.DSC.DSType);
            }
            else
            {
                xExcelExportQuery.ValueTextBox.Text = mExcelConfig.CreateQueryWithWhereList(mExcelConfig.ColumnList.ToList().FindAll(x => x.IsSelected), mExcelConfig.WhereConditionStringList, mDataTable.TableName, mDataSourceTable.DSC.DSType);
            }
        }

        private void xRdoByCustomExport_Checked(object sender, RoutedEventArgs e)
        {
            xByQueryPanel.Visibility = Visibility.Collapsed;
            xExcelExportCustomPanel.Visibility = Visibility.Visible;
            if (mActDSTableElement !=null)
            {
                mActDSTableElement.ExcelConfig.IsCustomExport = Convert.ToBoolean(xRdoByCustomExport.IsChecked);
            }
        }


        private void xExportWhereChkBox_Click(object sender, RoutedEventArgs e)
        {
            if(xExportWhereChkBox.IsChecked.Equals(true))
            {
                xGrdExportCondition.Visibility = Visibility.Visible;
                if (mActDSTableElement != null)
                {
                    mActDSTableElement.ExcelConfig.ExportByWhere = true;
                }
            }
            else
            {
                xGrdExportCondition.Visibility = Visibility.Collapsed;
                mWhereConditionList.Clear();
                
                if (mActDSTableElement != null)
                {
                    mActDSTableElement.ExcelConfig.ExportByWhere = false;
                }
            }
            CreateQueryBasedWhereCondition();
        }

        private void GridVEButton_Click(object sender, RoutedEventArgs e)
        {
            //ValueExpressionButton clicked
            ActDSConditon ADSC = (ActDSConditon)xGrdExportCondition.CurrentItem;
            ValueExpressionEditorPage VEEW = new ValueExpressionEditorPage(ADSC, ActDSConditon.Fields.wValue, Context.GetAsContext(mActDSTableElement.Context));
            VEEW.ShowAsWindow();
        }

        private void AddWhereCondition(object sender, RoutedEventArgs e)
        {
            ActDSConditon.eCondition defaultCondition = ActDSConditon.eCondition.EMPTY;
            ObservableList<string> Condition = new ObservableList<string>();
            if (mWhereConditionList.Count > 0)
            {
                foreach (ActDSConditon.eCondition item in Enum.GetValues(typeof(ActDSConditon.eCondition)))
                {
                    if (item.ToString() != "EMPTY")
                    {
                        Condition.Add(item.ToString());
                    }
                }

                defaultCondition = ActDSConditon.eCondition.AND;
            }
            List<string> cols = new List<string>();
            var columns = mDataTable.Columns;
            foreach (var item in columns)
            {
                cols.Add(item.ToString());
            }
            mWhereConditionList.Add(new ActDSConditon() { PossibleCondValues = Condition, PossibleColumnValues = cols, wCondition = defaultCondition, wTableColumn = cols[0] });
            xGrdExportCondition.DataSourceList = mWhereConditionList;
            CreateQueryBasedWhereCondition();
        }

        private void CreateQueryBasedWhereCondition()
        {
            if (mActDSTableElement != null)
            {
               mActDSTableElement.ExcelConfig.WhereConditionStringList = mActDSTableElement.ExcelConfig.CreateConditionStringList(mWhereConditionList);
            }
            else
            {
               mExcelConfig.WhereConditionStringList = mExcelConfig.CreateConditionStringList(mWhereConditionList);
            }
        }

        private void DeleteWhereCondition(object sender, RoutedEventArgs e)
        {
            if (mWhereConditionList.Count > 0)
            {
                mWhereConditionList[0].PossibleCondValues = new ObservableList<string>();
                mWhereConditionList[0].wCondition = ActDSConditon.eCondition.EMPTY;
            }
            xGrdExportCondition.DataSourceList = mWhereConditionList;
            CreateQueryBasedWhereCondition();
        }

    }

}
