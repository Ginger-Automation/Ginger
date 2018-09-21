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

using Amdocs.Ginger.Repository;
using Ginger.ApplicationModelsLib.ModelOptionalValue;
using Ginger.DataSource;
using Ginger.Environments;
using Ginger.SolutionWindows.TreeViewItems;
using GingerCore;
using GingerCore.DataSource;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace GingerWPF.SolutionLib
{
    public class ImportDataSourceFromExcelWizard : WizardBase
    {
        public DataSourceBase DSDetails { get; set; }

        public override string Title { get { return "Create new solution wizard"; } }

        public List<PluginPackage> SelectedPluginPackages = new List<PluginPackage>();

        /// <summary>
        /// This is used to initialise the wizard
        /// </summary>
        public ImportDataSourceFromExcelWizard(DataSourceBase mDSDetails)
        {
            DSDetails = mDSDetails;
            AddPage(Name: "Browse File", Title: "Browse File", SubTitle: "Import DataSource From Excel File", Page: new ImportDataSourceBrowseFile());
            AddPage(Name: "Sheet Selection", Title: "Sheet Selection", SubTitle: "Import DataSource From Excel File", Page: new ImportDataSourceSheetSelection());
            AddPage(Name: "Display Data", Title: "Display Data", SubTitle: "Import DataSource From Excel File", Page: new ImportDataSourceDisplayData(), AlternatePage: new ImportDataSourceDisplayAllData());
        }

        /// <summary>
        /// This method is the final finish method
        /// </summary>
        public override void Finish()
        {
            try
            {
                ImportOptionalValuesForParameters impParams = new ImportOptionalValuesForParameters();
                string path = ((ImportDataSourceBrowseFile)(Pages[0]).Page).Path;
                string sheetName = ((ImportDataSourceSheetSelection)(Pages[1]).Page).SheetName;
                bool headingRow = false;

                if(((ImportDataSourceDisplayData)(Pages[2]).Page).IsAlternatePageToLoad())
                {
                    headingRow = ((ImportDataSourceDisplayData)(Pages[2]).AlternatePage).HeadingRow;
                }
                else
                {
                    headingRow = ((ImportDataSourceDisplayData)(Pages[2]).Page).HeadingRow;
                }

                impParams.ExcelFileName = path;
                impParams.ExcelSheetName = sheetName;

                DataSet ExcelImportData = ((ImportDataSourceDisplayData)(Pages[2]).Page).ExcelImportData;
                if (ExcelImportData == null || ExcelImportData.Tables.Count <= 0)
                {
                    ExcelImportData = impParams.GetExcelAllSheetData(sheetName, headingRow);
                }
                string cols = GetColumnNameListForTableCreation(ExcelImportData.Tables[0]);
                AddDefaultColumn(ExcelImportData.Tables[0]);
                string fileName = CreateTable(ExcelImportData.Tables[0].TableName, cols);
                ((AccessDataSource)(DSDetails)).Init(fileName);
                ((AccessDataSource)(DSDetails)).SaveTable(ExcelImportData.Tables[0]);
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
    }
}