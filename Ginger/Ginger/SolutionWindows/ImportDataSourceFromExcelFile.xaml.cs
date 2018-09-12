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

namespace Ginger.DataSource
{
    /// <summary>
    /// Interaction logic for ImportDataSourceFromExcelFile.xaml
    /// </summary>
    public partial class ImportDataSourceFromExcelFile : Page
    {
        ImportOptionalValuesForParameters impParams;
        GenericWindow _pageGenericWin = null;
        DataSet ExcelImportData = null;
        
        /// <summary>
        /// Constrtuctor for ImportDataSourceFromExcelFile class
        /// </summary>
        public ImportDataSourceFromExcelFile()
        {           
            InitializeComponent();
            impParams = new ImportOptionalValuesForParameters();
            ShowRelevantPanel();
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
                xSaveExcelLable.Visibility = Visibility.Visible;
                xExcelDataGridDockPanel.Visibility = Visibility.Collapsed;
                xExcelViewDataButton.Visibility = Visibility.Collapsed;
                xExcelViewWhereButton.Visibility = Visibility.Collapsed;
                xExcelGridSplitter.Visibility = Visibility.Collapsed;
                xSheetNameComboBox.Visibility = Visibility.Visible;
                xSheetLable.Visibility = Visibility.Visible;
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }
        }

        /// <summary>
        /// Ok button click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _pageGenericWin.Close();
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }
        }

        /// <summary>
        /// This method is used to show the window
        /// </summary>
        /// <param name="windowStyle"></param>
        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            try
            {
                Button okBtn = new Button();
                okBtn.Content = "OK";
                okBtn.Click += new RoutedEventHandler(OKButton_Click);
                ObservableList<Button> winButtons = new ObservableList<Button>();
                winButtons.Add(okBtn);
                GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, this.Title, this, winButtons, true, "Cancel");
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }
        }

        /// <summary>
        /// This event handles browsing of Script File from user desktop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void xBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog();
                dlg.Multiselect = false;
                dlg.Filter = "Excel Files|*.xls;*.xlsx;*.xlsm";
                System.Windows.Forms.DialogResult result = dlg.ShowDialog();
                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    xPathTextBox.Text = dlg.FileName;
                    impParams.ExcelFileName = dlg.FileName;
                    List<string> SheetsList = impParams.GetSheets(false);
                    GingerCore.General.FillComboFromList(xSheetNameComboBox, SheetsList);
                }
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
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
                if (xSheetNameComboBox.SelectedValue != null)
                {
                    impParams.ExcelSheetName = xSheetNameComboBox.SelectedValue.ToString();
                    if (!string.IsNullOrEmpty(xSheetNameComboBox.SelectedValue.ToString()))
                    {
                        xExcelDataGridDockPanel.Visibility = Visibility.Collapsed;
                        xExcelViewDataButton.Visibility = Visibility.Visible;
                        xExcelViewWhereButton.Visibility = Visibility.Visible;
                        xExcelGridSplitter.Visibility = Visibility.Visible;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }
        }

        /// <summary>
        /// This event is used to refresh the sheet dropdown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                List<string> SheetsList = impParams.GetSheets(false);
                GingerCore.General.FillComboFromList(xSheetNameComboBox, SheetsList);
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }
        }

        /// <summary>
        /// This event is used to view the data from excel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void xExcelViewDataButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExcelImportData = impParams.GetExcelAllSheetData(Convert.ToString(xSheetNameComboBox.Text));
                if (ExcelImportData != null && ExcelImportData.Tables.Count >= 1)
                {
                    if (ExcelImportData.Tables.Count == 1)
                    {
                        xExcelDataGrid.ItemsSource = ExcelImportData.Tables[0].AsDataView();
                        xExcelDataGridDockPanel.Visibility = Visibility.Visible; 
                    }
                }
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }
        }

        /// <summary>
        /// This event is used to view the data from excel with the condition
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void xExcelViewWhereButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                impParams.ExcelWhereCondition = Convert.ToString(xSelectRowTextBox.Text);
                DataTable dt = impParams.GetExceSheetlData(true);
                if (dt != null)
                {
                    xExcelDataGrid.ItemsSource = dt.AsDataView();
                    xExcelDataGridDockPanel.Visibility = Visibility.Visible;
                }
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }
        }

        /// <summary>
        /// This method is used to get the columnList for exporting the parameters to datasource
        /// </summary>
        /// <returns></returns>
        private string GetColumnNameListForTableCreation(DataTable dt)
        {
            string cols = string.Empty;
            try
            {
                StringBuilder colList = new StringBuilder();
                colList.Append("[GINGER_ID] AUTOINCREMENT,[GINGER_USED] Text,[GINGER_LAST_UPDATED_BY] Text,[GINGER_LAST_UPDATE_DATETIME] Text,");
                foreach (DataColumn col in dt.Columns)
                {
                    colList.Append(string.Format("[{0}] Text,", col.ColumnName));
                }

                cols = colList.ToString().Remove(colList.ToString().LastIndexOf(","), 1);
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.StackTrace);
            }
            return cols;
        }

    }
}
