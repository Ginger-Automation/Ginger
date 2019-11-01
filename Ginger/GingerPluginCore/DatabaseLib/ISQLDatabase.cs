using System;
using System.Collections.Generic;
using System.Text;

namespace Amdocs.Ginger.Plugin.Core.DatabaseLib
{
    public interface ISQLDatabase
    {
        List<string> GetTablesList();

        List<string> GetTablesColumns(string table);
        
        Int64 GetRecordCount(string Query);
    }
}
