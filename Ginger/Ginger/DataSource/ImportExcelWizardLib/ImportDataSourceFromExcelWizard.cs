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

using Amdocs.Ginger.Common;
using Amdocs.Ginger.Repository;
using Ginger.ApplicationModelsLib.ModelOptionalValue;
using Ginger.SolutionWindows.TreeViewItems;
using Ginger.WizardLib;
using GingerCore.DataSource;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace Ginger.DataSource.ImportExcelWizardLib
{
    public class ImportDataSourceFromExcelWizard : WizardBase
    {
        public DataSourceBase mDSDetails;

        public override string Title { get { return "Create new solution wizard"; } }

        public List<PluginPackage> SelectedPluginPackages = [];

        public string Path { get; set; }
        public string SheetName { get; set; }
        public DataSet ExcelImportData { get; set; }

        /// <summary>
        /// Gets sets the HeadingRow
        /// </summary>
        public bool HeadingRow
        {
            get;
            set;
        }

        /// <summary>
        /// Gets sets the IsModelParamsFile
        /// </summary>
        public bool IsModelParamsFile
        {
            get;
            set;
        }
        public bool IsImportEmptyColumns
        {
            get;
            set;
        }
        /// <summary>
        /// This is used to initialize the wizard
        /// </summary>
        public ImportDataSourceFromExcelWizard(DataSourceBase DSDetails)
        {
            mDSDetails = DSDetails;
            AddPage(Name: "Introduction", Title: "Introduction", SubTitle: "Import DataSource From Excel File", Page: new WizardIntroPage("/DataSource/ImportExcelWizardLib/ImportDataSourceIntro.md"));
            AddPage(Name: "Browse File", Title: "Browse File", SubTitle: "Import DataSource From Excel File", Page: new ImportDataSourceBrowseFile());
            AddPage(Name: "Sheet Selection", Title: "Sheet Selection", SubTitle: "Import DataSource From Excel File", Page: new ImportDataSourceSheetSelection());
            AddPage(Name: "Display Data", Title: "Display Data", SubTitle: "Import DataSource From Excel File", Page: new ImportDataSourceDisplayData());
        }

        /// <summary>
        /// This method is the final finish method
        /// </summary>
        public override void Finish()
        {
            try
            {
                ImportOptionalValuesForParameters impParams = new ImportOptionalValuesForParameters
                {
                    ExcelFileName = Path,
                    ExcelSheetName = SheetName
                };

                if (ExcelImportData == null || ExcelImportData.Tables.Count <= 0)
                {
                    ExcelImportData = impParams.GetExcelAllSheetData(SheetName, HeadingRow, IsImportEmptyColumns, IsModelParamsFile);
                }
                foreach (DataTable dt in ExcelImportData.Tables)
                {
                    if (!IsImportEmptyColumns)
                    {
                        // Removing empty rows
                        RemoveEmptyRows(dt);
                    }
                    
                    string cols = GetColumnNameListForTableCreation(dt);
                    AddDefaultColumn(dt);
                    CreateTable(dt.TableName, cols);
                    mDSDetails.SaveTable(dt);
                }
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
            }
        }

        private void RemoveEmptyRows(DataTable dt)
        {
            var rowsToRemove = new List<DataRow>();
            foreach (DataRow row in dt.Rows)
            {
                bool isEmpty = true;
                foreach (var item in row.ItemArray)
                {
                    if (item != null && !string.IsNullOrWhiteSpace(item.ToString()))
                    {
                        isEmpty = false;
                        break;
                    }
                }
                if (isEmpty)
                {
                    rowsToRemove.Add(row);
                }
            }
            foreach (var row in rowsToRemove)
            {
                dt.Rows.Remove(row);
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
                colList.Append(mDSDetails.AddNewCustomizedTableQuery() + ",");
                foreach (DataColumn col in dt.Columns)
                {
                    if (col.ColumnName is "GINGER_ID" or "GINGER_USED" or "GINGER_LAST_UPDATED_BY" or "GINGER_LAST_UPDATE_DATETIME")
                    {
                        continue;
                    }
                    colList.Append(mDSDetails.AddColumnName(col.ColumnName));

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
                    dr["GINGER_USED"] = "False";
                }
            }
            catch (System.Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}", ex);
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
                DataSourceTable dsTableDetails = new DataSourceTable
                {
                    DSTableType = DataSourceTable.eDSTableType.Customized,
                    Name = name,
                    DSC = mDSDetails
                };
                DataSourceTableTreeItem DSTTI = new DataSourceTableTreeItem
                {
                    DSTableDetails = dsTableDetails,
                    DSDetails = mDSDetails
                };
                dsTableDetails.DSC.AddTable(dsTableDetails.Name, query);
                mDSDetails.DSTableList.Add(dsTableDetails);
                fileName = mDSDetails.FileFullPath;
            }
            catch (Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, ex.StackTrace);
            }
            return fileName;
        }
    }
}