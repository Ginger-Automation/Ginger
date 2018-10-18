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
using System.Collections.Generic;
using Amdocs.Ginger.Common;
using Ginger.UserControlsLib.UCDataGridView;

namespace Ginger.DataSource.ImportExcelWizardLib
{
    /// <summary>
    /// Interaction logic for ImportDataSourceDisplayData.xaml
    /// </summary>
    public partial class ImportDataSourceDisplayData : Page, IWizardPage
    {
        public DataSourceBase DSDetails { get; set; }
        ImportOptionalValuesForParameters impParams;
        public DataSet ExcelImportData;
        WizardEventArgs mWizardEventArgs;
                
        private string Path;
        private string SheetName;
        private bool HeadingRow;
        private bool IsModelParamsFile;

        private List<TabItem> _tabItems;

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
                    Path = ((ImportDataSourceFromExcelWizard)mWizardEventArgs.Wizard).Path;
                    SheetName = ((ImportDataSourceFromExcelWizard)mWizardEventArgs.Wizard).SheetName;
                    HeadingRow = ((ImportDataSourceFromExcelWizard)mWizardEventArgs.Wizard).HeadingRow;
                    IsModelParamsFile = ((ImportDataSourceFromExcelWizard)mWizardEventArgs.Wizard).IsModelParamsFile;

                    impParams.ExcelFileName = Path;
                    impParams.ExcelSheetName = SheetName;
                    DisplayData();
                    break;                
                case EventType.LeavingForNextPage:
                    ((ImportDataSourceFromExcelWizard)mWizardEventArgs.Wizard).ExcelImportData = ExcelImportData;
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
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
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
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }
        }

        /// <summary>
        /// This method is used to display the data for the selected sheet
        /// </summary>
        private void DisplayData()
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                mWizardEventArgs.Wizard.ProcessStarted();


                if (SheetName != "-- All --")
                {
                    xExcelDataGrid.Visibility = Visibility.Visible;
                    tabDynamic.Visibility = Visibility.Collapsed;
                    SelectPanel.Visibility = Visibility.Visible;
                    xExcelGridSplitter.Visibility = Visibility.Visible;

                    ExcelImportData = impParams.GetExcelAllSheetData(SheetName, HeadingRow, true, IsModelParamsFile);
                    if (ExcelImportData != null && ExcelImportData.Tables.Count >= 1)
                    {
                        if (ExcelImportData.Tables.Count == 1)
                        {
                            xExcelDataGrid.ItemsSource = ExcelImportData.Tables[0].AsDataView();
                            xExcelDataGridDockPanel.Visibility = Visibility.Visible;
                        }
                    }                    
                }
                else
                {
                    SelectPanel.Visibility = Visibility.Collapsed;
                    xExcelGridSplitter.Visibility = Visibility.Collapsed;
                    xExcelDataGrid.Visibility = Visibility.Collapsed;
                    tabDynamic.Visibility = Visibility.Visible;
                    xExcelDataGridDockPanel.Visibility = Visibility.Visible;

                    ExcelImportData = impParams.GetExcelAllSheetData(SheetName, HeadingRow, true, IsModelParamsFile);
                    if (ExcelImportData != null && ExcelImportData.Tables.Count >= 1)
                    {
                        _tabItems = new List<TabItem>();
                        foreach (DataTable dt in ExcelImportData.Tables)
                        {
                            AddTabItem(dt.TableName, dt);
                        }
                        // bind tab control
                        tabDynamic.DataContext = _tabItems;
                        tabDynamic.SelectedIndex = 0;
                    }
                }
                mWizardEventArgs.Wizard.ProcessEnded();
                Mouse.OverrideCursor = null;
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
                mWizardEventArgs.Wizard.ProcessEnded();
                Mouse.OverrideCursor = null;
            }
        }
        private TabItem AddTabItem(string name, DataTable dt)
        {
            int count = _tabItems.Count;

            // create new tab item
            TabItem tab = new TabItem();
            tab.Header = string.Format("{0}", name);
            tab.Name = string.Format("{0}", name);

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(new ucDataGrid() { ExcelData = dt });
            tab.Content = stackPanel;

            _tabItems.Add(tab);
            return tab;
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
                Reporter.ToLog(eAppReporterLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }
        }
    }
}
