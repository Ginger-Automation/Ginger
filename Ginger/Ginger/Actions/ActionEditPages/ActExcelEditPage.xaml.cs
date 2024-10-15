#region License
/*
Copyright © 2014-2024 European Support Limited

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
using GingerCore.Actions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

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
        private const int VIEW_DATA_ROW_LIMIT = 50;
        private List<string>? SheetsList;
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
            HeaderRowNumTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, nameof(ActExcel.HeaderRowNum));

            SetDataUsedTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, nameof(ActExcel.SetDataUsed));
            ColMappingRulesTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, nameof(ActExcel.ColMappingRules));

            if (mAct.ExcelActionType is ActExcel.eExcelActionType.ReadData or ActExcel.eExcelActionType.ReadCellData)
            {
                this.ColMappingRulesSection.Visibility = Visibility.Collapsed;
                SetDataUsedSection.Visibility = Visibility.Visible;
            }
            if (mAct.ExcelActionType == ActExcel.eExcelActionType.WriteData)
            {
                this.ColMappingRulesSection.Visibility = Visibility.Visible;
            }

            EnableSheetNameComboBox();
            // populate Sheet drop down
            if (!string.IsNullOrEmpty(mAct.ExcelFileName))
            {
                FillSheetCombo();
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
                SheetsList = null;
                // Empty the list in the UI before Creating a new list
                GingerCore.General.FillComboFromList(SheetNamComboBox, SheetsList);
                mAct.SheetName = "";
                FillSheetCombo();
            }
        }

        private async Task FillSheetCombo()
        {
            ContextProcessInputValueForDriver();
            if (SheetsList == null || !SheetsList.Any())
            {
                DisableSheetNameComboBox();
                MakeLoaderVisible();
                mExcelOperations.Dispose();
                await Task.Run(() =>
                {
                    try
                    {
                        SheetsList = mExcelOperations.GetSheets(mAct.CalculatedFileName);
                        Dispatcher.Invoke(() =>
                        {
                            GingerCore.General.FillComboFromList(SheetNamComboBox, SheetsList);
                        });
                        if (SheetsList == null || SheetsList.Count == 0)
                        {
                            ShowErrorResponse();
                            return;
                        }

                    }
                    catch (Exception)
                    {
                        ShowErrorResponse();
                    }
                    finally
                    {
                        Dispatcher.Invoke(() =>
                        {
                            EnableSheetNameComboBox();
                            HideLoader();
                        });
                    }
                });
            }
        }
        private void DisableSheetNameComboBox()
        {
            WeakEventManager<ComboBox, EventArgs>.RemoveHandler(source: SheetNamComboBox, eventName: nameof(ComboBox.DropDownOpened), handler: SheetNamComboBox_DropDownOpened);
        }

        private void EnableSheetNameComboBox()
        {
            WeakEventManager<ComboBox, EventArgs>.AddHandler(source: SheetNamComboBox, eventName: nameof(ComboBox.DropDownOpened), handler: SheetNamComboBox_DropDownOpened);
        }

        private void ContextProcessInputValueForDriver()
        {
            var context = Context.GetAsContext(mAct.Context);
            if (context != null)
            {
                context.Runner.ProcessInputValueForDriver(mAct);
            }
        }

        private async void ViewDataButton_Click(object sender, RoutedEventArgs e)
        {
            xViewDataLoader.Visibility = Visibility.Visible;
            ExcelDataGrid.Visibility = Visibility.Collapsed;

            await Task.Run(() =>
            {
                try
                {
                    ContextProcessInputValueForDriver();
                    DataTable excelSheetData = GetExcelSheetData(true);
                    if (excelSheetData == null)
                    {
                        return;
                    }

                    Dispatcher.Invoke(() =>
                    {
                        ExcelDataGrid.Visibility = Visibility.Visible;

                        SetExcelDataGridItemsSource(excelSheetData);

                    });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Reporter.ToUser(eUserMsgKey.StaticErrorMessage, ex.Message);
                    });
                }
                finally
                {
                    Dispatcher.Invoke(() => xViewDataLoader.Visibility = Visibility.Collapsed);
                }
            });
        }


        private void SetExcelDataGridItemsSource(DataTable excelSheetData)
        {
            CreateExcelDataGridColumns(excelSheetData);

            ObservableCollection<object?[]> excelDataGridRows = [];
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

        private void ShowErrorResponse()
        {
            Dispatcher.Invoke(() =>
            {
                Reporter.ToUser(eUserMsgKey.ExcelInvalidFieldData);
            });
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
                }
            });
        }
        private void MakeLoaderVisible()
        {
            xLoader.Visibility = Visibility.Visible;
        }

        private void HideLoader()
        {
            xLoader.Visibility = Visibility.Hidden;

        }

        private async void ViewWhereButton_Click(object sender, RoutedEventArgs e)
        {
            xViewDataLoader.Visibility = Visibility.Visible;
            ExcelDataGrid.Visibility = Visibility.Collapsed;

            await Task.Run(() =>
            {
                try
                {
                    ContextProcessInputValueForDriver();

                    if (!mAct.CheckMandatoryFieldsExists([
                nameof(mAct.CalculatedFileName), nameof(mAct.CalculatedSheetName),  nameof(mAct.SelectRowsWhere)]))
                    {
                        return;
                    }
                    DataTable excelSheetData = GetExcelSheetData(false);
                    if (excelSheetData == null)
                    {
                        return;
                    }
                    Dispatcher.Invoke(() =>
                    {
                        ExcelDataGrid.Visibility = Visibility.Visible;
                        SetExcelDataGridItemsSource(excelSheetData);
                    });
                }
                catch (Exception ex)
                {
                    string errorMessage = ex.Message;
                    if (!string.IsNullOrEmpty(ex.Message) && ex.Message.StartsWith("Cannot find column"))
                    {
                        errorMessage = errorMessage + " " + $"at Row Number {mAct.HeaderRowNum}";
                    }

                    Dispatcher.Invoke(() =>
                    {
                        Reporter.ToUser(eUserMsgKey.StaticErrorMessage, errorMessage);
                    });
                }
                finally
                {
                    Dispatcher.Invoke(() => xViewDataLoader.Visibility = Visibility.Collapsed);
                }

            });
        }

        DataTable GetExcelSheetData(bool isViewAllData)
        {
            try
            {
                if (!mAct.CheckMandatoryFieldsExists([nameof(mAct.CalculatedFileName), nameof(mAct.CalculatedSheetName)]))
                {
                    return null;
                }
                if (!isViewAllData && mAct.ExcelActionType == ActExcel.eExcelActionType.ReadCellData && !string.IsNullOrWhiteSpace(mAct.CalculatedFilter))
                {
                    return mExcelOperations.ReadCellData(mAct.CalculatedFileName, mAct.CalculatedSheetName, mAct.CalculatedFilter, mAct.SelectAllRows, mAct.CalculatedHeaderRowNum);
                }
                return mExcelOperations.ReadDataWithRowLimit(mAct.CalculatedFileName, mAct.CalculatedSheetName, isViewAllData ? null : mAct.CalculatedFilter, mAct.SelectAllRows, mAct.CalculatedHeaderRowNum, VIEW_DATA_ROW_LIMIT);
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex);
                throw;
            }
        }
        private void ExcelActionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ContextProcessInputValueForDriver();

            if (ExcelActionComboBox.SelectedValue.ToString() is "ReadData" or "ReadCellData")
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
            if (!mAct.CheckMandatoryFieldsExists([nameof(mAct.CalculatedFileName)]))
            {
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
            if (!mAct.CheckMandatoryFieldsExists([nameof(mAct.CalculatedFileName)]))
            {
                return;
            }
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo() { FileName = mAct.CalculatedFileName, UseShellExecute = true });
        }

        private void HeaderNumValidation(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
