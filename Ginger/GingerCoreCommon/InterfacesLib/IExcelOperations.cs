using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Amdocs.Ginger.Common.InterfacesLib
{
    public interface IExcelOperations
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
        bool updateExcelData(string fileName, string sheetName, string filter, string setDataUsed, string primaryKey = null, string key = null);
        /// <summary>
        /// Get list of excel sheets name.
        /// </summary>
        /// <returns>List excel sheets name</returns>
        List<string> GetSheets(string fileName);
    }
}
