#region License
/*
Copyright © 2014-2025 European Support Limited

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
using Org.BouncyCastle.Ocsp;
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
            this.DataContext = mAct;
            Bind();
            mAct.SolutionFolder = WorkSpace.Instance.Solution.Folder.ToUpper();
        }

        public void Bind()
        {
            //GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(rdByAddress, RadioButton.IsCheckedProperty, mAct, nameof(ActExcel.eDataSelectionMethod));

            ExcelActionComboBox.BindControl(mAct, nameof(ActExcel.ExcelActionType));
            ExcelFileNameTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, nameof(ActExcel.ExcelFileName));
            SheetNamComboBox.BindControl(mAct, nameof(ActExcel.SheetName));
            SelectRowsWhereTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, nameof(ActExcel.SelectRowsWhere));
            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(SelectAllRows, CheckBox.IsCheckedProperty, mAct, nameof(ActExcel.SelectAllRows));
            PrimaryKeyColumnTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, nameof(ActExcel.PrimaryKeyColumn));
            HeaderRowNumTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, nameof(ActExcel.HeaderRowNum));

            SetDataUsedTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, nameof(ActExcel.SetDataUsed));
            ColMappingRulesTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, nameof(ActExcel.ColMappingRules));

            GingerCore.GeneralLib.BindingHandler.ObjFieldBinding(xPullCellAddressCheckBox, CheckBox.IsCheckedProperty, mAct, nameof(ActExcel.PullCellAddress));
            CellAddressTextBox.BindControl(Context.GetAsContext(mAct.Context), mAct, nameof(ActExcel.SelectCellAddress));

           

            UpdateVisibility();
            EnableSheetNameComboBox();
            if (!string.IsNullOrEmpty(mAct.ExcelFileName))
            {
                FillSheetCombo();
            }
        }


        private void UpdateVisibility()
        {
            // 1. Radio Buttons Visibility (Only for ReadCellData & WriteData)
            bool showRadios = mAct.ExcelActionType is ActExcel.eExcelActionType.ReadData or ActExcel.eExcelActionType.ReadCellData or ActExcel.eExcelActionType.WriteData; xRadioDataSelectionPanel.Visibility = showRadios ? Visibility.Visible : Visibility.Collapsed;

            // 2. Determine if "By Address" is active
            // Safety check: rdByAddress might be null during init, so we check visibility too
            bool isByAddress = (mAct.GetInputParamValue(nameof(mAct.DataSelectionMethod)) == "ByCellAddress" ? true : false);
            rdByAddress.IsChecked = isByAddress;
            rdByParams.IsChecked = !isByAddress;
            // 3. Main Input Panels Logic
            if (mAct.ExcelActionType == ActExcel.eExcelActionType.GetSheetDetails)
            {
                // Hide inputs for GetSheetDetails
                pnlDatabaseOptions.Visibility = Visibility.Collapsed;
                xAddressInputPanel.Visibility = Visibility.Collapsed;
            }
            else if (isByAddress)
            {
                // ADDRESS MODE: Show Address Box, Hide Database params
                pnlDatabaseOptions.Visibility = Visibility.Collapsed;
                xAddressInputPanel.Visibility = Visibility.Visible;
            }
            else
            {
                // PARAM MODE: Show Database params, Hide Address Box
                pnlDatabaseOptions.Visibility = Visibility.Visible;
                xAddressInputPanel.Visibility = Visibility.Collapsed;
            }

            // 4. "Pull Cell Address" Checkbox (Only ReadData)
            if ((mAct.ExcelActionType == ActExcel.eExcelActionType.ReadData || mAct.ExcelActionType == ActExcel.eExcelActionType.ReadCellData) &&  rdByParams.IsChecked == true)
            {
                xPullCellAddressCheckBox.Visibility = Visibility.Visible;
            }
            else
            {
                xPullCellAddressCheckBox.Visibility = Visibility.Collapsed;
            }

            // 5. Write Value Field (SetDataUsed)
            // Show for WriteData (Always)
            // Show for ReadData
            // HIDE for ReadCellData 
            if (mAct.ExcelActionType == ActExcel.eExcelActionType.WriteData )
            {
                ColMappingRulesSection.Visibility = Visibility.Visible;
            }
            else
            {
                ColMappingRulesSection.Visibility = Visibility.Collapsed;
            }

            // 6. Bulk Write Field (ColMappingRules)
            // Show ONLY for WriteData in PARAMETER mode.
            if (mAct.ExcelActionType == ActExcel.eExcelActionType.WriteData && isByAddress)
            {
                ColMappingRulesSection.Visibility = Visibility.Visible;
                SetDataUsedSection.Visibility = Visibility.Collapsed;

            }
            else if (mAct.ExcelActionType == ActExcel.eExcelActionType.WriteData && !isByAddress)
            {
                ColMappingRulesSection.Visibility = Visibility.Visible;
                SetDataUsedSection.Visibility = Visibility.Visible;
            }
            else
            {
                ColMappingRulesSection.Visibility = Visibility.Collapsed;
                SetDataUsedSection.Visibility = Visibility.Collapsed;
            }
        }

        private void ExcelActionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ContextProcessInputValueForDriver();
            //mAct.SelectCellAddress = string.Empty;
            UpdateVisibility();

            if (mAct.ExcelActionType != ActExcel.eExcelActionType.WriteData)
            {
                mAct.ColMappingRules = string.Empty;
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
                    }
                    catch (Exception) 
                    {
                        ShowErrorResponse(); 
                    }
                    finally 
                    { 
                        Dispatcher.Invoke(() => { EnableSheetNameComboBox(); HideLoader(); }); 
                    }
                });
            }
        }

        private void DisableSheetNameComboBox() { WeakEventManager<ComboBox, EventArgs>.RemoveHandler(source: SheetNamComboBox, eventName: nameof(ComboBox.DropDownOpened), handler: SheetNamComboBox_DropDownOpened); }
        private void EnableSheetNameComboBox() { WeakEventManager<ComboBox, EventArgs>.AddHandler(source: SheetNamComboBox, eventName: nameof(ComboBox.DropDownOpened), handler: SheetNamComboBox_DropDownOpened); }
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
                    Dispatcher.Invoke(() => { ExcelDataGrid.Visibility = Visibility.Visible; SetExcelDataGridItemsSource(excelSheetData); });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => { Reporter.ToUser(eUserMsgKey.StaticErrorMessage, ex.Message); });
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
                ExcelDataGrid.Columns.Add(new DataGridTextColumn() { Header = excelSheetColumn.ColumnName, Binding = new Binding($"[{columnIndex}]") });
            }
        }

        private void ShowErrorResponse() { Dispatcher.Invoke(() => { Reporter.ToUser(eUserMsgKey.ExcelInvalidFieldData); }); }
        private Task CopyExcelSheetRowsToExcelDataGridRows(DataRowCollection excelSheetRows, ObservableCollection<object?[]> excelDataGridRows)
        {
            return Task.Run(() => { foreach (DataRow excelSheetRow in excelSheetRows) { lock (excelDataGridRows) { excelDataGridRows.Add(excelSheetRow.ItemArray); } } });
        }
        private void MakeLoaderVisible() { xLoader.Visibility = Visibility.Visible; }
        private void HideLoader() { xLoader.Visibility = Visibility.Hidden; }

        private async void ViewWhereButton_Click(object sender, RoutedEventArgs e)
        {
            xViewDataLoader.Visibility = Visibility.Visible;
            ExcelDataGrid.Visibility = Visibility.Collapsed;
            await Task.Run(() =>
            {
                try
                {
                    ContextProcessInputValueForDriver();
                    bool isByAddressPresent = mAct.DataSelectionMethod == ActExcel.eDataSelectionMethod.ByCellAddress;
                    List<string> requiredFields = isByAddressPresent ? [nameof(mAct.CalculatedFileName), nameof(mAct.CalculatedSheetName), nameof(mAct.SelectCellAddress)] : [nameof(mAct.CalculatedFileName), nameof(mAct.CalculatedSheetName), nameof(mAct.SelectRowsWhere)];
                    if (!mAct.CheckMandatoryFieldsExists(requiredFields))
                        {
                            return;
                        }

                    DataTable excelSheetData = GetExcelSheetData(false);
                    if (excelSheetData == null)
                    {
                        return;
                    }
                    Dispatcher.Invoke(() => { ExcelDataGrid.Visibility = Visibility.Visible; SetExcelDataGridItemsSource(excelSheetData); });
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => { Reporter.ToUser(eUserMsgKey.StaticErrorMessage, ex.Message); });
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
                if (!mAct.CheckMandatoryFieldsExists([nameof(mAct.CalculatedFileName), nameof(mAct.CalculatedSheetName)])) return null;
                if (!isViewAllData && (mAct.ExcelActionType == ActExcel.eExcelActionType.ReadCellData || (mAct.ExcelActionType == ActExcel.eExcelActionType.ReadData)) && mAct.DataSelectionMethod == ActExcel.eDataSelectionMethod.ByCellAddress && !string.IsNullOrWhiteSpace(mAct.CalculatedFilter))
                {
                    return mExcelOperations.ReadCellData(mAct.CalculatedFileName, mAct.CalculatedSheetName, mAct.CalculatedFilter, mAct.SelectAllRows, mAct.CalculatedHeaderRowNum);
                }
                return mExcelOperations.ReadDataWithRowLimit(mAct.CalculatedFileName, mAct.CalculatedSheetName, isViewAllData ? null : mAct.CalculatedFilter, mAct.SelectAllRows, mAct.CalculatedHeaderRowNum, VIEW_DATA_ROW_LIMIT);
            }
            catch (Exception ex) { Reporter.ToLog(eLogLevel.ERROR, ex.Message, ex); throw; }
        }
        private void DataSelection_Changed(object sender, RoutedEventArgs e)
        {
           
            if (rdByAddress == null || rdByParams == null)
            {
                return;
            }

            if ((bool)rdByAddress.IsChecked)
            {
              
                mAct.DataSelectionMethod = ActExcel.eDataSelectionMethod.ByCellAddress;
                rdByParams.IsChecked = false;
                xPullCellAddressCheckBox.IsChecked = false;
                mAct.SelectRowsWhere = string.Empty;
                mAct.PrimaryKeyColumn = string.Empty;

            }
            else
            {
                mAct.DataSelectionMethod = ActExcel.eDataSelectionMethod.ByParameters;
                xPullCellAddressCheckBox.IsChecked = false;
                mAct.SelectCellAddress = string.Empty;
            }

            UpdateVisibility();
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
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = mAct.CalculatedFileName,
                UseShellExecute = true
            });
        }

        private void HeaderNumValidation(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}