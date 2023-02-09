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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.InterfacesLib;
using Amdocs.Ginger.CoreNET.ActionsLib;
using GingerCore;
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Ginger.Actions
{
    /// <summary>
    /// Interaction logic for XLSReadDataToVariablesPage.xaml
    /// </summary>
    public partial class ActExcelEditPage
    {
        public ActionEditPage actp;
        private ActExcel mAct;
        private IExcelOperations mExcelOperations = new ExcelNPOIOperations();
        public ActExcelEditPage(ActExcel act)
        {
            InitializeComponent();
            mAct = act;
            Bind();
            mAct.SolutionFolder = WorkSpace.Instance.Solution.Folder.ToUpper();
        }

        public void Bind()
        {
            ExcelActionComboBox.BindControl(mAct, nameof(ActExcel.ExcelActionType));
            ExcelFileNameTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, nameof(ActExcel.ExcelFileName));
            SheetNamComboBox.BindControl(mAct, nameof(ActExcel.SheetName));
            SelectRowsWhereTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, nameof(ActExcel.SelectRowsWhere));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SelectAllRows, CheckBox.IsCheckedProperty, mAct, nameof(ActExcel.SelectAllRows));
            PrimaryKeyColumnTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, nameof(ActExcel.PrimaryKeyColumn));
            SetDataUsedTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, nameof(ActExcel.SetDataUsed));
            ColMappingRulesTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, nameof(ActExcel.ColMappingRules));

            if (mAct.ExcelActionType == ActExcel.eExcelActionType.ReadData)
            {
                this.ColMappingRulesSection.Visibility = Visibility.Collapsed;
                SetDataUsedSection.Visibility = Visibility.Visible;
            }
            if (mAct.ExcelActionType == ActExcel.eExcelActionType.WriteData)
            {
                this.ColMappingRulesSection.Visibility = Visibility.Visible;
            }

            // populate Sheet dropdown
            if (!string.IsNullOrEmpty(mAct.ExcelFileName))
            {
                FillSheetCombo();
                if (mAct.SheetName != null)
                {
                    SheetNamComboBox.Items.Add(mAct.SheetName);
                    SheetNamComboBox.SelectedValue = mAct.SheetName;
                }
            }
        }

        private void BrowseExcelButton_Click(object sender, RoutedEventArgs e)
        {
            if (General.SetupBrowseFile(new System.Windows.Forms.OpenFileDialog()
            {
                DefaultExt = "*.xlsx or .xls or .xlsm",
                Filter = "Excel Files (*.xlsx, *.xls, *.xlsm)|*.xlsx;*.xls;*.xlsm"
            }) is string fileName)
            {
                ExcelFileNameTextBox.ValueTextBox.Text = fileName;
                FillSheetCombo();
            }
        }

        private void FillSheetCombo()
        {
            ContextProcessInputValueForDriver();
            //Move code to ExcelFunction no in Act...
            List<string> SheetsList = mExcelOperations.GetSheets(mAct.CalculatedFileName);
            if (SheetsList == null || SheetsList.Count == 0)
            {
                Reporter.ToUser(eUserMsgKey.ExcelInvalidFieldData);
                return;
            }
            GingerCore.General.FillComboFromList(SheetNamComboBox, SheetsList);
        }

        private void ContextProcessInputValueForDriver()
        {
            var context = Context.GetAsContext(mAct.Context);
            if (context != null)
            {
                context.Runner.ProcessInputValueForDriver(mAct);
            }
        }

        private void ViewDataButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ContextProcessInputValueForDriver();

                DataTable excelSheetData = GetExcelSheetData(true);
                if (excelSheetData == null)
                {
                    Reporter.ToUser(eUserMsgKey.ExcelInvalidFieldData);
                    return;
                }

                SetExcelDataGridItemsSource(excelSheetData);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
            }
        }

        private void SetExcelDataGridItemsSource(DataTable excelSheetData)
        {
            CreateExcelDataGridColumns(excelSheetData);

            ObservableCollection<object?[]> excelDataGridRows = new ObservableCollection<object?[]>();
            BindingOperations.EnableCollectionSynchronization(excelDataGridRows, excelDataGridRows);
            ExcelDataGrid.ItemsSource = excelDataGridRows;

            CopyExcelSheetRowsToExcelDataGridRows(excelSheetData.Rows, excelDataGridRows);
        }

        private void CreateExcelDataGridColumns(DataTable excelSheetData)
        {
            ExcelDataGrid.Columns.Clear();
            for (int columnIndex = 0; columnIndex < excelSheetData.Columns.Count; columnIndex++)
            {
                DataColumn excelSheetColumn = excelSheetData.Columns[columnIndex];
                ExcelDataGrid.Columns.Add(
                    new DataGridTextColumn()
                    {
                        Header = excelSheetColumn.ColumnName,
                        Binding = new Binding($"[{columnIndex}]")
                    });
            }
        }

        private Task CopyExcelSheetRowsToExcelDataGridRows(DataRowCollection excelSheetRows, ObservableCollection<object?[]> excelDataGridRows)
        {
            return Task.Run(() =>
            {
                foreach (DataRow excelSheetRow in excelSheetRows)
                {
                    lock (excelDataGridRows)
                    {
                        excelDataGridRows.Add(excelSheetRow.ItemArray);
                    }
                    Thread.Sleep(10);
                }
            });
        }

        private void ViewWhereButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!mAct.CheckMandatoryFieldsExists(new List<string>() {
                nameof(mAct.CalculatedFileName), nameof(mAct.CalculatedSheetName),  nameof(mAct.SelectRowsWhere)}))
                {
                    Reporter.ToUser(eUserMsgKey.ExcelInvalidFieldData);
                    return;
                }
                ContextProcessInputValueForDriver();
                DataTable excelSheetData = GetExcelSheetData(false);
                if (excelSheetData == null)
                {
                    Reporter.ToUser(eUserMsgKey.ExcelInvalidFieldData);
                    return;
                }

                SetExcelDataGridItemsSource(excelSheetData);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
            }
        }
        DataTable GetExcelSheetData(bool isViewAllData)
        {
            try
            {
                if (!mAct.CheckMandatoryFieldsExists(new List<string>() { nameof(mAct.CalculatedFileName), nameof(mAct.CalculatedSheetName) }))
                {
                    return null;
                }
                if (!isViewAllData && mAct.ExcelActionType == ActExcel.eExcelActionType.ReadCellData && !string.IsNullOrWhiteSpace(mAct.CalculatedFilter))
                {
                    return mExcelOperations.ReadCellData(mAct.CalculatedFileName, mAct.CalculatedSheetName, mAct.CalculatedFilter, true);
                }
                return mExcelOperations.ReadData(mAct.CalculatedFileName, mAct.CalculatedSheetName, isViewAllData ? null : mAct.CalculatedFilter, true);
            }
            catch (DuplicateNameException ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
                throw;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
                return null;
            }
        }
        private void ExcelActionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ContextProcessInputValueForDriver();

            if (ExcelActionComboBox.SelectedValue.ToString() == "ReadData")
            {
                SetDataUsedSection.Visibility = Visibility.Visible;
                ColMappingRulesSection.Visibility = Visibility.Collapsed;
                //fixing for #3811 : to clear Data from Write Data column
                mAct.ColMappingRules = string.Empty;
            }

            if (ExcelActionComboBox.SelectedValue.ToString() == "WriteData")
            {
                ColMappingRulesSection.Visibility = Visibility.Visible;
            }
        }

        private void SheetNamComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mAct.SheetName = SheetNamComboBox.Text;
            ContextProcessInputValueForDriver();
        }

        private void SheetNamComboBox_DropDownOpened(object sender, EventArgs e)
        {
            if (!mAct.CheckMandatoryFieldsExists(new List<string>() { nameof(mAct.CalculatedFileName) }))
            {
                Reporter.ToUser(eUserMsgKey.ExcelInvalidFieldData);
                return;
            }
            FillSheetCombo();
        }

        private void SheetNamVEButton_Click(object sender, RoutedEventArgs e)
        {
            ValueExpressionEditorPage w = new ValueExpressionEditorPage(mAct, nameof(ActExcel.SheetName), Context.GetAsContext(mAct.Context));
            w.ShowAsWindow(eWindowShowStyle.Dialog);
            SheetNamComboBox.Text = mAct.SheetName;
        }

        private void xOpenExcelButton_Click(object sender, RoutedEventArgs e)
        {
            if (!mAct.CheckMandatoryFieldsExists(new List<string>() { nameof(mAct.CalculatedFileName) }))
            {
                Reporter.ToUser(eUserMsgKey.ExcelInvalidFieldData);
                return;
            }
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = mAct.CalculatedFileName, UseShellExecute = true });
        }

    }
}
