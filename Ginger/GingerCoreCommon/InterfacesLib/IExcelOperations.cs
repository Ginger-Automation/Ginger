using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Amdocs.Ginger.Common.InterfacesLib
{
    public interface IExcelOperations
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <param name="selectedRows"></param>
        /// <param name="primaryKey"></param>
        /// <param name="setData"></param>
        /// <returns></returns>
        DataTable ReadData(string fileName, string sheetName, string filter, bool selectedRows, string primaryKey, string setData);
        bool WriteData(string fileName, string sheetName, string filter, string setDataUsed, List<Tuple<string, object>> updateCellValuesList, string primaryKey = null, string key = null);
        DataTable ReadCellData(string fileName, string sheetName, string filter, bool selectedRows);
        bool updateExcelData(string fileName, string sheetName, string filter, string setDataUsed, string primaryKey = null, string key = null);
        List<string> GetSheets();
    }
}
