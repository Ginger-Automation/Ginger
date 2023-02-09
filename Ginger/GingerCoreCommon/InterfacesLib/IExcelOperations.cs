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

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Amdocs.Ginger.Common.InterfacesLib
{
    public interface IExcelOperations : IDisposable
    {
        /// <summary>
        /// Read excel sheet data from file with selected filters 
        /// </summary>
        /// <param name="fileName">Full path of selected excel file</param>
        /// <param name="sheetName">Selected excel sheet name</param>
        /// <param name="filter">Data table filter, like ColName=Value</param>
        /// <param name="selectedRows">false: first row in data table, True: all rows</param>
        /// <returns>Sheet as Data table</returns>
        DataTable ReadData(string fileName, string sheetName, string filter, bool selectedRows);
        /// <summary>
        /// Update Excel cells by user data set.
        /// </summary>
        /// <param name="fileName">Full path of selected excel file</param>
        /// <param name="sheetName">Selected excel sheet name</param>
        /// <param name="filter">Data table filter, like ColName=Value</param>
        /// <param name="setDataUsed"></param>
        /// <param name="updateCellValuesList">Cell values to be updated</param>
        /// <param name="primaryKey">Primary key by column name, like ColumnName</param>
        /// <param name="key"></param>
        /// <returns></returns>
        bool WriteData(string fileName, string sheetName, string filter, string setDataUsed, List<Tuple<string, object>> updateCellValuesList, string primaryKey = null, string key = null);
        /// <summary>
        /// Read excel data with cell filter
        /// </summary>
        /// <param name="fileName">Full path of selected excel file</param>
        /// <param name="sheetName">Selected excel sheet name</param>
        /// <param name="filter">Filter by one cell or multi cells adding cell location
        /// , Like: A2 for one cell, A2:D4 for multi cells</param>
        /// <param name="selectedRows">false: first cell after filter, True: all cells</param>
        /// <returns></returns>
        DataTable ReadCellData(string fileName, string sheetName, string filter, bool selectedRows);
        /// <summary>
        /// Update excel cells
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="sheetName"></param>
        /// <param name="filter"></param>
        /// <param name="setDataUsed"></param>
        /// <param name="primaryKey"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        bool UpdateExcelData(string fileName, string sheetName, string filter, List<Tuple<string, object>> updateCellValuesList, string primaryKey = null, string key = null);
        /// <summary>
        /// Get list of excel sheets name.
        /// </summary>
        /// <returns>List excel sheets name</returns>
        List<string> GetSheets(string fileName);
    }
}
