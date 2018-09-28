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

using System;
using System.Data;
using System.Reflection;
using System.Text;
using System.Windows.Controls;
using System.Windows.Input;
using Ginger.ApplicationModelsLib.ModelOptionalValue;
using Ginger.SolutionWindows.TreeViewItems;
using GingerCore;
using GingerCore.DataSource;
using GingerWPF.WizardLib;

namespace Ginger.SolutionWindows
{
    /// <summary>
    /// Interaction logic for ImportDataSourceFinishPage.xaml
    /// </summary>
    public partial class ImportDataSourceFinishPage : Page, IWizardPage
    {
        public DataSourceBase DSDetails { get; set; }

        /// <summary>
        /// This method is default wizard action event
        /// </summary>
        /// <param name="WizardEventArgs"></param>
        public void WizardEvent(WizardEventArgs WizardEventArgs)
        {
            switch (WizardEventArgs.EventType)
            {
                case EventType.Init:
                    break;
                case EventType.Active:                    
                    FinishImport(WizardEventArgs);
                    xLable.Content = "Data Imported Successfully!";
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Constrtuctor for ImportDataSourceFinishPage class
        /// </summary>
        public ImportDataSourceFinishPage(DataSourceBase mDSDetails)
        {           
            InitializeComponent();
            DSDetails = mDSDetails;
        }

        /// <summary>
        /// This method is the final FinishImport method
        /// </summary>
        public void FinishImport(WizardEventArgs WizardEventArgs)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                WizardEventArgs.Wizard.ProcessStarted();

                ImportOptionalValuesForParameters impParams = new ImportOptionalValuesForParameters();
                string path = ((ImportDataSourceBrowseFile)(WizardEventArgs.Wizard.Pages[1].Page)).Path;
                string sheetName = ((ImportDataSourceSheetSelection)(WizardEventArgs.Wizard.Pages[2].Page)).SheetName;
                bool headingRow = false;

                if (((ImportDataSourceDisplayData)(WizardEventArgs.Wizard.Pages[3]).Page).IsAlternatePageToLoad())
                {
                    headingRow = ((ImportDataSourceDisplayAllData)(WizardEventArgs.Wizard.Pages[3]).AlternatePage).HeadingRow;
                }
                else
                {
                    headingRow = ((ImportDataSourceDisplayData)(WizardEventArgs.Wizard.Pages[3]).Page).HeadingRow;
                }

                impParams.ExcelFileName = path;
                impParams.ExcelSheetName = sheetName;

                DataSet ExcelImportData = ((ImportDataSourceDisplayData)(WizardEventArgs.Wizard.Pages[3]).Page).ExcelImportData;
                if (ExcelImportData == null || ExcelImportData.Tables.Count <= 0)
                {
                    ExcelImportData = impParams.GetExcelAllSheetData(sheetName, headingRow);
                }
                foreach (DataTable dt in ExcelImportData.Tables)
                {
                    string cols = GetColumnNameListForTableCreation(dt);
                    AddDefaultColumn(dt);
                    string fileName = CreateTable(dt.TableName, cols);
                    ((AccessDataSource)(DSDetails)).Init(fileName);
                    ((AccessDataSource)(DSDetails)).SaveTable(dt); 
                }

                WizardEventArgs.Wizard.ProcessEnded();
                Mouse.OverrideCursor = Cursors.Arrow;
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

        /// <summary>
        /// This method is used to add the defaul columns to the table
        /// </summary>
        /// <param name="dataTable"></param>
        private void AddDefaultColumn(DataTable dataTable)
        {
            try
            {
                if (!dataTable.Columns.Contains("GINGER_ID"))
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
