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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using GingerCore;
using GingerCore.DataSource;
using System.Reflection;
using Ginger.ApplicationModelsLib.ModelOptionalValue;
using System.Data;
using System.Collections.Generic;
using System;
using System.Text;
using Ginger.SolutionWindows.TreeViewItems;
using Amdocs.Ginger.ValidationRules;
using System.Windows.Media;
using System.Windows.Data;
using GingerWPF;
using GingerWPF.WizardLib;

namespace Ginger.DataSource.ImportExcelWizardLib
{
    /// <summary>
    /// Interaction logic for ImportDataSourceSheetSelection.xaml
    /// </summary>
    public partial class ImportDataSourceSheetSelection : Page, IWizardPage
    {
        ImportOptionalValuesForParameters impParams;

        ImportDataSourceFromExcelWizard mWizard;

        /// <summary>
        /// This method is default wizard action event
        /// </summary>
        /// <param name="WizardEventArgs"></param>
        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    mWizard = (ImportDataSourceFromExcelWizard)WizardEventArgs.Wizard;
                    xSheetNameComboBox.BindControl(mWizard, nameof(ImportDataSourceFromExcelWizard.SheetName));
                    xSheetNameComboBox.AddValidationRule(new EmptyValidationRule());

                    chkHeadingRow.BindControl(mWizard, nameof(ImportDataSourceFromExcelWizard.HeadingRow));
                    chkModelParamsFile.BindControl(mWizard, nameof(ImportDataSourceFromExcelWizard.IsModelParamsFile));
                    chkImportEmptyColumns.BindControl(mWizard, nameof(ImportDataSourceFromExcelWizard.IsImportEmptyColumns));
                    break;
                case EventType.Active:
                    string excelPath = ((ImportDataSourceFromExcelWizard)WizardEventArgs.Wizard).Path;
                    if (!string.IsNullOrEmpty(excelPath))
                    {
                        impParams.ExcelFileName = excelPath;
                        List<string> SheetsList = impParams.GetSheets(false);
                        SheetsList.Insert(0, "-- All --");
                        GingerCore.General.FillComboFromList(xSheetNameComboBox, SheetsList);
                        if (SheetsList.Contains(mWizard.SheetName))
                        {
                            xSheetNameComboBox.SelectedIndex = SheetsList.IndexOf(mWizard.SheetName);
                        }
                    }
                    break;
                case EventType.LeavingForNextPage:
                    mWizard = (ImportDataSourceFromExcelWizard)WizardEventArgs.Wizard;
                    string excelPathForSpaceCheck = mWizard.Path;

                    string sheetName = mWizard.SheetName;
                    bool headingRow = mWizard.HeadingRow;
                    bool isModelParamsFile = mWizard.IsModelParamsFile;
                    bool isImportEmptyColumns = mWizard.IsImportEmptyColumns;
                    WizardEventArgs.CancelEvent = CheckIfCancelEvent(excelPathForSpaceCheck, sheetName, headingRow, isImportEmptyColumns, isModelParamsFile);
                    break;
                default:
                    break;
            }
        }

        private bool CheckIfCancelEvent(string Path, string SheetName, bool HeadingRow, bool IsImportEmptyColumns, bool IsModelParamsFile)
        {
            string outErrCol = string.Empty;
            DataSet ExcelImportData;
            impParams.ExcelFileName = Path;
            if (SheetName != "-- All --")
            {
                if (SheetName.Contains(' '))
                {
                    Reporter.ToUser(eUserMsgKey.DataSourceSheetNameHasSpace);
                    return true;
                }
                impParams.ExcelSheetName = SheetName;
                impParams.ExcelWhereCondition = String.Empty;
                ExcelImportData = impParams.GetExcelAllSheetData(SheetName, HeadingRow, IsImportEmptyColumns, IsModelParamsFile);
                if (ExcelImportData != null && ExcelImportData.Tables.Count >= 1)
                {
                    var columnList = ExcelImportData.Tables[0].Columns;
                    if (CheckIfColumnListContainsSpace(columnList, out outErrCol))
                    {
                        Reporter.ToUser(eUserMsgKey.DataSourceColumnHasSpace, outErrCol);
                        return true;
                    }
                }
            }
            else
            {
                List<string> SheetsList = impParams.GetSheets(false);
                if (SheetsList.Any(s => s.Contains(' ')))
                {
                    Reporter.ToUser(eUserMsgKey.DataSourceSheetNameHasSpace);
                    return true;
                }
                ExcelImportData = impParams.GetExcelAllSheetData(SheetName, HeadingRow, IsImportEmptyColumns, IsModelParamsFile);
                if (ExcelImportData != null && ExcelImportData.Tables.Count >= 1)
                {
                    foreach (DataTable dt in ExcelImportData.Tables)
                    {
                        if (CheckIfColumnListContainsSpace(dt.Columns, out outErrCol))
                        {
                            Reporter.ToUser(eUserMsgKey.DataSourceColumnHasSpace, outErrCol);
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private bool CheckIfColumnListContainsSpace(DataColumnCollection colList, out string colName)
        {
            for (int i = 0; i < colList.Count; i++)
            {
                if (colList[i].ColumnName.Contains(' '))
                {
                    colName = colList[i].ColumnName;
                    return true;
                }
            }
            colName = string.Empty;
            return false;
        }

        /// <summary>
        /// Constructor for ImportDataSourceSheetSelection class
        /// </summary>
        public ImportDataSourceSheetSelection()
        {
            InitializeComponent();
            impParams = new ImportOptionalValuesForParameters();
            ShowRelevantPanel();

            xSheetNameComboBox.Style = this.FindResource("$FlatInputComboBoxStyle") as Style;
            chkHeadingRow.Checked += ChkHeadingRow_Checked;
            chkModelParamsFile.Checked += ChkModelParamsFile_Checked;
            chkImportEmptyColumns.Checked += ChkExactValues_Checked;
            xSheetNameComboBox.Focus();
        }

        private void ChkModelParamsFile_Checked(object sender, RoutedEventArgs e)
        {
            mWizard.IsModelParamsFile = Convert.ToBoolean(chkModelParamsFile.IsChecked);
        }

        private void ChkHeadingRow_Checked(object sender, RoutedEventArgs e)
        {
            mWizard.HeadingRow = Convert.ToBoolean(chkHeadingRow.IsChecked);
        }
        private void ChkExactValues_Checked(object sender, RoutedEventArgs e)
        {
            mWizard.IsImportEmptyColumns = Convert.ToBoolean(chkImportEmptyColumns.IsChecked);
        }

        /// <summary>
        /// This method is used to ShowRelevantPanel
        /// </summary>
        /// <param name="FileType"></param>
        private void ShowRelevantPanel()
        {
            try
            {
                xExcelFileStackPanel.Visibility = Visibility.Visible;
                xSheetNameComboBox.Visibility = Visibility.Visible;
                xSheetLable.Visibility = Visibility.Visible;
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
        }

        /// <summary>
        /// This event is used for selection changed on the SheetName ComboBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void xSheetNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (xSheetNameComboBox.SelectedValue != null && xSheetNameComboBox.SelectedValue.ToString() != "-- All --")
                {
                    if (xSheetNameComboBox.SelectedValue.ToString().Contains(' '))
                    {
                        Reporter.ToUser(eUserMsgKey.DataSourceSheetNameHasSpace);
                        return;
                    }
                    mWizard.SheetName = Convert.ToString(xSheetNameComboBox.SelectedValue);
                    impParams.ExcelSheetName = mWizard.SheetName;
                }
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
        }
    }
}
