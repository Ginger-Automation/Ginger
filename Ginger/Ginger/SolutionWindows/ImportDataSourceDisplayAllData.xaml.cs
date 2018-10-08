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
using System.Collections.Generic;
using System;
using System.Text;
using Ginger.SolutionWindows.TreeViewItems;
using GingerWPF.WizardLib;
using System.Windows.Input;

namespace Ginger.SolutionWindows
{
    /// <summary>
    /// Interaction logic for ImportDataSourceDisplayAllData.xaml
    /// </summary>
    public partial class ImportDataSourceDisplayAllData : Page, IWizardPage
    {
        public DataSourceBase DSDetails { get; set; }
        ImportOptionalValuesForParameters impParams;
        public DataSet ExcelImportData = null;
        WizardEventArgs WizardEventArgs;

        /// <summary>
        /// Gets sets the File path
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Gets sets the SheetName
        /// </summary>
        public string SheetName { get; set; }

        ///// <summary>
        ///// Gets sets the HeadingRow
        ///// </summary>
        //public bool HeadingRow
        //{
        //    get
        //    {
        //        return Convert.ToBoolean(chkHeadingRow.IsChecked);
        //    }
        //}

        /// <summary>
        /// This method is default wizard action event
        /// </summary>
        /// <param name="WizardEventArgs"></param>
        public void WizardEvent(WizardEventArgs mWizardEventArgs)
        {
            switch (mWizardEventArgs.EventType)
            {
                case EventType.Init:
                    break;
                case EventType.Active:
                    Path = ((ImportDataSourceBrowseFile)(mWizardEventArgs.Wizard.Pages[1].Page)).Path;
                    SheetName = ((ImportDataSourceSheetSelection)(mWizardEventArgs.Wizard.Pages[2].Page)).SheetName;

                    WizardEventArgs = mWizardEventArgs;
                    impParams.ExcelFileName = Path;
                    impParams.ExcelSheetName = SheetName;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Constrtuctor for ImportDataSourceDisplayAllData class
        /// </summary>
        public ImportDataSourceDisplayAllData()
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
                xExcelViewDataButton.Visibility = Visibility.Visible;
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
                Mouse.OverrideCursor = Cursors.Wait;
                WizardEventArgs.Wizard.ProcessStarted();

                ExcelImportData = impParams.GetExcelAllSheetData(SheetName, Convert.ToBoolean(chkHeadingRow.IsChecked));
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

                WizardEventArgs.Wizard.ProcessEnded();
                Mouse.OverrideCursor = null;
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }
        }

        private List<TabItem> _tabItems;
        /// <summary>
        /// This method will return the tab
        /// </summary>
        /// <returns></returns>
        private TabItem AddTabItem(string name, DataTable dt)
        {
            int count = _tabItems.Count;

            // create new tab item
            TabItem tab = new TabItem();
            tab.Header = string.Format("{0}", name);
            tab.Name = string.Format("{0}", name);

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(new GridPage() { ExcelData = dt });
            tab.Content = stackPanel;

            _tabItems.Add(tab);
            return tab;
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

        /// <summary>
        /// This method is used to add the defaul columns to the table
        /// </summary>
        /// <param name="dataTable"></param>
        private void AddDefaultColumn(DataTable dataTable)
        {
            try
            {
                if(!dataTable.Columns.Contains("GINGER_ID"))
                {
                    dataTable.Columns.Add("GINGER_ID");
                }
                if (!dataTable.Columns.Contains("GINGER_USED"))
                {
                    dataTable.Columns.Add("GINGER_USED");
                }
                if (!dataTable.Columns.Contains("GINGER_LAST_UPDATED_BY"))
                {
                    dataTable.Columns.Add("GINGER_LAST_UPDATED_BY");
                }
                if (!dataTable.Columns.Contains("GINGER_LAST_UPDATE_DATETIME"))
                {
                    dataTable.Columns.Add("GINGER_LAST_UPDATE_DATETIME");
                }
                foreach (DataRow dr in dataTable.Rows)
                {
                    dr["GINGER_USED"] = "True";
                }
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }
        }

        /// <summary>
        /// This method is used to create the table
        /// </summary>
        /// <param name="query"></param>
        private string CreateTable(string name, string query)
        {
            string fileName = string.Empty;
            try
            {
                DataSourceTable dsTableDetails = new DataSourceTable();
                if (dsTableDetails != null)
                {
                    dsTableDetails.Name = name;
                    dsTableDetails.DSC = DSDetails.DSC;
                    DataSourceTableTreeItem DSTTI = new DataSourceTableTreeItem();
                    DSTTI.DSTableDetails = dsTableDetails;
                    DSTTI.DSDetails = DSDetails;
                    dsTableDetails.DSC.AddTable(dsTableDetails.Name, query);
                    DSDetails.DSTableList.Add(dsTableDetails);

                    fileName = DSDetails.FileFullPath;
                }
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.StackTrace);
            }
            return fileName;
        }
               
        /// <summary>
        /// This method is used to cehck whether alternate page is required to load
        /// </summary>
        /// <returns></returns>
        public bool IsAlternatePageToLoad()
        {
            return false;
        }
    }
}
