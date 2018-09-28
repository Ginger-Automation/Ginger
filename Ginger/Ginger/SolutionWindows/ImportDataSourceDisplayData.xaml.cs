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

using System.Windows;
using System.Windows.Controls;
using GingerCore;
using GingerCore.DataSource;
using System.Reflection;
using Ginger.ApplicationModelsLib.ModelOptionalValue;
using System.Data;
using System;
using GingerWPF.WizardLib;
using System.Windows.Input;

namespace Ginger.SolutionWindows
{
    /// <summary>
    /// Interaction logic for ImportDataSourceDisplayData.xaml
    /// </summary>
    public partial class ImportDataSourceDisplayData : Page, IWizardPage
    {
        public DataSourceBase DSDetails { get; set; }
        ImportOptionalValuesForParameters impParams;
        public DataSet ExcelImportData = null;
        WizardEventArgs mWizardEventArgs;

        /// <summary>
        /// Gets sets the File path
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets sets the SheetName
        /// </summary>
        public string SheetName { get; set; }

        /// <summary>
        /// Gets sets the HeadingRow
        /// </summary>
        public bool HeadingRow
        {
            get
            {
                return Convert.ToBoolean(chkHeadingRow.IsChecked);
            }
        }

        /// <summary>
        /// This method is default wizard action event
        /// </summary>
        /// <param name="WizardEventArgs"></param>
        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            mWizardEventArgs = WizardEventArgs;
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    break;
                case EventType.Active:                    
                    Path = ((ImportDataSourceBrowseFile)(WizardEventArgs.Wizard.Pages[1].Page)).Path;
                    SheetName = ((ImportDataSourceSheetSelection)(WizardEventArgs.Wizard.Pages[2].Page)).SheetName;                    
                    break;
                case EventType.AfterLoad:
                    DisplayData();
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Constrtuctor for ImportDataSourceDisplayData class
        /// </summary>
        public ImportDataSourceDisplayData()
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
                xExcelDataGridDockPanel.Visibility = Visibility.Collapsed;
                xExcelViewWhereButton.Visibility = Visibility.Visible;
                xExcelGridSplitter.Visibility = Visibility.Collapsed;
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
                DisplayData();
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }
        }

        /// <summary>
        /// This method is used to display the data for the selected sheet
        /// </summary>
        private void DisplayData()
        {
            try
            {
                if (!IsAlternatePageToLoad())
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    mWizardEventArgs.Wizard.ProcessStarted();

                    impParams.ExcelFileName = Path;
                    impParams.ExcelSheetName = SheetName;
                    ExcelImportData = impParams.GetExcelAllSheetData(SheetName, Convert.ToBoolean(chkHeadingRow.IsChecked));
                    if (ExcelImportData != null && ExcelImportData.Tables.Count >= 1)
                    {
                        if (ExcelImportData.Tables.Count == 1)
                        {
                            xExcelDataGrid.ItemsSource = ExcelImportData.Tables[0].AsDataView();
                            xExcelDataGridDockPanel.Visibility = Visibility.Visible;
                        }
                    }

                    mWizardEventArgs.Wizard.ProcessEnded();
                    Mouse.OverrideCursor = null; 
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
                Mouse.OverrideCursor = Cursors.Wait;
                mWizardEventArgs.Wizard.ProcessStarted();

                impParams.ExcelFileName = Path;
                impParams.ExcelSheetName = SheetName;
                impParams.ExcelWhereCondition = Convert.ToString(xSelectRowTextBox.Text);
                DataTable dt = impParams.GetExceSheetlData(true);
                if (dt != null)
                {
                    xExcelDataGrid.ItemsSource = dt.AsDataView();
                    xExcelDataGridDockPanel.Visibility = Visibility.Visible;
                }

                mWizardEventArgs.Wizard.ProcessEnded();
                Mouse.OverrideCursor = null;
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }
        }
                
        /// <summary>
        /// This method is used to cehck whether alternate page is required to load
        /// </summary>
        /// <returns></returns>
        public bool IsAlternatePageToLoad()
        {
            bool isAlternatePage = false;
            if (SheetName == "-- All --")
            {
                isAlternatePage = true;
            }
            return isAlternatePage;
        }
    }
}
